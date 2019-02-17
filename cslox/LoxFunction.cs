using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cslox
{
    class LoxFunction : ICallable
    {
        private FunctionDeclaration _decl;

        public LoxFunction(FunctionDeclaration decl)
        {
            _decl = decl;
        }

        public int Arity => _decl.Parameters.Count;

        public object Call(Interpreter interpreter, List<object> args)
        {
            Environment env = new Environment(interpreter.Globals);
            for(int i = 0; i < _decl.Parameters.Count; i++)
                env.Define(_decl.Parameters[i].Lexeme, args[i]);

            interpreter.Execute(_decl.Body, env);

            return null;
        }

        public override string ToString()
        {
            return $"<fn {_decl.Name.Lexeme}>";
        }
    }
}
