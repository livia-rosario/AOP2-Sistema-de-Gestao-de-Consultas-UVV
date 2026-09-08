using System.Security.Claims;
using GestaoConsultasUVV.Data;
using GestaoConsultasUVV.Models;
using GestaoConsultasUVV.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace GestaoConsultasUVV.Controllers;

public class ContaController : Controller
{
    private readonly AppDbContext db;
    private readonly IPasswordHasher<Usuario> hasher;

    public ContaController(AppDbContext context, IPasswordHasher<Usuario> passwordHasher)
    {
        db = context;
        hasher = passwordHasher;
    }

    [HttpGet, AllowAnonymous]
    public IActionResult Cadastro() => User.Identity?.IsAuthenticated == true
        ? RedirectToAction("Index", "Consultas") : View(new CadastroViewModel());

    [HttpPost, AllowAnonymous]
    public async Task<IActionResult> Cadastro(CadastroViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var email = model.Email.Trim().ToLowerInvariant();
        if (await db.Usuarios.AnyAsync(u => u.Email == email))
        {
            ModelState.AddModelError(nameof(model.Email), "Este e-mail já está cadastrado.");
            return View(model);
        }
        var usuario = new Usuario { Nome = model.Nome.Trim(), Email = email };
        usuario.SenhaHash = hasher.HashPassword(usuario, model.Senha);
        db.Usuarios.Add(usuario);
        try { await db.SaveChangesAsync(); }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2601 or 2627 })
        {
            ModelState.AddModelError(nameof(model.Email), "Este e-mail já está cadastrado.");
            return View(model);
        }
        TempData["Sucesso"] = "Conta criada! Entre com seu e-mail e senha.";
        return RedirectToAction(nameof(Login));
    }

    [HttpGet, AllowAnonymous]
    public IActionResult Login(string? returnUrl = null) => User.Identity?.IsAuthenticated == true
        ? RedirectToAction("Index", "Consultas") : View(new LoginViewModel { ReturnUrl = returnUrl });

    [HttpPost, AllowAnonymous]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var email = model.Email.Trim().ToLowerInvariant();
        var usuario = await db.Usuarios.SingleOrDefaultAsync(u => u.Email == email);
        var result = usuario is null ? PasswordVerificationResult.Failed
            : hasher.VerifyHashedPassword(usuario, usuario.SenhaHash, model.Senha);
        if (usuario is null || result == PasswordVerificationResult.Failed)
        {
            ModelState.AddModelError("", "E-mail ou senha incorretos.");
            return View(model);
        }
        if (result == PasswordVerificationResult.SuccessRehashNeeded)
        {
            usuario.SenhaHash = hasher.HashPassword(usuario, model.Senha);
            await db.SaveChangesAsync();
        }
        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new Claim(ClaimTypes.Name, usuario.Nome)
        }, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity), new AuthenticationProperties { IsPersistent = false });
        return Url.IsLocalUrl(model.ReturnUrl) ? LocalRedirect(model.ReturnUrl!)
            : RedirectToAction("Index", "Consultas");
    }

    [HttpPost, Authorize]
    public async Task<IActionResult> Sair()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }
}
