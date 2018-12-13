using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TurnerStarterApi.Core.Data
{
    public class DataContextFactory : IDesignTimeDbContextFactory<DataContext>
    {
        public DataContext CreateDbContext(string[] args)
        {
            return new DataContext(new DbContextOptionsBuilder<DataContext>().Options);
        }
    }
}