using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;
using MineSweeperSolver;

namespace MineSweeperSolver
{
    public class Program
    {
        public Form3 form;
        public Cell uncover;
        public bool stop;
        public Cell[,] grid;
        public Queue<Cell> q, deadend;
        public int width, height, remaining;
       // public static Stack<Cell> s;

        public Program(Form3 f, int h, int w, int i, int j)
        {
            form = f;
            grid = new Cell[h, w];
            height = h;
            width = w;
            remaining = h * w;

            for (int x = 0; x < h; x++)
                for (int y = 0; y < w; y++)
                    grid[x, y] = new Cell(x, y);
            uncover = null;
            q = new Queue<Cell>();
            deadend = new Queue<Cell>();

            Random r = new Random();
            if (i < 0 || i >= height)
                i = r.Next(height);

            if (j < 0 || j >= width)
                j = r.Next(width);

            q.Enqueue(grid[i, j]);
        }

        public void probagate()
        {
            while (q.Count > 0)
            {
                Cell cell = q.Dequeue();
                while (q.Count > 0 && (cell.marked || cell.complete || cell.visited))
                    cell = q.Dequeue();

                List<Cell> neighbour = neighbours(cell);
                if (!cell.visible)
                {
                    //form.GetValue(cell).BackColor = Color.Yellow;

                    uncover = cell;
                    --remaining;
                    //if (new WindowCell(form.minesweeperMemory.Cells[cell.x][cell.y].Name).Concealed)
                    //{
                    //    form.BackColor = SystemColors.Control;
                    //    stop = true;
                    //}
                    //else
                    //{
                    Application.DoEvents();
                    form.next();
                    Application.DoEvents();
                    cell.visible = true;
                    //}

                    foreach (Cell check in neighbour)
                        if (check.visible && !(check.complete || check.marked))
                        {
                            check.visited = false;
                            q.Enqueue(check);
                        }
                }

                cell.visited = true;
                if (cell.bombs == 0)
                {
                    cell.complete = true;
                    foreach (Cell explore in neighbour)
                        if (!(explore.visible && (explore.visited || explore.complete || explore.marked)))
                            q.Enqueue(explore);
                }
                else
                {
                    int bombs = 0;
                    List<Cell> possible = new List<Cell>(9), toSolve = new List<Cell>();
                    foreach (Cell check in neighbour)
                        if (check.marked)
                            ++bombs;
                        else if (!check.visible)
                            possible.Add(check);
                        else if (!(check.complete || check.visited))
                            toSolve.Add(check);
                    bool pop = bombs == cell.bombs, mark = bombs + possible.Count == cell.bombs;

                    if (mark || pop)
                        cell.complete = true;

                    if (pop)
                        toSolve.AddRange(possible);
                    else if (mark)
                    {
                        remaining -= possible.Count;
                        foreach (Cell tomark in possible)
                        {
                            tomark.marked = true;
                            foreach (Cell check in neighbours(tomark))
                                if (check.visible && !(check.complete || check.marked) && !toSolve.Contains(check))
                                    toSolve.Add(check);
                            //TextBox t = form.GetValue(tomark);
                            //t.Text = "B";
                            //t.BackColor = Color.Red;
                            //Application.DoEvents();

                            WindowCell windowCell = new WindowCell(form.minesweeperMemory.Cells[tomark.x][tomark.y].Name);
                            form.minesweeperMemory.FlagCell(form.minesweeperMemory.Cells[tomark.x][tomark.y], windowCell.Flagged, windowCell.QuestionMark);
                        }
                    }

                    if (toSolve.Count == 0)
                        deadend.Enqueue(cell);
                    else
                    {
                        if (pop || mark)
                            foreach (Cell modify in toSolve)
                                modify.visited = false;

                        foreach (Cell solve in toSolve)
                            if (!q.Contains(solve))
                                q.Enqueue(solve);
                    }
                }
            }
        }

        public void deduce()
        {
            foreach (Cell cell in grid)
                if (cell.visible && !(cell.marked || cell.complete))
                    cell.visited = false;

            if (deadend.Count == 0)
                return;
            List<List<bool>> senarios = new List<List<bool>>();
            Dictionary<Cell, int> map = new Dictionary<Cell, int>();
            Dictionary<int, Cell> revmap = new Dictionary<int,Cell>();
            int id = 0;

            senarios.Add(new List<bool>());

            foreach (Cell start in deadend)
            {
                if (!start.visited)
                {
                    q.Enqueue(start);
                    while (q.Count > 0)
                    {
                        Cell cell = q.Dequeue();
                        while (q.Count > 0 && cell.visited)
                            cell = q.Dequeue();

                        if (cell.visited)
                            break;

                        int b = cell.bombs;
                        List<Cell> mapped = new List<Cell>(), free = new List<Cell>();

                        foreach (Cell check in neighbours(cell))
                        {
                            if (check.marked)
                                --b;
                            else if (!check.visible)
                                if (map.ContainsKey(check))
                                    mapped.Add(check);
                                else
                                    free.Add(check);
                            else if (!check.visited)
                                q.Enqueue(check);
                        }

                        foreach (Cell log in free)
                        {
                            revmap.Add(id, log);
                            map.Add(log, id++);
                        }

                        int sencount = senarios.Count;
                        for (int x = 0; x < sencount; ++x)
                        {
                            List<bool> sen = senarios[x];
                            int bombs = b;
                            foreach (Cell cont in mapped)
                                bombs -= sen[map[cont]] ? 1 : 0;

                            if (bombs < 0 || bombs > free.Count)
                            {
                                senarios.Remove(sen);
                                --x;
                                --sencount;
                            }
                            else if (bombs == 0)
                                for (int i = 0; i < free.Count; ++i)
                                    sen.Add(false);
                            else if (bombs == free.Count)
                                for (int i = 0; i < free.Count; ++i)
                                    sen.Add(true);
                            else
                            {
                                int comb = C(free.Count, bombs);

                                List<List<bool>> addto = new List<List<bool>>(comb),
                                                 addend = combine(free.Count, bombs, comb);
                                addto.Add(sen);
                                for (int i = 1; i < comb; ++i)
                                {
                                    List<bool> l = clone(sen);
                                    senarios.Add(l);
                                    addto.Add(l);
                                }

                                for (int i = 0; i < Math.Max(addend.Count, addto.Count); ++i)
                                    addto[i].AddRange(addend[i]);
                            }
                        }

                        cell.visited = true;
                    }
                }
            }

            //foreach (List<bool> l in senarios)
            //{
            //    foreach (bool f in l)
            //        Console.Write(f + " ");
            //    Console.WriteLine();
            //}
            //for (int i = 0; i < revmap.Count; ++i)
            //    Console.WriteLine(i + " -> " + revmap[i].ToString());

            for (int j = 0; j < map.Count; ++j)
            {
                bool certain = true, value = senarios[0][j];
                for (int i = 1; certain && i < senarios.Count; ++i)
                {
                    certain = !value ^ senarios[i][j];
                    value = senarios[i][j];
                }

                if (certain)
                {
                    Cell cert = revmap[j];

                    if (value)
                    {
                        --remaining;
                        cert.marked = true;
                        cert.complete = true;

                        //TextBox textbox = form.GetValue(cert);
                        //textbox.BackColor = Color.Red;
                        //textbox.Text = "B";
                        Application.DoEvents();

                        WindowCell windowCell = new WindowCell(form.minesweeperMemory.Cells[cert.x][cert.y].Name);
                        form.minesweeperMemory.FlagCell(form.minesweeperMemory.Cells[cert.x][cert.y], windowCell.Flagged, windowCell.QuestionMark);
                    }
                    else
                        q.Enqueue(cert);

                    foreach (Cell explore in neighbours(cert))
                        if (explore.visible && !(explore.marked || explore.complete || q.Contains(explore)))
                        {
                            explore.visited = false;
                            q.Enqueue(explore);
                        }
                }
            }
        }

        public static List<bool> clone(List<bool> l)
        {
            List<bool> r = new List<bool>(l.Count);
            foreach (bool f in l)
                r.Add(f);
            return r;
        }

        public static int C(int n, int r)
        {
            if (n < r)
                throw new InvalidOperationException();

            int[] f = {1, 1, 2, 6, 24, 120, 720, 5040, 40320};

            return f[n] / f[n - r] / f[r];

            //int c = 1;
            //for (int i = n; i > n - r; --i)
            //    c *= i;
            //for (int i = r; i > 1; --i)
            //    c /= i;
            //return c;
        }

        //public static List<List<bool>> propMask(int n, int r, int c)
        //{
        //    int[] comb = new int[c];
        //    int idx = 0;
        //    for (int s = (1 << r) - 1; s < (1 << n) - 1; ++s)
        //    {
        //        int x = 0;
        //        for (int i = 0; i < n; ++i)
        //            x += (s & (1 << i)) >> i;
        //        if (x == r)
        //            comb[idx++] = x;
        //    }

        //    List<List<bool>> l = new List<List<bool>>(c);
        //    foreach (int flag in comb)
        //    {
        //        List<bool> sub = new List<bool>(n);
        //        for (int i = 0; i < n; ++i)
        //            sub.Add((flag & (1 << i)) != 0);
        //        l.Add(sub);
        //    }

        //    return l;
        //}

        public static List<List<bool>> combine(int n, int r, int c)
        {
            List<List<bool>> l = new List<List<bool>>(c);
            int[] count = new int[c];

            for (int i = 0; i < c; ++i)
                l.Add(new List<bool>(n));

            help(l, 0, c, n, r);
            return l;
        }

        public static void help(List<List<bool>> l, int s, int e, int n, int r)
        {
            if (e - s == 1)
            {
                for (int i = 0; i < n; ++i)
                    l[s].Add(r > 0);
                return;
            }
            int c = s + C(n - 1, r);
            for (int i = s; i < e; ++i)
                l[i].Add(i >= c);
            help(l, s, c, n - 1, r);
            help(l, c, e, n - 1, r - 1);
        }

        static void Main(string[] args)
        {
            System.Windows.Forms.Application.EnableVisualStyles();
            Form3 form = new Form3();
            System.Windows.Forms.Application.Run(form);
        }

        public List<Cell> neighbours(Cell c)
        {
            Cell[,] g = grid;
            int[] di = { -1, -1, 0, 1, 1, 1, 0, -1 }, dj = { 0, 1, 1, 1, 0, -1, -1, -1 };
            List<Cell> l = new List<Cell>(9);
            for (int n = 0; n < 9; ++n)
                try { l.Add(g[c.x + di[n], c.y + dj[n]]); }
                catch (IndexOutOfRangeException) { }
            return l;
        }
    }

    //public class Cell
    //{
    //    public bool visible, visited, complete, marked;
    //    public int bombs, x, y;

    //    public Cell(int i, int j)
    //    {
    //        x = i;
    //        y = j;
    //    }

    //    public bool Equals(Cell c)
    //    {
    //        return x == c.x && y == c.y;
    //    }

    //    public String ToString()
    //    {
    //        return "(" + x + ", " + y + ")";
    //    }
    //}
}