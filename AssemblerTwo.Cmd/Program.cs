using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text;

using CommandLine;

using AssemblerTwo.Cmd.VirtualTerminal;
using AssemblerTwo.Lib;
using AssemblerTwo.Machine;
using AssemblerTwo.Machine.IOBusBridge;

using Parser = AssemblerTwo.Lib.Parser;
using CmdParser = CommandLine.Parser;

namespace AssemblerTwo.Cmd
{
    class Program
    {
        class Options
        {
            public const string kDefaultFilename = "out.a";
            public const string kSymbolTableFilename = "out.st";

            [Option('m', "mode", HelpText = "Assembler mode")]
            public string Mode { get; set; }

            [Option('f', "file", Required = true, HelpText = "File to assemble")]
            public string Filename { get; set; }

            [Option('o', "output", HelpText = "output file")]
            public string Output { get; set; } = kDefaultFilename;

            [Option("st", HelpText = "symbol table file")]
            public string SymbolTable { get; set; }
        }

        static void Main(string[] args)
        {
            Console.WriteLine("AssemblerTwo.Cmd");
            CommandlineMain(args);
        }

        static void CommandlineMain(string[] args)
        {
            CmdParser.Default.ParseArguments<Options>(args).WithParsed(o => {
                switch (o.Mode)
                {
#if DEBUG
                    case "tests-bulk-tokenize":
                    {
                        Console.WriteLine("tests_bulk_tokenize");
                        // here we cheat a bit a use filename as the folder instead
                        if (!Directory.Exists(o.Filename))
                        {
                            Console.WriteLine("despite evidence to the contrary, filename needs to be a directory");
                            return;
                        }
                        int files = 0;
                        foreach (string filePath in Directory.GetFiles(o.Filename, "*.input.txt"))
                        {
                            var sourceText = File.ReadAllText(filePath);
                            var stringTokens = StaticAssembler.StringTokenize(sourceText);
                            var dumpText = DumpUtility.DumpStringTokens(stringTokens, showIndices: false);
                            var outputPath = filePath.Replace(".input.txt", ".output.txt");
                            File.WriteAllText(outputPath, dumpText);
                            files++;
                        }
                        Console.WriteLine($"Processed {files} files");
                        return;
                    }
                    case "debug-dump-opcode-defs":
                    {
                        var allDefs = OpcodeDefinition.GetDefinitionEnumerator();
                        while(allDefs.MoveNext())
                        {
                            var opcodeDef = (OpcodeDefinition)allDefs.Current;
                            Console.WriteLine(opcodeDef.Name);
                        }
                        return;
                    }
#endif
                    case "dump-tokenize":
                    {
                        var sourceText = File.ReadAllText(o.Filename);
                        Console.WriteLine($"File: {o.Filename} ({Encoding.UTF8.GetByteCount(sourceText)} bytes)");
                        var stringTokens = StaticAssembler.StringTokenize(sourceText);
                        var dumpText = DumpUtility.DumpStringTokens(stringTokens, hideEoLCharacters: false);
                        Console.Write(dumpText);
                        string outFilename = "./dump-tokenize.output.txt";
                        Console.WriteLine($"Writing {outFilename} ({Encoding.UTF8.GetByteCount(sourceText)} bytes)");
                        File.WriteAllText(outFilename, dumpText);
                        return;
                    }
                    case "dump-ast":
                    {
                        var sourceText = File.ReadAllText(o.Filename);
                        Console.WriteLine($"File: {o.Filename} ({Encoding.UTF8.GetByteCount(sourceText)} bytes)");
                        var stringTokens = StaticAssembler.StringTokenize(sourceText);
                        var lexTokens = StaticAssembler.Lex(stringTokens);
                        var parseResult = Parser.Parse(lexTokens);
                        var dumpText = DumpUtility.DumpASTNodes(parseResult.SyntaxTree, true, true);
                        Console.Write(dumpText);
                        string outFilename = "./dump-ast.output.txt";
                        Console.WriteLine($"Writing {outFilename} ({Encoding.UTF8.GetByteCount(dumpText)} bytes)");
                        File.WriteAllText(outFilename, dumpText);
                        return;
                    }
                    case "dump-st":
                    {
                        var symbolTable = SymbolTable.FromFile(o.Filename);
                        Console.WriteLine($"File: {o.Filename}");
                        var dumpText = symbolTable.GetDump();
                        Console.Write(dumpText);
                        string outFilename = "./dump-st.output.txt";
                        Console.WriteLine($"Writing {outFilename} ({Encoding.UTF8.GetByteCount(dumpText)} bytes)");
                        File.WriteAllText(outFilename, dumpText);
                        return;
                    }
                    case "build":
                    {
                        var sourceText = File.ReadAllText(o.Filename);
                        Console.WriteLine($"File: {o.Filename}");

                        var buildResult = StaticAssembler.Build(sourceText);
                        var bytes = buildResult.FinalBytes?.Bytes;
                        if (bytes != null)
                        {
                            var symbolTableFilename = $"{o.Output}.st";
                            if (!string.IsNullOrEmpty(o.SymbolTable))
                            {
                                symbolTableFilename = o.SymbolTable;
                            }

                            var symbolTableBytes = buildResult.FinalBytes.SymbolTable.GetBytes();
                            Console.WriteLine($"Writing {symbolTableFilename} ({symbolTableBytes.Length} bytes)...");
                            File.WriteAllBytes(symbolTableFilename, symbolTableBytes);

                            Console.WriteLine($"Writing {o.Output} ({bytes.Length} bytes)");
                            File.WriteAllBytes(o.Output, bytes);
                        }
                        else
                        {
                            Console.WriteLine($"Failed to write (Assember returned no bytes!)");
                        }
                        return;
                    }
                    case "run":
                    {
                        RunVirtualMachine(o.Filename);
                        return;
                    }
                    case "vterm":
                    {
                        var vterm = new VTerm();
                        vterm.LoadSource(o.Filename, 0);
                        vterm.Run();
                        return;
                    }
                    default:
                    {
                        Console.WriteLine("[default mode]");

                        var sourceText = File.ReadAllText(o.Filename);
                        Console.WriteLine($"File: {o.Filename} ({Encoding.UTF8.GetByteCount(sourceText)} bytes)");
                        
                        AssemblerBuildResult asmResult = null;
                        using (var consoleWriter = new StreamWriter(Console.OpenStandardOutput(), null, -1, true))
                        {
                            var asm = new Assembler(consoleWriter);
                            asm.MaxLogLevel = Assembler.LogLevel.Error;
                            asmResult = asm.Build(sourceText);
                        }

                        var byteGroup = asmResult.FinalBytes;

                        Console.WriteLine("Bytecode:");
                        var finalBytes = byteGroup.Bytes;
                        Console.WriteLine(DumpUtility.DumpBytes(byteGroup.Bytes));

                        Console.WriteLine("DumpSymbolTable:");
                        Console.Write(byteGroup.SymbolTable.GetDump(true));

                        var symbolTableBytes = byteGroup.SymbolTable.GetBytes();
                        //Console.WriteLine("Symbol Table Bytes:");
                        //Console.WriteLine(DumpUtility.DumpBytes(symbolTableBytes));

                        var symbolTableFilename = $"{o.Output}.st";
                        if (!string.IsNullOrEmpty(o.SymbolTable))
                        {
                            symbolTableFilename = o.SymbolTable;
                        }
                        
                        Console.WriteLine($"Writing {symbolTableFilename} ({symbolTableBytes.Length} bytes)...");
                        File.WriteAllBytes(symbolTableFilename, symbolTableBytes);

                        var symbolTable2 = SymbolTable.FromFile(symbolTableFilename);
                        Console.Write(symbolTable2.GetDump());

                        //bytes = BytecodeUtil.Relocate(byteGroup, 0x0002);

                        Console.WriteLine($"Writing {o.Output} ({finalBytes.Length} bytes)...");
                        File.WriteAllBytes(o.Output, finalBytes);

                        Console.WriteLine();
                        
                        /*
                        var sb = new StringBuilder();
                        var doc = new DocumentBuilder();
                        var byteIndex = 0;
                        Disassembler.PrimitiveDisassembleInstructions(doc, finalBytes);
                        foreach(var chunk in doc.GetChunkEnumerable())
                        {
                            sb.Append($"{byteIndex:X4}: ");
                            switch (chunk.chunkType)
                            {
                                case DocumentChunkType.Instruction:
                                {
                                    var opcodeInstance = (OpcodeInstance)chunk.dataObject;
                                    var bytes = opcodeInstance.GetBytes();

                                    sb.Append($"{bytes[0]:X2}{bytes[1]:X2}");
                                    if (opcodeInstance.Def.ByteLength == 4)
                                    {
                                        sb.Append($" {bytes[2]:X2}{bytes[3]:X2}");
                                    }
                                    else
                                    {
                                        sb.Append(new string(' ', 5));
                                    }
                                    sb.Append($" {DocumentBuilder.FancyPrintColumnSep} ");
                                    sb.Append(opcodeInstance.GetString());
                                    sb.AppendLine();
                                    byteIndex += bytes.Length;
                                    continue;
                                }
                                case DocumentChunkType.String:
                                {
                                    throw new NotImplementedException();
                                }
                                case DocumentChunkType.Binary:
                                {
                                    var byteArray = (byte[])chunk.dataObject;
                                    //DumpUtility.DumpBytes(sb, byteArray);
                                    sb.Append($"BIN  {byteArray.Length:X4}");
                                    sb.Append($" {DocumentBuilder.FancyPrintColumnSep} ");
                                    var byteDumpStr = DumpUtility.DumpBytes(byteArray);
                                    var byteDumpLines = byteDumpStr.Split(Environment.NewLine);
                                    sb.Append(byteDumpLines[0]);
                                    sb.AppendLine();
                                    if (byteDumpLines.Length > 1)
                                    {
                                        for (int x = 1; x < byteDumpLines.Length; x++)
                                        {
                                            sb.Append(new string(' ', 15));
                                            sb.Append($" {DocumentBuilder.FancyPrintColumnSep} ");
                                            sb.Append(byteDumpLines[x]);
                                            sb.AppendLine();
                                        }
                                    }
                                    byteIndex += byteArray.Length;
                                    continue;
                                }
                            }
                        }

                        Console.WriteLine(sb.ToString());
                        */

                        Console.WriteLine("Process complete!");

                        Console.WriteLine("Key Menu:");
                        Console.WriteLine("N - Exit now");
                        Console.WriteLine("S - Simple VM load");
                        Console.WriteLine("Else: VTerm load");

                        var result = Console.ReadKey(true);
                        switch (result.Key)
                        {
                            case ConsoleKey.N:
                            {
                                break;
                            }
                            case ConsoleKey.S:
                            {
                                RunVirtualMachine(o.Output);
                                break;
                            }
                            default:
                            {
                                var vterm = new VTerm();
                                vterm.LoadSource(o.Filename, 0);
                                vterm.Run();
                                break;
                            }
                        }

                        return;
                    }
                }
            });
        }

        static void RunVirtualMachine(string filename)
        {
            const short baseAddress = 0x0000;

            var bytes = File.ReadAllBytes(filename);
            Console.WriteLine($"Running {Path.GetFileName(filename)} ({bytes.Length} bytes)");

            var memBus = new DefaultMemoryBus();
            memBus.CopyInto(bytes, baseAddress);

            //var ioBus = new DefaultIOBus(Console.Write);
            var ioBus = new DefaultIOBusBridge(true, DefaultIOBusBridge.UnconnectedPortBehaviour.Excep);
            ioBus.Connect(new ConsoleBusDevice());

            var vm = new VirtualMachine(memBus, ioBus);
            vm.ProgramCounter = baseAddress;

            int totalSteps = 0;
            while (!vm.IsHalted)
            {
                totalSteps += vm.StepInstruction();
            }

            Console.WriteLine();
            Console.WriteLine("program complete");
            Console.ReadLine();
        }
    }
}
