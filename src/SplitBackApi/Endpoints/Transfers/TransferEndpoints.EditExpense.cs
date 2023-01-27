using SplitBackApi.Data;
using MongoDB.Bson;
using SplitBackApi.Helper;
using SplitBackApi.Endpoints.Requests;
using SplitBackApi.Extensions;
using SplitBackApi.Domain;
using AutoMapper;

namespace SplitBackApi.Endpoints;
public static partial class TransferEndpoints {
  private static async Task<IResult> EditTransfer(IRepository repo, HttpRequest request, EditTransferDto editTransferDto, IMapper mapper) {


    var groupId = ObjectId.Parse(editTransferDto.GroupId);
    var transferValidator = new TransferValidator();
    var validationResult = transferValidator.Validate(editTransferDto);

    if(validationResult.Errors.Count > 0) {
      return Results.Ok(validationResult.Errors.Select(x => new {
        Message = x.ErrorMessage,
        Field = x.PropertyName
      }));
    }

    var newTransfer = mapper.Map<Transfer>(editTransferDto);
    var transferId = ObjectId.Parse(editTransferDto.TransferId);

    var editTansferRes = await repo.EditTransfer(newTransfer, groupId, transferId);
    if(editTansferRes.IsFailure) return Results.BadRequest(editTansferRes.Error);

    var getGroupRes = await repo.GetGroupById(groupId);
    if(getGroupRes.IsFailure) return Results.BadRequest(getGroupRes.Error);

    return Results.Ok(getGroupRes.Value.PendingTransactions());
  }
}