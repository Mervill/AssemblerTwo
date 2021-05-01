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
    // ─┌┐└┘│

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
            var buildResult = StaticAssembler.Build(sourceText);
            if (buildResult.FinalBytes != null)
            {
                mMemoryBus.CopyInto(buildResult.FinalBytes.Bytes, memoryOffset);
            }
        }

        public void Run()
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.Clear();
            Console.CursorVisible = false;
            
            Canvas.Startup(100, 40);

            var baseLayer = Canvas.GetNewLayer();
            var virtualConsoleBorder = Canvas.GetNewLayer();
            mVirtualConsoleLayer = Canvas.GetNewLayer();
            var memoryViewLayer = Canvas.GetNewLayer();
            //var blinkLayer = Canvas.GetNewLayer();
            //var topLayer = Canvas.GetNewLayer();

            virtualConsoleBorder.Write(0, 0, $"┌{new string('─', mVirtualConsoleWidth)}┐");
            for (int x = 0; x < mVirtualConsoleHeight; x++)
            {
                virtualConsoleBorder.Write(1 + x, 0, "│");
                virtualConsoleBorder.Write(1 + x, mVirtualConsoleWidth + 1, "│");
            }
            virtualConsoleBorder.Write(mVirtualConsoleHeight + 1, 0, $"└{new string('─', mVirtualConsoleWidth)}┘");

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

                /*var rootX = 18;
                var rootY = 1;

                bool IsHalted = mVM.IsHalted;
                bool CanInterrupt = mVM.CanInterrupt;

                blinkLayer.Clear();

                blinkLayer.Write(rootX + 0, rootY + 0, " W H I          ");
                blinkLayer.Write(rootX + 1, rootY + 0, "◌○◌○◌○◌◌◌◌◌◌◌◌◌◌");

                blinkLayer.Write(rootX + 1, rootY + 1, mDoVMSteps == 0 ? '●' : '○');
                blinkLayer.Write(rootX + 1, rootY + 3, IsHalted ? '●' : '○');
                blinkLayer.Write(rootX + 1, rootY + 5, CanInterrupt ? '●' : '○');

                
                blinkLayer.Write(rootX + 0, rootY + 17, "┌───── PC ─────┐");
                blinkLayer.Write(rootX + 1, rootY + 17, GetDots((ushort)mVM.ProgramCounter));

                blinkLayer.Write(rootX + 2, rootY + 0, GetDots((ushort)mVM.StackPointer));
                blinkLayer.Write(rootX + 3, rootY + 0, "└ STACK ───────┘");

                blinkLayer.Write(rootX + 2, rootY + 17, GetDots(mMemoryBus.Read16(mVM.ProgramCounter)));
                blinkLayer.Write(rootX + 3, rootY + 17, "└──── DATA ────┘");

                blinkLayer.Write(rootX + 5, rootY + 0, "F      87      0");
                blinkLayer.Write(rootX + 6, rootY + 0, GetSquares(mToggleWord));
                blinkLayer.Write(rootX + 7, rootY + controlFocusIndex, "▲");

                blinkLayer.Write(rootX + 9, rootY + 0, "F1 - start/stop");
                blinkLayer.Write(rootX + 10, rootY + 0, "F2 - single step");
                blinkLayer.Write(rootX + 11, rootY + 0, Canvas.LastUpdatedPixels);*/

                memoryViewLayer.Clear();

                var rootX = 18;
                var rootY = 1;

                var vmCounter = (ushort)mVM.ProgramCounter;
                memoryViewLayer.Write(rootX + 0, rootY + 0, $"PC:{vmCounter:X4}");
                memoryViewLayer.Write(rootX + 0, rootY + 32, "│");
                memoryViewLayer.Write(rootX + 1, rootY + 0, $"A: {mVM.Registers[0]:X4} B: {mVM.Registers[1]:X4} C: {mVM.Registers[2]:X4} D: {mVM.Registers[3]:X4} │");
                memoryViewLayer.Write(rootX + 2, rootY + 0, $"E: {mVM.Registers[4]:X4} F: {mVM.Registers[5]:X4} G: {mVM.Registers[6]:X4} H: {mVM.Registers[7]:X4} │");
                memoryViewLayer.Write(rootX + 3, rootY + 0, $"I: {mVM.Registers[8]:X4} J: {mVM.Registers[9]:X4} K: {mVM.Registers[10]:X4} L: {mVM.Registers[11]:X4} │");
                memoryViewLayer.Write(rootX + 4, rootY + 0, $"M: {mVM.Registers[12]:X4} N: {mVM.Registers[13]:X4} P: {mVM.Registers[14]:X4} S: {mVM.Registers[15]:X4} │");

                ushort memViewCounter = vmCounter;
                ushort memViewWord = 0;
                /*if (memViewCounter == 0)
                {

                }
                else
                {
                    memViewCounter = (ushort)(vmCounter - 1);
                    memViewWord = mMemoryBus.Read16(memViewCounter);
                    memViewCounter += WriteMemoryLine(rootX + 1, rootY + 0, memoryViewLayer, memViewCounter, memViewWord);
                }*/

                memViewCounter += WriteMemoryLine(rootX + 0, rootY + 34, memoryViewLayer, memViewCounter);
                memViewCounter += WriteMemoryLine(rootX + 1, rootY + 34, memoryViewLayer, memViewCounter);


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

        private ushort WriteMemoryLine(int x, int y, CanvasLayer memoryViewLayer, ushort address)
        {
            var currentWord = mMemoryBus.Read16(address);

            RegisterName? registerA;
            RegisterName? registerB;
            var opcodeDef = OpcodeDefinition.Decode(currentWord, out registerA, out registerB);

            if (opcodeDef != null)
            {
                var immediateWord = new string(' ', 4);
                var argumentSection = string.Empty;
                
                switch (opcodeDef.ArgumentType)
                {
                    case OpcodeArgumentType.NONE:
                    {
                        break;
                    }
                    case OpcodeArgumentType.REG:
                    {
                        argumentSection = $" {registerB}";
                        break;
                    }
                    case OpcodeArgumentType.IMMED:
                    {
                        argumentSection = $" {mMemoryBus.Read16(address + 2):X4}";
                        break;
                    }
                    case OpcodeArgumentType.REG_REG:
                    {
                        argumentSection = $" {registerA}, {registerB}";
                        break;
                    }
                    case OpcodeArgumentType.REG_IMMED:
                    {
                        immediateWord = mMemoryBus.Read16(address + 2).ToString("X4");
                        argumentSection = $" {registerB}, {immediateWord}";
                        break;
                    }
                    case OpcodeArgumentType.REG_REG_IMMED:
                    {
                        immediateWord = mMemoryBus.Read16(address + 2).ToString("X4");
                        argumentSection = $" {registerA}, {registerB}, {immediateWord}";
                        break;
                    }
                }
                
                memoryViewLayer.Write(x, y, $"{address:X4}: {currentWord:X4} {immediateWord} | {opcodeDef.Name,-5}{argumentSection}");

                return (ushort)opcodeDef.ByteLength;
            }
            else
            {
                return 4;
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
