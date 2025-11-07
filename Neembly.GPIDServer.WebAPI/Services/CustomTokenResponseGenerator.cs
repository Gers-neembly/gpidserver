using IdentityServer4.ResponseHandling;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using IdentityServer4.Validation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Neembly.GPIDServer.SharedClasses;
using System;
using System.Net.Http;
using System.Threading.Tasks;

public class CustomTokenResponseGenerator : TokenResponseGenerator
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CustomTokenResponseGenerator> _logger;
    private readonly AuthTokenInfo _authTokenInfo;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CustomTokenResponseGenerator(
        ISystemClock clock,
        ILogger<TokenResponseGenerator> baseLogger,
        ITokenService tokenService,
        IRefreshTokenService refreshTokenService,
        IResourceStore resources,
        IClientStore clients,
        IHttpClientFactory httpClientFactory,
        ILogger<CustomTokenResponseGenerator> logger,
         AuthTokenInfo authTokenInfo,
         IHttpContextAccessor httpContextAccessor)
        : base(clock, tokenService, refreshTokenService, resources, clients, logger)
    {
        _httpClient = httpClientFactory.CreateClient();
        _logger = logger;
        _authTokenInfo = authTokenInfo;
        _httpContextAccessor = httpContextAccessor;
    }

    public override async Task<TokenResponse> ProcessAsync(TokenRequestValidationResult request)
    {
        var response = await base.ProcessAsync(request);
        try
        {
            var httpContext = _httpContextAccessor.HttpContext;

            var userAgent = httpContext?.Request.Headers["User-Agent"].ToString();
            var origin = httpContext?.Request.Headers["Origin"].ToString();
            var referer = httpContext?.Request.Headers["Referer"].ToString();
            var sessionId = httpContext?.Request.Headers["SessionId"].ToString();
            var username = request.ValidatedRequest.UserName;
            var token = response.AccessToken;

            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(token))
            {
                var url = $"{_authTokenInfo.ApiUrl}/api/players/signin?username={username}";
                var req = new HttpRequestMessage(HttpMethod.Get, url);

                req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                req.Headers.TryAddWithoutValidation("User-Agent", userAgent);
                req.Headers.TryAddWithoutValidation("Origin", origin);
                req.Headers.TryAddWithoutValidation("Referer", referer);
                req.Headers.TryAddWithoutValidation("SessionId", sessionId);

                var httpResponse = await _httpClient.SendAsync(req);
                httpResponse.EnsureSuccessStatusCode();


                _logger.LogInformation("Successfully notified SignIn API for user {UserId}", username);
            }
            else
            {
                _logger.LogWarning("Skipped SignIn API call: missing username or token");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling SignIn API after token issuance");
        }
        return response;
    }
}
