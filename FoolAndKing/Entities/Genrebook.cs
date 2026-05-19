using System;
using System.Collections.Generic;

namespace FoolAndKing.Entities;

public partial class Genrebook
{
    public int Id { get; set; }

    public int GenreId { get; set; }

    public int BookId { get; set; }

    public virtual Book Book { get; set; } = null!;

    public virtual Genre Genre { get; set; } = null!;
}
