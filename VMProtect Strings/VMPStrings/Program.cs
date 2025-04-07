using dnlib.DotNet;
using dnlib.DotNet.Emit;
using dnlib.DotNet.Writer;
using System;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Reflection;

namespace VMPStrings
{
    internal class Program
    {
        
        static ModuleDefMD _module;
        static Assembly _assembly;
        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Usage: VMPStrings.exe module.exe ref_module.exe");
                Console.ReadKey();
                return;
            }
            _module = ModuleDefMD.Load(args[0]);
            _assembly = Assembly.LoadFrom(args[1]);
         
            DecryptStrings();
     
            string filePath = Path.GetDirectoryName(_module.Location);
            string fileName = Path.GetFileNameWithoutExtension(_module.Location);
            string newName = fileName + "-decrypted" + Path.GetExtension(_module.Location);


           
            var NativemoduleWriterOptions = new NativeModuleWriterOptions(_module, false);
            NativemoduleWriterOptions.MetadataOptions.Flags = MetadataFlags.PreserveAll;
            NativemoduleWriterOptions.MetadataLogger = DummyLogger.NoThrowInstance;
            _module.NativeWrite(Path.Combine(filePath, newName), NativemoduleWriterOptions);
            
            Console.WriteLine($"File saved in: {Path.Combine(filePath, newName)}");
        }

        private static void DecryptStrings()
        {
            foreach (var type in _module.GetTypes().Where(t => !t.IsGlobalModuleType))
            {
                foreach (var method in type.Methods.Where(m => m.HasBody))
                {
                    var instructions = method.Body.Instructions;

                    for (int i = 0; i < instructions.Count; i++)
                    {
                        var instruction = instructions[i];

                        if (instruction.OpCode != OpCodes.Call)
                            continue;

                        var operand = instruction.Operand as MethodDef;
                        if (operand == null || !operand.ToString().Contains("System.String") || !operand.ToString().Contains("(System.UInt32)"))
                            continue;

                        uint id = (uint)instructions[i - 1].GetLdcI4Value();
                        string decryptedStr = _assembly.ManifestModule.ResolveMethod(operand.MDToken.ToInt32()).Invoke(null, new object[] { id }) as string;

                        instruction.OpCode = OpCodes.Nop;
                        instructions[i - 1].OpCode = OpCodes.Ldstr;
                        instructions[i - 1].Operand = decryptedStr;

                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"Decrypted string: {decryptedStr}");
                    }
                }
            }
        }
    }
}
