using System.Text.Json.Serialization;
using TodoAPI.Contexts;
using TodoAPI.Endpoints;
using TodoAPI.Models;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<ApplicationDbContext>();
builder.Services.AddIdentityApiEndpoints<TodoUser>().AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddAuthorization();
builder.Services.ConfigureHttpJsonOptions(opt =>
{
    opt.SerializerOptions.IncludeFields = true;
    opt.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});

var app = builder.Build();
app.MapIdentityApi<TodoUser>();
app.MapTodoItems();
app.MapTodoLists();

app.Run();
