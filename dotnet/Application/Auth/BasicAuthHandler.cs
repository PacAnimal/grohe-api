using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using Application.Utils;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Application.Auth;

// handles basic authentication
public class BasicAuthHandler(
    IConfiguration config,
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{   
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        await Task.CompletedTask;
        
        // check if the request has an authorization header
        if (!Request.Headers.ContainsKey("Authorization"))
        {
            return AuthenticateResult.NoResult();
        }

        // check if the authorization header is valid
        var authHeader = AuthenticationHeaderValue.Parse(Request.Headers.Authorization);
        if (authHeader.Scheme != "Basic")
        {
            return AuthenticateResult.NoResult();
        }

        // check if the username and password are correct
        var credentialBytes = Convert.FromBase64String(authHeader.Parameter!);
        var credentials = Encoding.UTF8.GetString(credentialBytes).Split(':', 2);
        if (credentials.Length != 2)
        {
            return AuthenticateResult.Fail("Invalid credentials");
        }

        // parse username and password
        var username = credentials[0];
        var password = credentials[1];
        if (username != config.GetString("API_USER") || password != config.GetString("API_PASS"))
        {
            return AuthenticateResult.Fail("Invalid credentials");
        }
        
        // authentication successful
        var ticket = new AuthenticationTicket(
            new ClaimsPrincipal(
                new ClaimsIdentity(
                    new[]
                    {
                        new Claim(ClaimTypes.Name, username)
                    },
                    Scheme.Name
                )
            ),
            Scheme.Name
        );
        
        // return success
        return AuthenticateResult.Success(ticket);
    }
}