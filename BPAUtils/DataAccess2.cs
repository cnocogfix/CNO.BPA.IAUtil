using System;
using System.Globalization;
using System.Text;
using System.Data;
using Oracle.ManagedDataAccess.Client;
using System.Collections.Generic;
using System.Xml;


namespace CNO.BPA.IAUtil
{
   public class DataAccess2
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



      private string _AppName;
      private string _DSN = "";
      private string _DBUser = "";
      private string _DBPass = "";
      #endregion


      public DataAccess2(string DSN, string User, string Pass, string appName)
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
            throw new Exception("An error occurred while connecting to the database. " + ex.Message, ex);
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
         _transaction.Rollback();
         _connection.Close();
         _connection = null;
         _transaction = null;
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






   }

}
