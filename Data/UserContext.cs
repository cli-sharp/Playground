﻿namespace Playground.Data;

public class UserContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
}