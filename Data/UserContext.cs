namespace Playground.Data;

using Microsoft.EntityFrameworkCore;
using Playground.Entities;

public class UserContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
}