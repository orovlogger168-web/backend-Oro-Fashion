using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using OroFashionAPI.Services;

namespace OroFashionAPI.Controllers;

[ApiController]
[Route("api/newsletter")]
public class NewsletterController : ControllerBase
{
    private readonly NewsletterService _newsletterService;
    public NewsletterController(NewsletterService newsletterService)
    {
        _newsletterService = newsletterService;
    }

    [HttpPost("subscribe")]
    public async Task<IActionResult> Subscribe([FromBody] string email)
    {
        if(string.IsNullOrWhiteSpace(email))
            return BadRequest(new {message = "Email is required"});
        var (success, message) = await _newsletterService.SubscribeAsync(email);
        if(!success)
            return BadRequest(new {message});
        return Ok(new {message});
    }

    [HttpGet]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> GetAll() =>
    Ok(await _newsletterService.GetAllAsync());
}