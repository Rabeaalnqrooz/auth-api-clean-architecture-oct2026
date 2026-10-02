using AuthApi.Application.DTOs;
using AuthApi.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthApi.API.Controllers;

[ApiController]
[Route("api/v1/user")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly AdminService _adminService;

    public AdminController(AdminService adminService)
    {
        _adminService = adminService;
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetUsers([FromQuery] int page = 1, [FromQuery] int limit = 10)
    {
        var result = await _adminService.GetPagedUsersAsync(page, limit);
        return Ok(new
        {
            success = true,
            users = result.Users,
            total = result.Total,
            page = result.Page,
            limit = result.Limit,
            pages = result.Pages
        });
    }

    [HttpGet("users/{id:guid}")]
    public async Task<IActionResult> GetUser(Guid id)
    {
        var user = await _adminService.GetUserByIdAsync(id);
        return Ok(new { success = true, user });
    }

    [HttpDelete("users/{id:guid}")]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        await _adminService.DeleteUserAsync(id);
        return Ok(new { success = true, message = "تم حذف المستخدم" });
    }

    [HttpPut("users/{id:guid}/role")]
    public async Task<IActionResult> UpdateRole(Guid id, [FromBody] UpdateUserRoleRequestDto request)
    {
        await _adminService.UpdateUserRoleAsync(id, request);
        return Ok(new { success = true, message = "تم تحديث الدور" });
    }

    [HttpPut("users/{id:guid}/disable")]
    public async Task<IActionResult> Disable(Guid id)
    {
        await _adminService.DisableUserAsync(id);
        return Ok(new { success = true, message = "تم تعطيل المستخدم" });
    }

    [HttpPut("users/{id:guid}/enable")]
    public async Task<IActionResult> Enable(Guid id)
    {
        await _adminService.EnableUserAsync(id);
        return Ok(new { success = true, message = "تم تفعيل المستخدم" });
    }
}