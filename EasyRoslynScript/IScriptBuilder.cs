namespace EasyRoslynScript
{
    public interface IScriptBuilder
    {
        void AppendScriptFile(string path);
        void AppendScriptString(string script);
    }
}
