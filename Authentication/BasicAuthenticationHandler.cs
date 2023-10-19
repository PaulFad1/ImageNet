using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using ImageNet.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

public class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly AppContext db;
    public BasicAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder url, ISystemClock clock, AppContext context):
    base(options, logger, url, clock)
    {
        db = context;
    }
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if(!Request.Headers.ContainsKey("Authorization"))
        {
            return AuthenticateResult.Fail("Authorization header was not found");
        }

        try
        {
            var authenticationHeaderValue = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
            var bytes = Convert.FromBase64String(authenticationHeaderValue.Parameter);
            string[] credentials = Encoding.UTF8.GetString(bytes).Split(":");
            string login = credentials[0];
            string password = credentials[1];

            User user = db.Users.Where(x => x.Login == login && x.Password == password).First();

            if(user == null)
            {
                return AuthenticateResult.Fail("Invalid login or password");
            }
            else
            {
                var claims = new[] {new Claim(ClaimTypes.Name, user.Login)};
                var identity = new ClaimsIdentity(claims, Scheme.Name);
                var principal = new ClaimsPrincipal(identity);
                var ticket = new AuthenticationTicket(principal,Scheme.Name);

                return AuthenticateResult.Success(ticket);
            }
        }
        catch(Exception ex)
        {
            return AuthenticateResult.Fail("Error");
        }
    }
}