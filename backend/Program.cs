const string URLS_ENV_KEY = "ASPNETCORE_URLS";

var builder = WebApplication.CreateBuilder(args);
DotNetEnv.Env.Load();

if (Environment.GetEnvironmentVariable(URLS_ENV_KEY) != null) {
    string[] urls = Environment.GetEnvironmentVariable(URLS_ENV_KEY)!.Split(";");
    builder.WebHost.UseUrls(urls);
}
var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();