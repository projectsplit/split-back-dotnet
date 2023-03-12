namespace SplitBackApi.Api.Endpoints.OpenAI.Requests;

public class OpenAIModelRequest {

  public string model { get; set; }
  public string prompt { get; set; }
  public int max_tokens { get; set; }
  public double temperature { get; set; }
  public double top_p { get; set; }
}