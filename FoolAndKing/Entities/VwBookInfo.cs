using System;
using System.Collections.Generic;

namespace FoolAndKing.Entities;

public partial class VwBookInfo
{
    public int BookId { get; set; }

    public string BookName { get; set; } = null!;

    public string? Description { get; set; }

    public string? CoverPath { get; set; }

    public bool IsFrozen { get; set; }

    public int AuthorId { get; set; }

    public string AuthorName { get; set; } = null!;

    public int? FeedbackCount { get; set; }

    public decimal? AvgScore { get; set; }
}
