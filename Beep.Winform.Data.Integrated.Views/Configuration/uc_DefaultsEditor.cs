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
    [AddinAttribute(Caption = "Defaults Editor", Name = "uc_DefaultsEditor",
        misc = "Config", menu = "Configuration", addinType = AddinType.Control,
        displayType = DisplayType.InControl, ObjectType = "Beep")]
    [AddinVisSchema(BranchID = 13, RootNodeName = "Configuration", Order = 13, ID = 13,
        BranchText = "Defaults Editor", BranchType = EnumPointType.Function,
        IconImageName = "defaults.svg", BranchClass = "ADDIN",
        BranchDescription = "Edit field-level default-value rules with dry-run preview.")]

    public partial class uc_DefaultsEditor : TemplateUserControl, IAddinVisSchema
    {
        public event EventHandler<WizardCompletedEventArgs>? Completed;

        public uc_DefaultsEditor() : this(null) { }
        public uc_DefaultsEditor(IServiceProvider services) : base(services)
        {
            InitializeComponent();
            Details.AddinName = "Defaults Editor";
            WireEvents();
            ApplyDpiScaledLayout();
        }

        #region "IAddinVisSchema"
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string RootNodeName { get; set; } = "Configuration";
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string CatgoryName { get; set; } = string.Empty;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int Order { get; set; } = 13;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int ID { get; set; } = 13;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string BranchText { get; set; } = "Defaults Editor";
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int Level { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public EnumPointType BranchType { get; set; } = EnumPointType.Function;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int BranchID { get; set; } = 13;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string IconImageName { get; set; } = "defaults.svg";
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string BranchStatus { get; set; } = string.Empty;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int ParentBranchID { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string BranchDescription { get; set; } = "Edit field-level default-value rules with dry-run preview.";
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string BranchClass { get; set; } = "ADDIN";
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string AddinName { get; set; } = "uc_DefaultsEditor";
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