using System;
using Gtk;
using UI = Gtk.Builder.ObjectAttribute;

namespace gtclient
{
    class ConnectWindow : Window
    {
        //[UI] private Label _label1 = null;
        //[UI] private Button _button1 = null;
        [UI] private Button btnConnect = null;

        private int _counter;

        public ConnectWindow() : this(new Builder("connect.glade")) { }

        private ConnectWindow(Builder builder) : base(builder.GetRawOwnedObject("ConnectWindow"))
        {
            builder.Autoconnect(this);

            DeleteEvent += Window_DeleteEvent;
            //_button1.Clicked += Button1_Clicked;
            btnConnect.Clicked += onbtnConnectClick;
        }

        private void Window_DeleteEvent(object sender, DeleteEventArgs a)
        {
            Application.Quit();
        }

        private void Button1_Clicked(object sender, EventArgs a)
        {
            _counter++;
            //_label1.Text = "Hello World! This button has been clicked " + _counter + " time(s).";
        }

        private void onbtnConnectClick(object sender, EventArgs a)
        {
            
        }
    }
}
