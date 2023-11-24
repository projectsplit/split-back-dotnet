namespace SplitBackApi.Api.Models;
public class Choice {
  public Message Message { get; set; }
  public string Text { get; set; }
  public int Index { get; set; }
  public object Logprobs { get; set; }
  public string Finish_reason { get; set; }
}
