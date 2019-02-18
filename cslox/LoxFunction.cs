using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cslox
{
    class LoxFunction : ICallable
    {
        public class ReturnException : Exception
        {
            public readonly object ReturnValue;

            public ReturnException(object value)
            {
                ReturnValue = value;
            }
        }

        public static void Return(object value)
        {
            throw new ReturnException(value);
        }

        private readonly FunctionDeclaration _decl;
        private readonly Environment _closure;

        public LoxFunction(FunctionDeclaration decl, Environment closure)
        {
            _decl = decl;
            _closure = closure;
        }

        public int Arity => _decl.Parameters.Count;

        public object Call(Interpreter interpreter, List<object> args)
        {
            Environment env = new Environment(_closure);
            for(int i = 0; i < _decl.Parameters.Count; i++)
                env.Define(_decl.Parameters[i].Lexeme, args[i]);

            try
            {
                interpreter.Execute(_decl.Body, env);
            }
            catch(ReturnException returnEx)
            {
                return returnEx.ReturnValue;
            }

            return null;
        }

        public override string ToString()
        {
            return $"<fn {_decl.Name.Lexeme}>";
        }
    }
}
