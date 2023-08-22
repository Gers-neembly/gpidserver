
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Encodings.Web;

namespace Neembly.GPIDServer.Security.OAuth.Telegram
{
    public partial class TelegramAuthenticationHandler : OAuthHandler<TelegramAuthenticationOptions>
    {
        public TelegramAuthenticationHandler(
            IOptionsMonitor<TelegramAuthenticationOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock)
            : base(options, logger, encoder, clock)
        {
        }
    }
}
