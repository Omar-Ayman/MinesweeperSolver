using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using Accessibility;

namespace MineSweeperSolver
{
    public class Win32API
    {
        /*as this project was built for "Any-CPU", it always imports functions from the dlls found at: "C:\Windows\System32"*/

        /// <summary>
        /// defines the x- and y- coordinates of a point.
        /// </summary>
        public struct POINT
        {
            /// <summary>
            /// Specifies the x-coordinate of the point. 
            /// </summary>
            public int x;

            /// <summary>
            /// Specifies the y-coordinate of the point.
            /// </summary>
            public int y;

            public POINT(int x, int y)
            {
                this.x = x;
                this.y = y;
            }
        }

        public delegate bool EnumDesktopWindowsProc(IntPtr hwnd, IntPtr lParam);

        /// <summary>
        /// enumerates all windows in a desktop by passing the handle of each window, in turn, to an application-defined callback function.
        /// </summary>
        /// <param name="hWndParent">handle to desktop to enumerate</param>
        /// <param name="enumFunc">points to application's callback function</param>
        /// <param name="lParam">32-bit value to pass to the callback function</param>
        /// <returns>The EnumDesktopWindows function repeatedly invokes the 'enumFunc' callback function until the last window is enumerated or the callback function returns FALSE.</returns>
        [DllImport("user32.dll")]
        public static extern bool EnumDesktopWindows(IntPtr hWndParent, EnumDesktopWindowsProc enumFunc, IntPtr lParam);

        /// <summary>
        /// retrieves information about the specified window. The function also retrieves the 32-bit (long) value at the specified offset into the extra window memory of a window. 
        /// </summary>
        /// <param name="hWnd">handle of window</param>
        /// <param name="nIndex">offset of value to retrieve</param>
        /// <returns>If the function succeeds, the return value is the requested 32-bit value.
        /// If the function fails, the return value is zero. To get extended error information, call GetLastError.
        /// 
        /// </returns>
        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        /// <summary>
        /// Checks if the specified bit is set in the given integer value
        /// </summary>
        /// <param name="Value">the value inside which the bit should be checked</param>
        /// <param name="bit">the bit to check for in the given value</param>
        /// <returns></returns>
        public static bool IsBitSet(int Value, int bit)
        {
            return (Value & bit) == bit;
        }

        /// <summary>
        /// copies the text of the specified window's title bar (if it has one) into a buffer. If the specified window is a control, the text of the control is copied. 
        /// </summary>
        /// <param name="hWnd">handle of window or control with text</param>
        /// <param name="text">address of buffer for text</param>
        /// <param name="nMax">maximum number of characters to copy</param>
        /// <returns>If the function succeeds, the return value is the length, in characters, of the copied string, not including the terminating null character. If the window has no title bar or text, if the title bar is empty, or if the window or control handle is invalid, the return value is zero.</returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int nMax);

        public static string GetWindowText(IntPtr hWnd)
        {
            StringBuilder stringBuilder = new StringBuilder(261);
            Win32API.GetWindowText(hWnd, stringBuilder, 260);
            return stringBuilder.ToString();
        }

        /// <summary>
        /// retrieves the identifier of the thread that created the specified window and, optionally, the identifier of the process that created the window. This function supersedes the GetWindowTask function.
        /// </summary>
        /// <param name="hwnd">handle of window</param>
        /// <param name="dwProcessId">address of variable for process identifier</param>
        /// <returns>The return value is the identifier of the thread that created the window.</returns>
        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern uint GetWindowThreadProcessId(IntPtr hwnd, out uint dwProcessId);

        [DllImport("oleacc.dll")]
        public static extern int WindowFromAccessibleObject(IAccessible acc, ref IntPtr hwnd);

        [DllImport("oleacc.dll")]
        public static extern int AccessibleObjectFromPoint(POINT pt, [In] [Out] ref IAccessible ppvObject, [In] [Out] ref object varChild);

        public static Guid IID_IAccessible = new Guid(1636251360, 15421, 4559, 129, 12, 0, 170, 0, 56, 155, 113);

        [DllImport("oleacc.dll")]
        public static extern int AccessibleObjectFromWindow(IntPtr hwnd, uint id, ref Guid iid, [In] [Out] [MarshalAs(UnmanagedType.IUnknown)] ref object ppvObject);

        [DllImport("oleacc.dll")]
        public static extern int AccessibleObjectFromEvent(IntPtr hwnd, int idObject, int idChild, [In] [Out] ref IAccessible ppvObject, [In] [Out] ref object varChild);

        [DllImport("oleacc.dll")]
        public static extern int AccessibleChildren(IAccessible paccContainer, int iChildStart, int cChildren, [In] [Out] [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] object[] rgvarChildren, out int pcObtained);

        [DllImport("oleacc.dll", CharSet = CharSet.Unicode)]
        public static extern int GetRoleText(int dwRole, StringBuilder lpszRole, uint cchRoleMax);

    }
}
