using System;
using System.Collections.Generic;

namespace FoolAndKing.Entities;

public partial class Claim
{
    public int Id { get; set; }

    public int? BookId { get; set; }

    public int UserId { get; set; }

    public int? FeedBackId { get; set; }

    public int ReasonId { get; set; }

    public string? Description { get; set; }

    public virtual Book? Book { get; set; }

    public virtual Feedback? FeedBack { get; set; }

    public virtual Reason Reason { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
