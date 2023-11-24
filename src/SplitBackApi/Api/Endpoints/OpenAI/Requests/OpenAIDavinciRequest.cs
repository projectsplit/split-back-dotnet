namespace SplitBackApi.Api.Endpoints.OpenAI.Requests;

public class OpenAIDavinciRequest {

  public string Model { get; set; }
  public string Prompt { get; set; }
  public int Max_tokens { get; set; }
  public double Temperature { get; set; }
  public double Top_p { get; set; }
  public double Presence_penalty { get; set; }
  public double Frequency_penalty { get; set; }
  public int Best_of{ get; set; }
}