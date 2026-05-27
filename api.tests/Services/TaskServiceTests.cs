using Xunit;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Api.Data;
using Api.Models;
using Api.Services;
using Api.Repositories;
using Api.Types;
using Microsoft.Extensions.Logging;
using Moq;

namespace Api.Tests.Services;

public class TaskServiceTests
{
    private AppDbContext CreateInMemoryDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private (AppDbContext db, TaskService taskService) CreateTaskService()
    {
        var db = CreateInMemoryDb();
        var mockProjectRepoLogger = new Mock<ILogger<ProjectRepository>>();
        var projectRepository = new ProjectRepository(db, mockProjectRepoLogger.Object);
        var taskRepository = new TaskRepository(db);
        var commentRepository = new CommentRepository(db);
        var mockLogger = new Mock<ILogger<TaskService>>();
        var taskService = new TaskService(taskRepository, projectRepository, commentRepository, mockLogger.Object);
        return (db, taskService);
    }

    private User CreateUser(AppDbContext db, string name = "Test User", string email = "test@test.com")
    {
        var user = new User { Name = name, Email = email, PasswordHash = "hash" };
        db.Users.Add(user);
        db.SaveChanges();
        return user;
    }

    private Project CreateProject(AppDbContext db, Guid ownerId, string title = "Test Project")
    {
        var project = new Project { Title = title, OwnerId = ownerId };
        db.Projects.Add(project);
        db.SaveChanges();
        return project;
    }

    private TodoTask CreateTask(AppDbContext db, Guid projectId, Guid createdById, string title = "Test Task")
    {
        var task = new TodoTask
        {
            Title = title,
            ProjectId = projectId,
            CreatedById = createdById,
            Priority = "Medium",
            Status = "Todo"
        };
        db.Tasks.Add(task);
        db.SaveChanges();
        return task;
    }

    [Fact]
    public async Task GetTaskById_WhenUserIsProjectMember_ReturnsTask()
    {
        var (db, taskService) = CreateTaskService();
        var owner = CreateUser(db, "Owner", "owner@test.com");
        var member = CreateUser(db, "Member", "member@test.com");
        var project = CreateProject(db, owner.Id);
        db.ProjectMembers.Add(new ProjectMember { ProjectId = project.Id, UserId = member.Id });
        db.SaveChanges();
        var task = CreateTask(db, project.Id, owner.Id);

        var result = await taskService.GetTaskById(task.Id, member.Id);

        result.Should().NotBeNull();
        result.Title.Should().Be("Test Task");
    }

    [Fact]
    public async Task GetTaskById_WhenUserIsNotProjectMember_ThrowsKeyNotFoundException()
    {
        var (db, taskService) = CreateTaskService();
        var owner = CreateUser(db, "Owner", "owner@test.com");
        var otherUser = CreateUser(db, "Other", "other@test.com");
        var project = CreateProject(db, owner.Id);
        var task = CreateTask(db, project.Id, owner.Id);

        var act = async () => await taskService.GetTaskById(task.Id, otherUser.Id);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task UpdateTask_WhenUserIsCreator_UpdatesTask()
    {
        var (db, taskService) = CreateTaskService();
        var user = CreateUser(db);
        var project = CreateProject(db, user.Id);
        var task = CreateTask(db, project.Id, user.Id);

        var result = await taskService.UpdateTask(task.Id, user.Id, new UpdateTaskRequest(
            "Updated Title", null, "High", "In Progress", null, null, null));

        result.Title.Should().Be("Updated Title");
        result.Priority.Should().Be("High");
        result.Status.Should().Be("In Progress");
    }

    [Fact]
    public async Task UpdateTask_WhenUserIsNotCreator_CanOnlyUpdateStatus()
    {
        var (db, taskService) = CreateTaskService();
        var owner = CreateUser(db, "Owner", "owner@test.com");
        var member = CreateUser(db, "Member", "member@test.com");
        var project = CreateProject(db, owner.Id);
        db.ProjectMembers.Add(new ProjectMember { ProjectId = project.Id, UserId = member.Id });
        db.SaveChanges();
        var task = CreateTask(db, project.Id, owner.Id, "Original Title");

        var result = await taskService.UpdateTask(task.Id, member.Id, new UpdateTaskRequest(
            "Changed Title", null, null, "Done", null, null, DateTime.UtcNow));

        result.Title.Should().Be("Original Title");
        result.Status.Should().Be("Done");
        result.CompletedByName.Should().Be("Member");
    }

    [Fact]
    public async Task DeleteTask_WhenUserIsCreator_DeletesTask()
    {
        var (db, taskService) = CreateTaskService();
        var user = CreateUser(db);
        var project = CreateProject(db, user.Id);
        var task = CreateTask(db, project.Id, user.Id);

        await taskService.DeleteTask(task.Id, user.Id);

        db.Tasks.Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteTask_WhenUserIsNotCreator_ThrowsUnauthorizedAccessException()
    {
        var (db, taskService) = CreateTaskService();
        var owner = CreateUser(db, "Owner", "owner@test.com");
        var member = CreateUser(db, "Member", "member@test.com");
        var project = CreateProject(db, owner.Id);
        db.ProjectMembers.Add(new ProjectMember { ProjectId = project.Id, UserId = member.Id });
        db.SaveChanges();
        var task = CreateTask(db, project.Id, owner.Id);

        var act = async () => await taskService.DeleteTask(task.Id, member.Id);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task CreateTask_WithEmptyTitle_ThrowsArgumentException()
    {
        var (db, taskService) = CreateTaskService();
        var user = CreateUser(db);
        var project = CreateProject(db, user.Id);

        var act = async () => await taskService.CreateTask(project.Id, user.Id,
            new CreateTaskRequest("", null, null, null, null, null));

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Title is required");
    }

    [Fact]
    public async Task AddCommentToTask_WithValidData_ReturnsComment()
    {
        var (db, taskService) = CreateTaskService();
        var user = CreateUser(db);
        var project = CreateProject(db, user.Id);
        var task = CreateTask(db, project.Id, user.Id);

        var result = await taskService.AddCommentToTask(task.Id, user.Id, new CreateCommentRequest("Task comment!"));

        result.Should().NotBeNull();
        result.Body.Should().Be("Task comment!");
        result.TaskId.Should().Be(task.Id);
    }

    [Fact]
    public async Task AddCommentToTask_WithEmptyBody_ThrowsArgumentException()
    {
        var (db, taskService) = CreateTaskService();
        var user = CreateUser(db);
        var project = CreateProject(db, user.Id);
        var task = CreateTask(db, project.Id, user.Id);

        var act = async () => await taskService.AddCommentToTask(task.Id, user.Id, new CreateCommentRequest(""));

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Body is required");
    }

    [Fact]
    public async Task AddCommentToTask_WhenUserIsNotProjectMember_ThrowsKeyNotFoundException()
    {
        var (db, taskService) = CreateTaskService();
        var owner = CreateUser(db, "Owner", "owner@test.com");
        var otherUser = CreateUser(db, "Other", "other@test.com");
        var project = CreateProject(db, owner.Id);
        var task = CreateTask(db, project.Id, owner.Id);

        var act = async () => await taskService.AddCommentToTask(task.Id, otherUser.Id, new CreateCommentRequest("Hello!"));

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
