using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace EasyRoslynScript
{
    public class ScriptRunner : IScriptRunner
    {
        private readonly IEnumerable<IScriptPreCompileHandler> _preCompileHandlers;

        public ScriptRunner(IEnumerable<IScriptPreCompileHandler> preCompileHandlers)
        {
            _preCompileHandlers = preCompileHandlers;
        }

        public string Script { get; private set; }

        public async Task ExecuteFile(string path)
        {
            var dir = Path.GetDirectoryName(path);
            await Execute(File.ReadAllText(path), dir);
        }

        public async Task ExecuteString(string script)
        {
            await Execute(script);
        }

        private async Task Execute(string scriptText, string folder = "")
        {
            var references = new List<Assembly>();
            var imports = new List<string>();

            //Process all script handlers.
            foreach (var handler in _preCompileHandlers.OrderBy(h => h.Priority))
            {
                scriptText = handler.Process(scriptText, folder);
                references.AddRange(handler.References);
                imports.AddRange(handler.Imports);
            }

            Script = scriptText;

            //Remove duplicates
            references = references.Distinct().ToList();
            imports = imports.Distinct().ToList();

            var script = CSharpScript.Create(scriptText)
                .WithOptions(ScriptOptions.Default
                    .AddReferences(references)
                    .AddImports(imports)
                );

            try
            {
                var result = await script.RunAsync();

                if (result.Exception != null)
                    throw new EasyRoslynException("Script executed but threw the following exception!", result.Exception, scriptText);
            }
            catch (EasyRoslynException)
            {
                throw;
            }
            catch(Exception e)
            {
                throw new EasyRoslynException("There was a problem running the script. See inner exception for details!", e, scriptText);
            }
        }
    }
}
