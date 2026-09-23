using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmacyIMS.Data;
using PharmacyIMS.Models;
using PharmacyIMS.ViewModels;

namespace PharmacyIMS.Controllers
{
    public class SalesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private const int PageSize = 10;

        public SalesController(ApplicationDbContext context)
        {
            _context = context;
        }

        
        public async Task<IActionResult> Index(string? searchTerm, DateTime? fromDate, DateTime? toDate, int page = 1)
        {
            var query = _context.Sales
                .Include(s => s.SaleItems)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(s => s.CustomerInfo != null && s.CustomerInfo.Contains(searchTerm));
            }

            if (fromDate.HasValue)
            {
                query = query.Where(s => s.SaleDate >= fromDate.Value.Date);
            }

            if (toDate.HasValue)
            {
                var inclusiveEnd = toDate.Value.Date.AddDays(1);
                query = query.Where(s => s.SaleDate < inclusiveEnd);
            }

            int totalItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalItems / (double)PageSize);
            page = page < 1 ? 1 : page;

            var sales = await query
                .OrderByDescending(s => s.SaleDate)
                .ThenByDescending(s => s.SaleID)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            ViewBag.CurrentSearch = searchTerm;
            ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
            ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;

            return View(sales);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var sale = await _context.Sales
                .Include(s => s.SaleItems)
                    .ThenInclude(si => si.Product)
                .FirstOrDefaultAsync(s => s.SaleID == id);

            if (sale == null) return NotFound();

            return View(sale);
        }

        public async Task<IActionResult> Create()
        {
            await PopulateProductsViewBagAsync();
            return View(new CreateSaleViewModel { SaleDate = DateTime.Now });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateSaleViewModel model)
        {
            model.Items = model.Items?.Where(i => i.ProductID > 0 && i.Quantity > 0).ToList() ?? new List<SaleItemInputModel>();

            ModelState.Clear();
            TryValidateModel(model);

            if (!model.Items.Any())
            {
                ModelState.AddModelError(string.Empty, "يجب إضافة صنف واحد على الأقل للفاتورة");
            }

            if (!ModelState.IsValid)
            {
                await PopulateProductsViewBagAsync();
                return View(model);
            }

            var groupedItems = model.Items
                .GroupBy(i => i.ProductID)
                .Select(g => new { ProductID = g.Key, Quantity = g.Sum(x => x.Quantity) })
                .ToList();

            var productIds = groupedItems.Select(g => g.ProductID).ToList();

            var products = await _context.Products
                .Where(p => productIds.Contains(p.ProductID))
                .ToListAsync();

            bool hasError = false;

            foreach (var item in groupedItems)
            {
                var product = products.FirstOrDefault(p => p.ProductID == item.ProductID);
                if (product == null)
                {
                    ModelState.AddModelError(string.Empty, "أحد المنتجات المختارة لم يعد موجودًا في النظام");
                    hasError = true;
                    continue;
                }

                if (item.Quantity > product.StockQuantity)
                {
                    ModelState.AddModelError(string.Empty,
                        $"الكمية المتاحة من {product.ProductName} هي {product.StockQuantity} فقط");
                    hasError = true;
                }
            }

            if (hasError)
            {
                await PopulateProductsViewBagAsync();
                return View(model);
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var sale = new Sale
                {
                    SaleDate = model.SaleDate == default ? DateTime.Now : model.SaleDate,
                    CustomerInfo = model.CustomerInfo,
                    TotalAmount = 0m
                };

                decimal total = 0m;

                foreach (var item in groupedItems)
                {
                    var product = products.First(p => p.ProductID == item.ProductID);

                    if (item.Quantity > product.StockQuantity)
                    {
                        throw new InvalidOperationException(
                            $"الكمية المتاحة من {product.ProductName} هي {product.StockQuantity} فقط");
                    }

                    var saleItem = new SaleItem
                    {
                        ProductID = product.ProductID,
                        Quantity = item.Quantity,
                        UnitPrice = product.UnitPrice 
                    };

                    sale.SaleItems.Add(saleItem);
                    total += item.Quantity * product.UnitPrice;

                    product.StockQuantity -= item.Quantity;
                }

                sale.TotalAmount = total;

                _context.Sales.Add(sale);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["ToastMessage"] = "تمت عملية البيع بنجاح وتم تحديث المخزون";
                TempData["ToastType"] = "success";

                return RedirectToAction(nameof(Details), new { id = sale.SaleID });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                ModelState.AddModelError(string.Empty,
                    ex is InvalidOperationException ? ex.Message : "حدث خطأ أثناء تنفيذ عملية البيع، الرجاء المحاولة مرة أخرى");
                await PopulateProductsViewBagAsync();
                return View(model);
            }
        }

        private async Task PopulateProductsViewBagAsync()
        {
            var products = await _context.Products
                .OrderBy(p => p.ProductName)
                .Select(p => new ProductPickerItem
                {
                    ProductID = p.ProductID,
                    ProductName = p.ProductName,
                    SKU = p.SKU,
                    UnitPrice = p.UnitPrice,
                    StockQuantity = p.StockQuantity
                })
                .ToListAsync();

            ViewBag.Products = products;
            ViewBag.ProductsJson = JsonSerializer.Serialize(products, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
        }
    }
}
