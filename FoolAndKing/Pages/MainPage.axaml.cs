using Avalonia.Controls;
using Avalonia.Interactivity;
using FoolAndKing.Context;
using FoolAndKing.Entities;

namespace FoolAndKing.Pages;

public partial class MainPage : ContentPage
{
    private readonly MyDbContext _db;
    private readonly User _currentUser;

    private readonly CatalogBook _catalogPage;
    private readonly BookList _bookListPage;
    private readonly AdminPage _adminPage;
    private readonly AutorPage _authorPage;
    private readonly UserPage _userPage;
    private readonly FrozenPage _frozenPage;

    public MainPage() : this(new MyDbContext(), new User()) { }

    public MainPage(MyDbContext db, User user)
    {
        InitializeComponent();
        _db = db;
        _currentUser = user;

        _catalogPage = new CatalogBook(_db, _currentUser, ShowPage);
        _bookListPage = new BookList();
        _adminPage = new AdminPage();
        _authorPage = new AutorPage(_db, _currentUser, ShowPage);
        _userPage = new UserPage(_db, _currentUser);
        _frozenPage = new FrozenPage();

        SetupSidebar();

        ShowPage(_catalogPage);
    }

    private void SetupSidebar()
    {
        BtnAdmin.IsVisible = _currentUser.RoleId == 3;
        BtnAuthorPage.IsVisible = _currentUser.RoleId == 2;
        BtnFrozen.IsVisible = _currentUser.IsFrozen;
    }

    private void ShowPage(ContentPage page)
    {
        PageContent.Content = page;
    }

    private void OnCatalogClick(object? sender, RoutedEventArgs e)
        => ShowPage(_catalogPage);

    private void OnListsClick(object? sender, RoutedEventArgs e)
        => ShowPage(_bookListPage);

    private void OnAdminClick(object? sender, RoutedEventArgs e)
        => ShowPage(_adminPage);

    private void OnAuthorPageClick(object? sender, RoutedEventArgs e)
        => ShowPage(_authorPage);

    private void OnFrozenClick(object? sender, RoutedEventArgs e)
        => ShowPage(_frozenPage);

    private void OnProfileClick(object? sender, RoutedEventArgs e)
        => ShowPage(_userPage);
}