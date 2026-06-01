using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using TheTechIdea.Beep.Winform.Controls;
using TheTechIdea.Beep.Winform.Controls.Base;
using TheTechIdea.Beep.Winform.Controls.FontManagement;
using TheTechIdea.Beep.Winform.Controls.Integrated.Forms.Helpers;
using TheTechIdea.Beep.Winform.Controls.Integrated.Forms.Models;

namespace TheTechIdea.Beep.Winform.Controls.Integrated.Forms
{
    [ToolboxItem(true)]
    [ToolboxBitmap(typeof(BeepFormsHeader))]
    [Category("Beep Controls")]
    [DisplayName("Beep Forms Header")]
    [Description("Standalone title and active-context header surface for a BeepForms host.")]
    [Designer("TheTechIdea.Beep.Winform.Controls.Design.Server.Designers.BeepFormsHeaderDesigner, TheTechIdea.Beep.Winform.Controls.Design.Server")]
    public class BeepFormsHeader : BaseControl
    {
        private readonly TableLayoutPanel _table;
        private readonly BeepLabel _titleLabel;
        private readonly BeepLabel _contextLabel;
        private BeepForms? _formsHost;
        private bool _autoBindFormsHost = true;
        private bool _showActiveBlock = true;
        private bool _showStateSummary = true;

        public BeepFormsHeader()
        {
            UseThemeColors = true;
            Padding = new Padding(0);
            Margin = new Padding(0);
            MinimumSize = new Size(0, 52);
            Height = 60;

            _table = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Padding = new Padding(10, 6, 10, 6),
                Margin = new Padding(0)
            };
            _table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            _table.RowStyles.Add(new RowStyle(SizeType.Absolute, 28f));
            _table.RowStyles.Add(new RowStyle(SizeType.Absolute, 18f));

            _titleLabel = new BeepLabel
            {
                Dock = DockStyle.Fill,
                Font = BeepFontManager.TitleFont,
                TextAlign = ContentAlignment.MiddleLeft,
                UseThemeColors = true,
                Margin = new Padding(0)
            };

            _contextLabel = new BeepLabel
            {
                Dock = DockStyle.Fill,
                Font = BeepFontManager.StatusBarFont,
                TextAlign = ContentAlignment.MiddleLeft,
                UseThemeColors = true,
                Margin = new Padding(0)
            };

            _table.Controls.Add(_titleLabel, 0, 0);
            _table.Controls.Add(_contextLabel, 0, 1);

            Controls.Add(_table);
            UpdateFromViewState();
        }

        [Browsable(true)]
        [Category("Behavior")]
        [Description("Optional BeepForms coordinator surfaced by this header.")]
        [DefaultValue(null)]
        public BeepForms? FormsHost
        {
            get => _formsHost;
            set
            {
                if (ReferenceEquals(_formsHost, value))
                {
                    return;
                }

                DetachFormsHost(_formsHost);
                _formsHost = value;
                AttachFormsHost(_formsHost);
                UpdateFromViewState();
            }
        }

        [Browsable(true)]
        [Category("Behavior")]
        [Description("Automatically resolve a nearby BeepForms host when FormsHost is not set explicitly.")]
        [DefaultValue(true)]
        public bool AutoBindFormsHost
        {
            get => _autoBindFormsHost;
            set
            {
                if (_autoBindFormsHost == value)
                {
                    return;
                }

                _autoBindFormsHost = value;
                if (_autoBindFormsHost && _formsHost == null)
                {
                    TryBindFormsHostFromHierarchy();
                }
            }
        }

        [Browsable(true)]
        [Category("Header")]
        [Description("Show the active block name in the header context line.")]
        [DefaultValue(true)]
        public bool ShowActiveBlock
        {
            get => _showActiveBlock;
            set
            {
                if (_showActiveBlock == value)
                {
                    return;
                }

                _showActiveBlock = value;
                UpdateFromViewState();
            }
        }

        [Browsable(true)]
        [Category("Header")]
        [Description("Show query mode and dirty-state summary in the header context line.")]
        [DefaultValue(true)]
        public bool ShowStateSummary
        {
            get => _showStateSummary;
            set
            {
                if (_showStateSummary == value)
                {
                    return;
                }

                _showStateSummary = value;
                UpdateFromViewState();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                DetachFormsHost(_formsHost);
            }

            base.Dispose(disposing);
        }

        protected override void OnCreateControl()
        {
            base.OnCreateControl();
            TryBindFormsHostFromHierarchy();
        }

        protected override void OnParentChanged(EventArgs e)
        {
            base.OnParentChanged(e);
            TryBindFormsHostFromHierarchy();
        }

        private void AttachFormsHost(BeepForms? formsHost)
        {
            if (formsHost == null)
            {
                return;
            }

            formsHost.ActiveBlockChanged += FormsHost_StateChanged;
            formsHost.FormsManagerChanged += FormsHost_StateChanged;
            formsHost.ViewStateChanged += FormsHost_StateChanged;
            formsHost.Disposed += FormsHost_Disposed;
        }

        private void DetachFormsHost(BeepForms? formsHost)
        {
            if (formsHost == null)
            {
                return;
            }

            formsHost.ActiveBlockChanged -= FormsHost_StateChanged;
            formsHost.FormsManagerChanged -= FormsHost_StateChanged;
            formsHost.ViewStateChanged -= FormsHost_StateChanged;
            formsHost.Disposed -= FormsHost_Disposed;
        }

        private void FormsHost_StateChanged(object? sender, EventArgs e)
        {
            UpdateFromViewState();
        }

        private void FormsHost_Disposed(object? sender, EventArgs e)
        {
            FormsHost = null;
            TryBindFormsHostFromHierarchy();
        }

        private void TryBindFormsHostFromHierarchy()
        {
            if (!AutoBindFormsHost || _formsHost != null || Parent == null)
            {
                return;
            }

            BeepForms? resolvedHost = BeepFormsHostResolver.Find(this);
            if (resolvedHost != null)
            {
                FormsHost = resolvedHost;
            }
        }

        private void UpdateFromViewState()
        {
            _titleLabel.Text = BeepFormsDisplayTextResolver.ResolveTitle(_formsHost);

            string contextText = BeepFormsDisplayTextResolver.ResolveContext(_formsHost, ShowActiveBlock, ShowStateSummary);
            _contextLabel.Text = contextText;
            _contextLabel.ForeColor = ResolveContextColor(_formsHost?.ViewState);
            _contextLabel.Visible = !string.IsNullOrWhiteSpace(contextText);

            _table.RowStyles[1].Height = _contextLabel.Visible ? 18f : 0f;
            Height = _contextLabel.Visible ? 60 : 42;
        }

        private static Color ResolveContextColor(BeepFormsViewState? viewState)
        {
            if (viewState == null)
            {
                return Color.DimGray;
            }

            if (viewState.IsDirty)
            {
                return Color.DarkOrange;
            }

            if (viewState.IsQueryMode)
            {
                return Color.SteelBlue;
            }

            if (!string.IsNullOrWhiteSpace(viewState.ActiveBlockName))
            {
                return Color.ForestGreen;
            }

            return Color.DimGray;
        }
    }
}