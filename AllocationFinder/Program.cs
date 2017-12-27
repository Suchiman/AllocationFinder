using System;
using System.IO;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace AllocationFinder
{
    class Program
    {
        static void Main(string[] args)
        {
            string dllPath = Path.Combine(Environment.CurrentDirectory, "bin", "Debug", "net4.7.1", "AllocationFinder.exe");
            string srcBasePath = Environment.CurrentDirectory;

            var module = ModuleDefinition.ReadModule(dllPath);
            module.ReadSymbols();

            foreach (var method in module.Types.SelectMany(x => x.Methods).Where(x => x.Body != null))
            {
                foreach (var inst in method.Body.Instructions)
                {
                    if (inst.OpCode.Code == Code.Box || (inst.OpCode.Code == Code.Ldftn && inst.Operand is MethodReference methodRef && methodRef.DeclaringType.Name.Contains("<>c__")))
                    {
                        var seq = method.DebugInformation.SequencePoints.Where(x => !x.IsHidden).OrderByDescending(x => x.Offset).FirstOrDefault(x => x.Offset <= inst.Offset);
                        if (seq != null)
                        {
                            Console.WriteLine($"{GetRelativePath(seq.Document.Url, srcBasePath)}({seq.StartLine}): {(inst.OpCode.Code == Code.Box ? "Boxing " : "Closure")} -> {GetCodeSnippet(seq)}");
                        }
                        else
                        {
                            Console.WriteLine(method.ToString() + (inst.OpCode.Code == Code.Box ? " Boxing" : " Closure"));
                        }
                    }
                }
            }
        }

        private static string GetCodeSnippet(SequencePoint seq)
        {
            string[] lines;
            try
            {
                lines = File.ReadAllLines(seq.Document.Url);
            }
            catch (Exception ex)
            {
                return "Could not read file: " + ex.Message;
            }

            string line;
            if (seq.StartLine == seq.EndLine)
            {
                line = lines[seq.StartLine - 1].Substring(seq.StartColumn - 1, seq.EndColumn - seq.StartColumn);
            }
            else
            {
                line = lines[seq.StartLine - 1].Substring(seq.StartColumn - 1);
                for (int i = seq.StartLine; i < seq.EndLine - 1; i++)
                {
                    line += lines[i] + "\r\n";
                }
                line += lines[seq.EndLine - 1].Substring(0, seq.EndColumn - 1);
            }

            return line;
        }

        static string GetRelativePath(string filespec, string folder)
        {
            Uri pathUri = new Uri(filespec);
            // Folders must end in a slash
            if (!folder.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                folder += Path.DirectorySeparatorChar;
            }
            Uri folderUri = new Uri(folder);
            return Uri.UnescapeDataString(folderUri.MakeRelativeUri(pathUri).ToString().Replace('/', Path.DirectorySeparatorChar));
        }
    }
}
