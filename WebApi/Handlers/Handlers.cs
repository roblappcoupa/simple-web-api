namespace WebApi.Handlers;

using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

public abstract class QueryStringAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    protected QueryStringAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }
    
    protected abstract string SchemeName { get; }

    protected abstract string GetUser();

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!this.Request.Query.TryGetValue(AuthConstants.SchemeQueryStringParameterName, out var strValues))
        {
            return Task.FromResult(
                AuthenticateResult.Fail($"Query string parameter '{AuthConstants.SchemeQueryStringParameterName}' is missing"));
        }
        
        var queryParamValue = strValues.ToString();

        if (string.IsNullOrEmpty(queryParamValue))
        {
            return Task.FromResult(
                AuthenticateResult.Fail($"Query string parameter '{AuthConstants.SchemeQueryStringParameterName}' is null or empty"));
        }
        
        if (!string.Equals(queryParamValue, this.SchemeName, StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult(AuthenticateResult.Fail($"Query string value is not the expected scheme ({this.SchemeName})"));
        }
        
        // Custom authentication logic
        var user = this.GetUser();
        var claims = new[] { new Claim(ClaimTypes.Name, user) };
        var identity = new ClaimsIdentity(claims, this.Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, this.Scheme.Name);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}

public class CustomAuthHandler1 : QueryStringAuthHandler
{
    public CustomAuthHandler1(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override string SchemeName => AuthConstants.Scheme1;

    protected override string GetUser() => "User1";
}

public class CustomAuthHandler2 : QueryStringAuthHandler
{
    public CustomAuthHandler2(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override string SchemeName => AuthConstants.Scheme2;

    protected override string GetUser() => "User2";
}