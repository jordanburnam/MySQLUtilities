using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace RJBMySQLUtilities.DataLogging.Helpers
{
    static class ErrorHelper
    {
       [MethodImpl(MethodImplOptions.NoInlining)]
       public static string GetCurrentMethodName()
       {
           StackTrace st = new StackTrace();
           StackFrame sf = st.GetFrame(1);

           return sf.GetMethod().Name;
       }
    }
}
