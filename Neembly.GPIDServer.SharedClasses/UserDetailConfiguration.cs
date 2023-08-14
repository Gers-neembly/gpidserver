namespace Neembly.GPIDServer.SharedClasses
{
    public class Avatar
    {
        public string DefaultUrl { get; set; }
        public bool Enabled { get; set; } = true;
    }
    public class UserDetailConfiguration
    {
        public Avatar AvatarInfo {get; set;}
    }
}
