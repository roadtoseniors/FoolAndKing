using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using FoolAndKing.Context;
using FoolAndKing.Entities;
using Microsoft.EntityFrameworkCore;

namespace FoolAndKing.Pages;

public partial class UserPage : ContentPage
{
    private readonly MyDbContext _db;
    private readonly User _currentUser;

    public UserPage() : this(new MyDbContext(), new User()) { }

    public UserPage(MyDbContext db, User user)
    {
        InitializeComponent();
        _db          = db;
        _currentUser = user;

        LoadProfile();
        LoadFeedbacks();
        SetupFrozenBanner();
        SetupAuthorRequest();
    }

    private void LoadProfile()
    {
        LblName.Text = _currentUser.Name;
        LblLogin.Text = _currentUser.Login;
        LblEmail.Text = _currentUser.Email;
        LblRole.Text = _currentUser.RoleId switch
        {
            1 => "Читатель",
            2 => "Автор",
            3 => "Администратор",
            _ => "—"
        };
    }

    private void SetupFrozenBanner()
    {
        FrozenBanner.IsVisible = _currentUser.IsFrozen;

        if (!_currentUser.IsFrozen) return;

        var hasAppeal = _db.Requestfrozens.Any(r => r.UserId == _currentUser.Id);
        if (hasAppeal)
        {
            BtnAppeal.IsEnabled = false;
            AppealText.IsEnabled = false;
            AppealStatus.Text = "Заявка на оспаривание уже отправлена. Ожидайте решения администратора.";
            AppealStatus.Foreground = Brushes.Gray;
            AppealStatus.IsVisible = true;
        }
    }
    
    private void SetupAuthorRequest()
    {
        AuthorRequestBlock.IsVisible = _currentUser.RoleId == 1;

        if (_currentUser.RoleId != 1) return;

        var hasRequest = _db.Requests.Any(r =>
            r.UserId == _currentUser.Id && r.RoleId == 2);

        if (hasRequest)
        {
            BtnAuthorRequest.IsEnabled = false;
            AuthorRequestText.IsEnabled = false;
            AuthorRequestStatus.Text = "Заявка уже отправлена. Ожидайте решения администратора.";
            AuthorRequestStatus.Foreground = Brushes.Gray;
            AuthorRequestStatus.IsVisible = true;
        }
    }

    private void OnAuthorRequestClick(object? sender, RoutedEventArgs e)
    {
        var text = AuthorRequestText.Text?.Trim();
        if (string.IsNullOrEmpty(text))
        {
            AuthorRequestStatus.Text = "Пожалуйста, заполните описание заявки.";
            AuthorRequestStatus.Foreground = Brushes.Red;
            AuthorRequestStatus.IsVisible = true;
            return;
        }

        if (_db.Requests.Any(r => r.UserId == _currentUser.Id && r.RoleId == 2))
        {
            AuthorRequestStatus.Text = "Заявка уже отправлена.";
            AuthorRequestStatus.Foreground = Brushes.Gray;
            AuthorRequestStatus.IsVisible = true;
            return;
        }

        _db.Requests.Add(new Request
        {
            UserId = _currentUser.Id,
            RoleId = 2, // Автор
            Description = text
        });
        _db.SaveChanges();

        BtnAuthorRequest.IsEnabled = false;
        AuthorRequestText.IsEnabled = false;
        AuthorRequestStatus.Text = "Заявка отправлена! Ожидайте решения администратора.";
        AuthorRequestStatus.Foreground = Brushes.Green;
        AuthorRequestStatus.IsVisible = true;
    }

    private void OnAppealClick(object? sender, RoutedEventArgs e)
    {
        var text = AppealText.Text?.Trim();
        if (string.IsNullOrEmpty(text))
        {
            AppealStatus.Text = "Пожалуйста, опишите причину оспаривания.";
            AppealStatus.Foreground = Brushes.Red;
            AppealStatus.IsVisible = true;
            return;
        }

        // Проверка на дубль
        if (_db.Requestfrozens.Any(r => r.UserId == _currentUser.Id))
        {
            AppealStatus.Text = "Заявка уже отправлена.";
            AppealStatus.Foreground = Brushes.Gray;
            AppealStatus.IsVisible = true;
            return;
        }

        _db.Requestfrozens.Add(new Requestfrozen
        {
            UserId = _currentUser.Id,
            Description = text
        });
        _db.SaveChanges();

        BtnAppeal.IsEnabled = false;
        AppealText.IsEnabled = false;
        AppealStatus.Text = "Заявка отправлена. Ожидайте решения администратора.";
        AppealStatus.Foreground = Brushes.Green;
        AppealStatus.IsVisible = true;
    }
    
    private void LoadFeedbacks()
    {
        var feedbacks = _db.Feedbacks
            .Include(f => f.Book)
            .Where(f => f.UserId == _currentUser.Id)
            .OrderByDescending(f => f.CreatedAt)
            .Select(f => new UserFeedbackViewModel
            {
                BookName = f.Book.Name,
                Text = f.Text,
                Score = f.Score,
                CreatedAt = f.CreatedAt
            })
            .ToList();

        if (feedbacks.Count == 0)
        {
            NoFeedbacksText.IsVisible = true;
            UserFeedbacks.IsVisible = false;
        }
        else
        {
            NoFeedbacksText.IsVisible = false;
            UserFeedbacks.IsVisible = true;
            UserFeedbacks.ItemsSource = feedbacks;
        }
    }
}

public class UserFeedbackViewModel
{
    public string BookName { get; init; } = "";
    public string Text { get; init; } = "";
    public byte Score { get; init; }
    public System.DateTime CreatedAt { get; init; }
}