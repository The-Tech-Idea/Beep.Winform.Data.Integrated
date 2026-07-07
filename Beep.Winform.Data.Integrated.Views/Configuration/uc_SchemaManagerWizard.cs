using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using TheTechIdea.Beep.Addin;
using TheTechIdea.Beep.Vis;
using TheTechIdea.Beep.Vis.Modules;
using TheTechIdea.Beep.Winform.Controls;
using TheTechIdea.Beep.Winform.Controls.Layouts.Helpers;
using TheTechIdea.Beep.Winform.Default.Views.Template;

namespace TheTechIdea.Beep.Winform.Default.Views.Configuration
{
    [AddinAttribute(Caption = "Schema Manager", Name = "uc_SchemaManagerWizard",
        misc = "Config", menu = "Configuration", addinType = AddinType.Control,
        displayType = DisplayType.InControl, ObjectType = "Beep")]
    [AddinVisSchema(BranchID = 10, RootNodeName = "Configuration", Order = 10, ID = 10,
        BranchText = "Schema Manager", BranchType = EnumPointType.Function,
        IconImageName = "schema.svg", BranchClass = "ADDIN",
        BranchDescription = "Plan, dry-run, and apply schema migrations.")]

    public partial class uc_SchemaManagerWizard : TemplateUserControl, IAddinVisSchema
    {
        public event EventHandler<WizardCompletedEventArgs>? Completed;

        public uc_SchemaManagerWizard() : this(null) { }

        public uc_SchemaManagerWizard(IServiceProvider services) : base(services)
        {
            InitializeComponent();
            Details.AddinName = "Schema Manager";
            WireEvents();
            ApplyDpiScaledLayout();
        }

        #region "IAddinVisSchema"
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string RootNodeName { get; set; } = "Configuration";
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string CatgoryName { get; set; } = string.Empty;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int Order { get; set; } = 10;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int ID { get; set; } = 10;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string BranchText { get; set; } = "Schema Manager";
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int Level { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public EnumPointType BranchType { get; set; } = EnumPointType.Function;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int BranchID { get; set; } = 10;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string IconImageName { get; set; } = "schema.svg";
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string BranchStatus { get; set; } = string.Empty;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int ParentBranchID { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string BranchDescription { get; set; } = "Plan, dry-run, and apply schema migrations.";
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string BranchClass { get; set; } = "ADDIN";
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string AddinName { get; set; } = "uc_SchemaManagerWizard";
        #endregion

        private void WireEvents()
        {
            _btnCancel.Click += (_, _) => Completed?.Invoke(this, new WizardCompletedEventArgs { Cancelled = true });
            _btnNext.Click += (_, _) => Completed?.Invoke(this, new WizardCompletedEventArgs { Succeeded = true });
        }

        private void ApplyDpiScaledLayout()
        {
            Size = BeepLayoutMetrics.DialogLarge.ScaleSize(this);
            _rootPanel.Padding = BeepLayoutMetrics.DialogPadding.ScalePadding(this);
            _headerPanel.Size = new System.Drawing.Size(
                BeepLayoutMetrics.DialogLarge.Width.ScaleValue(this),
                _headerPanel.Size.Height);
            _contentHost.Padding = BeepLayoutMetrics.ContainerPadding.ScalePadding(this);
            _actionsPanel.Padding = BeepLayoutMetrics.ButtonStripPd.ScalePadding(this);
            int btnH = BeepLayoutMetrics.ButtonStandard.Height.ScaleValue(this);
            int btnLargeH = BeepLayoutMetrics.ButtonLarge.Height.ScaleValue(this);
            _btnCancel.Height = System.Math.Max(_btnCancel.Height, btnH);
            _btnBack.Height   = System.Math.Max(_btnBack.Height, btnH);
            _btnNext.Height   = System.Math.Max(_btnNext.Height, btnLargeH);
        }

        public sealed class WizardCompletedEventArgs : EventArgs
        {
            public bool Succeeded { get; init; }
            public bool Cancelled { get; init; }
            public string Summary { get; init; } = string.Empty;
        }
    }
}