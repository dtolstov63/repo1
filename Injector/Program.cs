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
        //static string MetodNameFunc = "System.Reflection.MethodBase System.Reflection.MethodBase::GetCurrentMethod()";
        public static MethodDefinition GetCurrentMethodCall = null;
        public static MethodReference InjectMeRef = null;
        public static MethodReference getCurrMethodRef = null;

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

        static bool InsertCil(MethodDefinition method)
        {
            Console.WriteLine("Injecting in method = " + method.Name);
            if (method.Name == "MethodA")
            {
               // var theMet  = method.Module.Import(typeof(System.Reflection.MethodBase).GetMethod("GetCurrentMethod"));
                var theMet = method.Module.ImportReference(typeof(System.Reflection.MethodBase).GetMethod("GetCurrentMethod"));
                TypeReference tr =  new TypeReference("[mscorlib]System.Reflection", ".MethodBase", method.Module, method.Module);
                VariableDefinition vd = new VariableDefinition(tr);
                method.Body.Variables.Add(vd);
                method.Body.Instructions.Insert(0, Instruction.Create(OpCodes.Nop));
                method.Body.Instructions.Insert(1, Instruction.Create(OpCodes.Call, theMet));
                method.Body.Instructions.Insert(2, Instruction.Create(OpCodes.Stloc_0));
               // method.Body.Instructions.Insert(3, Instruction.Create(OpCodes.Ldloc_0));
                method.Body.Instructions.Insert(4, Instruction.Create(OpCodes.Nop));
                method.Body.Instructions.Insert(5, Instruction.Create(OpCodes.Nop));
                method.Body.Instructions.Insert(6, Instruction.Create(OpCodes.Nop));
            }
            return true;
        }


        static void Main(string[] args)
        {
            var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
            Console.WriteLine(currentMethod.ToString());
            // Get GetCurrentMethod() from mscorlib
            string mscorlipPath = @"C:\Dev\CSharp\Injector\mscorlib.dll";
            /*
            AssemblyDefinition mscorasm = AssemblyDefinition.ReadAssembly(mscorlipPath);
            OutputInfo(mscorasm, (m) =>
            {
                if ( (m.Name == "GetCurrentMethod") && (m.ReturnType.Name == "MethodBase")) {
                    Console.WriteLine(m.FullName);
                    Program.GetCurrentMethodCall = m;
                }
                 return true;
            });

            if (GetCurrentMethodCall == null) {
                Console.WriteLine("Can not find method GetCurrentMethod() in mscorlib");
                return;
            }
            */
            AssemblyDefinition mscorasm = AssemblyDefinition.ReadAssembly(mscorlipPath);
            var TheMethod = typeof(System.Reflection.MethodBase).GetMethod("GetCurrentMethod");
            getCurrMethodRef = mscorasm.MainModule.Import(TheMethod);
            //getCurrMethodRef = mscorasm.MainModule.ImportReference(typeof(System.Reflection.MethodBase).GetMethod("GetCurrentMethod"));
            //getCurrMethodRef.Resolve();
            // getCurrMethodRef = mscorasm.MainModule.Import(GetCurrentMethodCall);


            //Reading the .NET target assembly
            AssemblyDefinition asm = AssemblyDefinition.ReadAssembly(@"C:\Dev\CSharp\Injector\SomeAsm\SomeAsm\bin\Debug\SomeAsm.exe");
            OutputInfo(asm, (m) =>
            {
                if (m.Name == "InjectMe")
                {
                    Program.InjectMeRef = asm.MainModule.Import(m);
                }
                return true;
            });
            if (InjectMeRef == null)
            {
                Console.WriteLine("Can not find InjectMe method");
                return;
            }

            // Insert CIL
            OutputInfo(asm, InsertCil);

#if BLAH
            foreach (var typeDef in asm.MainModule.Types) //foreach type in the target assembly
            {
                Console.WriteLine("typedefname=" + typeDef.FullName);
                foreach (var method in typeDef.Methods) {//and for each method in it too

                    Console.WriteLine("method = " + method.Name);
                    if (method.Name != "InjectMe")
                    {
                        method.Body.Instructions.Insert(0, Instruction.Create(OpCodes.Nop));
                        method.Body.Instructions.Insert(2, Instruction.Create(OpCodes.Call, InjectMeRef));
                        method.Body.Instructions.Insert(3, Instruction.Create(OpCodes.Nop));
                        method.Body.Instructions.Insert(4, Instruction.Create(OpCodes.Nop));
                    }
                    //Let's push a string using the Ldstr Opcode to the stack
                    //method.Body.Instructions.Insert(0, Instruction.Create(OpCodes.Ldstr, "INJECTED!"));

                    //We add the call to the Console.WriteLine() method. It will read from the stack
                    // method.Body.Instructions.Insert(1, Instruction.Create(OpCodes.Call, writeLineRef));

                    //We push the path of the executable you want to run to the stack
                    //method.Body.Instructions.Insert(2, Instruction.Create(OpCodes.Ldstr, @"calc.exe"));

                    //Adding the call to the Process.Start() method, It will read from the stack
                    // method.Body.Instructions.Insert(3, Instruction.Create(OpCodes.Call, pStartRef));

                    //Removing the value from stack with pop
                    // method.Body.Instructions.Insert(4, Instruction.Create(OpCodes.Pop));
                }
            }
        #endif
            asm.Write(@"C:\Dev\CSharp\Injector\SomeAsm\SomeAsm\bin\Debug\SomeAsm-mod.exe");
        }

    }
}
