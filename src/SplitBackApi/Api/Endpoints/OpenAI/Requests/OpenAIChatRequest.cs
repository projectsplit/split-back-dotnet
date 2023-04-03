namespace SplitBackApi.Api.Endpoints.OpenAI.Requests;


public class OpenAIChatRequest {

  public string model { get; set; }

  public Message[] messages { get; set; }

}
