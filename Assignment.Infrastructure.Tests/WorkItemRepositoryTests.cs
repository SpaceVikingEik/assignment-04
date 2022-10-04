namespace Assignment.Infrastructure.Tests;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Assignment.Core;

public class WorkItemRepositoryTests
{
    private readonly KanbanContext _context;
    private readonly WorkItemRepository _repository;
    public WorkItemRepositoryTests()
    {
        var connection = new SqliteConnection("Filename=:memory:");
        connection.Open();
        var builder = new DbContextOptionsBuilder<KanbanContext>();
        builder.UseSqlite(connection);
        var context = new KanbanContext(builder.Options);
        context.Database.EnsureCreated();
        var user1 = new User { Id = 1, Name = "ObiWan", Email = "DankRepublic@corusant.com", WorkItems = new List<Assignment.Infrastructure.WorkItem> { } };
        var WorkItems1 = new WorkItem { Id = 1, Title = "HewwoTwere", AssignedTo = user1, Description = "uwu the owo", Created = DateTime.Now, State = State.New, Tags = new List<Tag> { } , StateUpdated = DateTime.Now};
        var WorkItems2 = new WorkItem { Id = 2, Title = "Hi", AssignedTo = user1, Description = "hello there", Created = DateTime.Now,  State = State.Active, Tags = new List<Tag> { } , StateUpdated = DateTime.Now};
        var tag1 = new Tag { Id = 1, Name = "HelloThere", WorkItems = new List<Assignment.Infrastructure.WorkItem> { } };
        WorkItems1.Tags.Add(tag1);
        user1.WorkItems.Add(WorkItems1);
        tag1.WorkItems.Add(WorkItems1);

        context.Tags.Add(tag1);
        context.WorkItems.Add(WorkItems1);
        context.WorkItems.Add(WorkItems2);
        context.Users.Add(user1);
        context.SaveChanges();

        _context = context;
        _repository = new WorkItemRepository(_context);
    }

    [Fact]
    public void delete_on_non_existing_entity_returns_NotFound()
    {
        //Arrange
        var expectedRepsonse = Response.NotFound;
        //Act
        var actualResponse = _repository.Delete(254);
        //Assert
        actualResponse.Should().Be(expectedRepsonse);
    }

    [Fact]
    public void update_on_non_existing_entity_returns_NotFound()
    {
        //Arrange
        var expectedRepsonse = Response.NotFound;
        //Act
        var actualResponse = _repository.Update(new WorkItemUpdateDTO(234, "Laundry", 235, "Do the laundry", new List<string>(), State.New));
        //Assert
        actualResponse.Should().Be(expectedRepsonse);
    }

    [Fact]
    public void correctly_deleting_WorkItem_returns_Deleted() 
    {
        //Arrange
        var expectedRepsonse = Response.Deleted;
        //Act
        var actualResponse = _repository.Delete(1);
        //Assert
        actualResponse.Should().Be(expectedRepsonse);
    }

    [Fact]
    public void correctly_updating_WorkItem_returns_Updated()
    {
        //Arrange
        var expectedRepsonse = Response.Updated;
        //Act
        var actualResponse = _repository.Update(new WorkItemUpdateDTO(1, "Laundry", 1, "Do the laundry", new List<string>(), State.New));
        //Assert
        actualResponse.Should().Be(expectedRepsonse);
    }

    [Fact]
    public void deleting_Active_WorkItem_updates_state_to_removed() 
    {
        //Arrange
        var expectedState = State.Removed;
        //Act
        _repository.Delete(2);
        //Assert
        _repository.Find(2).State.Should().Be(expectedState);
    }

    [Fact]
    public void creating_WorkItem_sets_state_to_new()
    {
        //Arrange
        var expectedState = State.New;
        //Act
        _repository.Create(new WorkItemCreateDTO("Laundry", 1, "do the laundry!", new List<string>()));
        //Assert
        _repository.Find(3).State.Should().Be(expectedState);
    }

    [Fact]
    public void creating_WorkItem_sets_correct_times()
    {
        //Arrange
        _repository.Create(new WorkItemCreateDTO("Laundry", 1, "do the laundry!", new List<string>()));
        var actualTime1 = _repository.Find(2).Created;
        var actualTime2 = _repository.Find(2).StateUpdated;
        var expectedTime = DateTime.Now;

        //Assert
        actualTime1.Should().BeCloseTo(expectedTime, precision: TimeSpan.FromSeconds(5));
        actualTime2.Should().BeCloseTo(expectedTime, precision: TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void updating_tags_in_WorkItem()
    {
        //Arrange
        var expectedTags = new List<string>(){"HelloThere"};
        //Act
        _repository.Update(new WorkItemUpdateDTO(1, "Laundry", 1, "Do the laundry", new List<string>(){"HelloThere"}, State.Resolved));
        //Assert
        _repository.Find(1).Tags.Should().BeEquivalentTo(expectedTags);
    }

    [Fact]
    public void updating_state_of_WorkItem_updates_time()
    {
        //Arrange
        _repository.Update(new WorkItemUpdateDTO(1, "Laundry", 1, "Do the laundry", new List<string>(){"HelloThere"}, State.Resolved));
        DateTime expectedTime = DateTime.Now;
        //Act
        DateTime actualTime = _repository.Find(1).StateUpdated;
        //Assert
        actualTime.Should().BeCloseTo(expectedTime, precision: TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void assigning_non_existing_user_returns_bad_request() 
    {
        //Arrange
        var expectedRepsonse = Response.BadRequest;
        //Act
        var actualResponse = _repository.Update(new WorkItemUpdateDTO(1, "Laundry", 2, "Do the laundry", new List<string>(){"HelloThere"}, State.Resolved));
        //Assert
        actualResponse.Should().Be(expectedRepsonse);
    }

    [Fact]
    public void read_returns_null_when_searching_for_nonexisting_WorkItem()
    {
        //Act
        WorkItemDetailsDTO Actual = _repository.Find(25);
        //Assert
        Actual.Should().BeNull();
    }

    [Fact]
    public void read_all_returns_correct_elements() 
    {
        //Arrange
        var expectedLength = 2;
        //Act
        var actualList = _repository.Read();
        //Assert
        actualList.Count().Should().Be(expectedLength);
    }

    [Fact]
    public void read_all_removed_returns_correct_output()
    {
        //Arrange
        var expectedLength = 1;
        //Act
        _repository.Delete(2);
        var actualList = _repository.ReadRemoved();
        //Assert
        actualList.Count().Should().Be(expectedLength);
    }
}