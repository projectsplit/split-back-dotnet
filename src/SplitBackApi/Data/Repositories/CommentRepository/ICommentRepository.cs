using CSharpFunctionalExtensions;
using SplitBackApi.Domain.Models;

namespace SplitBackApi.Data.Repositories.CommentRepository;

public interface ICommentRepository {

  Task Create(Comment comment);
  Task<Result<Comment>> GetById(string commentId);
  Task<Result> Update(Comment editedComment);
  Task<Result> DeleteById(string commentId);
}