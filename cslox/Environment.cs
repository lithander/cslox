using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cslox
{
    class Environment
    {
        private Dictionary<string, object> _globals = new Dictionary<string, object>();

        internal object Get(Token name)
        {
            if (_globals.TryGetValue(name.Lexeme, out var value))
                return value;

            //TODO: consider defining and using EnvironmentError
            throw new Interpreter.InterpreterError(name, "Variable must be declared before use!");
        }

        internal void Declare(string name, object value)
        {
            _globals[name] = value;
        }
    }
}
