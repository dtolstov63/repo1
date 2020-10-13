using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Injector
{
    public class InjectorRt
    {
        static public void CallMe(System.Reflection.MethodBase mb)
        {
            ParameterInfo[] pars = mb.GetParameters();
            foreach (ParameterInfo p in pars)
            {
               // Console.WriteLine(p.ParameterType);
            }

            Console.WriteLine("CALL:" + mb.Name + "()" );
        }
    }
}
