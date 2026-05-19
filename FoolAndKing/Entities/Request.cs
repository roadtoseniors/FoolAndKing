using System;
using System.Collections.Generic;

namespace FoolAndKing.Entities;

public partial class Request
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string? Description { get; set; }

    public int RoleId { get; set; }

    public virtual Role Role { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
