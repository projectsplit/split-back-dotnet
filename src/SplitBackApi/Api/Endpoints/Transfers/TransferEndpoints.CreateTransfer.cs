using SplitBackApi.Data;
using SplitBackApi.Requests;
using SplitBackApi.Domain;
using System.Security.Claims;
using SplitBackApi.Extensions;
using SplitBackApi.Domain.Extensions;

namespace SplitBackApi.Endpoints;

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
    
    if(member.Permissions.HasFlag(Permissions.WriteAccess) is false) return Results.Forbid();
    
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