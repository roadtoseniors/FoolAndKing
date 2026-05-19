using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using FoolAndKing.Context;
using FoolAndKing.Entities;

namespace FoolAndKing.Pages;

public partial class BookPage : ContentPage
{
    private readonly MyDbContext _db;
    private readonly User _currentUser;
    private Book _book;

    public BookPage() : this(new MyDbContext(), new User(), new Book()) { }

    public BookPage(MyDbContext db, User user, Book book)
    {
        InitializeComponent();
        _db = db;
        _currentUser = user;
        _book = book;

        LoadBook();
    }

    private void LoadBook()
    {
        
    }

    private void OnBackClick(object? sender, RoutedEventArgs e)
    {
        throw new System.NotImplementedException();
    }

    private void OnClaimBookClick(object? sender, RoutedEventArgs e)
    {
        throw new System.NotImplementedException();
    }

    private void OnClaimAuthorClick(object? sender, RoutedEventArgs e)
    {
        throw new System.NotImplementedException();
    }

    private void OnSubmitFeedbackClick(object? sender, RoutedEventArgs e)
    {
        throw new System.NotImplementedException();
    }

    private void OnClaimFeedbackClick(object? sender, RoutedEventArgs e)
    {
        throw new System.NotImplementedException();
    }

    private void OnFreezeFeedbackClick(object? sender, RoutedEventArgs e)
    {
        throw new System.NotImplementedException();
    }

    private void OnFreezeBookClick(object? sender, RoutedEventArgs e)
    {
        throw new System.NotImplementedException();
    }
}