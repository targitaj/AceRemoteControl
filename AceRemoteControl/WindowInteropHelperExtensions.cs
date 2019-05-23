using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

namespace AceRemoteControl
{
    public static class WindowInteropHelperExtensions
    {
        /// <summary>
        /// Creates the HWND of the window if the HWND has not been created yet.
        /// </summary>
        /// <param name="helper">An instance of WindowInteropHelper class.</param>
        /// <returns>An IntPtr that represents the HWND.</returns>
        /// <remarks>Use the EnsureHandle method when you want to separate
        /// window handle (HWND) creation from the
        /// actual showing of the managed Window.</remarks>
        public static IntPtr EnsureHandle(this WindowInteropHelper helper)
        {
            if (helper == null)
                throw new ArgumentNullException("helper");

            if (helper.Handle == IntPtr.Zero)
            {
                var window = (Window)typeof(WindowInteropHelper).InvokeMember("_window",
                    BindingFlags.GetField | BindingFlags.Instance | BindingFlags.NonPublic,
                    null, helper, null);

                typeof(Window).InvokeMember("SafeCreateWindow",
                    BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.NonPublic,
                    null, window, null);
            }

            return helper.Handle;
        }
    }
}

