using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using Aris.SystemExtension.Windows;

namespace GeelyPTL
{
    /// <summary>
    /// 应用程序入口。
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            TimeSpan timeout = TimeSpan.FromSeconds(10);
            DateTime beginTime = DateTime.Now;

            while (Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName).Length > 1 && DateTime.Now - beginTime < timeout)
                Thread.Sleep(1);

            if (App.OtherApplicationInstanceIsRunning())
            {
                App.Current.Shutdown();
                return;
            }
        }

        static bool OtherApplicationInstanceIsRunning()
        {
            GuidAttribute[] guidAttributeArray = (GuidAttribute[])Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(GuidAttribute), false);
            Guid currentAppId = new Guid(guidAttributeArray[0].Value);
            string appUniqueName = currentAppId.ToString();

            return SingletonWpfApplication.Instance.IsRunning(appUniqueName);
        }

        private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show(e.Exception.ToString(), "Geely PTL", MessageBoxButton.OK, MessageBoxImage.Error);

            e.Handled = true;
        }

    }
}