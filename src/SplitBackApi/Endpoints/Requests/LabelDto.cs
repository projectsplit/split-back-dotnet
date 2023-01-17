using System.ComponentModel.DataAnnotations;
namespace SplitBackApi.Endpoints.Requests;
  public class LabelDto
  {
    [MaxLength(30)]
    public string Name { get; set; } = null!;
  }