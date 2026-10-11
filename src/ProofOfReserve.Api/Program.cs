using Sakura.ProofOfReserve.Api.Endpoints;
using Sakura.ProofOfReserve.Api.Models;
using Sakura.ProofOfReserve.Api.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<UserDatabase>(); // The in-memory database for storing users.
builder.Services.AddScoped<UserService>();

var app = builder.Build();

app.MapUserEndpoints();

app.Run();
