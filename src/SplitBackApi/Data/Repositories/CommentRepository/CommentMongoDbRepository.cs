using CSharpFunctionalExtensions;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using SplitBackApi.Configuration;
using SplitBackApi.Domain.Extensions;
using SplitBackApi.Domain.Models;

namespace SplitBackApi.Data.Repositories.CommentRepository;

public class CommentMongoDbRepository : ICommentRepository {
  
  private readonly MongoClient _mongoClient;
  private readonly IMongoCollection<Comment> _commentCollection;
  private readonly IMongoCollection<PastComment> _pastCommentCollection;

  public CommentMongoDbRepository(
    IOptions<AppSettings> appSettings
  ) {

    var dbSettings = appSettings.Value.MongoDb;
    _mongoClient = new MongoClient(dbSettings.ConnectionString);
    var mongoDatabase = _mongoClient.GetDatabase(dbSettings.Database.Name);

    _commentCollection = mongoDatabase.GetCollection<Comment>(dbSettings.Database.Collections.Comments);
    _pastCommentCollection = mongoDatabase.GetCollection<PastComment>(dbSettings.Database.Collections.PastComments);
  }
  
  public async Task Create(Comment comment) {
    
    await _commentCollection.InsertOneAsync(comment);
  }

  public async Task<Result> DeleteById(string commentId) {
    
    var findCommentFilter = Builders<Comment>.Filter.Eq(c => c.Id, commentId);
    
    var commentFound = await _commentCollection.Find<Comment>(findCommentFilter).FirstOrDefaultAsync();
    if (commentFound is null) return Result.Failure($"Comment with id {commentId} has not been found to be deleted");

    using var session = await _mongoClient.StartSessionAsync();
    session.StartTransaction();

    try {

      await _commentCollection.DeleteOneAsync(session, findCommentFilter);
      
      await _pastCommentCollection.InsertOneAsync(session, commentFound.ToPastComment());
      
      await session.CommitTransactionAsync();
      
    } catch(MongoException e) {
      
      await session.AbortTransactionAsync();
      return Result.Failure(e.ToString());
    }

    return Result.Success();
  }

  public async Task<Result<Comment>> GetById(string commentId) {
    
    var findCommentFilter = Builders<Comment>.Filter.Eq(c => c.Id, commentId);
    var comment = await _commentCollection.Find(findCommentFilter).FirstOrDefaultAsync();
    if(comment is null) return Result.Failure<Comment>($"Comment with id {commentId} has not been found");
    
    return comment;
  }

  public async Task<Result> Update(Comment editedComment) {
    
    var findCommentFilter = Builders<Comment>.Filter.Eq(c => c.Id, editedComment.Id);
    
    var currentComment = await _commentCollection.Find(findCommentFilter).FirstOrDefaultAsync();
    if(currentComment is null) return Result.Failure($"Comment with id {editedComment.Id} has not been found");

    using var session = await _mongoClient.StartSessionAsync();
    session.StartTransaction();

    try {
      await _commentCollection.ReplaceOneAsync(session, findCommentFilter, editedComment);

      await _pastCommentCollection.InsertOneAsync(session, currentComment.ToPastComment());

      await session.CommitTransactionAsync();

    } catch(MongoException e) {

      await session.AbortTransactionAsync();

      return Result.Failure(e.ToString());
    }

    return Result.Success();
  }
}