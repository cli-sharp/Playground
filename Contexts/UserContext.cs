using Microsoft.EntityFrameworkCore;
using Playground.Entities;

namespace Playground.Contexts;

public class UserContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
}