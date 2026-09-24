using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PharmacyIMS.Data;
using PharmacyIMS.Models;
using PharmacyIMS.ViewModels;

namespace PharmacyIMS.Controllers
{
    public class PurchasesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private const int PageSize = 10;

        public PurchasesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Purchases
        public async Task<IActionResult> Index(string? searchTerm, DateTime? fromDate, DateTime? toDate, int page = 1)
        {
            var query = _context.Purchases
                .Include(p => p.Supplier)
                .Include(p => p.PurchaseItems)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(p => p.Supplier != null && p.Supplier.SupplierName.Contains(searchTerm));
            }

            if (fromDate.HasValue)
            {
                query = query.Where(p => p.PurchaseDate >= fromDate.Value.Date);
            }

            if (toDate.HasValue)
            {
                var inclusiveEnd = toDate.Value.Date.AddDays(1);
                query = query.Where(p => p.PurchaseDate < inclusiveEnd);
            }

            int totalItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalItems / (double)PageSize);
            page = page < 1 ? 1 : page;

            var purchases = await query
                .OrderByDescending(p => p.PurchaseDate)
                .ThenByDescending(p => p.PurchaseID)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            ViewBag.CurrentSearch = searchTerm;
            ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
            ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;

            return View(purchases);
        }

        // GET: Purchases/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var purchase = await _context.Purchases
                .Include(p => p.Supplier)
                .Include(p => p.PurchaseItems)
                    .ThenInclude(pi => pi.Product)
                .FirstOrDefaultAsync(p => p.PurchaseID == id);

            if (purchase == null) return NotFound();

            return View(purchase);
        }

        // GET: Purchases/Create
        public async Task<IActionResult> Create()
        {
            await PopulateLookupsViewBagAsync();
            return View(new CreatePurchaseViewModel { PurchaseDate = DateTime.Now });
        }

        // POST: Purchases/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreatePurchaseViewModel model)
        {
            // Drop empty/incomplete rows the same way SalesController does
            model.Items = model.Items?
                .Where(i => i.ProductID > 0 && i.Quantity > 0 && i.UnitCost > 0)
                .ToList() ?? new List<PurchaseItemInputModel>();

            ModelState.Clear();
            TryValidateModel(model);

            if (!model.Items.Any())
            {
                ModelState.AddModelError(string.Empty, "يجب إضافة صنف واحد على الأقل لطلب الشراء");
            }

            var supplierExists = model.SupplierID > 0 &&
                await _context.Suppliers.AnyAsync(s => s.SupplierID == model.SupplierID);

            if (!supplierExists)
            {
                ModelState.AddModelError(nameof(model.SupplierID), "يجب اختيار مورد صحيح");
            }

            if (!ModelState.IsValid)
            {
                await PopulateLookupsViewBagAsync();
                return View(model);
            }

            // Group by product in case the same product was added in more than one row
            var groupedItems = model.Items
                .GroupBy(i => i.ProductID)
                .Select(g => new
                {
                    ProductID = g.Key,
                    Quantity = g.Sum(x => x.Quantity),
                    // Weighted average unit cost if the same product appears more than once
                    UnitCost = Math.Round(g.Sum(x => x.Quantity * x.UnitCost) / g.Sum(x => x.Quantity), 2)
                })
                .ToList();

            var productIds = groupedItems.Select(g => g.ProductID).ToList();

            var products = await _context.Products
                .Where(p => productIds.Contains(p.ProductID))
                .ToListAsync();

            if (products.Count != productIds.Count)
            {
                ModelState.AddModelError(string.Empty, "أحد المنتجات المختارة لم يعد موجودًا في النظام");
                await PopulateLookupsViewBagAsync();
                return View(model);
            }

            // Database transaction: guarantees the purchase header, its line items and
            // the stock increase on every product either all succeed or all roll back.
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var purchase = new Purchase
                {
                    SupplierID = model.SupplierID,
                    PurchaseDate = model.PurchaseDate == default ? DateTime.Now : model.PurchaseDate,
                    TotalAmount = 0m
                };

                decimal total = 0m;

                foreach (var item in groupedItems)
                {
                    var product = products.First(p => p.ProductID == item.ProductID);

                    var purchaseItem = new PurchaseItem
                    {
                        ProductID = product.ProductID,
                        Quantity = item.Quantity,
                        UnitCost = item.UnitCost
                    };

                    purchase.PurchaseItems.Add(purchaseItem);
                    total += item.Quantity * item.UnitCost;

                    // Core business rule: incoming purchase quantity increases stock
                    product.StockQuantity += item.Quantity;
                }

                purchase.TotalAmount = total;

                _context.Purchases.Add(purchase);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["ToastMessage"] = "تم تسجيل عملية الشراء بنجاح وتم تحديث المخزون";
                TempData["ToastType"] = "success";

                return RedirectToAction(nameof(Details), new { id = purchase.PurchaseID });
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                ModelState.AddModelError(string.Empty,
                    "حدث خطأ أثناء تسجيل عملية الشراء، الرجاء المحاولة مرة أخرى");
                await PopulateLookupsViewBagAsync();
                return View(model);
            }
        }

        private async Task PopulateLookupsViewBagAsync()
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

            var suppliers = await _context.Suppliers
                .OrderBy(s => s.SupplierName)
                .Select(s => new SupplierPickerItem
                {
                    SupplierID = s.SupplierID,
                    SupplierName = s.SupplierName
                })
                .ToListAsync();

            ViewBag.Products = products;
            ViewBag.ProductsJson = JsonSerializer.Serialize(products, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            ViewBag.Suppliers = new SelectList(suppliers, "SupplierID", "SupplierName");
        }
    }
}
