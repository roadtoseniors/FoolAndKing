using System;
using System.Collections.Generic;

namespace FoolAndKing.Entities;

public partial class Requestfrozen
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string? Description { get; set; }

    public virtual User User { get; set; } = null!;
}
