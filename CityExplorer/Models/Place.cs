using SQLite;

namespace CityExplorer.Models;

public class Place
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string CategoryEmoji { get; set; } = string.Empty;
    public bool IsFavorite { get; set; }
}
