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

    await repo.AddNewTransfer(newTransfer, groupId);
    var result = await repo.GetGroupById(groupId);

    return result.Match(group => {
      return Results.Ok(group.PendingTransactions());
    }, e => {
      return Results.BadRequest(e.Message);
    });

  }
}