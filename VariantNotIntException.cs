using System;

namespace MineSweeperSolver
{
    public class VariantNotIntException : Exception
    {
        private string _variantType;

        public string VariantType
        {
            get
            {
                return this._variantType;
            }
        }

        public VariantNotIntException(object variant)
        {
            this._variantType = ((variant == null) ? "[NULL]" : variant.GetType().ToString());
        }
    }
}
