using System;
using System.Windows.Forms;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.Winform.Controls.Layouts.Helpers;
using TheTechIdea.Beep.Helpers;
using TheTechIdea.Beep.Services;
using TheTechIdea.Beep.SetUp;
using TheTechIdea.Beep.SetUp.Steps;
using TheTechIdea.Beep.Winform.Controls;
using TheTechIdea.Beep.Winform.Default.Views.DataSource_Connection_Controls;
using TheTechIdea.Beep.Winform.Default.Views.Template;

namespace TheTechIdea.Beep.Winform.Default.Views.Setup
{
    // Full refactor: UI step wraps a typed ConnectionConfigStepOptions directly.
    // No more IWizardStepContent / WizardContext / Properties[] dictionary hack.
    // The canonical ConnectionConfigStep reads options.ConnectionProperties during Execute,
    // so UI mutations take effect immediately.
    public partial class uc_SetupConnectionStep : TemplateUserControl
    {
        private uc_DataConnectionBase? _connectionEditor;
        private ConnectionConfigStepOptions? _options;
        private SetupContext? _context;

        // These reuse uc_DataConnectionBase's event args rather than re-declaring identical nested
        // clones. This step only embeds that editor and forwards its events, so a second set of
        // types was pure duplication — and the clone of ConnectionTestCompletedEventArgs silently
        // dropped ConnectionProperties, giving subscribers less than the source event carried.
        public event EventHandler<uc_DataConnectionBase.ConnectionSavedEventArgs>? ConnectionSaved;
        public event EventHandler? ConnectionCancelled;
        public event EventHandler<uc_DataConnectionBase.ConnectionTestCompletedEventArgs>? ConnectionTestCompleted;

        public uc_SetupConnectionStep()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Bind the connection editor to the typed <see cref="ConnectionConfigStepOptions"/>.
        /// The wizard host passes the same options object that will be handed to the canonical
        /// <see cref="ConnectionConfigStep"/> when execution starts, so UI changes flow into
        /// Execute without a string-keyed dictionary.
        /// </summary>
        public void InitializeStep(ConnectionConfigStepOptions options, SetupContext context)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _context = context ?? throw new ArgumentNullException(nameof(context));

            _contentHost.Controls.Clear();

            // Seed the editor with whatever the wizard host has pre-populated.
            var initial = options.ConnectionProperties ?? new ConnectionProperties();

            _connectionEditor = new uc_DataConnectionBase { Dock = DockStyle.Fill };
            if (_context.Properties.TryGetValue("beepService", out var svcObj) && svcObj is IBeepService svc)
                _connectionEditor.BeepService = svc;

            _connectionEditor.ConnectionSaved += ConnectionEditor_ConnectionSaved;
            _connectionEditor.ConnectionCancelled += ConnectionEditor_ConnectionCancelled;
            _connectionEditor.ConnectionTestCompleted += ConnectionEditor_ConnectionTestCompleted;

            _connectionEditor.InitializeDialog(initial);
            _contentHost.Controls.Add(_connectionEditor);
        }

        /// <summary>
        /// Returns the current connection-properties snapshot from the editor. Useful for callers
        /// that want to read state without subscribing to <see cref="ConnectionSaved"/>.
        /// </summary>
        public ConnectionProperties? GetConnectionProperties()
            => _connectionEditor?.GetUpdatedProperties();

        /// <summary>
        /// True when the user has supplied a connection name + type + connection string.
        /// Drives the next-button enabled state on the wizard host.
        /// </summary>
        public bool IsReadyForSetup()
        {
            var cp = GetConnectionProperties();
            if (cp == null) return false;
            if (string.IsNullOrWhiteSpace(cp.ConnectionName)) return false;
            if (cp.DatabaseType == DataSourceType.Unknown) return false;
            if (string.IsNullOrWhiteSpace(cp.ConnectionString)) return false;
            return true;
        }

        /// <summary>One-line human-readable summary for the review step.</summary>
        public string GetStepSummary()
        {
            var cp = GetConnectionProperties();
            if (cp == null) return "Connection: Not initialized.";

            var name = string.IsNullOrWhiteSpace(cp.ConnectionName) ? "(unnamed)" : cp.ConnectionName;
            var dbType = cp.DatabaseType.ToString();
            var hasConnectionString = string.IsNullOrWhiteSpace(cp.ConnectionString) ? "No" : "Yes";
            var driver = string.IsNullOrWhiteSpace(cp.DriverName) ? "(unresolved)" : cp.DriverName;
            var ready = IsReadyForSetup() ? "Ready" : "Incomplete";
            return $"Connection: Name={name}, Type={dbType}, ConnectionString={hasConnectionString}, Driver={driver}, Status={ready}";
        }

        private void ConnectionEditor_ConnectionSaved(object? sender, uc_DataConnectionBase.ConnectionSavedEventArgs e)
        {
            // Skill: write the user-edited connection DIRECTLY into the typed options, so the
            // canonical ConnectionConfigStep reads it during Execute. No dictionary, no reflection.
            if (_options != null)
                _options.ConnectionProperties = e.ConnectionProperties;

            ConnectionSaved?.Invoke(this, e);
        }

        private void ConnectionEditor_ConnectionCancelled(object? sender, EventArgs e)
        {
            ConnectionCancelled?.Invoke(this, EventArgs.Empty);
        }

        private void ConnectionEditor_ConnectionTestCompleted(object? sender, uc_DataConnectionBase.ConnectionTestCompletedEventArgs e)
        {
            // Forwarded as-is: the base's args also carry ConnectionProperties, which the old
            // re-wrap discarded.
            ConnectionTestCompleted?.Invoke(this, e);
        }

        /// <summary>
        /// Overlays DPI-scaled padding on the Designer's design-time pixels.
        /// </summary>
        /// <remarks>
        /// Invoked by TemplateUserControl from OnHandleCreated and OnDpiChangedAfterParent — never
        /// from the ctor, where DpiScalingHelper reports a scale of 1.0 because the handle does not
        /// exist yet and nothing would actually scale.
        /// <para>
        /// Only the docked panels' padding is scaled. Their children are all Dock=Top/Fill, so the
        /// layout reflows from the padding alone; pushing size tokens onto individual controls is
        /// what broke uc_ImportStep5_Run, whose Designer positions its row absolutely.
        /// </para>
        /// </remarks>
        protected override void ApplyDpiScaledLayout()
        {
            _rootPanel.Padding = BeepLayoutMetrics.DialogPadding.ScalePadding(this);
            _contentHost.Padding = BeepLayoutMetrics.ContainerPadding.ScalePadding(this);
        }

    }
}
