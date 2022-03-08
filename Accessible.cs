using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Accessibility;
using System.Drawing;
using System.Runtime.InteropServices;

namespace MineSweeperSolver
{
    public class Accessible
    {
        private const int S_OK = 0;

        private IAccessible _accessible;

        private int _childId;

        public IAccessible IAccessible
        {
            get
            {
                return this._accessible;
            }
        }

        public int ChildId
        {
            get
            {
                return this._childId;
            }
        }

        /// <summary>
        /// Gets the Parent of the current accessible
        /// </summary>
        public Accessible Parent
        {
            get
            {
                object parent = null;
                if (this._childId == 0)
                {
                    IAccessible accessible = this._accessible.accParent as IAccessible;
                    parent = ((accessible != null) ? new Accessible(accessible, 0) : null);
                }
                else
                {
                    parent = new Accessible(this._accessible, 0);
                }

                return parent as Accessible;
            }
        }

        public string Name
        {
            get
            {
                return this._accessible.get_accName((object)this._childId);
            }
        }

        public int Role
        {
            get
            {
                object obj = this._accessible.get_accRole((object)this._childId);
                if (!(obj is int))
                {
                    throw new VariantNotIntException(obj);
                }
                return (int)obj;
            }
        }

        public int State
        {
            get
            {
                object obj = this._accessible.get_accState((object)this._childId);
                if (!(obj is int))
                {
                    throw new VariantNotIntException(obj);
                }
                return (int)obj;
            }
        }

        public string Value
        {
            get
            {
                string text = this._accessible.get_accValue((object)this._childId);
                if (text == null)
                {
                    text = "";
                }
                return text;
            }
        }

        public Rectangle Location
        {
            get
            {
                int x = 0;
                int y = 0;
                int width = 0;
                int height = 0;
                this._accessible.accLocation(out x, out y, out width, out height, (object)this._childId);
                return new Rectangle(x, y, width, height);
            }
        }

        public string KeyboardShortcut
        {
            get
            {
                return this._accessible.get_accKeyboardShortcut((object)this._childId);
            }
        }

        public IntPtr Hwnd
        {
            get
            {
                IntPtr zero = IntPtr.Zero;
                Win32API.WindowFromAccessibleObject(this._accessible, ref zero);
                return zero;
            }
        }

        public Accessible(IAccessible accessible, int childId)
        {
            this._accessible = accessible;
            this._childId = childId;
        }

        public Accessible()
        { 
        
        }

        public static int FromPoint(Point point, out Accessible accessible)
        {
            accessible = null;
            IAccessible AccessibleInstance = null;
            object obj = null;
            int AccessibleObjectsCounts = Win32API.AccessibleObjectFromPoint(new Win32API.POINT(point.X, point.Y), ref AccessibleInstance, ref obj);

            if (AccessibleObjectsCounts == 0 && AccessibleInstance != null)
            {
                if (!(obj is int))
                    throw new VariantNotIntException(obj);

                accessible = new Accessible(AccessibleInstance, (int)obj);
            }
            return AccessibleObjectsCounts;
        }

        /// <summary>
        /// creates a new Accessible using the a given window handle
        /// </summary>
        /// <param name="hwnd"></param>
        /// <returns></returns>
        public static Accessible FromWindow(IntPtr hwnd)
        {
            Accessible accessible = null;
            FromWindow(hwnd, out accessible);
            return accessible;
        }

        /// <summary>
        /// creates an accessible Instance using the a given window handle
        /// </summary>
        /// <param name="accessible">an Accessible Variable to which the result will be returned</param>
        /// <returns></returns>
        public static int FromWindow(IntPtr hwnd, out Accessible accessible)
        {
            accessible = null;
            Guid iID_IAccessible = Win32API.IID_IAccessible;
            object obj = null;
            int result = Win32API.AccessibleObjectFromWindow(hwnd, 0u, ref iID_IAccessible, ref obj);
            IAccessible accessible2 = obj as IAccessible;
            if (accessible2 != null)
            {
                accessible = new Accessible(accessible2, 0);
            }
            return result;
        }

        public static int FromEvent(IntPtr hwnd, int idObject, int idChild, out Accessible accessible)
        {
            accessible = null;
            object obj = null;
            IAccessible accessible2 = null;
            int result = Win32API.AccessibleObjectFromEvent(hwnd, idObject, idChild, ref accessible2, ref obj);
            if (accessible2 != null)
            {
                accessible = new Accessible(accessible2, (int)obj);
            }
            return result;
        }

        public Accessible[] Children()
        {
            Accessible[] Children = null;
            this.Children(out Children);
            return Children;
        }

        /// <summary>
        /// Returns An Accessible Array that represents the all the child parts of the main window
        /// </summary>
        /// <param name="accessible">an Accessible Array to which the result will be returned</param>
        /// <returns></returns>
        public int Children(out Accessible[] accessible)
        {
            accessible = null;

            // return Empty Array if no children are found
            if (this._childId != 0)
            {
                accessible = new Accessible[0];
                return 0;
            }

            int accChildCount = this._accessible.accChildCount;
            // Throw exception if childrenCount is less than zero
            if (accChildCount < 0)
                throw new ChildCountInvalidException(accChildCount);

            object[] array = new object[accChildCount];
            int num = Win32API.AccessibleChildren(this._accessible, 0, accChildCount, array, out accChildCount);
            if (num == 0)
            {
                accessible = new Accessible[accChildCount];
                int num2 = 0;
                object[] array2 = array;
                foreach (object obj in array2)
                {
                    if (obj != null)
                    {
                        if (obj is IAccessible)
                        {
                            accessible[num2++] = new Accessible((IAccessible)obj, 0);
                        }
                        else if (obj is int)
                        {
                            accessible[num2++] = new Accessible(this._accessible, (int)obj);
                        }
                    }
                }

                if (accChildCount != num2)
                {
                    Accessible[] array3 = new Accessible[num2];
                    Array.Copy(accessible, array3, num2);
                    accessible = array3;
                }
            }
            return num;
        }

        public void Select(int flag)
        {
            this._accessible.accSelect(flag, this._childId);
        }

        public bool Compare(Accessible acc)
        {
            if (acc == null)
            {
                return false;
            }
            Rectangle rectangle;
            try
            {
                rectangle = acc.Location;
            }
            catch (Exception e)
            {
                if (Accessible.IsExpectedException(e))
                {
                    rectangle = Rectangle.Empty;
                    goto end_IL_000e;
                }
                throw;
            end_IL_000e: ;
            }
            Rectangle rectangle2;
            try
            {
                rectangle2 = this.Location;
            }
            catch (Exception e2)
            {
                if (Accessible.IsExpectedException(e2))
                {
                    rectangle2 = Rectangle.Empty;
                    goto end_IL_002c;
                }
                throw;
            end_IL_002c: ;
            }
            if (!rectangle2.Equals(rectangle))
            {
                return false;
            }
            string text = "";
            try
            {
                text = acc.Name;
            }
            catch (Exception e3)
            {
                if (Accessible.IsExpectedException(e3))
                {
                    text = "";
                    goto end_IL_0069;
                }
                throw;
            end_IL_0069: ;
            }
            string text2 = "";
            try
            {
                text2 = this.Name;
            }
            catch (Exception e4)
            {
                if (Accessible.IsExpectedException(e4))
                {
                    text2 = "";
                    goto end_IL_0092;
                }
                throw;
            end_IL_0092: ;
            }
            if (text2 != text)
            {
                return false;
            }
            int num = 0;
            try
            {
                num = acc.Role;
            }
            catch (Exception e5)
            {
                if (Accessible.IsExpectedException(e5))
                {
                    num = 0;
                    goto end_IL_00c4;
                }
                throw;
            end_IL_00c4: ;
            }
            int num2 = 0;
            try
            {
                num2 = this.Role;
            }
            catch (Exception e6)
            {
                if (Accessible.IsExpectedException(e6))
                {
                    num2 = 0;
                    goto end_IL_00e5;
                }
                throw;
            end_IL_00e5: ;
            }
            if (num2 != num)
            {
                return false;
            }
            string text3 = "";
            try
            {
                text3 = acc.Value;
            }
            catch (Exception e7)
            {
                if (Accessible.IsExpectedException(e7))
                {
                    text3 = "";
                    goto end_IL_0112;
                }
                throw;
            end_IL_0112: ;
            }
            string text4 = "";
            try
            {
                text4 = this.Value;
            }
            catch (Exception e8)
            {
                if (Accessible.IsExpectedException(e8))
                {
                    text4 = "";
                    goto end_IL_013b;
                }
                throw;
            end_IL_013b: ;
            }
            if (text4 != text3)
            {
                return false;
            }
            return true;
        }

        public string CreateParentChain()
        {
            List<string> list = new List<string>();
            Accessible accessible = this.Parent;
            int num = 0;
            while (accessible != null)
            {
                num++;
                string text = "";
                try
                {
                    text = this.Name;
                }
                catch (Exception e)
                {
                    if (Accessible.IsExpectedException(e))
                    {
                        text = "";
                        goto end_IL_0024;
                    }
                    throw;
                end_IL_0024: ;
                }
                if (!string.IsNullOrEmpty(text))
                {
                    list.Add(text);
                }
                try
                {
                    accessible = this.Parent;
                }
                catch (Exception e2)
                {
                    if (Accessible.IsExpectedException(e2))
                    {
                        accessible = null;
                        goto end_IL_0053;
                    }
                    throw;
                end_IL_0053: ;
                }
                if (num > 100)
                {
                    break;
                }
            }
            list.Reverse();
            return string.Join(".", list.ToArray());
        }

        public override string ToString()
        {
            string text = "";
            try
            {
                text = this.Name;
                if (text == null)
                {
                    text = "";
                }
            }
            catch (Exception e)
            {
                if (Accessible.IsExpectedException(e))
                {
                    text = "";
                    goto end_IL_0018;
                }
                throw;
            end_IL_0018: ;
            }
            int num = 0;
            try
            {
                num = this.Role;
            }
            catch (Exception e2)
            {
                if (Accessible.IsExpectedException(e2))
                {
                    num = 0;
                    goto end_IL_0038;
                }
                throw;
            end_IL_0038: ;
            }
            StringBuilder stringBuilder = new StringBuilder(255);
            Win32API.GetRoleText(num, stringBuilder, (uint)stringBuilder.Capacity);
            return "[IAccessible.ChildId(" + this._childId + ").Role(" + stringBuilder + ").Name(" + text + ")]";
        }

        public static bool IsExpectedException(Exception e)
        {
            if (!(e is NotImplementedException) && !(e is COMException) && !(e is VariantNotIntException) && !(e is ArgumentException) && !(e is InvalidCastException) && !(e is UnauthorizedAccessException))
            {
                return false;
            }
            return true;
        }
    }
}
