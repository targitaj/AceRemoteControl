using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Newtonsoft.Json;
using RawInputProcessor;

namespace AceRemoteControl
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private const string FILE_SETTINGS = "settings.json";

        private RawPresentationInput _rawInput;
        private int _deviceCount;
        private RawInputEventArgs _event;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowModel();

            if (File.Exists(FILE_SETTINGS))
            {
                var settings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText(FILE_SETTINGS));

                Height = settings.Height ?? Height;
                Width = settings.Width ?? Width;
                Left = settings.X ?? Left;
                Top = settings.Y ?? Top;
            }

            Closing += (sender, args) =>
            {
                File.WriteAllText(FILE_SETTINGS, JsonConvert.SerializeObject(new Settings()
                {
                    Height = Height,
                    Width = Width,
                    X = Left,
                    Y = Top
                }));
            };
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void UIElement_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            File.AppendAllText("keylog.txt", e.Key.ToString());
        }

        private void Control_OnPreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Information.StartVideo("0", null, ((Channel)((ListBox)sender).SelectedItems[0]).Text);
        }

        private void ToggleButton_OnChecked(object sender, RoutedEventArgs e)
        {
            foreach (var lb1Item in lb1.Items)
            {
                ((Channel) lb1Item).IsSelected = false;
            }
        }

        private void ToggleButton_OnChecked1(object sender, RoutedEventArgs e)
        {
            foreach (var lb1Item in lb2.Items)
            {
                ((Channel)lb1Item).IsSelected = false;
            }
        }

        public RawInputEventArgs Event
        {
            get { return _event; }
            set
            {
                _event = value;
                //OnPropertyChanged();
            }
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            //StartWndProcHandler();
            base.OnSourceInitialized(e);
        }

        private void OnKeyPressed(object sender, RawInputEventArgs e)
        {
            //Event = e;
            //DeviceCount = _rawInput.NumberOfKeyboards;
            //e.Handled = (ShouldHandle.IsChecked == true);
        }

        private void StartWndProcHandler()
        {
            //_rawInput = new RawPresentationInput((new HwndSource)null, RawInputCaptureMode.Foreground);
            //_rawInput.KeyPressed += OnKeyPressed;
                //DeviceCount = _rawInput.NumberOfKeyboards;
        }
    }
}
