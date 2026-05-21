using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using FoolAndKing.Context;
using FoolAndKing.Entities;
using Microsoft.EntityFrameworkCore;

namespace FoolAndKing.Pages;

public partial class AutorPage : ContentPage
{
    private readonly MyDbContext _db;
    private readonly User _currentUser;
    private readonly Action<ContentPage>? _navigate;

    public AutorPage() : this(new MyDbContext(), new User(), null) { }

    public AutorPage(MyDbContext db, User user, Action<ContentPage>? navigate)
    {
        InitializeComponent();
        _db = db;
        _currentUser = user;
        _navigate = navigate;
        LoadBooks();
    }

    public void Refresh()
    {
        LoadBooks();
    }

    private void LoadBooks()
    {
        var books = _db.Books
            .Include(b => b.Feedbacks)
            .Include(b => b.Genrebooks).ThenInclude(gb => gb.Genre)
            .Where(b => b.AuthorId == _currentUser.Id)
            .AsEnumerable()
            .Select(b => new AuthorBookModel(b))
            .ToList();

        var active = books.Where(b => !b.Book.IsFrozen).ToList();
        var frozen = books.Where(b => b.Book.IsFrozen).ToList();

        NoBooksText.IsVisible = active.Count == 0;
        ActiveBooksList.IsVisible = active.Count > 0;
        ActiveBooksList.ItemsSource = active;

        NoFrozenText.IsVisible = frozen.Count == 0;
        FrozenBooksList.IsVisible = frozen.Count > 0;
        FrozenBooksList.ItemsSource = frozen;
    }

    private void GoTo(ContentPage page) => _navigate?.Invoke(page);
    private void GoBack() => GoTo(this);

    private void OnAddBookClick(object? sender, RoutedEventArgs e)
        => GoTo(new BookEditPage(_db, _currentUser, null, OnBookSaved, GoBack));

    private void OnEditBookClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: AuthorBookModel vm })
            GoTo(new BookEditPage(_db, _currentUser, vm.Book, OnBookSaved, GoBack));
    }

    private void OnBookSaved() => LoadBooks();

    private async void OnAppealFrozenClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: AuthorBookModel vm }) return;

        var window = TopLevel.GetTopLevel(this) as Window;
        if (window is null) return;

        var alreadySent = _db.Requestfrozens.Any(r =>
            r.UserId == _currentUser.Id &&
            r.Description != null &&
            r.Description.Contains($"[BookId:{vm.Book.Id}]"));

        var dialog = new Window { Title = "Оспорить заморозку", Width = 380, Height = 240, CanResize = false };
        var stack = new StackPanel { Margin = new Avalonia.Thickness(16), Spacing = 10 };

        if (alreadySent)
        {
            stack.Children.Add(new TextBlock
            {
                Text = $"Заявка на оспаривание книги «{vm.Name}» уже отправлена.\nОжидайте решения администратора.",
                TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                Foreground = Brushes.Gray
            });
            var closeBtn = new Button { Content = "Закрыть" };
            closeBtn.Click += (_, _) => dialog.Close();
            stack.Children.Add(closeBtn);
        }
        else
        {
            stack.Children.Add(new TextBlock
            {
                Text = $"Оспорить заморозку книги «{vm.Name}»:",
                TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                FontWeight = FontWeight.Medium
            });

            var descBox = new TextBox
            {
                Watermark = "Опишите причину оспаривания...",
                AcceptsReturn = true,
                Height = 80,
                TextWrapping = Avalonia.Media.TextWrapping.Wrap
            };
            stack.Children.Add(descBox);

            var statusText = new TextBlock { IsVisible = false, FontSize = 12 };
            stack.Children.Add(statusText);

            var sendBtn = new Button { Content = "Отправить заявку" };
            sendBtn.Click += (_, _) =>
            {
                var text = descBox.Text?.Trim();
                if (string.IsNullOrEmpty(text))
                {
                    statusText.Text = "Заполните описание.";
                    statusText.Foreground = Brushes.Red;
                    statusText.IsVisible = true;
                    return;
                }
                _db.Requestfrozens.Add(new Requestfrozen
                {
                    UserId = _currentUser.Id,
                    Description = $"[BookId:{vm.Book.Id}] {text}"
                });
                _db.SaveChanges();
                dialog.Close();
            };
            stack.Children.Add(sendBtn);
        }

        dialog.Content = stack;
        await dialog.ShowDialog(window);
    }
}

public class AuthorBookModel
{
    public Book Book { get; }
    public string Name => Book.Name;
    public string? CoverPath => Book.CoverPath;
    public string GenresText => string.Join(", ", Book.Genrebooks.Select(gb => gb.Genre.Name));
    public double AverageScore => Book.Feedbacks.Count > 0
        ? Book.Feedbacks.Average(f => (double)f.Score) : 0;

    public AuthorBookModel(Book book) => Book = book;
}