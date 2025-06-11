using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using AlphaShop.Models;

namespace AlphaShop.Data
{
    public class AlphaShopContext : DbContext
    {
        public AlphaShopContext (DbContextOptions<AlphaShopContext> options)
            : base(options)
        {
        }
        public DbSet<AlphaShop.Models.Customer> Customer { get; set; } = default!;
        public DbSet<AlphaShop.Models.Category> Category { get; set; } = default!;
        public DbSet<AlphaShop.Models.Brand> Brand { get; set; } = default!;
        public DbSet<AlphaShop.Models.Product> Product { get; set; } = default!;
        public DbSet<AlphaShop.Models.Tag> Tag { get; set; } = default!;
        public DbSet<AlphaShop.Models.ProductTag> ProductTag { get; set; } = default!;
        public DbSet<AlphaShop.Models.ProductVariant> ProductVaraint { get; set; } = default!;
        public DbSet<AlphaShop.Models.OrderItem> OrderItem { get; set; } = default!;
        public DbSet<AlphaShop.Models.Order> Order { get; set; } = default!;
        public DbSet<AlphaShop.Models.Supplier> Supplier { get; set; } = default!;
        public DbSet<AlphaShop.Models.StockTransaction> StockTransactions { get; set; } = default!;
        public DbSet<AlphaShop.Models.WareHouse> WareHouse { get; set; } = default!;
        public DbSet<AlphaShop.Models.InventoryLog> InventoryLog { get; set; } = default!;
        public DbSet<AlphaShop.Models.PurchaseOrder> PurchaseOrder { get; set; } = default!;
        public DbSet<AlphaShop.Models.OrderItemSuppliers> OrderItemSuppliers { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Supplier>(entity =>
            {
                entity.ToTable("Suppliers");

                entity.HasKey(s => s.Id);

                entity.Property(s => s.SupplierName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(s => s.ImageSupplier)
                    .HasMaxLength(255);

                entity.Property(s => s.Email)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(s => s.Phone)
                    .IsRequired()
                    .HasMaxLength(20);

                entity.Property(s => s.Address)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(s => s.Street)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(s => s.City)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(s => s.Country)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(s => s.ContactPerson)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(s => s.ContractDate)
                    .IsRequired();

                entity.Property(s => s.Status)
                    .IsRequired()
                    .HasDefaultValue(StatusSupplier.Active)
                    .HasConversion<string>();

                entity.Property(s => s.CreatedAt)
                    .IsRequired()
                    .HasDefaultValueSql("GETDATE()");

                entity.Property(s => s.UpdatedAt)
                    .IsRequired()
                    .HasDefaultValueSql("GETDATE()");

                // Relationships
                entity.HasMany(s => s.Products)
                    .WithOne(p => p.Supplier)
                    .HasForeignKey(p => p.SupplierId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(s => s.PurchaseOrders)
                    .WithOne(po => po.Supplier)
                    .HasForeignKey(po => po.SupplierId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
            modelBuilder.Entity<Product>(entity =>
            {
                entity.ToTable("Products");

                entity.Property(p => p.ProductName)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(p => p.ProductImage)
                    .HasMaxLength(255);

                entity.Property(p => p.ProductDescription)
                    .HasMaxLength(2000);

                entity.Property(p => p.Profits)
                    .HasColumnType("decimal(18,2)");

                entity.Property(p => p.DisccoutPrice)
                    .HasColumnType("decimal(18,2)");

                entity.Property(p => p.SKU)
                    .HasMaxLength(50);

                entity.Property(p => p.Weight)
                    .HasColumnType("decimal(10,3)");

                entity.Property(p => p.Price)
                    .HasColumnType("decimal(18,2)");

                entity.Property(p => p.IsFeatured)
                    .HasDefaultValue(false);

                entity.Property(p => p.IsActive)
                    .HasDefaultValue(true);

                entity.Property(p => p.CreatedAt)
                    .HasDefaultValueSql("GETDATE()");

                entity.Property(p => p.UpdatedAt)
                    .HasDefaultValueSql("GETDATE()");

                // Relationships
                entity.HasOne(p => p.Category)
                    .WithMany()
                    .HasForeignKey(p => p.CategoryId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(p => p.Brand)
                    .WithMany()
                    .HasForeignKey(p => p.BrandId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(p => p.Supplier)
                    .WithMany(s => s.Products)
                    .HasForeignKey(p => p.SupplierId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(p => p.WareHouse)
                    .WithMany()
                    .HasForeignKey(p => p.WareHouseId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasMany(p => p.InventoryLogs)
                    .WithOne()
                    .HasForeignKey(il => il.ProductId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasMany(p => p.ProductTags)
                    .WithOne(pt => pt.Product)
                    .HasForeignKey(pt => pt.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(p => p.ProductVariants)
                    .WithOne(pv => pv.Product)
                    .HasForeignKey(pv => pv.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(p => p.OrderItems)
                    .WithOne(oi => oi.Product)
                    .HasForeignKey(oi => oi.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(p => p.OrderItemSuppliers)
                    .WithOne(ois => ois.Product)
                    .HasForeignKey(ois => ois.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
            modelBuilder.Entity<Customer>(entity =>
            {
                entity.ToTable("Customers");

                // Properties configuration
                entity.Property(c => c.CustomerId)
                    .HasMaxLength(50);

                entity.Property(c => c.FirstName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(c => c.LastName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(c => c.DateOfBirth)
                    .HasColumnType("date");

                entity.Property(c => c.Email)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(c => c.Password)
                    .IsRequired()
                    .HasMaxLength(255); // Store hashed passwords only

                entity.Property(c => c.Phone)
                    .HasMaxLength(20);

                entity.Property(c => c.Address)
                    .HasMaxLength(200);

                entity.Property(c => c.City)
                    .HasMaxLength(50);

                entity.Property(c => c.Country)
                    .HasMaxLength(50);

                entity.Property(c => c.State)
                    .HasMaxLength(50);

                entity.Property(c => c.Status)
                    .HasDefaultValue(CustomerStatus.Active)
                    .HasConversion<string>();

                entity.Property(c => c.RegisterDate)
                    .HasDefaultValueSql("GETDATE()");

                entity.Property(c => c.EmailVerificationToken)
                    .HasMaxLength(100);

                entity.Property(c => c.ProfilePicture)
                    .HasMaxLength(255);

                entity.Property(c => c.CreatedAt)
                    .HasDefaultValueSql("GETDATE()");

                entity.Property(c => c.UpdatedAt)
                    .HasDefaultValueSql("GETDATE()");

                // Relationships
                entity.HasMany(c => c.Order)
                    .WithOne(o => o.Customer)
                    .HasForeignKey(o => o.CustomerId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Indexes
                entity.HasIndex(c => c.Email)
                    .IsUnique();

                entity.HasIndex(c => c.CustomerId)
                    .IsUnique()
                    .HasFilter("[CustomerId] IS NOT NULL");

                entity.HasIndex(c => new { c.LastName, c.FirstName });
            });
            modelBuilder.Entity<Category>(entity =>
            {
                entity.ToTable("Categories");

                // Properties
                entity.Property(c => c.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(c => c.Description)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.Property(c => c.IconUrl)
                    .HasMaxLength(255);

                entity.Property(c => c.CreatedAt)
                    .HasDefaultValueSql("GETDATE()");

                entity.Property(c => c.UpdatedAt)
                    .HasDefaultValueSql("GETDATE()");

                // Self-referencing relationship (Hierarchical structure)
                entity.HasOne(c => c.ParentCategory)
                    .WithMany(c => c.SubCategories)
                    .HasForeignKey(c => c.ParentCategoryId)
                    .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete to avoid circular reference

                // Relationship with Products
                entity.HasMany(c => c.Products)
                    .WithOne(p => p.Category)
                    .HasForeignKey(p => p.CategoryId)
                    .OnDelete(DeleteBehavior.SetNull); // If category is deleted, set product's CategoryId to null

                // Indexes
                entity.HasIndex(c => c.Name)
                    .IsUnique();

                entity.HasIndex(c => c.ParentCategoryId);
            });
            modelBuilder.Entity<Brand>(entity =>
            {
                entity.ToTable("Brands");

                // Properties configuration
                entity.Property(b => b.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(b => b.Description)
                    .HasMaxLength(500);

                entity.Property(b => b.LogoUrl)
                    .HasMaxLength(255);

                entity.Property(b => b.WebsiteUrl)
                    .HasMaxLength(255);

                entity.Property(b => b.SocialMediaUrl)
                    .HasMaxLength(255);

                entity.Property(b => b.Country)
                    .HasMaxLength(50);

                entity.Property(b => b.CreatedAt)
                    .HasDefaultValueSql("GETDATE()");

                entity.Property(b => b.UpdatedAt)
                    .HasDefaultValueSql("GETDATE()");

                entity.Property(b => b.IsActive)
                    .HasDefaultValue(true);

                // Relationship with Products
                entity.HasMany(b => b.Products)
                    .WithOne(p => p.Brand)
                    .HasForeignKey(p => p.BrandId)
                    .OnDelete(DeleteBehavior.Restrict); // Prevent brand deletion if products exist

                // Indexes
                entity.HasIndex(b => b.Name)
                    .IsUnique();

                entity.HasIndex(b => b.Country);
            });
            modelBuilder.Entity<Tag>(entity =>
            {
                entity.ToTable("Tags");

                entity.Property(t => t.Name)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(t => t.Slug)
                    .HasMaxLength(60);

                // Indexes
                entity.HasIndex(t => t.Name)
                    .IsUnique();

                entity.HasIndex(t => t.Slug)
                    .IsUnique()
                    .HasFilter("[Slug] IS NOT NULL");

                // Relationship with Products (through ProductTag)
                entity.HasMany(t => t.ProductTags)
                    .WithOne(pt => pt.Tag)
                    .HasForeignKey(pt => pt.TagId)
                    .OnDelete(DeleteBehavior.Cascade); // Delete ProductTag when Tag is deleted
            });
            modelBuilder.Entity<ProductTag>(entity =>
            {
                entity.ToTable("ProductTags");

                // Composite primary key
                entity.HasKey(pt => new { pt.ProductId, pt.TagId });

                // Relationships
                entity.HasOne(pt => pt.Product)
                    .WithMany(p => p.ProductTags)
                    .HasForeignKey(pt => pt.ProductId)
                    .OnDelete(DeleteBehavior.Cascade); // Delete ProductTag when Product is deleted

                entity.HasOne(pt => pt.Tag)
                    .WithMany(t => t.ProductTags)
                    .HasForeignKey(pt => pt.TagId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
            modelBuilder.Entity<ProductTag>(entity =>
            {
                entity.ToTable("ProductTags");

                // Primary Key (using Id as per your model)
                entity.HasKey(pt => pt.Id);

                // Alternate Key (unique combination of ProductId and TagId)
                entity.HasIndex(pt => new { pt.ProductId, pt.TagId })
                    .IsUnique();

                // Properties
                entity.Property(pt => pt.CreateAt)
                    .HasColumnName("CreatedAt") // Fixing naming consistency
                    .HasDefaultValueSql("GETDATE()");

                entity.Property(pt => pt.UpdateAt)
                    .HasColumnName("UpdatedAt") // Fixing naming consistency
                    .HasDefaultValueSql("GETDATE()");

                // Relationships
                entity.HasOne(pt => pt.Product)
                    .WithMany(p => p.ProductTags)
                    .HasForeignKey(pt => pt.ProductId)
                    .OnDelete(DeleteBehavior.Cascade); // Delete join record when Product is deleted

                entity.HasOne(pt => pt.Tag)
                    .WithMany(t => t.ProductTags)
                    .HasForeignKey(pt => pt.TagId)
                    .OnDelete(DeleteBehavior.Cascade); // Delete join record when Tag is deleted
            });
            modelBuilder.Entity<ProductVariant>(entity =>
            {
                entity.ToTable("ProductVariants");

                // Primary Key
                entity.HasKey(pv => pv.Id);

                // Properties configuration
                entity.Property(pv => pv.attributeName)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("AttributeName"); // Standardizing naming convention

                entity.Property(pv => pv.attributeValue)
                    .HasMaxLength(100)
                    .HasColumnName("AttributeValue"); // Standardizing naming convention

                entity.Property(pv => pv.price_modifier)
                    .HasColumnType("decimal(18,2)")
                    .HasColumnName("PriceModifier"); // Standardizing naming convention

                entity.Property(pv => pv.sku_suffix)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnName("SkuSuffix"); // Standardizing naming convention

                entity.Property(pv => pv.CreateAt)
                    .HasColumnName("CreatedAt") // Standardizing naming
                    .HasDefaultValueSql("GETDATE()");

                entity.Property(pv => pv.UpdateAt)
                    .HasColumnName("UpdatedAt") // Standardizing naming
                    .HasDefaultValueSql("GETDATE()");

                // Relationships
                entity.HasOne(pv => pv.Product)
                    .WithMany(p => p.ProductVariants)
                    .HasForeignKey(pv => pv.ProductId)
                    .OnDelete(DeleteBehavior.Cascade); // Delete variants when product is deleted

                entity.HasMany(pv => pv.OrderItems)
                    .WithOne(oi => oi.PriceVariant)
                    .HasForeignKey(oi => oi.Id)
                    .OnDelete(DeleteBehavior.Restrict); // Prevent variant deletion if ordered

                // Indexes
                entity.HasIndex(pv => pv.ProductId);
                entity.HasIndex(pv => new { pv.attributeName, pv.attributeValue });
                entity.HasIndex(pv => pv.sku_suffix);
            });
            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.ToTable("OrderItems");

                // Properties configuration
                entity.Property(oi => oi.Quantity)
                    .IsRequired()
                    .HasDefaultValue(1);

                entity.Property(oi => oi.Price)
                    .HasColumnType("decimal(18,2)")
                    .IsRequired();

                entity.Property(oi => oi.Discount)
                    .HasColumnType("decimal(18,2)")
                    .HasDefaultValue(0);

                entity.Property(oi => oi.SubTotal)
                    .HasColumnType("decimal(18,2)")
                    .HasComputedColumnSql("([Price] * [Quantity]) - [Discount]"); // Auto-calculated

                entity.Property(oi => oi.CreatedAt)
                    .HasDefaultValueSql("GETDATE()");

                entity.Property(oi => oi.UpdatedAt)
                    .HasDefaultValueSql("GETDATE()");

                entity.Property(oi => oi.Status)
                    .HasDefaultValue(StatusOrderItem.Pending)
                    .HasConversion<string>();

                // Relationships
                entity.HasOne(oi => oi.Product)
                    .WithMany(p => p.OrderItems)
                    .HasForeignKey(oi => oi.ProductId)
                    .OnDelete(DeleteBehavior.SetNull); // Keep order items if product is deleted

                entity.HasOne(oi => oi.PriceVariant)
                    .WithMany()
                    .HasForeignKey(oi => oi.PriceVariantId)
                    .OnDelete(DeleteBehavior.Restrict); // Prevent deletion if used in orders

                entity.HasOne(oi => oi.Order)
                    .WithMany(o => o.OrderItems)
                    .HasForeignKey(oi => oi.OrderId)
                    .OnDelete(DeleteBehavior.Cascade); // Delete items when order is deleted

                // Indexes
                entity.HasIndex(oi => oi.OrderId);
                entity.HasIndex(oi => oi.ProductId);
                entity.HasIndex(oi => oi.PriceVariantId);
                entity.HasIndex(oi => oi.Status);
            });
            modelBuilder.Entity<Order>(entity =>
            {
                entity.ToTable("Orders");

                // Properties configuration
                entity.Property(o => o.OrderDate)
                    .IsRequired()
                    .HasDefaultValueSql("GETDATE()");

                entity.Property(o => o.TotalAmount)
                    .HasColumnType("decimal(18,2)")
                    .IsRequired();

                entity.Property(o => o.TrackingNumber)
                    .HasMaxLength(50);

                entity.Property(o => o.CreatedAt)
                    .HasDefaultValueSql("GETDATE()");

                entity.Property(o => o.UpdatedAt)
                    .HasDefaultValueSql("GETDATE()");

                entity.Property(o => o.Status)
                    .HasDefaultValue(OrderStatus.Pending)
                    .HasConversion<string>()
                    .HasMaxLength(20);

                // Relationships
                entity.HasOne(o => o.Customer)
                    .WithMany(c => c.Order)
                    .HasForeignKey(o => o.CustomerId)
                    .OnDelete(DeleteBehavior.Restrict); // Prevent customer deletion if orders exist

                entity.HasMany(o => o.OrderItems)
                    .WithOne(oi => oi.Order)
                    .HasForeignKey(oi => oi.OrderId)
                    .OnDelete(DeleteBehavior.Cascade); // Delete all items when order is deleted

                // Indexes
                entity.HasIndex(o => o.CustomerId);
                entity.HasIndex(o => o.OrderDate);
                entity.HasIndex(o => o.Status);
                entity.HasIndex(o => o.TrackingNumber)
                    .IsUnique()
                    .HasFilter("[TrackingNumber] IS NOT NULL");

                // For better query performance on common filters
                entity.HasIndex(o => new { o.Status, o.OrderDate });
            });
            modelBuilder.Entity<StockTransaction>(entity =>
            {
                entity.ToTable("StockTransactions");

                // Properties configuration
                entity.Property(st => st.Id)
                    .ValueGeneratedOnAdd();

                entity.Property(st => st.TypeName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(st => st.Direction)
                    .IsRequired()
                    .HasConversion<string>()
                    .HasMaxLength(10);

                entity.Property(st => st.StockTransactionDate)
                    .IsRequired()
                    .HasDefaultValueSql("GETDATE()");

                entity.Property(st => st.IsActive)
                    .IsRequired()
                    .HasDefaultValue(true);

                // Relationships
                entity.HasMany(st => st.InventoryLogs)
                    .WithOne(il => il.StockTransacition)
                    .HasForeignKey(il => il.Id)
                    .OnDelete(DeleteBehavior.Cascade); // Delete all inventory logs when stock transaction is deleted

                // Indexes
                entity.HasIndex(st => st.TypeName);
                entity.HasIndex(st => st.Direction);
                entity.HasIndex(st => st.StockTransactionDate);
                entity.HasIndex(st => st.IsActive);

                // Composite index for common queries
                entity.HasIndex(st => new { st.Direction, st.StockTransactionDate, st.IsActive });
            });
            modelBuilder.Entity<WareHouse>(entity =>
            {
                entity.ToTable("WareHouses");

                // Properties configuration
                entity.Property(wh => wh.Id)
                    .ValueGeneratedOnAdd();

                entity.Property(wh => wh.WareHouseName)
                    .HasMaxLength(100);

                entity.Property(wh => wh.WareHouseCode)
                    .HasMaxLength(50);

                entity.Property(wh => wh.WareHouseLocation)
                    .HasMaxLength(200);

                entity.Property(wh => wh.WareHousePhone)
                    .HasMaxLength(20);

                // Relationships
                entity.HasMany(wh => wh.Products)
                    .WithOne(p => p.WareHouse)
                    .HasForeignKey(p => p.WareHouseId)
                    .OnDelete(DeleteBehavior.Restrict); // Prevent warehouse deletion if products exist

                // Indexes
                entity.HasIndex(wh => wh.WareHouseName);
                entity.HasIndex(wh => wh.WareHouseCode)
                    .IsUnique()
                    .HasFilter("[WareHouseCode] IS NOT NULL");
                entity.HasIndex(wh => wh.WareHouseLocation);

                // Composite index for common queries
                entity.HasIndex(wh => new { wh.WareHouseName, wh.WareHouseLocation });
            });
            modelBuilder.Entity<InventoryLog>(entity =>
            {
                entity.ToTable("InventoryLogs");

                // Properties configuration
                entity.Property(il => il.Id)
                    .ValueGeneratedOnAdd();

                entity.Property(il => il.Qunatity)  // Note: Typo in property name (should be Quantity)
                    .IsRequired();

                entity.Property(il => il.Note)
                    .HasMaxLength(500);

                entity.Property(il => il.Log_Date)
                    .IsRequired()
                    .HasDefaultValueSql("GETDATE()");

                // Relationships
                entity.HasOne(il => il.StockTransacition)  // Note: Typo in property name (should be StockTransaction)
                    .WithMany(st => st.InventoryLogs)
                    .HasForeignKey(il => il.StockTransacitionId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(il => il.Product)
                    .WithMany(p => p.InventoryLogs)
                    .HasForeignKey(il => il.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(il => il.PurchaseOrder)
                    .WithMany(po => po.InventoryLogs)
                    .HasForeignKey(il => il.PurchaseOrderId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Indexes
                entity.HasIndex(il => il.Log_Date);
                entity.HasIndex(il => il.StockTransacitionId);
                entity.HasIndex(il => il.ProductId);
                entity.HasIndex(il => il.PurchaseOrderId);

                // Composite index for common queries
                entity.HasIndex(il => new { il.ProductId, il.Log_Date });
            });
            modelBuilder.Entity<PurchaseOrder>(entity =>
            {
                entity.ToTable("PurchaseOrders");

                // Properties configuration
                entity.Property(po => po.Id)
                    .ValueGeneratedOnAdd();

                entity.Property(po => po.OrderNumber)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(po => po.OrderDate)
                    .IsRequired()
                    .HasDefaultValueSql("GETDATE()");

                entity.Property(po => po.ExpectedDate)
                    .IsRequired();

                entity.Property(po => po.TotalAmount)
                    .IsRequired()
                    .HasColumnType("decimal(18,2)");

                entity.Property(po => po.Note)
                    .HasMaxLength(1000);

                entity.Property(po => po.OrderStatus)
                    .IsRequired()
                    .HasConversion<string>()
                    .HasMaxLength(20)
                    .HasDefaultValue(Status.Draft);

                entity.Property(po => po.CreatedAt)
                    .IsRequired()
                    .HasDefaultValueSql("GETDATE()");

                entity.Property(po => po.UpdatedAt)
                    .IsRequired(false);

                // Relationships
                entity.HasOne(po => po.Supplier)
                    .WithMany(s => s.PurchaseOrders)
                    .HasForeignKey(po => po.SupplierId)
                    .OnDelete(DeleteBehavior.SetNull); // Set SupplierId to null if supplier is deleted

                entity.HasMany(po => po.OrderItemSuppliers)  // Note: Typo in property name (should likely be OrderItemSuppliers)
                    .WithOne(ois => ois.PurchaseOrder)
                    .HasForeignKey(ois => ois.PurchaseOrderId)
                    .OnDelete(DeleteBehavior.Cascade); // Delete all items when PO is deleted

                entity.HasMany(po => po.InventoryLogs)
                    .WithOne(il => il.PurchaseOrder)
                    .HasForeignKey(il => il.PurchaseOrderId)
                    .OnDelete(DeleteBehavior.Restrict); // Prevent PO deletion if inventory logs exist

                // Indexes
                entity.HasIndex(po => po.OrderNumber)
                    .IsUnique();

                entity.HasIndex(po => po.OrderDate);
                entity.HasIndex(po => po.ExpectedDate);
                entity.HasIndex(po => po.OrderStatus);
                entity.HasIndex(po => po.SupplierId);
                entity.HasIndex(po => po.CreatedAt);

                // Composite indexes for common queries
                entity.HasIndex(po => new { po.OrderStatus, po.ExpectedDate });
                entity.HasIndex(po => new { po.SupplierId, po.OrderDate });
            });
            modelBuilder.Entity<OrderItemSuppliers>(entity =>
            {
                entity.ToTable("OrderItemSuppliers");

                // Properties configuration
                entity.Property(oi => oi.Id)
                    .ValueGeneratedOnAdd();

                entity.Property(oi => oi.Quantity)
                    .IsRequired();

                entity.Property(oi => oi.UnitPrice)
                    .IsRequired()
                    .HasColumnType("decimal(18,2)");

                entity.Property(oi => oi.RecievedQuantity)  // Note: Typo (should be ReceivedQuantity)
                    .IsRequired()
                    .HasDefaultValue(0);

                entity.Property(oi => oi.StatusOrderItemSupplier)
                    .IsRequired()
                    .HasConversion<string>()
                    .HasMaxLength(20)
                    .HasDefaultValue(StatusOrderItemSupplier.Pending);

                // Relationships
                entity.HasOne(oi => oi.Product)
                    .WithMany(p => p.OrderItemSuppliers)
                    .HasForeignKey(oi => oi.ProductId)
                    .OnDelete(DeleteBehavior.SetNull); // Set ProductId to null if product is deleted

                entity.HasOne(oi => oi.PurchaseOrder)
                    .WithMany(po => po.OrderItemSuppliers)
                    .HasForeignKey(oi => oi.PurchaseOrderId)
                    .OnDelete(DeleteBehavior.Cascade); // Delete item when PO is deleted

                // Indexes
                entity.HasIndex(oi => oi.ProductId);
                entity.HasIndex(oi => oi.PurchaseOrderId);
                entity.HasIndex(oi => oi.StatusOrderItemSupplier);

                // Composite index for common queries
                entity.HasIndex(oi => new { oi.PurchaseOrderId, oi.StatusOrderItemSupplier });
            });
        }
    }
}
