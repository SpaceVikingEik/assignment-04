using System.Xml.Linq;
namespace Assignment.Infrastructure;
using Assignment.Core;
using Assignment.Infrastructure;
public class UserRepository : IUserRepository
{
     private readonly KanbanContext _context;
        public UserRepository(KanbanContext context)
    {
        _context = context;
    }

    public (Response Response, int UserId) Create(UserCreateDTO user)
    {
        var entity = _context.Users.FirstOrDefault(u => u.Email == user.Email);
        Response response;

        if (entity is null)
        {
            entity = new User{Id = 1, Email = user.Email};

            _context.Users.Add(entity);
            _context.SaveChanges();

            response = Response.Created;
        }
        else
        {
            response = Response.Conflict;
        }

        return (response, entity.Id);

    }

    public IReadOnlyCollection<UserDTO> Read()
    {
        var tags = from u in _context.Users
                     orderby u.Name
                     select new UserDTO(u.Id, u.Name!, u.Email);

        return tags.ToArray();
    }

    public UserDTO Find(int userId)
    {
        var tags = from u in _context.Users
                    where u.Id == userId
                    select new UserDTO(u.Id, u.Name!, u.Email);

        return tags.FirstOrDefault()!;
    }

    public Response Update(UserUpdateDTO user)
    {
        var entity = _context.Users.Find(user.Id);
        Response response;

        if (entity is null)
        {
            response = Response.NotFound;
        }
        else if (_context.Users.FirstOrDefault(u => u.Id != user.Id && u.Email == user.Email) != null)
        {
            response = Response.Conflict;
        }
        else
        {
            entity.Name = user.Name;
            _context.SaveChanges();
            response = Response.Updated;
        }

        return response;

    }

    public Response Delete(int userId, bool force = false)
    {
        var user = (from u in _context.Users
                    where u.Id == userId
                    select u).FirstOrDefault();
        Response response;
        if (user is null)
        {
            response = Response.NotFound;
        }
        else if (user.WorkItems!.Any() && !force)
        {
            response = Response.Conflict;
        }
        else
        {
            _context.Users.Remove(user);
            _context.SaveChanges();

            response = Response.Deleted;
        }

        return response;
    }

}
