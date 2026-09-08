using System.Security.Claims;
using GestaoConsultasUVV.Data;
using GestaoConsultasUVV.Models;
using GestaoConsultasUVV.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GestaoConsultasUVV.Controllers;

[Authorize]
[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
public class ConsultasController : Controller
{
    private readonly AppDbContext db;

    public ConsultasController(AppDbContext context)
    {
        db = context;
    }

    private int UsuarioId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> Index() => View(await db.Consultas.AsNoTracking()
        .Where(c => c.UsuarioId == UsuarioId).OrderBy(c => c.DataHora).ToListAsync());

    [HttpGet]
    public IActionResult Criar() => View(new ConsultaViewModel());

    [HttpPost]
    public async Task<IActionResult> Criar(ConsultaViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        db.Consultas.Add(new Consulta
        {
            Especialidade = model.Especialidade.Trim(),
            DataHora = model.DataHora!.Value,
            Descricao = model.Descricao.Trim(),
            // A conta vem da sessão autenticada, nunca de um campo enviado pelo navegador.
            UsuarioId = UsuarioId
        });
        await db.SaveChangesAsync();
        TempData["Sucesso"] = "Consulta cadastrada com sucesso.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Editar(int id)
    {
        var consulta = await MinhaConsulta(id);
        if (consulta is null) return NotFound();
        return View(new ConsultaViewModel
        {
            Especialidade = consulta.Especialidade, DataHora = consulta.DataHora,
            Descricao = consulta.Descricao
        });
    }

    [HttpPost]
    public async Task<IActionResult> Editar(int id, ConsultaViewModel model)
    {
        var consulta = await MinhaConsulta(id);
        if (consulta is null) return NotFound();
        if (!ModelState.IsValid) return View(model);
        consulta.Especialidade = model.Especialidade.Trim();
        consulta.DataHora = model.DataHora!.Value;
        consulta.Descricao = model.Descricao.Trim();
        try { await db.SaveChangesAsync(); }
        catch (DbUpdateConcurrencyException) { return NotFound(); }
        TempData["Sucesso"] = "Consulta atualizada.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Excluir(int id)
    {
        var consulta = await MinhaConsulta(id);
        return consulta is null ? NotFound() : View(consulta);
    }

    [HttpPost, ActionName("Excluir")]
    public async Task<IActionResult> ConfirmarExclusao(int id)
    {
        var consulta = await MinhaConsulta(id);
        if (consulta is null) return NotFound();
        db.Consultas.Remove(consulta);
        try { await db.SaveChangesAsync(); }
        catch (DbUpdateConcurrencyException) { return NotFound(); }
        TempData["Sucesso"] = "Consulta excluída.";
        return RedirectToAction(nameof(Index));
    }

    private Task<Consulta?> MinhaConsulta(int id) =>
        db.Consultas.SingleOrDefaultAsync(c => c.Id == id && c.UsuarioId == UsuarioId);
}
