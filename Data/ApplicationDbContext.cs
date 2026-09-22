using Microsoft.EntityFrameworkCore;
using PharmacyIMS.Models;

namespace PharmacyIMS.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<SupplierProduct> SupplierProducts { get; set; }
        public DbSet<Purchase> Purchases { get; set; }
        public DbSet<PurchaseItem> PurchaseItems { get; set; }
        public DbSet<Sale> Sales { get; set; }
        public DbSet<SaleItem> SaleItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Product>().Property(p => p.UnitPrice).HasPrecision(18, 2);
            modelBuilder.Entity<SupplierProduct>().Property(sp => sp.ContractPrice).HasPrecision(18, 2);
            modelBuilder.Entity<Purchase>().Property(p => p.TotalAmount).HasPrecision(18, 2);
            modelBuilder.Entity<PurchaseItem>().Property(pi => pi.UnitCost).HasPrecision(18, 2);
            modelBuilder.Entity<Sale>().Property(s => s.TotalAmount).HasPrecision(18, 2);
            modelBuilder.Entity<SaleItem>().Property(si => si.UnitPrice).HasPrecision(18, 2);

            modelBuilder.Entity<Product>().HasIndex(p => p.SKU).IsUnique();

            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Purchase>()
                .HasOne(pu => pu.Supplier)
                .WithMany(s => s.Purchases)
                .HasForeignKey(pu => pu.SupplierID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SupplierProduct>()
                .HasOne(sp => sp.Supplier)
                .WithMany(s => s.SupplierProducts)
                .HasForeignKey(sp => sp.SupplierID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SupplierProduct>()
                .HasOne(sp => sp.Product)
                .WithMany(p => p.SupplierProducts)
                .HasForeignKey(sp => sp.ProductID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PurchaseItem>()
                .HasOne(pi => pi.Purchase)
                .WithMany(p => p.PurchaseItems)
                .HasForeignKey(pi => pi.PurchaseID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PurchaseItem>()
                .HasOne(pi => pi.Product)
                .WithMany(p => p.PurchaseItems)
                .HasForeignKey(pi => pi.ProductID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SaleItem>()
                .HasOne(si => si.Sale)
                .WithMany(s => s.SaleItems)
                .HasForeignKey(si => si.SaleID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SaleItem>()
                .HasOne(si => si.Product)
                .WithMany(p => p.SaleItems)
                .HasForeignKey(si => si.ProductID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Category>().HasData(
                new Category { CategoryID = 1, CategoryName = "مسكنات وخافضات حرارة", Description = "أدوية تسكين الألم وخفض الحرارة" },
                new Category { CategoryID = 2, CategoryName = "مضادات حيوية", Description = "أدوية علاج الالتهابات البكتيرية" },
                new Category { CategoryID = 3, CategoryName = "فيتامينات ومكملات غذائية", Description = "فيتامينات ومعادن ومكملات صحية" },
                new Category { CategoryID = 4, CategoryName = "أدوات ومستلزمات طبية", Description = "أدوات طبية ومستهلكات صيدلية" },
                new Category { CategoryID = 5, CategoryName = "مستحضرات العناية الشخصية", Description = "منتجات العناية بالبشرة والعناية الشخصية" }
            );

            modelBuilder.Entity<Supplier>().HasData(
                new Supplier { SupplierID = 1, SupplierName = "شركة النصر للأدوية", ContactName = "أحمد فتحي", Phone = "01001234567", Email = "sales@nasr-pharma.com", Address = "القاهرة - المنطقة الصناعية" },
                new Supplier { SupplierID = 2, SupplierName = "المتحدة للمستلزمات الطبية", ContactName = "منى عبد الله", Phone = "01112345678", Email = "info@united-med.com", Address = "الجيزة - 6 أكتوبر" },
                new Supplier { SupplierID = 3, SupplierName = "شركة الحياة للفيتامينات", ContactName = "كريم صلاح", Phone = "01223456789", Email = "contact@hayah-vit.com", Address = "الإسكندرية - سموحة" }
            );

            modelBuilder.Entity<Product>().HasData(
                new Product { ProductID = 1, SKU = "MED-1001", ProductName = "باراسيتامول 500 مجم", Manufacturer = "النصر للأدوية", DosageForm = "أقراص", CategoryID = 1, UnitPrice = 12.50m, StockQuantity = 150, LowStockThreshold = 30, ExpiryDate = new DateTime(2027, 6, 30) },
                new Product { ProductID = 2, SKU = "MED-1002", ProductName = "إيبوبروفين 400 مجم", Manufacturer = "النصر للأدوية", DosageForm = "أقراص", CategoryID = 1, UnitPrice = 18.00m, StockQuantity = 8, LowStockThreshold = 20, ExpiryDate = new DateTime(2026, 12, 31) },
                new Product { ProductID = 3, SKU = "MED-2001", ProductName = "أموكسيسيلين 500 مجم", Manufacturer = "المتحدة للأدوية", DosageForm = "كبسولات", CategoryID = 2, UnitPrice = 35.75m, StockQuantity = 5, LowStockThreshold = 15, ExpiryDate = new DateTime(2026, 9, 30) },
                new Product { ProductID = 4, SKU = "MED-2002", ProductName = "أزيثروميسين 250 مجم", Manufacturer = "المتحدة للأدوية", DosageForm = "أقراص", CategoryID = 2, UnitPrice = 42.00m, StockQuantity = 60, LowStockThreshold = 15, ExpiryDate = new DateTime(2027, 3, 15) },
                new Product { ProductID = 5, SKU = "VIT-3001", ProductName = "فيتامين سي 1000 مجم فوار", Manufacturer = "شركة الحياة", DosageForm = "أقراص فوارة", CategoryID = 3, UnitPrice = 55.00m, StockQuantity = 90, LowStockThreshold = 20, ExpiryDate = new DateTime(2027, 11, 1) },
                new Product { ProductID = 6, SKU = "VIT-3002", ProductName = "زنك + فيتامين د3", Manufacturer = "شركة الحياة", DosageForm = "كبسولات", CategoryID = 3, UnitPrice = 65.00m, StockQuantity = 12, LowStockThreshold = 15, ExpiryDate = new DateTime(2027, 5, 20) },
                new Product { ProductID = 7, SKU = "DEV-4001", ProductName = "جهاز قياس ضغط الدم الرقمي", Manufacturer = "المتحدة للمستلزمات", DosageForm = "جهاز", CategoryID = 4, UnitPrice = 450.00m, StockQuantity = 10, LowStockThreshold = 5, ExpiryDate = null },
                new Product { ProductID = 8, SKU = "DEV-4002", ProductName = "كمامات طبية (علبة 50)", Manufacturer = "المتحدة للمستلزمات", DosageForm = "علبة", CategoryID = 4, UnitPrice = 60.00m, StockQuantity = 200, LowStockThreshold = 40, ExpiryDate = null },
                new Product { ProductID = 9, SKU = "CARE-5001", ProductName = "كريم مرطب للبشرة الجافة", Manufacturer = "شركة الحياة", DosageForm = "كريم", CategoryID = 5, UnitPrice = 85.00m, StockQuantity = 3, LowStockThreshold = 10, ExpiryDate = new DateTime(2027, 1, 10) }
            );

            modelBuilder.Entity<SupplierProduct>().HasData(
                new SupplierProduct { SupplierProductID = 1, SupplierID = 1, ProductID = 1, SupplierSKU = "NSR-P500", ContractPrice = 9.50m, LeadTimeDays = 3 },
                new SupplierProduct { SupplierProductID = 2, SupplierID = 1, ProductID = 2, SupplierSKU = "NSR-I400", ContractPrice = 14.00m, LeadTimeDays = 3 },
                new SupplierProduct { SupplierProductID = 3, SupplierID = 2, ProductID = 3, SupplierSKU = "UM-AMX500", ContractPrice = 28.00m, LeadTimeDays = 5 },
                new SupplierProduct { SupplierProductID = 4, SupplierID = 2, ProductID = 4, SupplierSKU = "UM-AZI250", ContractPrice = 33.00m, LeadTimeDays = 5 },
                new SupplierProduct { SupplierProductID = 5, SupplierID = 3, ProductID = 5, SupplierSKU = "HY-VITC1000", ContractPrice = 42.00m, LeadTimeDays = 2 },
                new SupplierProduct { SupplierProductID = 6, SupplierID = 3, ProductID = 6, SupplierSKU = "HY-ZND3", ContractPrice = 50.00m, LeadTimeDays = 2 },
                new SupplierProduct { SupplierProductID = 7, SupplierID = 2, ProductID = 7, SupplierSKU = "UM-BPM01", ContractPrice = 380.00m, LeadTimeDays = 7 }
            );
        }
    }
}
