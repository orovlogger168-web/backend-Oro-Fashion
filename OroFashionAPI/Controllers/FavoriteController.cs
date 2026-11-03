using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using OroFashionAPI.Services;
using System.Security.Claims;

namespace OroFashionAPI.Controllers;

[ApiController]
[Route("api/favorite")]
[Authorize]

public class FavoriteController : ControllerBase
{
    private readonly FavoriteServices _favoriteService;
    public FavoriteController(FavoriteServices favoriteService)
    {
        _favoriteService = favoriteService;
    }
    [HttpGet]
    public async Task<IActionResult> GetFavorites()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
        ?? User.FindFirst("sub")?.Value;
        if(userId == null )
            return Unauthorized();
        var ids = await _favoriteService.GetFavoriteIdAsync(userId);
        return Ok(ids);
    }

    [HttpPost("{productId}")]
    public async Task<IActionResult> Toggle(string productId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
        ?? User.FindFirst("sub")?.Value;
        if(userId == null)
            return Unauthorized();
        var ids = await _favoriteService.ToggleFavoriteAsync(userId, productId);
        return Ok(ids);
    }

    [HttpDelete]
    public async Task<IActionResult> Clear()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
        ?? User.FindFirst("sub")?.Value;
        if(userId == null)
            return Unauthorized();
        await _favoriteService.ClearAsync(userId);
        return Ok(new {message = "Favorite cleared."});
    }
}