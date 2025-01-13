using backend.Data;
using backend.Dtos;
using backend.Entities;
using backend.Mappers;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

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
builder.Services.AddSwaggerGen(options => {
    options.AddSecurityDefinition("oauth2", new Microsoft.OpenApi.Models.OpenApiSecurityScheme {
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey
    });

    options.OperationFilter<SecurityRequirementsOperationFilter>();
});

builder.Services.AddAuthorization();
builder.Services.AddAuthentication()
    .AddBearerToken(IdentityConstants.BearerScheme);
builder.Services
    .AddIdentityCore<User>(options => { options.User.RequireUniqueEmail = true; options.SignIn.RequireConfirmedEmail = false; })
    .AddRoles<Role>()
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
app.MapPost("/api/v1/users", async (UserCreation user, UserManager<User> userManager, RoleManager<Role> roleManager) =>
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

    var findRoleTask = roleManager.FindByIdAsync(user.roleId);

    userEntity.PasswordHash = userManager.PasswordHasher.HashPassword(userEntity, user.Password);

    var role = await findRoleTask;

    if (role == null) return Results.BadRequest(new ErrorResponse("NF_InvalidRole", "Provided role is invalid"));

    await userManager.CreateAsync(userEntity);

    await userManager.AddToRoleAsync(userEntity, role.Name!);

    return Results.Created();
}).WithParameterValidation();

app.MapGet("/", () => "Hello World!");

app.Run();