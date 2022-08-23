using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CNO.BPA.IAUtil;




namespace WindowsFormsApplication1
{
   public partial class Form1 : Form
   {
      public Form1()
      {
         InitializeComponent();
      }

      private void button1_Click(object sender, EventArgs e)
      {

         DataAccess2 da = new DataAccess2( "BPAD", "mwDiWvgcGRZWa+FybiRAjg==", "mwDiWvgcGRZWa+FybiRAjg==", "test");
         da.Connect();

         MessageBox.Show("OPEN");

      }
   }
}
