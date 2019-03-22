// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Parbad.Data.Context;

namespace Parbad.Internal
{
    public class ParbadDatabaseCreator : IParbadDatabaseCreator
    {
        private readonly ParbadDataContext _dataContext;
        private readonly ILogger<IOnlinePayment> _logger;

        private static bool _isCreated;

        public ParbadDatabaseCreator(ParbadDataContext dataContext, ILogger<IOnlinePayment> logger)
        {
            _dataContext = dataContext;
            _logger = logger;
        }

        public void Create()
        {
            if (_isCreated) return;

            _isCreated = true;

            if (_dataContext.Database.IsInMemory())
            {
                _logger.LogInformation(LoggingEvents.DatabaseCreating, "Creating the database.");

                _dataContext.Database.EnsureCreated();

                _logger.LogInformation(LoggingEvents.DatabaseCreated, "Database created.");
            }
            else
            {
                _logger.LogInformation(LoggingEvents.DatabaseCreating, "Creating and migrating the database.");

                _dataContext.Database.Migrate();

                _logger.LogInformation(LoggingEvents.DatabaseCreated, "Database created and migrated.");
            }
        }

        public Task CreateAsync(CancellationToken cancellationToken = default)
        {
            if (_isCreated) return Task.CompletedTask;

            _isCreated = true;

            if (_dataContext.Database.IsInMemory())
            {
                return _dataContext.Database.EnsureCreatedAsync(cancellationToken);
            }

            return _dataContext.Database.MigrateAsync(cancellationToken);
        }
    }
}
