using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Parbad.Utilities
{
    internal class SqlServerHelper
    {
        private readonly string _connectionString;

        public SqlServerHelper(string connectionString)
        {
            _connectionString = connectionString;
        }

        public DataTable Read(string query, IDictionary<string, object> parameters = null)
        {
            DataTable dataTable;

            using (var sqlConnection = new SqlConnection(_connectionString))
            {
                using (var sqlCommand = new SqlCommand(query, sqlConnection))
                {
                    if (parameters != null)
                    {
                        foreach (var parameter in parameters)
                        {
                            sqlCommand.Parameters.AddWithValue(parameter.Key, parameter.Value);
                        }
                    }

                    sqlConnection.Open();

                    using (var sqlDataReader = sqlCommand.ExecuteReader())
                    {
                        dataTable = new DataTable();

                        dataTable.Load(sqlDataReader);
                    }
                }
            }

            return dataTable;
        }

        public async Task<DataTable> ReadAsync(string query, IDictionary<string, object> parameters = null)
        {
            DataTable dataTable;

            using (var sqlConnection = new SqlConnection(_connectionString))
            {
                using (var sqlCommand = new SqlCommand(query, sqlConnection))
                {
                    if (parameters != null)
                    {
                        foreach (var parameter in parameters)
                        {
                            sqlCommand.Parameters.AddWithValue(parameter.Key, parameter.Value);
                        }
                    }

                    await sqlConnection.OpenAsync();

                    using (var sqlDataReader = await sqlCommand.ExecuteReaderAsync())
                    {
                        dataTable = new DataTable();

                        dataTable.Load(sqlDataReader);
                    }
                }
            }

            return dataTable;
        }

        public int Execute(string query, IDictionary<string, object> parameters = null)
        {
            using (var sqlConnection = new SqlConnection(_connectionString))
            {
                using (var sqlCommand = new SqlCommand(query, sqlConnection))
                {
                    if (parameters != null)
                    {
                        foreach (var parameter in parameters)
                        {
                            sqlCommand.Parameters.AddWithValue(parameter.Key, parameter.Value ?? DBNull.Value);
                        }
                    }

                    sqlConnection.Open();

                    return sqlCommand.ExecuteNonQuery();
                }
            }
        }

        public async Task<int> ExecuteAsync(string query, IDictionary<string, object> parameters = null)
        {
            using (var sqlConnection = new SqlConnection(_connectionString))
            {
                using (var sqlCommand = new SqlCommand(query, sqlConnection))
                {
                    if (parameters != null)
                    {
                        foreach (var parameter in parameters)
                        {
                            sqlCommand.Parameters.AddWithValue(parameter.Key, parameter.Value ?? DBNull.Value);
                        }
                    }

                    await sqlConnection.OpenAsync();

                    return await sqlCommand.ExecuteNonQueryAsync();
                }
            }
        }
    }
}