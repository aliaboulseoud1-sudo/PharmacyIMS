namespace PharmacyIMS.Controllers
{
    public class SupplierProductsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SupplierProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: SupplierProducts/Create?supplierId=5
        public async Task<IActionResult> Create(int supplierId)
        {
            var supplier = await _context.Suppliers.FindAsync(supplierId);
            if (supplier == null) return NotFound();

            var vm = new SupplierProductViewModel
            {
                SupplierID = supplier.SupplierID,
                SupplierName = supplier.SupplierName
            };

            await LoadAvailableProducts(vm);
            return View(vm);
        }

        // POST: SupplierProducts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SupplierProductViewModel vm)
        {
            bool alreadyLinked = await _context.SupplierProducts.AnyAsync(sp =>
                sp.SupplierID == vm.SupplierID && sp.ProductID == vm.ProductID);

            if (alreadyLinked)
            {
                ModelState.AddModelError(string.Empty, "هذا المنتج مرتبط بالفعل بهذا المورد.");
            }

            if (!ModelState.IsValid)
            {
                var supplier = await _context.Suppliers.FindAsync(vm.SupplierID);
                vm.SupplierName = supplier?.SupplierName ?? vm.SupplierName;
                await LoadAvailableProducts(vm);
                return View(vm);
            }

            var link = new SupplierProduct
            {
                SupplierID = vm.SupplierID,
                ProductID = vm.ProductID,
                SupplierSKU = vm.SupplierSKU,
                ContractPrice = vm.ContractPrice,
                LeadTimeDays = vm.LeadTimeDays
            };

            _context.SupplierProducts.Add(link);
            await _context.SaveChangesAsync();

            TempData["ToastMessage"] = "تم ربط المنتج بالمورد بنجاح";
            TempData["ToastType"] = "success";
            return RedirectToAction("Details", "Suppliers", new { id = vm.SupplierID });
        }

        // GET: SupplierProducts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var link = await _context.SupplierProducts
                .Include(sp => sp.Supplier)
                .Include(sp => sp.Product)
                .FirstOrDefaultAsync(sp => sp.SupplierProductID == id);

            if (link == null) return NotFound();

            var vm = new SupplierProductViewModel
            {
                SupplierProductID = link.SupplierProductID,
                SupplierID = link.SupplierID,
                SupplierName = link.Supplier.SupplierName,
                ProductID = link.ProductID,
                ProductName = link.Product.ProductName,
                SupplierSKU = link.SupplierSKU,
                ContractPrice = link.ContractPrice,
                LeadTimeDays = link.LeadTimeDays
            };

            return View(vm);
        }

        // POST: SupplierProducts/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SupplierProductViewModel vm)
        {
            if (id != vm.SupplierProductID) return NotFound();

            ModelState.Remove(nameof(vm.ProductID));

            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            var link = await _context.SupplierProducts.FindAsync(id);
            if (link == null) return NotFound();

            link.SupplierSKU = vm.SupplierSKU;
            link.ContractPrice = vm.ContractPrice;
            link.LeadTimeDays = vm.LeadTimeDays;

            try
            {
                _context.Update(link);
                await _context.SaveChangesAsync();
                TempData["ToastMessage"] = "تم تحديث شروط التعاقد بنجاح";
                TempData["ToastType"] = "success";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.SupplierProducts.Any(e => e.SupplierProductID == id)) return NotFound();
                throw;
            }

            return RedirectToAction("Details", "Suppliers", new { id = link.SupplierID });
        }

        // POST: SupplierProducts/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var link = await _context.SupplierProducts.FindAsync(id);
            if (link == null) return NotFound();

            int supplierId = link.SupplierID;

            _context.SupplierProducts.Remove(link);
            await _context.SaveChangesAsync();

            TempData["ToastMessage"] = "تم إلغاء ربط المنتج عن المورد بنجاح";
            TempData["ToastType"] = "success";
            return RedirectToAction("Details", "Suppliers", new { id = supplierId });
        }

        private async Task LoadAvailableProducts(SupplierProductViewModel vm)
        {
            var linkedProductIds = await _context.SupplierProducts
                .Where(sp => sp.SupplierID == vm.SupplierID)
                .Select(sp => sp.ProductID)
                .ToListAsync();

            vm.AvailableProducts = await _context.Products
                .Where(p => !linkedProductIds.Contains(p.ProductID))
                .OrderBy(p => p.ProductName)
                .Select(p => new SelectListItem
                {
                    Value = p.ProductID.ToString(),
                    Text = p.ProductName
                })
                .ToListAsync();
        }
    }
}
