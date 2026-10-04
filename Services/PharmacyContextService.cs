namespace PharmacyIMS.Services
{
    public class PharmacyContextService : IPharmacyContextService
    {
        private const string FinancialAccessRole = "Admin";

        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PharmacyContextService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<string> BuildContextSummaryAsync()
        {
            var includeFinancials = CanViewFinancials();

            var now = DateTime.Now;
            var monthStart = new DateTime(now.Year, now.Month, 1);
            var sb = new StringBuilder();

            if (!includeFinancials)
            {
                AppendRestrictedAccessNotice(sb);
            }

            await AppendGeneralSummaryAsync(sb, includeFinancials);
            await AppendLowStockAsync(sb);
            await AppendTopSellingAsync(sb, includeFinancials);
            await AppendSupplierProductsAsync(sb, includeFinancials);
            await AppendSupplierDirectoryAsync(sb);
            await AppendMonthlyPerformanceAsync(sb, monthStart, includeFinancials);
            await AppendRecentSalesAsync(sb, includeFinancials);

            if (includeFinancials)
            {
                await AppendRecentPurchasesAsync(sb);
            }

            return sb.ToString();
        }

        private bool CanViewFinancials() =>
            _httpContextAccessor.HttpContext?.User?.IsInRole(FinancialAccessRole) == true;

        private static void AppendRestrictedAccessNotice(StringBuilder sb)
        {
            sb.AppendLine("=== تنبيه صلاحيات الوصول ===");
            sb.AppendLine("صلاحيات المستخدم الحالي تقتصر على البيانات التشغيلية للمخزون (المنتجات، المخزون، الموردين، وحركة المبيعات من حيث العدد والكميات).");
            sb.AppendLine("البيانات المالية (إيرادات المبيعات، تكاليف المشتريات، أسعار التعاقد مع الموردين، الأرباح) غير متاحة لهذا المستخدم ولا تتوفر لديك أصلاً.");
            sb.AppendLine("إذا سأل عنها، فاعتذر بلطف وأخبره أنها متاحة لمدير النظام فقط، ولا تحاول تقديرها أو استنتاجها.");
            sb.AppendLine();
        }

        private async Task AppendGeneralSummaryAsync(StringBuilder sb, bool includeFinancials)
        {
            var totalProducts = await _context.Products.CountAsync();
            var totalCategories = await _context.Categories.CountAsync();
            var totalSuppliers = await _context.Suppliers.CountAsync();
            var totalStock = await _context.Products.SumAsync(p => (int?)p.StockQuantity) ?? 0;

            sb.AppendLine("=== ملخص عام عن حالة الصيدلية (بيانات حية من قاعدة البيانات) ===");
            sb.AppendLine($"إجمالي عدد المنتجات: {totalProducts}");
            sb.AppendLine($"إجمالي عدد الفئات: {totalCategories}");
            sb.AppendLine($"إجمالي عدد الموردين: {totalSuppliers}");
            sb.AppendLine($"إجمالي الكمية الفعلية بالمخزون: {totalStock} وحدة");

            if (includeFinancials)
            {
                var totalSalesRevenue = await _context.Sales.SumAsync(s => (decimal?)s.TotalAmount) ?? 0;
                var totalPurchaseCost = await _context.Purchases.SumAsync(p => (decimal?)p.TotalAmount) ?? 0;

                sb.AppendLine($"إجمالي إيرادات المبيعات (كل الفترات): {totalSalesRevenue:N2} جنيه");
                sb.AppendLine($"إجمالي تكلفة المشتريات (كل الفترات): {totalPurchaseCost:N2} جنيه");
            }

            sb.AppendLine();
        }

        private async Task AppendLowStockAsync(StringBuilder sb)
        {
            var lowStockProducts = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.StockQuantity <= p.LowStockThreshold)
                .OrderBy(p => p.StockQuantity)
                .ToListAsync();

            sb.AppendLine($"=== الأصناف التي أوشكت على النفاد أو تحتاج إعادة طلب (العدد: {lowStockProducts.Count}) ===");
            if (lowStockProducts.Any())
            {
                foreach (var p in lowStockProducts)
                {
                    sb.AppendLine($"- {p.ProductName} (SKU: {p.SKU}) | الفئة: {p.Category?.CategoryName} | الكمية المتبقية: {p.StockQuantity} | حد إعادة الطلب: {p.LowStockThreshold}");
                }
            }
            else
            {
                sb.AppendLine("لا توجد حالياً أصناف منخفضة المخزون.");
            }
            sb.AppendLine();
        }

        private async Task AppendTopSellingAsync(StringBuilder sb, bool includeFinancials)
        {
            var groupedByProduct = _context.SaleItems
                .Where(si => si.Product != null)
                .GroupBy(si => new { si.ProductID, si.Product!.ProductName });

            List<string> lines;

            if (includeFinancials)
            {
                var topSelling = await groupedByProduct
                    .Select(g => new
                    {
                        g.Key.ProductName,
                        TotalQty = g.Sum(x => x.Quantity),
                        TotalRevenue = g.Sum(x => x.Quantity * x.UnitPrice)
                    })
                    .OrderByDescending(x => x.TotalQty)
                    .Take(5)
                    .ToListAsync();

                lines = topSelling
                    .Select(item => $"- {item.ProductName}: تم بيع {item.TotalQty} وحدة بإجمالي إيراد {item.TotalRevenue:N2} جنيه")
                    .ToList();
            }
            else
            {
                var topSelling = await groupedByProduct
                    .Select(g => new
                    {
                        g.Key.ProductName,
                        TotalQty = g.Sum(x => x.Quantity)
                    })
                    .OrderByDescending(x => x.TotalQty)
                    .Take(5)
                    .ToListAsync();

                lines = topSelling
                    .Select(item => $"- {item.ProductName}: تم بيع {item.TotalQty} وحدة")
                    .ToList();
            }

            sb.AppendLine("=== الأصناف الأكثر مبيعاً (حسب الكمية) ===");
            AppendLinesOrFallback(sb, lines, "لا توجد بيانات مبيعات كافية بعد.");
            sb.AppendLine();
        }

        private async Task AppendSupplierProductsAsync(StringBuilder sb, bool includeFinancials)
        {
            List<string> lines;

            if (includeFinancials)
            {
                var rows = await _context.SupplierProducts
                    .OrderBy(sp => sp.Product!.ProductName)
                    .ThenBy(sp => sp.ContractPrice)
                    .Select(sp => new
                    {
                        ProductName = sp.Product!.ProductName,
                        SupplierName = sp.Supplier!.SupplierName,
                        SupplierPhone = sp.Supplier!.Phone,
                        sp.ContractPrice,
                        sp.LeadTimeDays
                    })
                    .ToListAsync();

                lines = rows
                    .Select(sp => $"- المنتج: {sp.ProductName} | المورد: {sp.SupplierName} | سعر التعاقد: {sp.ContractPrice:N2} جنيه | مدة التوريد: {sp.LeadTimeDays} يوم | هاتف المورد: {sp.SupplierPhone}")
                    .ToList();

                sb.AppendLine("=== الموردون والمنتجات التي يوردونها (مرتبة تصاعدياً حسب السعر لكل منتج) ===");
            }
            else
            {
                var rows = await _context.SupplierProducts
                    .OrderBy(sp => sp.Product!.ProductName)
                    .ThenBy(sp => sp.Supplier!.SupplierName)
                    .Select(sp => new
                    {
                        ProductName = sp.Product!.ProductName,
                        SupplierName = sp.Supplier!.SupplierName,
                        SupplierPhone = sp.Supplier!.Phone,
                        sp.LeadTimeDays
                    })
                    .ToListAsync();

                lines = rows
                    .Select(sp => $"- المنتج: {sp.ProductName} | المورد: {sp.SupplierName} | مدة التوريد: {sp.LeadTimeDays} يوم | هاتف المورد: {sp.SupplierPhone}")
                    .ToList();

                sb.AppendLine("=== الموردون والمنتجات التي يوردونها ===");
            }

            AppendLinesOrFallback(sb, lines, "لا توجد بيانات ربط بين الموردين والمنتجات بعد.");
            sb.AppendLine();
        }

        private async Task AppendSupplierDirectoryAsync(StringBuilder sb)
        {
            var suppliers = await _context.Suppliers.OrderBy(s => s.SupplierName).ToListAsync();

            sb.AppendLine("=== دليل الموردين ===");
            foreach (var s in suppliers)
            {
                sb.AppendLine($"- {s.SupplierName} | المسؤول: {s.ContactName} | هاتف: {s.Phone} | بريد: {s.Email} | العنوان: {s.Address}");
            }
            sb.AppendLine();
        }

        private async Task AppendMonthlyPerformanceAsync(StringBuilder sb, DateTime monthStart, bool includeFinancials)
        {
            var salesCount = await _context.Sales.CountAsync(s => s.SaleDate >= monthStart);

            sb.AppendLine($"=== أداء الشهر الحالي ({monthStart:yyyy-MM}) ===");

            if (includeFinancials)
            {
                var salesTotal = await _context.Sales
                    .Where(s => s.SaleDate >= monthStart)
                    .SumAsync(s => (decimal?)s.TotalAmount) ?? 0;

                var purchasesCount = await _context.Purchases.CountAsync(p => p.PurchaseDate >= monthStart);
                var purchasesTotal = await _context.Purchases
                    .Where(p => p.PurchaseDate >= monthStart)
                    .SumAsync(p => (decimal?)p.TotalAmount) ?? 0;

                sb.AppendLine($"عدد عمليات البيع: {salesCount} | إجمالي إيراد الشهر: {salesTotal:N2} جنيه");
                sb.AppendLine($"عدد عمليات الشراء: {purchasesCount} | إجمالي تكلفة الشراء هذا الشهر: {purchasesTotal:N2} جنيه");
            }
            else
            {
                sb.AppendLine($"عدد عمليات البيع: {salesCount}");
            }

            sb.AppendLine();
        }

        private async Task AppendRecentSalesAsync(StringBuilder sb, bool includeFinancials)
        {
            sb.AppendLine("=== آخر 5 عمليات بيع ===");

            if (includeFinancials)
            {
                var recentSales = await _context.Sales
                    .OrderByDescending(s => s.SaleDate)
                    .Take(5)
                    .Select(s => new { s.SaleDate, s.CustomerInfo, s.TotalAmount })
                    .ToListAsync();

                foreach (var s in recentSales)
                {
                    sb.AppendLine($"- بتاريخ {s.SaleDate:yyyy-MM-dd} | العميل: {s.CustomerInfo ?? "غير محدد"} | الإجمالي: {s.TotalAmount:N2} جنيه");
                }
            }
            else
            {
                var recentSales = await _context.Sales
                    .OrderByDescending(s => s.SaleDate)
                    .Take(5)
                    .Select(s => new { s.SaleDate, s.CustomerInfo })
                    .ToListAsync();

                foreach (var s in recentSales)
                {
                    sb.AppendLine($"- بتاريخ {s.SaleDate:yyyy-MM-dd} | العميل: {s.CustomerInfo ?? "غير محدد"}");
                }
            }

            sb.AppendLine();
        }

        private async Task AppendRecentPurchasesAsync(StringBuilder sb)
        {
            var recentPurchases = await _context.Purchases
                .OrderByDescending(p => p.PurchaseDate)
                .Take(5)
                .Select(p => new
                {
                    p.PurchaseDate,
                    SupplierName = p.Supplier != null ? p.Supplier.SupplierName : null,
                    p.TotalAmount
                })
                .ToListAsync();

            sb.AppendLine("=== آخر 5 عمليات شراء ===");
            foreach (var p in recentPurchases)
            {
                sb.AppendLine($"- بتاريخ {p.PurchaseDate:yyyy-MM-dd} | المورد: {p.SupplierName} | الإجمالي: {p.TotalAmount:N2} جنيه");
            }
        }

        private static void AppendLinesOrFallback(StringBuilder sb, List<string> lines, string fallback)
        {
            if (lines.Count == 0)
            {
                sb.AppendLine(fallback);
                return;
            }

            foreach (var line in lines)
            {
                sb.AppendLine(line);
            }
        }
    }
}
