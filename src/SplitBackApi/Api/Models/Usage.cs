namespace SplitBackApi.Api.Models;
public class Usage {
  public int Prompt_tokens { get; set; }
  public int Completion_tokens { get; set; }
  public int Total_tokens { get; set; }
}