using SplitBackApi.Data;
using MongoDB.Bson;
using SplitBackApi.Requests;
using SplitBackApi.Extensions;
using SplitBackApi.Domain;
using AutoMapper;

namespace SplitBackApi.Endpoints;

public static partial class TransferEndpoints {
  
  private static async Task<IResult> EditTransfer(
    IRepository repo,
    HttpRequest request,
    EditTransferDto editTransferDto,
    IMapper mapper) {

    var transferValidator = new TransferValidator();
    var validationResult = transferValidator.Validate(editTransferDto);

    if(validationResult.Errors.Count > 0) {
      return Results.Ok(validationResult.Errors.Select(x => new {
        Message = x.ErrorMessage,
        Field = x.PropertyName
      }));
    }

    var newTransfer = mapper.Map<Transfer>(editTransferDto);

    var editTansferRes = await repo.EditTransfer(newTransfer, editTransferDto.GroupId, editTransferDto.TransferId);
    if(editTansferRes.IsFailure) return Results.BadRequest(editTansferRes.Error);

    var getGroupRes = await repo.GetGroupById(editTransferDto.GroupId);
    if(getGroupRes.IsFailure) return Results.BadRequest(getGroupRes.Error);

    return Results.Ok(getGroupRes.Value.PendingTransactions());
  }
}