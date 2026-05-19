using System;
using System.Collections.Generic;

namespace FoolAndKing.Entities;

public partial class VwUserInfo
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Login { get; set; } = null!;

    public string Email { get; set; } = null!;

    public bool IsFrozen { get; set; }

    public string RoleName { get; set; } = null!;
}
