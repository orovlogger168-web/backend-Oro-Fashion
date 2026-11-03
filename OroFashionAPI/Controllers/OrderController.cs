using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using OroFashionAPI.DTOs;
using OroFashionAPI.Services;
using System.Security.Claims;
using OroFashionAPI.Models;

namespace OroFashionAPI.Controllers;

[ApiController]
[Route("api/order")]
[Authorize]
public class OrderController : ControllerBase
{
    private readonly OrderService _orderService;
    public OrderController(OrderService orderService)
    {
        _orderService = orderService;
    }

[HttpPost]
public async Task<IActionResult> Create([FromBody] CreateOrderDTO dto)
{
    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
              ?? User.FindFirst("sub")?.Value;
    if (userId == null) return Unauthorized();
    if (!dto.Items.Any())
        return BadRequest(new { message = "Order must have at least one item." });

    try
    {
        var order = await _orderService.CreateAsync(userId, dto);
        // return id explicitly so Angular can read it
        return Ok(new {
            id = order.Id,        // ← explicit id field
            _id = order.Id,       // ← also return _id for compatibility
            orderRef = order.OrderRef,
            status = order.Status,
            total = order.Total,
            createdAt = order.CreateAt
        });
    }
    catch (Exception ex)
    {
        return StatusCode(500, new { message = ex.Message });
    }
}

    [HttpGet("my")]
    public async Task<IActionResult> GetMyOrder()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
        ?? User.FindFirst("sub")?.Value;
        if(userId == null)
            return Unauthorized();
        var orders = await _orderService.GetUserOrderAsync(userId);
        return Ok(orders);  
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var order = await _orderService.GetByIdAsync(id);
        if(order == null)
            return NotFound(new {message = "Order not found."});

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            if(order.UserId != userId && role != "admin")
                return Forbid();
            return Ok(order);                       
    }

    [HttpGet("all")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> GetAll() =>
    Ok(await _orderService.GetAllOrderAsync());

    [HttpPatch("{id}/status")]
    [Authorize(Roles = "admin")]
    public async  Task<IActionResult> UpdateStatus(
        string id, [FromBody] UpdateOrderStatusDTO dto
    )
    {
        var updated = await _orderService.UpdateStatusAsync(id, dto.Status);
        if(!updated)
            return NotFound(new {message = "Order not found."});
        return Ok(new {message = "Status updated."});
    }
}