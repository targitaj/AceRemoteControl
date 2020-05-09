using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using log4net;
using Application = System.Windows.Application;

namespace AceRemoteControl
{
    /// <summary>
    /// Interaction logic for Information.xaml
    /// </summary>
    public partial class Information : Window
    {
        //private static ILog _logger = LogManagerHelper.GetLogger<Information>();
        private static DateTime _closeTime;
        private static Thread _lastThread;
        private static Thread _windowCloseThread;
        private static Thread _monitorStatusThread;

        private const int SW_MAXIMIZE = 3;
        private const int SW_MINIMIZE = 6;
        [DllImport("user32.dll", EntryPoint = "FindWindow")]
        public static extern IntPtr FindWindowByCaption(IntPtr ZeroOnly, string lpWindowName);
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private bool IsUpDown
        {
            get { return _isUpDown;}
            set
            {
                _isUpDown = value;

                if (value)
                {
                    
                    Dispatcher.Invoke(() =>
                    {
                        spTop.Visibility = Visibility.Collapsed;
                        spBottom.Visibility = Visibility.Visible;

                    });
                }
                else
                {
                    Dispatcher.Invoke(() =>
                    {
                        spTop.Visibility = Visibility.Visible;
                        spBottom.Visibility = Visibility.Collapsed;

                    });
                }
                Dispatcher.Invoke(() =>
                {
                    SetPosition();
                });

            }
        } 

        public string Text
        {
            get { return tbText?.Text; }
            set
            {
                IsUpDown = false;
                tbText.Text = value;
                _closeTime = DateTime.Now.AddSeconds(2);

                var mychannels = MainWindowModel.ReadChannels();
                var myNumber = int.Parse(tbText.Text);
                var myChannel = string.Empty;

                if (mychannels.Count > 0)
                {
                    myNumber = mychannels.Count > myNumber ? myNumber : mychannels.Count - 1;
                    myChannel = mychannels[myNumber].Text;
                }

                File.WriteAllText(NotifyIconViewModel.HistoryFile, myNumber.ToString());

                tbName.Text = myChannel;
                StartVideo(tbText.Text, this, string.Empty);
            }
        }

        public void UpDown(bool? isUp)
        {
            string number = "0";

            if (!IsUpDown)
            {
                if (File.Exists(NotifyIconViewModel.HistoryFile))
                {
                    number = File.ReadAllText(NotifyIconViewModel.HistoryFile);
                    number = (int.Parse(number) - 1).ToString();
                }

                File.WriteAllText(NotifyIconViewModel.UpDownFile, number);
            }
            


            number = File.ReadAllText(NotifyIconViewModel.UpDownFile);

            var myNumber = int.Parse(number);

            if (!isUp.HasValue && IsUpDown)
            {
                Text = number;
            }
            else
            {
                var mychannels = MainWindowModel.ReadChannels();
                if (isUp == true)
                {
                    myNumber--;

                    if (myNumber < 0)
                    {
                        myNumber = mychannels.Count - 1;
                    }
                }
                else if (isUp == false)
                {
                    myNumber++;

                    if (myNumber >= mychannels.Count)
                    {
                        myNumber = 0;
                    }
                }

                var first = myNumber - 2;
                var second = myNumber - 1;
                var forth = myNumber + 1;
                var fith = myNumber + 2;

                if (first == -1)
                {
                    first = mychannels.Count - 1;
                }

                if (first == -2)
                {
                    first = mychannels.Count - 2;
                    second = mychannels.Count - 1;
                }

                if (fith == mychannels.Count)
                {
                    fith = 0;
                }

                if (fith == mychannels.Count + 1)
                {
                    forth = 0;
                    fith = 1;
                }

                tb1.Text = first + " " + mychannels[first].Text;
                tb2.Text = second + " " + mychannels[second].Text;
                tb3.Text = myNumber + " " + mychannels[myNumber].Text;
                tb4.Text = forth + " " + mychannels[forth].Text;
                tb5.Text = fith + " " + mychannels[fith].Text;

                IsUpDown = true;

                File.WriteAllText(NotifyIconViewModel.UpDownFile, myNumber.ToString());
            }
        }

        public static void StartVideo(string nuber, Window window, string text)
        {
            TryAction(() => { stream?.Close(); });
            TryAction(() => { stream?.Dispose(); });
            TryAction(() => { contextResponse.OutputStream.Close(); });
            TryAction(() => { contextResponse.OutputStream.Dispose(); });
            TryAction(() => { contextResponse.Close(); });
            TryAction(() => { httpListener.Close(); });
            TryAction(() => { _lastThread.Abort(); });
            TryAction(() => { _windowCloseThread.Abort(); });
            TryAction(() => { _monitorStatusThread.Abort(); });
            TryAction(() => { httpListener.Close(); });

            _windowCloseThread = new Thread(() =>
            {
                Thread.Sleep(2000);
                if (_closeTime <= DateTime.Now)
                {
                    Application.Current.Dispatcher.Invoke(() => { window?.Close(); });
                }
            });

            if (ConfigurationManager.AppSettings["UseEdem"] == "false")
            {
                _lastThread = new Thread(() =>
                {
                    try
                    {
                        started = null;
                        Application.Current.Dispatcher.Invoke(() => { Helper.RefreshTrayArea(); });
                        string list = GetListOfChannels();
                        var channels = MainWindowModel.ReadChannels();
                        var number = int.Parse(nuber);
                        var channel = text;

                        if (channels.Count > 0 && channel == string.Empty)
                        {
                            number = channels.Count > number ? number : channels.Count - 1;
                            channel = channels[number].Text;

                        }

                        if (!string.IsNullOrWhiteSpace(channel))
                        {
                            string regex =
                                $"{Regex.Escape($",{channel}")}\n(.*?)\n(.*?)\n";
                            var matches = Regex.Matches(list, regex, RegexOptions.Singleline);
                            var url = matches[0].Groups[2].Value;

                            var vlcEngineProcess = Process.GetProcessesByName("mpc-hc64");
                            foreach (var process in vlcEngineProcess)
                            {
                                process.Kill();
                            }

                            Process.Start(ConfigurationManager.AppSettings["AcePlayerPath"], $" /monitor {ConfigurationManager.AppSettings["ScreenNumber"]} /fullscreen {url}");
                        }
                    }
                    catch
                    {
                    }
                });

                _lastThread.Start();
                _windowCloseThread.Start();

            }
        }

        private static int tryCount = 0;
        private static bool? started = null;
        private static Stream stream = null;
        private static HttpListenerResponse contextResponse;
        private static HttpListener httpListener;
        private bool _isUpDown;

        private static void MonitorStatus(string url, string nuber, Window window, string text)
        {
            
            
            try
            {
                var tryTimeTill = DateTime.Now.AddMinutes(1);
                httpListener = new HttpListener();
                WebResponse response = null;
                WebRequest req;
                var processes = Process.GetProcessesByName("chrome").ToList();

                var chromeKill = new Action(() =>
                
                    
                    {
                        try
                        {
                            DateTime dtDateTime = DateTime.Now;


                            var founded = false;
                            while (dtDateTime > DateTime.Now.AddSeconds(-30) && !founded)
                            {
                                var newProcesses = Process.GetProcessesByName("chrome").ToList();

                                var newProc = newProcesses.Where(n =>
                                    !processes.Select(s => s.Id).Contains(n.Id)).ToList();

                                if (newProc.Count != 0)
                                {
                                    founded = true;
                                    newProc.ForEach(f =>
                                    {
                                        ShowWindow(f.MainWindowHandle, SW_MINIMIZE);
                                        try
                                        {
                                            f.Kill();
                                        }
                                        catch (Exception e)
                                        {

                                        }

                                    });

                                    processes.ForEach(f => { ShowWindow(f.MainWindowHandle, SW_MINIMIZE); });
                                }
                                else
                                {
                                    Thread.Sleep(100);
                                }
                            }
                        }
                        catch 
                        {
                        }

                });



                Thread th = null;

                while (DateTime.Now <= tryTimeTill)
                {
                    try
                    {
                        th = new Thread(chromeKill.Invoke);
                        th.Start();
                        req = WebRequest.Create(url);
                        req.Timeout = 5000;
                        response = req.GetResponse();
                        
                        break;
                    }
                    catch (Exception e)
                    {
                        TryAction(() => { th?.Abort(); });
                        Thread.Sleep(20);
                    }
                }

                if (response == null)
                {
                    started = false;
                    return;
                }

                stream = response.GetResponseStream();

                string localHostUrl = "http://localhost";
                string port = "9988";

                string prefix = $"{localHostUrl}:{port}/";
                httpListener.Prefixes.Add(prefix);
                httpListener.Start();
                started = true;

                var context = httpListener.GetContext();

                contextResponse = context.Response;
                SaveStreamToFile("VideoFileMonitorStatus.avi", stream, contextResponse.OutputStream);
                //stream?.CopyTo(contextResponse.OutputStream);

                //tryCount = 0;

                var aceEngineFileInfo = new FileInfo(ConfigurationManager.AppSettings["AceEnginePath"]);
                var aceEngineProcess = Process.GetProcessesByName(
                    aceEngineFileInfo.Name.Replace(aceEngineFileInfo.Extension, ""));

                foreach (var process in aceEngineProcess)
                {
                    try
                    {
                        process.Kill();
                    }
                    catch { }
                }

                StartVideo(nuber, window, text);

                //_logger.Debug("Kill after SaveStreamToFile");
                //File.AppendAllText("kill.txt",
                //    DateTime.Now.ToString("O") + " " + "Was killed");

                
                TryAction(()=>{ stream?.Close(); });
                TryAction(() => { stream?.Dispose(); });
                TryAction(() => { contextResponse.OutputStream.Close(); });
                TryAction(() => { contextResponse.OutputStream.Dispose(); });
                TryAction(() => { contextResponse.Close(); });
                TryAction(() => { httpListener.Close(); });
            }
            catch (HttpListenerException le)
            {
                //_logger.Debug("All is ok HttpListenerException");
            }
            catch (Exception e)
            {
                var aceEngineFileInfo = new FileInfo(ConfigurationManager.AppSettings["AceEnginePath"]);
                var aceEngineProcess = Process.GetProcessesByName(
                    aceEngineFileInfo.Name.Replace(aceEngineFileInfo.Extension, ""));

                foreach (var process in aceEngineProcess)
                {
                    try
                    {
                        process.Kill();
                    }
                    catch { }
                }

                //_logger.Debug("Killed", e);
                //File.AppendAllText("kill.txt",
                //    DateTime.Now.ToString("O") + " " + "Was killed" + Environment.NewLine +
                //    App.GetExceptionFullInformation(e) + Environment.NewLine);
                StartVideo(nuber, window, text);
            }
            finally
            {
                if (stream != null)
                {
                    stream.Close();
                    stream.Dispose();
                }
            }
        }

        private static void TryAction(Action tryAction)
        {
            try
            {
                tryAction.Invoke();
            }
            catch { }
        }

        public static void SaveStreamToFile(string fileFullPath, Stream stream, Stream webStream)
        {
            File.Delete(fileFullPath);
            FileStream fileStream = null;

            try
            {
                fileStream = File.Create(fileFullPath);

                int length = 10000000;
                var bytes = new byte[length];
                int offset;
                DateTime endTime = DateTime.Now.AddSeconds(5);
                DateTime closeTime = DateTime.Now.AddSeconds(1);
                do
                {
                    stream.ReadTimeout = 10000;
                    offset = stream.Read(bytes, 0, length);

                    //if (Config.WriteVideoFileMonitorStatus)
                    //{
                    fileStream.Write(bytes, 0, offset);
                    webStream.Write(bytes, 0, offset);
                    //}

                    if (closeTime <= DateTime.Now)
                    {
                        fileStream.Flush();
                        closeTime = DateTime.Now.AddSeconds(1);
                        fileStream.Close();
                        fileStream.Dispose();

                        fileStream = File.Open(fileFullPath, FileMode.Append);
                    }

                    if (offset > 0)
                    {
                        endTime = DateTime.Now.AddSeconds(5);
                    }

                    
                } while (endTime >= DateTime.Now);
            }
            catch (Exception exception)
            {
                //_logger.Debug("SaveStreamToFile", exception);
                throw;
            }
            finally
            {
                if (fileStream != null)
                {
                    fileStream.Close();
                    fileStream.Dispose();
                }
            }
        }

        public static string GetListOfChannels()
        {
            string list;

            var channelList = new FileInfo(ConfigurationManager.AppSettings["FileName"]);
            if (!channelList.Exists || channelList.LastWriteTime < DateTime.Now.AddMinutes(-2))
            {
                using (WebClient myWebClient = new WebClient())
                {
                    list = myWebClient.DownloadString(ConfigurationManager.AppSettings["AceContentIdList"]);
                }

                byte[] bytes = Encoding.Default.GetBytes(list);
                list = Encoding.UTF8.GetString(bytes);
                File.WriteAllText(channelList.FullName, list, Encoding.UTF8);
            }
            else
            {
                list = File.ReadAllText(channelList.FullName);
            }

            return list;
        }

        public Information()
        {
            InitializeComponent();
            Activated += (sender, args) =>
            {
                SetPosition();
                //var returnScreens = new Func<Screen[]>(() => { return Screen.AllScreens; });

                //Task<Screen[]> task = new Task<Screen[]>();

                ;

                //var screens = await Task.Run(() => Screen.AllScreens);
                ////while (screens.Length <= 1)
                ////{
                ////    Thread.Sleep(100);
                ////    screens = await Task.Run(() => Screen.AllScreens);
                ////}

                //var notPrimary = screens.FirstOrDefault(f => !Equals(f, Screen.PrimaryScreen));

                //if (notPrimary == null)
                //{
                //    notPrimary = screens.First();
                //}

                //Left = notPrimary.Bounds.X + 40;
                //Top = 40;
            };
        }

        private void SetPosition()
        {
            var screens = Screen.AllScreens;

            var notPrimary = screens.FirstOrDefault(f => !Equals(f, Screen.PrimaryScreen));

            if (notPrimary == null)
            {
                notPrimary = screens.First();
            }

            Left = notPrimary.Bounds.X + 40;

            if (IsUpDown)
            {
                Top = notPrimary.Bounds.Height - 40 - Height;

                System.Windows.Forms.Cursor.Position = new System.Drawing.Point((int)Left + 4, (int)Top + 4);
            }
            else
            {
                Top = 40;
            }

            //Activate();
            //Focus();
            GlobalActivate(this);
        }

        const UInt32 SWP_NOSIZE = 0x0001;
        const UInt32 SWP_NOMOVE = 0x0002;
        const UInt32 SWP_SHOWWINDOW = 0x0040;

        /// <summary>
        /// Activate a window from anywhere by attaching to the foreground window
        /// </summary>
        public static void GlobalActivate(Window w)
        {
            //Get the process ID for this window's thread
            var interopHelper = new WindowInteropHelper(w);
            var thisWindowThreadId = GetWindowThreadProcessId(interopHelper.Handle, IntPtr.Zero);

            //Get the process ID for the foreground window's thread
            var currentForegroundWindow = GetForegroundWindow();
            var currentForegroundWindowThreadId = GetWindowThreadProcessId(currentForegroundWindow, IntPtr.Zero);

            //Attach this window's thread to the current window's thread
            AttachThreadInput(currentForegroundWindowThreadId, thisWindowThreadId, true);

            //Set the window position
            SetWindowPos(interopHelper.Handle, new IntPtr(0), 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE | SWP_SHOWWINDOW);

            //Detach this window's thread from the current window's thread
            AttachThreadInput(currentForegroundWindowThreadId, thisWindowThreadId, false);

            //Show and activate the window
            if (w.WindowState == WindowState.Minimized) w.WindowState = WindowState.Normal;
            w.Show();
            w.Activate();
        }

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, IntPtr ProcessId);

        [DllImport("user32.dll")]
        private static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);

        [DllImport("user32.dll")]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    }
}
