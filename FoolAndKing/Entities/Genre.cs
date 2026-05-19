using System;
using System.Collections.Generic;

namespace FoolAndKing.Entities;

public partial class Genre
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public virtual ICollection<Genrebook> Genrebooks { get; set; } = new List<Genrebook>();
}
