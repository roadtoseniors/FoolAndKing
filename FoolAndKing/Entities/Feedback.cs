using System;
using System.Collections.Generic;

namespace FoolAndKing.Entities;

public partial class Feedback
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string Text { get; set; } = null!;

    public byte Score { get; set; }

    public int BookId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Book Book { get; set; } = null!;

    public virtual ICollection<Claim> Claims { get; set; } = new List<Claim>();

    public virtual User User { get; set; } = null!;
}
