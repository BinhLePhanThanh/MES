using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Formula> Formulas => Set<Formula>();
    public DbSet<Material> Materials => Set<Material>();
    public DbSet<FormulaMaterial> FormulaMaterials => Set<FormulaMaterial>();
    public DbSet<Unit> Units => Set<Unit>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Type> Types => Set<Type>();
    public DbSet<Property> Properties => Set<Property>();
    public DbSet<FormulaProperty> FormulaProperties => Set<FormulaProperty>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<FormulaLog> FormulaLogs { get; set; }
    public DbSet<FormulaFile> FormulaFiles { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FormulaMaterial>()
            .HasKey(fm => new { fm.FormulaId, fm.MaterialId });

        modelBuilder.Entity<FormulaMaterial>()
            .HasOne(fm => fm.Formula)
            .WithMany(f => f.FormulaMaterials)
            .HasForeignKey(fm => fm.FormulaId);

        modelBuilder.Entity<FormulaMaterial>()
            .HasOne(fm => fm.Material)
            .WithMany(m => m.FormulaMaterials)
            .HasForeignKey(fm => fm.MaterialId);

        modelBuilder.Entity<FormulaMaterial>()
            .HasOne(fm => fm.Unit)
            .WithMany(u => u.FormulaMaterials)
            .HasForeignKey(fm => fm.UnitId);

        modelBuilder.Entity<Formula>()
            .HasOne(f => f.CreatedBy)
            .WithMany(e => e.CreatedFormulas)
            .HasForeignKey(f => f.CreatedById)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Formula>()
            .HasOne(f => f.ProcessedBy)
            .WithMany(e => e.ApprovedFormulas)
            .HasForeignKey(f => f.ProcessedById)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Product>()
            .HasOne(p => p.CreatedBy)
            .WithMany(e => e.CreatedProducts)
            .HasForeignKey(p => p.CreatedById)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<FormulaProperty>()
.HasKey(fp => new { fp.FormulaId, fp.PropertyId });

        modelBuilder.Entity<FormulaProperty>()
            .HasOne(fp => fp.Formula)
            .WithMany(f => f.FormulaProperties)
            .HasForeignKey(fp => fp.FormulaId);

        modelBuilder.Entity<FormulaProperty>()
            .HasOne(fp => fp.Property)
            .WithMany(p => p.FormulaProperties)
            .HasForeignKey(fp => fp.PropertyId);

        modelBuilder.Entity<FormulaProperty>()
            .HasOne(fp => fp.Unit)
            .WithMany()
            .HasForeignKey(fp => fp.UnitId);

    }
}
