using System.Collections.Generic;

namespace cslox
{
    internal interface ICallable
    {
        int Arity { get; }

        object Call(Interpreter interpreter, List<object> args);
    }
}