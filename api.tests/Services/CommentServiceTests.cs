using Xunit;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Api.Data;
using Api.Models;
using Api.Services;
using Api.Repositories;
using Api.Types;
using Moq;
using Microsoft.Extensions.Logging;

namespace Api.Tests.Services;

public class CommentServiceTests
{
    private AppDbContext CreateInMemoryDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private (AppDbContext db, CommentService commentService) CreateCommentService()
    {
        var db = CreateInMemoryDb();
        var mockProjectRepoLogger = new Mock<ILogger<ProjectRepository>>();
        var projectRepository = new ProjectRepository(db, mockProjectRepoLogger.Object);
        var commentRepository = new CommentRepository(db);
        var taskRepository = new TaskRepository(db);
        var mockLogger = new Mock<ILogger<CommentService>>();
        var commentService = new CommentService(commentRepository, projectRepository, taskRepository, mockLogger.Object);
        return (db, commentService);
    }

    private User CreateUser(AppDbContext db, string name = "Test User", string email = "test@test.com")
    {
        var user = new User { Name = name, Email = email, PasswordHash = "hash" };
        db.Users.Add(user);
        db.SaveChanges();
        return user;
    }

    private Project CreateProject(AppDbContext db, Guid ownerId)
    {
        var project = new Project { Title = "Test Project", OwnerId = ownerId };
        db.Projects.Add(project);
        db.SaveChanges();
        return project;
    }

    private Comment CreateComment(AppDbContext db, Guid authorId, Guid? projectId = null, Guid? taskId = null, string body = "Test comment")
    {
        var comment = new Comment
        {
            Body = body,
            AuthorId = authorId,
            ProjectId = projectId,
            TaskId = taskId
        };
        db.Comments.Add(comment);
        db.SaveChanges();
        return comment;
    }

    [Fact]
    public async Task UpdateComment_WhenUserIsAuthor_UpdatesComment()
    {
        var (db, commentService) = CreateCommentService();
        var user = CreateUser(db);
        var project = CreateProject(db, user.Id);
        var comment = CreateComment(db, user.Id, projectId: project.Id);

        var result = await commentService.UpdateComment(comment.Id, user.Id, new UpdateCommentRequest("Updated body"));

        result.Body.Should().Be("Updated body");
    }

    [Fact]
    public async Task UpdateComment_WhenUserIsNotAuthor_ThrowsKeyNotFoundException()
    {
        var (db, commentService) = CreateCommentService();
        var owner = CreateUser(db, "Owner", "owner@test.com");
        var otherUser = CreateUser(db, "Other", "other@test.com");
        var project = CreateProject(db, owner.Id);
        var comment = CreateComment(db, owner.Id, projectId: project.Id);

        var act = async () => await commentService.UpdateComment(comment.Id, otherUser.Id, new UpdateCommentRequest("Updated body"));

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task UpdateComment_WithEmptyBody_ThrowsArgumentException()
    {
        var (db, commentService) = CreateCommentService();
        var user = CreateUser(db);
        var project = CreateProject(db, user.Id);
        var comment = CreateComment(db, user.Id, projectId: project.Id);

        var act = async () => await commentService.UpdateComment(comment.Id, user.Id, new UpdateCommentRequest(""));

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Body is required");
    }

    [Fact]
    public async Task DeleteComment_WhenUserIsAuthor_DeletesComment()
    {
        var (db, commentService) = CreateCommentService();
        var user = CreateUser(db);
        var project = CreateProject(db, user.Id);
        var comment = CreateComment(db, user.Id, projectId: project.Id);

        await commentService.DeleteComment(comment.Id, user.Id);

        db.Comments.Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteComment_WhenUserIsNotAuthor_ThrowsKeyNotFoundException()
    {
        var (db, commentService) = CreateCommentService();
        var owner = CreateUser(db, "Owner", "owner@test.com");
        var otherUser = CreateUser(db, "Other", "other@test.com");
        var project = CreateProject(db, owner.Id);
        var comment = CreateComment(db, owner.Id, projectId: project.Id);

        var act = async () => await commentService.DeleteComment(comment.Id, otherUser.Id);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
