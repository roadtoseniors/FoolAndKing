using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using FoolAndKing.Context;
using FoolAndKing.Entities;
using Microsoft.EntityFrameworkCore;

namespace FoolAndKing.Pages;

public partial class BookPage : ContentPage
{
    private readonly MyDbContext _db;
    private readonly User _currentUser;
    private Book _book;
    private readonly Action? _onBack;

    public BookPage() : this(new MyDbContext(), new User(), new Book(), null) { }

    public BookPage(MyDbContext db, User user, Book book, Action? onBack)
    {
        InitializeComponent();
        _db = db;
        _currentUser = user;
        _book = book;
        _onBack = onBack;
        LoadBook();
    }

    private void LoadBook()
    {
        _book = _db.Books
            .Include(b => b.Author)
            .Include(b => b.Feedbacks).ThenInclude(f => f.User)
            .Include(b => b.Genrebooks).ThenInclude(gb => gb.Genre)
            .First(b => b.Id == _book.Id);

        HeaderTitle.Text = _book.Name;
        BookTitle.Text = _book.Name;
        BookAuthor.Text = $"Автор: {_book.Author.Name}";
        BookGenres.Text = "Жанры: " + string.Join(", ", _book.Genrebooks.Select(gb => gb.Genre.Name));
        BookDescription.Text = _book.Description ?? "Описание отсутствует.";
        BookText.Text = _book.Text ?? "Текст книги недоступен.";

        var avg = _book.Feedbacks.Count > 0
            ? _book.Feedbacks.Average(f => (double)f.Score) : 0;
        BookScore.Text = _book.Feedbacks.Count > 0
            ? $"Рейтинг {avg:0.0} ({_book.Feedbacks.Count} отзывов)" : "Нет оценок";

        if (!string.IsNullOrEmpty(_book.CoverPath))
        {
            try { CoverImage.Source = new Bitmap(_book.CoverPath); }
            catch { /* файл не найден — просто не показываем */ }
        }

        BtnFreeze.IsVisible = _currentUser.RoleId == 3;
        if (_currentUser.RoleId == 3)
            BtnFreeze.Content = _book.IsFrozen ? "Разморозить книгу" : "Заморозить книгу";

        LoadFeedbacks();
    }

    private void LoadFeedbacks()
    {
        FeedbackList.ItemsSource = _book.Feedbacks
            .OrderByDescending(f => f.CreatedAt)
            .Select(f => new FeedbackViewModel(f, _currentUser.RoleId == 3))
            .ToList();
    }

    private void OnBackClick(object? sender, RoutedEventArgs e) => _onBack?.Invoke();

    private void OnFreezeBookClick(object? sender, RoutedEventArgs e)
    {
        var book = _db.Books.Find(_book.Id);
        if (book is null) return;
        book.IsFrozen = !book.IsFrozen;
        _db.SaveChanges();
        _book.IsFrozen = book.IsFrozen;
        BtnFreeze.Content = book.IsFrozen ? "Разморозить книгу" : "Заморозить книгу";
    }

    private void OnSubmitFeedbackClick(object? sender, RoutedEventArgs e)
    {
        var text = FeedbackText.Text?.Trim();
        if (string.IsNullOrEmpty(text)) return;

        var score = (byte)(ScoreBox.SelectedIndex + 1);

        _db.Feedbacks.Add(new Feedback
        {
            UserId = _currentUser.Id,
            BookId = _book.Id,
            Text = text,
            Score = score,
            CreatedAt = DateTime.Now
        });
        _db.SaveChanges();

        FeedbackText.Text = "";
        LoadBook();
    }

    private async void OnClaimBookClick(object? sender, RoutedEventArgs e)
        => await ShowClaimDialog(bookId: _book.Id, feedbackId: null);

    private async void OnClaimAuthorClick(object? sender, RoutedEventArgs e)
        => await ShowClaimDialog(bookId: _book.Id, feedbackId: null, isAuthorClaim: true);

    private async void OnClaimFeedbackClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: FeedbackViewModel vm })
            await ShowClaimDialog(bookId: null, feedbackId: vm.Feedback.Id);
    }

    private void OnFreezeFeedbackClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: FeedbackViewModel vm }) return;

        var fb = _db.Feedbacks
            .Include(f => f.Claims)
            .FirstOrDefault(f => f.Id == vm.Feedback.Id);

        if (fb is null) return;
        
        _db.Claims.RemoveRange(fb.Claims);
        _db.Feedbacks.Remove(fb);
        _db.SaveChanges();
        LoadBook();
    }

    private async System.Threading.Tasks.Task ShowClaimDialog(int? bookId, int? feedbackId, bool isAuthorClaim = false)
    {
        var reasons = _db.Reasons.ToList();

        var title = isAuthorClaim ? "Жалоба на автора" : feedbackId is not null ? "Жалоба на отзыв" : "Жалоба на книгу";

        var dialog = new Window { Title = title, Width = 360, Height = 280, CanResize = false };
        var stack = new StackPanel { Margin = new Avalonia.Thickness(16), Spacing = 8 };

        stack.Children.Add(new TextBlock { Text = "Выберите причину жалобы:" });

        var reasonBox = new ComboBox
        {
            ItemsSource = reasons,
            DisplayMemberBinding = new Avalonia.Data.Binding("Name"),
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            SelectedIndex = 0
        };
        stack.Children.Add(reasonBox);

        var descBox = new TextBox
        {
            Watermark = "Дополнительное описание (необязательно)",
            AcceptsReturn = true,
            Height = 80,
            TextWrapping = Avalonia.Media.TextWrapping.Wrap
        };
        stack.Children.Add(descBox);

        var sendBtn = new Button { Content = "Отправить жалобу", HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch };
        sendBtn.Click += (_, _) =>
        {
            if (reasonBox.SelectedItem is not Reason reason) return;
            _db.Claims.Add(new Claim
            {
                UserId = _currentUser.Id,
                BookId = bookId,
                FeedBackId = feedbackId,
                ReasonId = reason.Id,
                Description = isAuthorClaim
                    ? $"[AuthorId:{_book.AuthorId}] {descBox.Text?.Trim()}"
                    : descBox.Text?.Trim()
            });
            _db.SaveChanges();
            dialog.Close();
        };
        stack.Children.Add(sendBtn);

        dialog.Content = stack;
        var window = TopLevel.GetTopLevel(this) as Window;
        if (window is not null) await dialog.ShowDialog(window);
    }
}

public class FeedbackViewModel
{
    public Feedback Feedback { get; }
    public User User => Feedback.User;
    public string Text => Feedback.Text;
    public byte Score => Feedback.Score;
    public DateTime CreatedAt => Feedback.CreatedAt;
    public bool IsAdmin { get; }

    public FeedbackViewModel(Feedback feedback, bool isAdmin)
    {
        Feedback = feedback;
        IsAdmin = isAdmin;
    }
}