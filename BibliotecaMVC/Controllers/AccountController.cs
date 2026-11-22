using BibliotecaMVC.Enums;
using BibliotecaMVC.Exceptions;
using BibliotecaMVC.Models;
using BibliotecaMVC.Services;
using BibliotecaMVC.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace BibliotecaMVC.Controllers;

public class AccountController : Controller
{
    private readonly AuthService _authService;

    public AccountController(AuthService authService)
    {
        _authService = authService;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        // Se já estiver logado, redireciona para a Home
        if (HttpContext.Session.GetInt32("UsuarioId").HasValue)
            return RedirectToAction("Index", "Home");

        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            var usuario = await _authService.AutenticarAsync(model.Email, model.Senha);

            // Grava os dados essenciais na sessão
            HttpContext.Session.SetInt32("UsuarioId", usuario.ID);
            HttpContext.Session.SetString("UsuarioNome", usuario.Nome);
            HttpContext.Session.SetString("UsuarioEmail", usuario.Email);

            // Identifica o perfil para controle visual e autorização
            var tipoUsuario = usuario switch
            {
                Administrador => TipoUsuario.Administrador,
                Bibliotecario => TipoUsuario.Bibliotecario,
                Cliente => TipoUsuario.Cliente,
                _ => throw new InvalidOperationException("Tipo de usuário desconhecido.")
            };
            HttpContext.Session.SetString("TipoUsuario", tipoUsuario.ToString());

            if (usuario is Cliente cliente)
                HttpContext.Session.SetString("TipoCliente", cliente.Perfil.ToString());

            if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                return Redirect(model.ReturnUrl);

            return RedirectToAction("Index", "Home");
        }
        catch (RegraNegocioException ex)
        {
            // Trata as exceções de regra de negócio, exibindo a mensagem na tela
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear(); // Limpa a sessão
        return RedirectToAction("Login", "Account");
    }

    [HttpGet]
    public IActionResult AcessoNegado()
    {
        return View();
    }
}