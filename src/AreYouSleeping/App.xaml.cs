using System;
using System.Windows;


namespace AreYouSleeping
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            var icon = new System.Windows.Forms.NotifyIcon
            {
                Icon = ProjectResources.SysTrayIcon,
                Visible = true
            };

            var contextMenu = new System.Windows.Forms.ContextMenuStrip();
            contextMenu.Items.Add(new System.Windows.Forms.ToolStripMenuItem("Configure", null, Configure_Clicked));
            contextMenu.Items.Add(new System.Windows.Forms.ToolStripMenuItem("Exit", null, Exit_Clicked));

            icon.ContextMenuStrip = contextMenu;
            icon.DoubleClick += Icon_DoubleClick;

            base.OnStartup(e);
        }

        private void Icon_DoubleClick(object? sender, EventArgs e)
        {
            MainWindowHandler.ShowOrFocus();
        }

        private void Configure_Clicked(object? sender, EventArgs e)
        {
            MainWindowHandler.ShowOrFocus();
        }

        private void Exit_Clicked(object? sender, EventArgs e)
        {
            this.Shutdown();
        }
    }
}
