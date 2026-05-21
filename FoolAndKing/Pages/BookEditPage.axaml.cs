using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using FoolAndKing.Context;
using FoolAndKing.Entities;
using Microsoft.EntityFrameworkCore;

namespace FoolAndKing.Pages;

public partial class BookEditPage : ContentPage
{
    private readonly MyDbContext _db;
    private readonly User _currentUser;
    private readonly Book? _existingBook;
    private readonly Action? _onSaved;
    private readonly Action? _onBack;
    private List<GenreCheckItem> _genres = new();

    public BookEditPage() : this(new MyDbContext(), new User(), null, null, null) { }

    public BookEditPage(MyDbContext db, User user, Book? book, Action? onSaved, Action? onBack)
    {
        InitializeComponent();
        _db = db;
        _currentUser = user;
        _existingBook = book;
        _onSaved = onSaved;
        _onBack = onBack;

        PageTitle.Text = book is null ? "Добавить книгу" : "Редактировать книгу";

        LoadGenres();
        if (book is not null) FillFields(book);
    }

    private void LoadGenres()
    {
        var selectedIds = _existingBook is null ? new HashSet<int>()
            : _existingBook.Genrebooks.Select(gb => gb.GenreId).ToHashSet();

        _genres = _db.Genres.ToList().Select(g => new GenreCheckItem
        {
            Genre = g,
            IsSelected = selectedIds.Contains(g.Id)
        }).ToList();

        GenreCheckList.ItemsSource = _genres;
    }

    private void FillFields(Book book)
    {
        NameBox.Text = book.Name;
        DescriptionBox.Text = book.Description;
        CoverPathBox.Text = book.CoverPath;
        TextBox.Text = book.Text;
    }

    private void OnBackClick(object? sender, RoutedEventArgs e) => _onBack?.Invoke();

    private async void OnPickCoverClick(object? sender, RoutedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel is null) return;

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Выберите обложку",
            AllowMultiple = false,
            FileTypeFilter = new List<FilePickerFileType>
            {
                new FilePickerFileType("Изображения")
                {
                    Patterns = new List<string> { "*.png", "*.jpg", "*.jpeg", "*.webp" }
                }
            }
        });

        if (files is { Count: > 0 })
            CoverPathBox.Text = files[0].Path.LocalPath;
    }

    private void OnSaveClick(object? sender, RoutedEventArgs e)
    {
        var name = NameBox.Text?.Trim();
        if (string.IsNullOrEmpty(name))
        {
            ShowStatus("Введите название книги.", Brushes.Red);
            return;
        }

        var selectedGenreIds = _genres
            .Where(g => g.IsSelected)
            .Select(g => g.Genre.Id)
            .ToList();

        if (_existingBook is null)
            CreateBook(name, selectedGenreIds);
        else
            UpdateBook(name, selectedGenreIds);

        ShowStatus("Сохранено успешно!", Brushes.Green);
        _onSaved?.Invoke();

        Task.Delay(800).ContinueWith(_ =>
            Dispatcher.UIThread.Post(() => _onBack?.Invoke()));
    }

    private void CreateBook(string name, List<int> genreIds)
    {
        var book = new Book
        {
            Name = name,
            Description = DescriptionBox.Text?.Trim(),
            CoverPath = CoverPathBox.Text?.Trim(),
            Text = TextBox.Text?.Trim(),
            AuthorId = _currentUser.Id,
            IsFrozen = false
        };
        _db.Books.Add(book);
        _db.SaveChanges();

        foreach (var genreId in genreIds)
            _db.Genrebooks.Add(new Genrebook { BookId = book.Id, GenreId = genreId });
        _db.SaveChanges();
    }

    private void UpdateBook(string name, List<int> genreIds)
    {
        var book = _db.Books.Include(b => b.Genrebooks).First(b => b.Id == _existingBook!.Id);
        book.Name = name;
        book.Description = DescriptionBox.Text?.Trim();
        book.CoverPath = CoverPathBox.Text?.Trim();
        book.Text = TextBox.Text?.Trim();

        _db.Genrebooks.RemoveRange(book.Genrebooks);
        foreach (var genreId in genreIds)
            _db.Genrebooks.Add(new Genrebook { BookId = book.Id, GenreId = genreId });
        _db.SaveChanges();
    }

    private void ShowStatus(string msg, IBrush color)
    {
        StatusText.Text = msg;
        StatusText.Foreground = color;
        StatusText.IsVisible = true;
    }
}

public class GenreCheckItem
{
    public Genre Genre { get; init; } = null!;
    public bool IsSelected { get; set; }
}