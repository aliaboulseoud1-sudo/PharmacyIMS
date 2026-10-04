namespace PharmacyIMS.viewModels
{
    public class DashboardViewModels
    {
        public int TotalProducts { get; set; }
        public int TotalCategories { get; set; }
        public int TotalSuppliers { get; set; }
        public int TotalStockQuantity { get; set; }
        public int LowStockCount { get; set; }
        public decimal TotalSalesRevenue { get; set; }
        public decimal TotalPurchaseCost { get; set; }

        public List<Product> LowStockProducts { get; set; } = new();

        public List<TopSellingProductDto> TopSellingProducts { get; set; } = new();

        public List<string> MonthLabels { get; set; } = new();
        public List<decimal> MonthlySales { get; set; } = new();
        public List<decimal> MonthlyPurchases { get; set; } = new();

        public List<ActivityLogItem> RecentActivity { get; set; } = new();
    }

    public class TopSellingProductDto
    {
        public string ProductName { get; set; } = string.Empty;
        public int TotalQuantitySold { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class ActivityLogItem
    {
        public DateTime Date { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string IconClass { get; set; } = "bi-receipt";
        public string BadgeClass { get; set; } = "bg-success";
    }
}
