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
    private readonly IWebHostEnvironment _env;

    public UsersController(AppDbContext context, UserManager<User> userManager, IWebHostEnvironment env)
    {
        _context = context;
        _userManager = userManager;
        _env = env;
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
    
    [Authorize(Roles = "admin")]
    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [Authorize(Roles = "admin")]
    [HttpPost]
    public async Task<IActionResult> Create(string userName, string email, string password, DateTime birthDate, IFormFile? avatarFile)
    {
        string? avatarPath = null;
        if (avatarFile != null && avatarFile.Length > 0)
        {
            string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads/avatars");
            Directory.CreateDirectory(uploadsFolder);
            string uniqueFileName = Guid.NewGuid().ToString() + "_" + avatarFile.FileName;
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await avatarFile.CopyToAsync(fileStream);
            }
            avatarPath = "/uploads/avatars/" + uniqueFileName;
        }

        var user = new User
        {
            UserName = userName,
            Email = email,
            BirthDate = DateTime.SpecifyKind(birthDate, DateTimeKind.Utc),
            Avatar = avatarPath ?? "/images/default-avatar.png",
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, password);
        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(user, "user");
            return RedirectToAction(nameof(AdminPanel));
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError("", error.Description);
        }
        return View();
    }
    
    [Authorize(Roles = "admin")]
    [HttpGet]
    public async Task<IActionResult> Edit(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();
        return View(user);
    }

    [Authorize(Roles = "admin")]
    [HttpPost]
    public async Task<IActionResult> Edit(string id, string userName, string email, DateTime birthDate, IFormFile? avatarFile)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        user.UserName = userName;
        user.Email = email;
        user.BirthDate = DateTime.SpecifyKind(birthDate, DateTimeKind.Utc);

        if (avatarFile != null && avatarFile.Length > 0)
        {
            string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads/avatars");
            Directory.CreateDirectory(uploadsFolder);
            string uniqueFileName = Guid.NewGuid().ToString() + "_" + avatarFile.FileName;
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await avatarFile.CopyToAsync(fileStream);
            }
            user.Avatar = "/uploads/avatars/" + uniqueFileName;
        }

        var result = await _userManager.UpdateAsync(user);
        if (result.Succeeded)
        {
            return RedirectToAction(nameof(AdminPanel));
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError("", error.Description);
        }
        return View(user);
    }
    
    [HttpGet]
    public async Task<IActionResult> EditProfile()
    {
        var userId = _userManager.GetUserId(User);
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return NotFound();
        return View(user);
    }

    [HttpPost]
    public async Task<IActionResult> EditProfile(string userName, string email, DateTime birthDate, IFormFile? avatarFile)
    {
        var userId = _userManager.GetUserId(User);
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return NotFound();

        user.UserName = userName;
        user.Email = email;
        user.BirthDate = DateTime.SpecifyKind(birthDate, DateTimeKind.Utc);

        if (avatarFile != null && avatarFile.Length > 0)
        {
            string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads/avatars");
            Directory.CreateDirectory(uploadsFolder);
            string uniqueFileName = Guid.NewGuid().ToString() + "_" + avatarFile.FileName;
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await avatarFile.CopyToAsync(fileStream);
            }
            user.Avatar = "/uploads/avatars/" + uniqueFileName;
        }

        var result = await _userManager.UpdateAsync(user);
        if (result.Succeeded)
        {
            return RedirectToAction("Profile", new { id = user.Id });
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError("", error.Description);
        }
        return View(user);
    }
}