using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using project_1.Data;
using Xunit;

namespace project_1.Tests
{
    public class TaskApiTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public TaskApiTests(CustomWebApplicationFactory factory)
        {
            using var scope = factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureCreated();

            _client = factory.CreateClient();
        }

        [Fact]
        public async Task PostTask_WithTooShortTitle_ReturnsBadRequest()
        {
            var response = await _client.PostAsJsonAsync("/api/tasks", new { title = "Hi" });
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task PostTask_WithValidTitle_ReturnsCreatedTask()
        {
            var response = await _client.PostAsJsonAsync("/api/tasks", new { title = "Valid Task Title" });

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        public async Task GetTasks_ReturnsOk()
        {
            var response = await _client.GetAsync("/api/tasks");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetTask_WithNonexistentId_ReturnsNotFound()
        {
            var response = await _client.GetAsync("/api/tasks/999");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task PutTask_WithValidData_ReturnsOk()
        {
            // Create a task first.
            var createResponse = await _client.PostAsJsonAsync("/api/tasks", new { title = "Task to Update" });

            Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

            var createdTask = await createResponse.Content.ReadFromJsonAsync<TaskResponse>();

            Assert.NotNull(createdTask);

            // Now, update the task
            var updateResponse = await _client.PutAsJsonAsync($"/api/tasks/{createdTask.Id}", new { title = "Updated task Title", description = "Updated Description", isCompleted = true });

            Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        }

        [Fact]
        public async Task PutTask_WithNonexistentId_ReturnsNotFound()
        {
            var response = await _client.PutAsJsonAsync("/api/tasks/999", new { title = "Updated Title", description = "Updated Description", isCompleted = true });

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task DeleteTask_WithExistingId_ReturnsNoContent()
        {
            // Create a task to delete.
            var createResponse = await _client.PostAsJsonAsync("api/tasks",
            new
            {
                title = "Task to Delete",
                description = "Testing DELETE"
            });

            Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

            var createdTask = await createResponse.Content.ReadFromJsonAsync<TaskResponse>();

            Assert.NotNull(createdTask);

            // Delete the task using its actual ID.
            var deleteResponse = await _client.DeleteAsync($"/api/tasks/{createdTask.Id}");

            Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

            // Verify that the task no longer exists.
            var getResponse = await _client.GetAsync($"/api/tasks/{createdTask.Id}");

            Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
        }

        [Fact]
        public async Task DeleteTask_WithNonexistentId_ReturnsNotFound()
        {
            var response = await _client.DeleteAsync("/api/tasks/999");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        private record TaskResponse(int Id);
    }
}