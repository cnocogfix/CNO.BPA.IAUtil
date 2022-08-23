// Copyright © 2003–2009 EMC Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Data.OracleClient;
//----------------------------------------------------------------
// InputAccel namespaces - QuickModule and Client.
//----------------------------------------------------------------
using Emc.InputAccel.QuickModule;
using Emc.InputAccel.Workflow.Client;

namespace CNO.BPA.IAUtil
{
   //----------------------------------------------------------------
   // A QuickModule setup panel must implement the IPanel interface as shown below.
   // IPanelTitle provides a panel title of the same text style as other QuickModule panels.
   //----------------------------------------------------------------
   public partial class SetupPanel : UserControl, IPanel, IPanelTitle
   {
      //---------------------------------------------------------------- 
      // IAValue names used to store file name and path properties.
      //----------------------------------------------------------------


      public static readonly string DBUSER = "_DBUser";
      public static readonly string DBPASS = "_DBPass";
      public static readonly string DSN = "_DSN";
      public static readonly string INSTANCETYPE = "_InstanceType";
      public static readonly string FIELDMAPS = "_FieldMaps";


      //----------------------------------------------------------------
      // The following private members reference various objects during the
      // lifetime of this setup panel.
      //----------------------------------------------------------------
      private IStep step = null;
      private IHelper helper = null;
      private IPanelsSheet panelsSheet = null;
      private IValueProvider valueProvider = null;

      private Dictionary<string, string> Fields = null;
      private CNO.BPA.Framework.Cryptography crypto = new CNO.BPA.Framework.Cryptography();
      //----------------------------------------------------------------
      // The default constructor is modified to take two parameters, IHelper helper
      // and IStep step, that are referenced during the lifetime of the panel.
      //----------------------------------------------------------------
      public SetupPanel(IHelper helper, IStep step)
      {
         InitializeComponent();

         this.helper = helper;
         this.step = step;
         this.valueProvider = step.Value();

         this.txtDBPass.TextChanged += new EventHandler(ValueChanged);
         this.txtDBUser.TextChanged += new EventHandler(ValueChanged);
         this.txtDSN.TextChanged += new EventHandler(ValueChanged);
         this.listInstanceTypes.SelectedIndexChanged += new EventHandler(ValueChanged);
         this.listInstanceTypes.SelectedIndexChanged += new EventHandler(this.listInstanceTypes_SelectedIndexChanged);



      }

      #region IPanel Members

      //----------------------------------------------------------------
      // ApplyChanges() is called after the user clicks the OK or Apply button and ValidateContent()
      // has returned TRUE (validated) for all panels. Therefore, data is assumed to be valid and 
      // must be saved in ApplyChanges().
      //----------------------------------------------------------------
      public void ApplyChanges()
      {
         //----------------------------------------------------------------
         // Copy the desired image naming properties over to the server.
         //----------------------------------------------------------------
         this.valueProvider.Set(DSN, this.txtDSN.Text);
         this.valueProvider.Set(DBUSER, crypto.Encrypt(this.txtDBUser.Text));
         this.valueProvider.Set(DBPASS, crypto.Encrypt(this.txtDBPass.Text));
         if (this.listInstanceTypes.SelectedItem != null)
         {
            this.valueProvider.Set(INSTANCETYPE, this.listInstanceTypes.SelectedItem.ToString());
         }

         //check to see if we have a fields collection already
         if (Fields == null)
         {
            Fields = new Dictionary<string, string>();
         }
         //check to see if there are any rows to process
         if (dataGridView1 == null || dataGridView1.Rows.Count == 0)
         {

            Fields.Add("","");
            return;
         }
         //as long as there is more than 1 row
         Fields.Clear();//rebuild dictionary based on what is currently in grid
         foreach (DataGridViewRow row in dataGridView1.Rows)
         {
            string iavalue = "";
            if (row.Cells[3].Value != null)
            {
               iavalue = row.Cells[3].Value.ToString();
            }
            Fields.Add(row.Cells[0].Value.ToString(), iavalue);
         }

         this.valueProvider.Set<Dictionary<string, string>>(FIELDMAPS, Fields);





         
      }

      //----------------------------------------------------------------
      // OnKillActive() is called when the panel is active and
      // 
      // 1) The panel is about to become inactive because the setup window is switching to another
      //    setup panel, or
      // 2) The Cancel button is clicked.
      // 
      // Returns TRUE to leave the panel, otherwise FALSE to remain on the panel.
      //----------------------------------------------------------------
      public bool OnKillActive()
      {
         return true;
      }

      //----------------------------------------------------------------
      // OnLoad() is called one time during intialization. Setup values should be fetched and the
      // user interface populated.
      //----------------------------------------------------------------
      public void OnLoad(IPanelsSheet Sheet)
      {


         this.Fields = this.valueProvider.Get<Dictionary<string, string>>(FIELDMAPS, new Dictionary<string, string>());


         this.txtDSN.Text = this.valueProvider.Get(DSN, String.Empty);

         if (this.valueProvider.Get(DBUSER, String.Empty).Length > 0)
         {
            this.txtDBUser.Text = crypto.Decrypt(this.valueProvider.Get(DBUSER, String.Empty));
         }
         if (this.valueProvider.Get(DBPASS, String.Empty).Length > 0)
         {
            this.txtDBPass.Text = crypto.Decrypt(this.valueProvider.Get(DBPASS, String.Empty));
         }

         loadInstanceTypes();
         if (listInstanceTypes.Items.Contains(this.valueProvider.Get(INSTANCETYPE, String.Empty)))
         {
            int itemLocation = listInstanceTypes.Items.IndexOf(this.valueProvider.Get(INSTANCETYPE, String.Empty));
            listInstanceTypes.SelectedIndex = itemLocation;
         }


         this.panelsSheet = Sheet;
      }
      private void loadInstanceTypes()
      {



         listInstanceTypes.Items.Add("RouteCodeDetect");
         listInstanceTypes.Items.Add("Direct2Workflow");
         listInstanceTypes.Items.Add("IntakeAudit");
         listInstanceTypes.Items.Add("FAXAudit");
         listInstanceTypes.Items.Add("ClassifyAudit");
         listInstanceTypes.Items.Add("AutoClassAudit");
         listInstanceTypes.Items.Add("AutoValAudit");
         listInstanceTypes.Items.Add("PreManIndexAudit");
         listInstanceTypes.Items.Add("ManClassAudit");
         listInstanceTypes.Items.Add("ManValAudit");
         listInstanceTypes.Items.Add("ValidationAudit");
         listInstanceTypes.Items.Add("LogStats");
         listInstanceTypes.Items.Add("ExportAudit");
         listInstanceTypes.Items.Add("DatabaseExport");
         listInstanceTypes.Items.Add("CompleteAudit");
         listInstanceTypes.Items.Add("ErrorHandler");



         if (listInstanceTypes.Items.Count == 1)
         {
            //default to the first item
            listInstanceTypes.SelectedIndex = 0;
         }
      }
      private void listInstanceTypes_SelectedIndexChanged(object sender, EventArgs e)
      {
         // put current Setup IAValues into controls on Setup panel
         switch (listInstanceTypes.SelectedItem.ToString())
         {

            case "RouteCodeDetect":
            case "Direct2Workflow":
               //we need to hide those items not needed
               panelGrid.Visible = false;
               break;
            case "IntakeAudit":
            case "ClassifyAudit":
            case "AutoClassAudit":
            case "AutoValAudit":
            case "ManClassAudit":
            case "ManValAudit":
            case "ExportAudit":
            case "PreManIndexAudit":
            case "CompleteAudit":
               //we need to treat this as though the user is starting over
               InitializeGrid();
               populateGrid(txtDSN.Text, txtDBUser.Text, txtDBPass.Text,
                  "PKG_BATCH", "CREATE_BATCH");
               loadDataMapping();
               break;
            case "LogStats":
               //we need to treat this as though the user is starting over
               InitializeGrid();
               populateGrid(txtDSN.Text, txtDBUser.Text, txtDBPass.Text,
                  "", "CREATE_IA_STATISTICS");
               loadDataMapping();
               break;
            case "ValidationAudit":
            case "FAXAudit":
            case "DatabaseExport":
               //we need to treat this as though the user is starting over
               InitializeGrid();
               populateGrid(txtDSN.Text, txtDBUser.Text, txtDBPass.Text,
                  "PKG_BATCH", "CREATE_BATCH_ITEM");
               loadDataMapping();
               break;
             case "ErrorHandler":
               //we need to treat this as though the user is starting over
               InitializeGrid();
               populateGrid(txtDSN.Text, txtDBUser.Text, txtDBPass.Text,
                  "PKG_BATCH", "CREATE_BATCH_ERROR");
               loadDataMapping();
               break;
            default:
               break;

         }
         //check to see the grid was populated... if not, show the load
         if (panelGrid.Visible)
         {
            btnLoadInstance.Visible = true;
         }
         else
         {
            btnLoadInstance.Visible = false;
         }
      }
      private void InitializeGrid()
      {
         //start by clearing the grid
         dataGridView1.Rows.Clear();
         dataGridView1.Columns.Clear();
         dataGridView1.Dock = DockStyle.Fill;
         //define and create the db parameter column
         DataGridViewTextBoxColumn dbParm = new DataGridViewTextBoxColumn();
         dbParm.Name = "SP_PARM";
         dbParm.HeaderText = "Name";
         dbParm.DefaultCellStyle.BackColor = Color.LightGray;
         dbParm.ReadOnly = true;
         //dbParm.Width = 120;
         dbParm.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
         dataGridView1.Columns.Add(dbParm);
         //define and create the db direction column
         DataGridViewTextBoxColumn dbParmDirection = new DataGridViewTextBoxColumn();
         dbParmDirection.Name = "SP_PARM_DIRECTION";
         dbParmDirection.HeaderText = "In/Out";
         dbParmDirection.DefaultCellStyle.BackColor = Color.LightGray;
         dbParmDirection.ReadOnly = true;
         dbParmDirection.Width = 40;
         dbParmDirection.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
         dataGridView1.Columns.Add(dbParmDirection);
         //define and create the db parameter column
         DataGridViewTextBoxColumn dbParmType = new DataGridViewTextBoxColumn();
         dbParmType.Name = "SP_PARM_TYPE";
         dbParmType.HeaderText = "Type";
         dbParmType.DefaultCellStyle.BackColor = Color.LightGray;
         dbParmType.ReadOnly = true;
         dbParmType.Width = 66;
         dataGridView1.Columns.Add(dbParmType);
         //define and create the ia value column
         DataGridViewTextBoxColumn iaValue = new DataGridViewTextBoxColumn();
         iaValue.HeaderText = "IA Value";
         iaValue.Width = 120;
         dataGridView1.Columns.Add(iaValue);
         //define and create the ia button column
         DataGridViewButtonColumn iaValueButton = new DataGridViewButtonColumn();
         DataGridViewButtonCell buttonCell = new DataGridViewButtonCell();
         iaValueButton.HeaderText = "";
         iaValueButton.Width = 20;
         buttonCell.Style.BackColor = Color.LightGray;
         buttonCell.Style.ForeColor = Color.Black;
         buttonCell.ToolTipText = "Launch IA Value Builder";
         iaValueButton.Text = "...";
         iaValueButton.CellTemplate = buttonCell;
         iaValueButton.UseColumnTextForButtonValue = true;
         iaValueButton.Name = "iaValueBuilder";
         dataGridView1.Columns.Add(iaValueButton);

         panelGrid.Visible = true;
      }
      private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
      {
         // Ignore clicks that are not on button cells. 
         if (e.RowIndex < 0 || e.ColumnIndex !=
             dataGridView1.Columns["iaValueBuilder"].Index)
         {
            return;
         }
         string iaValue = "";

         iaValue = GetIAValue();

         if (iaValue != null)
         {
            if (iaValue.Length > 0)
            {
               iaValue = "$instance=" + iaValue.Replace(".", "/").Replace("@", "").Replace("(", "").Replace(")", "");
               dataGridView1.CurrentRow.Cells[3].Value = iaValue;
               SetModified();
            }
         }

         

      }
      private void loadDataMapping()
      {
         if (Fields != null)
         {
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
               foreach (KeyValuePair<string, string> fm in Fields)

               {
                  if (fm.Key == row.Cells[0].Value.ToString())
                  {
                     row.Cells[3].Value = fm.Value;
                  }
               }
            }
            //once all previous data is loaded... clear out the list
            Fields.Clear();
         }
      }
      private void populateGrid(string DSN, string user, string password,
                               string packageName, string objectName)
      {
         try
         {
            if (DSN.Length == 0 & user.Length == 0 &
               password.Length == 0)
            {
               return;
            }
            string selectParameters = "";
            //build the connection string
            string BPAConnection = "Data Source=" + DSN + ";Persist Security Info=True;User ID="
               + user + ";Password=" + password + ";";
            //build the selection string
            if (packageName.Trim().Length == 0)
            {
               selectParameters = "SELECT argument_name, in_out, data_type "
                  + "FROM all_arguments WHERE owner = 'BPA_APPS' AND "
                  + "object_name = '" + objectName + "' AND in_out = 'IN' order by position";
            }
            else
            {
               selectParameters = "SELECT argument_name, in_out, data_type "
                  + "FROM all_arguments WHERE owner = 'BPA_APPS' AND package_name = '"
                  + packageName + "' AND object_name = '" + objectName + "' AND in_out = 'IN' order by position";
            }
            //setup the database object            
            OracleConnection objConn = new OracleConnection(BPAConnection);
            using (objConn)
            {
               OracleCommand objCommand = new OracleCommand();
               objCommand.CommandType = CommandType.Text;
               objCommand.Connection = objConn;
               objCommand.CommandText = selectParameters;
               objConn.Open();
               using (OracleDataReader objReader = objCommand.ExecuteReader())
               {
                  // Always call Read before accessing data.
                  while (objReader.Read())
                  {

                     DataGridViewRow row = new DataGridViewRow();
                     string[] dbValues = new string[3];
                     objReader.GetValues(dbValues);
                     switch (dbValues[0].ToString().ToLower())
                     {
                        case "p_in_action_indicator":
                        case "p_in_app_name":
                        case "p_in_machine_name":
                        case "p_in_user_id":
                        case "p_in_kicker_object_id":
                        case "p_in_record_type":
                           //do not display
                           break;
                        default:
                           dataGridView1.Rows.Add(dbValues);
                           break;
                     }
                  }
               }
               objConn.Close();
            }
         }
         catch (Exception ex)
         {
            MessageBox.Show(ex.Message);
         }

      }

      //----------------------------------------------------------------
      // OnSetActive() is called when the panel has become the active panel.
      //----------------------------------------------------------------
      public void OnSetActive()
      {
      }

      //----------------------------------------------------------------
      // OnUnload() is called after ApplyChanges() is called (triggered by the user clicking the OK
      // button), or after OnKillActive() is called (triggered by the user clicking the Cancel button).
      //----------------------------------------------------------------
      public void OnUnload()
      {
      }

      //----------------------------------------------------------------
      // The PanelIcon (optional, can be null) is displayed on the navigation option on the setup window.
      //----------------------------------------------------------------
      public Image PanelIcon
      {
         get { return null; }
      }

      //----------------------------------------------------------------
      // The PanelName is displayed on the navigation option on the setup window.
      // 
      // TODO: Set the panel name to a word or phrase that describes the feature(s) the user will set up.
      // Add an ampersand sign '&' before a letter in the panel name to create a keyboard shortcut. Make
      // sure it does not conflict with other shortcut keys on your panel or other QuickModule panels you
      // are using.
      //----------------------------------------------------------------
      public string PanelName
      {
         get { return Resource.SetupPanelName; }
      }

      //----------------------------------------------------------------
      // The Subpanel (optional, can be null) is displayed on the left-hand side of the setup window under
      // the navigation options.
      //----------------------------------------------------------------
      public Control Subpanel
      {
         get { return null; }
      }

      //----------------------------------------------------------------
      // ValidateContent() is called to verify if the data on the panel is valid. Returns TRUE if the data is
      // valid, or FALSE if the data is invalid.
      // 
      // ValidateContent() should not save or modify data, however it is appropriate to use IHelper.PopupError
      // to notify the user of invalid or missing data.
      //----------------------------------------------------------------
      public bool ValidateContent()
      {
         bool valid = true;

         //----------------------------------------------------------------
         // The only required settings are the name of the folder and the base name of the images.
         // Display an error popup next to the UI element if the values are not present.
         //----------------------------------------------------------------
         //if (String.IsNullOrEmpty(this.fileName.Text))
         //{
         //    valid = false;
         //    this.helper.PopupError(Resource.ValidationFailed, Resource.ValidationEmpty, this.fileName);
         //}

         //if (String.IsNullOrEmpty(this.filePath.Text))
         //{
         //    valid = false;
         //    this.helper.PopupError(Resource.ValidationFailed, Resource.ValidationEmpty, this.filePath);
         //}

         return valid;
      }

      #endregion

      #region IPanelTitle Members

      //----------------------------------------------------------------
      // The PanelTitle property is displayed above the panel. The text is formatted in the same style
      // as other QuickModule panels such as Information, Error, and Scripting.
      // 
      // TODO: Set the panel title to a word or phrase that describes the feature(s) of the panel. In most
      // cases this will be the same as IPanel.PanelName (without the ampersand sign).
      //----------------------------------------------------------------
      public string PanelTitle
      {
         get { return Resource.SetupPanelTitle; }
      }

      #endregion


      //----------------------------------------------------------------
      // Helper function to display a dialog to allow the user to pick an IA Value.
      // Returns a value name in the format STEP.VALUE or an empty string if the user 
      // canceled the dialog.
      //----------------------------------------------------------------
      private string GetIAValue()
      {
         IVarListDialog valueDialog = this.helper.QuickModuleDialog.Get(typeof(IVarListDialog)) as IVarListDialog;

         if (valueDialog != null)
         {
            valueDialog.SetLevelName(3, "Envelope");
            valueDialog.BatchProcessId = this.step.BatchProcess;
            valueDialog.ShowOnlyValues(VarListDialogValueTypes.Value);
            if (valueDialog.ShowVarListDialog(step) == DialogResult.OK)
            {
               return valueDialog.SelectedValue;
            }
            else
            {
               return String.Empty;
            }
         }
         else
         {
            throw new NullReferenceException();
         }
      }



      void ValueChanged(object sender, EventArgs e)
      {
         SetModified();
      }



      //----------------------------------------------------------------
      // Helper function to set the modified flag on the IPanelsSheet object. When the modified flag
      // is set the Apply button on the user interface becomes enabled.
      //----------------------------------------------------------------
      void SetModified()
      {
         if (this.panelsSheet != null)
         {
            this.panelsSheet.SetModified();
         }
      }

      private void btnLoadInstance_Click(object sender, EventArgs e)
      {

        
         listInstanceTypes_SelectedIndexChanged(sender, e);


      }


   }
}
