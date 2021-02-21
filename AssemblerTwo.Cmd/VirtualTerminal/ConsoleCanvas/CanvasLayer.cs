using System;
using System.Collections;

namespace ConsoleCanvas
{
    public class CanvasLayer //: IEnumerable<CanvasPixel>
    {
        private CanvasPixel[,] mPixels;

        public int Width { get; private set; }
        public int Height { get; private set; }
        
        public CanvasLayer(int width, int height)
        {
            Width = width;
            Height = height;
            mPixels = new CanvasPixel[Height, Width];
        }

        public void Clear()
        {
            mPixels = new CanvasPixel[Height, Width];
        }

        public CanvasPixel GetPixel(int x, int y)
        {
            return mPixels[x, y];
        }

        public void Write(int x, int y, object obj)
        {
            Write(x, y, obj.ToString());
        }

        public void Write(int x, int y, string str)
        {
            for (int i = 0; i < str.Length; i++)
            {
                SetPixel(x, y + i, new CanvasPixel(str[i], true));
            }
        }

        public void SetPixel(int x, int y, CanvasPixel newPixel)
        {
            mPixels[x, y] = newPixel;
        }

        public void SetPixelCharacter(int x, int y, char c)
        {
            mPixels[x, y].Character = c;
        }

        /*#region IEnumerable

        public IEnumerator<CanvasPixel> GetEnumerator()
            => (IEnumerator<CanvasPixel>)mPixels.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        #endregion*/
    }
}
