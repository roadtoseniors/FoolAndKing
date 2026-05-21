using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using FoolAndKing.Context;
using FoolAndKing.Entities;

namespace FoolAndKing.Pages;

public partial class AuthPage : ContentPage
{
    private readonly MyDbContext _db;

    public AuthPage() : this(new MyDbContext()) { }

    public AuthPage(MyDbContext db)
    {
        InitializeComponent();
        _db = db;
    }

    private void OnLoginClick(object? sender, RoutedEventArgs e)
    {
        var login = LoginTextBox.Text?.Trim();
        var password = PasswordTextBox.Text;

        if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
        {
            ShowLoginError("Заполните все поля.");
            return;
        }

        var user = _db.Users.FirstOrDefault(u => u.Login == login && u.Password == password);

        if (user is null)
        {
            ShowLoginError("Неверный логин или пароль.");
            return;
        }

        Navigation.PushAsync(new MainPage(_db, user));
    }

    private void OnRegisterClick(object? sender, RoutedEventArgs e)
    {
        var name = RegNameTextBox.Text?.Trim();
        var email = RegEmailTextBox.Text?.Trim();
        var login = RegLoginTextBox.Text?.Trim();
        var password = RegPasswordTextBox.Text;
        var confirm = RegPasswordConfirmTextBox.Text;

        RegErrorText.Foreground = Brushes.Red;

        if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(email) ||
            string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
        {
            ShowRegError("Заполните все поля.");
            return;
        }

        if (password != confirm)
        {
            ShowRegError("Пароли не совпадают.");
            return;
        }

        if (_db.Users.Any(u => u.Login == login))
        {
            ShowRegError("Такой логин уже занят.");
            return;
        }

        if (_db.Users.Any(u => u.Email == email))
        {
            ShowRegError("Этот email уже используется.");
            return;
        }

        _db.Users.Add(new User
        {
            Name = name,
            Login = login,
            Password = password,
            Email = email,
            RoleId = 1,
            IsFrozen = false
        });
        _db.SaveChanges();

        RegErrorText.Foreground = Brushes.Green;
        ShowRegError("Регистрация прошла успешно! Теперь войдите.");
    }

    private void ShowLoginError(string msg)
    {
        LoginErrorText.Text = msg;
        LoginErrorText.IsVisible = true;
    }

    private void ShowRegError(string msg)
    {
        RegErrorText.Text = msg;
        RegErrorText.IsVisible = true;
    }
}