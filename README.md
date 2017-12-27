# Allocation Finder
Finds boxing and closures in .NET assemblies by inspecting IL.  
Debug information is then used to locate the offending line in source code.

## Example Output
```
Program.cs(25): Closure -> var seq = method.DebugInformation.SequencePoints.Where(x => !x.IsHidden).OrderByDescending(x => x.Offset).FirstOrDefault(x => x.Offset <= inst.Offset);
Program.cs(28): Boxing  -> Console.WriteLine($"{GetRelativePath(seq.Document.Url, srcBasePath)}({seq.StartLine}): {(inst.OpCode.Code == Code.Box ? "Boxing " : "Closure")} -> {GetCodeSnippet(seq)}");
```
