public class Choice {
  public Message Message { get; set; }
  public string text { get; set; }
  public int index { get; set; }
  public object logprobs { get; set; }
  public string finish_reason { get; set; }
}
