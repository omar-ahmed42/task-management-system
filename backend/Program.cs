using backend.Data;
using backend.Dtos;
using backend.Entities;
using backend.Mappers;
using Microsoft.AspNetCore.Identity;

const string URLS_ENV_KEY = "ASPNETCORE_URLS";
const string CONNECTION_STRING_KEY = "CONNECTION_STRING";

var builder = WebApplication.CreateBuilder(args);
DotNetEnv.Env.Load();

if (Environment.GetEnvironmentVariable(URLS_ENV_KEY) != null)
{
    string[] urls = Environment.GetEnvironmentVariable(URLS_ENV_KEY)!.Split(";");
    builder.WebHost.UseUrls(urls);
}

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAuthorization();
builder.Services.AddAuthentication()
    .AddBearerToken(IdentityConstants.BearerScheme);
builder.Services
    .AddIdentityCore<User>(options => { options.User.RequireUniqueEmail = true; options.SignIn.RequireConfirmedEmail = false; })
    .AddEntityFrameworkStores<TaskManagementDbContext>()
    .AddApiEndpoints();

string ConnectionString = "";
if (Environment.GetEnvironmentVariable(CONNECTION_STRING_KEY) != null)
{
    ConnectionString = Environment.GetEnvironmentVariable(CONNECTION_STRING_KEY)!;
}

builder.Services.AddSqlServer<TaskManagementDbContext>(ConnectionString);

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.ApplyMigrations();
}

app.UseHttpsRedirection();
app.MapIdentityApi<User>();
app.MapPost("/api/v1/users", async (UserRegistration user, UserManager<User> userManager) =>
{

    UserMapper userMapper = new();

    User userEntity = userMapper.ToUser(user);
    userEntity.SetUsername(user.Email);

    List<ErrorResponse> errors = [];
    foreach (var validator in userManager.PasswordValidators)
    {
        var result = await validator.ValidateAsync(userManager, userEntity, user.Password);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                errors.Add(new ErrorResponse(error.Code, error.Description));
            }
        }
    }


    foreach (var validator in userManager.UserValidators)
    {
        var result = await validator.ValidateAsync(userManager, userEntity);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                if (!error.Code.Contains("UserName")) errors.Add(new ErrorResponse(error.Code, error.Description));
            }
        }
    }

    if (errors.Count > 0)
    {
        return Results.BadRequest(errors);
    }

    userEntity.PasswordHash = userManager.PasswordHasher.HashPassword(userEntity, user.Password);

    await userManager.CreateAsync(userEntity);

    return Results.Created();
}).WithParameterValidation();

app.MapGet("/", () => "Hello World!");

app.Run();