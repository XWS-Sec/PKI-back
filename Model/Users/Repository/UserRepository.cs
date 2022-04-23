using Model.Shared;

namespace Model.Users.Repository
{
    public class UserRepository : BaseRepository<User, string>, IUserRepository
    {
        public UserRepository(AppDbContext context) : base(context)
        {
        }
    }
}