using TheTechIdea.Beep.Vis.Modules;
using TheTechIdea.Beep.Winform.Controls;
using TheTechIdea.Beep.Winform.Controls.AppBars;
using TheTechIdea.Beep.Winform.Controls.DisplayContainers;
using TheTechIdea.Beep.Winform.Controls.Managers;
using TheTechIdea.Beep.Winform.Controls.Models;

namespace TheTechIdea.Beep.Winform.Default.Views
{
    partial class MainFrm_Tree
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainFrm_Tree));
            beepAppTree1 = new TheTechIdea.Beep.Winform.Controls.ITrees.BeepTreeView.BeepAppTree();
            beepDisplayContainer1 = new BeepDisplayContainer2();
            SuspendLayout();
            // 
            // beepAppTree1
            // 
            beepAppTree1.AllowMultiSelect = false;
            beepAppTree1.AnimationDuration = 500;
            beepAppTree1.AnimationType = DisplayAnimationType.None;
            beepAppTree1.ApplyThemeToChilds = false;
            beepAppTree1.args = null;
            beepAppTree1.AutoDrawHitListComponents = true;
            beepAppTree1.BackColor = Color.White;
            beepAppTree1.BadgeBackColor = Color.Red;
            beepAppTree1.BadgeFont = new Font("Arial", 8F, FontStyle.Bold);
            beepAppTree1.BadgeForeColor = Color.White;
            beepAppTree1.BadgeShape = BadgeShape.Circle;
            beepAppTree1.BadgeText = "";
            beepAppTree1.BeepService = null;
            beepAppTree1.BlockID = null;
            beepAppTree1.BorderColor = Color.FromArgb(200, 200, 200);
            beepAppTree1.BorderDashStyle = DashStyle.Solid;
            beepAppTree1.BorderPainter = BeepControlStyle.None;
            beepAppTree1.BorderRadius = 8;
            beepAppTree1.BorderStyle = BorderStyle.FixedSingle;
            beepAppTree1.BorderThickness = 1;
            beepAppTree1.BottomoffsetForDrawingRect = 0;
            beepAppTree1.BoundProperty = null;
            beepAppTree1.Branches = null;
            beepAppTree1.CanBeFocused = true;
            beepAppTree1.CanBeHovered = false;
            beepAppTree1.CanBePressed = true;
            beepAppTree1.CanBeSelected = true;
            beepAppTree1.Category = Utilities.DbFieldCategory.String;
            beepAppTree1.CategoryIcon = "Category.svg";
            beepAppTree1.ComponentName = "BeepControl";
            beepAppTree1.CurrentBranch = null;
            beepAppTree1.CurrentMenutems = null;
            beepAppTree1.DataContext = null;
            beepAppTree1.DataSourceProperty = null;
            beepAppTree1.DisabledBackColor = Color.White;
            beepAppTree1.DisabledBorderColor = Color.Empty;
            beepAppTree1.DisabledForeColor = Color.Black;
            beepAppTree1.DMEEditor = null;
            beepAppTree1.Dock = DockStyle.Left;
            beepAppTree1.DrawingRect = new Rectangle(0, 0, 211, 398);
            beepAppTree1.DropHandler = null;
            beepAppTree1.Easing = EasingType.Linear;
            beepAppTree1.EnableHighQualityRendering = true;            beepAppTree1.EnableRippleEffect = true;
            beepAppTree1.EnableSplashEffect = false;
            beepAppTree1.ErrorColor = Color.FromArgb(176, 0, 32);
            beepAppTree1.ErrorText = "";
            beepAppTree1.ExtensionsHelpers = null;
            beepAppTree1.ExternalDrawingLayer = DrawingLayer.AfterAll;
            beepAppTree1.FieldID = null;
            beepAppTree1.FilledBackgroundColor = Color.FromArgb(20, 0, 0, 0);
            beepAppTree1.Filterstring = null;
            beepAppTree1.FloatingLabel = string.Empty;
            beepAppTree1.FocusBackColor = Color.White;
            beepAppTree1.FocusBorderColor = Color.RoyalBlue;
            beepAppTree1.FocusForeColor = Color.Black;
            beepAppTree1.FocusIndicatorColor = Color.Blue;
            beepAppTree1.Font = new Font("Arial", 10F);
            beepAppTree1.ForeColor = Color.Black;
            beepAppTree1.Form = null;
            beepAppTree1.GlassmorphismBlur = 10F;
            beepAppTree1.GlassmorphismOpacity = 0.1F;
            beepAppTree1.GradientAngle = 0F;
            beepAppTree1.GradientDirection = LinearGradientMode.Horizontal;
            beepAppTree1.GradientEndColor = Color.FromArgb(230, 230, 230);
            beepAppTree1.GradientStartColor = Color.FromArgb(255, 255, 255);
            beepAppTree1.GridMode = false;
            beepAppTree1.GuidID = "0c8afc7c-7c68-4501-8665-10e143d25228";
            beepAppTree1.HasError = false;
            beepAppTree1.HelperText = "";
            beepAppTree1.HelperTextOn = false;
            beepAppTree1.HitAreaEventOn = false;
            beepAppTree1.HitTestControl = null;
            beepAppTree1.HoverBackColor = Color.Wheat;
            beepAppTree1.HoverBorderColor = Color.Gray;
            beepAppTree1.HoveredBackcolor = Color.Wheat;
            beepAppTree1.HoverForeColor = Color.Black;
            beepAppTree1.IconSize = 20;
            beepAppTree1.Id = -1;
            beepAppTree1.InactiveBorderColor = Color.Gray;
            beepAppTree1.InnerShape = null;
            beepAppTree1.IsAcceptButton = false;
            beepAppTree1.IsBorderAffectedByTheme = true;
            beepAppTree1.IsCancelButton = false;
            beepAppTree1.IsCheckBoxon = false;
            beepAppTree1.IsChild = false;
            beepAppTree1.IsCustomeBorder = false;
            beepAppTree1.IsDefault = false;
            beepAppTree1.IsDeleted = false;
            beepAppTree1.IsDirty = false;
            beepAppTree1.IsEditable = false;
            beepAppTree1.IsFocused = false;
            beepAppTree1.IsFrameless = false;
            beepAppTree1.IsHovered = false;
            beepAppTree1.IsNew = false;
            beepAppTree1.IsPressed = false;
            beepAppTree1.IsReadOnly = false;
            beepAppTree1.IsRequired = false;
            beepAppTree1.IsRounded = false;
            beepAppTree1.IsRoundedAffectedByTheme = true;
            beepAppTree1.IsSelected = false;
            beepAppTree1.IsSelectedOptionOn = false;
            beepAppTree1.IsShadowAffectedByTheme = true;
            beepAppTree1.IsTransparentBackground = false;
            beepAppTree1.IsValid = true;
            beepAppTree1.IsVisible = true;
            beepAppTree1.Items = (List<object>)resources.GetObject("beepAppTree1.Items");
            beepAppTree1.LabelText = "";
            beepAppTree1.LabelTextOn = false;
            beepAppTree1.LeadingIconPath = "";
            beepAppTree1.LeadingImagePath = "";
            beepAppTree1.LeftoffsetForDrawingRect = 0;
            beepAppTree1.LinkedProperty = null;
            beepAppTree1.Location = new Point(4, 48);            beepAppTree1.MaxHitListDrawPerFrame = 0;
            beepAppTree1.ModernGradientType = ModernGradientType.None;
            beepAppTree1.Name = "beepAppTree1";
            beepAppTree1.ObjectType = "Beep";
            beepAppTree1.OverrideFontSize = TypeStyleFontSize.None;
            beepAppTree1.PainterKind = BaseControlPainterKind.Classic;
            beepAppTree1.ParentBackColor = Color.Empty;
            beepAppTree1.ParentControl = null;
            beepAppTree1.PressedBackColor = Color.White;
            beepAppTree1.PressedBorderColor = Color.Gray;
            beepAppTree1.PressedForeColor = Color.Gray;
            beepAppTree1.RadialCenter = (PointF)resources.GetObject("beepAppTree1.RadialCenter");
            beepAppTree1.RightoffsetForDrawingRect = 0;
            beepAppTree1.SavedGuidID = null;
            beepAppTree1.SavedID = null;
            beepAppTree1.ScaleMode = ImageScaleMode.KeepAspectRatio;
            beepAppTree1.SelectedBackColor = Color.White;
            beepAppTree1.SelectedBorderColor = Color.Empty;
            beepAppTree1.SelectedBranch = null;
            beepAppTree1.SelectedBranchID = 0;
            beepAppTree1.SelectedBranchs = (List<int>)resources.GetObject("beepAppTree1.SelectedBranchs");
            beepAppTree1.SelectedForeColor = Color.Black;
            beepAppTree1.SelectedValue = null;
            beepAppTree1.SelectIcon = "Select.svg";
            beepAppTree1.SeqID = 2;
            beepAppTree1.ShadowColor = Color.FromArgb(50, 0, 0, 0);
            beepAppTree1.ShadowOffset = 0;
            beepAppTree1.ShadowOpacity = 0.5F;
            beepAppTree1.ShowAllBorders = false;
            beepAppTree1.ShowBottomBorder = false;
            beepAppTree1.ShowCheckBox = true;
            beepAppTree1.ShowFocusIndicator = false;
            beepAppTree1.ShowHorizontalScrollBar = true;
            beepAppTree1.ShowLeftBorder = false;
            beepAppTree1.ShowRightBorder = false;
            beepAppTree1.ShowShadow = false;
            beepAppTree1.ShowTopBorder = false;
            beepAppTree1.ShowVerticalScrollBar = true;
            beepAppTree1.Size = new Size(211, 398);
            beepAppTree1.SlideFrom = SlideDirection.Left;
            beepAppTree1.StaticNotMoving = false;
            beepAppTree1.TabIndex = 1;
            beepAppTree1.Tag = this;
            beepAppTree1.TempBackColor = Color.Empty;
            beepAppTree1.Text = "beepAppTree1";
            beepAppTree1.TextAlignment = TextAlignment.Left;
            beepAppTree1.TextFont = new Font("Segoe UI", 10F);
            beepAppTree1.Theme = "DefaultType";
            beepAppTree1.ToolTipText = "";
            beepAppTree1.TopoffsetForDrawingRect = 0;
            beepAppTree1.TrailingIconPath = "";
            beepAppTree1.TrailingImagePath = "";
            beepAppTree1.Treebranchhandler = null;
            beepAppTree1.TreeStyle = TreeStyle.AntDesign;
            beepAppTree1.TreeType = "Beep";
            beepAppTree1.UseExternalBufferedGraphics = true;
            beepAppTree1.UseGlassmorphism = false;
            beepAppTree1.UseGradientBackground = false;
            beepAppTree1.UseRichToolTip = true;
            beepAppTree1.UseScaledFont = false;
            beepAppTree1.UseThemeFont = false;
            beepAppTree1.VirtualizationBufferRows = 100;
            beepAppTree1.VirtualizeLayout = true;
            beepAppTree1.VisManager = null;
            // 
            // beepDisplayContainer1
            // 
            beepDisplayContainer1.AnimationDuration = 500;
            beepDisplayContainer1.AnimationType = DisplayAnimationType.None;
            beepDisplayContainer1.ApplyThemeToChilds = false;
            beepDisplayContainer1.AutoDrawHitListComponents = true;
            beepDisplayContainer1.BackColor = Color.White;
            beepDisplayContainer1.BadgeBackColor = Color.Red;
            beepDisplayContainer1.BadgeFont = new Font("Arial", 8F, FontStyle.Bold);
            beepDisplayContainer1.BadgeForeColor = Color.White;
            beepDisplayContainer1.BadgeShape = BadgeShape.Circle;
            beepDisplayContainer1.BadgeText = "";
            beepDisplayContainer1.BlockID = null;
            beepDisplayContainer1.BorderColor = Color.Black;
            beepDisplayContainer1.BorderDashStyle = DashStyle.Solid;
            beepDisplayContainer1.BorderPainter = BeepControlStyle.None;
            beepDisplayContainer1.BorderRadius = 8;
            beepDisplayContainer1.BorderStyle = BorderStyle.FixedSingle;
            beepDisplayContainer1.BorderThickness = 1;
            beepDisplayContainer1.BottomoffsetForDrawingRect = 0;
            beepDisplayContainer1.BoundProperty = null;
            beepDisplayContainer1.CanBeFocused = true;
            beepDisplayContainer1.CanBeHovered = false;
            beepDisplayContainer1.CanBePressed = true;
            beepDisplayContainer1.CanBeSelected = true;
            beepDisplayContainer1.Category = Utilities.DbFieldCategory.String;
            beepDisplayContainer1.ComponentName = "BeepDisplayContainer";
            beepDisplayContainer1.ContainerType = ContainerTypeEnum.TabbedPanel;
            beepDisplayContainer1.DataContext = null;
            beepDisplayContainer1.DataSourceProperty = null;
            beepDisplayContainer1.DisabledBackColor = Color.White;
            beepDisplayContainer1.DisabledBorderColor = Color.Empty;
            beepDisplayContainer1.DisabledForeColor = Color.Black;
            beepDisplayContainer1.Dock = DockStyle.Fill;
            beepDisplayContainer1.DrawingRect = new Rectangle(0, 0, 581, 398);
            beepDisplayContainer1.Easing = EasingType.Linear;
            beepDisplayContainer1.EnableHighQualityRendering = true;            beepDisplayContainer1.EnableRippleEffect = true;
            beepDisplayContainer1.EnableSplashEffect = false;
            beepDisplayContainer1.ErrorColor = Color.FromArgb(176, 0, 32);
            beepDisplayContainer1.ErrorText = "";
            beepDisplayContainer1.ExternalDrawingLayer = DrawingLayer.AfterAll;
            beepDisplayContainer1.FieldID = null;
            beepDisplayContainer1.FilledBackgroundColor = Color.FromArgb(20, 0, 0, 0);
            beepDisplayContainer1.FloatingLabel = string.Empty;
            beepDisplayContainer1.FocusBackColor = Color.White;
            beepDisplayContainer1.FocusBorderColor = Color.RoyalBlue;
            beepDisplayContainer1.FocusForeColor = Color.Black;
            beepDisplayContainer1.FocusIndicatorColor = Color.Blue;
            beepDisplayContainer1.Font = new Font("Arial", 10F);
            beepDisplayContainer1.Form = null;
            beepDisplayContainer1.GlassmorphismBlur = 10F;
            beepDisplayContainer1.GlassmorphismOpacity = 0.1F;
            beepDisplayContainer1.GradientAngle = 0F;
            beepDisplayContainer1.GradientDirection = LinearGradientMode.Horizontal;
            beepDisplayContainer1.GradientEndColor = Color.Gray;
            beepDisplayContainer1.GradientStartColor = Color.Gray;
            beepDisplayContainer1.GridMode = false;
            beepDisplayContainer1.GuidID = "cf74e178-0c33-4f0b-8ee3-8d24f22cc479";
            beepDisplayContainer1.HasError = false;
            beepDisplayContainer1.HelperText = "";
            beepDisplayContainer1.HelperTextOn = false;
            beepDisplayContainer1.HitAreaEventOn = false;
            beepDisplayContainer1.HitTestControl = null;
            beepDisplayContainer1.HoverBackColor = Color.Wheat;
            beepDisplayContainer1.HoverBorderColor = Color.Gray;
            beepDisplayContainer1.HoveredBackcolor = Color.Wheat;
            beepDisplayContainer1.HoverForeColor = Color.Black;
            beepDisplayContainer1.IconSize = 20;
            beepDisplayContainer1.Id = -1;
            beepDisplayContainer1.InactiveBorderColor = Color.Gray;
            beepDisplayContainer1.InnerShape = null;
            beepDisplayContainer1.IsAcceptButton = false;
            beepDisplayContainer1.IsBorderAffectedByTheme = true;
            beepDisplayContainer1.IsCancelButton = false;
            beepDisplayContainer1.IsChild = false;
            beepDisplayContainer1.IsCustomeBorder = false;
            beepDisplayContainer1.IsDefault = false;
            beepDisplayContainer1.IsDeleted = false;
            beepDisplayContainer1.IsDirty = false;
            beepDisplayContainer1.IsEditable = false;
            beepDisplayContainer1.IsFocused = false;
            beepDisplayContainer1.IsFrameless = false;
            beepDisplayContainer1.IsHovered = false;
            beepDisplayContainer1.IsNew = false;
            beepDisplayContainer1.IsPressed = false;
            beepDisplayContainer1.IsReadOnly = false;
            beepDisplayContainer1.IsRequired = false;
            beepDisplayContainer1.IsRounded = false;
            beepDisplayContainer1.IsRoundedAffectedByTheme = true;
            beepDisplayContainer1.IsSelected = false;
            beepDisplayContainer1.IsSelectedOptionOn = false;
            beepDisplayContainer1.IsShadowAffectedByTheme = true;
            beepDisplayContainer1.IsTransparentBackground = false;
            beepDisplayContainer1.IsValid = true;
            beepDisplayContainer1.IsVisible = true;
            beepDisplayContainer1.Items = (List<object>)resources.GetObject("beepDisplayContainer1.Items");
            beepDisplayContainer1.LabelText = "";
            beepDisplayContainer1.LabelTextOn = false;
            beepDisplayContainer1.LeadingIconPath = "";
            beepDisplayContainer1.LeadingImagePath = "";
            beepDisplayContainer1.LeftoffsetForDrawingRect = 0;
            beepDisplayContainer1.LinkedProperty = null;
            beepDisplayContainer1.Location = new Point(215, 48);            beepDisplayContainer1.MaxHitListDrawPerFrame = 0;
            beepDisplayContainer1.ModernGradientType = ModernGradientType.None;
            beepDisplayContainer1.Name = "beepDisplayContainer1";
            beepDisplayContainer1.OverrideFontSize = TypeStyleFontSize.None;
            beepDisplayContainer1.Padding = new Padding(2);
            beepDisplayContainer1.PainterKind = BaseControlPainterKind.None;
            beepDisplayContainer1.ParentBackColor = Color.Empty;
            beepDisplayContainer1.ParentControl = null;
            beepDisplayContainer1.PressedBackColor = Color.White;
            beepDisplayContainer1.PressedBorderColor = Color.Gray;
            beepDisplayContainer1.PressedForeColor = Color.Gray;
            beepDisplayContainer1.RadialCenter = (PointF)resources.GetObject("beepDisplayContainer1.RadialCenter");
            beepDisplayContainer1.RightoffsetForDrawingRect = 0;
            beepDisplayContainer1.SavedGuidID = null;
            beepDisplayContainer1.SavedID = null;
            beepDisplayContainer1.ScaleMode = ImageScaleMode.KeepAspectRatio;
            beepDisplayContainer1.SelectedBackColor = Color.White;
            beepDisplayContainer1.SelectedBorderColor = Color.Empty;
            beepDisplayContainer1.SelectedForeColor = Color.Black;
            beepDisplayContainer1.SelectedValue = null;
            beepDisplayContainer1.ShadowColor = Color.Black;
            beepDisplayContainer1.ShadowOffset = 0;
            beepDisplayContainer1.ShadowOpacity = 0.5F;
            beepDisplayContainer1.ShowAllBorders = false;
            beepDisplayContainer1.ShowBottomBorder = false;
            beepDisplayContainer1.ShowFocusIndicator = false;
            beepDisplayContainer1.ShowLeftBorder = false;
            beepDisplayContainer1.ShowRightBorder = false;
            beepDisplayContainer1.ShowShadow = false;
            beepDisplayContainer1.ShowTopBorder = false;
            beepDisplayContainer1.Size = new Size(581, 398);
            beepDisplayContainer1.SlideFrom = SlideDirection.Left;
            beepDisplayContainer1.StaticNotMoving = false;
            beepDisplayContainer1.TabIndex = 2;
            beepDisplayContainer1.Tag = this;
            beepDisplayContainer1.TempBackColor = Color.Empty;
            beepDisplayContainer1.Text = "beepDisplayContainer1";
            beepDisplayContainer1.TextFont = new Font("Arial", 10F);
            beepDisplayContainer1.Theme = "DefaultType";
            beepDisplayContainer1.ToolTipText = "";
            beepDisplayContainer1.TopoffsetForDrawingRect = 0;
            beepDisplayContainer1.TrailingIconPath = "";
            beepDisplayContainer1.TrailingImagePath = "";
            beepDisplayContainer1.UseExternalBufferedGraphics = false;
            beepDisplayContainer1.UseFormStylePaint = true;
            beepDisplayContainer1.UseGlassmorphism = false;
            beepDisplayContainer1.UseGradientBackground = false;
            beepDisplayContainer1.UseRichToolTip = true;
            beepDisplayContainer1.UseThemeFont = true;
            // 
            // MainFrm_Tree
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BorderColor = Color.FromArgb(200, 200, 200);
            ClientSize = new Size(800, 450);
            Controls.Add(beepDisplayContainer1);
            Controls.Add(beepAppTree1);
            Location = new Point(0, 0);
            Name = "MainFrm_Tree";
            Text = "MainFrm_Tree1";
            Load += MainFrm_Tree_Load;
            ResumeLayout(false);
        }

        #endregion
        private BeepDisplayContainer2 beepDisplayContainer1;
        private Controls.ITrees.BeepTreeView.BeepAppTree beepAppTree1;
    }
}