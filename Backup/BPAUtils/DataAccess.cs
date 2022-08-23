using System;
using System.Globalization;
using System.Text;
using System.Data;
using System.Data.OracleClient;
using System.Collections.Generic;
using System.Xml;
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
   public class DataAccess
   {



      #region Procedure Names
      private const string CREATE_BATCH = "BPA_APPS.PKG_BATCH.CREATE_BATCH";
      private const string CREATE_BATCH_ITEM = "BPA_APPS.PKG_BATCH.CREATE_BATCH_ITEM";
      private const string CREATE_BATCH_ERROR = "BPA_APPS.PKG_BATCH.CREATE_BATCH_ERROR";
      private const string CREATE_BATCH_DELETE = "BPA_APPS.PKG_BATCH.CREATE_BATCH_DELETE";
      private const string CREATE_DOCUSTREAM_REQUEST = "BPA_APPS.CREATE_DOCUSTREAM_REQUEST";
      private const string CREATE_FRONT_OFFICE_REQUEST = "BPA_APPS.CREATE_FRONT_OFFICE_REQUEST";
      private const string CREATE_IA_STATISTICS = "BPA_APPS.CREATE_IA_STATISTICS";
      private const string CREATE_AWD_REQUEST = "BPA_APPS.PKG_AWD_REQUEST.CREATE_REQUEST";
      private const string UPDATE_AWD_REQUEST = "BPA_APPS.PKG_AWD_REQUEST.UPDATE_REQUEST";
      private const string CREATE_AWD_LOB = "BPA_APPS.PKG_AWD_REQUEST.CREATE_LOB";
      #endregion
      #region Variables
      private CNO.BPA.Framework.Cryptography crypto = new CNO.BPA.Framework.Cryptography(); 
      private OracleConnection _connection = null;
      private string _connectionString = null;
      private OracleTransaction _transaction = null;
      private IValueProvider _taskValueProvider = null;


      private string _AppName;
      private string _DSN = "";
      private string _DBUser = "";
      private string _DBPass = "";
      #endregion

      public DataAccess(IValueProvider TaskValueProvider, string DSN, string User, string Pass, string appName)
      {

         _taskValueProvider = TaskValueProvider;
         //next grab a copy of each of the db values
         _DSN = DSN;
         _DBUser = crypto.Decrypt(User);
         _DBPass = crypto.Decrypt(Pass);

         _AppName = appName;
         //check to see that we have values for the db info
         if (_DSN.Length != 0 & _DBUser.Length != 0 &
             _DBPass.Length != 0 )
         {
            //build the connection string
            _connectionString = "Data Source=" + _DSN + ";Persist Security Info=True;User ID="
               + _DBUser + ";Password=" + _DBPass + "";
         }
         else
         {
            throw new ArgumentNullException("-266088525; Database information could "
               + "not be found in the setup for this instance step.");
         }
      }
      /// <summary>
      /// Connects and logs in to the database, and begins a transaction.
      /// </summary>
      /// 
      public DataAccess(string DSN, string User, string Pass, string appName)
      {


         //next grab a copy of each of the db values
         _DSN = DSN;
         _DBUser = crypto.Decrypt(User);
         _DBPass = crypto.Decrypt(Pass);

         _AppName = appName;
         //check to see that we have values for the db info
         if (_DSN.Length != 0 & _DBUser.Length != 0 &
             _DBPass.Length != 0)
         {
            //build the connection string
            _connectionString = "Data Source=" + _DSN + ";Persist Security Info=True;User ID="
               + _DBUser + ";Password=" + _DBPass + "";
         }
         else
         {
            throw new ArgumentNullException("-266088525; Database information could "
               + "not be found in the setup for this instance step.");
         }
      }
      /// <summary>
      /// Connects and logs in to the database, and begins a transaction.
      /// </summary>
      public void Connect()
      {
         _connection = new OracleConnection();
         _connection.ConnectionString = _connectionString;
         try
         {
            _connection.Open();
            _transaction = _connection.BeginTransaction();
         }
         catch (Exception ex)
         {
            throw new Exception("An error occurred while connecting to the database. " + ex.Message , ex);
         }
      }
      /// <summary>
      /// Commits the current transaction and disconnects from the database.
      /// </summary>
      public void Disconnect()
      {
         try
         {
            if (null != _connection)
            {
               _transaction.Commit();
               _connection.Close();
               _connection.Dispose();
               _transaction.Dispose();
               _connection = null;
               _transaction = null;
            }
         }
         catch { } // ignore an error here
      }
      /// <summary>
      /// Commits all of the data changes to the database.
      /// </summary>
      internal void Commit()
      {
         _transaction.Commit();
      }
      /// <summary>
      /// Cancels the transaction and voids any changes to the database.
      /// </summary>
      public void Cancel()
      {
         try
         {
            _transaction.Rollback();
            _connection.Close();
            _connection.Dispose();
            _transaction.Dispose();
            _connection = null;
            _transaction = null;
         }
         catch { }
      }
      /// <summary>
      /// Generates the command object and associates it with the current transaction object
      /// </summary>
      /// <param name="commandText"></param>
      /// <param name="commandType"></param>
      /// <returns></returns>
      private OracleCommand GenerateCommand(string commandText, System.Data.CommandType commandType)
      {
         OracleCommand cmd = new OracleCommand(commandText, _connection);
         cmd.Transaction = _transaction;
         cmd.CommandType = commandType;
         return cmd;
      }
      /// <summary>
      /// creates the command, add parameters and executes an insert into the batch table
      /// </summary>
      /// <param name="node"></param>
      /// <param name="ActionIndicator"></param>
      /// <returns></returns>
      public void createBatchProcedure( INodeValueProvider nodeValueProvider,INode node, string ActionIndicator, Dictionary<string, string> Fields)
      {
            try
            {
               DateTime outDate;
               string parameterValue = "";
               using (OracleCommand cmd = GenerateCommand(CREATE_BATCH,
                   CommandType.StoredProcedure))
               {
                  DBUtilities.CreateAndAddParameter("p_in_action_indicator",
                     ActionIndicator, OracleType.VarChar, ParameterDirection.Input, cmd);
                  DBUtilities.CreateAndAddParameter("p_in_app_name", _AppName,
                     OracleType.VarChar, ParameterDirection.Input, cmd);
                  DBUtilities.CreateAndAddParameter("p_in_machine_name",
                     System.Environment.MachineName.ToString().ToUpper(),
                     OracleType.VarChar, ParameterDirection.Input, cmd);
                  DBUtilities.CreateAndAddParameter("p_in_user_id",
                     System.Environment.UserName.ToString().ToUpper(),
                     OracleType.VarChar, ParameterDirection.Input, cmd);
                  foreach (KeyValuePair<string, string> fm in Fields)
                  {
                     if (fm.Value.Length > 0)
                     {
                        if (fm.Value.Contains("$instance"))
                        {
                           //create each parameter and add them to the command
                           parameterValue = nodeValueProvider.Get("$node=" + node.Id.ToString() + "/" + fm.Value, "");
                           if (parameterValue.Trim().Length != 0)
                           {
                              if (DateTime.TryParse(parameterValue, out outDate))
                              {
                                 parameterValue = outDate.ToString("yyyy/MM/dd HH:mm:ss");
                              }
                              DBUtilities.CreateAndAddParameter(fm.Key,
                                 parameterValue.ToUpper(), OracleType.VarChar,
                                 ParameterDirection.Input, cmd);
                           }
                        }
                        else
                        {
                           DBUtilities.CreateAndAddParameter(fm.Key,
                              fm.Value.ToUpper(), OracleType.VarChar,
                              ParameterDirection.Input, cmd);
                        }
                     }
                  }
                  DBUtilities.CreateAndAddParameter("p_out_result", OracleType.VarChar,
                     ParameterDirection.Output, 255, cmd);
                  DBUtilities.CreateAndAddParameter("p_out_error_message", OracleType.VarChar,
                     ParameterDirection.Output, 4000, cmd);

                  cmd.ExecuteNonQuery();

                  if (cmd.Parameters["p_out_result"].Value.ToString().ToUpper() != "SUCCESSFUL")
                  {
                     throw new Exception("-266088529; Procedure Error: " +
                        cmd.Parameters["p_out_result"].Value.ToString() + "; Oracle Error: " +
                        cmd.Parameters["p_out_error_message"].Value.ToString());
                  }
               }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message.ToString(), ex);
            }

      }
      /// <summary>
      /// creates the command, add parameters and executes an insert into the batch item table
      /// </summary>
      /// <param name="node"></param>
      /// <param name="ActionIndicator"></param>
      /// <returns></returns>
      public void createBatchItemProcedure( INodeValueProvider nodeValueProvider,  INode node, string ActionIndicator, Dictionary<string, string> Fields)
      {
         try
         {
            DateTime outDate;
            string parameterValue = "";
            using (OracleCommand cmd = GenerateCommand(CREATE_BATCH_ITEM,
               CommandType.StoredProcedure))
            {
               DBUtilities.CreateAndAddParameter("p_in_action_indicator",
                  ActionIndicator, OracleType.VarChar, ParameterDirection.Input, cmd);
               DBUtilities.CreateAndAddParameter("p_in_app_name", "IA BPA UTILITIES",
                  OracleType.VarChar, ParameterDirection.Input, cmd);
               DBUtilities.CreateAndAddParameter("p_in_machine_name",
                  System.Environment.MachineName.ToString().ToUpper(),
                  OracleType.VarChar, ParameterDirection.Input, cmd);
               DBUtilities.CreateAndAddParameter("p_in_user_id",
                  System.Environment.UserName.ToString().ToUpper(),
                  OracleType.VarChar, ParameterDirection.Input, cmd);
               foreach (KeyValuePair<string, string> fm in Fields)
               {
                  string Value = fm.Value;
                  if (ActionIndicator == "U" && fm.Key.ToUpper() == "P_IN_ITEM_ID")
                  {
                     //check to see that the item id was configured
                     //and that it has a value
                     if (Value.Length == 0)
                     {
                        //configuration failure
                        Value = "$instance=Standard_MDF/D_BATCH_ITEM_ID";
                     }

                     parameterValue = nodeValueProvider.Get("$node=" + node.Id.ToString() + "/" + Value, "");
                     if (parameterValue.Length == 0 || parameterValue == "0")
                     {
                        //instance configured as update, however, no value exists
                        throw new Exception("A Batch_Item table update could not be " +
                           "performed due to the value of the D_BATCH_ITEM_ID value");
                     }
                  }
                  if (Value.Length > 0)
                  {
                     if (Value.Contains("$instance"))
                     {
                        //create each parameter and add them to the command
                        parameterValue = nodeValueProvider.Get(Value, "");
                        if (parameterValue.Trim().Length != 0)
                        {
                           if (DateTime.TryParse(parameterValue, out outDate))
                           {
                              parameterValue = outDate.ToString("yyyy/MM/dd HH:mm:ss");
                           }
                           DBUtilities.CreateAndAddParameter(fm.Key,
                              parameterValue.ToUpper(), OracleType.VarChar,
                              ParameterDirection.Input, cmd);
                        }
                     }
                     else
                     {
                        DBUtilities.CreateAndAddParameter(fm.Key,
                           Value.ToUpper(), OracleType.VarChar,
                           ParameterDirection.Input, cmd);
                     }
                  }
               }

               DBUtilities.CreateAndAddParameter("p_out_item_id", OracleType.VarChar,
                  ParameterDirection.Output, 25, cmd);
               DBUtilities.CreateAndAddParameter("p_out_result", OracleType.VarChar,
                  ParameterDirection.Output, 255, cmd);
               DBUtilities.CreateAndAddParameter("p_out_error_message", OracleType.VarChar,
                  ParameterDirection.Output, 4000, cmd);

               cmd.ExecuteNonQuery();

               if (cmd.Parameters["p_out_result"].Value.ToString().ToUpper() == "SUCCESSFUL")
               {
                  if (ActionIndicator == "I")
                  {

                     nodeValueProvider.Set("$instance=Standard_MDF/D_BATCH_ITEM_ID",
                        cmd.Parameters["p_out_item_id"].Value.ToString());
                  }
                  //update the status of the batch
                  updateBatchStatus(nodeValueProvider, node);
               }
               else
               {
                  throw new Exception("-266088529; Procedure Error: " +
                     cmd.Parameters["p_out_result"].Value.ToString() + "; Oracle Error: " +
                     cmd.Parameters["p_out_error_message"].Value.ToString());
               }
            }
         }
         catch (Exception ex)
         {
            throw new Exception(ex.Message.ToString(), ex);
         }

      }
      /// <summary>
      /// creates the command, add parameters and executes an insert into the batch table
      /// </summary>
      /// <param name="node"></param>
      /// <param name="ActionIndicator"></param>
      /// <returns></returns>
      internal void updateBatchStatus(INodeValueProvider nodeValueProvider, INode node)
      {
         try
         {            
            string parameterValue = "";
            string batchNoParameter = "";
            using (OracleCommand cmd = GenerateCommand(CREATE_BATCH,
               CommandType.StoredProcedure))
            {
               DBUtilities.CreateAndAddParameter("p_in_action_indicator",
                  "U", OracleType.VarChar, ParameterDirection.Input, cmd);
               DBUtilities.CreateAndAddParameter("p_in_app_name", _AppName,
                  OracleType.VarChar, ParameterDirection.Input, cmd);
               DBUtilities.CreateAndAddParameter("p_in_machine_name",
                  System.Environment.MachineName.ToString().ToUpper(),
                  OracleType.VarChar, ParameterDirection.Input, cmd);
               DBUtilities.CreateAndAddParameter("p_in_user_id",
                  System.Environment.UserName.ToString().ToUpper(),
                  OracleType.VarChar, ParameterDirection.Input, cmd);
               batchNoParameter = nodeValueProvider.Get("$node=" + node.Id.ToString() + "/$instance=Standard_MDF/BATCH_NO", "");
               if (batchNoParameter.Trim().Length != 0)
               {
                  DBUtilities.CreateAndAddParameter("P_IN_BATCH_NO",
                     batchNoParameter.ToUpper(), OracleType.VarChar,
                     ParameterDirection.Input, cmd);
               }
               parameterValue = nodeValueProvider.Get("$node=" + node.Id.ToString() + "/$instance=Standard_MDF/BATCH_STATUS", "");
               if (parameterValue.Trim().Length != 0)
               {
                  DBUtilities.CreateAndAddParameter("P_IN_STATUS",
                     parameterValue.ToUpper(), OracleType.VarChar,
                     ParameterDirection.Input, cmd);
               }
               DBUtilities.CreateAndAddParameter("p_out_result", OracleType.VarChar,
                  ParameterDirection.Output, 255, cmd);
               DBUtilities.CreateAndAddParameter("p_out_error_message", OracleType.VarChar,
                  ParameterDirection.Output, 4000, cmd);

               cmd.ExecuteNonQuery();

               if (cmd.Parameters["p_out_result"].Value.ToString().ToUpper() != "SUCCESSFUL")
               {
                  throw new Exception("-266088529; Procedure Error: " +
                     cmd.Parameters["p_out_result"].Value.ToString() + "; Oracle Error: " +
                     cmd.Parameters["p_out_error_message"].Value.ToString());
               }
            }
         }
         catch (Exception ex)
         {
            throw new Exception(ex.Message.ToString(), ex);
         }

      }      
      /// <summary>
      /// creates the command, add parameters and executes an insert into the ia statistics table
      /// </summary>
      /// <param name="node"></param>
      /// <param name="ActionIndicator"></param>
      /// <returns></returns>
      public void createIAStatisticsProcedure(INodeValueProvider nodeValueProvider, INode node, string ActionIndicator, Dictionary<string, string> Fields)
      {
         try
         {
            DateTime outDate;
            string parameterValue = "";
            using (OracleCommand cmd = GenerateCommand(CREATE_IA_STATISTICS,
               CommandType.StoredProcedure))
            {
               DBUtilities.CreateAndAddParameter("p_in_action_indicator",
                  ActionIndicator, OracleType.VarChar, ParameterDirection.Input, cmd);
               DBUtilities.CreateAndAddParameter("p_in_app_name", _AppName,
                  OracleType.VarChar, ParameterDirection.Input, cmd);
               DBUtilities.CreateAndAddParameter("p_in_machine_name",
                  System.Environment.MachineName.ToString().ToUpper(),
                  OracleType.VarChar, ParameterDirection.Input, cmd);
               DBUtilities.CreateAndAddParameter("p_in_user_id",
                  System.Environment.UserName.ToString().ToUpper(),
                  OracleType.VarChar, ParameterDirection.Input, cmd);
               foreach (KeyValuePair<string, string> fm in Fields)
               {
                  if (fm.Value.Length > 0)
                  {
                     if (fm.Value.Contains("$instance"))
                     {
                        //create each parameter and add them to the command
                        parameterValue = nodeValueProvider.Get("$node=" + node.Id.ToString() + "/" + fm.Value, "");//node.GetValueString(fm.IAValue);
                        if (parameterValue.Trim().Length != 0)
                        {
                           if (DateTime.TryParse(parameterValue, out outDate))
                           {
                              parameterValue = outDate.ToString("yyyy/MM/dd HH:mm:ss");
                           }
                           DBUtilities.CreateAndAddParameter(fm.Key,
                              parameterValue.ToUpper(), OracleType.VarChar,
                              ParameterDirection.Input, cmd);
                        }
                     }
                     else
                     {
                        DBUtilities.CreateAndAddParameter(fm.Key,
                           fm.Value.ToUpper(), OracleType.VarChar,
                           ParameterDirection.Input, cmd);
                     }
                  }
               }
               DBUtilities.CreateAndAddParameter("p_out_ia_statistics_id", OracleType.VarChar,
                  ParameterDirection.Output, 25, cmd);
               DBUtilities.CreateAndAddParameter("p_out_result", OracleType.VarChar,
                  ParameterDirection.Output, 255, cmd);
               DBUtilities.CreateAndAddParameter("p_out_error_message", OracleType.VarChar,
                  ParameterDirection.Output, 4000, cmd);

               cmd.ExecuteNonQuery();

               if (cmd.Parameters["p_out_result"].Value.ToString().ToUpper() == "SUCCESSFUL")
               {
                  if (ActionIndicator == "I")
                  {
                     _taskValueProvider.Set("IAStatsId", cmd.Parameters["p_out_ia_statistics_id"].Value.ToString());

                  }
               }
               else
               {
                  throw new Exception("-266088529; Procedure Error: " +
                     cmd.Parameters["p_out_result"].Value.ToString() + "; Oracle Error: " +
                     cmd.Parameters["p_out_error_message"].Value.ToString());
               }
            }
         }
         catch (Exception ex)
         {
            throw new Exception(ex.Message.ToString(), ex);
         }

      }
      /// <summary>
      /// creates the command, add parameters and executes an insert into the batch error table
      /// </summary>
      /// <param name="node"></param>
      /// <returns></returns>
      public void createBatchError(INodeValueProvider nodeValueProvider, INode node, Dictionary<string, string> Fields)
      {
         try
         {
            DateTime outDate;
            string parameterValue = "";
            using (OracleCommand cmd = GenerateCommand(CREATE_BATCH_ERROR,
               CommandType.StoredProcedure))
            {
               foreach (KeyValuePair<string, string> fm in Fields)
               {
                  if (fm.Value.Length > 0)
                  {
                     if (fm.Value.Contains("$instance"))
                     {
                        //create each parameter and add them to the command
                        parameterValue = nodeValueProvider.Get("$node=" + node.Id.ToString() + "/" + fm.Value, ""); //node.GetValueString(fm.IAValue);
                        if (parameterValue.Trim().Length != 0)
                        {
                           if (DateTime.TryParse(parameterValue, out outDate))
                           {
                              parameterValue = outDate.ToString("yyyy/MM/dd HH:mm:ss");
                           }
                           DBUtilities.CreateAndAddParameter(fm.Key,
                              parameterValue.ToUpper(), OracleType.VarChar,
                              ParameterDirection.Input, cmd);
                        }
                     }
                     else
                     {
                        DBUtilities.CreateAndAddParameter(fm.Key,
                           fm.Value.ToUpper(), OracleType.VarChar,
                           ParameterDirection.Input, cmd);
                     }
                  }
               }
               DBUtilities.CreateAndAddParameter("p_out_batch_err_id", OracleType.VarChar,
                  ParameterDirection.Output, 25, cmd);
               DBUtilities.CreateAndAddParameter("p_out_result", OracleType.VarChar,
                  ParameterDirection.Output, 255, cmd);
               DBUtilities.CreateAndAddParameter("p_out_error_message", OracleType.VarChar,
                  ParameterDirection.Output, 4000, cmd);

               cmd.ExecuteNonQuery();

               if (cmd.Parameters["p_out_result"].Value.ToString().ToUpper() == "SUCCESSFUL")
               {
                  _taskValueProvider.Set("BatchErrorId", cmd.Parameters["p_out_batch_err_id"].Value.ToString());

               }
               else
               {
                  throw new Exception("-266088529; Procedure Error: " +
                     cmd.Parameters["p_out_result"].Value.ToString() + "; Oracle Error: " +
                     cmd.Parameters["p_out_error_message"].Value.ToString());
               }
            }
         }
         catch (Exception ex)
         {
            throw new Exception(ex.Message.ToString(), ex);
         }

      }
      internal void direct2Workflow(INodeValueProvider nodeValueProvider, INode node)
      {
         try
         {
            string IndexType;
            IndexType = nodeValueProvider.Get("$node=" + node.Id.ToString() + "/$instance=Standard_MDF/D_LAUNCH_TYPE",  "");
            writeRecord(nodeValueProvider, node, IndexType);
            //check second launch type for additional workflow needs
            IndexType = nodeValueProvider.Get("$node=" + node.Id.ToString() + "/$instance=Standard_MDF/D_LAUNCH_TYPE_2", "");
            if (IndexType.Length > 0)
            {
               writeRecord(nodeValueProvider, node, IndexType);
            }
            //check third launch type for additional workflow needs
            IndexType = nodeValueProvider.Get("$node=" + node.Id.ToString() + "/$instance=Standard_MDF/D_LAUNCH_TYPE_3", "");
            if (IndexType.Length > 0)
            {
               writeRecord(nodeValueProvider, node, IndexType);
            }
            //check fourth launch type for additional workflow needs
            IndexType = nodeValueProvider.Get("$node=" + node.Id.ToString() + "/$instance=Standard_MDF/D_LAUNCH_TYPE_4", "");
            if (IndexType.Length > 0)
            {
               writeRecord(nodeValueProvider, node, IndexType);
            }

         }
         catch (Exception ex)
         {
            throw new Exception(ex.Message.ToString(), ex);
         }
      }
      internal void writeRecord(INodeValueProvider nodeValueProvider, INode node,string IndexType)
      {
         try
         {
            Int32 imgcount;
            CNO.BPA.DataHandler.CommonParameters CP = new CNO.BPA.DataHandler.CommonParameters();
            CNO.BPA.DataHandler.DataAccess dataaccess = new CNO.BPA.DataHandler.DataAccess();
            CNO.BPA.MVI.Indexer indx = new CNO.BPA.MVI.Indexer();

            CP.IndexType = IndexType;
            CP.AgentNo = nodeValueProvider.Get("$node=" + node.Id.ToString() + "/$instance=Standard_MDF/D_AGENT_NO", "");
            CP.ApplicationNo = nodeValueProvider.Get("$node=" + node.Id.ToString() + "/$instance=Standard_MDF/D_APPLICATION_NO", "");
            CP.AWDSourceType = nodeValueProvider.Get("$node=" + node.Id.ToString() + "/$instance=Standard_MDF/D_AWD_SOURCE_TYPE", "");
            CP.AWDStatus = nodeValueProvider.Get("$node=" + node.Id.ToString() + "/$instance=Standard_MDF/D_AWD_STATUS", "");
            CP.BatchNo = nodeValueProvider.Get("$node=" + node.Id.ToString() + "/$instance=Standard_MDF/BATCH_NO", "");
            CP.BirthDate = nodeValueProvider.Get("$node=" + node.Id.ToString() + "/$instance=Standard_MDF/D_INSURED_BIRTHDATE", "");
            CP.BoxNo = nodeValueProvider.Get("$node=" + node.Id.ToString() + "/$instance=Standard_MDF/BOX_NO", "");
            CP.BusinessArea = nodeValueProvider.Get("$node=" + node.Id.ToString() + "/$instance=Standard_MDF/D_BUSINESS_AREA", "");
            CP.CompanyCode = nodeValueProvider.Get("$node=" + node.Id.ToString() + "/$instance=Standard_MDF/D_COMPANY_CODE", "");
            CP.FDocClassName = nodeValueProvider.Get("$node=" + node.Id.ToString() + "/$instance=Standard_MDF/D_F_DOCCLASSNAME", "");
            CP.DocType = nodeValueProvider.Get("$node=" + node.Id.ToString() + "/$instance=Standard_MDF/D_DOC_TYPE", "");
            CP.CurrentNodeID = node.Id.ToString();
            CP.GroupNo = nodeValueProvider.Get("$node=" + node.Id.ToString() + "/$instance=Standard_MDF/D_GROUP_NO", "");
            CP.FaxAccount = nodeValueProvider.Get("$node=" + node.Id.ToString() + "/$instance=Standard_MDF/D_FAX_ACCOUNT", "");
            CP.FaxID = nodeValueProvider.Get("$node=" + node.Id.ToString() + "/$instance=Standard_MDF/D_FAX_ID", "");
            CP.FaxFrom = nodeValueProvider.Get("$node=" + node.Id.ToString() + "/$instance=Standard_MDF/D_FAX_SENDER", "");
            CP.FaxTo = nodeValueProvider.Get("$node=" + node.Id.ToString() + "/$instance=Standard_MDF/D_FAX_RECIPIENT", "");
            CP.FaxServer = nodeValueProvider.Get("$node=" + node.Id.ToString() + "/$instance=Standard_MDF/D_FAX_SERVER", "");
            Int32.TryParse(nodeValueProvider.Get("$node=" + node.Id.ToString() + "/$instance=Standard_MDF/D_IMG_COUNT", ""), out imgcount);
            CP.ImgCount = imgcount;
            CP.LineOfBusiness = nodeValueProvider.Get("$node=" + node.Id.ToString() + "/$instance=Standard_MDF/D_LOB", "");
            CP.InsuredName = nodeValueProvider.Get("$node=" + node.Id.ToString() + "/$instance=Standard_MDF/D_INSURED_NAME", "");
            CP.Phone = nodeValueProvider.Get("$node=" + node.Id.ToString() + "/$instance=Standard_MDF/D_PHONE", "");
            CP.PolicyNo = nodeValueProvider.Get("$node=" + node.Id.ToString() + "/$instance=Standard_MDF/D_POLICY_NO", "");
            CP.ReceivedDate = nodeValueProvider.Get("$node=" + node.Id.ToString() + "/$instance=Standard_MDF/RECEIVED_DATE", "");
            CP.ReceivedDateCRD = nodeValueProvider.Get("$node=" + node.Id.ToString() + "/$instance=Standard_MDF/CRD_RECEIVED_DATE", "");
            CP.RouteCode = nodeValueProvider.Get("$node=" + node.Id.ToString() + "/$instance=Standard_MDF/D_ROUTE_CODE", "");
            CP.ScannerID = nodeValueProvider.Get("$node=" + node.Id.ToString() + "/$instance=Standard_MDF/SCANNER_ID", "");
            CP.ScanDate = nodeValueProvider.Get("$node=" + node.Id.ToString() + "/$instance=Standard_MDF/SCAN_START", "");
            CP.SiteID = nodeValueProvider.Get("$node=" + node.Id.ToString() + "/$instance=Standard_MDF/D_SITE_ID", "");
            CP.SSN = nodeValueProvider.Get("$node=" + node.Id.ToString() + "/$instance=Standard_MDF/D_SSN", "");
            CP.State = nodeValueProvider.Get("$node=" + node.Id.ToString() + "/$instance=Standard_MDF/D_STATE", "");
            CP.Status = nodeValueProvider.Get("$node=" + node.Id.ToString() + "/$instance=Standard_MDF/BATCH_STATUS", "");
            CP.SystemID = nodeValueProvider.Get("$node=" + node.Id.ToString() + "/$instance=Standard_MDF/D_SYSTEM_ID", "");
            CP.TypeOfBill = nodeValueProvider.Get("$node=" + node.Id.ToString() + "/$instance=Standard_MDF/D_TYPE_OF_BILL", "");
            CP.WorkType = nodeValueProvider.Get("$node=" + node.Id.ToString() + "/$instance=Standard_MDF/D_WORK_TYPE", "");
            CP.WorkCategory = nodeValueProvider.Get("$node=" + node.Id.ToString() + "/$instance=Standard_MDF/D_WORK_CATEGORY", "");
            CP.ZipCode = nodeValueProvider.Get("$node=" + node.Id.ToString() + "/$instance=Standard_MDF/D_ZIP", "");
            CP.CountAttachment = nodeValueProvider.Get("$node=" + node.Id.ToString() + "/$instance=Standard_MDF/D_ATTACHMENT_COUNT", "");
            CP.CountEOMB = nodeValueProvider.Get("$node=" + node.Id.ToString() + "/$instance=Standard_MDF/D_EOMB_COUNT", "");
            CP.CountHCFA = nodeValueProvider.Get("$node=" + node.Id.ToString() + "/$instance=Standard_MDF/D_HCFA_COUNT", "");
            CP.CountPHEOMB = nodeValueProvider.Get("$node=" + node.Id.ToString() + "/$instance=Standard_MDF/D_PHEOMB_COUNT", "");
            CP.CountUB92 = nodeValueProvider.Get("$node=" + node.Id.ToString() + "/$instance=Standard_MDF/D_UB92_COUNT", "");
            dataaccess.getMVIFieldData(ref CP);
            processCustParmsD(nodeValueProvider, node, ref CP);
            indx.direct2Workflow(ref CP);
            nodeValueProvider.Set("$node=" + node.Id.ToString() + "/$instance=Standard_MDF/D_BATCH_ITEM_ID", CP.BatchItemID.ToString());


         }
         catch (Exception ex)
         {
            throw new Exception(ex.Message.ToString(), ex);
         }
      }
      private void processCustParmsD(INodeValueProvider nodeValueProvider, INode node, ref CNO.BPA.DataHandler.CommonParameters CP)
      {
         try
         {

            foreach (DataRow dr in CP._MVIFieldData.DD_MVI_FIELD_DEFINITION.Rows)
            {

               string dName = dr["IA_MDF_NAME"].ToString();

               if (dName.Length != 0)
               //update the localValue in the dataset
               {
                  string mdfValue = nodeValueProvider.Get("$node=" + node.Id.ToString() + "/$instance=Standard_MDF/" + dName, "");
                  if (mdfValue.Trim().Length != 0)
                  {
                     dr["LOCAL_VALUE"] = mdfValue.Trim();
                     CP[dr["FIELD_NAME"].ToString()] = mdfValue.Trim();
                  }
               }

            }


         }
         catch (Exception ex)
         {
            throw new Exception("DataAccess.processCustParmsD: " + ex.Message);
         }
      }








   }

}
