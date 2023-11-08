namespace SplitBackApi.Api.Endpoints.OpenAI.Response;
public class OpenAIChatCompletionResponse {
  public string Id { get; set; }
  public string Object { get; set; }
  public long Created { get; set; }
  public string Model { get; set; }
  public Usage Usage { get; set; }
  public List<Choice> Choices { get; set; }
  public Message Content { get; set; }
}