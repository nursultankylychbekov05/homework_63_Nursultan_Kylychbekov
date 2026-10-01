using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using homeWork_63.Data;
using homeWork_63.Models;

namespace homeWork_63.Controllers;

[Authorize]
public class UsersController : Controller
{
    private readonly AppDbContext _context;
    private readonly UserManager<User> _userManager;

    public UsersController(AppDbContext context, UserManager<User> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Profile(string id)
    {
        var user = await _context.Users
            .Include(u => u.Messages)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null) return NotFound();

        ViewBag.MessageCount = user.Messages.Count;
        ViewBag.IsCurrentUser = _userManager.GetUserId(User) == id;

        return View(user);
    }

    [Authorize(Roles = "admin")]
    [HttpGet]
    public async Task<IActionResult> AdminPanel()
    {
        var users = await _context.Users.ToListAsync();
        return View(users);
    }

    [Authorize(Roles = "admin")]
    [HttpPost]
    public async Task<IActionResult> ToggleBlock(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        var isLockedOut = await _userManager.IsLockedOutAsync(user);
        if (isLockedOut)
        {
            await _userManager.SetLockoutEndDateAsync(user, null);
        }
        else
        {
            await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
        }

        return RedirectToAction("AdminPanel");
    }
}