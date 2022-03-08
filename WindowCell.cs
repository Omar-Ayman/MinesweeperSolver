using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Accessibility;

namespace MineSweeperSolver
{
    public class WindowCell
    {
        public Point Index
        {
            get;
            set;
        }

        public byte Value
        {
            get;
            set;
        }

        public bool Concealed
        {
            get;
            set;
        }

        public bool QuestionMark
        {
            get;
            set;
        }

        public bool Flagged
        {
            get;
            set;
        }

        public void loadFromName(string Name)
        {
            #region Visibility
            if (Name.Contains("(Concealed)."))
                Concealed = true;
            else if (Name.Contains("(Cleared)."))
                Concealed = false;
            else
                throw new Exception("This isn't Minesweeper");
            #endregion

            #region Value
            Value = byte.Parse(Name.Split('.')[1].Trim().Split(' ')[0]);
            #endregion

            //string[] nameSplit = null;
        }

        /// <summary>
        /// Creates a new WindowCell From The Accessible Name
        /// </summary>
        /// <param name="Name"></param>
        public WindowCell(string Name)
        {
            #region Visibility
            if (Name.Contains("(Concealed)."))
                Concealed = true;
            else if (Name.Contains("(Cleared)."))
            {
                Concealed = false;
                #region Value
                try
                {
                    Value = byte.Parse(Name.Split('.')[1].Trim().Split(' ')[0]);
                }
                catch (Exception)
                { Value = 0; }
                #endregion
            }
            else if (Name.Contains("(Concealed with Question Mark)."))
            {
                Concealed = true;
                QuestionMark = true;
            }
            else if (Name.Contains("(Concealed and Flagged)."))
            {
                Concealed = true;
                Flagged = true;
            }
            else
                throw new Exception("This isn't Minesweeper");
            #endregion
        }
    }
}
