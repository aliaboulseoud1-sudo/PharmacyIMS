using System.Globalization;
using System.Text;
using Microsoft.EntityFrameworkCore;
using PharmacyIMS.Data;

namespace PharmacyIMS.Services
{
    public class PharmacyContextService : IPharmacyContextService
    {
        private readonly ApplicationDbContext _context;

        public PharmacyContextService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<string> BuildContextSummaryAsync()
        {
            var now = DateTime.Now;
            var monthStart = new DateTime(now.Year, now.Month, 1);
            var sb = new StringBuilder();

            var totalProducts = await _context.Products.CountAsync();
            var totalCategories = await _context.Categories.CountAsync();
            var totalSuppliers = await _context.Suppliers.CountAsync();
            var totalStock = await _context.Products.SumAsync(p => (int?)p.StockQuantity) ?? 0;
            var totalSalesRevenue = await _context.Sales.SumAsync(s => (decimal?)s.TotalAmount) ?? 0;
            var totalPurchaseCost = await _context.Purchases.SumAsync(p => (decimal?)p.TotalAmount) ?? 0;

            sb.AppendLine("=== ملخص عام عن حالة الصيدلية (بيانات حية من قاعدة البيانات) ===");
            sb.AppendLine($"إجمالي عدد المنتجات: {totalProducts}");
            sb.AppendLine($"إجمالي عدد الفئات: {totalCategories}");
            sb.AppendLine($"إجمالي عدد الموردين: {totalSuppliers}");
            sb.AppendLine($"إجمالي الكمية الفعلية بالمخزون: {totalStock} وحدة");
            sb.AppendLine($"إجمالي إيرادات المبيعات (كل الفترات): {totalSalesRevenue:N2} جنيه");
            sb.AppendLine($"إجمالي تكلفة المشتريات (كل الفترات): {totalPurchaseCost:N2} جنيه");
            sb.AppendLine();

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

            var topSelling = await _context.SaleItems
                .Include(si => si.Product)
                .Where(si => si.Product != null)
                .GroupBy(si => new { si.ProductID, si.Product!.ProductName })
                .Select(g => new
                {
                    g.Key.ProductName,
                    TotalQty = g.Sum(x => x.Quantity),
                    TotalRevenue = g.Sum(x => x.Quantity * x.UnitPrice)
                })
                .OrderByDescending(x => x.TotalQty)
                .Take(5)
                .ToListAsync();

            sb.AppendLine("=== الأصناف الأكثر مبيعاً (حسب الكمية) ===");
            if (topSelling.Any())
            {
                foreach (var item in topSelling)
                {
                    sb.AppendLine($"- {item.ProductName}: تم بيع {item.TotalQty} وحدة بإجمالي إيراد {item.TotalRevenue:N2} جنيه");
                }
            }
            else
            {
                sb.AppendLine("لا توجد بيانات مبيعات كافية بعد.");
            }
            sb.AppendLine();

            var supplierProducts = await _context.SupplierProducts
                .Include(sp => sp.Supplier)
                .Include(sp => sp.Product)
                .OrderBy(sp => sp.Product!.ProductName)
                .ThenBy(sp => sp.ContractPrice)
                .ToListAsync();

            sb.AppendLine("=== الموردون والمنتجات التي يوردونها (مرتبة تصاعدياً حسب السعر لكل منتج) ===");
            if (supplierProducts.Any())
            {
                foreach (var sp in supplierProducts)
                {
                    sb.AppendLine($"- المنتج: {sp.Product?.ProductName} | المورد: {sp.Supplier?.SupplierName} | سعر التعاقد: {sp.ContractPrice:N2} جنيه | مدة التوريد: {sp.LeadTimeDays} يوم | هاتف المورد: {sp.Supplier?.Phone}");
                }
            }
            else
            {
                sb.AppendLine("لا توجد بيانات ربط بين الموردين والمنتجات بعد.");
            }
            sb.AppendLine();

            var suppliers = await _context.Suppliers.OrderBy(s => s.SupplierName).ToListAsync();
            sb.AppendLine("=== دليل الموردين ===");
            foreach (var s in suppliers)
            {
                sb.AppendLine($"- {s.SupplierName} | المسؤول: {s.ContactName} | هاتف: {s.Phone} | بريد: {s.Email} | العنوان: {s.Address}");
            }
            sb.AppendLine();

            var monthlySales = await _context.Sales
                .Where(s => s.SaleDate >= monthStart)
                .ToListAsync();
            var monthlyPurchases = await _context.Purchases
                .Where(p => p.PurchaseDate >= monthStart)
                .ToListAsync();

            sb.AppendLine($"=== أداء الشهر الحالي ({monthStart:yyyy-MM}) ===");
            sb.AppendLine($"عدد عمليات البيع: {monthlySales.Count} | إجمالي إيراد الشهر: {monthlySales.Sum(s => s.TotalAmount):N2} جنيه");
            sb.AppendLine($"عدد عمليات الشراء: {monthlyPurchases.Count} | إجمالي تكلفة الشراء هذا الشهر: {monthlyPurchases.Sum(p => p.TotalAmount):N2} جنيه");
            sb.AppendLine();

            var recentSales = await _context.Sales.OrderByDescending(s => s.SaleDate).Take(5).ToListAsync();
            var recentPurchases = await _context.Purchases.Include(p => p.Supplier).OrderByDescending(p => p.PurchaseDate).Take(5).ToListAsync();

            sb.AppendLine("=== آخر 5 عمليات بيع ===");
            foreach (var s in recentSales)
            {
                sb.AppendLine($"- بتاريخ {s.SaleDate:yyyy-MM-dd} | العميل: {s.CustomerInfo ?? "غير محدد"} | الإجمالي: {s.TotalAmount:N2} جنيه");
            }
            sb.AppendLine();

            sb.AppendLine("=== آخر 5 عمليات شراء ===");
            foreach (var p in recentPurchases)
            {
                sb.AppendLine($"- بتاريخ {p.PurchaseDate:yyyy-MM-dd} | المورد: {p.Supplier?.SupplierName} | الإجمالي: {p.TotalAmount:N2} جنيه");
            }

            return sb.ToString();
        }
    }
}
