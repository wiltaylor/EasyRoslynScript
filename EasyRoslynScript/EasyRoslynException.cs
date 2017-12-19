using System;
using System.Collections.Generic;
using System.Text;

namespace EasyRoslynScript
{
    public class EasyRoslynException : Exception
    {
        public string Script { get; }


        public EasyRoslynException(string message, string script) : base(message)
        {
            Script = script;
        }

        public EasyRoslynException(string message, Exception inner, string script) : base(message, inner)
        {
            Script = script;
        }
    }
}
