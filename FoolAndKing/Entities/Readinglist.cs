using System;
using System.Collections.Generic;

namespace FoolAndKing.Entities;

public partial class Readinglist
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Userreadinglist> Userreadinglists { get; set; } = new List<Userreadinglist>();
}
