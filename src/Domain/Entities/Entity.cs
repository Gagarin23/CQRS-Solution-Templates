using System;
using MassTransit;

namespace Domain.Entities;

public class Entity
{
    /// <summary>
    /// Example for sequential id. https://andrewlock.net/generating-sortable-guids-using-newid/
    /// </summary>
    public Guid Id { get; set; } = NewId.NextGuid();
}
