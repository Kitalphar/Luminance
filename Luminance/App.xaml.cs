using System.IO;
using System.Windows;
using Luminance.Helpers;
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
            //string connectionString = $"Data Source={dbPath};Version=3;";
            string connectionString = $"Data Source={dbPath};";

            // Initialize the App database service
            AppDatabaseService.Initialize(connectionString);

            //UI thread exceptions
            this.DispatcherUnhandledException += (s, e) =>
            {
                HandleFatalError(e.Exception);
                e.Handled = true; // prevent crash
            };

            //Non-UI thread exceptions
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                if (e.ExceptionObject is Exception ex)
                    HandleFatalError(ex);
            };

            //Unobserved task exceptions
            TaskScheduler.UnobservedTaskException += (s, e) =>
            {
                HandleFatalError(e.Exception);
                e.SetObserved();
            };

            StartupWindow startupWindow = new StartupWindow();
            StartupWindowViewModel startupWindowViewModel = new StartupWindowViewModel(); 

            startupWindow.DataContext = startupWindowViewModel;
            startupWindow.ShowDialog();
        }

        private void HandleFatalError(Exception ex)
        {
            //Send through errorHandler
            string msg = ErrorHandler.FindErrorMessage(ex.Message);
            ErrorHandler.ShowErrorMessage(msg);
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
