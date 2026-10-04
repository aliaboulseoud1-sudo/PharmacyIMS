namespace PharmacyIMS.Controllers
{
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        private static readonly string[] ArabicMonths =
        {
            "يناير", "فبراير", "مارس", "أبريل", "مايو", "يونيو",
            "يوليو", "أغسطس", "سبتمبر", "أكتوبر", "نوفمبر", "ديسمبر"
        };

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var now = DateTime.Now;
            var vm = new DashboardViewModels();

            vm.TotalProducts = await _context.Products.CountAsync();
            vm.TotalCategories = await _context.Categories.CountAsync();
            vm.TotalSuppliers = await _context.Suppliers.CountAsync();
            vm.TotalStockQuantity = await _context.Products.SumAsync(p => (int?)p.StockQuantity) ?? 0;
            vm.TotalSalesRevenue = await _context.Sales.SumAsync(s => (decimal?)s.TotalAmount) ?? 0;
            vm.TotalPurchaseCost = await _context.Purchases.SumAsync(p => (decimal?)p.TotalAmount) ?? 0;

            vm.LowStockProducts = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.StockQuantity <= p.LowStockThreshold)
                .OrderBy(p => p.StockQuantity)
                .ToListAsync();
            vm.LowStockCount = vm.LowStockProducts.Count;

            vm.TopSellingProducts = await _context.SaleItems
                .Include(si => si.Product)
                .Where(si => si.Product != null)
                .GroupBy(si => new { si.ProductID, si.Product!.ProductName })
                .Select(g => new TopSellingProductDto
                {
                    ProductName = g.Key.ProductName,
                    TotalQuantitySold = g.Sum(x => x.Quantity),
                    TotalRevenue = g.Sum(x => x.Quantity * x.UnitPrice)
                })
                .OrderByDescending(x => x.TotalQuantitySold)
                .Take(5)
                .ToListAsync();

            for (int i = 5; i >= 0; i--)
            {
                var monthDate = now.AddMonths(-i);
                var monthStart = new DateTime(monthDate.Year, monthDate.Month, 1);
                var monthEnd = monthStart.AddMonths(1);

                var salesTotal = await _context.Sales
                    .Where(s => s.SaleDate >= monthStart && s.SaleDate < monthEnd)
                    .SumAsync(s => (decimal?)s.TotalAmount) ?? 0;

                var purchasesTotal = await _context.Purchases
                    .Where(p => p.PurchaseDate >= monthStart && p.PurchaseDate < monthEnd)
                    .SumAsync(p => (decimal?)p.TotalAmount) ?? 0;

                vm.MonthLabels.Add($"{ArabicMonths[monthDate.Month - 1]} {monthDate.Year}");
                vm.MonthlySales.Add(salesTotal);
                vm.MonthlyPurchases.Add(purchasesTotal);
            }

            var recentSales = await _context.Sales
                .OrderByDescending(s => s.SaleDate)
                .Take(5)
                .Select(s => new ActivityLogItem
                {
                    Date = s.SaleDate,
                    Type = "بيع",
                    Description = $"عملية بيع {(string.IsNullOrEmpty(s.CustomerInfo) ? "" : $"للعميل: {s.CustomerInfo}")}",
                    Amount = s.TotalAmount,
                    IconClass = "bi-receipt",
                    BadgeClass = "bg-success"
                })
                .ToListAsync();

            var recentPurchases = await _context.Purchases
                .Include(p => p.Supplier)
                .OrderByDescending(p => p.PurchaseDate)
                .Take(5)
                .Select(p => new ActivityLogItem
                {
                    Date = p.PurchaseDate,
                    Type = "شراء",
                    Description = $"توريد من المورد: {(p.Supplier != null ? p.Supplier.SupplierName : "غير محدد")}",
                    Amount = p.TotalAmount,
                    IconClass = "bi-cart-plus",
                    BadgeClass = "bg-primary"
                })
                .ToListAsync();

            vm.RecentActivity = recentSales
                .Concat(recentPurchases)
                .OrderByDescending(a => a.Date)
                .Take(10)
                .ToList();

            return View(vm);
        }
    }
}
