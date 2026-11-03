using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using OroFashionAPI.Services;
using OroFashionAPI.DTOs;

namespace OroFashionAPI.Controllers;

[ApiController]
[Route("api/product")]
public class ProductController : ControllerBase
{
    private readonly ProductServices _productService;
    public ProductController(ProductServices productServices)
    {
        _productService = productServices;
    }
    //get api/product - public anyone can browse products
    [HttpGet]
    public async Task<IActionResult> GetAll() =>
    Ok(await _productService.GetAllAsync());

    //get api/product/featured - public home page featured products
    [HttpGet("featured")]
    public async Task<IActionResult> GetFeatured() =>
    Ok(await _productService.GetByFeaturedAsync());

    //get api/product/{id} - public view one product
    //{id} = route parameter - url: api/product/999luck168
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var product = await _productService.GetByIdAsync(id);
        //not found = http 404
        if(product == null)
            return NotFound (new {message = "Product not found."});
        return Ok(product);
    }
    // GET api/product/category/{category} — PUBLIC — filter by men/women/kids
    [HttpGet("category/{category}")]
    public async Task<IActionResult> GetByCategory(string category) =>
    Ok(await _productService.GetByCategoryAsync(category));

    // GET api/product/search?q=blazer — PUBLIC — search products
    // [FromQuery] = read from URL query string: ?q=blazer
    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string q) =>
    Ok(await _productService.SearchAsync(q));

    //Post api/product - admin only
    [HttpPost]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Create([FromBody] CreateProductDTO dto) =>
    Ok(await _productService.CreateAsync(dto));

    //Put api/product/{id} - admin only
    [HttpPut("{id}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Update(string id, [FromBody] CreateProductDTO dto)
    {
        var updated = await _productService.UpdateAsync(id, dto);
        if(!updated)
            return NotFound(new {message = "Product not found"});
        return Ok(new {message = "Product update."});
    }

    //delete api/product/{id} - admin only (soft delete)
    [HttpDelete("{id}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Delete(string id)
    {
        var deleted = await _productService.DeleteAsync(id);
        if(!deleted)
            return NotFound(new {message = "Product not found"});
        return Ok(new {message = "Product deleted"});
    }
}