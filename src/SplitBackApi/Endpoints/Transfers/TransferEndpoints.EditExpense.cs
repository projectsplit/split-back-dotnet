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

    await repo.EditTransfer(newTransfer,groupId,transferId);
    var getGroupResult = await repo.GetGroupById(groupId);
    return getGroupResult.Match(group => {
      return Results.Ok(group.PendingTransactions());

    }, e => {

      return Results.BadRequest(e.Message);
    });



  }
}