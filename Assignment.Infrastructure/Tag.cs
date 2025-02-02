namespace Assignment.Infrastructure;

public class Tag
{
    public int Id { get; set; }
    
    [StringLength(50), Required]
    public string Name { get; set; }
    public ICollection<WorkItem> WorkItems { get; set; }
}
