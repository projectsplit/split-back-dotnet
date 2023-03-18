namespace SplitBackApi.Api.Endpoints.OpenAI.Requests;

public class OpenAIUserInputRequest {

  public string GroupId { get; set; }
  public string Content { get; set; }
}

public class OpenAIChatRequest {

  public string model { get; set; }

  public Message[] messages { get; set; }

}

public class Message {

  public string role { get; set; }

  public string content { get; set; }
}