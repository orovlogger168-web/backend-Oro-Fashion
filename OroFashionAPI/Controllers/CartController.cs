using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using OroFashionAPI.DTOs;
using OroFashionAPI.Services;
using System.Security.Claims;
using OroFashionAPI.Models;

namespace OroFashionAPI.Controllers;

[ApiController]
[Route("api/cart")]
[Authorize]

public class CartController : ControllerBase
{
    private readonly CartService _cartService;
    public CartController(CartService cartService)
    {
        _cartService = cartService;
    }

    [HttpGet]
    public async Task<IActionResult> GetCart()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
        ?? User.FindFirst("sub")?.Value;
        if(userId == null)
            return Unauthorized();
        var cart = await _cartService.GetCartAsync(userId);
        return Ok(cart?.Items ?? new List<CartItem>());
    }

    [HttpPost("sync")]
    public async Task<IActionResult> Sync([FromBody] SyncCartDTO dto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
        ?? User.FindFirst("sub")?.Value;
        if(userId == null)
            return Unauthorized();
        await _cartService.SyncCartAsync(userId, dto);
        return Ok(new {message = "Cart synced."});
    }

    [HttpDelete]
    public async Task<IActionResult> Clear()
    {
        var UserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
        ?? User.FindFirst("sub")?.Value;
        if(UserId == null)
            return Unauthorized();
        await _cartService.ClearCartAsync(UserId);
        return Ok(new {message = "Cart cleared."});
    }
}