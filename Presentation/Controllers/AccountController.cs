using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Presentation;

namespace Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountController : ControllerBase
{
    private readonly AccountGrpcService.AccountGrpcServiceClient _grpc;

    public AccountController(AccountGrpcService.AccountGrpcServiceClient grpcClient)
    {
        _grpc = grpcClient;
    }

    /// <summary>
    /// Creates a new account using gRPC.
    /// </summary>
    [HttpPost("register")]
    [SwaggerOperation(Summary = "Creates a new account")]
    [SwaggerResponse(200, "Account created", typeof(CreateAccountReply))]
    [SwaggerResponse(400, "Invalid input")]
    public async Task<IActionResult> Register([FromBody] CreateAccountRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest("Email and password are required.");

        var result = await _grpc.CreateAccountAsync(request);
        return Ok(result);
    }

    /// <summary>
    /// Validates a user's credentials.
    /// </summary>
    [HttpPost("validate")]
    [SwaggerOperation(Summary = "Validates user credentials")]
    [SwaggerResponse(200, "Validation result", typeof(ValidateCredentialsReply))]
    public async Task<IActionResult> ValidateCredentials([FromBody] ValidateCredentialsRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest("Email and password are required.");

        var result = await _grpc.ValidateCredentialsAsync(request);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves all user accounts.
    /// </summary>
    [HttpGet("all")]
    [SwaggerOperation(Summary = "Gets all user accounts")]
    [SwaggerResponse(200, "List of accounts", typeof(GetAccountsReply))]
    public async Task<IActionResult> GetAllAccounts()
    {
        var result = await _grpc.GetAccountsAsync(new GetAccountsRequest());
        return Ok(result);
    }

    /// <summary>
    /// Gets an account by user ID.
    /// </summary>
    [HttpGet("{id}")]
    [SwaggerOperation(Summary = "Get account by user ID")]
    [SwaggerResponse(200, "Account details", typeof(GetAccountByIdReply))]
    [SwaggerResponse(404, "Account not found")]
    public async Task<IActionResult> GetAccountById(string id)
    {
        var result = await _grpc.GetAccountByIdAsync(new GetAccountByIdRequest { UserId = id });

        if (!result.Succeeded)
            return NotFound(result.Message);

        return Ok(result);
    }
}
