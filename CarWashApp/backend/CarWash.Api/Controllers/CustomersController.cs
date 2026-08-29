using CarWash.Api.DTOs;
using CarWash.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CarWash.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class CustomersController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;

    public CustomersController(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<ActionResult<List<CustomerDetailsDto>>> GetAll()
    {
        var customers = await _userManager.GetUsersInRoleAsync("Customer");
        return Ok(customers
            .OrderBy(customer => customer.FullName)
            .Select(customer => new CustomerDetailsDto(
                customer.Id,
                customer.FullName,
                customer.Email ?? string.Empty,
                customer.PhoneNumber,
                customer.Address))
            .ToList());
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var customer = await _userManager.FindByIdAsync(id);
        if (customer is null || !await _userManager.IsInRoleAsync(customer, "Customer"))
            return NotFound();

        var result = await _userManager.DeleteAsync(customer);
        if (!result.Succeeded)
            return Problem(
                detail: string.Join(" ", result.Errors.Select(error => error.Description)),
                statusCode: StatusCodes.Status500InternalServerError);

        return NoContent();
    }
}