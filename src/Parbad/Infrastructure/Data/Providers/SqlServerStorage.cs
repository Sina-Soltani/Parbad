using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Parbad.Core;
using Parbad.Providers;
using Parbad.Utilities;

namespace Parbad.Infrastructure.Data.Providers
{
    /// <summary>
    /// A storage that uses SQL Server to read and write data.
    /// </summary>
    public class SqlServerStorage : Storage
    {
        /// <summary>
        /// Initializes SqlServerStorage by connection string and the name of payment's table.
        /// </summary>
        /// <param name="connectionString">connection string for connecting to SQL Server.</param>
        /// <param name="paymentsTableName">The name of payment's table.</param>
        public SqlServerStorage(string connectionString, string paymentsTableName)
        {
            if (connectionString.IsNullOrWhiteSpace())
            {
                throw new ArgumentNullException(nameof(connectionString));
            }

            if (paymentsTableName.IsNullOrWhiteSpace())
            {
                throw new ArgumentNullException(nameof(paymentsTableName));
            }

            ConnectionString = connectionString;

            PaymentsTableName = paymentsTableName;
        }

        public string ConnectionString { get; }

        public string PaymentsTableName { get; }

        protected internal override PaymentData SelectById(Guid id)
        {
            var query = $"SELECT * FROM [{PaymentsTableName}] WHERE ID=@ID";

            var sqlServerHelper = new SqlServerHelper(ConnectionString);

            var dataTable = sqlServerHelper.Read(query, new Dictionary<string, object>
            {
                {"ID", id}
            });

            if (dataTable.Rows.Count == 0)
            {
                return null;
            }

            var gateway = (Gateway)Convert.ToInt32(dataTable.Rows[0]["Gateway"]);
            var orderNumber = Convert.ToInt64(dataTable.Rows[0]["OrderNumber"]);
            var amount = long.Parse(dataTable.Rows[0]["Amount"]?.ToString());
            var referenceId = dataTable.Rows[0]["ReferenceID"]?.ToString();
            var transactionId = dataTable.Rows[0]["TransactionID"]?.ToString();
            var status = (PaymentDataStatus)Convert.ToInt32(dataTable.Rows[0]["Status"]);
            var additionalData = dataTable.Rows[0]["AdditionalData"]?.ToString();
            var createdOn = Convert.ToDateTime(dataTable.Rows[0]["CreatedOn"]);

            return new PaymentData
            {
                Id = id,
                Gateway = gateway,
                OrderNumber = orderNumber,
                Amount = amount,
                ReferenceId = referenceId,
                TransactionId = transactionId,
                Status = status,
                AdditionalData = additionalData,
                CreatedOn = createdOn
            };
        }

        protected internal override async Task<PaymentData> SelectByIdAsync(Guid id)
        {
            var query = $"SELECT * FROM [{PaymentsTableName}] WHERE ID=@ID";

            var sqlServerHelper = new SqlServerHelper(ConnectionString);

            var dataTable = await sqlServerHelper.ReadAsync(query, new Dictionary<string, object>
            {
                {"ID", id}
            });

            if (dataTable.Rows.Count == 0)
            {
                return null;
            }

            var gateway = (Gateway)Convert.ToInt32(dataTable.Rows[0]["Gateway"]);
            var orderNumber = Convert.ToInt64(dataTable.Rows[0]["OrderNumber"]);
            var amount = long.Parse(dataTable.Rows[0]["Amount"]?.ToString());
            var referenceId = dataTable.Rows[0]["ReferenceID"]?.ToString();
            var transactionId = dataTable.Rows[0]["TransactionID"]?.ToString();
            var status = (PaymentDataStatus)Convert.ToInt32(dataTable.Rows[0]["Status"]);
            var additionalData = dataTable.Rows[0]["AdditionalData"]?.ToString();
            var createdOn = Convert.ToDateTime(dataTable.Rows[0]["CreatedOn"]);

            return new PaymentData
            {
                Id = id,
                Gateway = gateway,
                OrderNumber = orderNumber,
                Amount = amount,
                ReferenceId = referenceId,
                TransactionId = transactionId,
                Status = status,
                AdditionalData = additionalData,
                CreatedOn = createdOn
            };
        }

        protected internal override PaymentData SelectByOrderNumber(long orderNumber)
        {
            var query = $"SELECT * FROM [{PaymentsTableName}] WHERE OrderNumber=@OrderNumber";

            var sqlServerHelper = new SqlServerHelper(ConnectionString);

            var dataTable = sqlServerHelper.Read(query, new Dictionary<string, object>
            {
                {"OrderNumber", orderNumber}
            });

            if (dataTable.Rows.Count == 0)
            {
                return null;
            }

            if (dataTable.Rows.Count > 1)
            {
                throw new Exception($"More than one record found for OrderNumber: {orderNumber}");
            }

            var id = Guid.Parse(dataTable.Rows[0]["ID"].ToString());
            var gateway = (Gateway)Convert.ToInt32(dataTable.Rows[0]["Gateway"]);
            var amount = long.Parse(dataTable.Rows[0]["Amount"]?.ToString());
            var referenceId = dataTable.Rows[0]["ReferenceID"]?.ToString();
            var transactionId = dataTable.Rows[0]["TransactionID"]?.ToString();
            var status = (PaymentDataStatus)Convert.ToInt32(dataTable.Rows[0]["Status"]);
            var additionalData = dataTable.Rows[0]["AdditionalData"]?.ToString();
            var createdOn = Convert.ToDateTime(dataTable.Rows[0]["CreatedOn"]);

            return new PaymentData
            {
                Id = id,
                Gateway = gateway,
                OrderNumber = orderNumber,
                Amount = amount,
                ReferenceId = referenceId,
                TransactionId = transactionId,
                Status = status,
                AdditionalData = additionalData,
                CreatedOn = createdOn
            };
        }

        protected internal override async Task<PaymentData> SelectByOrderNumberAsync(long orderNumber)
        {
            var query = $"SELECT * FROM [{PaymentsTableName}] WHERE OrderNumber=@OrderNumber";

            var sqlServerHelper = new SqlServerHelper(ConnectionString);

            var dataTable = await sqlServerHelper.ReadAsync(query, new Dictionary<string, object>
            {
                {"OrderNumber", orderNumber}
            });

            if (dataTable.Rows.Count == 0)
            {
                return null;
            }

            if (dataTable.Rows.Count > 1)
            {
                throw new Exception($"More than one record found for OrderNumber: {orderNumber}");
            }

            var id = Guid.Parse(dataTable.Rows[0]["ID"].ToString());
            var gateway = (Gateway)Convert.ToInt32(dataTable.Rows[0]["Gateway"]);
            var amount = long.Parse(dataTable.Rows[0]["Amount"]?.ToString());
            var referenceId = dataTable.Rows[0]["ReferenceID"]?.ToString();
            var transactionId = dataTable.Rows[0]["TransactionID"]?.ToString();
            var status = (PaymentDataStatus)Convert.ToInt32(dataTable.Rows[0]["Status"]);
            var additionalData = dataTable.Rows[0]["AdditionalData"]?.ToString();
            var createdOn = Convert.ToDateTime(dataTable.Rows[0]["CreatedOn"]);

            return new PaymentData
            {
                Id = id,
                Gateway = gateway,
                OrderNumber = orderNumber,
                Amount = amount,
                ReferenceId = referenceId,
                TransactionId = transactionId,
                Status = status,
                AdditionalData = additionalData,
                CreatedOn = createdOn
            };
        }

        protected internal override void Insert(PaymentData paymentData)
        {
            var sqlServerHelper = new SqlServerHelper(ConnectionString);

            var query = $"INSERT INTO [{PaymentsTableName}] " +
                                 "(ID, " +
                                 "OrderNumber, " +
                                 "Gateway, " +
                                 "Amount, " +
                                 "ReferenceID, " +
                                 "TransactionID, " +
                                 "Status, " +
                                 "AdditionalData, " +
                                 "CreatedOn)" +
                                 " VALUES" +
                                 "(@ID, " +
                                 "@OrderNumber, " +
                                 "@Gateway, " +
                                 "@Amount, " +
                                 "@ReferenceID, " +
                                 "@TransactionID, " +
                                 "@Status, " +
                                 "@AdditionalData, " +
                                 "@CreatedOn)";

            var affectedRows = sqlServerHelper.Execute(query, new Dictionary<string, object>
            {
                {"ID", paymentData.Id},
                {"OrderNumber", paymentData.OrderNumber},
                {"Gateway", (int)paymentData.Gateway},
                {"Amount", paymentData.Amount},
                {"ReferenceID", paymentData.ReferenceId},
                {"TransactionID", paymentData.TransactionId},
                {"Status", (int)paymentData.Status},
                {"AdditionalData", paymentData.AdditionalData},
                {"CreatedOn", paymentData.CreatedOn}
            });

            if (affectedRows == 0)
            {
                throw new Exception("No data inserted in SQL Server Database");
            }
        }

        protected internal override async Task InsertAsync(PaymentData paymentData)
        {
            var sqlServerHelper = new SqlServerHelper(ConnectionString);

            var query = $"INSERT INTO [{PaymentsTableName}] " +
                        "(ID, " +
                        "OrderNumber, " +
                        "Gateway, " +
                        "Amount, " +
                        "ReferenceID, " +
                        "TransactionID, " +
                        "Status, " +
                        "AdditionalData, " +
                        "CreatedOn)" +
                        " VALUES" +
                        "(@ID, " +
                        "@OrderNumber, " +
                        "@Gateway, " +
                        "@Amount, " +
                        "@ReferenceID, " +
                        "@TransactionID, " +
                        "@Status, " +
                        "@AdditionalData, " +
                        "@CreatedOn)";

            var affectedRows = await sqlServerHelper.ExecuteAsync(query, new Dictionary<string, object>
            {
                {"ID", paymentData.Id},
                {"OrderNumber", paymentData.OrderNumber},
                {"Gateway", (int)paymentData.Gateway},
                {"Amount", paymentData.Amount},
                {"ReferenceID", paymentData.ReferenceId},
                {"TransactionID", paymentData.TransactionId},
                {"Status", (int)paymentData.Status},
                {"AdditionalData", paymentData.AdditionalData},
                {"CreatedOn", paymentData.CreatedOn}
            });

            if (affectedRows == 0)
            {
                throw new Exception("No data inserted in SQL Server Database");
            }
        }

        protected internal override void Update(PaymentData paymentData)
        {
            var sqlServerHelper = new SqlServerHelper(ConnectionString);

            var query = $"UPDATE [{PaymentsTableName}] SET " +
                                 "Amount=@Amount, " +
                                 "ReferenceID=@ReferenceID, " +
                                 "TransactionID=@TransactionID, " +
                                 "Status=@Status, " +
                                 "AdditionalData=@AdditionalData" +
                                 " WHERE ID=@ID";

            var affectedRows = sqlServerHelper.Execute(query, new Dictionary<string, object>
            {
                {"ID", paymentData.Id},
                {"Amount", paymentData.Amount},
                {"ReferenceID", paymentData.ReferenceId},
                {"TransactionID", paymentData.TransactionId},
                {"Status", (int)paymentData.Status},
                {"AdditionalData", paymentData.AdditionalData}
            });

            if (affectedRows == 0)
            {
                throw new Exception($"No data updated in SQL Server Database for ID: {paymentData.Id}");
            }

            if (affectedRows > 1)
            {
                throw new Exception($"More than one record has been updated in SQL Server Database for ID: {paymentData.Id}");
            }
        }

        protected internal override async Task UpdateAsync(PaymentData paymentData)
        {
            var sqlServerHelper = new SqlServerHelper(ConnectionString);

            var query = $"UPDATE [{PaymentsTableName}] SET " +
                        "Amount=@Amount, " +
                        "ReferenceID=@ReferenceID, " +
                        "TransactionID=@TransactionID, " +
                        "Status=@Status, " +
                        "AdditionalData=@AdditionalData" +
                        " WHERE ID=@ID";

            var affectedRows = await sqlServerHelper.ExecuteAsync(query, new Dictionary<string, object>
            {
                {"ID", paymentData.Id},
                {"Amount", paymentData.Amount},
                {"ReferenceID", paymentData.ReferenceId},
                {"TransactionID", paymentData.TransactionId},
                {"Status", (int)paymentData.Status},
                {"AdditionalData", paymentData.AdditionalData}
            });

            if (affectedRows == 0)
            {
                throw new Exception($"No data updated in SQL Server Database for ID: {paymentData.Id}");
            }

            if (affectedRows > 1)
            {
                throw new Exception($"More than one record has been updated in SQL Server Database for ID: {paymentData.Id}");
            }
        }
    }
}