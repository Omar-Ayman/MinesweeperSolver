using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace MineSweeperSolver
{
    public class MineSweeperMemory
    {
        #region mouse_event
        [DllImport("user32.dll", EntryPoint = "SetCursorPos", ExactSpelling = true)]
        internal static extern bool SetCursorPos(int x, int y);

        /// <summary>
        /// synthesizes mouse motion and button clicks
        /// </summary>
        /// <param name="dwFlags">flags specifying various motion/click variants</param>
        /// <param name="dx">horizontal mouse position or position change</param>
        /// <param name="dy">vertical mouse position or position change</param>
        /// <param name="dwData">amount of wheel movement</param>
        /// <param name="dwExtraInfo">32 bits of application-defined information</param>
        [DllImport("user32.dll", EntryPoint = "mouse_event", ExactSpelling = true)]
        public static extern void mouse_event(mouse_eventFlags dwFlags, int dx, int dy, int dwData, int dwExtraInfo);

        public enum mouse_eventFlags
        {
            /// <summary>
            /// Specifies that the dx and dy parameters contain normalized absolute coordinates. If not set, those parameters contain relative data: the change in position since the last reported position. This flag can be set, or not set, regardless of what kind of mouse or mouse-like device, if any, is connected to the system.
            /// Coordinate (0,0) maps onto the upper-left corner of the display surface, (65535,65535) maps onto the lower-right corner. 
            /// </summary>
            MOUSEEVENTF_ABSOLUTE = 0x8000,

            /// <summary>
            /// Specifies that movement occurred.
            /// </summary>
            MOUSEEVENTF_MOVE = 0x1,

            /// <summary>
            /// Specifies that the left button changed to down.
            /// </summary>
            MOUSEEVENTF_LEFTDOWN = 0x2,

            /// <summary>
            /// Specifies that the left button changed to up.
            /// </summary>
            MOUSEEVENTF_LEFTUP = 0x4,

            /// <summary>
            /// Specifies that the right button changed to down.
            /// </summary>
            MOUSEEVENTF_RIGHTDOWN = 0x8,

            /// <summary>
            /// Specifies that the right button changed to up.
            /// </summary>
            MOUSEEVENTF_RIGHTUP = 0x10,

            /// <summary>
            /// Specifies that the middle button changed to down.
            /// </summary>
            MOUSEEVENTF_MIDDLEDOWN = 0x20,

            /// <summary>
            /// Specifies that the middle button changed to up.
            /// </summary>
            MOUSEEVENTF_MIDDLEUP = 0x40,

            /// <summary>
            ///  Windows NT only: Specifies that the wheel has been moved, if the mouse has a wheel. The amount of movement is given in dwData
            /// </summary>
            MOUSEEVENTF_WHEEL = 0x800
        }
        #endregion

        [DllImport("user32.dll", EntryPoint = "SetForegroundWindow", ExactSpelling = true)]
        internal static extern bool SetForegroundWindow(IntPtr Hwnd);

        [DllImport("user32.dll", EntryPoint = "ShowWindow", ExactSpelling = true)]
        internal static extern bool ShowWindow(IntPtr Hwnd, CmdShow_SW nCmdShow);
        public enum CmdShow_SW
        {
            SW_HIDE = 0,
            /// <summary>
            /// Activates and displays a window. If the window is minimized or maximized, Windows restores it to its original size and position. An application should specify this flag when displaying the window for the first time
            /// </summary>
            SW_NORMAL = 1,
            SW_MINIMIZED = 2,
            SW_MAXIMIZED = 3,
            SW_SHOWNOACTIVATE = 4,
            /// <summary>
            /// Activates the window and displays it in its current size and position
            /// </summary>
            SW_SHOW = 5,
            SW_MINIMIZE = 6,
            SW_SHOWMINNOACTIVE = 7,
            SW_SHOWNA = 8,
            SW_RESTORE = 9,
            SW_SHOWDEFAULT = 10,
            SW_FORCEMINIMIZE = 11,
            SW_MAX = 11
        }

        public List<Accessible> WindowsList = new List<Accessible>();
        public Accessible MineSweeperWindow;
        public List<Accessible> RawsAccessible = new List<Accessible>();
        public List<List<Accessible>> Cells = new List<List<Accessible>>();
        public Point GridDimentions;

        private bool TopLevelWindows(IntPtr hwnd, IntPtr lParam)
        {
            int windowLong = Win32API.GetWindowLong(hwnd, -16);
            if (Win32API.IsBitSet(windowLong, 268435456))
            {
                string WindowCaption = Win32API.GetWindowText(hwnd);
                WindowsList.Add(Accessible.FromWindow(hwnd));
            }

            return true;
        }

        public MineSweeperMemory()
        {
        MethodStart:
            WindowsList.Clear();
            RawsAccessible.Clear();
            Cells.Clear();

            Win32API.EnumDesktopWindows(IntPtr.Zero, this.TopLevelWindows, IntPtr.Zero);

            // find the minesweeper game
            foreach (Accessible window in WindowsList)
                if (window.Name == "Minesweeper")
                {
                    MineSweeperWindow = window;
                    Accessible[] children = window.Children();
                    foreach (Accessible Child in window.Children())
                        if (Child.Name == "Minesweeper")
                        {
                            MineSweeperWindow = Child;
                            break;
                        }
                    break;
                }
            try
            {
                if (MineSweeperWindow != null)
                    ;
                else
                    throw new Exception("Minesweeper is not running,\n please start \"Minesweeper\" then try again.");
            }
            catch (Exception ex)
            {
                if (MessageBox.Show(ex.Message, "Error!", MessageBoxButtons.RetryCancel) == DialogResult.Retry)
                    goto MethodStart;
                else
                    Application.Restart();
            }

            foreach (Accessible Raw in MineSweeperWindow.Children())
                if (Raw.Name != "Status information")
                {
                    RawsAccessible.Add(Raw);

                    List<Accessible> rawList = new List<Accessible>();
                    foreach (Accessible cell in Raw.Children())
                        rawList.Add(cell);
                    Cells.Add(rawList);
                }

            GridDimentions = new Point(RawsAccessible.Count, RawsAccessible[0].Children().Count());
        }

        public WindowCell GetCellValue(Accessible Cell)
        {
            WindowCell CurrentCell = new WindowCell(Cell.Name);

            if (CurrentCell.Concealed)
            {
                RevealCell(Cell, CurrentCell.Flagged, CurrentCell.QuestionMark);
                CurrentCell = new WindowCell(Cell.Name);
            }

            return CurrentCell;
        }

        public void MoveMouse(Point NewPosition)
        {
            const int steps = 20;
            for (int i = 0; i <= steps; i++)
            {
                Point PreviousMousePosition = Control.MousePosition;
                SetCursorPos(PreviousMousePosition.X - (PreviousMousePosition.X - NewPosition.X) / steps * i, PreviousMousePosition.Y - (PreviousMousePosition.Y - NewPosition.Y) / steps * i);
                Application.DoEvents();
                if (i != steps)
                    System.Threading.Thread.Sleep(25);
            }
        }

        public void MoveMouse(int X, int Y)
        {
            MoveMouse(new Point(X, Y));
        }

        public void Mouse_RightClick()
        {
            mouse_event(mouse_eventFlags.MOUSEEVENTF_RIGHTDOWN | mouse_eventFlags.MOUSEEVENTF_RIGHTUP, 0, 0, 0, 0);
        }

        public void Mouse_LeftClick()
        {
            mouse_event(mouse_eventFlags.MOUSEEVENTF_LEFTDOWN | mouse_eventFlags.MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
        }

        public void FlagCell(Accessible Cell, bool flagged, bool QuestionMark)
        {
            Point PreviousMousePosition = Control.MousePosition;
            Point ClickPosition = new Point((Cell.Location.Right + Cell.Location.Left) / 2, (Cell.Location.Top + Cell.Location.Bottom) / 2);

            SetForegroundWindow(Cell.Hwnd);

            //MoveMouse(ClickPosition);
            SetCursorPos(ClickPosition.X, ClickPosition.Y);
            if (ClickPosition.X < 0 || ClickPosition.Y < 0)
            {
                ShowWindow(Cell.Hwnd, CmdShow_SW.SW_NORMAL);
                FlagCell(Cell, flagged, QuestionMark);
                return;
            }

            SetForegroundWindow(Cell.Hwnd);
            if (!QuestionMark)
            {
                Mouse_RightClick();
            }
            else
            {
                Mouse_RightClick();
                Mouse_RightClick();
            }
            Thread.Sleep(50);

            SetForegroundWindow(Application.OpenForms[0].Handle);
            //SetCursorPos(PreviousMousePosition.X, PreviousMousePosition.Y);
        }

        public void RevealCell(Accessible Cell, bool flagged, bool QuestionMark)
        {
            Point PreviousMousePosition = Control.MousePosition;
            Point ClickPosition = new Point((Cell.Location.Right + Cell.Location.Left) / 2, (Cell.Location.Top + Cell.Location.Bottom) / 2);

            
            SetForegroundWindow(Cell.Hwnd);

            //MoveMouse(ClickPosition);
            SetCursorPos(ClickPosition.X, ClickPosition.Y);
            if (ClickPosition.X < 0 || ClickPosition.Y < 0)
            {
                ShowWindow(Cell.Hwnd, CmdShow_SW.SW_NORMAL);
                RevealCell(Cell, flagged, QuestionMark);
                return;
            }

            if (flagged)
            {
                SetForegroundWindow(Application.OpenForms[0].Handle);
                if (MessageBox.Show("This Cell Is Flagged, Do you want to Unflag & Reveal it?", "warning!", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No)
                    return;

                SetForegroundWindow(Cell.Hwnd);
                Thread.Sleep(50);

                Mouse_RightClick();
                Mouse_RightClick();
            }

            if (QuestionMark)
            {
                SetForegroundWindow(Cell.Hwnd);

                Mouse_RightClick();
            }

            SetForegroundWindow(Cell.Hwnd);

            Mouse_LeftClick();
            Thread.Sleep(50);

            SetForegroundWindow(Application.OpenForms[0].Handle);
            //SetCursorPos(PreviousMousePosition.X, PreviousMousePosition.Y);
        }
    }
}
