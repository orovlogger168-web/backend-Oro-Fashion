using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using OroFashionAPI.DTOs;
using OroFashionAPI.Services;
using System.Security.Claims;

namespace OroFashionAPI.Controllers;

[ApiController]
[Route("api/review")]

public class ReviewController : ControllerBase
{
    private readonly ReviewService _reviewService;
    public ReviewController(ReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    [HttpGet("product/{productId}")]
    public async Task<IActionResult> GetById(string productId)
    {
        var reviews = await _reviewService.GetByProductAsync(productId);
        var avgRating = await _reviewService.GetAverageRatingAsync(productId);
        return Ok(new {reviews, avgRating});
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] CreateReviewDTO dto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
        ?? User.FindFirst("sub")?.Value;
        var userName = User.FindFirst(ClaimTypes.Name)?.Value ?? "Customer";

        if(userId == null)
            return Unauthorized();
        var (success, message, review) =
        await _reviewService.CreateAsync(userId, userName, dto);
        if(!success)
            return BadRequest(new {message});
        return Ok(review);
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> Delete(string id)
    {
        var deleted = await _reviewService.DeleteAsync(id);
        if(!deleted)
            return NotFound(new {message = "Review not found"});
        return Ok(new {message = "Review deleted."});
    }
}