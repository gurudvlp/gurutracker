using System;
using Gtk;

namespace gtclient
{
    class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            Application.Init();

            var app = new Application("org.gtclient.gtclient", GLib.ApplicationFlags.None);
            app.Register(GLib.Cancellable.Current);

            var win = new ConnectWindow();
            app.AddWindow(win);

            win.Show();
            Application.Run();
        }
    }
}
