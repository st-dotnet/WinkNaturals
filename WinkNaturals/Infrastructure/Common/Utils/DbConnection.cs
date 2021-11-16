using System.Data.SqlClient;

namespace WinkNatural.Web.Common.Utils
{
    public static class DbConnection
    {
        public static SqlConnection Sql()
        {
            return new SqlConnection("server=sharedbi.database.windows.net;database=WinkNaturalsReporting;uid=WinkNaturals_exigoweb;pwd=d6jTpYX7Hr2JREUuzqZ5aeF8;pooling=false;Encrypt=True;TrustServerCertificate=True;");
        }
        public static SqlConnection Sql(string connectionString)
        {
            return new SqlConnection(connectionString);
        }
    }
}
