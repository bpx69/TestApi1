using Microsoft.EntityFrameworkCore;

namespace TestApi1.Model
{
    public class UserAPIDbContext : DbContext
    {
        public UserAPIDbContext(DbContextOptions<UserAPIDbContext> options)
            : base(options)
        {
        }
        public UserAPIDbContext() : base() {}

        public virtual DbSet<UserDbRecord> Users { get; set; }
        public virtual DbSet<ClientDbRecord> Clients { get; set; }

        public virtual void SetAsModified(UserDbRecord entity)
        {
            Entry(entity).State = EntityState.Modified;
        }
    }
}
