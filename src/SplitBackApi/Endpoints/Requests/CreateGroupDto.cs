namespace SplitBackApi.Endpoints.Requests;
using SplitBackApi.Domain;
public class CreateGroupDto
{
    public string Title { get; set; } = null!;
    public ICollection<Label>? GroupLabels { get; set; }
}