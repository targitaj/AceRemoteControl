using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using log4net;
using TCDaemonTray.Controls;

namespace AceRemoteControl
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private ILog _logger = LogManagerHelper.GetLogger<App>();

        private NotifyIconWpf notifyIcon;

        protected override void OnStartup(StartupEventArgs e)
        {
            notifyIcon = (NotifyIconWpf) FindResource("NotifyIconWpf");

            notifyIcon.Icon = new BitmapImage(new Uri("/AceRemoteControl;component/remote_control.ico",
                UriKind.RelativeOrAbsolute));
            ;

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            DispatcherUnhandledException += (sender, args) =>
            {
                _logger.Debug("DispatcherUnhandledException", args.Exception);
                args.Handled = true;
            };
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            _logger.Debug("CurrentDomain_UnhandledException", e.ExceptionObject as Exception);
        }


        /// <summary>
        /// Receives exception all messages and stack trace
        /// </summary>
        /// <param name="exception">Exception</param>
        public static string GetExceptionFullInformation(Exception exception)
        {
            var result = string.Empty;
            var exceptionList = new List<Exception>();

            while (exception != null && !exceptionList.Contains(exception))
            {
                result += exception.Message + Environment.NewLine + exception.StackTrace + Environment.NewLine;

                exceptionList.Add(exception);
                exception = exception.InnerException;
            }

            return result;
        }
    }
}
