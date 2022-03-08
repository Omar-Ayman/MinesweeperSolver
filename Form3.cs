using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MineSweeperSolver
{
    public partial class Form3 : Form
    {
        public MineSweeperMemory minesweeperMemory;
        public Solver solver;

        public Form3()
        {
            InitializeComponent();
        }

        public void next()
        {
            Cell cell = solver.Uncover;
            try
            {
                cell.bombs = (int) minesweeperMemory.GetCellValue(minesweeperMemory.Cells[cell.x][cell.y]).Value;
            }
            catch (Exception)
            {
                MessageBox.Show("This Looks Like a new game.");
                solve.PerformClick();
                next();
            }
        }

        private void solve_Click(object sender, EventArgs e)
        {
            initialize();
            //try
            //{
                solver = new Solver(this, minesweeperMemory.GridDimentions.X, minesweeperMemory.GridDimentions.Y, 8, 0);
            //}
            //catch (Exception)
            //{
            //    MessageBox.Show("Enter valid bounds");
            //    return;
            //}

            if (solver.solve())
                MessageBox.Show("The game is solved :)");
            else
                MessageBox.Show("This game could not be solved by certainity only");

            Refresh();
            GC.Collect();
        }

        private void initialize()
        {
            try
            {
                for (int x = 0; x < minesweeperMemory.GridDimentions.X; x++)
                    for (int y = 0; y < minesweeperMemory.GridDimentions.Y; y++)
                    {
                        WindowCell cell;

                        cell = new WindowCell(minesweeperMemory.Cells[x][y].Name);
                    }
            }
            catch (Exception)
            {
                minesweeperMemory = new MineSweeperMemory();
                initialize();
                return;
            }
        }

        private void Form3_KeyDown(object sender, KeyEventArgs e)
        {
            if ((e.KeyCode & (Keys.Shift | Keys.Control | Keys.P)) == (Keys.Shift | Keys.Control | Keys.P))
                solver.stop = true;

            else if ((e.KeyCode & (Keys.Shift | Keys.Control | Keys.C)) == (Keys.Shift | Keys.Control | Keys.C))
                solver.stop = false;
        }

        private void pause_Click(object sender, EventArgs e)
        {
            solver.stop = true;

            while (solver.stop)
            {
                System.Windows.Forms.Application.DoEvents();
            }
        }

        private void cont_Click(object sender, EventArgs e)
        {
            solver.stop = false;
        }
    }
}
