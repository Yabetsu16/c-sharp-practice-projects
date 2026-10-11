namespace project_1.Models;

public record UpdateTaskRequest(
    string? Title,
    string? Description,
    bool IsCompleted
);