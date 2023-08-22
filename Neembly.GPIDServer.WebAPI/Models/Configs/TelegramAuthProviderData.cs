namespace Neembly.GPIDServer.WebAPI.Models.Configs
{
    public class TelegramAuthProviderData
    {
        public string Bot_Id { get; set; }
        public string ClientId { get; set; }
        public string Secret { get; set; }
        public string Scope { get; set; }
        public string Public_Key { get; set; }
        public string Nonce { get; set; }
    }
}
