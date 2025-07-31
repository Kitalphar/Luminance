using System.IO;
using System.Windows;
using Luminance.Services;
using Luminance.ViewModels;

namespace Luminance
{
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            string basePath = AppDomain.CurrentDomain.BaseDirectory;
            string dbPath = Path.Combine(basePath, "App.db");
            string connectionString = $"Data Source={dbPath};Version=3;";

            // Initialize the database service
            AppDatabaseService.Initialize(connectionString);

            //MainWindow mainWindow = new MainWindow();
            //MainWindowViewModel mainWindowViewModel = new MainWindowViewModel();

            //mainWindow.DataContext = mainWindowViewModel;
            //mainWindow.ShowDialog();

            StartupWindow startupWindow = new StartupWindow();
            StartupWindowViewModel startupWindowViewModel = new StartupWindowViewModel(); 

            startupWindow.DataContext = startupWindowViewModel;
            startupWindow.ShowDialog();


            /*
            // Check if the user is authenticated (you could store this in the ViewModel)
            if (loginViewModel.IsAuthenticated)
            {
                // Once login is successful, show the main window
                var mainWindow = new MainWindow();
                mainWindow.Show();
            }
            else
            {
                // If authentication fails, exit the application
                Application.Current.Shutdown();
            }
            */
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);

            // Clean up database services
            /*AppDatabaseService.Instance?.Dispose();*/
            //UserDatabaseService.Instance?.Dispose();

            //EncryptedDatabaseService.Instance?.EncryptAndDispose();

        }

    }

}
