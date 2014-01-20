using System;
using Gtk;
using System.Net;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using translinkupdater;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

public partial class MainWindow: Gtk.Window
{	
	private IList<Thread> runningThreads;

	public MainWindow (): base (Gtk.WindowType.Toplevel)
	{
		Build ();
		runningThreads = new List<Thread> ();
	}

	protected void OnDeleteEvent (object sender, DeleteEventArgs a)
	{
		Application.Quit ();
		a.RetVal = true;
		foreach (var thread in runningThreads) {
			if (thread.IsAlive) 
				thread.Interrupt ();
		}
		runningThreads = new List<Thread> ();
	}

	protected void OnBtnUpdateClicked (object sender, EventArgs e)
	{
		SelectSource (true);
		if (Api.SignedInUser == null) {
			var signInWindow = new SignInWindow ();
			signInWindow.Show ();
			signInWindow.Destroyed += delegate(object sender2, EventArgs e2) {
				if (Api.SignedInUser != null) {
					OnBtnUpdateClicked (sender2, e2);
				}
			};
			return;
		}
		Console.WriteLine ("signed in as {0}", Api.SignedInUser);

		Thread thread = new Thread (new ThreadStart (Update));
		btnUpdate.Sensitive = false;
		buttonCancel.Sensitive = true;
		buttonCancel.GrabFocus ();
		btnUpdate.HasDefault = false;
		buttonApproveChange.HasDefault = true;

		runningThreads.Add (thread);
		thread.Start ();
	}

	protected void LogAll (string message, params object[] args)
	{
		Log (string.Format (message, args));
	}

	protected void Log (string message)
	{
		Gtk.Application.Invoke (delegate {
			if (textviewLog.Buffer.CharCount > 15000) {
				textviewLog.Buffer.Text = message + "\n" + textviewLog.Buffer.Text.Substring(0, 10000) + "...";
			} else {
				textviewLog.Buffer.Text = message + "\n" + textviewLog.Buffer.Text;
			}
		}
		);
		Console.WriteLine (message);
	}

	protected void Update ()
	{
		try {
			IEnumerable<Api.Page> pages;
			var maxPages = int.Parse (entryMax.Text);
			var startAt = entryStart.Text;
			if (source == "dump") {
				var dr = new DumpReader (dumpFilename);
				pages = dr.Pages (
						namespaces: new SortedSet<int> () {0},
						startAt: startAt,
						maxPages: maxPages
				);
	
			} else if (source == "live") {
				pages = Api.PagesInCategory (
					"Svenska/Alla uppslag",
					ns: 0,
					step: 250,
					maxPages: maxPages,
					startAt: startAt
				);
			} else if (source == "titles") {
				// TODO
				// pages = Api.GetPages();
				throw new NotImplementedException();
			} else {
				throw new Exception("Internal error: source not defined");
			}
			TranslationLinkUpdater.Update (
				pages: pages,
				saveCallback: SaveCallback,
				pageDoneCallback: PageDoneCallback,
				logCallback: Log);
		} catch (ThreadInterruptedException) {
			Gtk.Application.Invoke (delegate {
				entrySummary.Text = "";
				textviewBefore.Buffer.Text = "";
				textviewAfter.Buffer.Text = "";
				vboxConfirmEdit.Sensitive = false;
			}
			);

			Console.WriteLine ("Thread interrupted [no worries]");
		} catch (Language.LanguageException ex) {
			LogAll ("Language exception: {0}", ex.Message);
		} catch (TranslationLinkUpdater.SortException ex) {
			LogAll ("Sort exception: {0}", ex.Message);
		} catch (Api.NotLoggedInException) {
			Log ("Abort: Not logged in!");
		}
		Console.WriteLine ("Thread finished");

		Gtk.Application.Invoke (delegate {
			buttonCancel.Sensitive = false;
			btnUpdate.Sensitive = true;
			btnUpdate.GrabFocus ();
			btnUpdate.HasDefault = true;
			buttonApproveChange.HasDefault = false;
		}
		);
	}

	protected void PageDoneCallback (string title, bool addExclamation=true)
	{
		Gtk.Application.Invoke (delegate {
			if (addExclamation)
				entryStart.Text = title + "!";
			else
				entryStart.Text = title;
		}
		);
	}

	protected volatile string saveCallbackAnswer = null;

	protected bool SaveCallback (string title,
	                             string summary, out string changedSummary,
	                             string before, string after, out string changedWikitext)
	{
		PageDoneCallback (title, addExclamation: false);
		if (!checkbuttonConfirm.Active) {
			changedSummary = summary;
			changedWikitext = after;
			return true;
		}

		Gtk.Application.Invoke (delegate {
			entrySummary.Text = summary;
			textviewBefore.Buffer.Text = before;
			textviewAfter.Buffer.Text = after;
			vboxConfirmEdit.Sensitive = true;
			entrySummary.GrabFocus ();
		}
		);
		saveCallbackAnswer = null;

		/* 
		 * Trying to use thread interruption caused exceptions
		 * in Api.Save().
		 */

		while (saveCallbackAnswer == null) {
			Thread.Sleep (300);
		}
		changedSummary = entrySummary.Text;
		changedWikitext = textviewAfter.Buffer.Text;
		Gtk.Application.Invoke (delegate {
			vboxConfirmEdit.Sensitive = false;
			entrySummary.Text = "";
			textviewBefore.Buffer.Text = "";
			textviewAfter.Buffer.Text = "";
		}
		);
		return saveCallbackAnswer == "save";
	}

	protected void OnButtonCancelClicked (object sender, EventArgs e)
	{
		foreach (var thread in runningThreads) {
			thread.Interrupt ();
		}
		runningThreads = new List<Thread> ();
	}

	protected void OnButtonSkipClicked (object sender, EventArgs e)
	{
		saveCallbackAnswer = "skip";
	}

	protected void OnButtonApproveChangeClicked (object sender, EventArgs e)
	{
		saveCallbackAnswer = "save";
	}

	protected void OnButtonSpecifySourceClicked (object sender, EventArgs e)
	{
		SelectSource (false);
	}

	protected void OnComboboxSourceChanged (object sender, EventArgs e)
	{
		SelectSource (true);
	}

	string source = null;
	string dumpFilename = null;

	protected void SelectSource (bool useExistingSelection)
	{
		if (comboboxSource.Active == 0) {
			source = "dump";
			if (!useExistingSelection || dumpFilename == null) {
				var fc = new FileChooserDialog (
					"Välj dump-fil",
					this,
					FileChooserAction.Open,
					"Avbryt", ResponseType.Cancel,
					"Öppna", ResponseType.Accept);

				if (fc.Run () == (int)ResponseType.Accept) {
					dumpFilename = fc.Filename;
				}
				fc.Destroy ();
			}
			
			if (dumpFilename == null) {
				buttonSpecifySource.Label = "...";
			} else {
				if (dumpFilename.Length < 18)
					buttonSpecifySource.Label = dumpFilename;
				else
					buttonSpecifySource.Label = "..." +
						dumpFilename.Substring (dumpFilename.Length - 18);
			}
		} else if (comboboxSource.Active == 1) {
			// source = "titles";
			throw new NotImplementedException ();
		}
	}

	protected IEnumerable<Api.Page> GetPages ()
	{
		if (source == "dump") {
			var dr = new DumpReader (dumpFilename);
			return dr.Pages (namespaces: new SortedSet<int> () {0});
		} else {
			throw new NotImplementedException ();
		}

		/*
		var i = 0;
		var pages = dr.Pages (namespaces: new SortedSet<int> () {0});

		foreach (var page in pages) {
			if (i % 10000 == 0) {
				var now = DateTime.Now;
				Console.WriteLine (
					"title={0}  |  timestamp={1} | {2}",
					page.Title, page.Timestamp,

					now);
			}
			i++;
		}
		*/
	}
}
