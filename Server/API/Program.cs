using DataAccess;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

builder.Services.AddControllers();
builder.Services.AddDbContext<DebugDbContext>(options => 
    options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));


var app = builder.Build();

app.MapControllers();
app.Run();
