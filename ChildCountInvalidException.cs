using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MineSweeperSolver
{
    public class ChildCountInvalidException : Exception
    {
        private int _childCount;

        public int ChildCount
        {
            get
            {
                return this._childCount;
            }
        }

        public ChildCountInvalidException(int childCount)
        {
            this._childCount = childCount;
        }
    }
}
