﻿namespace Playground.Entities;

public record User
{
    [Key]
    public required string Name { get; set; }

    public required string LastName { get; set; }

    public string? Mail { get; set; }
}