using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using FoolAndKing.Context;
using FoolAndKing.Entities;
using Microsoft.EntityFrameworkCore;

namespace FoolAndKing.Pages;

public partial class BookList : ContentPage
{
    private readonly MyDbContext _db;
    private readonly User _currentUser;
    private List<Readinglist> _readinglists = new();
    private int _currentListId;
    private readonly HashSet<int> _selectedGenres = new();
    private List<BookListItemViewModel> _currentBooks = new();
    private bool _isLoaded;

    public BookList() : this(new MyDbContext(), new User()) { }

    public BookList(MyDbContext db, User user)
    {
        _db = db;
        _currentUser = user;
        InitializeComponent();
        _isLoaded = true;
        LoadGenres();
        LoadReadinglists();
    }

    private void LoadReadinglists()
    {
        _readinglists = _db.Readinglists.ToList();
        if (_readinglists.Count == 0) return;

        ListTabsPanel.Children.Clear();
        foreach (var list in _readinglists)
        {
            var btn = new ToggleButton
            {
                Content = list.Name,
                Tag = list.Id,
                Padding = new Avalonia.Thickness(14, 6),
                CornerRadius = new Avalonia.CornerRadius(16)
            };
            btn.IsCheckedChanged += OnListTabChanged;
            ListTabsPanel.Children.Add(btn);
        }

        _currentListId = _readinglists[0].Id;
        if (ListTabsPanel.Children[0] is ToggleButton first)
            first.IsChecked = true;
        else
            LoadBooks(); // на случай если IsChecked не сработал
    }

    private void OnListTabChanged(object? sender, RoutedEventArgs e)
    {
        if (sender is not ToggleButton { IsChecked: true, Tag: int listId }) return;

        foreach (var child in ListTabsPanel.Children)
            if (child is ToggleButton tb && tb != sender)
                tb.IsChecked = false;

        _currentListId = listId;
        LoadBooks();
    }

    private void LoadGenres()
    {
        GenreFilter.ItemsSource = _db.Genres.ToList();
    }

    private void OnGenreToggled(object? sender, RoutedEventArgs e)
    {
        if (sender is ToggleButton { Tag: int genreId } tb)
        {
            if (tb.IsChecked == true) _selectedGenres.Add(genreId);
            else _selectedGenres.Remove(genreId);
            ApplyFilters();
        }
    }

    private void LoadBooks()
    {
        _currentBooks = _db.Userreadinglists
            .Include(u => u.Book).ThenInclude(b => b.Author)
            .Include(u => u.Book).ThenInclude(b => b.Feedbacks)
            .Include(u => u.Book).ThenInclude(b => b.Genrebooks).ThenInclude(gb => gb.Genre)
            .Where(u => u.UserId == _currentUser.Id && u.ReadingListId == _currentListId)
            .AsEnumerable()
            .Select(u => new BookListItemViewModel(u, _readinglists))
            .ToList();

        ApplyFilters();
    }

    private void ApplyFilters()
    {
        if (BookGrid is null) return;

        var search = SearchBox?.Text?.Trim().ToLower() ?? "";
        var sort = SortBox?.SelectedIndex ?? 0;
        var filtered = _currentBooks.AsEnumerable();

        if (!string.IsNullOrEmpty(search))
            filtered = filtered.Where(b =>
                b.BookName.ToLower().Contains(search) ||
                b.AuthorName.ToLower().Contains(search));

        if (_selectedGenres.Count > 0)
            filtered = filtered.Where(b =>
                b.Entry.Book.Genrebooks.Any(gb => _selectedGenres.Contains(gb.GenreId)));

        filtered = sort switch
        {
            0 => filtered.OrderBy(b => b.BookName),
            1 => filtered.OrderByDescending(b => b.BookName),
            2 => filtered.OrderByDescending(b => b.AverageScore),
            3 => filtered.OrderBy(b => b.AverageScore),
            _ => filtered
        };

        var list = filtered.ToList();
        BookGrid.ItemsSource = list;
        EmptyText.IsVisible = list.Count == 0;
    }

    private void OnSearchChanged(object? sender, TextChangedEventArgs e)
    {
        if (!_isLoaded) return;
        ApplyFilters();
    }

    private void OnSortChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (!_isLoaded) return;
        ApplyFilters();
    }

    private void OnMoveToListChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (!_isLoaded) return;
        if (sender is not ComboBox { Tag: BookListItemViewModel vm,
            SelectedItem: Readinglist targetList }) return;
        if (targetList.Id == _currentListId) return;

        var entry = _db.Userreadinglists.Find(vm.Entry.Id);
        if (entry is null) return;

        var exists = _db.Userreadinglists.Any(u =>
            u.UserId == _currentUser.Id &&
            u.BookId == vm.Entry.BookId &&
            u.ReadingListId == targetList.Id);

        if (exists) _db.Userreadinglists.Remove(entry);
        else entry.ReadingListId = targetList.Id;

        _db.SaveChanges();
        LoadBooks();
    }
}

public class BookListItemViewModel
{
    public Userreadinglist Entry { get; }
    public string BookName => Entry.Book.Name;
    public string AuthorName => Entry.Book.Author.Name;
    public string? CoverPath => Entry.Book.CoverPath;
    public double AverageScore => Entry.Book.Feedbacks.Count > 0
        ? Entry.Book.Feedbacks.Average(f => (double)f.Score) : 0;
    public string AverageScoreText => $"Рейтинг {AverageScore:0.0}";
    public List<Readinglist> OtherLists { get; }

    public BookListItemViewModel(Userreadinglist entry, List<Readinglist> allLists)
    {
        Entry = entry;
        OtherLists = allLists.ToList();
    }
}