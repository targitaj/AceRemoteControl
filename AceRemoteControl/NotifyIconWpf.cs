using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Application = System.Windows.Application;
using MenuItem = System.Windows.Controls.MenuItem;

namespace TCDaemonTray.Controls
{
    /// <summary>
    /// Class for tray icon displaying
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class NotifyIconWpf : FrameworkElement, IDisposable
    {
        private NotifyIcon _notifyIcon = new NotifyIcon();

        private Dictionary<object, System.Windows.Forms.MenuItem> _menuItemsMap =
            new Dictionary<object, System.Windows.Forms.MenuItem>();

        /// <summary>
        /// Identifies the <see cref="ToolTipTextProperty"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ToolTipTextProperty = DependencyProperty.Register(
            nameof(ToolTipText), typeof(string), typeof(NotifyIconWpf),
            new PropertyMetadata(
                (d, e) =>
                {
                    var notifyIcon = (NotifyIconWpf)d;
                    notifyIcon._notifyIcon.Text = (string)e.NewValue;
                }));

        /// <summary>
        /// Tool tip text
        /// </summary>
        public string ToolTipText
        {
            get { return (string)GetValue(ToolTipTextProperty); }
            set { SetValue(ToolTipTextProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="IconProperty"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IconProperty = DependencyProperty.Register(
            nameof(Icon), typeof(BitmapImage), typeof(NotifyIconWpf),
            new PropertyMetadata(
                (d, e) =>
                {
                    var notifyIcon = (NotifyIconWpf)d;
                    var bitmapImage = (BitmapImage)e.NewValue;

                    notifyIcon._notifyIcon.Icon = bitmapImage != null
                        ? new Icon(Application.GetResourceStream(bitmapImage.UriSource).Stream)
                        : null;
                }));

        /// <summary>
        /// Icon
        /// </summary>
        public ImageSource Icon
        {
            get { return (ImageSource)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }

        public static readonly DependencyProperty DoubleClickProperty = DependencyProperty.Register(
            nameof(DoubleClick), typeof(ICommand), typeof(NotifyIconWpf));

        public ICommand DoubleClick
        {
            get { return (ICommand)GetValue(DoubleClickProperty); }
            set { SetValue(DoubleClickProperty, value); }
        }

        /// <summary>
        /// Constructor for <see cref="NotifyIconWpf"/>
        /// </summary>
        public NotifyIconWpf()
        {
            var contextMenuPropertyDescriptor =
                DependencyPropertyDescriptor.FromProperty(ContextMenuProperty, typeof(NotifyIconWpf));

            contextMenuPropertyDescriptor.AddValueChanged(this, (sender, args) =>
            {
                _notifyIcon.ContextMenu = new System.Windows.Forms.ContextMenu();
                _menuItemsMap.Clear();
                ContextMenu.DataContext = DataContext;

                foreach (var item in ContextMenu.Items)
                {
                    var formsItem = ContextMenuItemToFormsItem(item);
                    _notifyIcon.ContextMenu.MenuItems.Add(formsItem);

                    _menuItemsMap.Add(item, formsItem);
                }
            });

            _notifyIcon.Visible = true;

            if (PreviewMouseLeftButtonUpEvent != null)
            {

            }

            var dataContextPropertyDescriptor =
                DependencyPropertyDescriptor.FromProperty(DataContextProperty, typeof(NotifyIconWpf));

            dataContextPropertyDescriptor.AddValueChanged(this, (sender, args) =>
            {
                if (ContextMenu != null)
                {
                    ContextMenu.DataContext = DataContext;
                }
            });

            _notifyIcon.DoubleClick += _notifyIcon_DoubleClick;
        }

        private void _notifyIcon_DoubleClick(object sender, EventArgs e)
        {
            DoubleClick?.Execute(null);
        }

        private System.Windows.Forms.MenuItem ContextMenuItemToFormsItem(object @object)
        {
            var menuItem = @object as MenuItem;
            if (menuItem != null)
            {
                var formsMenuItem = new System.Windows.Forms.MenuItem(menuItem.Header?.ToString(), (sender, args) =>
                {
                    menuItem.Command.Execute(null);
                });

                formsMenuItem.Visible = menuItem.Visibility == Visibility.Visible;
                formsMenuItem.Enabled = menuItem.IsEnabled;

                var menuItemHeaderPropertyDescriptor =
                    DependencyPropertyDescriptor.FromProperty(HeaderedItemsControl.HeaderProperty, typeof(MenuItem));

                menuItemHeaderPropertyDescriptor.AddValueChanged(menuItem, (sender, args) =>
                {
                    var desciptorMenuItem = (MenuItem)sender;
                    _menuItemsMap[desciptorMenuItem].Text = desciptorMenuItem.Header.ToString();
                });

                var menuItemIsEnabledPropertyDescriptor =
                    DependencyPropertyDescriptor.FromProperty(IsEnabledProperty, typeof(MenuItem));

                menuItemIsEnabledPropertyDescriptor.AddValueChanged(menuItem, (sender, args) =>
                {
                    var desciptorMenuItem = (MenuItem)sender;
                    _menuItemsMap[desciptorMenuItem].Enabled = desciptorMenuItem.IsEnabled;
                });

                return formsMenuItem;
            }

            if (@object is Separator)
            {
                return new System.Windows.Forms.MenuItem("-");
            }

            throw new InvalidCastException("Can't cast " + @object?.GetType() + " to " + typeof(System.Windows.Forms.MenuItem));
        }

        public void Dispose()
        {
            _notifyIcon?.Dispose();
        }
    }
}
