namespace PharmacyIMS.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? searchTerm)
        {
            var categories = _context.Categories
                .Include(c => c.Products)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                categories = categories.Where(c => c.CategoryName.Contains(searchTerm));
                ViewData["CurrentFilter"] = searchTerm;
            }

            return View(await categories.OrderBy(c => c.CategoryName).ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var category = await _context.Categories
                .Include(c => c.Products)
                .FirstOrDefaultAsync(m => m.CategoryID == id);

            if (category == null) return NotFound();

            return View(category);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CategoryName,Description")] Category category)
        {
            if (ModelState.IsValid)
            {
                _context.Categories.Add(category);
                await _context.SaveChangesAsync();
                TempData["ToastMessage"] = "تم إضافة الفئة بنجاح";
                TempData["ToastType"] = "success";
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var category = await _context.Categories.FindAsync(id);
            if (category == null) return NotFound();

            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CategoryID,CategoryName,Description")] Category category)
        {
            if (id != category.CategoryID) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Categories.Update(category);
                    await _context.SaveChangesAsync();
                    TempData["ToastMessage"] = "تم تعديل الفئة بنجاح";
                    TempData["ToastType"] = "success";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(category.CategoryID)) return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var category = await _context.Categories
                .Include(c => c.Products)
                .FirstOrDefaultAsync(m => m.CategoryID == id);

            if (category == null) return NotFound();

            return View(category);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _context.Categories
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.CategoryID == id);

            if (category == null) return RedirectToAction(nameof(Index));

            if (category.Products.Any())
            {
                TempData["ToastMessage"] = "لا يمكن حذف الفئة لوجود منتجات مرتبطة بها";
                TempData["ToastType"] = "danger";
                return RedirectToAction(nameof(Index));
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            TempData["ToastMessage"] = "تم حذف الفئة بنجاح";
            TempData["ToastType"] = "success";
            return RedirectToAction(nameof(Index));
        }

        private bool CategoryExists(int id)
        {
            return _context.Categories.Any(e => e.CategoryID == id);
        }
    }
}
