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
			textviewLog.Buffer.Text = message + "\n" + textviewLog.Buffer.Text;
		}
		);
		Console.WriteLine (message);
	}

	protected void Update ()
	{
		try {
			TranslationLinkUpdater.Update (
				startAt: entryStart.Text,
				maxPages: int.Parse (entryMax.Text),
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
			entrySummary.GrabFocus();
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



}
