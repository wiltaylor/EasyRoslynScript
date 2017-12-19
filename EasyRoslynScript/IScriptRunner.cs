using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EasyRoslynScript
{
    public interface IScriptRunner
    {
        string Script { get; }
        Task ExecuteFile(string path);
        Task ExecuteString(string script);
    }
}
