// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Parbad.Storage.EntityFrameworkCore.Context
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
