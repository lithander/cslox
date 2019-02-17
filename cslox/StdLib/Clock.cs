using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cslox.StdLib
{
    class Clock : ICallable
    {
        static Stopwatch _sw = new Stopwatch();

        static Clock()
        {
            _sw.Start();
        }

        public int Arity => 0;

        public object Call(Interpreter interpreter, List<object> args)
        {
            double elapsedSeconds = _sw.ElapsedTicks / (double)Stopwatch.Frequency;
            return elapsedSeconds;
        }
    }
}
