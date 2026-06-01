// Example usage file for uc_DataConnectionBase
// This file demonstrates how to use the connection dialog control

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using TheTechIdea.Beep.Winform.Default.Views.DataSource_Connection_Controls;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.Utilities;

namespace TheTechIdea.Beep.Winform.Default.Views.Examples
{
    /// <summary>
    /// Example class demonstrating how to use uc_DataConnectionBase
    /// </summary>
    public class ConnectionDialogExamples
    {
        #region Example 1: Create New Connection
        
        /// <summary>
        /// Example: Create a new connection using the dialog
        /// </summary>
        public void CreateNewConnectionExample()
        {
            // Step 1: Create a new ConnectionProperties object
            ConnectionProperties newConnection = new ConnectionProperties
            {
                ConnectionName = "My New SQL Server Connection",
                DatabaseType = DataSourceType.SqlServer,
                Category = DatasourceCategory.RDBMS
            };
            
            // Step 2: Create the dialog control
            uc_DataConnectionBase connectionDialog = new uc_DataConnectionBase();
            
            // Step 3: Initialize the dialog with connection properties
            connectionDialog.InitializeDialog(newConnection);
            
            // Step 4: Create a form to host the control
            Form dialogForm = new Form
            {
                Text = "New Connection",
                Size = new Size(700, 800),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                ShowInTaskbar = false
            };
            
            // Step 5: Add control to form
            connectionDialog.Dock = DockStyle.Fill;
            dialogForm.Controls.Add(connectionDialog);
            
            // Step 6: Show dialog and handle result
            DialogResult result = dialogForm.ShowDialog();
            
            if (result == DialogResult.OK)
            {
                // Step 7: Get updated properties
                ConnectionProperties updatedConnection = connectionDialog.GetUpdatedProperties();
                
                // Step 8: Save the connection (implement your save logic)
                SaveConnection(updatedConnection);
                
                MessageBox.Show("Connection created successfully!", "Success", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("Connection creation cancelled.", "Cancelled", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        
        #endregion
        
        #region Example 2: Edit Existing Connection
        
        /// <summary>
        /// Example: Edit an existing connection
        /// </summary>
        public void EditExistingConnectionExample(ConnectionProperties existingConnection)
        {
            // Step 1: Clone the connection to avoid modifying the original
            ConnectionProperties connectionToEdit = CloneConnection(existingConnection);
            
            // Step 2: Create and initialize dialog
            uc_DataConnectionBase connectionDialog = new uc_DataConnectionBase();
            connectionDialog.InitializeDialog(connectionToEdit);
            
            // Step 3: Create and show form
            Form dialogForm = CreateDialogForm(connectionDialog, 
                $"Edit Connection: {connectionToEdit.ConnectionName}");
            
            DialogResult result = dialogForm.ShowDialog();
            
            if (result == DialogResult.OK)
            {
                // Step 4: Get updated properties
                ConnectionProperties updatedConnection = connectionDialog.GetUpdatedProperties();
                
                // Step 5: Update the connection (implement your update logic)
                UpdateConnection(updatedConnection);
                
                MessageBox.Show("Connection updated successfully!", "Success", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        
        #endregion
        
        #region Example 3: Create Connection with Default Parameters
        
        /// <summary>
        /// Example: Create connection with default parameters pre-set
        /// </summary>
        public void CreateConnectionWithDefaultsExample()
        {
            ConnectionProperties connection = new ConnectionProperties
            {
                ConnectionName = "MySQL Connection with Defaults",
                DatabaseType = DataSourceType.Mysql,
                Category = DatasourceCategory.RDBMS
            };
            
            uc_DataConnectionBase dialog = new uc_DataConnectionBase();
            
            // Set default parameters before initializing
            dialog.DefaultParameterList = new Dictionary<string, string>
            {
                { "ConnectionTimeout", "30" },
                { "CommandTimeout", "60" },
                { "Pooling", "true" },
                { "MinPoolSize", "5" },
                { "MaxPoolSize", "100" },
                { "AllowUserVariables", "true" }
            };
            
            dialog.InitializeDialog(connection);
            
            Form form = CreateDialogForm(dialog, "New MySQL Connection");
            if (form.ShowDialog() == DialogResult.OK)
            {
                ConnectionProperties updated = dialog.GetUpdatedProperties();
                SaveConnection(updated);
            }
        }
        
        #endregion
        
        #region Example 4: Complete Connection Manager
        
        /// <summary>
        /// Example: Complete connection manager class
        /// </summary>
        public class ConnectionManager
        {
            private List<ConnectionProperties> connections = new List<ConnectionProperties>();
            
            public void AddNewConnection()
            {
                ConnectionProperties newConnection = new ConnectionProperties
                {
                    ConnectionName = "New Connection",
                    DatabaseType = DataSourceType.SqlServer,
                    Category = DatasourceCategory.RDBMS
                };
                
                if (ShowConnectionDialog(newConnection, "New Connection"))
                {
                    connections.Add(newConnection);
                    SaveConnections();
                    MessageBox.Show("Connection added successfully!", "Success", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            
            public void EditConnection(ConnectionProperties connection)
            {
                if (connection == null) return;
                
                ConnectionProperties connectionToEdit = CloneConnection(connection);
                
                if (ShowConnectionDialog(connectionToEdit, $"Edit: {connection.ConnectionName}"))
                {
                    int index = connections.FindIndex(c => c.ID == connection.ID);
                    if (index >= 0)
                    {
                        connections[index] = connectionToEdit;
                        SaveConnections();
                        MessageBox.Show("Connection updated successfully!", "Success", 
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            
            private bool ShowConnectionDialog(ConnectionProperties connection, string title)
            {
                uc_DataConnectionBase dialog = new uc_DataConnectionBase();
                dialog.InitializeDialog(connection);
                
                Form dialogForm = CreateDialogForm(dialog, title);
                DialogResult result = dialogForm.ShowDialog();
                
                if (result == DialogResult.OK)
                {
                    ConnectionProperties updated = dialog.GetUpdatedProperties();
                    
                    // Copy updated values back
                    connection.ConnectionName = updated.ConnectionName;
                    connection.DatabaseType = updated.DatabaseType;
                    connection.Category = updated.Category;
                    connection.Host = updated.Host;
                    connection.Database = updated.Database;
                    connection.ConnectionString = updated.ConnectionString;
                    connection.UserID = updated.UserID;
                    connection.Password = updated.Password;
                    connection.ParameterList = new Dictionary<string, string>(updated.ParameterList);
                    // ... copy other properties as needed
                    
                    return true;
                }
                
                return false;
            }
            
            private void SaveConnections()
            {
                // Implement your save logic here
                // e.g., save to database, file, etc.
            }
        }
        
        #endregion
        
        #region Helper Methods
        
        /// <summary>
        /// Helper method to create a dialog form
        /// </summary>
        private static Form CreateDialogForm(uc_DataConnectionBase connectionControl, string title)
        {
            Form dialogForm = new Form
            {
                Text = title,
                Size = new Size(700, 800),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                ShowInTaskbar = false
            };
            
            connectionControl.Dock = DockStyle.Fill;
            dialogForm.Controls.Add(connectionControl);
            
            return dialogForm;
        }
        
        /// <summary>
        /// Helper method to clone a connection
        /// </summary>
        private static ConnectionProperties CloneConnection(ConnectionProperties original)
        {
            if (original == null) return null;
            
            ConnectionProperties clone = new ConnectionProperties
            {
                ID = original.ID,
                ConnectionName = original.ConnectionName,
                DatabaseType = original.DatabaseType,
                Category = original.Category,
                Host = original.Host,
                Database = original.Database,
                Port = original.Port,
                ConnectionString = original.ConnectionString,
                UserID = original.UserID,
                Password = original.Password,
                AuthenticationType = original.AuthenticationType,
                DriverName = original.DriverName,
                DriverVersion = original.DriverVersion,
                FilePath = original.FilePath,
                Url = original.Url,
                // ... copy other properties as needed
            };
            
            // Copy ParameterList
            if (original.ParameterList != null)
            {
                clone.ParameterList = new Dictionary<string, string>(original.ParameterList);
            }
            
            return clone;
        }
        
        /// <summary>
        /// Placeholder for save connection logic
        /// </summary>
        private static void SaveConnection(ConnectionProperties connection)
        {
            // Implement your save logic here
            // e.g., save to database, file, configuration, etc.
        }
        
        /// <summary>
        /// Placeholder for update connection logic
        /// </summary>
        private static void UpdateConnection(ConnectionProperties connection)
        {
            // Implement your update logic here
            // e.g., update in database, file, configuration, etc.
        }
        
        #endregion
    }
}
