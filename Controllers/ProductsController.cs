namespace PharmacyIMS.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private const int PageSize = 8;

        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? searchTerm, int? categoryId, bool lowStockOnly = false, int page = 1)
        {
            var query = _context.Products
                .Include(p => p.Category)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(p =>
                    p.ProductName.Contains(searchTerm) ||
                    p.SKU.Contains(searchTerm));
            }

            if (categoryId.HasValue && categoryId.Value > 0)
            {
                query = query.Where(p => p.CategoryID == categoryId.Value);
            }

            if (lowStockOnly)
            {
                query = query.Where(p => p.StockQuantity <= p.LowStockThreshold);
            }

            int totalItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalItems / (double)PageSize);
            page = page < 1 ? 1 : page;

            var products = await query
                .OrderBy(p => p.ProductName)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            ViewBag.Categories = new SelectList(await _context.Categories.OrderBy(c => c.CategoryName).ToListAsync(), "CategoryID", "CategoryName", categoryId);
            ViewBag.CurrentSearch = searchTerm;
            ViewBag.CurrentCategory = categoryId;
            ViewBag.LowStockOnly = lowStockOnly;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;

            return View(products);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.SupplierProducts).ThenInclude(sp => sp.Supplier)
                .FirstOrDefaultAsync(m => m.ProductID == id);

            if (product == null) return NotFound();

            return View(product);
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = new SelectList(await _context.Categories.OrderBy(c => c.CategoryName).ToListAsync(), "CategoryID", "CategoryName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("SKU,ProductName,Manufacturer,DosageForm,CategoryID,UnitPrice,StockQuantity,LowStockThreshold,ExpiryDate")] Product product)
        {
            if (await _context.Products.AnyAsync(p => p.SKU == product.SKU))
            {
                ModelState.AddModelError(nameof(product.SKU), "رمز الصنف (SKU) مستخدم بالفعل");
            }

            if (ModelState.IsValid)
            {
                _context.Add(product);
                await _context.SaveChangesAsync();
                TempData["ToastMessage"] = "تم إضافة المنتج بنجاح";
                TempData["ToastType"] = "success";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Categories = new SelectList(await _context.Categories.OrderBy(c => c.CategoryName).ToListAsync(), "CategoryID", "CategoryName", product.CategoryID);
            return View(product);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            ViewBag.Categories = new SelectList(await _context.Categories.OrderBy(c => c.CategoryName).ToListAsync(), "CategoryID", "CategoryName", product.CategoryID);
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ProductID,SKU,ProductName,Manufacturer,DosageForm,CategoryID,UnitPrice,StockQuantity,LowStockThreshold,ExpiryDate")] Product product)
        {
            if (id != product.ProductID) return NotFound();

            if (await _context.Products.AnyAsync(p => p.SKU == product.SKU && p.ProductID != product.ProductID))
            {
                ModelState.AddModelError(nameof(product.SKU), "رمز الصنف (SKU) مستخدم بالفعل");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(product);
                    await _context.SaveChangesAsync();
                    TempData["ToastMessage"] = "تم تعديل المنتج بنجاح";
                    TempData["ToastType"] = "success";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.ProductID)) return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Categories = new SelectList(await _context.Categories.OrderBy(c => c.CategoryName).ToListAsync(), "CategoryID", "CategoryName", product.CategoryID);
            return View(product);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.ProductID == id);

            if (product == null) return NotFound();

            return View(product);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products
                .Include(p => p.PurchaseItems)
                .Include(p => p.SaleItems)
                .FirstOrDefaultAsync(p => p.ProductID == id);

            if (product == null) return RedirectToAction(nameof(Index));

            if (product.PurchaseItems.Any() || product.SaleItems.Any())
            {
                TempData["ToastMessage"] = "لا يمكن حذف المنتج لوجود حركات شراء أو بيع مرتبطة به";
                TempData["ToastType"] = "danger";
                return RedirectToAction(nameof(Index));
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            TempData["ToastMessage"] = "تم حذف المنتج بنجاح";
            TempData["ToastType"] = "success";
            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.ProductID == id);
        }
    }
}
