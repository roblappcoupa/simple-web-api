using Microsoft.AspNetCore.Authentication;
using WebApi;
using WebApi.Handlers;

var builder = WebApplication.CreateBuilder(args);

// builder.Services.AddAuthentication()
//     .AddScheme<AuthenticationSchemeOptions, CustomAuthHandler2>(AuthConstants.Scheme2, options => { })
//     .AddScheme<AuthenticationSchemeOptions, CustomAuthHandler1>(AuthConstants.Scheme1, options => { });

// Add services to the container.
builder.Services.AddAuthentication(
        options =>
        {
            options.DefaultAuthenticateScheme = AuthConstants.Scheme1;
            options.DefaultChallengeScheme = AuthConstants.Scheme1;
        })
    .AddScheme<AuthenticationSchemeOptions, CustomAuthHandler1>(AuthConstants.Scheme1, options => { })
    .AddScheme<AuthenticationSchemeOptions, CustomAuthHandler2>(AuthConstants.Scheme2, options => { });

// builder.Services.AddAuthorization(
//     options =>
//     {
//         var defaultAuthorizationPolicyBuilder = new AuthorizationPolicyBuilder(
//             AuthConstants.Scheme1,
//             AuthConstants.Scheme2);
//
//         defaultAuthorizationPolicyBuilder = defaultAuthorizationPolicyBuilder.RequireAuthenticatedUser();
//         options.DefaultPolicy = defaultAuthorizationPolicyBuilder.Build();
//     });

builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();