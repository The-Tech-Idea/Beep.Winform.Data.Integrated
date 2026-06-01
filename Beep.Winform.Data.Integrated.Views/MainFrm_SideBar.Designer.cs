using TheTechIdea.Beep.Vis.Modules;
using TheTechIdea.Beep.Winform.Controls;
using TheTechIdea.Beep.Winform.Controls.AppBars;
using TheTechIdea.Beep.Winform.Controls.DisplayContainers;
using TheTechIdea.Beep.Winform.Controls.Models;

namespace TheTechIdea.Beep.Winform.Default.Views
{
    partial class MainFrm_SideBar
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainFrm_SideBar));
            GraphicsPath graphicsPath1 = new GraphicsPath();
            GraphicsPath graphicsPath2 = new GraphicsPath();
            GraphicsPath graphicsPath3 = new GraphicsPath();
            beepSideMenu1 = new BeepSideMenu();
            beepDisplayContainer1 = new BeepDisplayContainer2();
            SuspendLayout();
            // 
            // beepSideMenu1
            // 
            beepSideMenu1.AnimationDuration = 500;
            beepSideMenu1.AnimationStep = 2;
            beepSideMenu1.AnimationType = DisplayAnimationType.None;
            beepSideMenu1.ApplyThemeOnImages = false;
            beepSideMenu1.ApplyThemeToChilds = false;
            beepSideMenu1.AutoDrawHitListComponents = true;
            beepSideMenu1.BackColor = Color.White;
            beepSideMenu1.BadgeBackColor = Color.Red;
            beepSideMenu1.BadgeFont = new Font("Arial", 8F, FontStyle.Bold);
            beepSideMenu1.BadgeForeColor = Color.White;
            beepSideMenu1.BadgeShape = BadgeShape.Circle;
            beepSideMenu1.BadgeText = "";
            beepSideMenu1.BeepAppBar = null;
            beepSideMenu1.BeepForm = null;
            beepSideMenu1.BlockID = null;
            beepSideMenu1.BorderColor = Color.FromArgb(200, 200, 200);
            beepSideMenu1.BorderDashStyle = DashStyle.Solid;
            beepSideMenu1.BorderRadius = 8;
            beepSideMenu1.BorderStyle = BorderStyle.FixedSingle;
            beepSideMenu1.BorderThickness = 1;
            beepSideMenu1.BottomoffsetForDrawingRect = 0;
            beepSideMenu1.BoundProperty = null;
            beepSideMenu1.ButtonSize = new Size(190, 40);
            beepSideMenu1.CanBeFocused = true;
            beepSideMenu1.CanBeHovered = false;
            beepSideMenu1.CanBePressed = true;
            beepSideMenu1.Category = DbFieldCategory.String;
            beepSideMenu1.CollapsedWidth = 64;
            beepSideMenu1.CollapseOnItemClick = false;
            beepSideMenu1.ComponentName = "BeepControl";
            beepSideMenu1.DataSourceProperty = null;
            beepSideMenu1.DescriptionSize = new Size(100, 20);
            beepSideMenu1.DisabledBackColor = Color.White;
            beepSideMenu1.DisabledBorderColor = Color.Empty;
            beepSideMenu1.DisabledForeColor = Color.Black;
          
            beepSideMenu1.Dock = DockStyle.Left;
            beepSideMenu1.DrawingRect = new Rectangle(5, 5, 149, 568);
            beepSideMenu1.Easing = EasingType.Linear;
            beepSideMenu1.EnableHighQualityRendering = true;
            beepSideMenu1.EnableRippleEffect = true;
            beepSideMenu1.EnableSplashEffect = true;
            beepSideMenu1.ExpandedWidth = 200;
            beepSideMenu1.ExternalDrawingLayer = DrawingLayer.AfterAll;
            beepSideMenu1.FieldID = null;
            beepSideMenu1.FilledBackgroundColor = Color.FromArgb(20, 0, 0, 0);

            beepSideMenu1.FocusBackColor = Color.White;
            beepSideMenu1.FocusBorderColor = Color.RoyalBlue;
            beepSideMenu1.FocusForeColor = Color.Black;
            beepSideMenu1.FocusIndicatorColor = Color.Blue;
            beepSideMenu1.Font = new Font("Segoe UI", 8F);
            beepSideMenu1.ForeColor = Color.White;
            beepSideMenu1.Form = null;
            beepSideMenu1.GlassmorphismBlur = 10F;
            beepSideMenu1.GlassmorphismOpacity = 0.1F;
            beepSideMenu1.GradientAngle = 0F;
            beepSideMenu1.GradientDirection = LinearGradientMode.Horizontal;
            beepSideMenu1.GradientEndColor = Color.FromArgb(230, 230, 230);
            beepSideMenu1.GradientStartColor = Color.FromArgb(255, 255, 255);
            beepSideMenu1.GridMode = false;
            beepSideMenu1.GuidID = "766debb9-c948-435c-a4e3-7b9b335ac926";
            beepSideMenu1.HelperText = "";
            beepSideMenu1.HilightPanelSize = 5;
            beepSideMenu1.HitAreaEventOn = false;
            beepSideMenu1.HitTestControl = null;
            beepSideMenu1.HoverBackColor = Color.White;
            beepSideMenu1.HoverBorderColor = Color.Gray;
            beepSideMenu1.HoveredBackcolor = Color.Wheat;
            beepSideMenu1.HoverForeColor = Color.Black;
            beepSideMenu1.Id = -1;
            beepSideMenu1.InactiveBorderColor = Color.Gray;
            beepSideMenu1.IsAcceptButton = false;
            beepSideMenu1.IsBorderAffectedByTheme = false;
            beepSideMenu1.IsCancelButton = false;
            beepSideMenu1.IsChild = false;
            beepSideMenu1.IsCustomeBorder = false;
            beepSideMenu1.IsDefault = false;
            beepSideMenu1.IsDeleted = false;
            beepSideMenu1.IsDirty = false;
            beepSideMenu1.IsEditable = false;
            beepSideMenu1.IsFocused = false;
            beepSideMenu1.IsFrameless = true;
            beepSideMenu1.IsHovered = false;
            beepSideMenu1.IsNew = false;
            beepSideMenu1.IsPressed = false;
            beepSideMenu1.IsReadOnly = false;
            beepSideMenu1.IsRequired = false;
            beepSideMenu1.IsRounded = false;
            beepSideMenu1.IsRoundedAffectedByTheme = false;
            beepSideMenu1.IsSelected = false;
            beepSideMenu1.IsSelectedOptionOn = false;
            beepSideMenu1.IsShadowAffectedByTheme = false;
            beepSideMenu1.IsVisible = true;
            beepSideMenu1.LabelText = "";
            beepSideMenu1.LeftoffsetForDrawingRect = 0;
            beepSideMenu1.LinkedProperty = null;
            beepSideMenu1.ListButtonFont = new Font("Arial", 10F);
            beepSideMenu1.ListImageSize = new Size(20, 20);
            beepSideMenu1.Location = new Point(4, 48);
            beepSideMenu1.LogoImage = "";
            beepSideMenu1.LogoSize = new Size(100, 100);
         
            beepSideMenu1.MaxHitListDrawPerFrame = 0;
            beepSideMenu1.ModernGradientType = ModernGradientType.None;
            beepSideMenu1.Name = "beepSideMenu1";
            beepSideMenu1.OverrideFontSize = TypeStyleFontSize.None;
            beepSideMenu1.Padding = new Padding(5);
            beepSideMenu1.ParentBackColor = Color.Empty;
            beepSideMenu1.ParentControl = null;
            beepSideMenu1.PressedBackColor = Color.White;
            beepSideMenu1.PressedBorderColor = Color.Gray;
            beepSideMenu1.PressedForeColor = Color.Gray;
            beepSideMenu1.RadialCenter = (PointF)resources.GetObject("beepSideMenu1.RadialCenter");
            beepSideMenu1.RightoffsetForDrawingRect = 0;
            beepSideMenu1.SavedGuidID = null;
            beepSideMenu1.SavedID = null;
            beepSideMenu1.SelectedBackColor = Color.White;
            beepSideMenu1.SelectedBorderColor = Color.Empty;
            beepSideMenu1.SelectedForeColor = Color.Black;
            beepSideMenu1.SelectedValue = null;
            beepSideMenu1.ShadowColor = Color.FromArgb(50, 0, 0, 0);
            beepSideMenu1.ShadowOffset = 0;
            beepSideMenu1.ShadowOpacity = 0.5F;
            beepSideMenu1.ShowAllBorders = false;
            beepSideMenu1.ShowBottomBorder = false;
            beepSideMenu1.ShowFocusIndicator = false;
            beepSideMenu1.ShowLeftBorder = false;
            beepSideMenu1.ShowRightBorder = false;
            beepSideMenu1.ShowShadow = false;
            beepSideMenu1.ShowTopBorder = false;
            beepSideMenu1.Size = new Size(159, 578);
            beepSideMenu1.SlideFrom = SlideDirection.Left;
            beepSideMenu1.StaticNotMoving = false;
            beepSideMenu1.TabIndex = 1;
            beepSideMenu1.Tag = this;
            beepSideMenu1.TempBackColor = Color.Empty;
            beepSideMenu1.Text = "beepSideMenu1";
            beepSideMenu1.Theme = "DefaultType";
            beepSideMenu1.Title = "Beep Form";
            beepSideMenu1.TitleSize = new Size(139, 35);
            beepSideMenu1.ToolTipText = "";
            beepSideMenu1.TopoffsetForDrawingRect = 0;
           
            beepSideMenu1.UseExternalBufferedGraphics = false;
            beepSideMenu1.UseGlassmorphism = false;
            beepSideMenu1.UseGradientBackground = false;
            beepSideMenu1.UseThemeFont = true;
            // 
            // beepDisplayContainer1
            // 
            beepDisplayContainer1.AnimationDuration = 500;
            beepDisplayContainer1.AnimationType = DisplayAnimationType.None;
            beepDisplayContainer1.ApplyThemeToChilds = false;
            beepDisplayContainer1.AutoDrawHitListComponents = true;
            beepDisplayContainer1.BackColor = Color.FromArgb(248, 250, 255);
            beepDisplayContainer1.BadgeBackColor = Color.FromArgb(92, 139, 255);
            beepDisplayContainer1.BadgeFont = new Font("Arial", 8F, FontStyle.Bold);
            beepDisplayContainer1.BadgeForeColor = Color.FromArgb(58, 58, 58);
            beepDisplayContainer1.BadgeShape = BadgeShape.Circle;
            beepDisplayContainer1.BadgeText = "";
            beepDisplayContainer1.BlockID = null;
            beepDisplayContainer1.BorderColor = Color.FromArgb(209, 213, 219);
            beepDisplayContainer1.BorderDashStyle = DashStyle.Solid;
            beepDisplayContainer1.BorderPainter = BeepControlStyle.Modern;
            graphicsPath1.FillMode = FillMode.Alternate;
            beepDisplayContainer1.BorderPath = graphicsPath1;
            beepDisplayContainer1.BorderRadius = 8;
            beepDisplayContainer1.BorderRect = new Rectangle(0, 0, 1, 1);
            beepDisplayContainer1.BorderStyle = BorderStyle.FixedSingle;
            beepDisplayContainer1.BorderThickness = 2;
            beepDisplayContainer1.BottomoffsetForDrawingRect = 0;
            beepDisplayContainer1.BoundProperty = null;
            beepDisplayContainer1.CanBeFocused = true;
            beepDisplayContainer1.CanBeHovered = false;
            beepDisplayContainer1.CanBePressed = true;
            beepDisplayContainer1.CanBeSelected = true;
            beepDisplayContainer1.Category = DbFieldCategory.String;
            beepDisplayContainer1.ComponentName = "BeepDisplayContainer";
            beepDisplayContainer1.ContainerType = ContainerTypeEnum.TabbedPanel;
            beepDisplayContainer1.ContentRect = new Rectangle(0, 0, 0, 0);
            graphicsPath2.FillMode = FillMode.Alternate;
            beepDisplayContainer1.ContentShape = graphicsPath2;
            beepDisplayContainer1.ControlStyle = BeepControlStyle.Modern;
            beepDisplayContainer1.CustomPadding = new Padding(0);
            beepDisplayContainer1.DataContext = null;
            beepDisplayContainer1.DataSourceProperty = null;
            beepDisplayContainer1.DisabledBackColor = Color.FromArgb(244, 246, 252);
            beepDisplayContainer1.DisabledBorderColor = Color.FromArgb(220, 227, 240);
            beepDisplayContainer1.DisabledForeColor = Color.FromArgb(222, 223, 229);
            beepDisplayContainer1.Dock = DockStyle.Fill;
            beepDisplayContainer1.DrawingRect = new Rectangle(0, 0, 637, 578);
            beepDisplayContainer1.Easing = EasingType.Linear;
            beepDisplayContainer1.EnableHighQualityRendering = true;
            beepDisplayContainer1.EnableRippleEffect = true;
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
            beepDisplayContainer1.ForeColor = Color.FromArgb(38, 44, 57);
            beepDisplayContainer1.Form = null;
            beepDisplayContainer1.GlassmorphismBlur = 10F;
            beepDisplayContainer1.GlassmorphismOpacity = 0.1F;
            beepDisplayContainer1.GradientAngle = 0F;
            beepDisplayContainer1.GradientDirection = LinearGradientMode.Horizontal;
            beepDisplayContainer1.GradientEndColor = Color.FromArgb(242, 245, 252);
            beepDisplayContainer1.GradientStartColor = Color.FromArgb(251, 252, 255);
            beepDisplayContainer1.GridMode = false;
            beepDisplayContainer1.GuidID = "19bcd2cb-ed98-4f05-af5c-c537a78e2c46";
            beepDisplayContainer1.HasError = false;
            beepDisplayContainer1.HelperText = "";
            beepDisplayContainer1.HelperTextOn = false;
            beepDisplayContainer1.HitAreaEventOn = false;
            beepDisplayContainer1.HitTestControl = null;
            beepDisplayContainer1.HoverBackColor = Color.Wheat;
            beepDisplayContainer1.HoverBorderColor = Color.Gray;
            beepDisplayContainer1.HoveredBackcolor = Color.Wheat;
            beepDisplayContainer1.HoverForeColor = Color.Black;
            beepDisplayContainer1.IconKey = "";
            beepDisplayContainer1.IconSize = 20;
            beepDisplayContainer1.Id = -1;
            beepDisplayContainer1.InactiveBorderColor = Color.Gray;
            graphicsPath3.FillMode = FillMode.Alternate;
            beepDisplayContainer1.InnerShape = graphicsPath3;
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
            beepDisplayContainer1.LabelPosition = LabelPosition.Left;
            beepDisplayContainer1.LabelText = "";
            beepDisplayContainer1.LabelTextOn = false;
            beepDisplayContainer1.LeadingIconPath = "";
            beepDisplayContainer1.LeadingImagePath = "";
            beepDisplayContainer1.LeftoffsetForDrawingRect = 0;
            beepDisplayContainer1.LinkedProperty = null;
            beepDisplayContainer1.Location = new Point(163, 48);
            beepDisplayContainer1.MaxHitListDrawPerFrame = 0;
            beepDisplayContainer1.ModernGradientType = ModernGradientType.None;
            beepDisplayContainer1.Name = "beepDisplayContainer1";
            beepDisplayContainer1.OuterShape = null;
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
            beepDisplayContainer1.ShadowColor = Color.FromArgb(40, 38, 44, 57);
            beepDisplayContainer1.ShadowOffset = 0;
            beepDisplayContainer1.ShadowOpacity = 0.5F;
            beepDisplayContainer1.ShowAllBorders = false;
            beepDisplayContainer1.ShowBottomBorder = false;
            beepDisplayContainer1.ShowFocusIndicator = false;
            beepDisplayContainer1.ShowLabelAboveBorder = false;
            beepDisplayContainer1.ShowLeftBorder = false;
            beepDisplayContainer1.ShowRightBorder = false;
            beepDisplayContainer1.ShowShadow = false;
            beepDisplayContainer1.ShowTopBorder = false;
            beepDisplayContainer1.Size = new Size(637, 578);
            beepDisplayContainer1.SlideFrom = SlideDirection.Left;
            beepDisplayContainer1.StaticNotMoving = false;
            beepDisplayContainer1.TabIndex = 2;
            beepDisplayContainer1.TabStripGradientEndColor = Color.Empty;
            beepDisplayContainer1.Tag = this;
            beepDisplayContainer1.TempBackColor = Color.Empty;
            beepDisplayContainer1.Text = "beepDisplayContainer1";
            beepDisplayContainer1.TextFont = new Font("Segoe UI", 8F);
            beepDisplayContainer1.Theme = "ModernTheme";
            beepDisplayContainer1.TooltipFont = null;
            beepDisplayContainer1.TooltipMaxSize = null;
            beepDisplayContainer1.ToolTipText = "";
            beepDisplayContainer1.TopoffsetForDrawingRect = 0;
            beepDisplayContainer1.TrailingIconPath = "";
            beepDisplayContainer1.TrailingImagePath = "";
            beepDisplayContainer1.UseExternalBufferedGraphics = false;
            beepDisplayContainer1.UseFormStylePaint = true;
            beepDisplayContainer1.UseGlassmorphism = false;
            beepDisplayContainer1.UseGradientBackground = false;
            beepDisplayContainer1.UseRichToolTip = true;
            beepDisplayContainer1.UseThemeFont = false;
            // 
            // MainFrm_SideBar
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BorderColor = Color.FromArgb(200, 200, 200);
            ClientSize = new Size(804, 630);
            Controls.Add(beepDisplayContainer1);
            Controls.Add(beepSideMenu1);
            Name = "MainFrm_SideBar";
            Text = "MainFrm_SideBar";
            ResumeLayout(false);
        }

        #endregion
        private BeepDisplayContainer2 beepDisplayContainer1;
        private BeepSideMenu beepSideMenu1;
    }
}