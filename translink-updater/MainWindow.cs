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
			return;
		}
		Console.WriteLine ("signed in as {0}", Api.SignedInUser);

		Thread thread = new Thread (new ThreadStart (Update));
		thread.Start ();
		btnUpdate.Sensitive = false;
		buttonCancel.Sensitive = true;
		buttonCancel.GrabFocus ();
		runningThreads.Add (thread);
	}

	protected void LogAll (string message, params object[] args)
	{
		textviewLog.Buffer.Text += string.Format (message, args) + "\n";
		textviewLog.QueueDraw ();
		Console.WriteLine (message, arg: args);
	}

	protected void Log (string message)
	{
		textviewLog.Buffer.Text += message + "\n";
		textviewLog.QueueDraw ();
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
			entrySummary.Text = "";
			textviewBefore.Buffer.Text = "";
			textviewAfter.Buffer.Text = "";
			vboxConfirmEdit.Sensitive = false;

			Console.WriteLine ("Thread interrupted [no worries]");
		}
		Console.WriteLine ("Thread finished");
		buttonCancel.Sensitive = false;
		btnUpdate.Sensitive = true;
		btnUpdate.GrabFocus ();
	}

	protected void PageDoneCallback (string title)
	{
		Console.WriteLine ("In PageDoneCallback {0}", title);
		entryStart.Text = title;
		entryStart.QueueDraw ();
	}

	protected volatile string saveCallbackAnswer = null;

	protected bool SaveCallback (string title,
	                             string summary, out string changedSummary,
	                             string before, string after, out string changedWikitext)
	{
		PageDoneCallback (title);
		if (!checkbuttonConfirm.Active) {
			changedSummary = summary;
			changedWikitext = after;
			return true;
		}

		entrySummary.Text = summary;
		textviewBefore.Buffer.Text = before;
		textviewAfter.Buffer.Text = after;
		vboxConfirmEdit.Sensitive = true;
		buttonSkip.GrabFocus ();
		saveCallbackAnswer = null;

		/* 
		 * Trying to use thread interruption caused exceptions
		 * in Api.Save().
		 */

		while (saveCallbackAnswer == null) {
			Thread.Sleep (1000);
		}
		changedSummary = entrySummary.Text;
		changedWikitext = textviewAfter.Buffer.Text;
		vboxConfirmEdit.Sensitive = false;
		entrySummary.Text = "";
		textviewBefore.Buffer.Text = "";
		textviewAfter.Buffer.Text = "";
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
