using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using TheTechIdea.Beep.Addin;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.Vis;
using TheTechIdea.Beep.Vis.Modules;

using TheTechIdea.Beep.Winform.Default.Views.Template;
using TheTechIdea.Beep.Winform.Controls.Layouts.Helpers;
using TheTechIdea.Beep.Winform.Controls.Models;

namespace TheTechIdea.Beep.Winform.Default.Views.Configuration
{
    /// <summary>
    /// Maps an <see cref="IBranch"/> action to a target branch via the canonical
    /// <c>TreeEditor.Treebranchhandler.SendActionFromBranchToBranch</c> dispatcher.
    ///
    /// <para>Usage: the grid lists every registered <see cref="IBranch"/>. The user picks a
    /// source branch row + an action verb (e.g. <c>COPYENTITY</c>, <c>CREATEVIEWBASEDONTABLE</c>,
    /// <c>RENAMEENTITY</c>) and a target branch row; on confirmation the wizard dispatches the
    /// action through <see cref="ITreeBranchHandler.SendActionFromBranchToBranch"/>, which
    /// routes it to <see cref="IBranch.ExecuteBranchAction"/> on the target branch.</para>
    ///
    /// <para>This is the WinForms mirror of the Blazor <c>Beep.Razor.Components</c>
    /// branch-event mapping surface and the same pattern used by
    /// <c>DatabaseEntitesNode.CopyEntity</c>.</para>
    /// </summary>
    [AddinAttribute(Caption = "Function to Function Mapping", Name = "uc_FunctiontoFunctionMapping", misc = "Config", menu = "Configuration", addinType = AddinType.Control, displayType = DisplayType.InControl, ObjectType = "Beep")]
    [AddinVisSchema(BranchID = 1, RootNodeName = "Configuration", Order = 1, ID = 1, BranchText = "Function to Function Mapping", BranchType = EnumPointType.Function, IconImageName = "functiontofunctionmapping.svg", BranchClass = "ADDIN", BranchDescription = "Map branch actions across the application tree")]

    public partial class uc_FunctiontoFunctionMapping : TemplateUserControl, IAddinVisSchema
    {
        // Skill § onConfigure handler accumulation: re-entrant -= before += so multiple
        // Configure calls never stack delegates on the same event.
        private ITree? _tree;

        /// <summary>
        /// Designer/parameterless ctor. Must not chain to the IServiceProvider overload with null —
        /// that resolves services off a null provider and throws. For the designer only; the runtime
        /// must construct through the IServiceProvider overload.
        /// </summary>
        public uc_FunctiontoFunctionMapping() => InitializeControl();

        public uc_FunctiontoFunctionMapping(IServiceProvider services) : base(services) => InitializeControl();

        private void InitializeControl()
        {
            InitializeComponent();
            Details.AddinName = "Function to Function Mapping";
        }

        /// <summary>
        /// Skill § "Sizing tokens": apply DPI-scaled <see cref="BeepLayoutMetrics"/> values to
        /// chrome that the Designer serialized as static pixels. The Designer is the source
        /// of truth for layout; this method overlays DPI-scaled dimensions on top.
        /// </summary>
        protected override void ApplyDpiScaledLayout()
        {
            Size = BeepLayoutMetrics.DialogLarge.ScaleSize(this);
        }

        #region "IAddinVisSchema"
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string RootNodeName { get; set; } = "Configuration";
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string CatgoryName { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int Order { get; set; } = 1;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int ID { get; set; } = 1;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string BranchText { get; set; } = "Function to Function Mapping";
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int Level { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public EnumPointType BranchType { get; set; } = EnumPointType.Function;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int BranchID { get; set; } = 1;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string IconImageName { get; set; } = "functiontofunctionmapping.svg";
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string BranchStatus { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int ParentBranchID { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string BranchDescription { get; set; } = "Map branch actions across the application tree";
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string BranchClass { get; set; } = "ADDIN";
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string AddinName { get; set; }
        #endregion "IAddinVisSchema"

        /// <summary>
        /// Verifies the engine tree is reachable and populates the grid with every
        /// registered <see cref="IBranch"/>. No-op if the tree or its branch list is empty.
        /// </summary>
        public override void Configure(Dictionary<string, object> settings)
        {
            base.Configure(settings);

            // Resolve ITree from the IAppManager.Tree handle (established pattern:
            // see uc_CreateLocalDB.Configure for the same cast).
            _tree = appManager?.Tree as ITree;

            // Skill § re-entrant Configure: -= before +=.
            beepSimpleGrid1.CellValueChanged -= BeepSimpleGrid1_CellValueChanged;
            beepSimpleGrid1.CellValueChanged += BeepSimpleGrid1_CellValueChanged;

            if (_tree?.Branches == null || _tree.Branches.Count == 0) return;

            // Project IBranch into a flat row shape the grid can bind to. The grid itself
            // is read-only; mutating any cell updates the in-place BranchMappingViewModel.
            beepSimpleGrid1.DataSource = _tree.Branches
                .Where(b => b != null)
                .Select(b => new BranchMappingRow
                {
                    BranchID = b.ID,
                    BranchName = b.BranchText,
                    BranchClass = b.BranchClass,
                    BranchType = b.BranchType.ToString(),
                    DataSourceName = b.DataSourceName,
                    AvailableActions = string.Join(", ",
                        b.BranchActions ?? new List<string>())
                })
                .ToList();
        }

        /// <summary>
        /// Forwards every cell change through the canonical branch-to-branch dispatcher so
        /// downstream branches pick up the routing decision via
        /// <see cref="IBranch.ExecuteBranchAction"/>.
        /// </summary>
        private void BeepSimpleGrid1_CellValueChanged(object? sender, BeepCellEventArgs e)
        {
            if (_tree?.Branches == null || e?.Cell == null) return;
            if (e.Cell.RowIndex < 0 || e.Cell.RowIndex >= _tree.Branches.Count) return;
            if (!string.Equals(e.Cell.ColumnName, nameof(BranchMappingRow.SelectedAction), StringComparison.Ordinal)) return;

            var row = e.Cell.RowIndex;
            var source = _tree.Branches[row];
            var verb = e.Cell.CellValue?.ToString();
            if (string.IsNullOrWhiteSpace(verb)) return;

            // Find a target branch that listens for this action. BranchActions is the
            // list of verbs the branch is registered to receive.
            var target = _tree.Branches.FirstOrDefault(b =>
                b != null && b != source &&
                b.BranchActions != null &&
                b.BranchActions.Contains(verb, StringComparer.OrdinalIgnoreCase));

            if (target == null) return;

            _tree.Treebranchhandler?.SendActionFromBranchToBranch(target, source, verb);
        }

        /// <summary>Row shape projected into the designer grid.</summary>
        public sealed class BranchMappingRow
        {
            public int BranchID { get; set; }
            public string BranchName { get; set; } = string.Empty;
            public string BranchClass { get; set; } = string.Empty;
            public string BranchType { get; set; } = string.Empty;
            public string DataSourceName { get; set; } = string.Empty;
            public string AvailableActions { get; set; } = string.Empty;
            public string SelectedAction { get; set; } = string.Empty;
        }
    }
}