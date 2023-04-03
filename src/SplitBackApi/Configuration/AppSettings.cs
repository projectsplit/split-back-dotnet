namespace SplitBackApi.Configuration;

public class AppSettings {

  public const string SectionName = "App";

  public JwtSettings Jwt { get; set; }
  public GoogleSettings Google { get; set; }
  public MongoDbSettings MongoDb { get; set; }
  public OpenAISettings OpenAI { get; set; }
  public string FrontendUrl { get; set; }
}

public class GoogleSettings {

  public string ClientId { get; set; }
  public string ClientSecret { get; set; }

}

public class OpenAISettings {

  public string SecretKey { get; set; }

}

public class JwtSettings {

  public string Issuer { get; set; }
  public string Audience { get; set; }
  public string Key { get; set; }
}

public class MongoDbSettings {

  public string ConnectionString { get; set; }
  public DatabaseSettings Database { get; set; }
}

public class DatabaseSettings {

  public string Name { get; set; }
  public CollectionSettings Collections { get; set; }
}

public class CollectionSettings {

  public string Groups { get; set; }
  public string Expenses { get; set; }
  public string PastExpenses { get; set; }
  public string Transfers { get; set; }
  public string PastTransfers { get; set; }
  public string Comments { get; set; }
  public string PastComments { get; set; }
  public string Users { get; set; }
  public string Invitations { get; set; }
  public string Sessions { get; set; }
}
