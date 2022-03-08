using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MineSweeperSolver
{
    public class Solver
    {
        private Form3 form;
        private Grid grid;
        private Queue<Cell> queue, deadend;
        private int remaining;
        public bool stop;

        public Cell Uncover
        {
            get;
            private set;
        }

        public Solver(Form3 form, int width, int height, int i = -1, int j = -1)
        {
            this.form = form;
            this.grid = new Grid(width, height);
            remaining = width * height;

            if ((i < 0 || i >= width) && (j < 0 || j >= height))
                Uncover = grid.randomCell();
            else
            {
                Random rand = new Random();
                if (i < 0 || i >= width)
                    i = grid.normalRandom(rand, width);
                if (j < 0 || j >= height)
                    j = grid.normalRandom(rand, height);
            }
            Uncover = grid[i, j];
        }

        public bool solve()
        {
            queue = new Queue<Cell>();
            deadend = new Queue<Cell>();
            queue.Enqueue(grid.randomCell());
            while (remaining > 0 && queue.Count > 0)
            {
                probagate();
                if (remaining > 0)
                    deduce();
            }
            queue = null;
            deadend = null;
            return remaining == 0;
        }

        public void probagate()
        {

        }

        public void deduce()
        {

        }
    }

    public class Cell
    {
        public bool visible, visited, complete, marked;
        public int bombs, x, y;

        public Cell(int i, int j)
        {
            x = i;
            y = j;
        }

        public bool Equals(Cell c)
        {
            return x == c.x && y == c.y;
        }

        public String ToString()
        {
            return "(" + x + ", " + y + ")";
        }
    }

    public class Grid
    {
        private int width, height;
        private Cell[,] cells;
        private int[] xoffset = { -1, -1, 0, 1, 1, 1, 0, -1 }, yoffset = { 0, 1, 1, 1, 0, -1, -1, -1 };

        public Grid(int width, int height)
        {
            if (width <= 0 || height <= 0)
                throw new IndexOutOfRangeException();
            this.width = width;
            this.height = height;
            this.cells = new Cell[width, height];
            for (int i = 0; i < width; ++i)
                for (int j = 0; j < height; ++j)
                    cells[i, j] = new Cell(i, j);
        }

        public Cell this[int i, int j]
        {
            get { return cells[i, j]; }
            set { cells[i, j] = value; }
        }

        public List<Cell> neighbours(int x, int y)
        {
            List<Cell> l = new List<Cell>(8);
            for (int n = 0; n < 8; ++n)
                try { l.Add(cells[x + xoffset[n], y + yoffset[n]]); }
                catch (IndexOutOfRangeException) { }
            return l;
        }

        public List<Cell> neighbours(Cell c)
        {
            return neighbours(c.x, c.y);
        }

        public Cell randomCell()
        {
            Random rand = new Random();
            int x = normalRandom(rand, width),
                y = normalRandom(rand, height);
            return cells[x, y];
        }

        public int normalRandom(Random rand, int limit)
        {
            double mu = limit / 2;
            double sigma = limit / 4;
            double u1 = rand.NextDouble();
            double u2 = rand.NextDouble();
            double rand_std_normal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
            double rand_normal = mu + sigma * rand_std_normal;
            return (int)Math.Abs(rand_normal);
        }

        public int normalRandom(int limit)
        {
            return normalRandom(new Random(), limit);
        }
    }
}
