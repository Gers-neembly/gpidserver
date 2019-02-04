using System;

namespace Neembly.GPIDServer.WebAPI.Exceptions
{
    public class DeleteFailureException : Exception
    {
        public DeleteFailureException(string name, object key, string message)
            : base($"Deletion of record in \"{name}\" ({key}) failed. {message}")
        { }
    }
}
