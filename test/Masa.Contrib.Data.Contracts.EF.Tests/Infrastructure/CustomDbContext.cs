using Masa.Contrib.Data.Contracts.EF.Tests.Domain.Entities;

namespace Masa.Contrib.Data.Contracts.EF.Tests.Infrastructure;

public class CustomDbContext : MasaDbContext
{
    public DbSet<Students> Students { get; set; }

    public DbSet<Courses> Courses { get; set; }

    public CustomDbContext(MasaDbContextOptions<CustomDbContext> options) : base(options) { }
}