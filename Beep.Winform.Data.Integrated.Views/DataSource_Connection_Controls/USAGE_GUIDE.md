# DataSource Connection Controls - Developer Usage Guide

## Overview

The `uc_DataConnectionBase` control is a dialog-based user control for creating and editing data source connections. It accepts a `ConnectionProperties` object, allows the user to edit it through a tabbed interface, and returns the updated properties when the user clicks Save.

## Basic Usage Pattern

### 1. Creating a New Connection

```csharp
using TheTechIdea.Beep.Winform.Default.Views.DataSource_Connection_Controls;
using TheTechIdea.Beep.ConfigUtil;
using System.Windows.Forms;

// Create a new connection
public void CreateNewConnection()
{
    // Create a new ConnectionProperties object
    ConnectionProperties newConnection = new ConnectionProperties
    {
        ConnectionName = "My New Connection",
        DatabaseType = DataSourceType.SQLServer,
        Category = DatasourceCategory.Database
    };
    
    // Create the dialog control
    uc_DataConnectionBase connectionDialog = new uc_DataConnectionBase();
    
    // Initialize the dialog with the connection properties
    connectionDialog.InitializeDialog(newConnection);
    
    // Create a form to host the control
    Form dialogForm = new Form
    {
        Text = "New Connection",
        Size = new Size(700, 800),
        StartPosition = FormStartPosition.CenterParent,
        FormBorderStyle = FormBorderStyle.FixedDialog,
        MaximizeBox = false,
        MinimizeBox = false
    };
    
    // Add the control to the form
    connectionDialog.Dock = DockStyle.Fill;
    dialogForm.Controls.Add(connectionDialog);
    
    // Show the dialog
    DialogResult result = dialogForm.ShowDialog();
    
    // Check if user clicked Save
    if (result == DialogResult.OK)
    {
        // Get the updated connection properties
        ConnectionProperties updatedConnection = connectionDialog.GetUpdatedProperties();
        
        // Save the connection to your data store
        SaveConnection(updatedConnection);
        
        MessageBox.Show("Connection saved successfully!", "Success", 
            MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
    else
    {
        // User cancelled - do nothing or show message
        MessageBox.Show("Connection creation cancelled.", "Cancelled", 
            MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
}
```

### 2. Editing an Existing Connection

```csharp
public void EditExistingConnection(ConnectionProperties existingConnection)
{
    // Create a copy of the connection to avoid modifying the original
    ConnectionProperties connectionToEdit = CloneConnection(existingConnection);
    
    // Create the dialog control
    uc_DataConnectionBase connectionDialog = new uc_DataConnectionBase();
    
    // Initialize with existing connection
    connectionDialog.InitializeDialog(connectionToEdit);
    
    // Create and show the form
    Form dialogForm = new Form
    {
        Text = $"Edit Connection: {connectionToEdit.ConnectionName}",
        Size = new Size(700, 800),
        StartPosition = FormStartPosition.CenterParent,
        FormBorderStyle = FormBorderStyle.FixedDialog,
        MaximizeBox = false,
        MinimizeBox = false
    };
    
    connectionDialog.Dock = DockStyle.Fill;
    dialogForm.Controls.Add(connectionDialog);
    
    DialogResult result = dialogForm.ShowDialog();
    
    if (result == DialogResult.OK)
    {
        // Get updated properties
        ConnectionProperties updatedConnection = connectionDialog.GetUpdatedProperties();
        
        // Update in your data store
        UpdateConnection(updatedConnection);
        
        MessageBox.Show("Connection updated successfully!", "Success", 
            MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
}
```

### 3. Using with a Specific Connection Type (Inherited Control)

If you have a specific connection type control that inherits from `uc_DataConnectionBase`:

```csharp
public void CreateSQLServerConnection()
{
    ConnectionProperties sqlConnection = new ConnectionProperties
    {
        ConnectionName = "SQL Server Connection",
        DatabaseType = DataSourceType.SQLServer,
        Category = DatasourceCategory.Database,
        ServerName = "localhost",
        DatabaseName = "MyDatabase",
        AuthenticationType = AuthenticationType.SQLAuthentication,
        UserID = "sa",
        Password = "password123"
    };
    
    // Use the base control or a specific inherited control
    uc_DataConnectionBase connectionDialog = new uc_DataConnectionBase();
    connectionDialog.InitializeDialog(sqlConnection);
    
    // Show dialog and handle result
    Form dialogForm = CreateDialogForm(connectionDialog, "SQL Server Connection");
    DialogResult result = dialogForm.ShowDialog();
    
    if (result == DialogResult.OK)
    {
        ConnectionProperties updated = connectionDialog.GetUpdatedProperties();
        SaveConnection(updated);
    }
}
```

## Helper Methods

### Creating a Dialog Form

```csharp
private Form CreateDialogForm(uc_DataConnectionBase connectionControl, string title)
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
```

### Cloning Connection Properties

```csharp
private ConnectionProperties CloneConnection(ConnectionProperties original)
{
    // Create a new instance and copy properties
    ConnectionProperties clone = new ConnectionProperties
    {
        ID = original.ID,
        ConnectionName = original.ConnectionName,
        DatabaseType = original.DatabaseType,
        Category = original.Category,
        ServerName = original.ServerName,
        DatabaseName = original.DatabaseName,
        Port = original.Port,
        ConnectionString = original.ConnectionString,
        UserID = original.UserID,
        Password = original.Password,
        AuthenticationType = original.AuthenticationType,
        DriverName = original.DriverName,
        DriverVersion = original.DriverVersion,
        // ... copy other properties
    };
    
    // Copy ParameterList
    if (original.ParameterList != null)
    {
        clone.ParameterList = new Dictionary<string, string>(original.ParameterList);
    }
    
    return clone;
}
```

## Setting Default Parameters

You can set default parameters for specific connection types before showing the dialog:

```csharp
public void CreateConnectionWithDefaults()
{
    ConnectionProperties connection = new ConnectionProperties
    {
        ConnectionName = "New Connection",
        DatabaseType = DataSourceType.MySQL,
        Category = DatasourceCategory.Database
    };
    
    uc_DataConnectionBase dialog = new uc_DataConnectionBase();
    
    // Set default parameters for this connection type
    dialog.DefaultParameterList = new Dictionary<string, string>
    {
        { "ConnectionTimeout", "30" },
        { "CommandTimeout", "60" },
        { "Pooling", "true" },
        { "MinPoolSize", "5" },
        { "MaxPoolSize", "100" }
    };
    
    dialog.InitializeDialog(connection);
    
    Form form = CreateDialogForm(dialog, "New Connection");
    if (form.ShowDialog() == DialogResult.OK)
    {
        ConnectionProperties updated = dialog.GetUpdatedProperties();
        SaveConnection(updated);
    }
}
```

## Complete Example: Connection Manager

Here's a complete example of a connection manager that uses the dialog:

```csharp
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using TheTechIdea.Beep.Winform.Default.Views.DataSource_Connection_Controls;
using TheTechIdea.Beep.ConfigUtil;

public class ConnectionManager
{
    private List<ConnectionProperties> connections = new List<ConnectionProperties>();
    
    public void AddNewConnection()
    {
        ConnectionProperties newConnection = new ConnectionProperties
        {
            ConnectionName = "New Connection",
            DatabaseType = DataSourceType.SQLServer,
            Category = DatasourceCategory.Database
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
        
        // Clone to avoid modifying original
        ConnectionProperties connectionToEdit = CloneConnection(connection);
        
        if (ShowConnectionDialog(connectionToEdit, $"Edit: {connection.ConnectionName}"))
        {
            // Update the original connection
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
        
        dialog.Dock = DockStyle.Fill;
        dialogForm.Controls.Add(dialog);
        
        DialogResult result = dialogForm.ShowDialog();
        
        if (result == DialogResult.OK)
        {
            // Get updated properties
            ConnectionProperties updated = dialog.GetUpdatedProperties();
            
            // Copy updated values back to the original connection object
            connection.ConnectionName = updated.ConnectionName;
            connection.DatabaseType = updated.DatabaseType;
            connection.Category = updated.Category;
            connection.ServerName = updated.ServerName;
            connection.DatabaseName = updated.DatabaseName;
            connection.ConnectionString = updated.ConnectionString;
            connection.UserID = updated.UserID;
            connection.Password = updated.Password;
            connection.ParameterList = new Dictionary<string, string>(updated.ParameterList);
            // ... copy other properties as needed
            
            return true;
        }
        
        return false;
    }
    
    private ConnectionProperties CloneConnection(ConnectionProperties original)
    {
        // Implementation from helper method above
        return new ConnectionProperties { /* ... */ };
    }
    
    private void SaveConnections()
    {
        // Save connections to your data store (database, file, etc.)
        // Implementation depends on your storage mechanism
    }
}
```

## Key Points

1. **Always use `InitializeDialog()`** - This sets up all bindings and initializes the control
2. **Host in a Form** - The control must be hosted in a Form to show as a dialog
3. **Check DialogResult** - Always check if the user clicked Save (OK) or Cancel
4. **Use `GetUpdatedProperties()`** - This returns the updated ConnectionProperties after the user clicks Save
5. **Clone for Editing** - When editing, clone the connection to avoid modifying the original until saved
6. **Set DefaultParameterList** - Set default parameters before calling `InitializeDialog()` if needed

## Testing the Connection

The dialog includes a "Test Connection" button that users can click to validate their connection settings. The test is performed asynchronously and shows success/failure messages.

## Validation

The dialog automatically validates:
- Required fields based on connection category (Database, File, WebAPI)
- Authentication requirements based on DatabaseType
- Connection string or server name requirements

Validation errors are shown to the user before allowing save.

## ParameterList Usage

Provider-specific parameters are stored in `ConnectionProperties.ParameterList` (Dictionary<string, string>). The Driver Properties tab shows these as formatted key=value pairs that can be edited.

## Helper-Centric Behavior Matrix

| UI Area | Helper Entry Point | Behavior |
|---|---|---|
| Driver tab + save flow | `ConnectionHelper.GetBestMatchingDriver`, `ConnectionDriverLinkingHelper.IsDriverCompatible` | Applies package/version/type fallback recommendation and shows compatibility/fallback reason before save. |
| Connection string processing | `ConnectionHelper.ReplaceValueFromConnectionString` | Expands template placeholders to final provider string before persistence/test. |
| Placeholder checks | `ConnectionHelper.ValidateRequiredPlaceholders` | Surfaces missing placeholders in inline requirements label. |
| Path normalization | `ConnectionHelper.NormalizeFilePath`, `ConnectionHelper.NormalizePath` | Normalizes file connection paths before save/test/open lifecycle. |
| Provider validation | `ConnectionHelper.ValidateConnectionStringStructure`, `ConnectionHelper.IsConnectionStringValid`, `ConnectionHelper.GetValidationRequirements` | Produces inline validation status and provider requirement hints. |
| Security and preview | `ConnectionHelper.SecureConnectionString`, `ConnectionHelper.SelectiveMask`, `ConnectionHelper.ContainsSensitiveInformation` | Masks preview/log text and flags sensitive values to avoid plain-text leakage. |
| Save lifecycle | `ConfigEditor.DataConnections` + `DataConnectionManager.SaveDataConnectionsValues` | Persists new/updated connection and keeps single editor/config service usage. |
| Optional open after save | `IDMEEditor.CreateNewDataSourceConnection` | Executes persist -> optional open sequence with warning if open fails. |

## Tab Visibility Rules

- `Database`, `Network`, `Authentication` tabs are shown for database-style profiles (`Category=RDBMS` or `IsDatabase`).
- `File` tab is shown for file profiles (`Category=FILE` or `IsFile`).
- `Web API` + `OAuth/API Auth` tabs are shown for API profiles (`Category=WEBAPI` or `IsWebApi`).
- `Driver` remains visible for all profile types.

## Migration Notes

- Legacy free-form save behavior is now tightened into this sequence: `bind -> normalize/process -> validate -> secure preview/log -> persist -> optional open`.
- Save now blocks duplicate connection names to avoid conflicting entries in shared configuration.
- Driver recommendation is authoritative and helper-driven; manual values still allowed but incompatibility is clearly surfaced in the Driver tab.
