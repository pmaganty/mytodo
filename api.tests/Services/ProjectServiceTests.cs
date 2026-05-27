using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Api.Data;
using Api.Models;
using Api.Services;
using Api.Repositories;
using Api.Types;

namespace Api.Tests.Services;

public class ProjectServiceTests
{
    private AppDbContext CreateInMemoryDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private (AppDbContext db, ProjectService projectService) CreateProjectService()
    {
        var db = CreateInMemoryDb();
        var projectRepository = new ProjectRepository(db);
        var commentRepository = new CommentRepository(db);
        var taskRepository = new TaskRepository(db);
        var projectService = new ProjectService(projectRepository, commentRepository, taskRepository);
        return (db, projectService);
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

    [Fact]
    public async Task GetProjectsForUser_ReturnsOnlyUsersProjects()
    {
        var (db, projectService) = CreateProjectService();
        var user1 = CreateUser(db, "User One", "user1@test.com");
        var user2 = CreateUser(db, "User Two", "user2@test.com");
        CreateProject(db, user1.Id, "User 1 Project");
        CreateProject(db, user2.Id, "User 2 Project");

        var projects = await projectService.GetProjectsForUser(user1.Id);

        projects.Should().HaveCount(1);
        projects.First().Title.Should().Be("User 1 Project");
    }

    [Fact]
    public async Task GetProjectsForUser_IncludesSharedProjects()
    {
        var (db, projectService) = CreateProjectService();
        var owner = CreateUser(db, "Owner", "owner@test.com");
        var member = CreateUser(db, "Member", "member@test.com");
        var project = CreateProject(db, owner.Id, "Shared Project");

        db.ProjectMembers.Add(new ProjectMember { ProjectId = project.Id, UserId = member.Id });
        db.SaveChanges();

        var projects = await projectService.GetProjectsForUser(member.Id);

        projects.Should().HaveCount(1);
        projects.First().Title.Should().Be("Shared Project");
    }

    [Fact]
    public async Task CreateProject_WithValidData_ReturnsProject()
    {
        var (db, projectService) = CreateProjectService();
        var user = CreateUser(db);

        var result = await projectService.CreateProject(user.Id, new CreateProjectRequest("New Project", null, null, null));

        result.Should().NotBeNull();
        result.Title.Should().Be("New Project");
    }

    [Fact]
    public async Task CreateProject_WithEmptyTitle_ThrowsArgumentException()
    {
        var (db, projectService) = CreateProjectService();
        var user = CreateUser(db);

        var act = async () => await projectService.CreateProject(user.Id, new CreateProjectRequest("", null, null, null));

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Title is required");
    }

    [Fact]
    public async Task GetProjectById_WhenUserIsOwner_ReturnsProject()
    {
        var (db, projectService) = CreateProjectService();
        var user = CreateUser(db);
        var project = CreateProject(db, user.Id);

        var result = await projectService.GetProjectById(project.Id, user.Id);

        result.Should().NotBeNull();
        result.Title.Should().Be("Test Project");
    }

    [Fact]
    public async Task GetProjectById_WhenUserIsNotOwnerOrMember_ThrowsKeyNotFoundException()
    {
        var (db, projectService) = CreateProjectService();
        var owner = CreateUser(db, "Owner", "owner@test.com");
        var otherUser = CreateUser(db, "Other", "other@test.com");
        var project = CreateProject(db, owner.Id);

        var act = async () => await projectService.GetProjectById(project.Id, otherUser.Id);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task DeleteProject_WhenUserIsOwner_DeletesProject()
    {
        var (db, projectService) = CreateProjectService();
        var user = CreateUser(db);
        var project = CreateProject(db, user.Id);

        await projectService.DeleteProject(project.Id, user.Id);

        db.Projects.Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteProject_WhenUserIsNotOwner_ThrowsKeyNotFoundException()
    {
        var (db, projectService) = CreateProjectService();
        var owner = CreateUser(db, "Owner", "owner@test.com");
        var otherUser = CreateUser(db, "Other", "other@test.com");
        var project = CreateProject(db, owner.Id);

        var act = async () => await projectService.DeleteProject(project.Id, otherUser.Id);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
