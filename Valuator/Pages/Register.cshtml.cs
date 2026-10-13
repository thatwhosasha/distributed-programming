using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Valuator.Services;

namespace Valuator.Pages;

public class RegisterModel : PageModel
{
    private readonly UserService _users;
    public RegisterModel(UserService users) => _users = users;

    [BindProperty] public string Login { get; set; } = "";
    [BindProperty] public string Password { get; set; } = "";
    public string Error { get; set; } = "";

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrWhiteSpace(Login) || string.IsNullOrWhiteSpace(Password))
        {
            Error = "Логин и пароль обязательны";
            return Page();
        }

        if (!await _users.RegisterAsync(Login, Password))
        {
            Error = "Пользователь с таким логином уже существует";
            return Page();
        }

        return RedirectToPage("Login");
    }
}