namespace ToolLogix.Models
{
    public class InventoryHistoryViewModel
    {
        public int SpareItemId { get; set; }

        public string DetailNumber { get; set; } = string.Empty;

        public string? Description { get; set; }

        public int CurrentQuantity { get; set; }

        public List<InventoryTransactionRowViewModel> Transactions
        {
            get;
            set;
        } = new();
    }

    public class InventoryTransactionRowViewModel
    {
        public int Id { get; set; }

        public string AdjustmentType { get; set; } = string.Empty;

        public int Quantity { get; set; }

        public int PreviousQuantity { get; set; }

        public int NewQuantity { get; set; }

        public string? Reason { get; set; }

        public string AdjustedByName { get; set; } = string.Empty;

        public DateTime AdjustedAt { get; set; }
    }
}