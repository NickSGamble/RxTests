using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JustChat
{
    public static class ConsoleTrace
    {
        private static readonly object gate = new object();

        public static void PrintCurrentMethod([CallerMemberName] string methodName = null)
        {
            WriteLine(ConsoleColor.DarkCyan, "{0} invoked on thread {1}", methodName, Thread.CurrentThread.ManagedThreadId);
        }

        public static void WriteLine(ConsoleColor color, string format, params object[] args)
        {
            lock (gate)
            {
                Console.ForegroundColor = color;

                Console.WriteLine(format, args);

                Console.ResetColor();
            }
        }
    }
}
