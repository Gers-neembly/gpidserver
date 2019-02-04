using System;

namespace Neembly.GPIDServer.WebAPI.Exceptions
{
    public class NotFoundException : Exception
    {
        public NotFoundException(string name, object key)
            : base($"Record for \"{key}\" in \"{name}\"  was not found.")
        {}
    }
}
