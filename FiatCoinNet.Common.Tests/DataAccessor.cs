using FiatCoinNet.Interface;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FiatCoinNet.Common.Tests
{
    public class DataAccessor : ITestability
    {
        public static readonly string ConnectionString = "Server=tcp:fiatcoindb.database.windows.net,1433;Database=FiatCoinDB;User ID=sqladmin@fiatcoindb;Password=#Bugsfor$;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";

        #region singleton instance

        private static DataAccessor _instance;

        private static DataAccessor Instance
        {
            get
            {
                if (_instance == null)
                {
                    Interlocked.CompareExchange(ref _instance, new DataAccessor(), null);
                }
                return _instance;
            }
        }

        private DataAccessor()
        {

        }

        #endregion

        #region accessors

        private static ITestability _testabilityOverrided;

        public static ITestability Testability
        {
            get { return _testabilityOverrided ?? Instance; }
            set { _testabilityOverrided = value; }
        }

        #endregion

        #region helper functions

        [SuppressMessage("Microsoft.Security", "CA2100:Sql Injection", Justification = "Only parametered stored procedures will be used here internally")]
        private DataSet QueryStoreProcedure(string spName, IDictionary<string, object> parameters)
        {
            var ds = new DataSet();

            using (var cnn = new SqlConnection(ConnectionString))
            using (var cmd = new SqlCommand())
            using (var adapter = new SqlDataAdapter(cmd))
            {
                cmd.Connection = cnn;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = spName;
                if (parameters != null && parameters.Count > 0)
                {
                    cmd.Parameters.AddRange(parameters.Select(p => new SqlParameter(p.Key, p.Value)).ToArray());
                }
                cnn.Open();
                adapter.Fill(ds);
            }

            return ds;
        }

        public void Cleanup()
        {
            QueryStoreProcedure("Cleanup", new Dictionary<string, object>());
        }
        #endregion
    }
}
