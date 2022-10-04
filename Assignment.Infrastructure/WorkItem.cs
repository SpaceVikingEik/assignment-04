namespace Assignment.Infrastructure;

public class WorkItem
{
    public int Id { get; set; }
 [StringLength(100)]
    public string Title { get; set; }

    public DateTime Created { get; set; }

     public DateTime StateUpdated { get; set; }

    public int? AssignedToId { get; set; }

    public User? AssignedTo { get; set; }

        [StringLength(int.MaxValue)]
    public string? Description { get; set; }

    public State State { get; set; }

    public ICollection<Tag> Tags { get; set; }

    
}
