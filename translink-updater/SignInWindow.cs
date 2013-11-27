using System;
using Gtk;

namespace translinkupdater
{
	public partial class SignInWindow : Gtk.Dialog
	{
		public SignInWindow ()
		{
			this.Build ();
			entryPassword.GrabFocus();
		}

		protected void OnButtonOkClicked (object sender, EventArgs e)
		{
			labelStatus.Text = "Loggar in...";
			if (Api.SignIn (entryUsername.Text, entryPassword.Text)) {
				Console.WriteLine("Successfully signed in {0}", Api.SignedInUser);
				Destroy();
			} else {
				Console.WriteLine("Wrong username/password");
				labelStatus.Text = "Fel användarnamn/lösenord";
				entryPassword.Text = "";
				entryPassword.GrabFocus();
			}
		}
		protected void OnButtonCancelClicked (object sender, EventArgs e)
		{
			Destroy ();
		}

	}
}

