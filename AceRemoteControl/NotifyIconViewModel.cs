using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Pipes;
using System.Security.Permissions;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;
using AceRemoteControl;
using Microsoft.Win32;
using NHotkey.Wpf;
using Prism.Commands;
using Prism.Mvvm;
using RawInputProcessor;
using Application = System.Windows.Application;

namespace AceRemoteControl
{
    /// <summary>
    /// Provides bindable properties and commands for the NotifyIconWpf. In this sample, the
    /// view model is assigned to the NotifyIconWpf in XAML. Alternatively, the startup routing
    /// in App.xaml.cs could have created this view model, and assigned it to the NotifyIconWpf.
    /// </summary>
    public class NotifyIconViewModel : BindableBase
    {
        public const string HistoryFile = "history.txt";

        /// <summary>
        /// Shows TC Daemon Updater log
        /// </summary>
        [ExcludeFromCodeCoverage]
        public ICommand ChannelSetupCommand => new DelegateCommand(ShowMainWindow);

        public ICommand ExitCommand => new DelegateCommand(() =>
        {
            HotkeyManager.Current.Remove("Decimal");
            Environment.Exit(0);
        });

        //public static List<string> Records = new List<string>();

        public ICommand DoubleClick => new DelegateCommand(() => { ShowMainWindow(); });

        /// <summary>
        /// Constructor for <see cref="NotifyIconViewModel"/>
        /// </summary>
        [ExcludeFromCodeCoverage]
        public NotifyIconViewModel()
        {
            var window = new Window();
            var handle = new WindowInteropHelper(window).EnsureHandle();
            StartWndProcHandler(handle);

            if ((bool)(DesignerProperties.IsInDesignModeProperty.GetMetadata(typeof(DependencyObject)).DefaultValue))
            {
                return;
            }

            //HotkeyManager.Current.AddOrReplace("Divide", Key.Divide, ModifierKeys.None,
            //    (e, args) =>
            //    {
            //        new Process()
            //        {
            //            StartInfo =
            //                {
            //                    CreateNoWindow = true,
            //                    WindowStyle = ProcessWindowStyle.Hidden,
            //                    FileName = Environment.Is64BitOperatingSystem && !Environment.Is64BitProcess
            //                        ? Environment.ExpandEnvironmentVariables(@"%windir%\sysnative\DisplaySwitch.exe")
            //                        : "DisplaySwitch.exe",
            //                    Arguments = " /extend"
            //                }
            //        }.Start();
            //    });


            //HotkeyManager.Current.AddOrReplace("Decimal", Key.Decimal, ModifierKeys.None,
            //    (e, args) =>
            //    {
            //        Process.Start(new ProcessStartInfo("shutdown", "/s /t 1 /f")
            //        {
            //            CreateNoWindow = true,
            //            UseShellExecute = false
            //        });
            //    });

            //HotkeyManager.Current.AddOrReplace("Subtract", Key.Subtract, ModifierKeys.None,
            //    (e, args) =>
            //    {
            //        //Records.Add(DateTime.Now.ToString("O") + " START");

            //        if (!File.Exists(HistoryFile))
            //        {
            //            File.WriteAllText(HistoryFile, "0");
            //        }

            //        var mychannels = MainWindowModel.ReadChannels();
            //        var myNumber = int.Parse(File.ReadAllText(HistoryFile));
            //        myNumber++;

            //        if (mychannels.Count > 0)
            //        {
            //            myNumber = mychannels.Count > myNumber ? myNumber : 0;
            //        }

            //        //Records.Add(DateTime.Now.ToString("O") + " Before ShowInformation");

            //        ShowInformation(myNumber.ToString(), false);

            //        //Records.Add(DateTime.Now.ToString("O") + " END");
            //    });

            //HotkeyManager.Current.AddOrReplace("Add", Key.Add, ModifierKeys.None,
            //    (e, args) =>
            //    {
            //        if (!File.Exists(HistoryFile))
            //        {
            //            File.WriteAllText(HistoryFile, "0");
            //        }

            //        var mychannels = MainWindowModel.ReadChannels();
            //        var myNumber = int.Parse(File.ReadAllText(HistoryFile));
            //        myNumber--;

            //        if (mychannels.Count > 0)
            //        {
            //            myNumber = myNumber < 0 ? mychannels.Count - 1 : myNumber;
            //        }

            //        ShowInformation(myNumber.ToString(), false);
            //    });

            //RegisterNums();
        }

        private RawPresentationInput _rawInput;
        private List<string> keyboards = new List<string>();

        private void OnKeyPressed(object sender, RawInputEventArgs e)
        {
            lock (keyboards)
            {
                if (!keyboards.Contains(e.Device.Name))
                {
                    keyboards.Add(e.Device.Name);
                    File.AppendAllText("keyboards.txt", e.Device.Name + Environment.NewLine);
                }
            }

            if (e.Device.Name.Contains("VID_2319&PID_0014") && e.KeyPressState == KeyPressState.Up)
            {
                List<Channel> mychannels = null;
                int myNumber = 0;
                e.Handled = true;

                switch (e.Key)
                {
                    case Key.D1:
                        ShowInformation("1");
                        break;
                    case Key.D2:
                        ShowInformation("2");
                        break;
                    case Key.D3:
                        ShowInformation("3");
                        break;
                    case Key.D4:
                        ShowInformation("4");
                        break;
                    case Key.D5:
                        ShowInformation("5");
                        break;
                    case Key.D6:
                        ShowInformation("6");
                        break;
                    case Key.D7:
                        ShowInformation("7");
                        break;
                    case Key.D8:
                        ShowInformation("8");
                        break;
                    case Key.D9:
                        ShowInformation("9");
                        break;
                    case Key.D0:
                        ShowInformation("0");
                        break;
                    case Key.PageUp:
                        if (!File.Exists(HistoryFile))
                        {
                            File.WriteAllText(HistoryFile, "0");
                        }

                        mychannels = MainWindowModel.ReadChannels();
                        myNumber = int.Parse(File.ReadAllText(HistoryFile));
                        myNumber++;

                        if (mychannels.Count > 0)
                        {
                            myNumber = mychannels.Count > myNumber ? myNumber : 0;
                        }

                        ShowInformation(myNumber.ToString(), false);

                        break;
                    case Key.PageDown:
                        
                            if (!File.Exists(HistoryFile))
                            {
                                File.WriteAllText(HistoryFile, "0");
                            }

                            mychannels = MainWindowModel.ReadChannels();
                            myNumber = int.Parse(File.ReadAllText(HistoryFile));
                            myNumber--;

                            if (mychannels.Count > 0)
                            {
                                myNumber = myNumber < 0 ? mychannels.Count - 1 : myNumber;
                            }

                            ShowInformation(myNumber.ToString(), false);
                        break;
                        
                }
            }
            //ShowInformation(args.Name.Substring(6));
            //Event = e;
            //DeviceCount = _rawInput.NumberOfKeyboards;
            //e.Handled = (ShouldHandle.IsChecked == true);
        }

        private void StartWndProcHandler(IntPtr hwnd)
        {
            _rawInput = new RawPresentationInput(hwnd, RawInputCaptureMode.ForegroundAndBackground);
            _rawInput.KeyPressed += OnKeyPressed;
            //DeviceCount = _rawInput.NumberOfKeyboards;
        }

        private void RegisterNums()
        {
            for (int i = (int) Key.NumPad0; i <= (int) Key.NumPad9; i++)
            {
                HotkeyManager.Current.AddOrReplace(((Key) i).ToString(), (Key) i, ModifierKeys.None,
                    (e, args) => { Application.Current.Dispatcher.Invoke(() =>
                    {
                        ShowInformation(args.Name.Substring(6));
                    }); });
            }
        }

        public void ShowInformation(string text, bool add = true)
        {
            var mainWindow = Application.Current.MainWindow as Information;

            if (mainWindow == null)
            {
                Application.Current.MainWindow = new Information();

                //Records.Add(DateTime.Now.ToString("O") + " Before show");

                Application.Current.MainWindow.Show();

                //Records.Add(DateTime.Now.ToString("O") + " After show");
            }



            var infText = ((Information) Application.Current.MainWindow).Text;

            if (add)
            {
                ((Information)Application.Current.MainWindow).Text = infText + text;
            }
            else
            {
                ((Information)Application.Current.MainWindow).Text = text;
            }

            //Records.Add(DateTime.Now.ToString("O") + " Before Activate");

            Application.Current.MainWindow.Activate();


            //Records.Add(DateTime.Now.ToString("O") + " After Activate");
        }

        /// <summary>
        /// Show main window
        /// </summary>
        [ExcludeFromCodeCoverage]
        private void ShowMainWindow()
        {
            var mainWindow = Application.Current.MainWindow as MainWindow;

            if (mainWindow == null)
            {
                Application.Current.MainWindow = new MainWindow();
                Application.Current.MainWindow.Show();
            }

            Application.Current.MainWindow.Activate();
        }

        /// <summary>
        /// Set log information to main window
        /// </summary>
        /// <param name="logText">Log text</param>
        /// <param name="title">Log title</param>
        /// <param name="updateLogCommand">Command for log updating</param>
        [ExcludeFromCodeCoverage]
        private void SetLogInformation(string logText, string title, ICommand updateLogCommand)
        {
            //Application.Current.Dispatcher.Invoke(() =>
            //{
            //    var mainViewModel = (Application.Current.MainWindow as MainWindow)?.DataContext as MainViewModel;

            //    if (mainViewModel != null)
            //    {
            //        mainViewModel.LogText = logText;
            //        mainViewModel.Title = title;
            //        mainViewModel.UpdateLogCommand = updateLogCommand;
            //    }
            //});
        }
    }
}
