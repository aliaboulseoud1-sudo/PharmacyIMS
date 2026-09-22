using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PharmacyIMS.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    CategoryID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.CategoryID);
                });

            migrationBuilder.CreateTable(
                name: "Sales",
                columns: table => new
                {
                    SaleID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SaleDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CustomerInfo = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sales", x => x.SaleID);
                });

            migrationBuilder.CreateTable(
                name: "Suppliers",
                columns: table => new
                {
                    SupplierID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SupplierName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    ContactName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Suppliers", x => x.SupplierID);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    ProductID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SKU = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ProductName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Manufacturer = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DosageForm = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CategoryID = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    StockQuantity = table.Column<int>(type: "int", nullable: false),
                    LowStockThreshold = table.Column<int>(type: "int", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.ProductID);
                    table.ForeignKey(
                        name: "FK_Products_Categories_CategoryID",
                        column: x => x.CategoryID,
                        principalTable: "Categories",
                        principalColumn: "CategoryID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Purchases",
                columns: table => new
                {
                    PurchaseID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SupplierID = table.Column<int>(type: "int", nullable: false),
                    PurchaseDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Purchases", x => x.PurchaseID);
                    table.ForeignKey(
                        name: "FK_Purchases_Suppliers_SupplierID",
                        column: x => x.SupplierID,
                        principalTable: "Suppliers",
                        principalColumn: "SupplierID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SaleItems",
                columns: table => new
                {
                    SaleItemID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SaleID = table.Column<int>(type: "int", nullable: false),
                    ProductID = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SaleItems", x => x.SaleItemID);
                    table.ForeignKey(
                        name: "FK_SaleItems_Products_ProductID",
                        column: x => x.ProductID,
                        principalTable: "Products",
                        principalColumn: "ProductID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SaleItems_Sales_SaleID",
                        column: x => x.SaleID,
                        principalTable: "Sales",
                        principalColumn: "SaleID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SupplierProducts",
                columns: table => new
                {
                    SupplierProductID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SupplierID = table.Column<int>(type: "int", nullable: false),
                    ProductID = table.Column<int>(type: "int", nullable: false),
                    SupplierSKU = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ContractPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    LeadTimeDays = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplierProducts", x => x.SupplierProductID);
                    table.ForeignKey(
                        name: "FK_SupplierProducts_Products_ProductID",
                        column: x => x.ProductID,
                        principalTable: "Products",
                        principalColumn: "ProductID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SupplierProducts_Suppliers_SupplierID",
                        column: x => x.SupplierID,
                        principalTable: "Suppliers",
                        principalColumn: "SupplierID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseItems",
                columns: table => new
                {
                    PurchaseItemID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PurchaseID = table.Column<int>(type: "int", nullable: false),
                    ProductID = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UnitCost = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseItems", x => x.PurchaseItemID);
                    table.ForeignKey(
                        name: "FK_PurchaseItems_Products_ProductID",
                        column: x => x.ProductID,
                        principalTable: "Products",
                        principalColumn: "ProductID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PurchaseItems_Purchases_PurchaseID",
                        column: x => x.PurchaseID,
                        principalTable: "Purchases",
                        principalColumn: "PurchaseID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "CategoryID", "CategoryName", "Description" },
                values: new object[,]
                {
                    { 1, "مسكنات وخافضات حرارة", "أدوية تسكين الألم وخفض الحرارة" },
                    { 2, "مضادات حيوية", "أدوية علاج الالتهابات البكتيرية" },
                    { 3, "فيتامينات ومكملات غذائية", "فيتامينات ومعادن ومكملات صحية" },
                    { 4, "أدوات ومستلزمات طبية", "أدوات طبية ومستهلكات صيدلية" },
                    { 5, "مستحضرات العناية الشخصية", "منتجات العناية بالبشرة والعناية الشخصية" }
                });

            migrationBuilder.InsertData(
                table: "Suppliers",
                columns: new[] { "SupplierID", "Address", "ContactName", "Email", "Phone", "SupplierName" },
                values: new object[,]
                {
                    { 1, "القاهرة - المنطقة الصناعية", "أحمد فتحي", "sales@nasr-pharma.com", "01001234567", "شركة النصر للأدوية" },
                    { 2, "الجيزة - 6 أكتوبر", "منى عبد الله", "info@united-med.com", "01112345678", "المتحدة للمستلزمات الطبية" },
                    { 3, "الإسكندرية - سموحة", "كريم صلاح", "contact@hayah-vit.com", "01223456789", "شركة الحياة للفيتامينات" }
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "ProductID", "CategoryID", "DosageForm", "ExpiryDate", "LowStockThreshold", "Manufacturer", "ProductName", "SKU", "StockQuantity", "UnitPrice" },
                values: new object[,]
                {
                    { 1, 1, "أقراص", new DateTime(2027, 6, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), 30, "النصر للأدوية", "باراسيتامول 500 مجم", "MED-1001", 150, 12.50m },
                    { 2, 1, "أقراص", new DateTime(2026, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), 20, "النصر للأدوية", "إيبوبروفين 400 مجم", "MED-1002", 8, 18.00m },
                    { 3, 2, "كبسولات", new DateTime(2026, 9, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), 15, "المتحدة للأدوية", "أموكسيسيلين 500 مجم", "MED-2001", 5, 35.75m },
                    { 4, 2, "أقراص", new DateTime(2027, 3, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), 15, "المتحدة للأدوية", "أزيثروميسين 250 مجم", "MED-2002", 60, 42.00m },
                    { 5, 3, "أقراص فوارة", new DateTime(2027, 11, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 20, "شركة الحياة", "فيتامين سي 1000 مجم فوار", "VIT-3001", 90, 55.00m },
                    { 6, 3, "كبسولات", new DateTime(2027, 5, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), 15, "شركة الحياة", "زنك + فيتامين د3", "VIT-3002", 12, 65.00m },
                    { 7, 4, "جهاز", null, 5, "المتحدة للمستلزمات", "جهاز قياس ضغط الدم الرقمي", "DEV-4001", 10, 450.00m },
                    { 8, 4, "علبة", null, 40, "المتحدة للمستلزمات", "كمامات طبية (علبة 50)", "DEV-4002", 200, 60.00m },
                    { 9, 5, "كريم", new DateTime(2027, 1, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), 10, "شركة الحياة", "كريم مرطب للبشرة الجافة", "CARE-5001", 3, 85.00m }
                });

            migrationBuilder.InsertData(
                table: "SupplierProducts",
                columns: new[] { "SupplierProductID", "ContractPrice", "LeadTimeDays", "ProductID", "SupplierID", "SupplierSKU" },
                values: new object[,]
                {
                    { 1, 9.50m, 3, 1, 1, "NSR-P500" },
                    { 2, 14.00m, 3, 2, 1, "NSR-I400" },
                    { 3, 28.00m, 5, 3, 2, "UM-AMX500" },
                    { 4, 33.00m, 5, 4, 2, "UM-AZI250" },
                    { 5, 42.00m, 2, 5, 3, "HY-VITC1000" },
                    { 6, 50.00m, 2, 6, 3, "HY-ZND3" },
                    { 7, 380.00m, 7, 7, 2, "UM-BPM01" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Products_CategoryID",
                table: "Products",
                column: "CategoryID");

            migrationBuilder.CreateIndex(
                name: "IX_Products_SKU",
                table: "Products",
                column: "SKU",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseItems_ProductID",
                table: "PurchaseItems",
                column: "ProductID");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseItems_PurchaseID",
                table: "PurchaseItems",
                column: "PurchaseID");

            migrationBuilder.CreateIndex(
                name: "IX_Purchases_SupplierID",
                table: "Purchases",
                column: "SupplierID");

            migrationBuilder.CreateIndex(
                name: "IX_SaleItems_ProductID",
                table: "SaleItems",
                column: "ProductID");

            migrationBuilder.CreateIndex(
                name: "IX_SaleItems_SaleID",
                table: "SaleItems",
                column: "SaleID");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierProducts_ProductID",
                table: "SupplierProducts",
                column: "ProductID");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierProducts_SupplierID",
                table: "SupplierProducts",
                column: "SupplierID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PurchaseItems");

            migrationBuilder.DropTable(
                name: "SaleItems");

            migrationBuilder.DropTable(
                name: "SupplierProducts");

            migrationBuilder.DropTable(
                name: "Purchases");

            migrationBuilder.DropTable(
                name: "Sales");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "Suppliers");

            migrationBuilder.DropTable(
                name: "Categories");
        }
    }
}
