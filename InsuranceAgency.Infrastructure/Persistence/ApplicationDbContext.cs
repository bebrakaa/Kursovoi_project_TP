using InsuranceAgency.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace InsuranceAgency.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<Agent> Agents => Set<Agent>();
    public DbSet<InsuranceService> InsuranceServices => Set<InsuranceService>();
    public DbSet<Contract> Contracts => Set<Contract>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<User> Users => Set<User>();
    public DbSet<OperationHistory> OperationHistories => Set<OperationHistory>();
    public DbSet<ContractApplication> ContractApplications => Set<ContractApplication>();
    public DbSet<DocumentVerification> DocumentVerifications => Set<DocumentVerification>();

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
