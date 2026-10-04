namespace PharmacyIMS.Controllers
{
    public class SuppliersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SuppliersController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? searchTerm)
        {
            var suppliersQuery = _context.Suppliers.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                suppliersQuery = suppliersQuery.Where(s =>
                    s.SupplierName.Contains(searchTerm) ||
                    (s.ContactName != null && s.ContactName.Contains(searchTerm)) ||
                    (s.Phone != null && s.Phone.Contains(searchTerm)));
            }

            ViewData["CurrentFilter"] = searchTerm;

            var suppliers = await suppliersQuery
                .OrderBy(s => s.SupplierName)
                .ToListAsync();

            return View(suppliers);
        }


        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var supplier = await _context.Suppliers
                .Include(s => s.SupplierProducts)
                    .ThenInclude(sp => sp.Product)
                .FirstOrDefaultAsync(s => s.SupplierID == id);

            if (supplier == null)
                return NotFound();

            return View(supplier);
        }


        public IActionResult Create()
        {
            return View(new SupplierViewModel());
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SupplierViewModel vm)
        {
            if (!string.IsNullOrWhiteSpace(vm.SupplierName))
            {
                bool nameExists = await _context.Suppliers
                    .AnyAsync(s => s.SupplierName == vm.SupplierName);

                if (nameExists)
                {
                    ModelState.AddModelError(
                        nameof(vm.SupplierName),
                        "اسم المورد مستخدم بالفعل");
                }
            }

            if (!string.IsNullOrWhiteSpace(vm.Phone))
            {
                bool phoneExists = await _context.Suppliers
                    .AnyAsync(s => s.Phone == vm.Phone);

                if (phoneExists)
                {
                    ModelState.AddModelError(
                        nameof(vm.Phone),
                        "رقم الهاتف مستخدم بالفعل");
                }
            }

            if (!string.IsNullOrWhiteSpace(vm.Email))
            {
                bool emailExists = await _context.Suppliers
                    .AnyAsync(s => s.Email == vm.Email);

                if (emailExists)
                {
                    ModelState.AddModelError(
                        nameof(vm.Email),
                        "البريد الإلكتروني مستخدم بالفعل");
                }
            }


            if (!ModelState.IsValid)
            {
                return View(vm);
            }


            var supplier = new Supplier
            {
                SupplierName = vm.SupplierName.Trim(),
                ContactName = vm.ContactName.Trim(),
                Phone = vm.Phone.Trim(),
                Email = vm.Email.Trim(),
                Address = vm.Address.Trim()
            };

            _context.Suppliers.Add(supplier);

            await _context.SaveChangesAsync();

            TempData["ToastMessage"] = "تم إضافة المورد بنجاح";
            TempData["ToastType"] = "success";

            return RedirectToAction(nameof(Index));
        }


        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var supplier = await _context.Suppliers.FindAsync(id);

            if (supplier == null)
                return NotFound();

            var vm = new SupplierViewModel
            {
                SupplierID = supplier.SupplierID,
                SupplierName = supplier.SupplierName,
                ContactName = supplier.ContactName,
                Phone = supplier.Phone,
                Email = supplier.Email,
                Address = supplier.Address
            };

            return View(vm);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            SupplierViewModel vm)
        {
            if (id != vm.SupplierID)
                return NotFound();


            if (!string.IsNullOrWhiteSpace(vm.SupplierName))
            {
                bool nameExists = await _context.Suppliers
                    .AnyAsync(s =>
                        s.SupplierName == vm.SupplierName &&
                        s.SupplierID != vm.SupplierID);

                if (nameExists)
                {
                    ModelState.AddModelError(
                        nameof(vm.SupplierName),
                        "اسم المورد مستخدم بالفعل");
                }
            }


            if (!string.IsNullOrWhiteSpace(vm.Phone))
            {
                bool phoneExists = await _context.Suppliers
                    .AnyAsync(s =>
                        s.Phone == vm.Phone &&
                        s.SupplierID != vm.SupplierID);

                if (phoneExists)
                {
                    ModelState.AddModelError(
                        nameof(vm.Phone),
                        "رقم الهاتف مستخدم بالفعل");
                }
            }


            if (!string.IsNullOrWhiteSpace(vm.Email))
            {
                bool emailExists = await _context.Suppliers
                    .AnyAsync(s =>
                        s.Email == vm.Email &&
                        s.SupplierID != vm.SupplierID);

                if (emailExists)
                {
                    ModelState.AddModelError(
                        nameof(vm.Email),
                        "البريد الإلكتروني مستخدم بالفعل");
                }
            }


            if (!ModelState.IsValid)
            {
                return View(vm);
            }


            var supplier = await _context.Suppliers
                .FirstOrDefaultAsync(s => s.SupplierID == id);

            if (supplier == null)
                return NotFound();


            supplier.SupplierName = vm.SupplierName.Trim();
            supplier.ContactName = vm.ContactName.Trim();
            supplier.Phone = vm.Phone.Trim();
            supplier.Email = vm.Email.Trim();
            supplier.Address = vm.Address.Trim();


            try
            {
                await _context.SaveChangesAsync();

                TempData["ToastMessage"] =
                    "تم تعديل بيانات المورد بنجاح";

                TempData["ToastType"] =
                    "success";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SupplierExists(id))
                    return NotFound();

                throw;
            }


            return RedirectToAction(nameof(Index));
        }


        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var supplier = await _context.Suppliers
                .Include(s => s.SupplierProducts)
                .Include(s => s.Purchases)
                .FirstOrDefaultAsync(s => s.SupplierID == id);

            if (supplier == null)
                return NotFound();

            bool hasLinkedProducts =
                supplier.SupplierProducts.Any();

            bool hasLinkedPurchases =
                supplier.Purchases != null &&
                supplier.Purchases.Any();

            ViewData["HasLinks"] =
                hasLinkedProducts ||
                hasLinkedPurchases;

            ViewData["LinkedProductsCount"] =
                supplier.SupplierProducts.Count;

            ViewData["LinkedPurchasesCount"] =
                supplier.Purchases?.Count ?? 0;

            return View(supplier);
        }


        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var supplier = await _context.Suppliers
                .Include(s => s.SupplierProducts)
                .Include(s => s.Purchases)
                .FirstOrDefaultAsync(s => s.SupplierID == id);

            if (supplier == null)
                return NotFound();

            bool hasLinkedProducts =
                supplier.SupplierProducts.Any();

            bool hasLinkedPurchases =
                supplier.Purchases != null &&
                supplier.Purchases.Any();

            if (hasLinkedProducts || hasLinkedPurchases)
            {
                TempData["ToastMessage"] =
                    "لا يمكن حذف هذا المورد لارتباطه بمنتجات و/أو عمليات شراء حالية. قم بإزالة الارتباطات أولاً.";

                TempData["ToastType"] = "danger";

                return RedirectToAction(
                    nameof(Delete),
                    new { id });
            }

            _context.Suppliers.Remove(supplier);

            await _context.SaveChangesAsync();

            TempData["ToastMessage"] =
                "تم حذف المورد بنجاح";

            TempData["ToastType"] =
                "success";

            return RedirectToAction(nameof(Index));
        }


        private bool SupplierExists(int id)
        {
            return _context.Suppliers
                .Any(e => e.SupplierID == id);
        }
    }
}