namespace SplitBackApi.Api.Endpoints.OpenAI.Response;
public class OpenAIExplanatorResponse {
  public string Id { get; set; }
  public string Object { get; set; }
  public long Created { get; set; }
  public string Model { get; set; }
  public Choice[] Choices { get; set; }
  public Usage Usage { get; set; }
}



