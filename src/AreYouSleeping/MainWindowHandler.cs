namespace AreYouSleeping
{
    public static class MainWindowHandler
    {
        public static MainWindow? Instance { get; set; } = null;


        public static void ShowOrFocus()
        {
            if (Instance == null)
            {
                var mainWindow = new MainWindow();
                mainWindow.Show();
            }
            else
            {
                Instance.Focus();
            }
        }
    }
}
