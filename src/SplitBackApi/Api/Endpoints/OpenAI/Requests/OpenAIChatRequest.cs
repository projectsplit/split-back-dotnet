using SplitBackApi.Api.Models;

namespace SplitBackApi.Api.Endpoints.OpenAI.Requests;


public class OpenAIChatRequest {

  public string Model { get; set; }

  public Message[] Messages { get; set; }

}
