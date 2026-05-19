using System;
using System.Collections.Generic;

namespace FoolAndKing.Entities;

public partial class Userreadinglist
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int BookId { get; set; }

    public int ReadingListId { get; set; }

    public virtual Book Book { get; set; } = null!;

    public virtual Readinglist ReadingList { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
