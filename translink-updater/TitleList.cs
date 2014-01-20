using System;

namespace translinkupdater
{
	public partial class TitleList : Gtk.Window
	{
		public TitleList () : 
				base(Gtk.WindowType.Toplevel)
		{
			this.Build ();
		}
	}
}

