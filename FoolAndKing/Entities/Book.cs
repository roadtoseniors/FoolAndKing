using System;
using System.Collections.Generic;

namespace FoolAndKing.Entities;

public partial class Book
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public string? CoverPath { get; set; }

    public string? Text { get; set; }

    public int AuthorId { get; set; }

    public bool IsFrozen { get; set; }

    public virtual User Author { get; set; } = null!;

    public virtual ICollection<Claim> Claims { get; set; } = new List<Claim>();

    public virtual ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();

    public virtual ICollection<Genrebook> Genrebooks { get; set; } = new List<Genrebook>();

    public virtual ICollection<Userreadinglist> Userreadinglists { get; set; } = new List<Userreadinglist>();
}
