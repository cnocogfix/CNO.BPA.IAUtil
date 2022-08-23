namespace CNO.BPA.IAUtil
{
    partial class SetupPanel
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
           System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SetupPanel));
           this.setupDB = new System.Windows.Forms.GroupBox();
           this.DSNLbl = new System.Windows.Forms.Label();
           this.DBPassLbl = new System.Windows.Forms.Label();
           this.DBUserLbl = new System.Windows.Forms.Label();
           this.txtDSN = new System.Windows.Forms.TextBox();
           this.txtDBPass = new System.Windows.Forms.TextBox();
           this.txtDBUser = new System.Windows.Forms.TextBox();
           this.panelGrid = new System.Windows.Forms.Panel();
           this.dataGridView1 = new System.Windows.Forms.DataGridView();
           this.listInstanceTypes = new System.Windows.Forms.ComboBox();
           this.uxInstanceTypeGroup = new System.Windows.Forms.GroupBox();
           this.btnLoadInstance = new System.Windows.Forms.Button();
           this.setupDB.SuspendLayout();
           this.panelGrid.SuspendLayout();
           ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
           this.uxInstanceTypeGroup.SuspendLayout();
           this.SuspendLayout();
           // 
           // setupDB
           // 
           resources.ApplyResources(this.setupDB, "setupDB");
           this.setupDB.Controls.Add(this.DSNLbl);
           this.setupDB.Controls.Add(this.DBPassLbl);
           this.setupDB.Controls.Add(this.DBUserLbl);
           this.setupDB.Controls.Add(this.txtDSN);
           this.setupDB.Controls.Add(this.txtDBPass);
           this.setupDB.Controls.Add(this.txtDBUser);
           this.setupDB.Controls.Add(this.panelGrid);
           this.setupDB.Name = "setupDB";
           this.setupDB.TabStop = false;
           // 
           // DSNLbl
           // 
           resources.ApplyResources(this.DSNLbl, "DSNLbl");
           this.DSNLbl.Name = "DSNLbl";
           // 
           // DBPassLbl
           // 
           resources.ApplyResources(this.DBPassLbl, "DBPassLbl");
           this.DBPassLbl.Name = "DBPassLbl";
           // 
           // DBUserLbl
           // 
           resources.ApplyResources(this.DBUserLbl, "DBUserLbl");
           this.DBUserLbl.Name = "DBUserLbl";
           // 
           // txtDSN
           // 
           this.txtDSN.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
           resources.ApplyResources(this.txtDSN, "txtDSN");
           this.txtDSN.Name = "txtDSN";
           // 
           // txtDBPass
           // 
           resources.ApplyResources(this.txtDBPass, "txtDBPass");
           this.txtDBPass.Name = "txtDBPass";
           this.txtDBPass.UseSystemPasswordChar = true;
           // 
           // txtDBUser
           // 
           resources.ApplyResources(this.txtDBUser, "txtDBUser");
           this.txtDBUser.Name = "txtDBUser";
           // 
           // panelGrid
           // 
           resources.ApplyResources(this.panelGrid, "panelGrid");
           this.panelGrid.Controls.Add(this.dataGridView1);
           this.panelGrid.Name = "panelGrid";
           // 
           // dataGridView1
           // 
           this.dataGridView1.AllowUserToAddRows = false;
           this.dataGridView1.AllowUserToDeleteRows = false;
           this.dataGridView1.AllowUserToOrderColumns = true;
           this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
           resources.ApplyResources(this.dataGridView1, "dataGridView1");
           this.dataGridView1.Name = "dataGridView1";
           this.dataGridView1.RowHeadersVisible = false;
           this.dataGridView1.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView1_CellClick);
           // 
           // listInstanceTypes
           // 
           this.listInstanceTypes.FormattingEnabled = true;
           resources.ApplyResources(this.listInstanceTypes, "listInstanceTypes");
           this.listInstanceTypes.Name = "listInstanceTypes";
           // 
           // uxInstanceTypeGroup
           // 
           resources.ApplyResources(this.uxInstanceTypeGroup, "uxInstanceTypeGroup");
           this.uxInstanceTypeGroup.Controls.Add(this.btnLoadInstance);
           this.uxInstanceTypeGroup.Controls.Add(this.listInstanceTypes);
           this.uxInstanceTypeGroup.Controls.Add(this.setupDB);
           this.uxInstanceTypeGroup.Name = "uxInstanceTypeGroup";
           this.uxInstanceTypeGroup.TabStop = false;
           // 
           // btnLoadInstance
           // 
           resources.ApplyResources(this.btnLoadInstance, "btnLoadInstance");
           this.btnLoadInstance.Name = "btnLoadInstance";
           this.btnLoadInstance.UseVisualStyleBackColor = true;
           this.btnLoadInstance.Click += new System.EventHandler(this.btnLoadInstance_Click);
           // 
           // SetupPanel
           // 
           resources.ApplyResources(this, "$this");
           this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
           this.Controls.Add(this.uxInstanceTypeGroup);
           this.Name = "SetupPanel";
           this.setupDB.ResumeLayout(false);
           this.setupDB.PerformLayout();
           this.panelGrid.ResumeLayout(false);
           ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
           this.uxInstanceTypeGroup.ResumeLayout(false);
           this.ResumeLayout(false);
           this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox setupDB;
        private System.Windows.Forms.Label DSNLbl;
        private System.Windows.Forms.Label DBPassLbl;
        private System.Windows.Forms.Label DBUserLbl;
        private System.Windows.Forms.TextBox txtDSN;
        private System.Windows.Forms.TextBox txtDBPass;
        private System.Windows.Forms.TextBox txtDBUser;
        private System.Windows.Forms.Panel panelGrid;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.ComboBox listInstanceTypes;
        private System.Windows.Forms.GroupBox uxInstanceTypeGroup;
        private System.Windows.Forms.Button btnLoadInstance;



    }
}
