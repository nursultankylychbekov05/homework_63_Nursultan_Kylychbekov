using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using homeWork_63.Data;
using homeWork_63.Models;

namespace homeWork_63.Controllers;

[Authorize]
public class ChatController : Controller
{
    private readonly AppDbContext _context;
    private readonly UserManager<User> _userManager;

    public ChatController(AppDbContext context, UserManager<User> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [HttpGet]
    public IActionResult Index()
    {
        ViewBag.CurrentUserId = _userManager.GetUserId(User);
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> GetMessages()
    {
        var currentUserId = _userManager.GetUserId(User);
        var messages = await _context.Messages
            .Include(m => m.User)
            .OrderByDescending(m => m.CreatedAt)
            .Take(30)
            .OrderBy(m => m.CreatedAt)
            .Select(m => new
            {
                id = m.Id,
                text = m.Text,
                createdAt = m.CreatedAt.ToLocalTime().ToString("dd.MM.yyyy HH:mm"),
                userName = m.User != null ? m.User.UserName : "Аноним",
                userId = m.UserId,
                avatar = m.User != null ? m.User.Avatar : "/images/default-avatar.png",
                isCurrentUser = m.UserId == currentUserId
            })
            .ToListAsync();

        return Json(messages);
    }

    [HttpPost]
    public async Task<IActionResult> SendMessage(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return BadRequest();

        if (text.Length > 150)
            text = text.Substring(0, 150);

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var message = new Message
        {
            Text = text,
            CreatedAt = DateTime.UtcNow,
            UserId = user.Id
        };

        _context.Messages.Add(message);
        await _context.SaveChangesAsync();

        return Json(new
        {
            success = true,
            text = message.Text,
            createdAt = message.CreatedAt.ToLocalTime().ToString("dd.MM.yyyy HH:mm"),
            userName = user.UserName,
            userId = user.Id,
            avatar = user.Avatar
        });
    }
}