using System;
using System.Collections.Generic;

namespace FoolAndKing.Entities;

public partial class User
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public int RoleId { get; set; }

    public string Login { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string Email { get; set; } = null!;

    public bool IsFrozen { get; set; }

    public virtual ICollection<Book> Books { get; set; } = new List<Book>();

    public virtual ICollection<Claim> Claims { get; set; } = new List<Claim>();

    public virtual ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();

    public virtual ICollection<Requestfrozen> Requestfrozens { get; set; } = new List<Requestfrozen>();

    public virtual ICollection<Request> Requests { get; set; } = new List<Request>();

    public virtual Role Role { get; set; } = null!;

    public virtual ICollection<Userreadinglist> Userreadinglists { get; set; } = new List<Userreadinglist>();
}
