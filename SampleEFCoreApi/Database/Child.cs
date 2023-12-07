namespace SampleEFCoreApi.Database;

public class Child
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Region_code { get; set; }

    public int? PartnerId { get; set; }
    public virtual Child Partner { get; set; }

    public virtual ICollection<GrandChild> Children { get; set; }

    public int? FavoriteId { get; set; }
    public virtual GrandChild Favorite { get; set; }
}
