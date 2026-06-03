using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using TheTechIdea.Beep.SetUp.Seeding;
using TheTechIdea.Beep.SetUp.Steps;
using TheTechIdea.Beep.Winform.Controls;
using TheTechIdea.Beep.Winform.Controls.Wizards;

namespace TheTechIdea.Beep.Winform.Default.Views.Setup
{
    public partial class uc_SetupSeedingStep : UserControl, IWizardStepContent
    {
        private ISeederRegistry? _registry;
        private IReadOnlyList<Assembly>? _extraAssemblies;
        private WizardContext? _wizardContext;
        private bool _isComplete;

        public event EventHandler<SeedingSummaryEventArgs>? SummaryChanged;

        public uc_SetupSeedingStep()
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
                ValidationStateChanged?.Invoke(this, new StepValidationEventArgs(_isComplete, _isComplete ? string.Empty : "Seeder registry not configured"));
            }
        }

        public event EventHandler<StepValidationEventArgs>? ValidationStateChanged;

        public string NextButtonText { get; set; } = string.Empty;

        public void InitializeStep(ISeederRegistry? registry, IReadOnlyList<Assembly>? extraAssemblies = null)
        {
            _registry = registry;
            _extraAssemblies = extraAssemblies;
            RefreshSummary();
        }

        public ISeederRegistry? Registry
        {
            get => _registry;
            set { _registry = value; RefreshSummary(); }
        }

        public IReadOnlyList<Assembly>? ExtraAssemblies
        {
            get => _extraAssemblies;
            set { _extraAssemblies = value; RefreshSummary(); }
        }

        public bool IsReadyForSetup()
        {
            return _registry != null && _registry.All.Count > 0;
        }

        public SeedingStepOptions BuildStepOptions()
        {
            if (_registry == null)
                throw new InvalidOperationException("Seeder registry is not configured. Cannot build SeedingStepOptions.");

            return new SeedingStepOptions
            {
                Registry = _registry,
                SeederFilter = null
            };
        }

        public string GetStepSummary()
        {
            if (_registry == null)
                return "Seeding: No seeder registry configured (manual mode).";

            var count = _registry.All.Count;
            if (count == 0)
                return "Seeding: No seeders registered.";

            return $"Seeding: {count} seeder(s) registered and ready to run.";
        }

        private void RefreshSummary()
        {
            int total = _registry?.All.Count ?? 0;
            int ordered = 0;
            string message;

            if (_registry == null)
            {
                _lblTask1.Text = "- No seeder registry configured for this step.";
                _lblTask2.Text = "- Register an ISeederRegistry via InitializeStep before running setup.";
                _lblTask3.Text = "- Without a registry, seeding is treated as a manual review step.";
                message = "Seeder registry not provided.";
            }
            else
            {
                try
                {
                    var orderedSeeders = _registry.GetOrderedSeeders();
                    ordered = orderedSeeders.Count;

                    if (total == 0)
                    {
                        _lblTask1.Text = "- Seeder registry is empty.";
                        _lblTask2.Text = "- No seeders to execute during setup.";
                        _lblTask3.Text = "- Step will be skipped (CanSkip returns true).";
                        message = "No seeders registered.";
                    }
                    else
                    {
                        _lblTask1.Text = $"- {total} seeder(s) registered ({ordered} resolved in dependency order).";
                        _lblTask2.Text = "- Each seeder is idempotent (IsAlreadySeeded is checked before running).";
                        _lblTask3.Text = "- Partial progress is persisted to SetupState.CompletedSeederIds.";
                        message = $"{total} seeder(s) ready.";
                    }
                }
                catch (Exception ex)
                {
                    _lblTask1.Text = "- Failed to resolve seeder order (circular or missing dependency).";
                    _lblTask2.Text = "- Inspect the ISeeder.DependsOn declarations for each seeder.";
                    _lblTask3.Text = $"- Last error: {ex.Message}";
                    message = $"Seeder ordering failed: {ex.Message}";
                }
            }

            SummaryChanged?.Invoke(this, new SeedingSummaryEventArgs(total, ordered, message));
        }

        public void ApplyTheme(string theme)
        {
            ApplyThemeToControl(_rootPanel, theme);
            ApplyThemeToControl(_lblTitle, theme);
            ApplyThemeToControl(_lblDescription, theme);
            ApplyThemeToControl(_lblTask1, theme);
            ApplyThemeToControl(_lblTask2, theme);
            ApplyThemeToControl(_lblTask3, theme);
        }

        private static void ApplyThemeToControl(Control control, string theme)
        {
            if (control is IBeepUIComponent beepComponent)
                beepComponent.Theme = theme;
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

        void IWizardStepContent.OnStepEnter(WizardContext context)
        {
            _wizardContext = context;

            // Try to recover registry from context if not already set
            if (_registry == null)
            {
                _registry = context.GetValue<ISeederRegistry>("seederRegistry");
            }

            RefreshSummary();
            IsComplete = _registry != null;
        }

        void IWizardStepContent.OnStepLeave(WizardContext context)
        {
            if (_registry != null)
            {
                context.SetValue("seederCount", _registry.All.Count);
            }
        }

        WizardValidationResult IWizardStepContent.Validate()
        {
            if (_registry == null)
                return WizardValidationResult.Error("Seeder registry is not configured. Configure seeders or skip this step.");

            try
            {
                var ordered = _registry.GetOrderedSeeders();
                if (ordered.Count == 0)
                    return WizardValidationResult.Success();

                return WizardValidationResult.Success();
            }
            catch (Exception ex)
            {
                return WizardValidationResult.Error($"Seeder dependency ordering failed: {ex.Message}");
            }
        }

        Task<WizardValidationResult> IWizardStepContent.ValidateAsync()
        {
            return Task.FromResult(((IWizardStepContent)this).Validate());
        }
    }
}
