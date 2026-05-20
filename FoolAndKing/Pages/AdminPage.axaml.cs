using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using FoolAndKing.Context;
using FoolAndKing.Entities;
using Microsoft.EntityFrameworkCore;

namespace FoolAndKing.Pages;

public partial class AdminPage : ContentPage
{
    private readonly MyDbContext _db;
    private List<AdminUserViewModel> _allUsers = new();

    public AdminPage() : this(new MyDbContext()) { }

    public AdminPage(MyDbContext db)
    {
        InitializeComponent();
        _db = db;
        LoadAll();
    }

    private void LoadAll()
    {
        LoadClaims();
        LoadFrozenRequests();
        LoadAuthorRequests();
        LoadFrozen();
        LoadUsers();
    }

    private void LoadClaims()
    {
        var claims = _db.Claims.Include(c => c.User).Include(c => c.Reason)
            .Include(c => c.Book).Include(c => c.FeedBack).ToList().Select(c => new ClaimViewModel(c)).ToList();
        ClaimsList.ItemsSource = claims;
    }

    private void OnAcceptClaimClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: ClaimViewModel vm })
        {
            return;
        }
        
        if (vm.Claim.BookId is not null)
        {
            var book = _db.Books.Find(vm.Claim.BookId);
            if (book is not null) book.IsFrozen = true;
        }
        _db.Claims.Remove(vm.Claim);
        _db.SaveChanges();
        LoadClaims();
        LoadFrozen();
    }

    private void OnRejectClaimClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: ClaimViewModel vm }) return;
        _db.Claims.Remove(vm.Claim);
        _db.SaveChanges();
        LoadClaims();
    }

    private void LoadFrozenRequests()
    {
        var requests = _db.Requestfrozens.Include(r => r.User)
            .ToList().Select(r => new FrozenRequestViewModel(r)).ToList();
        FrozenRequestsList.ItemsSource = requests;
    }

    private void OnAcceptFrozenRequestClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: FrozenRequestViewModel vm }) return;
        var user = _db.Users.Find(vm.Request.UserId);
        if (user is not null) user.IsFrozen = false;
        _db.Requestfrozens.Remove(vm.Request);
        _db.SaveChanges();
        LoadFrozenRequests();
        LoadFrozen();
        LoadUsers();
    }

    private void OnRejectFrozenRequestClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: FrozenRequestViewModel vm }) return;
        _db.Requestfrozens.Remove(vm.Request);
        _db.SaveChanges();
        LoadFrozenRequests();
    }
    
    private void LoadAuthorRequests()
    {
        var requests = _db.Requests
            .Include(r => r.User).Where(r => r.RoleId == 2).ToList()
            .Select(r => new AuthorRequestViewModel(r)).ToList();
        AuthorRequestsList.ItemsSource = requests;
    }

    private void OnAcceptAuthorRequestClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: AuthorRequestViewModel vm }) return;
        var user = _db.Users.Find(vm.Request.UserId);
        if (user is not null) user.RoleId = 2;
        _db.Requests.Remove(vm.Request);
        _db.SaveChanges();
        LoadAuthorRequests();
        LoadUsers();
    }

    private void OnRejectAuthorRequestClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: AuthorRequestViewModel vm }) return;
        _db.Requests.Remove(vm.Request);
        _db.SaveChanges();
        LoadAuthorRequests();
    }

    private void LoadFrozen()
    {
        FrozenBooksList.ItemsSource = _db.Books.Include(b => b.Author).Where(b => b.IsFrozen)
            .ToList().Select(b => new FrozenBookViewModel(b)).ToList();

        FrozenUsersList.ItemsSource = _db.Users.Where(u => u.IsFrozen).ToList();
    }

    private void OnUnfreezeBookClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: FrozenBookViewModel vm }) return;
        var book = _db.Books.Find(vm.Book.Id);
        if (book is not null) book.IsFrozen = false;
        _db.SaveChanges();
        LoadFrozen();
    }

    private void OnUnfreezeUserClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: User user }) return;
        var u = _db.Users.Find(user.Id);
        if (u is not null) u.IsFrozen = false;
        _db.SaveChanges();
        LoadFrozen();
        LoadUsers();
    }


    private void LoadUsers(string? search = null)
    {
        _allUsers = _db.Users.Include(u => u.Role).ToList().Select(u => new AdminUserViewModel(u)).ToList();

        if (!string.IsNullOrWhiteSpace(search))
            _allUsers = _allUsers.Where(u =>
                u.User.Name.ToLower().Contains(search.ToLower()) ||
                u.User.Login.ToLower().Contains(search.ToLower())).ToList();

        UsersList.ItemsSource = _allUsers;
    }

    private void OnUserSearchChanged(object? sender, TextChangedEventArgs e)
        => LoadUsers(UserSearchBox.Text);

    private async void OnChangeRoleClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: AdminUserViewModel vm }) return;

        var dialog = new Window { Title = "Сменить роль", Width = 300, Height = 180, CanResize = false };
        var stack = new StackPanel { Margin = new Avalonia.Thickness(16), Spacing = 10 };

        stack.Children.Add(new TextBlock { Text = $"Роль для {vm.User.Name}:" });

        var roles = _db.Roles.ToList();
        var roleBox = new ComboBox
        {
            ItemsSource = roles,
            DisplayMemberBinding = new Avalonia.Data.Binding("Name"),
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            SelectedIndex = roles.FindIndex(r => r.Id == vm.User.RoleId)
        };
        stack.Children.Add(roleBox);

        var btn = new Button { Content = "Сохранить", HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left };
        btn.Click += (_, _) =>
        {
            if (roleBox.SelectedItem is not Role role) return;
            var user = _db.Users.Find(vm.User.Id);
            if (user is not null) user.RoleId = role.Id;
            _db.SaveChanges();
            LoadUsers(UserSearchBox.Text);
            dialog.Close();
        };
        stack.Children.Add(btn);

        dialog.Content = stack;
        var window = TopLevel.GetTopLevel(this) as Window;
        if (window is not null) await dialog.ShowDialog(window);
    }

    private async void OnChangePasswordClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: AdminUserViewModel vm }) return;

        var dialog = new Window { Title = "Сменить пароль", Width = 320, Height = 200, CanResize = false };
        var stack = new StackPanel { Margin = new Avalonia.Thickness(16), Spacing = 10 };

        stack.Children.Add(new TextBlock { Text = $"Новый пароль для {vm.User.Name}:" });

        var passBox = new TextBox { Watermark = "Введите новый пароль...", PasswordChar = '●' };
        stack.Children.Add(passBox);

        var statusText = new TextBlock { IsVisible = false, FontSize = 12 };
        stack.Children.Add(statusText);

        var btn = new Button { Content = "Сохранить", HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left };
        btn.Click += (_, _) =>
        {
            var pass = passBox.Text?.Trim();
            if (string.IsNullOrEmpty(pass))
            {
                statusText.Text = "Введите пароль.";
                statusText.Foreground = Avalonia.Media.Brushes.Red;
                statusText.IsVisible = true;
                return;
            }
            var user = _db.Users.Find(vm.User.Id);
            if (user is not null) user.Password = pass;
            _db.SaveChanges();
            dialog.Close();
        };
        stack.Children.Add(btn);

        dialog.Content = stack;
        var window = TopLevel.GetTopLevel(this) as Window;
        if (window is not null) await dialog.ShowDialog(window);
    }

    private void OnFreezeUserClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: AdminUserViewModel vm }) return;
        var user = _db.Users.Find(vm.User.Id);
        if (user is not null) user.IsFrozen = true;
        _db.SaveChanges();
        LoadUsers(UserSearchBox.Text);
        LoadFrozen();
    }
}

public class ClaimViewModel
{
    public Claim Claim { get; }
    public string TypeText => Claim.BookId is not null ? $"Жалоба на книгу: {Claim.Book?.Name}"
        : Claim.FeedBackId is not null ? "Жалоба на отзыв" : "Жалоба на автора";
    public string ReasonText => $"Причина: {Claim.Reason.Name}";
    public string? Description => Claim.Description;
    public string UserText => $"От: {Claim.User.Name} ({Claim.User.Login})";

    public ClaimViewModel(Claim claim) => Claim = claim;
}

public class FrozenRequestViewModel
{
    public Requestfrozen Request { get; }
    public string UserName => Request.User.Name;
    public string? Description => Request.Description;

    public FrozenRequestViewModel(Requestfrozen request) => Request = request;
}

public class AuthorRequestViewModel
{
    public Request Request { get; }
    public string UserName => Request.User.Name;
    public string? Description => Request.Description;

    public AuthorRequestViewModel(Request request) => Request = request;
}

public class FrozenBookViewModel
{
    public Book Book { get; }
    public string Name => Book.Name;
    public string AuthorName => Book.Author.Name;

    public FrozenBookViewModel(Book book) => Book = book;
}

public class AdminUserViewModel
{
    public User User { get; }
    public string Name => User.Name;
    public string LoginEmail => $"{User.Login} · {User.Email}";
    public string RoleName => User.Role?.Name ?? "—";
    public bool IsFrozen => User.IsFrozen;
    public bool IsNotFrozen => !User.IsFrozen;

    public AdminUserViewModel(User user) => User = user;
}