using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using FoolAndKing.Context;
using FoolAndKing.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace FoolAndKing.Pages;

public partial class CatalogBook : ContentPage
{
    private readonly MyDbContext _db;
    private readonly User _currentUser;
    private List<BookViewModel> _allBooks = new();
    private readonly HashSet<int> _selectedGenres = new();

    public CatalogBook() : this(new MyDbContext(), new User()) { }

    public CatalogBook(MyDbContext db, User user)
    {
        _db = db;
        _currentUser = user;
    
        InitializeComponent();
    
        LoadGenres();
        LoadBooks();
    }

    private void LoadGenres()
    {
        GenreFilter.ItemsSource = _db.Genres.ToList();
    }

    private void LoadBooks()
    {
        _allBooks = _db.Books
            .Include(b => b.Author)
            .Include(b => b.Feedbacks)
            .Include(b => b.Genrebooks).ThenInclude(gb => gb.Genre)
            .Where(b => !b.IsFrozen)
            .AsEnumerable()
            .Select(b => new BookViewModel(b))
            .ToList();

        ApplyFilters();
    }

    private void ApplyFilters()
    {
        if (_allBooks is null || BookGrid is null) return;

        var search = SearchBox?.Text?.Trim().ToLower() ?? "";
        var sort   = SortBox?.SelectedIndex ?? 0;

        var filtered = _allBooks.AsEnumerable();

        if (!string.IsNullOrEmpty(search))
            filtered = filtered.Where(b =>
                b.Name.ToLower().Contains(search) ||
                b.Author.Name.ToLower().Contains(search));

        if (_selectedGenres.Count > 0)
            filtered = filtered.Where(b =>
                b.Book.Genrebooks.Any(gb => _selectedGenres.Contains(gb.GenreId)));

        filtered = sort switch
        {
            0 => filtered.OrderBy(b => b.Name),
            1 => filtered.OrderByDescending(b => b.Name),
            2 => filtered.OrderByDescending(b => b.AverageScore),
            3 => filtered.OrderBy(b => b.AverageScore),
            _ => filtered
        };

        BookGrid.ItemsSource = filtered.ToList();
    }

    private void OnSearchChanged(object? sender, TextChangedEventArgs e)
        => ApplyFilters();

    private void OnSortChanged(object? sender, SelectionChangedEventArgs e)
        => ApplyFilters();

    private void OnGenreToggled(object? sender, RoutedEventArgs e)
    {
        if (sender is ToggleButton tb && tb.Tag is int genreId)
        {
            if (tb.IsChecked == true) _selectedGenres.Add(genreId);
            else _selectedGenres.Remove(genreId);
            ApplyFilters();
        }
    }

    private void OnOpenBookClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: BookViewModel vm })
            Navigation.PushAsync(new BookPage(_db, _currentUser, vm.Book));
    }

    private async void OnAddToListClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: BookViewModel vm }) return;

        var lists = _db.Readinglists.ToList();

        var dialog = new Window
        {
            Title = "Добавить в список",
            Width = 300,
            Height = 200,
            CanResize = false
        };

        var stack = new StackPanel { Margin = new Avalonia.Thickness(16), Spacing = 8 };
        stack.Children.Add(new TextBlock
        {
            Text = $"Добавить «{vm.Name}» в список:",
            TextWrapping = Avalonia.Media.TextWrapping.Wrap
        });

        foreach (var list in lists)
        {
            var btn = new Button
            {
                Content = list.Name,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                Tag = list.Id
            };
            btn.Click += (_, _) =>
            {
                var exists = _db.Userreadinglists.Any(u =>
                    u.UserId == _currentUser.Id &&
                    u.BookId == vm.Book.Id      &&
                    u.ReadingListId == list.Id);

                if (!exists)
                {
                    _db.Userreadinglists.Add(new Userreadinglist
                    {
                        UserId = _currentUser.Id,
                        BookId = vm.Book.Id,
                        ReadingListId = list.Id
                    });
                    _db.SaveChanges();
                }
                dialog.Close();
            };
            stack.Children.Add(btn);
        }

        dialog.Content = stack;
        await dialog.ShowDialog((Window)this.VisualRoot!);
    }
}

public class BookViewModel
{
    public Book Book { get; }
    public string Name => Book.Name;
    public string AuthorName => Book.Author.Name;
    public User   Author => Book.Author;
    public string? CoverPath => Book.CoverPath;
    public double AverageScore => Book.Feedbacks.Count > 0 ? Book.Feedbacks.Average(f => (double)f.Score) : 0;

    public BookViewModel(Book book) => Book = book;
}