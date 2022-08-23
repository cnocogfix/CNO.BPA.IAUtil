using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using Oracle.ManagedDataAccess.Client;
using CNO.BPA.Framework;
//----------------------------------------------------------------
// InputAccel namespaces - QuickModule, Processing Helper,
// Script Helper, Scripting interface, and Workflow Client.
//----------------------------------------------------------------
using Emc.InputAccel.QuickModule;
using Emc.InputAccel.QuickModule.Helpers.Processing;
using Emc.InputAccel.QuickModule.Plugins.Processing;
using Emc.InputAccel.Workflow.Client;


namespace CNO.BPA.IAUtil
{
   public class RouteCode
   {
      private IValueProvider _taskValueProvider = null;
      private IHelper _quickModulehelper = null;

      private CNO.BPA.Framework.Cryptography crypto =
         new CNO.BPA.Framework.Cryptography();
      
      private string _DSN = "";
      private string _DBUser = "";
      private string _DBPass = "";

      //Error Cosntants
      public const int IA_SUCCESS = 0;
      public const int IA_ERR_UNKNOWN = -4523;
      public const int IA_ERR_NOFUNC = -4518;
      public const int IA_ERR_CANCEL = -4526;
      public const int IA_ERR_NORETRY = -6112;
      public const int IA_ERR_RETRYSOME = -6113;
      public const int IA_ERR_RETRY = -6114;
      public const int IA_ERR_ACCESS = -4505;

      public RouteCode(IValueProvider TaskValueProvider,IHelper QuickModulehelper, string DSN, string User, string Pass)
      {
         _taskValueProvider = TaskValueProvider;
         _quickModulehelper = QuickModulehelper;
         //next grab a copy of each of the db values
         _DSN = DSN;
         _DBUser = crypto.Decrypt(User);
         _DBPass = crypto.Decrypt(Pass);

      }

      public int launchRCSearch()
      {
         try
         {        
            int result = 0;
            //first let's check the route code in value
            if (_taskValueProvider.Get("routeCodeIn","").Trim() != "10")
            {
               //log debug info
               LogMessage(_quickModulehelper , "Calling the processRequest method.",LogType.Information);
               //first let's call the route code procedure
               result = processRequest();
            }
            else if (_taskValueProvider.Get("routeCodeIn", "").Trim() == "10")
            {
               //if the input was ten, make sure the out is the same
               _taskValueProvider.Set("routeCodeOut", "10");
            }
            //if there was an error during the route code process
            //then there is no reason to generate a document sequence
            if (result != 0)
            {
               //log debug info

               LogMessage(_quickModulehelper , "Failed calling the processRequest method using: RouteCode="
                  + _taskValueProvider.Get("routeCodeIn", "") + ", SiteId=" + _taskValueProvider.Get("siteID", "") + ", SortCode="
                  + _taskValueProvider.Get("sortCode", "") + ", FormType=" + _taskValueProvider.Get("formType", "") + ", VATIN=" 
                  + _taskValueProvider.Get("vaTIN", "") + ", TOB=" + _taskValueProvider.Get("typeOfBill", "") + ", HCFA Count=" 
                  + _taskValueProvider.Get("countHCFA", "") + ", UB92 Count=" + _taskValueProvider.Get("countUB92", "") 
                  + ", PHEOMB Count=" + _taskValueProvider.Get("countPHEOMB", "") + ", EOMB Count="
                  + _taskValueProvider.Get("countEOMB", ""),LogType.Information );
            }
         }
         catch (Exception ex)
         {
            //indicate a non-zero
            _taskValueProvider.Set("QMResult", IA_ERR_UNKNOWN.ToString());
            _taskValueProvider.Set("QMErrorDesc", ex.Message);
            _taskValueProvider.Set("QMErrorNo", "- 266047710");

            return -1;
         }

         //if we make it here, return success
         return 0;
      }


      private int processRequest()
      {         
         try
         {

            //build the connection string
            string BPAConnection = "Data Source=" + _DSN + ";Persist Security Info=True;User ID="
               + _DBUser + ";Password=" + _DBPass + "";
            //setup the database object            
            OracleConnection objConn = new OracleConnection(BPAConnection);
            //package name.stored procedure
            string strSQL = "BPA_APPS.PKG_STNDCLAIM.CLAIMS_ROUTING";
            using (objConn)
            {

               OracleCommand cmd = new OracleCommand(strSQL, objConn);
               cmd.CommandType = CommandType.StoredProcedure;
                    cmd.BindByName = true;

                    DBUtilities.CreateAndAddParameter("p_in_site",
                  _taskValueProvider.Get("siteID",""), OracleDbType.Varchar2, ParameterDirection.Input, cmd);

               DBUtilities.CreateAndAddParameter("p_in_sort_code",
                  _taskValueProvider.Get("sortCode",""), OracleDbType.Varchar2, ParameterDirection.Input, cmd);

               DBUtilities.CreateAndAddParameter("p_in_form_name",
                  _taskValueProvider.Get("formType",""), OracleDbType.Varchar2, ParameterDirection.Input, cmd);

               DBUtilities.CreateAndAddParameter("p_in_va_tin",
                  _taskValueProvider.Get("vaTIN",""), OracleDbType.Varchar2, ParameterDirection.Input, cmd);

               DBUtilities.CreateAndAddParameter("p_in_tob",
                  _taskValueProvider.Get("typeOfBill",""), OracleDbType.Varchar2, ParameterDirection.Input, cmd);

               DBUtilities.CreateAndAddParameter("p_in_hcfa_count",
                  _taskValueProvider.Get("countHCFA",""), OracleDbType.Varchar2, ParameterDirection.Input, cmd);

               DBUtilities.CreateAndAddParameter("p_in_ub92_count",
                 _taskValueProvider.Get("countUB92",""), OracleDbType.Varchar2, ParameterDirection.Input, cmd);

               DBUtilities.CreateAndAddParameter("p_in_eomb_count",
                  _taskValueProvider.Get("countEOMB",""), OracleDbType.Varchar2, ParameterDirection.Input, cmd);

               DBUtilities.CreateAndAddParameter("p_in_pheomb_count",
                  _taskValueProvider.Get("countPHEOMB",""), OracleDbType.Varchar2, ParameterDirection.Input, cmd);

               DBUtilities.CreateAndAddParameter("p_in_policy_number",
                  _taskValueProvider.Get("policyNo",""), OracleDbType.Varchar2, ParameterDirection.Input, cmd);

               DBUtilities.CreateAndAddParameter("p_in_company_code",
                  _taskValueProvider.Get("companyCode", ""), OracleDbType.Varchar2, ParameterDirection.Input, cmd);

               DBUtilities.CreateAndAddParameter("p_out_route", OracleDbType.Varchar2,
                  ParameterDirection.Output, 3, cmd);

               DBUtilities.CreateAndAddParameter("p_out_doc_type", OracleDbType.Varchar2,
                  ParameterDirection.Output, 10, cmd);

               DBUtilities.CreateAndAddParameter("p_out_doc_class", OracleDbType.Varchar2,
                  ParameterDirection.Output, 20, cmd);

               DBUtilities.CreateAndAddParameter("p_out_docustream_code", OracleDbType.Varchar2,
                  ParameterDirection.Output, 1, cmd);

               DBUtilities.CreateAndAddParameter("p_out_business_area", OracleDbType.Varchar2,
                  ParameterDirection.Output, 25, cmd);

               DBUtilities.CreateAndAddParameter("p_out_work_type", OracleDbType.Varchar2,
                  ParameterDirection.Output, 25, cmd);

               DBUtilities.CreateAndAddParameter("p_out_result", OracleDbType.Varchar2,
                  ParameterDirection.Output, 255, cmd);
               DBUtilities.CreateAndAddParameter("p_out_error_message", OracleDbType.Varchar2,
                  ParameterDirection.Output, 4000, cmd);

               //execute the procedure
               objConn.Open();               
               cmd.ExecuteNonQuery();
               objConn.Close();
               //grab the values               
               if (cmd.Parameters["p_out_result"].Value.ToString().ToUpper() == "SUCCESSFUL")
               {
                  _taskValueProvider.Set("docType", Convert.ToString(cmd.Parameters
                     ["p_out_doc_type"].Value));
                  _taskValueProvider.Set("docClassName", Convert.ToString(cmd.Parameters
                     ["p_out_doc_class"].Value));
                  _taskValueProvider.Set("routeCodeOut", Convert.ToString(cmd.Parameters
                     ["p_out_route"].Value));
                  _taskValueProvider.Set("docustream", Convert.ToString(cmd.Parameters
                     ["p_out_docustream_code"].Value));
                  _taskValueProvider.Set("businessArea", Convert.ToString(cmd.Parameters
                     ["p_out_business_area"].Value));
                  _taskValueProvider.Set("workType", Convert.ToString(cmd.Parameters
                     ["p_out_work_type"].Value));


                  _taskValueProvider.Set("QMResult", "0");
                  return 0;
               }
               else if (cmd.Parameters["p_out_result"].Value.ToString().ToUpper() != "NODATA")
               {
                  throw new Exception("-266007829; Procedure Error: " +
                     cmd.Parameters["p_out_result"].Value.ToString() + "; Oracle Error: " +
                     cmd.Parameters["p_out_error_message"].Value.ToString());
               }
               return 0;
            }
         }
         catch (OracleException exOra)
         {


            _taskValueProvider.Set("QMResult", IA_ERR_UNKNOWN.ToString());
            _taskValueProvider.Set("QMErrorDesc", exOra.Message.ToString());
            _taskValueProvider.Set("QMErrorNo", exOra.ErrorCode.ToString());

            return -1;
         }
         catch (Exception ex)
         {

            _taskValueProvider.Set("QMResult", IA_ERR_UNKNOWN.ToString());
            _taskValueProvider.Set("QMErrorDesc", ex.Message);
            _taskValueProvider.Set("QMErrorNo", "-266047710");
          
            return -1;
         }

      }



      public static void LogMessage(IHelper helper, String message, LogType severity)
      {

         helper.LogMessage(message, severity);

      }
   }
}
