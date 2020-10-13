using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Diagnostics;

namespace Injector
{
    class Program
    {
        public static MethodDefinition GetCurrentMethodCall = null;
        public static MethodReference InjectMeRef = null;
        public static MethodReference getCurrMethodRef = null;
        public static AssemblyDefinition fakeasm;

  
        static MethodDefinition GetMethodFromAsm(AssemblyDefinition asm, string methodName)
        {
            foreach (var typeDef in asm.MainModule.Types)
            {
                foreach (var method in typeDef.Methods)
                {
                    if (method.Name == methodName) {
                        return method;
                    }
                }
            }
            return null;
        }

        static void OutputInfo(AssemblyDefinition asm, Func<MethodDefinition,bool> cbf)
        {
            foreach (var typeDef in asm.MainModule.Types)
            {
                foreach (var method in typeDef.Methods)
                {
                    cbf(method);
                }
            }
        }

        static bool OnFindMethod(MethodDefinition method)
        {
            if (method.Name == "GetCurrentMethod")
            {
                Console.WriteLine("Name = " + method.Name + " Full= " + method.FullName);
            }
            return true;
        }

        public static void AddAsmReference(ModuleDefinition module)
        {
            var references = module.AssemblyReferences;
            foreach (var r in references)
            {
                Console.WriteLine(r.FullName);
            }

        }

        public static bool InsertCil(MethodDefinition method)
        {
            Console.WriteLine("Injecting in method = " + method.Name);
            //if (method.Name == "MethodA")
            {
                var callMeRef = method.Module.ImportReference(typeof(InjectorRt).GetMethod("CallMe"));  // call implemented in InjectionRunTime.sll
                var theMet = method.Module.ImportReference(typeof(System.Reflection.MethodBase).GetMethod("GetCurrentMethod"));
                TypeReference tr =  new TypeReference("[mscorlib]System.Reflection", ".MethodBase", method.Module, method.Module);
                VariableDefinition vd = new VariableDefinition(tr);
                method.Body.Variables.Add(vd);
                method.Body.Instructions.Insert(0, Instruction.Create(OpCodes.Nop));
                method.Body.Instructions.Insert(1, Instruction.Create(OpCodes.Call, theMet));
                method.Body.Instructions.Insert(2, Instruction.Create(OpCodes.Stloc_0));
                method.Body.Instructions.Insert(3, Instruction.Create(OpCodes.Ldloc_0));
                method.Body.Instructions.Insert(4, Instruction.Create(OpCodes.Call, callMeRef));
                method.Body.Instructions.Insert(5, Instruction.Create(OpCodes.Nop));
                method.Body.Instructions.Insert(6, Instruction.Create(OpCodes.Nop));
                method.Body.Instructions.Insert(7, Instruction.Create(OpCodes.Nop));
            }
            return true;
        }


        static void Main(string[] args)
        {
            //Reading the .NET target assembly
            AssemblyDefinition asm = AssemblyDefinition.ReadAssembly(@"C:\Dev\CSharp\Injector\SomeAsm\SomeAsm\bin\Debug\SomeAsm.exe");
            // Insert CIL
            OutputInfo(asm, InsertCil);
            // Write modifyed assemply
            asm.Write(@"C:\Dev\CSharp\Injector\SomeAsm\SomeAsm\bin\Debug\SomeAsm-mod.exe");
        }

    }
}
