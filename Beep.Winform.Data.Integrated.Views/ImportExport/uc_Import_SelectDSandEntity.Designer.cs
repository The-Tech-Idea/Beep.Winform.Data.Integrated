using TheTechIdea.Beep.Icons;
using TheTechIdea.Beep.Vis.Modules;
using TheTechIdea.Beep.Winform.Controls;
using TheTechIdea.Beep.Winform.Controls.ComboBoxes;
using TheTechIdea.Beep.Winform.Controls.Models;
using TheTechIdea.Beep.Winform.Controls.CheckBoxes;

namespace TheTechIdea.Beep.Winform.Default.Views.ImportExport
{
    partial class uc_Import_SelectDSandEntity
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(uc_Import_SelectDSandEntity));
            SourcebeepComboBox = new TheTechIdea.Beep.Winform.Controls.BeepComboBox();
            beepLabel1 = new TheTechIdea.Beep.Winform.Controls.BeepLabel();
            beepComboBox1 = new TheTechIdea.Beep.Winform.Controls.BeepComboBox();
            // New purpose-panel controls
            pnlPurpose      = new System.Windows.Forms.Panel();
            lblPurpose      = new TheTechIdea.Beep.Winform.Controls.BeepLabel();
            cmbPurpose      = new TheTechIdea.Beep.Winform.Controls.BeepComboBox();
            lblMatchBy      = new TheTechIdea.Beep.Winform.Controls.BeepLabel();
            cmbMatchBy      = new TheTechIdea.Beep.Winform.Controls.BeepComboBox();
            chkUpdateEmpty  = new TheTechIdea.Beep.Winform.Controls.CheckBoxes.BeepCheckBoxBool();
            lblRowCount     = new TheTechIdea.Beep.Winform.Controls.BeepLabel();
            btnRefreshCount = new TheTechIdea.Beep.Winform.Controls.BeepButton();
            beepLabel2 = new TheTechIdea.Beep.Winform.Controls.BeepLabel();
            AddSourcebeepButton = new TheTechIdea.Beep.Winform.Controls.BeepButton();
            beepCheckBoxBool1 = new TheTechIdea.Beep.Winform.Controls.CheckBoxes.BeepCheckBoxBool();
            _sourceEntityLabel      = new TheTechIdea.Beep.Winform.Controls.BeepLabel();
            _sourceEntityCombo      = new TheTechIdea.Beep.Winform.Controls.BeepComboBox();
            _destinationEntityLabel = new TheTechIdea.Beep.Winform.Controls.BeepLabel();
            _destinationEntityCombo = new TheTechIdea.Beep.Winform.Controls.BeepComboBox();
            _statusLabel            = new TheTechIdea.Beep.Winform.Controls.BeepLabel();
            SuspendLayout();
            // 
            // SourcebeepComboBox
            // 
            SourcebeepComboBox.Anchor = AnchorStyles.None;
            SourcebeepComboBox.AnimationDuration = 500;
            SourcebeepComboBox.AnimationType = Vis.Modules.DisplayAnimationType.None;
            SourcebeepComboBox.ApplyThemeToChilds = false;
            SourcebeepComboBox.BackColor = Color.FromArgb(255, 255, 255);
            SourcebeepComboBox.BadgeBackColor = Color.Red;
            SourcebeepComboBox.BadgeFont = new Font("Arial", 8F, FontStyle.Bold);
            SourcebeepComboBox.BadgeForeColor = Color.White;
            SourcebeepComboBox.BadgeShape = Vis.Modules.BadgeShape.Circle;
            SourcebeepComboBox.BadgeText = "";
            SourcebeepComboBox.BlockID = null;
            SourcebeepComboBox.BorderColor = Color.FromArgb(173, 181, 189);
            SourcebeepComboBox.BorderDashStyle = System.Drawing.Drawing2D.DashStyle.Solid;
            SourcebeepComboBox.BorderRadius = 3;
            SourcebeepComboBox.BorderStyle = BorderStyle.FixedSingle;
            SourcebeepComboBox.BorderThickness = 1;
            SourcebeepComboBox.BottomoffsetForDrawingRect = 0;
            SourcebeepComboBox.BoundProperty = null;
            SourcebeepComboBox.CanBeFocused = true;
            SourcebeepComboBox.CanBeHovered = false;
            SourcebeepComboBox.CanBePressed = true;
            SourcebeepComboBox.Category = Utilities.DbFieldCategory.Numeric;
            SourcebeepComboBox.ComponentName = "SourcebeepComboBox";
            SourcebeepComboBox.DataSourceProperty = null;
            SourcebeepComboBox.DisabledBackColor = Color.White;
            SourcebeepComboBox.DisabledForeColor = Color.Black;
            SourcebeepComboBox.DrawingRect = new Rectangle(1, 1, 192, 25);
            SourcebeepComboBox.Easing = Vis.Modules.EasingType.Linear;
            SourcebeepComboBox.FieldID = null;
            SourcebeepComboBox.FocusBackColor = Color.White;
            SourcebeepComboBox.FocusBorderColor = Color.Gray;
            SourcebeepComboBox.FocusForeColor = Color.Black;
            SourcebeepComboBox.FocusIndicatorColor = Color.Blue;
            SourcebeepComboBox.Font = new Font("Arial", 11F);
            SourcebeepComboBox.Form = null;
            SourcebeepComboBox.GradientDirection = System.Drawing.Drawing2D.LinearGradientMode.Horizontal;
            SourcebeepComboBox.GradientEndColor = Color.FromArgb(255, 255, 255);
            SourcebeepComboBox.GradientStartColor = Color.FromArgb(245, 245, 245);
            SourcebeepComboBox.GuidID = "fbe53442-f5d5-466d-8750-b29902c7b7c0";
            SourcebeepComboBox.HitAreaEventOn = false;
            SourcebeepComboBox.HitTestControl = null;
            SourcebeepComboBox.HoverBackColor = Color.White;
            SourcebeepComboBox.HoverBorderColor = Color.Gray;
            SourcebeepComboBox.HoveredBackcolor = Color.Wheat;
            SourcebeepComboBox.HoverForeColor = Color.Black;
            SourcebeepComboBox.Id = -1;
            SourcebeepComboBox.InactiveBorderColor = Color.Gray;
     //       SourcebeepComboBox.Info = (SimpleItem)resources.GetObject("SourcebeepComboBox.Info");
            SourcebeepComboBox.IsAcceptButton = false;
            SourcebeepComboBox.IsBorderAffectedByTheme = true;
            SourcebeepComboBox.IsCancelButton = false;
            SourcebeepComboBox.IsChild = false;
            SourcebeepComboBox.IsCustomeBorder = false;
            SourcebeepComboBox.IsDefault = false;
            SourcebeepComboBox.IsDeleted = false;
            SourcebeepComboBox.IsDirty = false;
            SourcebeepComboBox.IsEditable = false;
            SourcebeepComboBox.IsFocused = false;
            SourcebeepComboBox.IsFrameless = false;
            SourcebeepComboBox.IsHovered = false;
            SourcebeepComboBox.IsNew = false;
         
            SourcebeepComboBox.IsPressed = false;
            SourcebeepComboBox.IsReadOnly = false;
            SourcebeepComboBox.IsRequired = false;
            SourcebeepComboBox.IsRounded = true;
            SourcebeepComboBox.IsRoundedAffectedByTheme = true;
            SourcebeepComboBox.IsSelected = false;
            SourcebeepComboBox.IsSelectedOptionOn = true;
            SourcebeepComboBox.IsShadowAffectedByTheme = true;
            SourcebeepComboBox.IsVisible = false;
          
            SourcebeepComboBox.LeftoffsetForDrawingRect = 0;
            SourcebeepComboBox.LinkedProperty = null;
            SourcebeepComboBox.Location = new Point(232, 269);
            SourcebeepComboBox.Name = "SourcebeepComboBox";
            SourcebeepComboBox.OverrideFontSize = Vis.Modules.TypeStyleFontSize.None;
            SourcebeepComboBox.ParentBackColor = Color.Empty;
            SourcebeepComboBox.ParentControl = null;
            SourcebeepComboBox.PressedBackColor = Color.White;
            SourcebeepComboBox.PressedBorderColor = Color.Gray;
            SourcebeepComboBox.PressedForeColor = Color.Gray;
            SourcebeepComboBox.RightoffsetForDrawingRect = 0;
            SourcebeepComboBox.SavedGuidID = null;
            SourcebeepComboBox.SavedID = null;
            SourcebeepComboBox.SelectedBackColor = Color.White;
            SourcebeepComboBox.SelectedForeColor = Color.Black;
            SourcebeepComboBox.SelectedIndex = -1;
            SourcebeepComboBox.SelectedItem = null;
            SourcebeepComboBox.SelectedValue = null;
            SourcebeepComboBox.ShadowColor = Color.FromArgb(173, 181, 189);
            SourcebeepComboBox.ShadowOffset = 0;
            SourcebeepComboBox.ShadowOpacity = 0.5F;
            SourcebeepComboBox.ShowAllBorders = true;
            SourcebeepComboBox.ShowBottomBorder = true;
            SourcebeepComboBox.ShowFocusIndicator = false;
            SourcebeepComboBox.ShowLeftBorder = true;
            SourcebeepComboBox.ShowRightBorder = true;
            SourcebeepComboBox.ShowShadow = false;
            SourcebeepComboBox.ShowTopBorder = true;
            SourcebeepComboBox.Size = new Size(194, 27);
            SourcebeepComboBox.SlideFrom = Vis.Modules.SlideDirection.Left;
            SourcebeepComboBox.StaticNotMoving = false;
            SourcebeepComboBox.TabIndex = 0;
            SourcebeepComboBox.TempBackColor = Color.Empty;
            SourcebeepComboBox.Text = "beepComboBox1";
            SourcebeepComboBox.TextFont = new Font("Arial", 11F);
            SourcebeepComboBox.Theme = "DefaultType";
            SourcebeepComboBox.ToolTipText = "";
            SourcebeepComboBox.TopoffsetForDrawingRect = 0;
            SourcebeepComboBox.UseGradientBackground = false;
            SourcebeepComboBox.UseThemeFont = true;
            // 
            // beepLabel1
            // 
            beepLabel1.Anchor = AnchorStyles.None;
            beepLabel1.AnimationDuration = 500;
            beepLabel1.AnimationType = Vis.Modules.DisplayAnimationType.None;
            beepLabel1.ApplyThemeOnImage = false;
            beepLabel1.ApplyThemeToChilds = false;
            beepLabel1.BackColor = Color.FromArgb(255, 255, 255);
            beepLabel1.BadgeBackColor = Color.Red;
            beepLabel1.BadgeFont = new Font("Arial", 8F, FontStyle.Bold);
            beepLabel1.BadgeForeColor = Color.White;
            beepLabel1.BadgeShape = Vis.Modules.BadgeShape.Circle;
            beepLabel1.BadgeText = "";
            beepLabel1.BlockID = null;
            beepLabel1.BorderColor = Color.Black;
            beepLabel1.BorderDashStyle = System.Drawing.Drawing2D.DashStyle.Solid;
            beepLabel1.BorderRadius = 3;
            beepLabel1.BorderStyle = BorderStyle.FixedSingle;
            beepLabel1.BorderThickness = 1;
            beepLabel1.BottomoffsetForDrawingRect = 0;
            beepLabel1.BoundProperty = "Text";
            beepLabel1.CanBeFocused = true;
            beepLabel1.CanBeHovered = false;
            beepLabel1.CanBePressed = true;
            beepLabel1.Category = Utilities.DbFieldCategory.String;
            beepLabel1.ComponentName = "beepLabel1";
            beepLabel1.DataSourceProperty = null;
            beepLabel1.DisabledBackColor = Color.White;
            beepLabel1.DisabledForeColor = Color.Black;
            beepLabel1.DrawingRect = new Rectangle(1, 1, 192, 21);
            beepLabel1.Easing = Vis.Modules.EasingType.Linear;
            beepLabel1.FieldID = null;
            beepLabel1.FocusBackColor = Color.White;
            beepLabel1.FocusBorderColor = Color.Gray;
            beepLabel1.FocusForeColor = Color.Black;
            beepLabel1.FocusIndicatorColor = Color.Blue;
            beepLabel1.Font = new Font("Arial", 12F);
            beepLabel1.ForeColor = Color.FromArgb(33, 37, 41);
            beepLabel1.Form = null;
            beepLabel1.GradientDirection = System.Drawing.Drawing2D.LinearGradientMode.Horizontal;
            beepLabel1.GradientEndColor = Color.Gray;
            beepLabel1.GradientStartColor = Color.Gray;
            beepLabel1.GuidID = "b1389fbc-96a2-4419-a54b-6fd3fa51799f";
            beepLabel1.HideText = false;
            beepLabel1.HitAreaEventOn = false;
            beepLabel1.HitTestControl = null;
            beepLabel1.HoverBackColor = Color.FromArgb(30, 140, 235);
            beepLabel1.HoverBorderColor = Color.Gray;
            beepLabel1.HoveredBackcolor = Color.Wheat;
            beepLabel1.HoverForeColor = Color.FromArgb(255, 255, 255);
            beepLabel1.Id = -1;
            beepLabel1.ImageAlign = ContentAlignment.MiddleLeft;
            beepLabel1.ImagePath = null;
            beepLabel1.InactiveBorderColor = Color.Gray;
   //         beepLabel1.Info = (SimpleItem)resources.GetObject("beepLabel1.Info");
            beepLabel1.IsAcceptButton = false;
            beepLabel1.IsBorderAffectedByTheme = true;
            beepLabel1.IsCancelButton = false;
            beepLabel1.IsChild = false;
            beepLabel1.IsCustomeBorder = false;
            beepLabel1.IsDefault = false;
            beepLabel1.IsDeleted = false;
            beepLabel1.IsDirty = false;
            beepLabel1.IsEditable = false;
            beepLabel1.IsFocused = false;
            beepLabel1.IsFrameless = false;
            beepLabel1.IsHovered = false;
            beepLabel1.IsNew = false;
            beepLabel1.IsPressed = false;
            beepLabel1.IsReadOnly = false;
            beepLabel1.IsRequired = false;
            beepLabel1.IsRounded = true;
            beepLabel1.IsRoundedAffectedByTheme = true;
            beepLabel1.IsSelected = false;
            beepLabel1.IsSelectedOptionOn = true;
            beepLabel1.IsShadowAffectedByTheme = true;
            beepLabel1.IsVisible = false;
        
            beepLabel1.LabelBackColor = Color.Empty;
            beepLabel1.LeftoffsetForDrawingRect = 0;
            beepLabel1.LinkedProperty = null;
            beepLabel1.Location = new Point(35, 271);
            beepLabel1.Margin = new Padding(0);
            beepLabel1.MaxImageSize = new Size(16, 16);
            beepLabel1.Multiline = false;
            beepLabel1.Name = "beepLabel1";
            beepLabel1.OverrideFontSize = Vis.Modules.TypeStyleFontSize.None;
            beepLabel1.Padding = new Padding(1);
            beepLabel1.ParentBackColor = Color.Empty;
            beepLabel1.ParentControl = null;
            beepLabel1.PressedBackColor = Color.White;
            beepLabel1.PressedBorderColor = Color.Gray;
            beepLabel1.PressedForeColor = Color.Gray;
            beepLabel1.RightoffsetForDrawingRect = 0;
            beepLabel1.SavedGuidID = null;
            beepLabel1.SavedID = null;
            beepLabel1.SelectedBackColor = Color.White;
            beepLabel1.SelectedForeColor = Color.Black;
            beepLabel1.SelectedValue = null;
            beepLabel1.ShadowColor = Color.Black;
            beepLabel1.ShadowOffset = 0;
            beepLabel1.ShadowOpacity = 0.5F;
            beepLabel1.ShowAllBorders = false;
            beepLabel1.ShowBottomBorder = false;
            beepLabel1.ShowFocusIndicator = false;
            beepLabel1.ShowLeftBorder = false;
            beepLabel1.ShowRightBorder = false;
            beepLabel1.ShowShadow = false;
            beepLabel1.ShowTopBorder = false;
            beepLabel1.Size = new Size(194, 23);
            beepLabel1.SlideFrom = Vis.Modules.SlideDirection.Left;
            beepLabel1.StaticNotMoving = false;
            beepLabel1.TabIndex = 1;
            beepLabel1.TempBackColor = Color.Empty;
            beepLabel1.Text = "Source";
            beepLabel1.TextAlign = ContentAlignment.MiddleLeft;
            beepLabel1.TextFont = new Font("Arial", 12F);
            beepLabel1.TextImageRelation = TextImageRelation.ImageBeforeText;
            beepLabel1.Theme = "DefaultType";
            beepLabel1.ToolTipText = "";
            beepLabel1.TopoffsetForDrawingRect = 0;
            beepLabel1.UseGradientBackground = false;
            beepLabel1.UseScaledFont = false;
            beepLabel1.UseThemeFont = true;
            // 
            // beepComboBox1
            // 
            beepComboBox1.Anchor = AnchorStyles.None;
            beepComboBox1.AnimationDuration = 500;
            beepComboBox1.AnimationType = Vis.Modules.DisplayAnimationType.None;
            beepComboBox1.ApplyThemeToChilds = false;
            beepComboBox1.BackColor = Color.FromArgb(255, 255, 255);
            beepComboBox1.BadgeBackColor = Color.Red;
            beepComboBox1.BadgeFont = new Font("Arial", 8F, FontStyle.Bold);
            beepComboBox1.BadgeForeColor = Color.White;
            beepComboBox1.BadgeShape = Vis.Modules.BadgeShape.Circle;
            beepComboBox1.BadgeText = "";
            beepComboBox1.BlockID = null;
            beepComboBox1.BorderColor = Color.FromArgb(173, 181, 189);
            beepComboBox1.BorderDashStyle = System.Drawing.Drawing2D.DashStyle.Solid;
            beepComboBox1.BorderRadius = 3;
            beepComboBox1.BorderStyle = BorderStyle.FixedSingle;
            beepComboBox1.BorderThickness = 1;
            beepComboBox1.BottomoffsetForDrawingRect = 0;
            beepComboBox1.BoundProperty = null;
            beepComboBox1.CanBeFocused = true;
            beepComboBox1.CanBeHovered = false;
            beepComboBox1.CanBePressed = true;
            beepComboBox1.Category = Utilities.DbFieldCategory.Numeric;
            beepComboBox1.ComponentName = "beepComboBox1";
            beepComboBox1.DataSourceProperty = null;
            beepComboBox1.DisabledBackColor = Color.White;
            beepComboBox1.DisabledForeColor = Color.Black;
            beepComboBox1.DrawingRect = new Rectangle(1, 1, 192, 25);
            beepComboBox1.Easing = Vis.Modules.EasingType.Linear;
            beepComboBox1.FieldID = null;
            beepComboBox1.FocusBackColor = Color.White;
            beepComboBox1.FocusBorderColor = Color.Gray;
            beepComboBox1.FocusForeColor = Color.Black;
            beepComboBox1.FocusIndicatorColor = Color.Blue;
            beepComboBox1.Font = new Font("Arial", 11F);
            beepComboBox1.Form = null;
            beepComboBox1.GradientDirection = System.Drawing.Drawing2D.LinearGradientMode.Horizontal;
            beepComboBox1.GradientEndColor = Color.FromArgb(255, 255, 255);
            beepComboBox1.GradientStartColor = Color.FromArgb(245, 245, 245);
            beepComboBox1.GuidID = "35aab860-7903-4cc7-8bb3-462750cfe84e";
            beepComboBox1.HitAreaEventOn = false;
            beepComboBox1.HitTestControl = null;
            beepComboBox1.HoverBackColor = Color.White;
            beepComboBox1.HoverBorderColor = Color.Gray;
            beepComboBox1.HoveredBackcolor = Color.Wheat;
            beepComboBox1.HoverForeColor = Color.Black;
            beepComboBox1.Id = -1;
            beepComboBox1.InactiveBorderColor = Color.Gray;
    //        beepComboBox1.Info = (SimpleItem)resources.GetObject("beepComboBox1.Info");
            beepComboBox1.IsAcceptButton = false;
            beepComboBox1.IsBorderAffectedByTheme = true;
            beepComboBox1.IsCancelButton = false;
            beepComboBox1.IsChild = false;
            beepComboBox1.IsCustomeBorder = false;
            beepComboBox1.IsDefault = false;
            beepComboBox1.IsDeleted = false;
            beepComboBox1.IsDirty = false;
            beepComboBox1.IsEditable = false;
            beepComboBox1.IsFocused = false;
            beepComboBox1.IsFrameless = false;
            beepComboBox1.IsHovered = false;
            beepComboBox1.IsNew = false;
        
            beepComboBox1.IsPressed = false;
            beepComboBox1.IsReadOnly = false;
            beepComboBox1.IsRequired = false;
            beepComboBox1.IsRounded = true;
            beepComboBox1.IsRoundedAffectedByTheme = true;
            beepComboBox1.IsSelected = false;
            beepComboBox1.IsSelectedOptionOn = true;
            beepComboBox1.IsShadowAffectedByTheme = true;
            beepComboBox1.IsVisible = false;
            beepComboBox1.Items = (List<object>)resources.GetObject("beepComboBox1.Items");
            beepComboBox1.LeftoffsetForDrawingRect = 0;
            beepComboBox1.LinkedProperty = null;
            beepComboBox1.Location = new Point(232, 301);
            beepComboBox1.Name = "beepComboBox1";
            beepComboBox1.OverrideFontSize = Vis.Modules.TypeStyleFontSize.None;
            beepComboBox1.ParentBackColor = Color.Empty;
            beepComboBox1.ParentControl = null;
            beepComboBox1.PressedBackColor = Color.White;
            beepComboBox1.PressedBorderColor = Color.Gray;
            beepComboBox1.PressedForeColor = Color.Gray;
            beepComboBox1.RightoffsetForDrawingRect = 0;
            beepComboBox1.SavedGuidID = null;
            beepComboBox1.SavedID = null;
            beepComboBox1.SelectedBackColor = Color.White;
            beepComboBox1.SelectedForeColor = Color.Black;
            beepComboBox1.SelectedIndex = -1;
            beepComboBox1.SelectedItem = null;
            beepComboBox1.SelectedValue = null;
            beepComboBox1.ShadowColor = Color.FromArgb(173, 181, 189);
            beepComboBox1.ShadowOffset = 0;
            beepComboBox1.ShadowOpacity = 0.5F;
            beepComboBox1.ShowAllBorders = true;
            beepComboBox1.ShowBottomBorder = true;
            beepComboBox1.ShowFocusIndicator = false;
            beepComboBox1.ShowLeftBorder = true;
            beepComboBox1.ShowRightBorder = true;
            beepComboBox1.ShowShadow = false;
            beepComboBox1.ShowTopBorder = true;
            beepComboBox1.Size = new Size(194, 27);
            beepComboBox1.SlideFrom = Vis.Modules.SlideDirection.Left;
            beepComboBox1.StaticNotMoving = false;
            beepComboBox1.TabIndex = 2;
            beepComboBox1.TempBackColor = Color.Empty;
            beepComboBox1.Text = "beepComboBox1";
            beepComboBox1.TextFont = new Font("Arial", 11F);
            beepComboBox1.Theme = "DefaultType";
            beepComboBox1.ToolTipText = "";
            beepComboBox1.TopoffsetForDrawingRect = 0;
            beepComboBox1.UseGradientBackground = false;
            beepComboBox1.UseThemeFont = true;
            // 
            // beepLabel2
            // 
            beepLabel2.Anchor = AnchorStyles.None;
            beepLabel2.AnimationDuration = 500;
            beepLabel2.AnimationType = Vis.Modules.DisplayAnimationType.None;
            beepLabel2.ApplyThemeOnImage = false;
            beepLabel2.ApplyThemeToChilds = false;
            beepLabel2.BackColor = Color.FromArgb(255, 255, 255);
            beepLabel2.BadgeBackColor = Color.Red;
            beepLabel2.BadgeFont = new Font("Arial", 8F, FontStyle.Bold);
            beepLabel2.BadgeForeColor = Color.White;
            beepLabel2.BadgeShape = Vis.Modules.BadgeShape.Circle;
            beepLabel2.BadgeText = "";
            beepLabel2.BlockID = null;
            beepLabel2.BorderColor = Color.Black;
            beepLabel2.BorderDashStyle = System.Drawing.Drawing2D.DashStyle.Solid;
            beepLabel2.BorderRadius = 3;
            beepLabel2.BorderStyle = BorderStyle.FixedSingle;
            beepLabel2.BorderThickness = 1;
            beepLabel2.BottomoffsetForDrawingRect = 0;
            beepLabel2.BoundProperty = "Text";
            beepLabel2.CanBeFocused = true;
            beepLabel2.CanBeHovered = false;
            beepLabel2.CanBePressed = true;
            beepLabel2.Category = Utilities.DbFieldCategory.String;
            beepLabel2.ComponentName = "beepLabel1";
            beepLabel2.DataSourceProperty = null;
            beepLabel2.DisabledBackColor = Color.White;
            beepLabel2.DisabledForeColor = Color.Black;
            beepLabel2.DrawingRect = new Rectangle(1, 1, 192, 21);
            beepLabel2.Easing = Vis.Modules.EasingType.Linear;
            beepLabel2.FieldID = null;
            beepLabel2.FocusBackColor = Color.White;
            beepLabel2.FocusBorderColor = Color.Gray;
            beepLabel2.FocusForeColor = Color.Black;
            beepLabel2.FocusIndicatorColor = Color.Blue;
            beepLabel2.Font = new Font("Arial", 12F);
            beepLabel2.ForeColor = Color.FromArgb(33, 37, 41);
            beepLabel2.Form = null;
            beepLabel2.GradientDirection = System.Drawing.Drawing2D.LinearGradientMode.Horizontal;
            beepLabel2.GradientEndColor = Color.Gray;
            beepLabel2.GradientStartColor = Color.Gray;
            beepLabel2.GuidID = "b1389fbc-96a2-4419-a54b-6fd3fa51799f";
            beepLabel2.HideText = false;
            beepLabel2.HitAreaEventOn = false;
            beepLabel2.HitTestControl = null;
            beepLabel2.HoverBackColor = Color.FromArgb(30, 140, 235);
            beepLabel2.HoverBorderColor = Color.Gray;
            beepLabel2.HoveredBackcolor = Color.Wheat;
            beepLabel2.HoverForeColor = Color.FromArgb(255, 255, 255);
            beepLabel2.Id = -1;
            beepLabel2.ImageAlign = ContentAlignment.MiddleLeft;
            beepLabel2.ImagePath = null;
            beepLabel2.InactiveBorderColor = Color.Gray;
  //          beepLabel2.Info = (SimpleItem)resources.GetObject("beepLabel2.Info");
            beepLabel2.IsAcceptButton = false;
            beepLabel2.IsBorderAffectedByTheme = true;
            beepLabel2.IsCancelButton = false;
            beepLabel2.IsChild = false;
            beepLabel2.IsCustomeBorder = false;
            beepLabel2.IsDefault = false;
            beepLabel2.IsDeleted = false;
            beepLabel2.IsDirty = false;
            beepLabel2.IsEditable = false;
            beepLabel2.IsFocused = false;
            beepLabel2.IsFrameless = false;
            beepLabel2.IsHovered = false;
            beepLabel2.IsNew = false;
            beepLabel2.IsPressed = false;
            beepLabel2.IsReadOnly = false;
            beepLabel2.IsRequired = false;
            beepLabel2.IsRounded = true;
            beepLabel2.IsRoundedAffectedByTheme = true;
            beepLabel2.IsSelected = false;
            beepLabel2.IsSelectedOptionOn = true;
            beepLabel2.IsShadowAffectedByTheme = true;
            beepLabel2.IsVisible = false;
            beepLabel2.Items = (List<object>)resources.GetObject("beepLabel2.Items");
            beepLabel2.LabelBackColor = Color.Empty;
            beepLabel2.LeftoffsetForDrawingRect = 0;
            beepLabel2.LinkedProperty = null;
            beepLabel2.Location = new Point(35, 303);
            beepLabel2.Margin = new Padding(0);
            beepLabel2.MaxImageSize = new Size(16, 16);
            beepLabel2.Multiline = false;
            beepLabel2.Name = "beepLabel2";
            beepLabel2.OverrideFontSize = Vis.Modules.TypeStyleFontSize.None;
            beepLabel2.Padding = new Padding(1);
            beepLabel2.ParentBackColor = Color.Empty;
            beepLabel2.ParentControl = null;
            beepLabel2.PressedBackColor = Color.White;
            beepLabel2.PressedBorderColor = Color.Gray;
            beepLabel2.PressedForeColor = Color.Gray;
            beepLabel2.RightoffsetForDrawingRect = 0;
            beepLabel2.SavedGuidID = null;
            beepLabel2.SavedID = null;
            beepLabel2.SelectedBackColor = Color.White;
            beepLabel2.SelectedForeColor = Color.Black;
            beepLabel2.SelectedValue = null;
            beepLabel2.ShadowColor = Color.Black;
            beepLabel2.ShadowOffset = 0;
            beepLabel2.ShadowOpacity = 0.5F;
            beepLabel2.ShowAllBorders = false;
            beepLabel2.ShowBottomBorder = false;
            beepLabel2.ShowFocusIndicator = false;
            beepLabel2.ShowLeftBorder = false;
            beepLabel2.ShowRightBorder = false;
            beepLabel2.ShowShadow = false;
            beepLabel2.ShowTopBorder = false;
            beepLabel2.Size = new Size(194, 23);
            beepLabel2.SlideFrom = Vis.Modules.SlideDirection.Left;
            beepLabel2.StaticNotMoving = false;
            beepLabel2.TabIndex = 2;
            beepLabel2.TempBackColor = Color.Empty;
            beepLabel2.Text = "Destination";
            beepLabel2.TextAlign = ContentAlignment.MiddleLeft;
            beepLabel2.TextFont = new Font("Arial", 12F);
            beepLabel2.TextImageRelation = TextImageRelation.ImageBeforeText;
            beepLabel2.Theme = "DefaultType";
            beepLabel2.ToolTipText = "";
            beepLabel2.TopoffsetForDrawingRect = 0;
            beepLabel2.UseGradientBackground = false;
            beepLabel2.UseScaledFont = false;
            beepLabel2.UseThemeFont = true;
            // 
            // AddSourcebeepButton
            // 
            AddSourcebeepButton.Anchor = AnchorStyles.None;
            AddSourcebeepButton.AnimationDuration = 500;
            AddSourcebeepButton.AnimationType = Vis.Modules.DisplayAnimationType.None;
            AddSourcebeepButton.ApplyThemeOnImage = false;
            AddSourcebeepButton.ApplyThemeToChilds = false;
            AddSourcebeepButton.BackColor = Color.FromArgb(108, 117, 125);
            AddSourcebeepButton.BadgeBackColor = Color.Red;
            AddSourcebeepButton.BadgeFont = new Font("Arial", 8F, FontStyle.Bold);
            AddSourcebeepButton.BadgeForeColor = Color.White;
            AddSourcebeepButton.BadgeShape = Vis.Modules.BadgeShape.Circle;
            AddSourcebeepButton.BadgeText = "";
            AddSourcebeepButton.BlockID = null;
            AddSourcebeepButton.BorderColor = Color.Black;
            AddSourcebeepButton.BorderDashStyle = System.Drawing.Drawing2D.DashStyle.Solid;
          
            AddSourcebeepButton.BoundProperty = null;
            AddSourcebeepButton.CanBeFocused = true;
            AddSourcebeepButton.CanBeHovered = true;
            AddSourcebeepButton.CanBePressed = true;
            AddSourcebeepButton.Category = Utilities.DbFieldCategory.Boolean;
            AddSourcebeepButton.ComponentName = "AddSourcebeepButton";
            AddSourcebeepButton.DataSourceProperty = null;
            AddSourcebeepButton.DisabledBackColor = Color.FromArgb(233, 236, 239);
            AddSourcebeepButton.DisabledForeColor = Color.FromArgb(173, 181, 189);
            AddSourcebeepButton.DrawingRect = new Rectangle(0, 0, 30, 23);
            AddSourcebeepButton.Easing = Vis.Modules.EasingType.Linear;
            AddSourcebeepButton.FieldID = null;
            AddSourcebeepButton.FocusBackColor = Color.FromArgb(0, 120, 215);
            AddSourcebeepButton.FocusBorderColor = Color.Gray;
            AddSourcebeepButton.FocusForeColor = Color.FromArgb(255, 255, 255);
            AddSourcebeepButton.FocusIndicatorColor = Color.Blue;
            AddSourcebeepButton.Font = new Font("Arial", 10F);
            AddSourcebeepButton.ForeColor = Color.FromArgb(255, 255, 255);
            AddSourcebeepButton.Form = null;
            AddSourcebeepButton.GradientDirection = System.Drawing.Drawing2D.LinearGradientMode.Horizontal;
            AddSourcebeepButton.GradientEndColor = Color.Gray;
            AddSourcebeepButton.GradientStartColor = Color.Gray;
            AddSourcebeepButton.GuidID = "852e34ef-8434-4557-81ff-24c0ffe45872";
            AddSourcebeepButton.HideText = false;
            AddSourcebeepButton.HitAreaEventOn = false;
            AddSourcebeepButton.HitTestControl = null;
            AddSourcebeepButton.HoverBackColor = Color.FromArgb(30, 140, 235);
            AddSourcebeepButton.HoverBorderColor = Color.Gray;
            AddSourcebeepButton.HoveredBackcolor = Color.Wheat;
            AddSourcebeepButton.HoverForeColor = Color.FromArgb(255, 255, 255);
            AddSourcebeepButton.Id = -1;
            AddSourcebeepButton.Image = null;
            AddSourcebeepButton.ImageAlign = ContentAlignment.MiddleLeft;
            AddSourcebeepButton.ImageClicked = null;
            AddSourcebeepButton.ImageEmbededin = ImageEmbededin.Button;
            AddSourcebeepButton.ImagePath = SvgsUIcons.Common.Add;
            AddSourcebeepButton.InactiveBorderColor = Color.Gray;
    //        AddSourcebeepButton.Info = (SimpleItem)resources.GetObject("AddSourcebeepButton.Info");
            AddSourcebeepButton.IsAcceptButton = false;
            AddSourcebeepButton.IsBorderAffectedByTheme = true;
            AddSourcebeepButton.IsCancelButton = false;
            AddSourcebeepButton.IsChild = false;
            AddSourcebeepButton.IsCustomeBorder = false;
            AddSourcebeepButton.IsDefault = false;
            AddSourcebeepButton.IsDeleted = false;
            AddSourcebeepButton.IsDirty = false;
            AddSourcebeepButton.IsEditable = false;
            AddSourcebeepButton.IsFocused = false;
            AddSourcebeepButton.IsFrameless = false;
            AddSourcebeepButton.IsHovered = false;
            AddSourcebeepButton.IsNew = false;
            AddSourcebeepButton.IsPopupOpen = false;
            AddSourcebeepButton.IsPressed = false;
            AddSourcebeepButton.IsReadOnly = false;
            AddSourcebeepButton.IsRequired = false;
            AddSourcebeepButton.IsRounded = false;
            AddSourcebeepButton.IsRoundedAffectedByTheme = true;
            AddSourcebeepButton.IsSelected = false;

            AddSourcebeepButton.IsSelectedOptionOn = true;
            AddSourcebeepButton.IsShadowAffectedByTheme = true;
            AddSourcebeepButton.IsSideMenuChild = false;
            AddSourcebeepButton.IsStillButton = false;
            AddSourcebeepButton.IsVisible = false;
            AddSourcebeepButton.Items = (List<object>)resources.GetObject("AddSourcebeepButton.Items");
            AddSourcebeepButton.LeftoffsetForDrawingRect = 0;
            AddSourcebeepButton.LinkedProperty = null;
            AddSourcebeepButton.Location = new Point(429, 271);
            AddSourcebeepButton.Margin = new Padding(0);
            AddSourcebeepButton.MaxImageSize = new Size(32, 32);
            AddSourcebeepButton.Name = "AddSourcebeepButton";
            AddSourcebeepButton.OverrideFontSize = Vis.Modules.TypeStyleFontSize.None;
            AddSourcebeepButton.ParentBackColor = Color.Empty;
            AddSourcebeepButton.ParentControl = null;
            AddSourcebeepButton.PopPosition = Vis.Modules.BeepPopupFormPosition.Bottom;
            AddSourcebeepButton.PopupListForm = null;
            AddSourcebeepButton.PopupMode = false;
            AddSourcebeepButton.PressedBackColor = Color.FromArgb(0, 100, 195);
            AddSourcebeepButton.PressedBorderColor = Color.FromArgb(255, 255, 255);
            AddSourcebeepButton.PressedForeColor = Color.FromArgb(255, 255, 255);
            AddSourcebeepButton.RightoffsetForDrawingRect = 0;
            AddSourcebeepButton.SavedGuidID = null;
            AddSourcebeepButton.SavedID = null;
            AddSourcebeepButton.SelectedBackColor = Color.White;
            AddSourcebeepButton.SelectedForeColor = Color.Black;
            AddSourcebeepButton.SelectedIndex = -1;
            AddSourcebeepButton.SelectedItem = null;
            AddSourcebeepButton.SelectedValue = null;
            AddSourcebeepButton.ShadowColor = Color.Black;
            AddSourcebeepButton.ShadowOffset = 0;
            AddSourcebeepButton.ShadowOpacity = 0.5F;
            AddSourcebeepButton.ShowAllBorders = false;
            AddSourcebeepButton.ShowBottomBorder = false;
            AddSourcebeepButton.ShowFocusIndicator = false;
            AddSourcebeepButton.ShowLeftBorder = false;
            AddSourcebeepButton.ShowRightBorder = false;
            AddSourcebeepButton.ShowShadow = false;
            AddSourcebeepButton.ShowTopBorder = false;
            AddSourcebeepButton.Size = new Size(30, 23);
            AddSourcebeepButton.SlideFrom = Vis.Modules.SlideDirection.Left;
            AddSourcebeepButton.SplashColor = Color.Gray;
          //  AddSourcebeepButton.StandardImages = (List<SimpleItem>)resources.GetObject("AddSourcebeepButton.StandardImages");
            AddSourcebeepButton.StaticNotMoving = false;
            AddSourcebeepButton.TabIndex = 3;
            AddSourcebeepButton.TempBackColor = Color.Empty;
            AddSourcebeepButton.Text = "+";
            AddSourcebeepButton.TextAlign = ContentAlignment.MiddleCenter;
            AddSourcebeepButton.TextFont = new Font("Arial", 10F);
            AddSourcebeepButton.TextImageRelation = TextImageRelation.ImageBeforeText;
            AddSourcebeepButton.Theme = "DefaultType";
            AddSourcebeepButton.ToolTipText = "";
            AddSourcebeepButton.TopoffsetForDrawingRect = 0;
            AddSourcebeepButton.UseGradientBackground = false;
            AddSourcebeepButton.UseScaledFont = false;
            AddSourcebeepButton.UseThemeFont = true;
            // 
            // beepCheckBoxBool1
            // 
            beepCheckBoxBool1.Anchor = AnchorStyles.None;
            beepCheckBoxBool1.AnimationDuration = 500;
            beepCheckBoxBool1.AnimationType = Vis.Modules.DisplayAnimationType.None;
            beepCheckBoxBool1.ApplyThemeToChilds = false;
            beepCheckBoxBool1.BackColor = Color.FromArgb(245, 245, 220);
            beepCheckBoxBool1.BadgeBackColor = Color.Red;
            beepCheckBoxBool1.BadgeFont = new Font("Arial", 8F, FontStyle.Bold);
            beepCheckBoxBool1.BadgeForeColor = Color.White;
            beepCheckBoxBool1.BadgeShape = Vis.Modules.BadgeShape.Circle;
            beepCheckBoxBool1.BadgeText = "";
            beepCheckBoxBool1.BlockID = null;
            beepCheckBoxBool1.BorderColor = Color.Black;
            beepCheckBoxBool1.BorderDashStyle = System.Drawing.Drawing2D.DashStyle.Solid;
            beepCheckBoxBool1.BorderRadius = 3;
            beepCheckBoxBool1.BorderStyle = BorderStyle.FixedSingle;
            beepCheckBoxBool1.BorderThickness = 1;
            beepCheckBoxBool1.BottomoffsetForDrawingRect = 0;
            beepCheckBoxBool1.BoundProperty = "State";
            beepCheckBoxBool1.CanBeFocused = true;
            beepCheckBoxBool1.CanBeHovered = false;
            beepCheckBoxBool1.CanBePressed = true;
            beepCheckBoxBool1.Category = Utilities.DbFieldCategory.String;
            beepCheckBoxBool1.ComponentName = "beepCheckBoxBool1";
            beepCheckBoxBool1.DataSourceProperty = null;
            beepCheckBoxBool1.DisabledBackColor = Color.White;
            beepCheckBoxBool1.DisabledForeColor = Color.Black;
            beepCheckBoxBool1.DrawingRect = new Rectangle(1, 1, 178, 21);
            beepCheckBoxBool1.Easing = Vis.Modules.EasingType.Linear;
            beepCheckBoxBool1.FieldID = null;
            beepCheckBoxBool1.FocusBackColor = Color.White;
            beepCheckBoxBool1.FocusBorderColor = Color.Gray;
            beepCheckBoxBool1.FocusForeColor = Color.Black;
            beepCheckBoxBool1.FocusIndicatorColor = Color.Blue;
            beepCheckBoxBool1.ForeColor = Color.FromArgb(75, 0, 130);
            beepCheckBoxBool1.Form = null;
            beepCheckBoxBool1.GradientDirection = System.Drawing.Drawing2D.LinearGradientMode.Horizontal;
            beepCheckBoxBool1.GradientEndColor = Color.Gray;
            beepCheckBoxBool1.GradientStartColor = Color.Gray;
            beepCheckBoxBool1.GuidID = "ac159498-0f07-4e96-9316-dbcb6dc99c53";
            beepCheckBoxBool1.HideText = false;
            beepCheckBoxBool1.HitAreaEventOn = false;
            beepCheckBoxBool1.HitTestControl = null;
            beepCheckBoxBool1.HoverBackColor = Color.White;
            beepCheckBoxBool1.HoverBorderColor = Color.Gray;
            beepCheckBoxBool1.HoveredBackcolor = Color.Wheat;
            beepCheckBoxBool1.HoverForeColor = Color.Black;
            beepCheckBoxBool1.Id = -1;
            beepCheckBoxBool1.ImagePath = null;
            beepCheckBoxBool1.InactiveBorderColor = Color.Gray;
    //        beepCheckBoxBool1.Info = (SimpleItem)resources.GetObject("beepCheckBoxBool1.Info");
            beepCheckBoxBool1.IsAcceptButton = false;
            beepCheckBoxBool1.IsBorderAffectedByTheme = true;
            beepCheckBoxBool1.IsCancelButton = false;
            beepCheckBoxBool1.IsChild = false;
            beepCheckBoxBool1.IsCustomeBorder = false;
            beepCheckBoxBool1.IsDefault = false;
            beepCheckBoxBool1.IsDeleted = false;
            beepCheckBoxBool1.IsDirty = false;
            beepCheckBoxBool1.IsEditable = false;
            beepCheckBoxBool1.IsFocused = false;
            beepCheckBoxBool1.IsFrameless = false;
            beepCheckBoxBool1.IsHovered = false;
            beepCheckBoxBool1.IsNew = false;
            beepCheckBoxBool1.IsPressed = false;
            beepCheckBoxBool1.IsReadOnly = false;
            beepCheckBoxBool1.IsRequired = false;
            beepCheckBoxBool1.IsRounded = true;
            beepCheckBoxBool1.IsRoundedAffectedByTheme = true;
            beepCheckBoxBool1.IsSelected = false;
            beepCheckBoxBool1.IsSelectedOptionOn = true;
            beepCheckBoxBool1.IsShadowAffectedByTheme = true;
            beepCheckBoxBool1.IsVisible = false;
            beepCheckBoxBool1.Items = (List<object>)resources.GetObject("beepCheckBoxBool1.Items");
            beepCheckBoxBool1.LeftoffsetForDrawingRect = 0;
            beepCheckBoxBool1.LinkedProperty = null;
            beepCheckBoxBool1.Location = new Point(432, 303);
            beepCheckBoxBool1.Name = "beepCheckBoxBool1";
            beepCheckBoxBool1.OverrideFontSize = Vis.Modules.TypeStyleFontSize.None;
            beepCheckBoxBool1.Padding = new Padding(1);
            beepCheckBoxBool1.ParentBackColor = Color.Empty;
            beepCheckBoxBool1.ParentControl = null;
            beepCheckBoxBool1.PressedBackColor = Color.White;
            beepCheckBoxBool1.PressedBorderColor = Color.Gray;
            beepCheckBoxBool1.PressedForeColor = Color.Gray;
            beepCheckBoxBool1.RightoffsetForDrawingRect = 0;
            beepCheckBoxBool1.SavedGuidID = null;
            beepCheckBoxBool1.SavedID = null;
            beepCheckBoxBool1.SelectedBackColor = Color.White;
            beepCheckBoxBool1.SelectedForeColor = Color.Black;
            beepCheckBoxBool1.SelectedValue = null;
            beepCheckBoxBool1.ShadowColor = Color.Black;
            beepCheckBoxBool1.ShadowOffset = 0;
            beepCheckBoxBool1.ShadowOpacity = 0.5F;
            beepCheckBoxBool1.ShowAllBorders = false;
            beepCheckBoxBool1.ShowBottomBorder = false;
            beepCheckBoxBool1.ShowFocusIndicator = false;
            beepCheckBoxBool1.ShowLeftBorder = false;
            beepCheckBoxBool1.ShowRightBorder = false;
            beepCheckBoxBool1.ShowShadow = false;
            beepCheckBoxBool1.ShowTopBorder = false;
            beepCheckBoxBool1.Size = new Size(180, 23);
            beepCheckBoxBool1.SlideFrom = Vis.Modules.SlideDirection.Left;
            beepCheckBoxBool1.Spacing = 5;
            beepCheckBoxBool1.StaticNotMoving = false;
            beepCheckBoxBool1.TabIndex = 4;
            beepCheckBoxBool1.TempBackColor = Color.Empty;
            beepCheckBoxBool1.Text = "Create Entity if it not Exist";
            beepCheckBoxBool1.Theme ="DefaultType";
            beepCheckBoxBool1.ToolTipText = "";
            beepCheckBoxBool1.TopoffsetForDrawingRect = 0;
            beepCheckBoxBool1.UseGradientBackground = false;
            beepCheckBoxBool1.UseThemeFont = true;
            // 
            // _sourceEntityLabel
            // 
            _sourceEntityLabel.AutoSize  = false;
            _sourceEntityLabel.Location  = new Point(35, 337);
            _sourceEntityLabel.Name      = "_sourceEntityLabel";
            _sourceEntityLabel.Size      = new Size(194, 23);
            _sourceEntityLabel.TabIndex  = 5;
            _sourceEntityLabel.Text      = "Source Entity";
            _sourceEntityLabel.TextAlign = ContentAlignment.MiddleLeft;
            _sourceEntityLabel.ShowAllBorders = false;
            _sourceEntityLabel.Theme = "DefaultType";
            // 
            // _sourceEntityCombo
            // 
            _sourceEntityCombo.Anchor    = AnchorStyles.None;
            _sourceEntityCombo.Location  = new Point(232, 335);
            _sourceEntityCombo.Name      = "_sourceEntityCombo";
            _sourceEntityCombo.Size      = new Size(194, 27);
            _sourceEntityCombo.TabIndex  = 6;
            _sourceEntityCombo.Theme     = "DefaultType";
            // 
            // _destinationEntityLabel
            // 
            _destinationEntityLabel.AutoSize  = false;
            _destinationEntityLabel.Location  = new Point(35, 369);
            _destinationEntityLabel.Name      = "_destinationEntityLabel";
            _destinationEntityLabel.Size      = new Size(194, 23);
            _destinationEntityLabel.TabIndex  = 7;
            _destinationEntityLabel.Text      = "Destination Entity";
            _destinationEntityLabel.TextAlign = ContentAlignment.MiddleLeft;
            _destinationEntityLabel.ShowAllBorders = false;
            _destinationEntityLabel.Theme = "DefaultType";
            // 
            // _destinationEntityCombo
            // 
            _destinationEntityCombo.Anchor    = AnchorStyles.None;
            _destinationEntityCombo.Location  = new Point(232, 367);
            _destinationEntityCombo.Name      = "_destinationEntityCombo";
            _destinationEntityCombo.Size      = new Size(194, 27);
            _destinationEntityCombo.TabIndex  = 8;
            _destinationEntityCombo.Theme     = "DefaultType";
            // 
            // _statusLabel
            // 
            _statusLabel.AutoSize  = false;
            _statusLabel.Location  = new Point(35, 400);
            _statusLabel.Name      = "_statusLabel";
            _statusLabel.Size      = new Size(560, 23);
            _statusLabel.TabIndex  = 9;
            _statusLabel.Text      = "Select source and destination entities.";
            _statusLabel.TextAlign = ContentAlignment.MiddleLeft;
            _statusLabel.ShowAllBorders = false;
            _statusLabel.Theme = "DefaultType";
            //
            // pnlPurpose  (docked top — Purpose, MatchBy, UpdateEmpty, RowCount rows)
            //
            pnlPurpose.Dock      = System.Windows.Forms.DockStyle.Top;
            pnlPurpose.Height    = 130;
            pnlPurpose.Name      = "pnlPurpose";
            pnlPurpose.TabIndex  = 20;
            pnlPurpose.Controls.Add(btnRefreshCount);
            pnlPurpose.Controls.Add(lblRowCount);
            pnlPurpose.Controls.Add(chkUpdateEmpty);
            pnlPurpose.Controls.Add(cmbMatchBy);
            pnlPurpose.Controls.Add(lblMatchBy);
            pnlPurpose.Controls.Add(cmbPurpose);
            pnlPurpose.Controls.Add(lblPurpose);
            //
            // lblPurpose
            //
            lblPurpose.AutoSize  = false;
            lblPurpose.Location  = new System.Drawing.Point(12, 8);
            lblPurpose.Size      = new System.Drawing.Size(110, 24);
            lblPurpose.Name      = "lblPurpose";
            lblPurpose.Text      = "Purpose:";
            lblPurpose.TextAlign = ContentAlignment.MiddleLeft;
            lblPurpose.ShowAllBorders = false;
            lblPurpose.Theme     = "DefaultType";
            //
            // cmbPurpose
            //
            cmbPurpose.Location  = new System.Drawing.Point(130, 6);
            cmbPurpose.Size      = new System.Drawing.Size(220, 27);
            cmbPurpose.Name      = "cmbPurpose";
            cmbPurpose.TabIndex  = 20;
            cmbPurpose.Theme     = "DefaultType";
            //
            // lblMatchBy
            //
            lblMatchBy.AutoSize  = false;
            lblMatchBy.Location  = new System.Drawing.Point(12, 40);
            lblMatchBy.Size      = new System.Drawing.Size(110, 24);
            lblMatchBy.Name      = "lblMatchBy";
            lblMatchBy.Text      = "Match By:";
            lblMatchBy.TextAlign = ContentAlignment.MiddleLeft;
            lblMatchBy.ShowAllBorders = false;
            lblMatchBy.Theme     = "DefaultType";
            lblMatchBy.Visible   = false;
            //
            // cmbMatchBy
            //
            cmbMatchBy.Location  = new System.Drawing.Point(130, 38);
            cmbMatchBy.Size      = new System.Drawing.Size(220, 27);
            cmbMatchBy.Name      = "cmbMatchBy";
            cmbMatchBy.TabIndex  = 21;
            cmbMatchBy.Theme     = "DefaultType";
            cmbMatchBy.Visible   = false;
            //
            // chkUpdateEmpty
            //
            chkUpdateEmpty.AutoSize = false;
            chkUpdateEmpty.Location = new System.Drawing.Point(12, 72);
            chkUpdateEmpty.Size     = new System.Drawing.Size(300, 24);
            chkUpdateEmpty.Name     = "chkUpdateEmpty";
            chkUpdateEmpty.Text     = "Overwrite empty fields on update";
            chkUpdateEmpty.TabIndex = 22;
            chkUpdateEmpty.Theme    = "DefaultType";
            chkUpdateEmpty.Visible  = false;
            //
            // lblRowCount
            //
            lblRowCount.AutoSize  = false;
            lblRowCount.Location  = new System.Drawing.Point(12, 102);
            lblRowCount.Size      = new System.Drawing.Size(200, 22);
            lblRowCount.Name      = "lblRowCount";
            lblRowCount.Text      = "Row count: —";
            lblRowCount.TextAlign = ContentAlignment.MiddleLeft;
            lblRowCount.ShowAllBorders = false;
            lblRowCount.Theme     = "DefaultType";
            //
            // btnRefreshCount
            //
            btnRefreshCount.Location = new System.Drawing.Point(218, 100);
            btnRefreshCount.Size     = new System.Drawing.Size(28, 24);
            btnRefreshCount.Name     = "btnRefreshCount";
            btnRefreshCount.ImagePath = SvgsUIcons.Common.Refresh;
            btnRefreshCount.TabIndex = 23;
            btnRefreshCount.Theme    = "DefaultType";
            // 
            // uc_Import_SelectDSandEntity
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(_statusLabel);
            Controls.Add(_destinationEntityCombo);
            Controls.Add(_destinationEntityLabel);
            Controls.Add(_sourceEntityCombo);
            Controls.Add(_sourceEntityLabel);
            Controls.Add(beepCheckBoxBool1);
            Controls.Add(AddSourcebeepButton);
            Controls.Add(beepLabel2);
            Controls.Add(beepComboBox1);
            Controls.Add(beepLabel1);
            Controls.Add(SourcebeepComboBox);
            Controls.Add(pnlPurpose);
            Name = "uc_Import_SelectDSandEntity";
            Size = new Size(614, 534);
            ResumeLayout(false);
        }

        #endregion

        private BeepComboBox SourcebeepComboBox;
        private Controls.BeepLabel beepLabel1;
        private BeepComboBox beepComboBox1;
        private Controls.BeepLabel beepLabel2;
        private BeepButton AddSourcebeepButton;
        private BeepCheckBoxBool beepCheckBoxBool1;
        private Controls.BeepLabel _sourceEntityLabel;
        private BeepComboBox _sourceEntityCombo;
        private Controls.BeepLabel _destinationEntityLabel;
        private BeepComboBox _destinationEntityCombo;
        private Controls.BeepLabel _statusLabel;

        // ── New controls added for Purpose/MatchBy/RowCount ───────────────────
        private System.Windows.Forms.Panel  pnlPurpose;
        private Controls.BeepLabel          lblPurpose;
        private BeepComboBox                cmbPurpose;
        private Controls.BeepLabel          lblMatchBy;
        private BeepComboBox                cmbMatchBy;
        private BeepCheckBoxBool            chkUpdateEmpty;
        private Controls.BeepLabel          lblRowCount;
        private BeepButton                  btnRefreshCount;
    }
}
