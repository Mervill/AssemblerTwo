using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

using CommandLine;

using AssemblerTwo.Cmd.VirtualTerminal;
using AssemblerTwo.Lib;
using AssemblerTwo.Machine;
using System.Text;

using Parser = AssemblerTwo.Lib.Parser;
using CmdParser = CommandLine.Parser;

namespace AssemblerTwo.Cmd
{
    class Program
    {
        class Options
        {
            const string kDefaultFilename = "out.a";

            [Option('m', "mode", HelpText = "Assembler mode")]
            public string Mode { get; set; }

            [Option('f', "file", Required = true, HelpText = "File to assemble")]
            public string Filename { get; set; }

            [Option('o', "output", HelpText = "output file")]
            public string Output { get; set; } = kDefaultFilename;
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
                            var stringTokens = Assembler.StringTokenize(sourceText);
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
                        var stringTokens = Assembler.StringTokenize(sourceText);
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
                        var stringTokens = Assembler.StringTokenize(sourceText);
                        var lexTokens = Assembler.Lex(stringTokens);
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

                        var buildResult = Assembler.Build(sourceText);
                        var bytes = buildResult.FinalBytes?.Bytes;
                        if (bytes != null)
                        {
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

                        Console.WriteLine("StringTokenize...");
                        var stringTokens = Assembler.StringTokenize(sourceText);

                        //Console.WriteLine("DumpStringTokens:");
                        //Console.WriteLine(DumpUtility.DumpStringTokens(stringTokens, hideEoLCharacters: false, csvFormat: false));

                        Console.WriteLine($"Got {stringTokens.Count} string tokens");

                        Console.WriteLine("Lex...");
                        var lexTokens = Assembler.Lex(stringTokens);

                        Console.WriteLine("Parse...");
                        var parseResult = Parser.Parse(lexTokens);

                        if (parseResult.NameDirectiveToken != null)
                        {
                            Console.WriteLine($"AST Name: {parseResult.NameDirectiveToken.Value}");
                        }
                        
                        Console.WriteLine("AST Nodes:");
                        Console.Write(DumpUtility.DumpASTNodes(parseResult.SyntaxTree, true, true));
                        
                        Console.WriteLine("GenerateBytecode...");
                        var byteGroup = Assembler.GenerateBytecode(parseResult.SyntaxTree, true, true);

                        Console.WriteLine("Bytecode:");
                        var bytes = byteGroup.Bytes;
                        Console.WriteLine(DumpUtility.DumpBytes(byteGroup.Bytes));

                        Console.WriteLine("DumpSymbolTable:");
                        Console.Write(byteGroup.SymbolTable.GetDump(true));

                        var symbolTableBytes = byteGroup.SymbolTable.GetBytes();
                        //Console.WriteLine("Symbol Table Bytes:");
                        //Console.WriteLine(DumpUtility.DumpBytes(symbolTableBytes));

                        const string symbolTableFilename = "out.st";
                        Console.WriteLine($"Writing {symbolTableFilename} ({symbolTableBytes.Length} bytes)...");
                        File.WriteAllBytes(symbolTableFilename, symbolTableBytes);

                        //var symbolTable2 = SymbolTable.FromFile(symbolTableFilename);
                        //Console.Write(symbolTable2.GetDump());

                        Console.WriteLine($"Writing {o.Output} ({bytes.Length} bytes)...");
                        File.WriteAllBytes(o.Output, bytes);

                        Console.WriteLine();

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

            var ioBus = new DefaultIOBus(Console.Write);

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
