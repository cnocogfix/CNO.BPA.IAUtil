// Copyright © 2003–2009 EMC Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Data;

//----------------------------------------------------------------
// InputAccel namespaces - QuickModule, Processing Helper,
// Script Helper, Scripting interface, and Workflow Client.
//----------------------------------------------------------------
using Emc.InputAccel.QuickModule;
using Emc.InputAccel.QuickModule.Helpers.Processing;
using Emc.InputAccel.QuickModule.Plugins.Processing;
using Emc.InputAccel.Workflow.Client;
//----------------------------------------------------------------
// Custom script event namespace defined in the ExportCS.Scripting project.
//----------------------------------------------------------------
//dt using Sample.ExportCS.Scripting;

namespace CNO.BPA.IAUtil
{
   //----------------------------------------------------------------
   // The IModule base interface is the main interface for QuickModule modules.
   // The IModuleInformation base interface is used to provide information when
   // installing the module as a service.
   //----------------------------------------------------------------
   public class ProcessModule
   {
      //----------------------------------------------------------------
      // The following private members reference various objects during the
      // lifetime of the module.
      //----------------------------------------------------------------
      private IHelper quickModulehelper = null;

      //Error Cosntants
      public const int IA_SUCCESS = 0;
      public const int IA_ERR_UNKNOWN = -4523;
      public const int IA_ERR_NOFUNC = -4518;
      public const int IA_ERR_CANCEL = -4526;
      public const int IA_ERR_NORETRY = -6112;
      public const int IA_ERR_RETRYSOME = -6113;
      public const int IA_ERR_RETRY = -6114;
      public const int IA_ERR_ACCESS = -4505;


      private string _AppName;

      //----------------------------------------------------------------
      // The following private members keep track of the processing state of 
      // the current task.
      //----------------------------------------------------------------

      private ITask currentTask = null;                       // The task currently being processed.
      private string _DSN = string.Empty;
      private string _DBUser = string.Empty;
      private string _DBPass = string.Empty;
      private string _InstanceType = string.Empty;
      private Dictionary<string, string> _Fields = null;

      private DataAccess da = null;




      IValueProvider _stepValueProvider = null;
      IValueProvider _taskValueProvider = null;






      //----------------------------------------------------------------
      // Miscellaneous private members. 
      //----------------------------------------------------------------
      private Random random = null;           // Used to generate a psuedo session ID for our fake third-party system.

      public ProcessModule(IHelper helper, string appName)
      {
         this.quickModulehelper = helper;
         _AppName = appName;
         this.random = new Random(System.DateTime.Now.Millisecond);
      }






      #region IModuleOptions (Setup) Event Handlers

      //----------------------------------------------------------------
      // BeginSetup is fired when the module is started in setup mode to setup a Step in a batch
      // or process.
      // 
      // Handle this event to add your own setup panels as shown below. Setup panels are 
      // automatically shown in setup mode.
      // 
      // To create a new setup panel:
      // 
      // 1) Add a new "User Control" item to the project.
      // 2) Add "using Emc.InputAccel.QuickModule;" and "using Emc.InputAccel.Workflow.Client;" 
      //    to the new user control cs file.
      // 3) Add IPanel as a base interface to the user control class as in
      //    "public partial class MySetupPanel : UserControl, IPanel".
      // 4) Modify the default constructor to take two parameters - IHelper and IStep - as in
      //    "public MySetupPanel(IHelper helper, IStep step)".
      // 5) Implement and add code where needed to the IPanel members.
      // 6) Create and add an instance of the panel to the SetupPanels List in BeginSetup as shown below.
      //----------------------------------------------------------------
      public void Process_BeginSetup(object sender, BeginSetupEventArgs e)
      {

         IPanel panel = new SetupPanel(this.quickModulehelper, e.Step);
         e.SetupPanels.Add(panel);
      }


      #endregion

      #region Processing Helper Event Handlers

      //----------------------------------------------------------------
      // BeginNode is fired for a node before its children (if applicable) are processed with
      // BeginNode and FinishNode.
      //----------------------------------------------------------------
      public void Process_BeginNode(object sender, BeginNodeEventsArgs e)
      {
         INodeValueProvider nodeValueProvider = e.Node.Value(this.currentTask.Step);
           // _InstanceType = "IntakeAudit";

         switch (_InstanceType )
         {
            #region RouteCode
            case "RouteCodeDetect":
               if (e.Node.Level.Number == 1)
               {
                  RouteCode routeCode = new RouteCode(_taskValueProvider,quickModulehelper,_DSN,_DBUser,_DBPass );
                  routeCode.launchRCSearch();
               }
               break;
            #endregion
            #region Direct2Workflow
            case "Direct2Workflow":
               if (e.Node.Level.Number == 1)
               {
                  try
                  {
                     da.direct2Workflow(nodeValueProvider,e.Node);
                  }
                  catch (Exception ex6)
                  {
                     da.Cancel();
                     handleError(ex6, "-266088521");
                     return;
                  }
                  _taskValueProvider.Set("QMResult", IA_SUCCESS.ToString());
               }
               break;
            #endregion
            #region IntakeAudit
            case "IntakeAudit":
               if (e.Node.Level.Number == 7)
               {
                  try
                  {
                     da.createBatchProcedure(nodeValueProvider,e.Node, "I",_Fields );
                  }
                  catch (Exception ex1)
                  {
                     da.Cancel();
                     handleError(ex1, "-266088521");
                     return;
                  }
                  _taskValueProvider.Set("QMResult", IA_SUCCESS.ToString());
               }
               break;
            #endregion
            #region FAXAudit
            case "FAXAudit":
            //this case can drop through, since the calls are all the same           
            #endregion
            #region ValidationAudit
            case "ValidationAudit":
               if (e.Node.Level.Number == 1)
               {
                  try
                  {
                     string batchItemID = nodeValueProvider.Get("$instance=Standard_MDF/D_BATCH_ITEM_ID", "");
                     if (batchItemID == "0" || batchItemID.Trim().Length == 0)
                     {
                        da.createBatchItemProcedure(nodeValueProvider,e.Node, "I",_Fields );
                     }
                     else
                     {
                        da.createBatchItemProcedure(nodeValueProvider, e.Node, "U", _Fields);
                     }
                  }
                  catch (Exception ex2)
                  {
                     da.Cancel();
                     handleError(ex2, "-266088521");
                     return;
                  }
                  _taskValueProvider.Set("QMResult", IA_SUCCESS.ToString());
               }
               break;
            #endregion
            #region ExportAudit
            case "ExportAudit":
               if (e.Node.Level.Number == 3)
               {
                  try
                  {
                     da.createBatchProcedure(nodeValueProvider,e.Node, "U",_Fields );
                  }
                  catch (Exception ex9)
                  {
                     da.Cancel();
                     handleError(ex9, "-266088521");
                     return;
                  }
                  _taskValueProvider.Set("QMResult", IA_SUCCESS.ToString());
               }
               break;
            #endregion
            #region ClassifyAudit
            case "ClassifyAudit":
            //this case can drop through, since the calls are all the same           
            #endregion
            #region AutoClassAudit
            case "AutoClassAudit":
            //this case can drop through, since the calls are all the same           
            #endregion
            #region AutoValAudit
            case "AutoValAudit":
            //this case can drop through, since the calls are all the same           
            #endregion
            #region ManClassAudit
            case "ManClassAudit":
            //this case can drop through, since the calls are all the same           
            #endregion
            #region ManValAudit
            case "ManValAudit":
            //this case can drop through, since the calls are all the same           
            #endregion
            #region PreManIndexAudit
            case "PreManIndexAudit":
            //this case can drop through, since the calls are all the same           
            #endregion
            #region CompleteAudit
            case "CompleteAudit":
               if (e.Node.Level.Number == 7)
               {
                  try
                  {
                     da.createBatchProcedure(nodeValueProvider,e.Node, "U",_Fields );
                  }
                  catch (Exception ex3)
                  {
                     da.Cancel();
                     handleError(ex3, "-266088521");
                     return;
                  }
                  _taskValueProvider.Set("QMResult", IA_SUCCESS.ToString());
               }
               break;
            #endregion
            #region LogStats
            case "LogStats":
               if (e.Node.Level.Number == 0)
               {
                  try
                  {
                     da.createIAStatisticsProcedure(nodeValueProvider,e.Node, "I",_Fields );
                  }
                  catch (Exception ex4)
                  {
                     da.Cancel();
                     handleError(ex4, "-266088521");
                     return;
                  }
                  _taskValueProvider.Set("QMResult", IA_SUCCESS.ToString());
               }
               break;
            #endregion
            #region DatabaseExport
            case "DatabaseExport":
               if (e.Node.Level.Number == 1)
               {
                  try
                  {
                     string batchItemID = nodeValueProvider.Get("$instance=Standard_MDF/D_BATCH_ITEM_ID", "");
                     if (batchItemID == "0" | batchItemID.Trim().Length == 0)
                     {
                        da.createBatchItemProcedure(nodeValueProvider, e.Node, "I", _Fields);
                     }
                     else
                     {
                        da.createBatchItemProcedure(nodeValueProvider, e.Node, "U", _Fields);
                     }
                  }
                  catch (Exception ex5)
                  {
                     da.Cancel();
                     handleError(ex5, "-266088521");
                     return;
                  }
                  _taskValueProvider.Set("QMResult", IA_SUCCESS.ToString());
               }
               break;
            #endregion

            #region ErrorHandler
            case "ErrorHandler":
               if (e.Node.Level.Number == 7)
               {
                  try
                  {
                     da.createBatchError(nodeValueProvider,e.Node,_Fields );
                  }
                  catch (Exception ex8)
                  {
                     da.Cancel();
                     handleError(ex8, "-266088521");
                     return;
                  }
                  _taskValueProvider.Set("QMResult", IA_SUCCESS.ToString());
               }
               break;
            #endregion
            #region Default
            default:
               Exception ex = new Exception("No Instance Type specified. " + _InstanceType);
               handleError(ex, "-266088550");
               break;
            #endregion
         }





      }



      //----------------------------------------------------------------
      // BeginTask is the first event fired when a task is received. It is fired before BeginNode.
      //----------------------------------------------------------------
      public void Process_BeginTask(object sender, BeginTaskEventsArgs e)
      {

         this.currentTask = e.Task;
         _stepValueProvider = this.currentTask.Step.Value();
         _taskValueProvider = this.currentTask.Value();


         //----------------------------------------------------------------
         // Load the setup settings from the server.
         //----------------------------------------------------------------
         this._DSN = _stepValueProvider.Get(SetupPanel.DSN, String.Empty);
         this._DBUser = _stepValueProvider.Get(SetupPanel.DBUSER, String.Empty);
         this._DBPass = _stepValueProvider.Get(SetupPanel.DBPASS, String.Empty);
         this._Fields = _stepValueProvider.Get<Dictionary<string, string>>(SetupPanel.FIELDMAPS, null);
         this._InstanceType = _stepValueProvider.Get(SetupPanel.INSTANCETYPE , String.Empty);
         //in the event this task fails, pull in the machine and 
         //username to send back

         _stepValueProvider.Set("QMMachineName",System.Environment.MachineName.ToString().ToUpper());
         _stepValueProvider.Set("QMOperatorID", System.Environment.UserName.ToString().ToUpper());

         try
         {
            da = new DataAccess(_taskValueProvider,_DSN,_DBUser ,_DBPass, _AppName);
            try
            {
               da.Connect();
            }
            catch (Exception ex1)
            {
               handleError(ex1, "-266088520");
               return;
            }
         }
         catch (Exception ex2)
         {
            handleError(ex2, "-266088522");
            return;
         }


         _taskValueProvider.Set("QMResult", IA_SUCCESS.ToString());


      }
      private void handleError(Exception ex, string errNo)
      {
         _taskValueProvider.Set("QMResult", IA_ERR_UNKNOWN.ToString());
         _taskValueProvider.Set("QMErrorDesc", ex.Message);
         _taskValueProvider.Set("QMErrorNo", errNo);
         this.quickModulehelper.LogMessage(errNo + " " + ex.Message, LogType.Error);

      }

      //----------------------------------------------------------------
      // FinishTask is fired after the task node and its children (if applicable) have been processed
      // with BeginNode and FinishNode.
      //----------------------------------------------------------------
      public void Process_FinishTask(object sender, FinishTaskEventsArgs e)
      {
         if (e.Reasons.Count >  0)
         {
            da.Cancel();
         }
         else
         {
            da.Disconnect();
         }
      }




      #endregion





   }
}
