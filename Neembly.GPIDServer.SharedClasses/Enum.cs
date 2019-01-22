using System.ComponentModel;

namespace Neembly.GPIDServer.SharedClasses
{
    public enum RegistrationStatusNames
    {
        [Description("Pending")]
        Pending = 1,
        [Description("Registered")]
        Registered = 2,
        [Description("Abandon")]
        Abandon = 3,
    }

    public enum HttpTransactType
    {
        [Description("Post")]
        Post = 1,
        [Description("Put")]
        Put = 2,
        [Description("Get")]
        Get = 3,
    }

}
