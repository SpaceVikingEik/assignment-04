namespace Assignment.Infrastructure.Tests;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Assignment.Core;

using Assignment.Infrastructure;

public class UserRepositoryTest
{
    private readonly KanbanContext _context;
    private readonly UserRepository _repository;
    public UserRepositoryTest()
    {
        var connection = new SqliteConnection("Filename=:memory:");
        connection.Open();
        var builder = new DbContextOptionsBuilder<KanbanContext>();
        builder.UseSqlite(connection);
        var context = new KanbanContext(builder.Options);
        context.Database.EnsureCreated();
        var tasks1 = new WorkItem { Id = 1, Title = "HewwoTwere", AssignedTo = null, State = State.New, Tags = new List<Tag> { } };
        var tag1 = new Tag { Id = 1, Name = "HelloThere", WorkItems = new List<Assignment.Infrastructure.WorkItem> { } };
        var user1 = new User { Id = 1, Name = "ObiWan", Email = "DankRepublic@corusant.com", WorkItems = new List<Assignment.Infrastructure.WorkItem> { } };
        tasks1.Tags.Add(tag1);
        user1.WorkItems.Add(tasks1);
        tag1.WorkItems.Add(tasks1);

        context.Tags.Add(tag1);
        context.WorkItems.Add(tasks1);
        context.Users.Add(user1);
        context.SaveChanges();

        _context = context;
        _repository = new UserRepository(_context);
    }

    [Fact]
    public void Existing_User_Can_Be_Deleted_Using_The_Force()
    {
        var response = _repository.Delete(1, true);

        response.Should().Be(Response.Deleted);

        var entity = _context.Users.Find(1);

        entity.Should().BeNull();
    }

    [Fact]
    public void Existing_User_Can_Not_Be_Deleted_Without_Using_The_Force()
    {
        var response = _repository.Delete(1, false);

        response.Should().Be(Response.Conflict);

        var entity = _context.Users.Find(1);

        entity.Should().NotBeNull();
    }

    [Fact]
    public void User_Which_Exists_Cannot_Be_Created()
    {
        var (response, id) = _repository.Create(new UserCreateDTO("ObiWan", "DankRepublic@corusant.com"));

        response.Should().Be(Response.Conflict);
    }

}