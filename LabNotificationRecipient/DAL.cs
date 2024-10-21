using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;
namespace LabNotificationRecipient
{
    public class DAL
    {
        private string connectionString = string.Empty;
        public DAL(string _connectionString)
        {
            connectionString = _connectionString;
        }

        public SqlConnection ConnectDB()
        {
            SqlConnection conn = new SqlConnection(connectionString);
            try
            {
                
                conn.Open();
                if (conn.State != System.Data.ConnectionState.Open)
                {
                    conn = null;
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.WriteError("AttachmentsSync - Error", DateTime.Now.ToString() + "--Err Msg" + ex.Message.ToString(), 1);
            }
            return conn;
        }
        public DataTable ExecuteQuery(string query, SqlParameter [] sqlParams)
        {
            DataTable dtResult = new DataTable();
            try
            {
                var con = ConnectDB();
                if (con !=null)
                {
                    using (con)
                    {
                        SqlCommand cmnd = new SqlCommand(query, con);
                        if (sqlParams != null)
                        {
                            foreach (SqlParameter p in sqlParams)
                            {
                                cmnd.Parameters.Add(p);
                            }
                        }
                        SqlDataAdapter adptr = new SqlDataAdapter(cmnd);
                        adptr.Fill(dtResult);
                        con.Close();
                    }
                    con.Dispose();
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.WriteError("AttachmentsSync - Error", DateTime.Now.ToString() + "--Err Msg" + ex.Message.ToString(), 0);
            }
            return dtResult;
        }
        public bool ExecuteNonQuery(string query, SqlParameter[] parameters)
        {

            bool result = true;
            try
            {
                var con = ConnectDB();
                if (con != null)
                {
                    using (con)
                    {
                        SqlCommand cmnd = new SqlCommand(query, con);
                        if (parameters != null)
                        {
                            foreach (SqlParameter p in parameters)
                            {
                                cmnd.Parameters.Add(p);
                            }
                        }
                       
                        cmnd.ExecuteNonQuery();
                        con.Close();
                    }
                    con.Dispose();
                }
            }
            catch (Exception ex)
            {
                result = false;
                Logger.Instance.WriteError("AttachmentsSync - Error", DateTime.Now.ToString() + "--Err Msg" + ex.Message.ToString(), 0);
                throw;
            }
            return result;
        }
    }
}
