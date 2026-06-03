using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.Helpers;
using TheTechIdea.Beep.Services;
using TheTechIdea.Beep.SetUp.Steps;
using TheTechIdea.Beep.Winform.Controls;
using TheTechIdea.Beep.Winform.Controls.Wizards;
using TheTechIdea.Beep.Winform.Default.Views.DataSource_Connection_Controls;

namespace TheTechIdea.Beep.Winform.Default.Views.Setup
{
    public partial class uc_SetupConnectionStep : UserControl, IWizardStepContent
    {
        private uc_DataConnectionBase? _connectionEditor;
        private WizardContext? _wizardContext;
        private bool _isComplete;

        public event EventHandler<ConnectionSavedEventArgs>? ConnectionSaved;
        public event EventHandler? ConnectionCancelled;
        public event EventHandler<ConnectionTestCompletedEventArgs>? ConnectionTestCompleted;

        public sealed class ConnectionSavedEventArgs : EventArgs
        {
            public ConnectionSavedEventArgs(ConnectionProperties connectionProperties)
            {
                ConnectionProperties = connectionProperties;
            }

            public ConnectionProperties ConnectionProperties { get; }
        }

        public sealed class ConnectionTestCompletedEventArgs : EventArgs
        {
            public ConnectionTestCompletedEventArgs(bool success, string message)
            {
                Success = success;
                Message = message;
            }

            public bool Success { get; }
            public string Message { get; }
        }

        public uc_SetupConnectionStep()
        {
            InitializeComponent();
        }

        public bool IsComplete
        {
            get => _isComplete;
            private set
            {
                if (_isComplete == value) return;
                _isComplete = value;
                ValidationStateChanged?.Invoke(this, new StepValidationEventArgs(_isComplete, _isComplete ? string.Empty : "Connection not ready"));
            }
        }

        public event EventHandler<StepValidationEventArgs>? ValidationStateChanged;

        public string NextButtonText { get; set; } = string.Empty;

        public void InitializeStep(IBeepService? beepService, string theme)
        {
            _contentHost.Controls.Clear();

            _connectionEditor = new uc_DataConnectionBase
            {
                Dock = DockStyle.Fill
            };

            if (beepService != null)
                _connectionEditor.BeepService = beepService;

            _connectionEditor.ConnectionSaved += ConnectionEditor_ConnectionSaved;
            _connectionEditor.ConnectionCancelled += ConnectionEditor_ConnectionCancelled;
            _connectionEditor.ConnectionTestCompleted += ConnectionEditor_ConnectionTestCompleted;

            _connectionEditor.InitializeDialog(new ConnectionProperties());
            _contentHost.Controls.Add(_connectionEditor);

            ApplyTheme(theme);
        }

        public ConnectionProperties? GetConnectionProperties()
        {
            return _connectionEditor?.GetUpdatedProperties();
        }

        public void SetConnectionProperties(ConnectionProperties connectionProperties)
        {
            if (_connectionEditor == null)
                return;

            _connectionEditor.InitializeDialog(connectionProperties ?? new ConnectionProperties());
        }

        /// <summary>
        /// Returns a connection-properties snapshot seeded with the best-matching driver
        /// resolved through <see cref="ConnectionHelper"/>, ready to feed a
        /// <see cref="ConnectionConfigStep"/>.
        /// </summary>
        public ConnectionProperties? GetConnectionPropertiesForStep()
        {
            var cp = GetConnectionProperties();
            if (cp == null)
                return null;

            var editor = _connectionEditor?.BeepService?.DMEEditor;
            if (editor?.ConfigEditor != null)
            {
                var driver = ConnectionHelper.GetBestMatchingDriver(cp, editor.ConfigEditor);
                if (driver != null)
                {
                    cp.DriverName = driver.PackageName;
                    cp.DriverVersion = driver.version;
                }
            }

            return cp;
        }

        public bool IsReadyForSetup()
        {
            var cp = GetConnectionProperties();
            if (cp == null) return false;
            if (string.IsNullOrWhiteSpace(cp.ConnectionName)) return false;
            if (cp.DatabaseType == DataSourceType.Unknown) return false;
            if (string.IsNullOrWhiteSpace(cp.ConnectionString)) return false;
            return true;
        }

        public string GetStepSummary()
        {
            var cp = GetConnectionProperties();
            if (cp == null)
                return "Connection: Not initialized.";

            var name = string.IsNullOrWhiteSpace(cp.ConnectionName) ? "(unnamed)" : cp.ConnectionName;
            var dbType = cp.DatabaseType.ToString();
            var hasConnectionString = string.IsNullOrWhiteSpace(cp.ConnectionString) ? "No" : "Yes";
            var driver = string.IsNullOrWhiteSpace(cp.DriverName) ? "(unresolved)" : cp.DriverName;
            var ready = IsReadyForSetup() ? "Ready" : "Incomplete";
            return $"Connection: Name={name}, Type={dbType}, ConnectionString={hasConnectionString}, Driver={driver}, Status={ready}";
        }

        public void ApplyTheme(string theme)
        {
            ApplyThemeToControl(_rootPanel, theme);
            ApplyThemeToControl(_headerPanel, theme);
            ApplyThemeToControl(_contentHost, theme);
            ApplyThemeToControl(_lblTitle, theme);
            ApplyThemeToControl(_lblDescription, theme);

            if (_connectionEditor is IBeepUIComponent beepControl)
                beepControl.Theme = theme;
        }

        private static void ApplyThemeToControl(Control control, string theme)
        {
            if (control is IBeepUIComponent beepComponent)
                beepComponent.Theme = theme;
        }

        private void ConnectionEditor_ConnectionSaved(object? sender, uc_DataConnectionBase.ConnectionSavedEventArgs e)
        {
            ConnectionSaved?.Invoke(this, new ConnectionSavedEventArgs(e.ConnectionProperties));
        }

        private void ConnectionEditor_ConnectionCancelled(object? sender, EventArgs e)
        {
            ConnectionCancelled?.Invoke(this, EventArgs.Empty);
        }

        private void ConnectionEditor_ConnectionTestCompleted(object? sender, uc_DataConnectionBase.ConnectionTestCompletedEventArgs e)
        {
            ConnectionTestCompleted?.Invoke(this, new ConnectionTestCompletedEventArgs(e.Success, e.Message));
        }

        void IWizardStepContent.OnStepEnter(WizardContext context)
        {
            _wizardContext = context;
            IsComplete = IsReadyForSetup();
        }

        void IWizardStepContent.OnStepLeave(WizardContext context)
        {
            var cp = GetConnectionPropertiesForStep();
            if (cp != null && !string.IsNullOrWhiteSpace(cp.ConnectionName))
            {
                context.SetValue("connectionProperties", cp);
                context.SetValue("connectionName", cp.ConnectionName);
            }
        }

        WizardValidationResult IWizardStepContent.Validate()
        {
            if (!IsReadyForSetup())
                return WizardValidationResult.Error("Connection configuration is incomplete. Fill all required fields before continuing.");

            return WizardValidationResult.Success();
        }

        Task<WizardValidationResult> IWizardStepContent.ValidateAsync()
        {
            return Task.FromResult(((IWizardStepContent)this).Validate());
        }
    }
}
