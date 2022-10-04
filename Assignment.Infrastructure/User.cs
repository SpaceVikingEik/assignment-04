namespace Assignment.Infrastructure;

public class User
{
    public int Id { get; set; }
    public string Name { get; set; }

    [EmailAddress]
    public string Email { get; set; }

    public ICollection<WorkItem> WorkItems { get; set; }
}
