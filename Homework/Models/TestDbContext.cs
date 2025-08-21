using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Homework.Models;

public partial class TestDbContext : DbContext
{
    public TestDbContext()
    {
    }

    public TestDbContext(DbContextOptions<TestDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Account> Accounts { get; set; }

    public virtual DbSet<Author> Authors { get; set; }

    public virtual DbSet<Book> Books { get; set; }

    public virtual DbSet<Emp> Emps { get; set; }

    public virtual DbSet<EmpSalary> EmpSalaries { get; set; }

    public virtual DbSet<FeePayment> FeePayments { get; set; }

    public virtual DbSet<InteriorWork> InteriorWorks { get; set; }

    public virtual DbSet<Inventory> Inventories { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<RefreshToken> RefreshTokens { get; set; }

    public virtual DbSet<SalesDatum> SalesData { get; set; }

    public virtual DbSet<StudGrade> StudGrades { get; set; }

    public virtual DbSet<Student> Students { get; set; }

    public virtual DbSet<StudentBook> StudentBooks { get; set; }

    public virtual DbSet<StudentGrade> StudentGrades { get; set; }

    public virtual DbSet<UserDetail> UserDetails { get; set; }

    public virtual DbSet<WorkItem> WorkItems { get; set; }

    public virtual DbSet<Worker> Workers { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=tcp:homeworkappdb.database.windows.net,1433;Initial Catalog=TestDb;Persist Security Info=False;User ID=HomeworkIssuer;Password=;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(e => e.AccountId).HasName("PK__Accounts__349DA5A6C5A79F44");

            entity.Property(e => e.Balance).HasComputedColumnSql("([TotalFee]-[TotalPaid])", true);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Student).WithMany(p => p.Accounts).HasConstraintName("FK_Accounts_Students");
        });

        modelBuilder.Entity<Author>(entity =>
        {
            entity.HasKey(e => e.AuthorId).HasName("PK__Authors__70DAFC345E34BAE1");
        });

        modelBuilder.Entity<Book>(entity =>
        {
            entity.HasKey(e => e.BookId).HasName("PK__Books__3DE0C207024BB0A2");

            entity.HasOne(d => d.Author).WithMany(p => p.Books).HasConstraintName("FK__Books__AuthorId__7E37BEF6");
        });

        modelBuilder.Entity<FeePayment>(entity =>
        {
            entity.HasKey(e => e.PaymentId).HasName("PK__FeePayme__9B556A3861111E41");

            entity.Property(e => e.PaymentDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();

            entity.HasOne(d => d.Student).WithMany(p => p.FeePayments).HasConstraintName("FK__FeePaymen__Stude__160F4887");
        });

        modelBuilder.Entity<InteriorWork>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Interior__3214EC074B6A468C");
        });

        modelBuilder.Entity<Inventory>(entity =>
        {
            entity.HasKey(e => e.BookId).HasName("PK__Inventor__3DE0C207EE7A4473");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.OrderId).HasName("PK__Orders__C3905BAFB199CA91");
        });

        modelBuilder.Entity<SalesDatum>(entity =>
        {
            entity.HasKey(e => e.SaleId).HasName("PK__SalesDat__1EE3C41FBB7F38C6");

            entity.Property(e => e.SaleId).ValueGeneratedNever();
        });

        modelBuilder.Entity<Student>(entity =>
        {
            entity.HasKey(e => e.StudentId).HasName("PK__Students__32C52B991EFE652D");

            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();
        });

        modelBuilder.Entity<StudentBook>(entity =>
        {
            entity.HasKey(e => new { e.StudentId, e.BookId }).HasName("PK__StudentB__511B27B960AD639C");

            entity.HasOne(d => d.Book).WithMany(p => p.StudentBooks)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__StudentBo__BookI__01142BA1");

            entity.HasOne(d => d.Student).WithMany(p => p.StudentBooks)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__StudentBo__Stude__00200768");
        });

        modelBuilder.Entity<StudentGrade>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__StudentG__3213E83F64B1BA5B");
        });

        modelBuilder.Entity<UserDetail>(entity =>
        {
            entity.HasKey(e => e.UserDetailId).HasName("PK__UserDeta__564F56B253077127");

            entity.HasOne(d => d.Student).WithMany(p => p.UserDetails)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__UserDetai__Stude__2CF2ADDF");
        });

        modelBuilder.Entity<WorkItem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__WorkItem__3214EC07B1915DA1");
        });

        modelBuilder.Entity<Worker>(entity =>
        {
            entity.Property(e => e.Name).IsFixedLength();
            entity.Property(e => e.WorkType).IsFixedLength();
            entity.Property(e => e.WorkerId).IsFixedLength();
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
