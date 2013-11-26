
// This file has been generated by the GUI designer. Do not modify.

public partial class MainWindow
{
	private global::Gtk.VBox vbox2;
	private global::Gtk.HBox hbox2;
	private global::Gtk.VBox vbox4;
	private global::Gtk.Label lblHeading;
	private global::Gtk.Table table2;
	private global::Gtk.Button btnUpdate;
	private global::Gtk.Button buttonCancel;
	private global::Gtk.CheckButton checkbuttonConfirm;
	private global::Gtk.Entry entryMax;
	private global::Gtk.Entry entryStart;
	private global::Gtk.Label label5;
	private global::Gtk.Label label6;
	private global::Gtk.VBox vbox5;
	private global::Gtk.Label lblLog;
	private global::Gtk.HBox hbox4;
	private global::Gtk.ScrolledWindow GtkScrolledWindow;
	private global::Gtk.TextView textviewLog;
	private global::Gtk.VBox vboxConfirmEdit;
	private global::Gtk.HBox hbox3;
	private global::Gtk.Frame frame1;
	private global::Gtk.Alignment GtkAlignment2;
	private global::Gtk.ScrolledWindow GtkScrolledWindow1;
	private global::Gtk.TextView textviewBefore;
	private global::Gtk.Label GtkLabel2;
	private global::Gtk.Frame frame2;
	private global::Gtk.Alignment GtkAlignment3;
	private global::Gtk.ScrolledWindow GtkScrolledWindow2;
	private global::Gtk.TextView textviewAfter;
	private global::Gtk.Label GtkLabel3;
	private global::Gtk.HBox hbox1;
	private global::Gtk.Label label1;
	private global::Gtk.Entry entrySummary;
	private global::Gtk.HBox hbox5;
	private global::Gtk.Button buttonSkip;
	private global::Gtk.Button buttonApproveChange;
	
	protected virtual void Build ()
	{
		global::Stetic.Gui.Initialize (this);
		// Widget MainWindow
		this.Name = "MainWindow";
		this.Title = global::Mono.Unix.Catalog.GetString ("Uppdatera {{ö}}");
		this.Icon = global::Gdk.Pixbuf.LoadFromResource ("translinkupdater.trans-icon.png");
		this.WindowPosition = ((global::Gtk.WindowPosition)(4));
		// Container child MainWindow.Gtk.Container+ContainerChild
		this.vbox2 = new global::Gtk.VBox ();
		this.vbox2.Name = "vbox2";
		this.vbox2.Spacing = 6;
		this.vbox2.BorderWidth = ((uint)(10));
		// Container child vbox2.Gtk.Box+BoxChild
		this.hbox2 = new global::Gtk.HBox ();
		this.hbox2.Name = "hbox2";
		this.hbox2.Spacing = 6;
		// Container child hbox2.Gtk.Box+BoxChild
		this.vbox4 = new global::Gtk.VBox ();
		this.vbox4.Name = "vbox4";
		this.vbox4.Spacing = 6;
		// Container child vbox4.Gtk.Box+BoxChild
		this.lblHeading = new global::Gtk.Label ();
		this.lblHeading.Name = "lblHeading";
		this.lblHeading.LabelProp = global::Mono.Unix.Catalog.GetString ("<b>Uppdatera {{ö}} och {{ö-}}</b>");
		this.lblHeading.UseMarkup = true;
		this.vbox4.Add (this.lblHeading);
		global::Gtk.Box.BoxChild w1 = ((global::Gtk.Box.BoxChild)(this.vbox4 [this.lblHeading]));
		w1.Position = 0;
		w1.Expand = false;
		w1.Fill = false;
		// Container child vbox4.Gtk.Box+BoxChild
		this.table2 = new global::Gtk.Table (((uint)(4)), ((uint)(2)), false);
		this.table2.Name = "table2";
		this.table2.RowSpacing = ((uint)(6));
		this.table2.ColumnSpacing = ((uint)(6));
		// Container child table2.Gtk.Table+TableChild
		this.btnUpdate = new global::Gtk.Button ();
		this.btnUpdate.CanDefault = true;
		this.btnUpdate.CanFocus = true;
		this.btnUpdate.Name = "btnUpdate";
		this.btnUpdate.UseUnderline = true;
		this.btnUpdate.Label = global::Mono.Unix.Catalog.GetString ("_Uppdatera sidor");
		this.table2.Add (this.btnUpdate);
		global::Gtk.Table.TableChild w2 = ((global::Gtk.Table.TableChild)(this.table2 [this.btnUpdate]));
		w2.TopAttach = ((uint)(2));
		w2.BottomAttach = ((uint)(3));
		w2.LeftAttach = ((uint)(1));
		w2.RightAttach = ((uint)(2));
		w2.XOptions = ((global::Gtk.AttachOptions)(4));
		w2.YOptions = ((global::Gtk.AttachOptions)(4));
		// Container child table2.Gtk.Table+TableChild
		this.buttonCancel = new global::Gtk.Button ();
		this.buttonCancel.Sensitive = false;
		this.buttonCancel.CanFocus = true;
		this.buttonCancel.Name = "buttonCancel";
		this.buttonCancel.UseUnderline = true;
		this.buttonCancel.Label = global::Mono.Unix.Catalog.GetString ("_Avbryt");
		this.table2.Add (this.buttonCancel);
		global::Gtk.Table.TableChild w3 = ((global::Gtk.Table.TableChild)(this.table2 [this.buttonCancel]));
		w3.TopAttach = ((uint)(2));
		w3.BottomAttach = ((uint)(3));
		w3.XOptions = ((global::Gtk.AttachOptions)(4));
		w3.YOptions = ((global::Gtk.AttachOptions)(4));
		// Container child table2.Gtk.Table+TableChild
		this.checkbuttonConfirm = new global::Gtk.CheckButton ();
		this.checkbuttonConfirm.CanFocus = true;
		this.checkbuttonConfirm.Name = "checkbuttonConfirm";
		this.checkbuttonConfirm.Label = global::Mono.Unix.Catalog.GetString ("Bekräfta före sparning");
		this.checkbuttonConfirm.Active = true;
		this.checkbuttonConfirm.DrawIndicator = true;
		this.checkbuttonConfirm.UseUnderline = true;
		this.table2.Add (this.checkbuttonConfirm);
		global::Gtk.Table.TableChild w4 = ((global::Gtk.Table.TableChild)(this.table2 [this.checkbuttonConfirm]));
		w4.TopAttach = ((uint)(3));
		w4.BottomAttach = ((uint)(4));
		w4.LeftAttach = ((uint)(1));
		w4.RightAttach = ((uint)(2));
		w4.XOptions = ((global::Gtk.AttachOptions)(4));
		w4.YOptions = ((global::Gtk.AttachOptions)(4));
		// Container child table2.Gtk.Table+TableChild
		this.entryMax = new global::Gtk.Entry ();
		this.entryMax.CanFocus = true;
		this.entryMax.Name = "entryMax";
		this.entryMax.Text = global::Mono.Unix.Catalog.GetString ("50");
		this.entryMax.IsEditable = true;
		this.entryMax.ActivatesDefault = true;
		this.entryMax.InvisibleChar = '•';
		this.table2.Add (this.entryMax);
		global::Gtk.Table.TableChild w5 = ((global::Gtk.Table.TableChild)(this.table2 [this.entryMax]));
		w5.TopAttach = ((uint)(1));
		w5.BottomAttach = ((uint)(2));
		w5.LeftAttach = ((uint)(1));
		w5.RightAttach = ((uint)(2));
		w5.XOptions = ((global::Gtk.AttachOptions)(4));
		w5.YOptions = ((global::Gtk.AttachOptions)(4));
		// Container child table2.Gtk.Table+TableChild
		this.entryStart = new global::Gtk.Entry ();
		this.entryStart.CanFocus = true;
		this.entryStart.Name = "entryStart";
		this.entryStart.Text = global::Mono.Unix.Catalog.GetString ("blixt");
		this.entryStart.IsEditable = true;
		this.entryStart.ActivatesDefault = true;
		this.entryStart.InvisibleChar = '•';
		this.table2.Add (this.entryStart);
		global::Gtk.Table.TableChild w6 = ((global::Gtk.Table.TableChild)(this.table2 [this.entryStart]));
		w6.LeftAttach = ((uint)(1));
		w6.RightAttach = ((uint)(2));
		w6.XOptions = ((global::Gtk.AttachOptions)(4));
		w6.YOptions = ((global::Gtk.AttachOptions)(4));
		// Container child table2.Gtk.Table+TableChild
		this.label5 = new global::Gtk.Label ();
		this.label5.Name = "label5";
		this.label5.Xalign = 1F;
		this.label5.LabelProp = global::Mono.Unix.Catalog.GetString ("Börja från");
		this.table2.Add (this.label5);
		global::Gtk.Table.TableChild w7 = ((global::Gtk.Table.TableChild)(this.table2 [this.label5]));
		w7.XOptions = ((global::Gtk.AttachOptions)(4));
		w7.YOptions = ((global::Gtk.AttachOptions)(4));
		// Container child table2.Gtk.Table+TableChild
		this.label6 = new global::Gtk.Label ();
		this.label6.Name = "label6";
		this.label6.Xalign = 1F;
		this.label6.LabelProp = global::Mono.Unix.Catalog.GetString ("Max antal sidor");
		this.table2.Add (this.label6);
		global::Gtk.Table.TableChild w8 = ((global::Gtk.Table.TableChild)(this.table2 [this.label6]));
		w8.TopAttach = ((uint)(1));
		w8.BottomAttach = ((uint)(2));
		w8.XOptions = ((global::Gtk.AttachOptions)(4));
		w8.YOptions = ((global::Gtk.AttachOptions)(4));
		this.vbox4.Add (this.table2);
		global::Gtk.Box.BoxChild w9 = ((global::Gtk.Box.BoxChild)(this.vbox4 [this.table2]));
		w9.Position = 1;
		w9.Expand = false;
		w9.Fill = false;
		this.hbox2.Add (this.vbox4);
		global::Gtk.Box.BoxChild w10 = ((global::Gtk.Box.BoxChild)(this.hbox2 [this.vbox4]));
		w10.Position = 0;
		w10.Expand = false;
		w10.Fill = false;
		// Container child hbox2.Gtk.Box+BoxChild
		this.vbox5 = new global::Gtk.VBox ();
		this.vbox5.Name = "vbox5";
		this.vbox5.Spacing = 6;
		// Container child vbox5.Gtk.Box+BoxChild
		this.lblLog = new global::Gtk.Label ();
		this.lblLog.Name = "lblLog";
		this.lblLog.LabelProp = global::Mono.Unix.Catalog.GetString ("<b>Logg</b>");
		this.lblLog.UseMarkup = true;
		this.vbox5.Add (this.lblLog);
		global::Gtk.Box.BoxChild w11 = ((global::Gtk.Box.BoxChild)(this.vbox5 [this.lblLog]));
		w11.Position = 0;
		w11.Expand = false;
		w11.Fill = false;
		// Container child vbox5.Gtk.Box+BoxChild
		this.hbox4 = new global::Gtk.HBox ();
		this.hbox4.Name = "hbox4";
		this.hbox4.Spacing = 6;
		// Container child hbox4.Gtk.Box+BoxChild
		this.GtkScrolledWindow = new global::Gtk.ScrolledWindow ();
		this.GtkScrolledWindow.Name = "GtkScrolledWindow";
		this.GtkScrolledWindow.ShadowType = ((global::Gtk.ShadowType)(1));
		// Container child GtkScrolledWindow.Gtk.Container+ContainerChild
		this.textviewLog = new global::Gtk.TextView ();
		this.textviewLog.WidthRequest = 300;
		this.textviewLog.HeightRequest = 50;
		this.textviewLog.CanFocus = true;
		this.textviewLog.Name = "textviewLog";
		this.textviewLog.AcceptsTab = false;
		this.GtkScrolledWindow.Add (this.textviewLog);
		this.hbox4.Add (this.GtkScrolledWindow);
		global::Gtk.Box.BoxChild w13 = ((global::Gtk.Box.BoxChild)(this.hbox4 [this.GtkScrolledWindow]));
		w13.Position = 0;
		this.vbox5.Add (this.hbox4);
		global::Gtk.Box.BoxChild w14 = ((global::Gtk.Box.BoxChild)(this.vbox5 [this.hbox4]));
		w14.Position = 1;
		this.hbox2.Add (this.vbox5);
		global::Gtk.Box.BoxChild w15 = ((global::Gtk.Box.BoxChild)(this.hbox2 [this.vbox5]));
		w15.Position = 1;
		this.vbox2.Add (this.hbox2);
		global::Gtk.Box.BoxChild w16 = ((global::Gtk.Box.BoxChild)(this.vbox2 [this.hbox2]));
		w16.Position = 0;
		// Container child vbox2.Gtk.Box+BoxChild
		this.vboxConfirmEdit = new global::Gtk.VBox ();
		this.vboxConfirmEdit.Sensitive = false;
		this.vboxConfirmEdit.Name = "vboxConfirmEdit";
		this.vboxConfirmEdit.Spacing = 6;
		// Container child vboxConfirmEdit.Gtk.Box+BoxChild
		this.hbox3 = new global::Gtk.HBox ();
		this.hbox3.Name = "hbox3";
		this.hbox3.Homogeneous = true;
		this.hbox3.Spacing = 6;
		// Container child hbox3.Gtk.Box+BoxChild
		this.frame1 = new global::Gtk.Frame ();
		this.frame1.Name = "frame1";
		this.frame1.ShadowType = ((global::Gtk.ShadowType)(0));
		// Container child frame1.Gtk.Container+ContainerChild
		this.GtkAlignment2 = new global::Gtk.Alignment (0F, 0F, 1F, 1F);
		this.GtkAlignment2.Name = "GtkAlignment2";
		this.GtkAlignment2.LeftPadding = ((uint)(12));
		// Container child GtkAlignment2.Gtk.Container+ContainerChild
		this.GtkScrolledWindow1 = new global::Gtk.ScrolledWindow ();
		this.GtkScrolledWindow1.Name = "GtkScrolledWindow1";
		this.GtkScrolledWindow1.ShadowType = ((global::Gtk.ShadowType)(1));
		// Container child GtkScrolledWindow1.Gtk.Container+ContainerChild
		this.textviewBefore = new global::Gtk.TextView ();
		this.textviewBefore.HeightRequest = 100;
		this.textviewBefore.CanFocus = true;
		this.textviewBefore.Name = "textviewBefore";
		this.textviewBefore.Editable = false;
		this.textviewBefore.AcceptsTab = false;
		this.textviewBefore.WrapMode = ((global::Gtk.WrapMode)(3));
		this.GtkScrolledWindow1.Add (this.textviewBefore);
		this.GtkAlignment2.Add (this.GtkScrolledWindow1);
		this.frame1.Add (this.GtkAlignment2);
		this.GtkLabel2 = new global::Gtk.Label ();
		this.GtkLabel2.Name = "GtkLabel2";
		this.GtkLabel2.LabelProp = global::Mono.Unix.Catalog.GetString ("<b>Före</b>");
		this.GtkLabel2.UseMarkup = true;
		this.frame1.LabelWidget = this.GtkLabel2;
		this.hbox3.Add (this.frame1);
		global::Gtk.Box.BoxChild w20 = ((global::Gtk.Box.BoxChild)(this.hbox3 [this.frame1]));
		w20.Position = 0;
		// Container child hbox3.Gtk.Box+BoxChild
		this.frame2 = new global::Gtk.Frame ();
		this.frame2.Name = "frame2";
		this.frame2.ShadowType = ((global::Gtk.ShadowType)(0));
		// Container child frame2.Gtk.Container+ContainerChild
		this.GtkAlignment3 = new global::Gtk.Alignment (0F, 0F, 1F, 1F);
		this.GtkAlignment3.Name = "GtkAlignment3";
		this.GtkAlignment3.LeftPadding = ((uint)(12));
		// Container child GtkAlignment3.Gtk.Container+ContainerChild
		this.GtkScrolledWindow2 = new global::Gtk.ScrolledWindow ();
		this.GtkScrolledWindow2.Name = "GtkScrolledWindow2";
		this.GtkScrolledWindow2.ShadowType = ((global::Gtk.ShadowType)(1));
		// Container child GtkScrolledWindow2.Gtk.Container+ContainerChild
		this.textviewAfter = new global::Gtk.TextView ();
		this.textviewAfter.HeightRequest = 100;
		this.textviewAfter.CanFocus = true;
		this.textviewAfter.Name = "textviewAfter";
		this.textviewAfter.AcceptsTab = false;
		this.textviewAfter.WrapMode = ((global::Gtk.WrapMode)(3));
		this.GtkScrolledWindow2.Add (this.textviewAfter);
		this.GtkAlignment3.Add (this.GtkScrolledWindow2);
		this.frame2.Add (this.GtkAlignment3);
		this.GtkLabel3 = new global::Gtk.Label ();
		this.GtkLabel3.Name = "GtkLabel3";
		this.GtkLabel3.LabelProp = global::Mono.Unix.Catalog.GetString ("<b>Efter</b>");
		this.GtkLabel3.UseMarkup = true;
		this.frame2.LabelWidget = this.GtkLabel3;
		this.hbox3.Add (this.frame2);
		global::Gtk.Box.BoxChild w24 = ((global::Gtk.Box.BoxChild)(this.hbox3 [this.frame2]));
		w24.Position = 1;
		this.vboxConfirmEdit.Add (this.hbox3);
		global::Gtk.Box.BoxChild w25 = ((global::Gtk.Box.BoxChild)(this.vboxConfirmEdit [this.hbox3]));
		w25.Position = 0;
		// Container child vboxConfirmEdit.Gtk.Box+BoxChild
		this.hbox1 = new global::Gtk.HBox ();
		this.hbox1.Name = "hbox1";
		this.hbox1.Spacing = 6;
		// Container child hbox1.Gtk.Box+BoxChild
		this.label1 = new global::Gtk.Label ();
		this.label1.Name = "label1";
		this.label1.LabelProp = global::Mono.Unix.Catalog.GetString ("Sammanfattning:");
		this.hbox1.Add (this.label1);
		global::Gtk.Box.BoxChild w26 = ((global::Gtk.Box.BoxChild)(this.hbox1 [this.label1]));
		w26.Position = 0;
		w26.Expand = false;
		w26.Fill = false;
		// Container child hbox1.Gtk.Box+BoxChild
		this.entrySummary = new global::Gtk.Entry ();
		this.entrySummary.CanFocus = true;
		this.entrySummary.Name = "entrySummary";
		this.entrySummary.IsEditable = true;
		this.entrySummary.InvisibleChar = '•';
		this.hbox1.Add (this.entrySummary);
		global::Gtk.Box.BoxChild w27 = ((global::Gtk.Box.BoxChild)(this.hbox1 [this.entrySummary]));
		w27.Position = 1;
		this.vboxConfirmEdit.Add (this.hbox1);
		global::Gtk.Box.BoxChild w28 = ((global::Gtk.Box.BoxChild)(this.vboxConfirmEdit [this.hbox1]));
		w28.Position = 1;
		w28.Expand = false;
		w28.Fill = false;
		// Container child vboxConfirmEdit.Gtk.Box+BoxChild
		this.hbox5 = new global::Gtk.HBox ();
		this.hbox5.Name = "hbox5";
		this.hbox5.Spacing = 6;
		// Container child hbox5.Gtk.Box+BoxChild
		this.buttonSkip = new global::Gtk.Button ();
		this.buttonSkip.CanFocus = true;
		this.buttonSkip.Name = "buttonSkip";
		this.buttonSkip.UseUnderline = true;
		this.buttonSkip.Label = global::Mono.Unix.Catalog.GetString ("Hoppa över");
		this.hbox5.Add (this.buttonSkip);
		global::Gtk.Box.BoxChild w29 = ((global::Gtk.Box.BoxChild)(this.hbox5 [this.buttonSkip]));
		w29.Position = 0;
		// Container child hbox5.Gtk.Box+BoxChild
		this.buttonApproveChange = new global::Gtk.Button ();
		this.buttonApproveChange.CanFocus = true;
		this.buttonApproveChange.Name = "buttonApproveChange";
		this.buttonApproveChange.UseUnderline = true;
		this.buttonApproveChange.Label = global::Mono.Unix.Catalog.GetString ("_Godkänn");
		this.hbox5.Add (this.buttonApproveChange);
		global::Gtk.Box.BoxChild w30 = ((global::Gtk.Box.BoxChild)(this.hbox5 [this.buttonApproveChange]));
		w30.Position = 1;
		this.vboxConfirmEdit.Add (this.hbox5);
		global::Gtk.Box.BoxChild w31 = ((global::Gtk.Box.BoxChild)(this.vboxConfirmEdit [this.hbox5]));
		w31.Position = 2;
		w31.Expand = false;
		w31.Fill = false;
		this.vbox2.Add (this.vboxConfirmEdit);
		global::Gtk.Box.BoxChild w32 = ((global::Gtk.Box.BoxChild)(this.vbox2 [this.vboxConfirmEdit]));
		w32.Position = 1;
		this.Add (this.vbox2);
		if ((this.Child != null)) {
			this.Child.ShowAll ();
		}
		this.DefaultWidth = 664;
		this.DefaultHeight = 410;
		this.btnUpdate.HasDefault = true;
		this.Show ();
		this.DeleteEvent += new global::Gtk.DeleteEventHandler (this.OnDeleteEvent);
		this.buttonCancel.Clicked += new global::System.EventHandler (this.OnButtonCancelClicked);
		this.btnUpdate.Clicked += new global::System.EventHandler (this.OnBtnUpdateClicked);
		this.buttonSkip.Clicked += new global::System.EventHandler (this.OnButtonSkipClicked);
		this.buttonApproveChange.Clicked += new global::System.EventHandler (this.OnButtonApproveChangeClicked);
	}
}
