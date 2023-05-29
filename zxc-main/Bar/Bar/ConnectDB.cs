using Microsoft.Data.SqlClient;

namespace Bar
{

    public class ConnectDB
    {

        static public SqlConnection connectDB_TEAMDB()
        {
            SqlConnection sqlConnection_TEAMDB = new SqlConnection("SERVER=localhost;DATABASE=DB_QA;Integrated Security=true;TrustServerCertificate=true;");

            try
            {
                sqlConnection_TEAMDB.Open();
                return sqlConnection_TEAMDB;
            }
            catch (Exception e)
            {
                Log.writeLog(e.ToString());
                return null;
            }

        }
    }
}