using Microsoft.AspNetCore.Identity;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;




namespace Presentation.Services;

public class AccountService(UserManager<IdentityUser> userManager, AccountServiceBusHandler serviceBus) : AccountGrpcService.AccountGrpcServiceBase
{
    private readonly UserManager<IdentityUser> _userManager = userManager;
    private readonly AccountServiceBusHandler _serviceBus = serviceBus;

    public override async Task<CreateAccountReply> CreateAccount(CreateAccountRequest request, ServerCallContext context)
    {
        var user = new IdentityUser
        {
            UserName = request.Email,
            Email = request.Email,
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        var payload = JsonSerializer.Serialize(new
        {
            userId = user.Id,
            email = user.Email,
        });

        await _serviceBus.PublishAsync(payload);

        var reply = new CreateAccountReply
        {
            Succeeded = result.Succeeded,
            Message = result.Succeeded
                ? "Account created successfully."
                : string.Join(", ", result.Errors.Select(e => e.Description))
        };

        if (result.Succeeded)
            reply.UserId = user.Id;

        return reply;
    }

    public override async Task<ValidateCredentialsReply> ValidateCredentials(ValidateCredentialsRequest request, ServerCallContext context)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return new ValidateCredentialsReply
            {
                Succeeded = false,
                Message = "Email and password must be provided."
            };
        }

        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            return new ValidateCredentialsReply
            {
                Succeeded = false,
                Message = "Invalid credentials."
            };
        }

        var isValid = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!isValid)
        {
            return new ValidateCredentialsReply
            {
                Succeeded = false,
                Message = "Invalid credentials."
            };
        }

        return new ValidateCredentialsReply
        {
            Succeeded = true,
            Message = "Login successful.",
            UserId = user.Id
        };
    }

    public override async Task<GetAccountsReply> GetAccounts(GetAccountsRequest request, ServerCallContext context)
    {
        var users = await _userManager.Users.ToListAsync();

        var reply = new GetAccountsReply
        {
            Succeeded = true,
            Message = users.Count > 0 ? "Accounts retrieved successfully." : "No accounts found."
        };

        foreach (var user in users)
        {
            reply.Accounts.Add(new Account
            {
                UserId = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber
            });
        }

        return reply;
    }

    public override async Task<GetAccountByIdReply> GetAccountById(GetAccountByIdRequest request, ServerCallContext context)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user == null)
            return new GetAccountByIdReply { Succeeded = false, Message = "User not found." };

        var account = new Account
        {
            UserId = user.Id,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber
        };

        return new GetAccountByIdReply { Succeeded = true, Account = account, Message = "Account retrieved successfully." };
    }

    public override async Task<UpdatePhoneNumberReply> UpdatePhoneNumber(UpdatePhoneNumberRequest request, ServerCallContext context)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user == null)
            return new UpdatePhoneNumberReply
            {
                Succeeded = false,
                Message = "User not found."
            };

        if (!string.Equals(user.PhoneNumber, request.PhoneNumber, StringComparison.Ordinal))
            user.PhoneNumber = request.PhoneNumber;

        var result = await _userManager.UpdateAsync(user);

        return new UpdatePhoneNumberReply
        {
            Succeeded = result.Succeeded,
            Message = result.Succeeded
                ? "Phone number updated successfully."
                : string.Join(", ", result.Errors.Select(e => e.Description))
        };
    }

    public override async Task<DeleteAccountByIdReply> DeleteAccountById(DeleteAccountByIdRequest request, ServerCallContext context)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user == null)
        {
            return new DeleteAccountByIdReply
            {
                Succeeded = false,
                Message = "User not found."
            };
        }

        var result = await _userManager.DeleteAsync(user);

        return new DeleteAccountByIdReply
        {
            Succeeded = result.Succeeded,
            Message = result.Succeeded
                ? "Account deleted successfully."
                : string.Join(", ", result.Errors.Select(e => e.Description))
        };

    }

    public override async Task<ConfirmAccountReply> ConfirmAccount(ConfirmAccountRequest request, ServerCallContext context)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user == null)
            return new ConfirmAccountReply { Succeeded = false, Message = "User not found." };

        if (await _userManager.IsEmailConfirmedAsync(user))
            return new ConfirmAccountReply { Succeeded = true, Message = "Account already confirmed." };

        var result = await _userManager.ConfirmEmailAsync(user, request.Token);

        return new ConfirmAccountReply
        {
            Succeeded = result.Succeeded,
            Message = result.Succeeded
                ? "Email confirmed successfully."
                : string.Join(", ", result.Errors.Select(e => e.Description))
        };
    }

    public override async Task<ConfirmEmailChangeReply> ConfirmEmailChange(ConfirmEmailChangeRequest request, ServerCallContext context)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user == null)
            return new ConfirmEmailChangeReply { Succeeded = false, Message = "User not found." };

        var result = await _userManager.ChangeEmailAsync(user, request.NewEmail, request.Token);

        return new ConfirmEmailChangeReply
        {
            Succeeded = result.Succeeded,
            Message = result.Succeeded
                ? "Email updated successfully."
                : string.Join(", ", result.Errors.Select(e => e.Description))
        };
    }

    public override async Task<UpdateEmailReply> UpdateEmail(UpdateEmailRequest request, ServerCallContext context)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user == null)
            return new UpdateEmailReply { Succeeded = false, Message = "User not found." };

        if (string.Equals(user.Email, request.NewEmail, StringComparison.OrdinalIgnoreCase))
        {
            return new UpdateEmailReply
            {
                Succeeded = false,
                Message = "New email cannot be the same as the current email.",
                Token = string.Empty
            };
        }

        var token = await _userManager.GenerateChangeEmailTokenAsync(user, request.NewEmail);

        return new UpdateEmailReply
        {
            Succeeded = true,
            Message = "Token generated successfully.",
            Token = token
        };
    }

    public override async Task<ResetPasswordReply> ResetPassword(ResetPasswordRequest request, ServerCallContext context)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);

        if (user == null)
            return new ResetPasswordReply { Succeeded = false, Message = "User not found." };

        var result = await _userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);

        return new ResetPasswordReply
        {
            Succeeded = result.Succeeded,
            Message = result.Succeeded
                ? "Password reset successfully."
                : string.Join(", ", result.Errors.Select(e => e.Description))
        };
    }

    public override async Task<GenerateTokenReply> GenerateEmailConfirmationToken(GenerateTokenRequest request, ServerCallContext context)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user == null)
            return new GenerateTokenReply { Succeeded = false, Message = "User not found." };

        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        return new GenerateTokenReply
        {
            Succeeded = true,
            Token = token,
            Message = "Email confirmation token generated."
        };
    }

    public override async Task<GenerateTokenReply> GeneratePasswordResetToken(GenerateTokenRequest request, ServerCallContext context)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user == null)
            return new GenerateTokenReply { Succeeded = false, Message = "User not found." };


        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        return new GenerateTokenReply
        {
            Succeeded = true,
            Token = token,
            Message = "Password reset token generated."
        };
    }
}
