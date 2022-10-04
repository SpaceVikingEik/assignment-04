using System.Xml.Linq;
namespace Assignment.Infrastructure;
using Assignment.Core;
using Assignment.Infrastructure;
public class TagRepository : ITagRepository
{
     private readonly KanbanContext _context;
        public TagRepository(KanbanContext context)
    {
        _context = context;
    }

    public (Response Response, int TagId) Create(TagCreateDTO tag)
    {
        var entity = _context.Tags.FirstOrDefault(t => t.Name == tag.Name);
        Response response;

        if (entity is null)
        {
            entity = new Tag{Id = 1, Name = tag.Name};

            _context.Tags.Add(entity);
            _context.SaveChanges();

            response = Response.Created;
        }
        else
        {
            response = Response.Conflict;
        }

        return (response, entity.Id);

    }

    public IReadOnlyCollection<TagDTO> Read()
    {
        var tags = from t in _context.Tags
                     orderby t.Name
                     select new TagDTO(t.Id, t.Name!);

        return tags.ToArray();
    }

    public TagDTO Find(int tagId)
    {
        var tags = from t in _context.Tags
                    where t.Id == tagId
                    select new TagDTO(t.Id, t.Name!);

        return tags.FirstOrDefault()!;
    }

    public Response Update(TagUpdateDTO tag)
    {
        var entity = _context.Tags.Find(tag.Id);
        Response response;

        if (entity is null)
        {
            response = Response.NotFound;
        }
        else if (_context.Tags.FirstOrDefault(t => t.Id != tag.Id && t.Name == tag.Name) != null)
        {
            response = Response.Conflict;
        }
        else
        {
            entity.Name = tag.Name;
            _context.SaveChanges();
            response = Response.Updated;
        }

        return response;

    }

    public Response Delete(int tagId, bool force = false)
    {
        var tag = (from t in _context.Tags
                    where t.Id == tagId
                    select t).FirstOrDefault();
        Response response;
        if (tag is null)
        {
            response = Response.NotFound;
        }
        else if (tag.WorkItems!.Any() && !force)
        {
            response = Response.Conflict;
        }
        else
        {
            _context.Tags.Remove(tag);
            _context.SaveChanges();

            response = Response.Deleted;
        }

        return response;
    }

}