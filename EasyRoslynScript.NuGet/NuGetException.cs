using System;

namespace EasyRoslynScript.NuGet
{
    public class NuGetException : Exception
    {
        public NuGetException(string message) : base(message)
        {
            
        }

        public NuGetException(string message, Exception inner) : base(message, inner)
        {
            
        }
    }
}
