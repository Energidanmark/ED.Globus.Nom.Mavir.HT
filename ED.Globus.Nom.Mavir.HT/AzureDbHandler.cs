using System.Collections.Generic;
using System.Data.Common;
using System.Data;
using System.Linq;
using System;
using System.Data.SqlClient;

namespace ED.Globus.Nom.Mavir.HT
{
    public class AzureDbHandler
    {
        private readonly FileLogger _log;

        public AzureDbHandler(FileLogger log)
        {
            _log = log;
        }  
        public bool ExecuteSql(string executeSql, string connectionString)
        {
            DbConnection sqlConnection = null;
            var didExecute = true;
            try
            {
                DbProviderFactories.RegisterFactory("System.Data.SqlClient", SqlClientFactory.Instance);
                var dbFactory = DbProviderFactories.GetFactory("System.Data.SqlClient");

                using (sqlConnection = dbFactory.CreateConnection())
                {
                    using (DbCommand cmd = sqlConnection.CreateCommand())
                    {
                        sqlConnection.ConnectionString = connectionString;
                        sqlConnection.Open();
                        cmd.Connection = sqlConnection;

                        cmd.CommandText = executeSql;

                        cmd.ExecuteNonQuery();
                    }

                }

            }
            catch (Exception ex)
            {
                didExecute = false;
                Console.WriteLine(ex.Message);
                _log.Debug(ex.Message);
            }
            finally
            {
                if (sqlConnection != null && sqlConnection.State != ConnectionState.Closed)
                {
                    sqlConnection.Close();
                }
            }

            return didExecute;
        }
        public bool ExecuteStoredProcedure(string executeStoredProcedure, string connectionString)
        {
            bool result = true;
            DbConnection sqlConnection = null;
            DbProviderFactories.RegisterFactory("System.Data.SqlClient", SqlClientFactory.Instance);
            var dbFactory = DbProviderFactories.GetFactory("System.Data.SqlClient");


            using (DbConnection conn = dbFactory.CreateConnection())
            {
                try
                {
                    conn.ConnectionString = connectionString;
                    conn.Open();

                    // 1.  create a command object identifying the stored procedure
                    DbCommand cmd = new SqlCommand();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Connection = conn;
                    cmd.CommandText =executeStoredProcedure;

                    // execute the command
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    _log?.Debug($"Failed to call import stored procdure '{executeStoredProcedure}' on {connectionString}. Ex: {ex.Message}");
                    result = false;
                }
            }


            return result;
        }
        public bool ExecuteErrorLogStoredProcedure(string message, bool errorOccured, string executeStoredProcedure, string connectionString)
        {
            bool result = true;
            
            var dbFactory = DbProviderFactories.GetFactory("System.Data.SqlClient");
            using (DbConnection conn = dbFactory.CreateConnection())
            {
                try
                {
                    conn.ConnectionString = connectionString;
                    conn.Open();

                    // 1.  create a command object identifying the stored procedure
                    DbCommand cmd = new SqlCommand();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@Path", SqlDbType.NVarChar));
                    cmd.Parameters.Add(new SqlParameter("@Message", SqlDbType.NVarChar));
                    cmd.Parameters.Add(new SqlParameter("@ErrorOccured", SqlDbType.Bit));

                    cmd.Parameters["@Path"].Value = @"Bigger Screen Intraday  C:\Users\Intraday\Desktop\ICS_Amprion\";

                    cmd.Parameters["@Message"].Value = message;

                    cmd.Parameters["@ErrorOccured"].Value = Convert.ToInt32(errorOccured);

                    cmd.Connection = conn;
                    cmd.CommandText = executeStoredProcedure;
                    cmd.CommandText = "[Logs].[LogCurrentState]";

                    // execute the command
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    //_log?.Debug($"Failed to call import stored procdure '{executeStoredProcedure}' on {connectionString}. Ex: {ex.Message}");
                    result = false;
                    _log.Debug(ex.Message);
                }
            }


            return result;
        }



        public List<DateTime> CallSpAndReturnDatesNewCapacityDataForDates(string executeSql, string connectionString)
        {
            DbConnection sqlConnection = null;
            List<DateTime> dates = new List<DateTime>();
            try
            {
                var dbFactory = DbProviderFactories.GetFactory("System.Data.SqlClient");

                using (sqlConnection = dbFactory.CreateConnection())
                {
                    using (DbCommand cmd = sqlConnection.CreateCommand())
                    {
                        sqlConnection.ConnectionString = connectionString;
                        sqlConnection.Open();
                        cmd.Connection = sqlConnection;
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.CommandText = executeSql;

                        var sqlParameter = new SqlParameter("@DataProvider", SqlDbType.NVarChar);
                        sqlParameter.Value = "Amprion";
                        cmd.Parameters.Add(sqlParameter);

                        var reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            DateTime dt = DateTime.MinValue;
                            DateTime.TryParse(reader[0]?.ToString(), out dt);
                            if (dt != DateTime.MinValue)
                            {
                                dates.Add(dt);
                            }
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                _log.Debug(ex.Message);
            }
            finally
            {
                if (sqlConnection != null && sqlConnection.State != ConnectionState.Closed)
                {
                    sqlConnection.Close();
                }
            }

            return dates;
        }
       
    }
}
