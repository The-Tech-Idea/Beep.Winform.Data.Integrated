using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TheTechIdea.Beep.ConfigUtil;

namespace TheTechIdea.Beep.Winform.Default.Views.DataSource_Connection_Controls
{
    /// <summary>
    /// Base class for connection property tab controls.
    /// Each child control represents one tab/region of IConnectionProperties.
    /// </summary>
    public partial class uc_DataConnectionPropertiesBaseControl : UserControl
    {
        // Main data object - ConnectionProperties that will be passed in and returned
        protected ConnectionProperties _connectionProperties;
        
        /// <summary>
        /// Extra parameters for specific connection types
        /// </summary>
        public Dictionary<string, string> DefaultParameterList { get; set; } = new Dictionary<string, string>();
        
        public ConnectionProperties ConnectionProperties
        {
            get => _connectionProperties;
            set
            {
                _connectionProperties = value;
                if (_connectionProperties != null)
                {
                    SetupBindings(_connectionProperties);
                }
            }
        }

        public virtual void SetupBindings(ConnectionProperties conn)
        {
            // Override in child classes to bind controls to ConnectionProperties
        }

        protected void EnsureParameterList(ConnectionProperties conn)
        {
            if (conn != null && conn.ParameterList == null)
            {
                conn.ParameterList = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            }
        }

        protected void EnsureParameter(ConnectionProperties conn, string parameterName, string defaultValue = "")
        {
            if (conn == null || string.IsNullOrWhiteSpace(parameterName))
            {
                return;
            }

            EnsureParameterList(conn);
            if (!conn.ParameterList.ContainsKey(parameterName))
            {
                conn.ParameterList[parameterName] = defaultValue ?? string.Empty;
            }
        }

        protected void BindTextToParameterList(Control control, ConnectionProperties conn, string parameterName, string defaultValue = "")
        {
            if (control == null || conn == null || string.IsNullOrWhiteSpace(parameterName))
            {
                return;
            }

            EnsureParameter(conn, parameterName, defaultValue);
            control.DataBindings.Clear();

            var binding = new Binding("Text", conn.ParameterList, $"[{parameterName}]", true, DataSourceUpdateMode.OnPropertyChanged);
            binding.Format += (s, e) => e.Value = e.Value?.ToString() ?? defaultValue ?? string.Empty;
            binding.Parse += (s, e) => e.Value = e.Value?.ToString() ?? defaultValue ?? string.Empty;
            control.DataBindings.Add(binding);
        }

        protected void BindCheckBoxToParameterList(CheckBox checkBox, ConnectionProperties conn, string parameterName, bool defaultValue = false)
        {
            if (checkBox == null || conn == null || string.IsNullOrWhiteSpace(parameterName))
            {
                return;
            }

            EnsureParameter(conn, parameterName, defaultValue.ToString().ToLowerInvariant());
            checkBox.DataBindings.Clear();

            var binding = new Binding("Checked", conn.ParameterList, $"[{parameterName}]", true, DataSourceUpdateMode.OnPropertyChanged);
            binding.Format += (s, e) =>
            {
                var value = e.Value?.ToString();
                e.Value = bool.TryParse(value, out var parsed) ? parsed : defaultValue;
            };
            binding.Parse += (s, e) =>
            {
                var checkedValue = e.Value is bool flag && flag;
                e.Value = checkedValue.ToString().ToLowerInvariant();
            };
            checkBox.DataBindings.Add(binding);
        }

        protected string FormatParameterListAsText(ConnectionProperties conn)
        {
            if (conn?.ParameterList == null || conn.ParameterList.Count == 0)
            {
                return string.Empty;
            }

            return string.Join("; ",
                conn.ParameterList
                    .Where(kvp => !string.IsNullOrWhiteSpace(kvp.Key))
                    .Select(kvp => $"{kvp.Key}={kvp.Value}"));
        }

        protected void ParseParameterListText(ConnectionProperties conn, string text)
        {
            if (conn == null)
            {
                return;
            }

            EnsureParameterList(conn);
            conn.ParameterList.Clear();

            if (string.IsNullOrWhiteSpace(text))
            {
                return;
            }

            var pairs = text.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var pair in pairs)
            {
                var parts = pair.Split(new[] { '=' }, 2, StringSplitOptions.None);
                if (parts.Length != 2)
                {
                    continue;
                }

                var key = parts[0].Trim();
                if (string.IsNullOrWhiteSpace(key))
                {
                    continue;
                }

                conn.ParameterList[key] = parts[1].Trim();
            }
        }

        public uc_DataConnectionPropertiesBaseControl()
        {
            InitializeComponent();
        }

        protected virtual void InitializeComponent()
        {
            SuspendLayout();
            // 
            // uc_DataConnectionPropertiesBaseControl
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Name = "uc_DataConnectionPropertiesBaseControl";
            Size = new Size(726, 694);
            ResumeLayout(false);
        }
    }
}
