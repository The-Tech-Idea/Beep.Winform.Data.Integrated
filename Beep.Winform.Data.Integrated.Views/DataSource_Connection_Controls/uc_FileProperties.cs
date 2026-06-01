using System;
using System.Windows.Forms;
using TheTechIdea.Beep.ConfigUtil;

namespace TheTechIdea.Beep.Winform.Default.Views.DataSource_Connection_Controls
{
    /// <summary>
    /// Tab control for File Properties region of IConnectionProperties
    /// Properties: FilePath, FileName, Ext, Delimiter
    /// </summary>
    public partial class uc_FileProperties : uc_DataConnectionPropertiesBaseControl
    {
        public uc_FileProperties()
        {
            InitializeComponent();
        }

        public override void SetupBindings(ConnectionProperties conn)
        {
            base.SetupBindings(conn);
            Text = "File";
            if (conn == null) return;

            // Clear existing bindings
            File_FilePathbeepTextBox.DataBindings.Clear();
            File_FileNamebeepTextBox.DataBindings.Clear();
            File_ExtbeepTextBox.DataBindings.Clear();
            File_DelimiterbeepTextBox.DataBindings.Clear();

            // Bindings for File Properties region
            File_FilePathbeepTextBox.DataBindings.Add(new Binding("Text", conn, nameof(conn.FilePath), true, DataSourceUpdateMode.OnPropertyChanged));
            File_FileNamebeepTextBox.DataBindings.Add(new Binding("Text", conn, nameof(conn.FileName), true, DataSourceUpdateMode.OnPropertyChanged));
            File_ExtbeepTextBox.DataBindings.Add(new Binding("Text", conn, nameof(conn.Ext), true, DataSourceUpdateMode.OnPropertyChanged));
            File_DelimiterbeepTextBox.DataBindings.Add(new Binding("Text", conn, nameof(conn.Delimiter), true, DataSourceUpdateMode.OnPropertyChanged));
        }
    }
}
