using SplitBackApi.Data;
using MongoDB.Bson;
using SplitBackApi.Helper;
using SplitBackApi.Endpoints.Requests;
using SplitBackApi.Extensions;
using SplitBackApi.Domain;
using AutoMapper;

namespace SplitBackApi.Endpoints;
public static partial class TransferEndpoints {
  private static async Task<IResult> AddTransfer(IRepository repo, HttpRequest request, NewTransferDto newTransferDto, IMapper mapper) {

    var groupId = ObjectId.Parse(newTransferDto.GroupId);
    var transferValidator = new TransferValidator();

    var validationResult = transferValidator.Validate(newTransferDto);
    if(validationResult.Errors.Count > 0) {
      return Results.Ok(validationResult.Errors.Select(x => new {
        Message = x.ErrorMessage,
        Field = x.PropertyName
      }));
    }
    var newTransfer = mapper.Map<Transfer>(newTransferDto);
    newTransfer.CreationTime = DateTime.Now;

    var transferRes = await repo.AddNewTransfer(newTransfer, groupId);
    if(transferRes.IsFailure) return Results.BadRequest(transferRes.Error);

    var getGroupRes = await repo.GetGroupById(groupId);
    if(getGroupRes.IsFailure) return Results.BadRequest(getGroupRes.Error);

    return Results.Ok(getGroupRes.Value.PendingTransactions());

  }
}