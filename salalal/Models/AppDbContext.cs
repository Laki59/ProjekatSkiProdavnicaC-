using Microsoft.EntityFrameworkCore;

using salalal.Models;
using System.Data;

public class AppDbContext : DbContext
{
    //Konstruktor koji prima opcije i prosledjuje ih baznoj klasi
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    //DbSet svojstva koja predstavljaju tabele u bazi
    public DbSet<User> Users { get; set; }
    public DbSet<Ski> Skis { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {   //Povezuje OrderItem sa Ski pomoću foreign key-a (SkiId)
        modelBuilder.Entity<OrderItem>()
            .HasOne(oi => oi.Ski)
            .WithMany()
            .HasForeignKey(oi => oi.SkiId);

        // Slika moze biti null(bug bez ovoga?)
        modelBuilder.Entity<Ski>()
            .Property(s => s.ImagePath)
            .HasMaxLength(255)
            .IsRequired(false);

        modelBuilder.Entity<User>().HasData(
            new User { Id = 1, Username = "admin", Password = "admin123", Role= "Admin" },
            new User { Id = 2, Username = "customer", Password = "customer123", Role="Customer" },
            new User { Id = 3, Username = "employee", Password = "employee123", Role="Employee" }
        );

        modelBuilder.Entity<Ski>().HasData(
    new Ski
    {
        Id = 1,
        Name = "Nove skije",
        Model = "Slope",
        StockQuantity = 4,
        Price = 700,
        ImagePath = "/Images/p50323_the_curv_ti_3.jpg"
    },
    new Ski
    {
        Id = 2,
        Name = "Fischer skije",
        Model = "All-Mountain",
        StockQuantity = 10,
        Price = 500,
        ImagePath = "/Images/SetWidth960-elan-rc-comprex-75-years3.png"
    });
    }
}
