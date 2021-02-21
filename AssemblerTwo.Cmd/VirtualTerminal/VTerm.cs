using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;

using AssemblerTwo.Lib;
using AssemblerTwo.Machine;

using ConsoleCanvas;

namespace AssemblerTwo.Cmd.VirtualTerminal
{
    // ● ○ ◌
    // ─┌┐└┘

    public class VTerm : IInputOutputBus
    {
        private bool mRunTerm;

        DefaultMemoryBus mMemoryBus;
        VirtualMachine mVM;

        private int mDoVMSteps;

        char[,] mVirtualConsoleGrid;
        CanvasLayer mVirtualConsoleLayer;
        int mVirtualConsoleWidth = 80;
        int mVirtualConsoleHeight = 16;
        int mVirtualCursorTop = 0;
        int mVirtualCursorLeft = 0;

        int controlFocusIndex;
        int controlFocusIndexMax = 16;

        ushort mToggleWord = 0;

        public VTerm()
        {
            mMemoryBus = new DefaultMemoryBus();
            mVM = new VirtualMachine(mMemoryBus, this);

            mVirtualConsoleGrid = new char[mVirtualConsoleHeight, mVirtualConsoleWidth];
        }

        public void LoadBinary(string filename, int memoryOffset)
        {
            var bytes = File.ReadAllBytes(filename);
            mMemoryBus.CopyInto(bytes, memoryOffset);
        }

        public void LoadSource(string filename, int memoryOffset)
        {
            var sourceText = File.ReadAllText(filename);
            var bytes = Assembler.Build(sourceText);
            mMemoryBus.CopyInto(bytes, memoryOffset);
        }

        public void Run()
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.Clear();
            Console.CursorVisible = false;
            
            Canvas.Startup(100, 40);

            var baseLayer = Canvas.GetNewLayer();
            mVirtualConsoleLayer = Canvas.GetNewLayer();
            //var topLayer = Canvas.GetNewLayer();

            /*var logicThread = new Thread(() => {
                while (true)
                {
                    if (mDoVMSteps != 0)
                    {
                        mVM.StepInstruction();
                        if (mDoVMSteps > 0)
                            mDoVMSteps--;
                    }
                    Thread.Sleep(1);
                }
            });
            logicThread.Start();*/

            ConsoleKeyInfo keyInfo;

            mRunTerm = true;
            mDoVMSteps = 0;
            while (mRunTerm)
            {
                if (Console.KeyAvailable)
                {
                    keyInfo = Console.ReadKey(true);

                    if (keyInfo.Key == ConsoleKey.F1)
                    {
                        mDoVMSteps = (mDoVMSteps != 0) ? 0 : -1;
                    }
                    if (keyInfo.Key == ConsoleKey.F2)
                    {
                        mDoVMSteps = 1;
                    }
                    if (keyInfo.Key == ConsoleKey.F3)
                    {
                        mDoVMSteps = 100;
                    }

                    if (keyInfo.Key == ConsoleKey.RightArrow)
                    {
                        controlFocusIndex++;
                        if (controlFocusIndex >= controlFocusIndexMax)
                            controlFocusIndex = controlFocusIndexMax - 1;
                    }

                    if (keyInfo.Key == ConsoleKey.LeftArrow)
                    {
                        controlFocusIndex--;
                        if (controlFocusIndex <= -1)
                            controlFocusIndex = 0;
                    }

                    if (keyInfo.Key == ConsoleKey.UpArrow || keyInfo.Key == ConsoleKey.DownArrow)
                    {
                        var index = 15 - controlFocusIndex;
                        mToggleWord = (ushort)(mToggleWord ^ 1 << index);
                    }
                }

                if (mDoVMSteps != 0)
                {
                    mVM.StepInstruction();
                    if (mDoVMSteps > 0)
                        mDoVMSteps--;
                }
                
                mVirtualConsoleLayer.Clear();
                for (int x = 0; x < mVirtualConsoleHeight; x++)
                {
                    for (int y = 0; y < mVirtualConsoleWidth; y++)
                    {
                        mVirtualConsoleLayer.SetPixel(x + 1, y + 1, new CanvasPixel(mVirtualConsoleGrid[x, y], true));
                    }
                }

                //baseLayer.Write(16, 0, Convert.ToString(programCounter, 2).PadLeft(16, '0'));

                var rootX = 18;
                var rootY = 1;

                bool IsHalted = mVM.IsHalted;
                bool CanInterrupt = mVM.CanInterrupt;

                baseLayer.Clear();

                baseLayer.Write(rootX + 0, rootY + 0, " W H I          ");
                baseLayer.Write(rootX + 1, rootY + 0, "◌○◌○◌○◌◌◌◌◌◌◌◌◌◌");

                baseLayer.Write(rootX + 1, rootY + 1, mDoVMSteps == 0 ? '●' : '○');
                baseLayer.Write(rootX + 1, rootY + 3, IsHalted ? '●' : '○');
                baseLayer.Write(rootX + 1, rootY + 5, CanInterrupt ? '●' : '○');

                
                baseLayer.Write(rootX + 0, rootY + 17, "┌───── PC ─────┐");
                baseLayer.Write(rootX + 1, rootY + 17, GetDots((ushort)mVM.ProgramCounter));

                baseLayer.Write(rootX + 2, rootY + 0, GetDots((ushort)mVM.StackPointer));
                baseLayer.Write(rootX + 3, rootY + 0, "└ STACK ───────┘");

                baseLayer.Write(rootX + 2, rootY + 17, GetDots(mMemoryBus.Read16(mVM.ProgramCounter)));
                baseLayer.Write(rootX + 3, rootY + 17, "└──── DATA ────┘");

                baseLayer.Write(rootX + 5, rootY + 0, "F      87      0");
                baseLayer.Write(rootX + 6, rootY + 0, GetSquares(mToggleWord));
                baseLayer.Write(rootX + 7, rootY + controlFocusIndex, "▲");

                baseLayer.Write(rootX + 9, rootY + 0, "F1 - start/stop");
                baseLayer.Write(rootX + 10, rootY + 0, "F2 - single step");
                baseLayer.Write(rootX + 11, rootY + 0, Canvas.LastUpdatedPixels);

                //Canvas.InvalidateRenderCache();
                Canvas.Render();
                //Thread.Sleep(1);

                //Console.Title = Canvas.LastUpdatedPixels.ToString();
            }
            Canvas.Shutdown();
        }

        private void VirtualConsoleWrite(char character)
        {
            if (character != '\n')
            {
                mVirtualConsoleGrid[mVirtualCursorTop, mVirtualCursorLeft] = character;
                mVirtualCursorLeft++;
                if (mVirtualCursorLeft == mVirtualConsoleWidth)
                {
                    mVirtualCursorLeft = 0;
                    mVirtualCursorTop++;
                    if (mVirtualCursorTop == mVirtualConsoleHeight)
                    {
                        mVirtualCursorTop = mVirtualConsoleHeight - 1;
                        VirtualConsoleShiftUp();
                    }
                }
            }
            else
            {
                mVirtualCursorLeft = 0;
                mVirtualCursorTop++;
                if (mVirtualCursorTop == mVirtualConsoleHeight)
                {
                    mVirtualCursorTop = mVirtualConsoleHeight - 1;
                    VirtualConsoleShiftUp();
                }
            }
        }

        private void VirtualConsoleShiftUp()
        {
            for (int x = 1; x < mVirtualConsoleHeight; x++)
            {
                for (int y = 0; y < mVirtualConsoleWidth; y++)
                {
                    mVirtualConsoleGrid[x - 1, y] = mVirtualConsoleGrid[x, y];
                }
            }

            for (int i = 0; i < mVirtualConsoleWidth; i++)
            {
                mVirtualConsoleGrid[mVirtualConsoleHeight - 1, i] = '\0';
            }
        }

        private string GetDots(ushort int16)
        {
            return Convert.ToString(int16, 2)
                .PadLeft(16, '0')
                .Replace('0', '○')
                .Replace('1', '●');
        }

        private string GetSquares(ushort int16)
        {
            return Convert.ToString(int16, 2)
                .PadLeft(16, '0')
                .Replace('0', '□')
                .Replace('1', '■');
        }

        public int MachineReadInput(int portNumber)
        {
            throw new NotImplementedException();
        }

        public void MachineWriteOutput(int portNumber, int value)
        {
            switch (portNumber)
            {
                case 0xA:
                {
                    VirtualConsoleWrite(Convert.ToChar(value & 0xFF));
                    return;
                }
            }
        }
    }
}
