using System;
using System.Collections.Generic;

namespace FoolAndKing.Entities;

public partial class Reason
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Claim> Claims { get; set; } = new List<Claim>();
}
