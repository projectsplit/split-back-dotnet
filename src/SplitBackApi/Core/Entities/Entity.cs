namespace SplitBackApi.Entities;

public class Entity<TId> {
  
  public TId Id { get; set; } = default!;
  
  public DateTime CreationTime { get; set; } = DateTime.UtcNow;
  
  public DateTime LastUpdateTime { get; set; } = DateTime.UtcNow;
}
