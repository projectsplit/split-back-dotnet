using System.Security.Claims;
using SplitBackApi.Api.Endpoints.Transfers.Requests;
using SplitBackApi.Api.Extensions;
using SplitBackApi.Data.Repositories.GroupRepository;
using SplitBackApi.Data.Repositories.TransferRepository;
using SplitBackApi.Domain.Extensions;
using SplitBackApi.Domain.Models;
using SplitBackApi.Domain.Validators;

namespace SplitBackApi.Api.Endpoints.Transfers;

public static partial class TransferEndpoints {

  private static async Task<IResult> CreateTransfer(
    ITransferRepository transferRepository,
    IGroupRepository groupRepository,
    HttpContext httpContext,
    ClaimsPrincipal claimsPrincipal,
    CreateTransferRequest request
  ) {
    
    var authenticatedUserId = claimsPrincipal.GetAuthenticatedUserId();
    
    var groupResult = await groupRepository.GetById(request.GroupId);
    if(groupResult.IsFailure) return Results.BadRequest(groupResult.Error);
    var group = groupResult.Value;
    
    var member = group.GetMemberByUserId(authenticatedUserId);
    if(member is null) return Results.BadRequest($"{authenticatedUserId} is not a member of group with id {request.GroupId}");
    
    if(member.Permissions.HasFlag(Domain.Models.Permissions.WriteAccess) is false) return Results.Forbid();
    
    var newTransfer = new Transfer {
      CreationTime = DateTime.UtcNow,
      LastUpdateTime = DateTime.UtcNow,
      Amount = request.Amount,
      Currency = request.Currency,
      Description = request.Description,
      GroupId = request.GroupId,
      ReceiverId = request.ReceiverId,
      SenderId = request.SenderId,
      TransferTime = request.TransferTime
    };

    var transferValidator = new TransferValidator();
    var validationResult = transferValidator.Validate(newTransfer);
    if(validationResult.IsValid is false) return Results.BadRequest(validationResult.Errors.Select(e => new {
      Field = e.PropertyName,
      ErrorMessage = e.ErrorMessage
    }));

    await transferRepository.Create(newTransfer);

    return Results.Ok();
  }
}