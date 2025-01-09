using backend.Data;

const string URLS_ENV_KEY = "ASPNETCORE_URLS";
const string CONNECTION_STRING_KEY = "CONNECTION_STRING";

var builder = WebApplication.CreateBuilder(args);
DotNetEnv.Env.Load();

if (Environment.GetEnvironmentVariable(URLS_ENV_KEY) != null) {
    string[] urls = Environment.GetEnvironmentVariable(URLS_ENV_KEY)!.Split(";");
    builder.WebHost.UseUrls(urls);
}

builder.Services.AddSwaggerGen();

string ConnectionString = "";
if (Environment.GetEnvironmentVariable(CONNECTION_STRING_KEY) != null) {
    ConnectionString = Environment.GetEnvironmentVariable(CONNECTION_STRING_KEY)!;
}

builder.Services.AddSqlServer<TaskManagementDbContext>(ConnectionString);

var app = builder.Build();
if (app.Environment.IsDevelopment()) {
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/", () => "Hello World!");

app.Run();