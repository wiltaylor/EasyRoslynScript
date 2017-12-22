using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace EasyRoslynScript
{
    public class ScriptBuilder : IScriptBuilder
    {
        private readonly List<string> _hashTags = new List<string>();
        private readonly List<string> _usingTags = new List<string>();
        private readonly List<string> _scriptText = new List<string>();

        public void AppendScriptFile(string path)
        {
            var oldDir = Environment.CurrentDirectory;
            Environment.CurrentDirectory = Directory.GetParent(path).FullName;            

            var text = File.ReadAllLines(path);

            foreach(var line in text)
            {
                if(line.Trim().StartsWith("using"))
                {
                    if(_usingTags.Contains(line.Trim()))
                    {
                        _usingTags.Add(line);
                        continue;                    
                    }
                }

                if(line.Trim().StartsWith("#r"))
                {
                    var assemblyRel = line.Trim().Substring(2);
                    var fullpath = Path.GetFullPath(assemblyRel);

                    if (File.Exists(fullpath))
                        _hashTags.Add($"#r {fullpath}");
                    else
                        _hashTags.Add(line);

                    continue;
                }

                if(line.Trim().StartsWith("#"))
                {
                    _hashTags.Add(line);
                    continue;
                }

                _scriptText.Add(line);
            }

            Environment.CurrentDirectory = oldDir;
        }

        public void AppendScriptString(string script)
        {
            var text = script.Split(Environment.NewLine.ToCharArray());

            foreach (var line in text)
            {
                if (line.Trim().StartsWith("using"))
                {
                    if (_usingTags.Contains(line.Trim()))
                    {
                        _usingTags.Add(line);
                        continue;
                    }
                }

                if (line.Trim().StartsWith("#r"))
                {
                    _hashTags.Add(line);
                    continue;
                }

                if (line.Trim().StartsWith("#"))
                {
                    _hashTags.Add(line);
                    continue;
                }

                _scriptText.Add(line);
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.Append(string.Join(Environment.NewLine, _hashTags));
            sb.AppendLine();
            sb.Append(string.Join(Environment.NewLine, _usingTags));
            sb.AppendLine();
            sb.Append(string.Join(Environment.NewLine, _scriptText));

            return sb.ToString();
        }
    }
}
