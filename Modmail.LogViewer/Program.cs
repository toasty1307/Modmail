using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OAuth;
using Modmail.Bot;
using Modmail.Config;
using Modmail.Data;
using Newtonsoft.Json;
using Serilog;
using Serilog.Events;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Debug)
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] [{SourceContext}] {Message}{NewLine}{Exception}")
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseSerilog();
builder.Configuration.AddEnvironmentVariables()
    .AddJsonFile("Config.json", true, true);
builder.Services.AddDbContext<GuildContext>(ServiceLifetime.Singleton);
builder.Services
    .AddControllersWithViews()
    .AddRazorRuntimeCompilation();
builder.Services.AddHostedService(provider =>
{
    var db = provider.GetService<GuildContext>() ?? throw new InvalidOperationException("Could not resolve GuildContext");
    var section = builder.Configuration.Get<BotConfig>();
    var botService = new BotService(section, db);
    return botService;
});
builder.Services.AddAuthentication(options =>
    {   
        options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    })
    .AddCookie(options =>
    {
        options.AccessDeniedPath = "/forbidden";
    })
    .AddOAuth("Discord", options =>
    {
        options.AuthorizationEndpoint = "https://discord.com/api/oauth2/authorize";
        options.TokenEndpoint = "https://discord.com/api/oauth2/token";
        options.UserInformationEndpoint = "https://discord.com/api/users/@me";
        options.CallbackPath = "/callback";
        options.ClientId = builder.Configuration["Server:DiscordClientId"];
        options.ClientSecret = builder.Configuration["Server:DiscordClientSecret"];
        options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id");
        options.ClaimActions.MapJsonKey(ClaimTypes.Hash, "avatar");
        options.ForwardSignOut = "Discord";
        options.AccessDeniedPath = "/forbidden";
        options.Scope.Add("identify");
        options.Scope.Add("guilds");
        options.Events = new OAuthEvents
        {
            OnCreatingTicket = async context =>
            {
                var request = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", context.AccessToken);
                
                var response = await context.Backchannel.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, context.HttpContext.RequestAborted);
                response.EnsureSuccessStatusCode();

                var user = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;
                context.RunClaimActions(user);
                request = new HttpRequestMessage(HttpMethod.Get, "https://discord.com/api/users/@me/guilds");
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", context.AccessToken);
                response = await context.Backchannel.SendAsync(request, HttpCompletionOption.ResponseHeadersRead,
                    context.HttpContext.RequestAborted);
                response.EnsureSuccessStatusCode();
                var guilds =
                    JsonConvert.DeserializeObject<
                        List<Dictionary<string, object>>>(
                        await response.Content.ReadAsStringAsync());
                if (guilds != null)
                {
                    var guildIds = guilds.Select(x => (string)x["id"]).ToList();
                    context.Identity!.AddClaim(new Claim("Guilds", string.Join(";", guildIds)));
                }
            }
        };
    });
var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAuthentication();
app.UseRouting();
app.UseAuthorization();
app.MapControllers();
app.Run();

