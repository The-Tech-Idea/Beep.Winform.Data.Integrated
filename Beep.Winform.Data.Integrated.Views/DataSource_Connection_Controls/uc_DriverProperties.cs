using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using TheTechIdea.Beep.DriversConfigurations;
using TheTechIdea.Beep.Winform.Controls;
using TheTechIdea.Beep.ConfigUtil;

namespace TheTechIdea.Beep.Winform.Default.Views.DataSource_Connection_Controls
{
    /// <summary>
    /// Tab control for Driver region of IConnectionProperties
    /// Properties: DriverName, DriverVersion.
    /// Extra provider-specific values are edited through ParameterList (non-typed keys only).
    /// </summary>
    public partial class uc_DriverProperties : uc_DataConnectionPropertiesBaseControl
    {
        private static readonly HashSet<string> StronglyTypedPropertyNames =
            new HashSet<string>(
                typeof(ConnectionProperties)
                    .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(p => p.CanRead && p.CanWrite && p.GetIndexParameters().Length == 0)
                    .Select(p => p.Name),
                StringComparer.OrdinalIgnoreCase);

        private bool _updatingExtrasText;

        public uc_DriverProperties()
        {
            InitializeComponent();
        }

        public override void SetupBindings(ConnectionProperties conn)
        {
            base.SetupBindings(conn);
            Text = "Driver";
            if (conn == null) return;

            // Clear existing bindings
            Driver_DriverNamebeepTextBox.DataBindings.Clear();
            Driver_DriverVersionbeepTextBox.DataBindings.Clear();
            Driver_ParametersbeepTextBox.DataBindings.Clear();

            // Bindings for Driver region
            Driver_DriverNamebeepTextBox.DataBindings.Add(new Binding("Text", conn, nameof(conn.DriverName), true, DataSourceUpdateMode.OnPropertyChanged));
            Driver_DriverVersionbeepTextBox.DataBindings.Add(new Binding("Text", conn, nameof(conn.DriverVersion), true, DataSourceUpdateMode.OnPropertyChanged));
            EnsureParameterList(conn);
            RemoveTypedKeysFromParameterList(conn.ParameterList);
            RefreshExtrasDisplay(conn);

            Driver_ParametersbeepTextBox.TextChanged -= Driver_ParametersbeepTextBox_TextChanged;
            Driver_ParametersbeepTextBox.TextChanged += Driver_ParametersbeepTextBox_TextChanged;

            RefreshDriverCompatibilityBadge(null);
        }

        private void Driver_ParametersbeepTextBox_TextChanged(object sender, EventArgs e)
        {
            if (_updatingExtrasText || ConnectionProperties == null)
            {
                return;
            }

            EnsureParameterList(ConnectionProperties);
            RemoveTypedKeysFromParameterList(ConnectionProperties.ParameterList);

            var parsedExtras = ParseExtras(Driver_ParametersbeepTextBox.Text);
            var keysToRemove = ConnectionProperties.ParameterList.Keys
                .Where(k => !StronglyTypedPropertyNames.Contains(k))
                .ToList();

            foreach (var key in keysToRemove)
            {
                ConnectionProperties.ParameterList.Remove(key);
            }

            foreach (var item in parsedExtras)
            {
                ConnectionProperties.ParameterList[item.Key] = item.Value;
            }
        }

        private void RefreshExtrasDisplay(ConnectionProperties conn)
        {
            _updatingExtrasText = true;
            Driver_ParametersbeepTextBox.Text = string.Join("; ",
                conn.ParameterList
                    .Where(kvp => !StronglyTypedPropertyNames.Contains(kvp.Key))
                    .Where(kvp => !string.IsNullOrWhiteSpace(kvp.Key))
                    .Select(kvp => $"{kvp.Key}={kvp.Value}"));
            _updatingExtrasText = false;
        }

        private static Dictionary<string, string> ParseExtras(string raw)
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (string.IsNullOrWhiteSpace(raw))
            {
                return result;
            }

            var entries = raw.Split(new[] { ';', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var entry in entries)
            {
                var parts = entry.Split(new[] { '=' }, 2, StringSplitOptions.None);
                var key = parts[0].Trim();
                if (string.IsNullOrWhiteSpace(key))
                {
                    continue;
                }

                if (StronglyTypedPropertyNames.Contains(key))
                {
                    continue;
                }

                var value = parts.Length > 1 ? parts[1].Trim() : string.Empty;
                result[key] = value;
            }

            return result;
        }

        private static void RemoveTypedKeysFromParameterList(IDictionary<string, string> parameterList)
        {
            if (parameterList == null || parameterList.Count == 0)
            {
                return;
            }

            var keys = parameterList.Keys.Where(StronglyTypedPropertyNames.Contains).ToList();
            foreach (var key in keys)
            {
                parameterList.Remove(key);
            }
        }

        public void SetDriverRecommendation(ConnectionDriversConfig recommendedDriver, bool isCompatible, string fallbackReason)
        {
            if (recommendedDriver == null)
            {
                RefreshDriverCompatibilityBadge(null);
                _fallbackReasonBeepLabel.Text = fallbackReason ?? string.Empty;
                return;
            }

            if (ConnectionProperties != null)
            {
                if (string.IsNullOrWhiteSpace(ConnectionProperties.DriverName))
                {
                    ConnectionProperties.DriverName = recommendedDriver.PackageName;
                    Driver_DriverNamebeepTextBox.Text = ConnectionProperties.DriverName;
                }

                if (string.IsNullOrWhiteSpace(ConnectionProperties.DriverVersion))
                {
                    ConnectionProperties.DriverVersion = recommendedDriver.NuggetVersion ?? recommendedDriver.version;
                    Driver_DriverVersionbeepTextBox.Text = ConnectionProperties.DriverVersion;
                }
            }

            RefreshDriverCompatibilityBadge(isCompatible);
            _fallbackReasonBeepLabel.Text = string.IsNullOrWhiteSpace(fallbackReason)
                ? "Driver recommendation source: package+version -> package -> datasource type -> extension."
                : fallbackReason;
        }

        private void RefreshDriverCompatibilityBadge(bool? isCompatible)
        {
            _compatibilityBeepLabel.Text = isCompatible switch
            {
                true => "Compatibility: Compatible",
                false => "Compatibility: Incompatible",
                _ => "Compatibility: Pending"
            };
        }
    }
}

