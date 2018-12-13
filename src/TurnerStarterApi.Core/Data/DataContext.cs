using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using TurnerStarterApi.Core.Features.Security;

namespace TurnerStarterApi.Core.Data
{
    public class DataContextFactory : IDesignTimeDbContextFactory<DataContext>
    {
        public DataContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<DataContext>();

            return new DataContext(optionsBuilder.Options);
        }
    }

    public class DataContext : DbContext
    {
        private readonly IIdentityContext _identityContext;

        public DataContext(DbContextOptions options) : base(options)
        {
        }

        public DataContext(IIdentityContext identityContext, DbContextOptions options) : base(options)
        {
            _identityContext = identityContext;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder builder)
        {
            if (builder.IsConfigured)
            {
                return;
            }

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            builder.UseSqlServer(configuration.GetConnectionString("DataContext"));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.AddFromAssembly(typeof(DataContext).GetTypeInfo().Assembly);

            foreach (var relationship in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior = DeleteBehavior.Restrict;
            }

            base.OnModelCreating(modelBuilder);
        }

        public override int SaveChanges()
        {
            EditEntityProperties();

            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            EditEntityProperties();

            return base.SaveChangesAsync(cancellationToken);
        }

        private void EditEntityProperties()
        {
            foreach (var entity in ChangeTracker.Entries().Where(x => x.State == EntityState.Added).ToList())
            {
                if (!(entity.Entity is IEntity ientity))
                    continue;

                ientity.CreatedDate = DateTime.UtcNow;
                ientity.CreatedByUserId = _identityContext?.RequestingUser?.Id;
                ientity.ModifiedDate = DateTime.UtcNow;
                ientity.ModifiedByUserId = _identityContext?.RequestingUser?.Id;
            }

            foreach (var entity in ChangeTracker.Entries().Where(x => x.State == EntityState.Modified).ToList())
            {
                if (!(entity.Entity is IEntity ientity))
                    continue;

                ientity.ModifiedDate = DateTime.UtcNow;
                ientity.ModifiedByUserId = _identityContext?.RequestingUser?.Id;
            }

            foreach (var entity in ChangeTracker.Entries().Where(x => x.State == EntityState.Deleted).ToList())
            {
                if (!(entity.Entity is IEntity ientity))
                    continue;

                ientity.IsDeleted = true;
                ientity.DeletedDate = DateTime.UtcNow;
                ientity.DeletedByUserId = _identityContext?.RequestingUser?.Id;
                entity.State = EntityState.Modified;
            }
        }
    }
}
