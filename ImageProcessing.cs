using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace MineSweeperSolver
{
    public class ImageProcessing
    {
        public static Bitmap FindVerticalEdges(Bitmap Input)
        {
            Bitmap Output = new Bitmap(Input);

            BitmapData bitmapData1 = Input.LockBits(new Rectangle(0, 0, Input.Width, Input.Height), System.Drawing.Imaging.ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            BitmapData bitmapData2 = Output.LockBits(new Rectangle(0, 0, Input.Width, Input.Height), System.Drawing.Imaging.ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            for (int i = 0; i < Input.Width * Input.Height * 3; i++)
            {
                byte R1 = 0, G1 = 0, B1 = 0;
                byte R2 = 0, G2 = 0, B2 = 0;
                try
                {
                    R1 = Marshal.ReadByte(bitmapData1.Scan0 + i);
                    G1 = Marshal.ReadByte(bitmapData1.Scan0 + i + 1);
                    B1 = Marshal.ReadByte(bitmapData1.Scan0 + i + 2);

                    R2 = Marshal.ReadByte(bitmapData1.Scan0 + i - 3);
                    G2 = Marshal.ReadByte(bitmapData1.Scan0 + i - 2);
                    B2 = Marshal.ReadByte(bitmapData1.Scan0 + i - 1);
                }
                catch (Exception)
                {
                    try
                    {
                        R2 = Marshal.ReadByte(bitmapData1.Scan0 + i);
                        G2 = Marshal.ReadByte(bitmapData1.Scan0 + i + 1);
                        B2 = Marshal.ReadByte(bitmapData1.Scan0 + i + 2);
                    }
                    catch (Exception) { }
                }
                byte Val = (byte)(Math.Abs((short)R2 - R1));
                Marshal.WriteByte(bitmapData2.Scan0 + i, Val);
                Marshal.WriteByte(bitmapData2.Scan0 + i + 1, Val);
                Marshal.WriteByte(bitmapData2.Scan0 + i + 2, Val);

                i += 2;
            }
            Input.UnlockBits(bitmapData1);

            Output.UnlockBits(bitmapData2);
            return Output;
        }

        public static Bitmap FindHorizontalEdges(Bitmap Input)
        {
            Bitmap Temp = new Bitmap(Input);
            Temp.RotateFlip(RotateFlipType.Rotate90FlipX);

            Bitmap Output = FindVerticalEdges(Temp);
            Output.RotateFlip(RotateFlipType.Rotate90FlipX);
            Temp.Dispose();
            GC.Collect();
            return Output;
        }
    }
}
