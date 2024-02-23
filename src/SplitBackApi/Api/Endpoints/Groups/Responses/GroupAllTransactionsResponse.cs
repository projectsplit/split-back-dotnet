using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using SplitBackApi.Domain.Models;

namespace SplitBackApi.Api.Endpoints.Groups.Responses;

public class TransferWithMemberNames: EntityBase
{

    public DateTime TransferTime { get; set; }

    public string Description { get; set; }

    public string Amount { get; set; }

    public string Currency { get; set; }

    public string SenderName { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    public string SenderUserId { get; set; }

    public string ReceiverName { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    public string ReceiverUserId { get; set; }

}

public class ExpenseWithMemberNames : EntityBase
{
    public string Description { get; set; }

    public string Amount { get; set; }

    public string Currency { get; set; }

    public ICollection<PayerWithName> Payers { get; set; } = new List<PayerWithName>();

    public ICollection<ParticipantWithName> Participants { get; set; } =
        new List<ParticipantWithName>();

    public DateTime ExpenseTime { get; set; }

    public ICollection<string> Labels { get; set; } = new List<string>();
}

public class PayerWithName
{
    [BsonRepresentation(BsonType.ObjectId)]
    public string UserId { get; set; }
    public string Name { get; set; }
    public string PaymentAmount { get; set; }
}

public class ParticipantWithName
{
    [BsonRepresentation(BsonType.ObjectId)]
    public string UserId { get; set; }
    public string Name { get; set; }
    public string ParticipationAmount { get; set; }
}

public class GroupAllTransactionsResponse
{
    public ExpenseWithMemberNames Expense { get; }
    public TransferWithMemberNames Transfer { get; }

    public GroupAllTransactionsResponse(ExpenseWithMemberNames expense)
    {
        Expense = expense;
    }

    public GroupAllTransactionsResponse(TransferWithMemberNames transfer)
    {
        Transfer = transfer;
    }

    public DateTime TransactionTime => Expense?.ExpenseTime ?? Transfer.TransferTime;
}
