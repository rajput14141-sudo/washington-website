using CarWash.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;

namespace CarWash.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class CustomersController : ControllerBase
{
    private readonly IAuthMirror _authMirror;
    private readonly ILogger<CustomersController> _logger;

    public CustomersController(IAuthMirror authMirror, ILogger<CustomersController> logger)
    {
        _authMirror = authMirror;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CustomerSignupDetails>>> GetAll()
    {
        try
        {
            return Ok(await _authMirror.GetCustomerSignupsAsync(HttpContext.RequestAborted));
        }
        catch (MySqlException exception)
        {
            _logger.LogError(exception, "Could not load customer signup details from MySQL");
            return Problem(
                "Could not load customer details from the signup database.",
                statusCode: StatusCodes.Status503ServiceUnavailable);
        }
    }
}