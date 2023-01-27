namespace SplitBackApi.Domain;

public class Label : EntityBase {

  public string Text { get; set; } = string.Empty;

  public string Color { get; set; } = string.Empty;
}
