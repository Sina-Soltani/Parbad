using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Parbad.Data.Context
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ParbadDataContext>
    {
        public ParbadDataContext CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<ParbadDataContext>();

            const string connectionString = "Server=.;Database=Parbad;Trusted_Connection=True;";

            builder.UseSqlServer(connectionString);

            return new ParbadDataContext(builder.Options);
        }
    }
}
