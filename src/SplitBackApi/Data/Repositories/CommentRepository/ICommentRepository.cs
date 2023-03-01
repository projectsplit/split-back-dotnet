using SplitBackApi.Domain;
using CSharpFunctionalExtensions;

namespace SplitBackApi.Data;

public interface ICommentRepository {

  Task Create(Comment comment);
  Task<Result<Comment>> GetById(string commentId);
  Task<Result> Update(Comment editedComment);
  Task<Result> DeleteById(string commentId);
}