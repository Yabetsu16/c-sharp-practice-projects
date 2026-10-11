using Microsoft.EntityFrameworkCore;
using project_1.Data;
using project_1.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/api/tasks", async (AppDbContext db) =>
{
    var tasks = await db.Tasks.ToListAsync();

    return tasks;
});

app.MapGet("/api/tasks/{id}", async (int id, AppDbContext db) =>
{
    if (!IsValidTaskId(id))
    {
        return Results.BadRequest(new
        {
            message = "Task ID must be greater than 0."
        });
    }

    var task = await db.Tasks.FindAsync(id);

    if (task is null)
    {
        return Results.NotFound(new
        {
            message = $"Task with ID {id} was not found."
        });
    }

    return Results.Ok(task);
});

app.MapPost("/api/tasks", async (CreateTaskRequest request, AppDbContext db) =>
{
    var validationError = ValidateTaskTitle(request.Title);

    if (validationError is not null)
    {
        return Results.BadRequest(new
        {
            message = validationError
        });
    }

    var title = request.Title!.Trim();

    var newTask = new TaskItem
    {
        Title = title,
        IsCompleted = false,
        CreatedAt = DateTime.UtcNow
    };

    db.Tasks.Add(newTask);
    await db.SaveChangesAsync();

    return Results.Created($"/api/tasks/{newTask.Id}", newTask);
});

app.MapPut("/api/tasks/{id}", async (int id, UpdateTaskRequest request, AppDbContext db) =>
{
    if (!IsValidTaskId(id))
    {
        return Results.BadRequest(new
        {
            message = "Task ID must be greater than 0."
        });
    }

    var task = await db.Tasks.FindAsync(id);

    if (task is null)
    {
        return Results.NotFound(new
        {
            message = $"Task with ID {id} was not found."
        });
    }

    var validationError = ValidateTaskTitle(request.Title);

    if (validationError is not null)
    {
        return Results.BadRequest(new
        {
            message = validationError
        });
    }

    var title = request.Title!.Trim();

    task.Title = title;
    task.Description = request.Description;
    task.IsCompleted = request.IsCompleted;

    await db.SaveChangesAsync();

    return Results.Ok(task);
});

app.MapDelete("/api/tasks/{id}", async (int id, AppDbContext db) =>
{
    if (!IsValidTaskId(id))
    {
        return Results.BadRequest(new
        {
            message = "Task ID must be greater than 0."
        });
    }

    var task = await db.Tasks.FindAsync(id);

    if (task is null)
    {
        return Results.NotFound(new
        {
            message = $"Task with ID {id} was not found."
        });
    }

    db.Tasks.Remove(task);
    await db.SaveChangesAsync();

    return Results.NoContent();
});

app.Run();

static bool IsValidTaskId(int id)
{
    return id > 0;
}

static string? ValidateTaskTitle(string? title)
{
    if (string.IsNullOrWhiteSpace(title))
    {
        return "Title is required.";
    }

    title = title.Trim();

    if (title.Length < 3 || title.Length > 100)
    {
        return "Title must be between 3 and 100 characters.";
    }

    return null;
}
record CreateTaskRequest(string? Title);


public partial class Program { }