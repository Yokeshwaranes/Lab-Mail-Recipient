using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

namespace LabNotificationRecipient
{
    class LabNotificationRecipient
    {
        public static string SETUPID;
        public static string PROJECTID;
        public static DateTime Currentdate = DateTime.Now;
        public static string Notificationdate = Currentdate.AddDays(-1).ToString("yyyy-MM-dd");
        public static string ToEmail = string.Empty;
        public static string connectionString = null;
        public static string connectionStringLab = null;
        public static string TemplateId;
        public static string ReportId;
       // public static string ParameterGroup;
        //public static string ParameterType;
        
       

        public LabNotificationRecipient()
        {

            PROJECTID = ConfigurationManager.AppSettings["Projectid"];
            SETUPID = ConfigurationManager.AppSettings["Setupid"];
            //ToEmail = ConfigurationManager.AppSettings["ToEmail"];
            connectionString = ConfigurationManager.AppSettings["Main.ConnectionString"];
            TemplateId = ConfigurationManager.AppSettings["TemplateId"];
            connectionStringLab = ConfigurationManager.AppSettings["Main.ConnectionStringLab"];
            ReportId= ConfigurationManager.AppSettings["ReportId"];
            //ParameterGroup = ConfigurationManager.AppSettings["ParameterGroup"];
            //ParameterType = ConfigurationManager.AppSettings["ParameterType"];
        }
        public void SendMail()
        {
            try
            {
                //Send email to Lab admin

                DataTable dtEmailSetup = GetEmailTemplate(short.Parse(TemplateId));// getting template from Hisgenx database
                
                DataTable dtCancerRecipient = GetCancerRecipient();// getting cancerpatient detail from Sentry database
                
                    
                
                
                DataTable dtLabMailRecipient = GetLabMailRecipient();// whom to send mail from LabMailRecipient
                if (dtEmailSetup != null && dtEmailSetup.Rows.Count > 0 ) 
                {
                    //Email Body
                    
                    if (dtLabMailRecipient != null && dtLabMailRecipient.Rows.Count > 0 && dtCancerRecipient.Rows.Count > 0 && dtCancerRecipient != null)
                    {
                        string table = GetHTMLBuilder(dtCancerRecipient);// returning as table                    
                        foreach (DataRow row in dtLabMailRecipient.Rows)
                        {
                            DateTime Date =DateTime.Parse(Notificationdate);
                            string formattedDate=Date.ToString("dd-MM-yyyy");
                            string emailBody = Convert.ToString(dtEmailSetup.Rows[0]["EmailTemplate"]);
                            string emailSubject = Convert.ToString(dtEmailSetup.Rows[0]["EmailSubject"]);
                            emailBody = emailBody.Replace("{strdate}", formattedDate);
                            emailBody = emailBody.Replace("*Table*", table);
                           // emailSubject = emailSubject.Replace("{DoctorName}", Convert.ToString(row["DOCTORNAME"]));
                            string mailID = Convert.ToString(row["EmailId"]);

                            string emailType = "LabNotificationRecipient";
                           

                            string ccMembers = string.Empty;
                            string bccMembers = string.Empty;
                            using (SqlConnection connection = new SqlConnection(connectionString))
                            {
                                using (SqlCommand command = new SqlCommand("HISGENX.dbo.CreateEmailQueue", connection))
                                {
                                    command.CommandType = CommandType.StoredProcedure;
                                    command.Parameters.Add("@SETUPID", SqlDbType.VarChar).Value = SETUPID;
                                    command.Parameters.Add("@PROJECTID", SqlDbType.VarChar).Value = PROJECTID;
                                    command.Parameters.Add("@EmailType", SqlDbType.VarChar).Value = emailType;
                                    command.Parameters.Add("@EmailTo", SqlDbType.VarChar).Value = mailID;
                                    command.Parameters.Add("@EmailCC", SqlDbType.VarChar).Value = ccMembers;
                                    command.Parameters.Add("@EmailBCC", SqlDbType.VarChar).Value = bccMembers;
                                    command.Parameters.Add("@EmailSubject", SqlDbType.VarChar).Value = emailSubject;
                                    command.Parameters.Add("@EmailBody", SqlDbType.VarChar).Value = emailBody;
                                    connection.Open();
                                    command.ExecuteNonQuery();

                                }
                            }
                        }

                    }
                    else
                        Logger.WriteError(null, "There is no record");
                }


                else
                    
                    Logger.WriteError(null, "Email setup is not configured");
            }
            catch (Exception ex)
            {
                
                Logger.WriteError(null, ex.Message);
            }
        }

        public static DataTable GetEmailTemplate(short emailTemplateId)
        {
            DataTable dtResult = new DataTable();
            try
            {

                string query = "Exec [HISGENX].[dbo].[HIS_GetEmailTemplate] " + "'" + SETUPID + "'" + "," + PROJECTID + "," + "'" + emailTemplateId + "'";
                DAL oprojectDAL = new DAL(connectionString);
                dtResult = oprojectDAL.ExecuteQuery(query, null);
            }
            catch (Exception ex)
            {
                Logger.WriteError(null, ex.Message);
            }
            return dtResult;
        }
        public static DataTable GetLabMailRecipient()
        {
            DataTable dtResult = new DataTable();
            try
            {
                string query = "Select EmailId from Lab_OncologyRecipients where SETUPID= " + "'" + SETUPID + "'" + "and Projectid=" + "'" + PROJECTID + "'" +"and IsActive='1'";
                DAL oprojectDAL = new DAL(connectionString);
                dtResult = oprojectDAL.ExecuteQuery(query, null);
            }
            catch (Exception ex)
            {
                Logger.WriteError(null, ex.Message);
            }
            return dtResult;
        }

        public static DataTable GetCancerRecipient()
        {
            DataTable dtResult = new DataTable();
            try
            {
                string query = "SELECT SQLQUERY from [dbo].[STP_DynamicReport] where ReportId=" + "'" + ReportId +"' ";// + " + "'" + SETUPID + "'" + "," + "'" + PROJECTID + "'" + "," + "'" + Notificationdate + "'";
                DAL oprojectDAL = new DAL(connectionStringLab);
                dtResult = oprojectDAL.ExecuteQuery(query, null);
               
                if (dtResult.Rows.Count > 0)
                {

                  // string Notificationdatefrom = "2024-01-01";
                  //string Notificationdateto = "2024-07-31";
                    
                    query = Convert.ToString(dtResult.Rows[0]["SQLQUERY"]);
                    
                    System.Data.SqlClient.SqlParameter[] sqlParams = new System.Data.SqlClient.SqlParameter[4];
                    sqlParams[0] = new System.Data.SqlClient.SqlParameter("@SetupID", SETUPID);
                    sqlParams[1] = new System.Data.SqlClient.SqlParameter("@ProjectID", PROJECTID);
                    sqlParams[2] = new System.Data.SqlClient.SqlParameter("@DateFrom", Notificationdate);
                    sqlParams[3] = new System.Data.SqlClient.SqlParameter("@DateTo", Notificationdate);
                    oprojectDAL = new DAL(connectionStringLab);
                    dtResult = oprojectDAL.ExecuteQuery(query, sqlParams);

                }
               
               
            }

            catch (Exception ex)
            {
                Logger.WriteError(null, ex.Message);
            }
            return dtResult;
        }
        private  static string GetHTMLBuilder(DataTable dt)
        {
            string Htmltext=string.Empty;
            try
            {
                StringBuilder strHTMLBuilder = new StringBuilder();
               
            strHTMLBuilder.Append("<table border='1px' cellpadding='1' cellspacing='1' bgcolor='lightyellow' style='font-family:Garamond; font-size:smaller'>");
            strHTMLBuilder.Append("<tr >");
            foreach (DataColumn myColumn in dt.Columns)
            {
                strHTMLBuilder.Append("<td >");
                strHTMLBuilder.Append(myColumn.ColumnName);
                strHTMLBuilder.Append("</td>");
            }
            strHTMLBuilder.Append("</tr>");
            foreach (DataRow myRow in dt.Rows)
            {
                strHTMLBuilder.Append("<tr >");
                foreach (DataColumn myColumn in dt.Columns)
                {
                    strHTMLBuilder.Append("<td >");
                    strHTMLBuilder.Append(myRow[myColumn.ColumnName].ToString());
                    strHTMLBuilder.Append("</td>");
                }
                strHTMLBuilder.Append("</tr>");
            }

            strHTMLBuilder.Append("</table>");
             Htmltext = strHTMLBuilder.ToString();
            }

            catch (Exception ex)
            {
                Logger.WriteError(null, ex.Message);
            }
            return Htmltext;
        }
       

    }
}
