namespace SplitBackApi.Configuration;

public class AppSettings {

  public const string SectionName = "App";

  public JwtSettings Jwt { get; set; } = null!;
  public MongoDbSettings MongoDb { get; set; } = null!;
}

public class JwtSettings {

  public string Issuer { get; set; } = String.Empty;
  public string Audience { get; set; } = String.Empty;
  public string Key { get; set; } = String.Empty;
}

public class MongoDbSettings {

  public string ConnectionString { get; set; } = null!;
  public DatabaseSettings Database { get; set; } = null!;
}

public class DatabaseSettings {

  public string Name { get; set; } = String.Empty;
  public CollectionSettings Collections { get; set; } = null!;
}

public class CollectionSettings {

  public string Groups { get; set; } = String.Empty;
  public string Expenses { get; set; } = String.Empty;
  public string PastExpenses { get; set; } = String.Empty;
  public string Transfers { get; set; } = String.Empty;
  public string PastTransfers { get; set; } = String.Empty;
  public string Comments { get; set; } = String.Empty;
  public string PastComments { get; set; } = String.Empty;
  public string Users { get; set; } = String.Empty;
  public string Invitations { get; set; } = String.Empty;
  public string Sessions { get; set; } = String.Empty;
}
