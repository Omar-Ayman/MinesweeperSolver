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
    public partial class Form1 : Form
    {
        public MineSweeperMemory minesweeperMemory;
        public Program program;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        public TextBox GetValue(int x, int y)
        {
            int n = (8 - y) * 9 + 8 - x;

            return (TextBox)panel1.Controls[n];
        }

        public TextBox GetValue(Cell c)
        {
            int n = (8 - c.y) * 9 + 8 - c.x;

            return (TextBox)panel1.Controls[n];
        }

        //public void mark(List<Cell> l, Color color)
        //{
        //    foreach(Cell c in l)
        //    {
        //        GetValue(c).BackColor = color;
        //    }
        //}

        private void Next_Click(object sender, EventArgs e)
        {
            BackColor = SystemColors.ActiveCaption;
            Cell cell = program.uncover;
            TextBox textbox = GetValue(cell);

            if (textbox.Text == "0")
                textbox.Text = "";
            else
                try
                {
                    int bombsCount = int.Parse(textbox.Text);

                    if (bombsCount > 0 && bombsCount < 9)
                        cell.bombs = bombsCount;
                }
                catch (FormatException)
                {
                    try
                    {
                        textbox.Text = minesweeperMemory.GetCellValue(minesweeperMemory.Cells[cell.x][cell.y]).Value.ToString();
                        Next_Click(sender, e);
                        //MessageBox.Show("Please enter an integer from 0 to 8");
                        return;
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("This Looks Like a new game.");
                        button2.PerformClick();
                        Next_Click(sender, e);
                        return;
                    }
                }

            textbox.BackColor = Color.Green;
            cell.visible = true;
            program.stop = false;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            button5.PerformClick();

            Next.Enabled = true;

            //program = new Program(this,9,9,8,0);

            foreach (TextBox txtbx in panel1.Controls)
            {
                txtbx.Text = "";
                txtbx.BackColor = Color.White;
            }

            //while (program.remaining > 0 && program.q.Count > 0)
            //{
                program.probagate();
            //    if (program.remaining > 0)
            //    {
            //        Color temp = BackColor;
            //        BackColor = Color.Magenta;
            //        Application.DoEvents();

            //        program.deduce();

            //        BackColor = temp;
            //        Application.DoEvents();
            //    }
            //}

            if (program.remaining > 0)
                MessageBox.Show("This game could not be solved by certainity only");
            else
                MessageBox.Show("The game is solved :)");

            Refresh();
            Next.Focus();
            GC.Collect();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            new Form2().Show();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            try
            {
                for (int x = 0; x < minesweeperMemory.GridDimentions.X; x++)
                    for (int y = 0; y < minesweeperMemory.GridDimentions.Y; y++)
                    {
                        TextBox txtbx = GetValue(y, x);
                        WindowCell cell;

                        cell = new WindowCell(minesweeperMemory.Cells[y][x].Name);

                        if (!cell.Concealed)
                            txtbx.Text = cell.Value.ToString();
                        if (cell.Flagged)
                            txtbx.BackColor = Color.Red;
                        if (cell.QuestionMark)
                            txtbx.BackColor = Color.LightGray;
                    }
            }
            catch (Exception)
            {
                minesweeperMemory = new MineSweeperMemory();
                button5_Click(sender, e);
                return;
            }
        }
    }
}
