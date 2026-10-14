namespace ToolLogix.Models
{
    public class AssetSearchViewModel
    {
        public string SearchTerm { get; set; } = string.Empty;

        public List<Asset> Results { get; set; } = new();

        public bool HasSearched { get; set; }
    }
}