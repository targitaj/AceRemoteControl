﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using System.IO.Pipes;
using System.Linq;
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
using Gma.System.MouseKeyHook;
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
        public const string UpDownFile = "updown.txt";

        private static string _keyBoardName;

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

            if (File.Exists("keyboards.txt"))
            {
                keyboards = File.ReadAllLines("keyboards.txt").ToList();
            }

            _keyBoardName = ConfigurationManager.AppSettings["KeyBoardName"];
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

            if (e.Device.Name.Contains(_keyBoardName) && e.KeyPressState == KeyPressState.Up)
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
                    case Key.Up:
                        UpDown(true);
                        break;
                    case Key.Down:
                        UpDown(false);
                        break;
                    case Key.Enter:
                        UpDown(null);
                        break;
                }
            }
            //ShowInformation(args.Name.Substring(6));
            //Event = e;
            //DeviceCount = _rawInput.NumberOfKeyboards;
            //e.Handled = (ShouldHandle.IsChecked == true);
        }

        private IKeyboardMouseEvents m_GlobalHook;

        private void StartWndProcHandler(IntPtr hwnd)
        {
            _rawInput = new RawPresentationInput(hwnd, RawInputCaptureMode.ForegroundAndBackground);
            _rawInput.KeyPressed += OnKeyPressed;

            m_GlobalHook = Hook.GlobalEvents();

            m_GlobalHook.MouseDownExt += (sender, args) =>
            {
                if (args.Button == MouseButtons.Right)
                {
                    if (Application.Current.MainWindow is Information mainWindow)
                    {
                        System.Windows.Forms.Cursor.Position = new System.Drawing.Point((int)mainWindow.Left + 4, (int)mainWindow.Top + 4);
                        //mainWindow.Close();
                    }
                }
            };

            m_GlobalHook.MouseUp += (sender, args) =>
            {
                if (args.Button == MouseButtons.Right)
                {
                    if (Application.Current.MainWindow is Information mainWindow)
                    {
                        //System.Windows.Forms.Cursor.Position = new System.Drawing.Point((int)mainWindow.Left + 4, (int)mainWindow.Top + 4);
                        mainWindow.Close();
                    }
                }
            };
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
                Application.Current.MainWindow.Show();
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

            Application.Current.MainWindow.Activate();
        }

        public void UpDown(bool? isUp)
        {
            var mainWindow = Application.Current.MainWindow as Information;

            if (mainWindow == null)
            {
                Application.Current.MainWindow = new Information();
                Application.Current.MainWindow.Show();
            }

            ((Information)Application.Current.MainWindow).UpDown(isUp);

            Application.Current.MainWindow.Activate();
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
