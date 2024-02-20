namespace SplitBackApi.Api.Models;
public class Choice {
  public Message Message { get; set; }
  public string Text { get; set; }
  public int Index { get; set; }
  public int? Logprobs { get; set; }
  public string FinishReason { get; set; }
}
