using ImageNet.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;

public class AppContext: DbContext
{
    public DbSet<User> Users{get;set;}
    public DbSet<Image> Images{get;set;}
    public AppContext(DbContextOptions<AppContext> options) : base(options)
    {
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().Metadata.FindNavigation("Images").SetPropertyAccessMode(PropertyAccessMode.Field);

       modelBuilder.Entity<User>()
                .HasMany(x => x.Friends)
                .WithMany();
       
    }

}