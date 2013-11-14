using System;
using Gtk;

namespace translinkupdater
{
	public partial class SignInWindow : Gtk.Dialog
	{
		public SignInWindow ()
		{
			this.Build ();
			buttonOk.GrabFocus();
		}

		protected void OnButtonOkClicked (object sender, EventArgs e)
		{
			labelStatus.Text = "Loggar in...";
			if (Api.SignIn (entryUsername.Text, entryPassword.Text)) {
				Console.WriteLine("Successfully signed in {0}", Api.SignedInUser);
				//Api.SavePage("Användare:Skalman/test", "testar lite...", "test");
				Destroy();
			} else {
				labelStatus.Text = "Fel användarnamn/lösenord";
				entryPassword.Text = "";
				entryPassword.GrabFocus();
			}
		}
	}
}

