using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using TheTechIdea.Beep.SetUp;
using TheTechIdea.Beep.Winform.Controls.Layouts.Helpers;
using TheTechIdea.Beep.SetUp.Seeding;
using TheTechIdea.Beep.SetUp.Steps;
using TheTechIdea.Beep.Winform.Controls;
using TheTechIdea.Beep.Winform.Default.Views.Template;

namespace TheTechIdea.Beep.Winform.Default.Views.Setup
{
    // Full refactor: UI step wraps a typed SeedingStepOptions directly.
    // The canonical SeedingStep reads options.Registry during Execute.
    public partial class uc_SetupSeedingStep : TemplateUserControl
    {
        private ISeederRegistry? _registry;
        private SeedingStepOptions? _options;
        private SetupContext? _context;

        public event EventHandler<SeedingSummaryEventArgs>? SummaryChanged;

        public uc_SetupSeedingStep()
        {
            InitializeComponent();
        }

        public ISeederRegistry? Registry
        {
            get => _registry;
            set { _registry = value; if (_options != null) _options.Registry = value; RefreshSummary(); }
        }

        public bool IsReadyForSetup()
        {
            return _registry != null && _registry.All.Count > 0;
        }

        /// <summary>
        /// Bind the seeding-summary UI to the typed <see cref="SeedingStepOptions"/>.
        /// </summary>
        public void InitializeStep(SeedingStepOptions options, SetupContext context)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _context = context ?? throw new ArgumentNullException(nameof(context));

            _registry = options.Registry;
            RefreshSummary();
        }

        private void RefreshSummary()
        {
            if (_registry != null && _registry.All.Count > 0)
            {
                _lblTask1.Text = "- Discover seeders from registry.";
                _lblTask2.Text = "- Resolve dependencies and order seeders.";
                _lblTask3.Text = $"- Execute {_registry.All.Count} seeder(s) and verify each result.";

                int total = _registry.All.Count;
                int ordered = _registry.All.Count;   // all registered seeders are ready
                string message = total > 0
                    ? $"{ordered}/{total} seeders ready to run."
                    : "No seeders available.";
                SummaryChanged?.Invoke(this, new SeedingSummaryEventArgs(total, ordered, message));
            }
            else
            {
                _lblTask1.Text = "- No seeder registry configured (waiting for connection step).";
                _lblTask2.Text = "- Resolve dependencies and order seeders.";
                _lblTask3.Text = "- Execute seeders and verify each result.";
                SummaryChanged?.Invoke(this, new SeedingSummaryEventArgs(0, 0, "No seeder registry configured."));
            }
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
        }

    }

    public sealed class SeedingSummaryEventArgs : EventArgs
    {
        public SeedingSummaryEventArgs(int totalSeeders, int orderedSeeders, string message)
        {
            TotalSeeders = totalSeeders;
            OrderedSeeders = orderedSeeders;
            Message = message;
        }

        public int TotalSeeders { get; }
        public int OrderedSeeders { get; }
        public string Message { get; }


    }
}
