namespace Assignment.Infrastructure;
using System.Collections.ObjectModel;

public class WorkItemRepository : IWorkItemRepository
{
    private readonly KanbanContext _context;
    public WorkItemRepository(KanbanContext context)
    {
        _context = context;
    }

    public (Response Response, int ItemId) Create(WorkItemCreateDTO WorkItem)
    {
        Response response;
        int WorkItemId;
        ICollection<Tag> tagsList = new List<Tag>();
        foreach (var s in WorkItem.Tags)
        {
            var tagsQuery = _context.Tags.Where(t => t.Name == s).Select(t => t);
            foreach (var t in tagsQuery)
            {
                tagsList.Add(t);
            }
        }
        WorkItem entity = new WorkItem
        {
            Title = WorkItem.Title,
            AssignedTo = _context.Users.Find(WorkItem.AssignedToId),
            Description = WorkItem.Description,
            Created = DateTime.Now,
            State = State.New,
            Tags = tagsList,
            StateUpdated = DateTime.Now
        };
        if (entity.AssignedTo == null)
        {
            return (Response.BadRequest, entity.Id);
        }
        _context.WorkItems.Add(entity);
        _context.SaveChanges();
        response = Response.Created;
        return (response, entity.Id);
    }
    public IReadOnlyCollection<WorkItemDTO> Read()
    {
        var WorkItemCollection =    from t in _context.WorkItems
                                select new WorkItemDTO(t.Id, t.Title, t.AssignedTo.Name, t.Tags.Select(x => new string(x.Name)).ToArray(), t.State);
        return new ReadOnlyCollection<WorkItemDTO>(WorkItemCollection.ToList());
    }
    public IReadOnlyCollection<WorkItemDTO> ReadRemoved()
    {
        var WorkItemCollection =    from t in _context.WorkItems
                                where t.State == State.Removed
                                select new WorkItemDTO(t.Id, t.Title, t.AssignedTo.Name, t.Tags.Select(x => new string(x.Name)).ToArray(), t.State);
        return new ReadOnlyCollection<WorkItemDTO>(WorkItemCollection.ToList());
    }
    public IReadOnlyCollection<WorkItemDTO> ReadByTag(string tag)
    {
        throw new NotImplementedException();
    }
    public IReadOnlyCollection<WorkItemDTO> ReadByUser(int userId)
    {
        var WorkItemCollection =    from t in _context.WorkItems
                                where t.AssignedTo.Id == userId
                                select new WorkItemDTO(t.Id, t.Title, t.AssignedTo.Name, t.Tags.Select(x => new string(x.Name)).ToArray(), t.State);
        return new ReadOnlyCollection<WorkItemDTO>(WorkItemCollection.ToList());
    }
    public IReadOnlyCollection<WorkItemDTO> ReadByState(State state)
    {
        var WorkItemCollection =    from t in _context.WorkItems
                                where t.State == state
                                select new WorkItemDTO(t.Id, t.Title, t.AssignedTo.Name, t.Tags.Select(x => new string(x.Name)).ToArray(), t.State);
        return new ReadOnlyCollection<WorkItemDTO>(WorkItemCollection.ToList());
    }
    public WorkItemDetailsDTO Find(int WorkItemId)
    {
        var WorkItems = from t in _context.WorkItems
                    where t.Id == WorkItemId
                    select new WorkItemDetailsDTO(t.Id, t.Title, t.Description, t.Created, t.AssignedTo.Name, t.Tags.Select(x => new string(x.Name)).ToArray(), t.State, t.StateUpdated);

        return WorkItems.FirstOrDefault();
    }
    public Response Update(WorkItemUpdateDTO WorkItem)
    {
        var entity = _context.WorkItems.Find(WorkItem.Id);
        Response response;

        if (entity is null)
        {
            response = Response.NotFound;
        }
        else if (_context.WorkItems.FirstOrDefault(t => t.Id != entity.Id && t.Title == entity.Title) != null)
        {
            response = Response.Conflict;
        }
        else
        {
            entity.Title = WorkItem.Title;
            entity.AssignedTo = _context.Users.Find(WorkItem.AssignedToId);
            entity.Description = WorkItem.Description;
            ICollection<Tag> tagsList = new List<Tag>();
            foreach (var s in WorkItem.Tags)
            {
                var tagsQuery = _context.Tags.Where(t => t.Name == s).Select(t => t);
                foreach (var t in tagsQuery)
                {
                    tagsList.Add(t);
                }
            }
            entity.Tags = tagsList;
            if (entity.State != WorkItem.State)
            {
                entity.StateUpdated = DateTime.Now;
                entity.State = WorkItem.State;
            }
            if (entity.AssignedTo == null)
            {
                return Response.BadRequest;
            }
            _context.SaveChanges();
            response = Response.Updated;
        }

        return response;
    }
    public Response Delete(int WorkItemId)
    {
        var WorkItem = _context.WorkItems.Include(t => t.Tags).FirstOrDefault(t => t.Id == WorkItemId);
        Response response;

        if (WorkItem is null)
        {
            response = Response.NotFound;
        }
        else if (WorkItem.State == State.Resolved || WorkItem.State == State.Closed || WorkItem.State == State.Removed)
        {
            response = Response.Conflict;
        }
        else
        {
            if (WorkItem.State == State.Active)
            {
                WorkItem.State = State.Removed;
            }
            else
            {
                _context.WorkItems.Remove(WorkItem);
            }
            _context.SaveChanges();

            response = Response.Deleted;
        }

        return response;
    }
}