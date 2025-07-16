using BlazorAuthApp.Entities;
using Microsoft.EntityFrameworkCore;

namespace BlazorAuthApp.Data
{
    public class UserDbContext : DbContext
    {
        public UserDbContext(DbContextOptions<UserDbContext> options)
            : base(options)
        {
        }
        public DbSet<User> Users { get; set; }
    }
}
