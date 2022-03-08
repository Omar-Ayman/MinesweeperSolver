using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

namespace MineSweeperSolver
{
    public partial class Form2 : Form
    {
        public List<Accessible> WindowsList = new List<Accessible>();
        public Accessible MineSweeperWindow;
        public List<Accessible> RawsAccessible = new List<Accessible>();
        public List<List<Accessible>> Cells = new List<List<Accessible>>();
        public Point GridDimentions;

        public Form2()
        {
            InitializeComponent();
        }

        // Minesweeper

        private void button1_Click(object sender, EventArgs e)
        {
            try { BackgroundImage.Dispose(); GC.Collect(); }
            catch (Exception) { }
            Rectangle WorkingArea = Screen.GetWorkingArea(this);
            Bitmap ScreenShot = new Bitmap(WorkingArea.Width, WorkingArea.Height);
            Graphics graph = Graphics.FromImage(ScreenShot);
            graph.CopyFromScreen(0, 0, 0, 0, WorkingArea.Size);
            BackgroundImage = ImageProcessing.FindHorizontalEdges(ScreenShot);
            ScreenShot.Dispose();
        }

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

        private void button3_Click(object sender, EventArgs e)
        {
            Accessible accessible = null;
            try
            {
                //Accessible.FromWindow((IntPtr)listBox1.SelectedItem, out accessible);

            }
            catch (Exception)
            {
                //accessible = (Accessible)listBox1.SelectedItem;
            }

            Text = accessible.Name;

            Accessible[] children = null;
            accessible.Children(out children);

            //foreach (Accessible child in children)
            //    if (!listBox1.Items.Contains(child))
            //    {
            //        listBox1.Items.Add(child);
            //        IntPtr hwnd = new IntPtr();
            //        AccCheck.Win32API.WindowFromAccessibleObject(child.IAccessible, ref hwnd);

            //        listBox1.Items.Add(child.Location.ToString());
            //    }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            WindowsList.Clear();
            RawsAccessible.Clear();
            Cells.Clear();

            Win32API.EnumDesktopWindows(IntPtr.Zero, this.TopLevelWindows, IntPtr.Zero);

            foreach (Accessible window in WindowsList)
                if (window.Name == "Minesweeper")
                {
                    MineSweeperWindow = window;
                    foreach (Accessible Child in window.Children())
                        if (Child.Name == "Minesweeper")
                        {
                            MineSweeperWindow = Child;
                            break;
                        }
                    break;
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

            Text = GridDimentions.ToString();
        }


    }
}
