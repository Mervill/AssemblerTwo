using System;
using System.Collections.Generic;

namespace ConsoleCanvas
{
    public static class Canvas
    {
        private static ConsoleWindowSettings mDefaultConsoleWindowSettings;

        private static List<CanvasLayer> mLayers = new List<CanvasLayer>();
        private static List<(bool Visible, String Name)> mLayerInfo = new List<(bool Visible, string Name)>();

        private static CanvasPixel[,] mRenderCache;

        private static int mWidth;
        /// <summary>
        /// The width of the canvas.
        /// </summary>
        public static int Width => mWidth;

        private static int mHeight;
        /// <summary>
        /// The height of the canvas.
        /// </summary>
        public static int Height => mHeight;

        public static int LastUpdatedPixels { get; private set; }

        static Canvas()
        {
            mDefaultConsoleWindowSettings = SaveCurrentConsoleWindowSettings();
        }

        public static void Startup(int width, int height)
        {
            width = Math.Clamp(width, 1, Console.LargestWindowWidth - 1);
            height = Math.Clamp(height, 1, Console.LargestWindowHeight - 1);

            mWidth = width;
            mHeight = height;

            //ResizeWindow();
            InvalidateRenderCache();
        }

        public static void Shutdown()
        {
            ApplyConsoleWindowSettings(mDefaultConsoleWindowSettings);
            Console.ResetColor();
        }

        public static void ResizeWindow()
        {
            Console.SetWindowSize(mWidth, mHeight + 1);
            Console.SetBufferSize(mWidth, mHeight + 1);
        }

        public static void Render()
        {
            Console.CursorVisible = false;
            //ResizeWindow();
            var newPixels = GetNewPixels();
            foreach(var kvp in newPixels)
            {
                Console.SetCursorPosition(kvp.Key.YPos, kvp.Key.XPos);
                var character = kvp.Value.Character == '\0' ? ' ' : kvp.Value.Character;
                Console.Write(character);
            }
            LastUpdatedPixels = newPixels.Count;
        }

        private static Dictionary<(int XPos, int YPos), CanvasPixel> GetNewPixels()
        {
            var newPixels = new Dictionary<(int XPos, int YPos), CanvasPixel>();
            CanvasPixel[,] newRenderCache = new CanvasPixel[Height, Width];
            for (int layerIndex = 0; layerIndex < mLayers.Count; layerIndex++)
            {
                var isVisible = mLayerInfo[layerIndex].Visible;
                if (!isVisible)
                    continue;

                var layer = mLayers[layerIndex];

                for (int layerX = 0; layerX < Height; layerX++)
                {
                    for (int layerY = 0; layerY < Width; layerY++)
                    {
                        var p = layer.GetPixel(layerX, layerY);
                        if (p.Visible)
                            newRenderCache[layerX, layerY] = p;
                    }
                }
            }

            for (int layerX = 0; layerX < Height; layerX++)
            {
                for (int layerY = 0; layerY < Width; layerY++)
                {
                    var newPixel = newRenderCache[layerX, layerY];
                    var cachePizel = mRenderCache[layerX, layerY];
                    if (newPixel != cachePizel)
                    {
                        newPixels.Add((layerX, layerY), newPixel);
                    }
                }
            }
            mRenderCache = newRenderCache;
            return newPixels;
        }

        public static void InvalidateRenderCache()
        {
            mRenderCache = new CanvasPixel[Height, Width];
        }

        public static int AddLayer(CanvasLayer newLayer, string layerName = "")
        {
            if (newLayer.Height > mHeight || newLayer.Width > mWidth)
                throw new IndexOutOfRangeException();

            mLayers.Add(newLayer);
            mLayerInfo.Add((true, layerName));

            return mLayers.Count;
        }

        public static CanvasLayer GetNewLayer(string layerName = "")
        {
            var newLayer = new CanvasLayer(mWidth, mHeight);
            AddLayer(newLayer);
            return newLayer;
        }

        private static ConsoleWindowSettings SaveCurrentConsoleWindowSettings()
        {
           return new ConsoleWindowSettings(
                Console.WindowLeft,
                Console.WindowTop,
                Console.WindowWidth, 
                Console.WindowHeight, 
                Console.BufferWidth, 
                Console.BufferHeight);
        }

        private static void ApplyConsoleWindowSettings(ConsoleWindowSettings settings)
        {
            Console.WindowLeft = settings.WindowLeft;
            Console.WindowTop = settings.WindowTop;
            Console.WindowWidth = settings.WindowWidth;
            Console.WindowHeight = settings.WindowHeight;
            Console.BufferWidth = settings.BufferWidth;
            Console.BufferHeight = settings.BufferHeight;
        }

        private struct ConsoleWindowSettings
        {
            public readonly int WindowLeft;   //  50
            public readonly int WindowTop;    //  50
            public readonly int WindowWidth;  //  80
            public readonly int WindowHeight; //  25
            public readonly int BufferWidth;  //  80
            public readonly int BufferHeight; // 300
            
            public ConsoleWindowSettings(int windowLeft, int windowTop, int windowWidth, int windowHeight, int bufferWidth, int bufferHeight)
            {
                WindowLeft = windowLeft;
                WindowTop = windowTop;
                WindowWidth = windowWidth;
                WindowHeight = windowHeight;
                BufferWidth = bufferWidth;
                BufferHeight = bufferHeight;
            }
        }
    }
}
