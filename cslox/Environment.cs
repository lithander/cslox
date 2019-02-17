using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cslox
{
    class Environment
    {
        //TODO: consider defining and using EnvironmentError

        private Dictionary<string, object> _values = new Dictionary<string, object>();
        private Environment _parent = null;

        internal Environment()
        {
        }

        internal Environment(Environment parent)
        {
            _parent = parent;
        }

        internal object Get(Token name)
        {
            if (_values.TryGetValue(name.Lexeme, out var value))
                return value;

            if (_parent != null)
                return _parent.Get(name);

            throw new Interpreter.InterpreterError(name, "Variable must be declared before use!");
        }

        internal void Define(string name, object value)
        {
            _values[name] = value;
        }

        internal void Assign(Token name, object value)
        {
            if (_values.ContainsKey(name.Lexeme))
                _values[name.Lexeme] = value;
            else if (_parent != null)
                _parent.Assign(name, value);
            else
                throw new Interpreter.InterpreterError(name, "Variable must be declared before it can be assigned!");
        }
    }
}
