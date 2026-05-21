using System.Linq;
using Avalonia.Controls;
using FoolAndKing.Context;
using FoolAndKing.Entities;

namespace FoolAndKing.Pages;

public partial class FrozenPage : ContentPage
{
    public FrozenPage() : this(new MyDbContext(), new User()) { }

    public FrozenPage(MyDbContext db, User user)
    {
        InitializeComponent();
        var lastRequest = db.Requestfrozens
            .Where(r => r.UserId == user.Id)
            .OrderByDescending(r => r.Id)
            .FirstOrDefault();

        if (lastRequest?.Description != null)
            ReasonText.Text = lastRequest.Description;
    }
}