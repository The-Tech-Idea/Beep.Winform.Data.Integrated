using System;
using System.Drawing;
using System.Windows.Forms;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Utilities;
using TheTechIdea.Beep.Winform.Controls;
using TheTechIdea.Beep.Winform.Controls.ComboBoxes;
using TheTechIdea.Beep.Winform.Controls.CheckBoxes;

namespace TheTechIdea.Beep.Winform.Controls.Integrated.Forms
{
    public class ConnectionEditorForm : Form
    {
        private IDMEEditor? _editor;
        private readonly ConnectionProperties? _existingConnection;
        private readonly bool _isEditMode;

        private TabControl _tabControl = null!;
        private TableLayoutPanel _layout = null!;
        private Panel _headerPanel = null!;
        private Panel _cardsPanel = null!;
        private FlowLayoutPanel _footerPanel = null!;

        private BeepTextBox _txtConnectionName = null!;
        private BeepComboBox _cmbCategory = null!;
        private BeepComboBox _cmbDatabaseType = null!;
        private BeepTextBox _txtHost = null!;
        private BeepTextBox _txtPort = null!;
        private BeepTextBox _txtDatabase = null!;
        private BeepTextBox _txtSchema = null!;
        private BeepCheckBoxBool _chkWindowsAuth = null!;
        private BeepCheckBoxBool _chkIntegratedSecurity = null!;
        private BeepTextBox _txtUserID = null!;
        private BeepTextBox _txtPassword = null!;
        private BeepCheckBoxBool _chkSavePassword = null!;
        private BeepTextBox _txtConnectionString = null!;
        private BeepTextBox _txtTimeout = null!;
        private BeepCheckBoxBool _chkUseSSL = null!;
        private BeepCheckBoxBool _chkTrustCert = null!;
        private BeepCheckBoxBool _chkEncrypt = null!;
        private BeepCheckBoxBool _chkReadOnly = null!;

        private BeepLabel _lblTitle = null!;
        private BeepLabel _lblStage = null!;
        private BeepLabel _lblStageSep = null!;
        private BeepLabel _lblProgress = null!;
        private BeepLabel _lblNextStep = null!;
        private BeepLabel _lblCardIdentity = null!;
        private BeepLabel _lblCardEndpoint = null!;
        private BeepLabel _lblCardSecurity = null!;
        private BeepLabel _lblCardValidation = null!;
        private BeepButton _btnTest = null!;
        private BeepButton _btnSave = null!;
        private BeepButton _btnCancel = null!;
        private BeepLabel _lblTestResult = null!;

        public ConnectionProperties? Result { get; private set; }

        public ConnectionEditorForm()
        {
            InitializeComponent();
            this.Load += (_, _) => { if (!DesignMode) LoadConnection(); };
        }

        public ConnectionEditorForm(IDMEEditor editor, ConnectionProperties? existing = null)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _existingConnection = existing;
            _isEditMode = existing != null;
            InitializeComponent();
            this.Load += (_, _) => LoadConnection();
        }

        private void InitializeComponent()
        {

            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(680, 560);
            FormBorderStyle = FormBorderStyle.Sizable;
            MinimizeBox = false;
            MaximizeBox = false;

            _layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4,
                Padding = new Padding(12)
            };
            _layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));
            _layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
            _layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            _layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));

            BuildHeader();
            BuildCards();
            BuildTabs();
            BuildFooter();

            Controls.Add(_layout);
        }

        private void BuildHeader()
        {
            _headerPanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(245, 245, 245), Padding = new Padding(10, 6, 10, 6), Margin = new Padding(0, 0, 0, 6) };

            _lblTitle = new BeepLabel { Text = Text, Font = new Font("Segoe UI", 14, FontStyle.Bold), AutoSize = true, UseThemeColors = true, Location = new Point(2, 2) };

            _lblStage = new BeepLabel { Text = "Define Connection", Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = Color.FromArgb(63, 81, 181), AutoSize = true, UseThemeColors = false, Location = new Point(2, 30) };
            var stageWidth = TextRenderer.MeasureText(_lblStage.Text, _lblStage.Font).Width;
            _lblStageSep = new BeepLabel { Text = "  •  ", Font = new Font("Segoe UI", 10), ForeColor = Color.Gray, AutoSize = true, UseThemeColors = false, Location = new Point(2 + stageWidth, 30) };
            var sepWidth = TextRenderer.MeasureText(_lblStageSep.Text, _lblStageSep.Font).Width;
            _lblProgress = new BeepLabel { Text = "Identity → Endpoint → Security → Validation → Advanced", Font = new Font("Segoe UI", 9), ForeColor = Color.Gray, AutoSize = true, UseThemeColors = false, Location = new Point(2 + stageWidth + sepWidth, 30) };

            _headerPanel.Controls.Add(_lblTitle);
            _headerPanel.Controls.Add(_lblStage);
            _headerPanel.Controls.Add(_lblStageSep);
            _headerPanel.Controls.Add(_lblProgress);

            _layout.Controls.Add(_headerPanel, 0, 0);
        }

        private void BuildCards()
        {
            _cardsPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Margin = new Padding(0, 0, 0, 6),
                Padding = new Padding(0)
            };

            _lblCardIdentity = CreateCard("Identity", "—");
            _lblCardEndpoint = CreateCard("Endpoint", "—");
            _lblCardSecurity = CreateCard("Security", "—");
            _lblCardValidation = CreateCard("Validation", "—");

            _cardsPanel.Controls.Add(WrapCardPanel(_lblCardIdentity));
            _cardsPanel.Controls.Add(WrapCardPanel(_lblCardEndpoint));
            _cardsPanel.Controls.Add(WrapCardPanel(_lblCardSecurity));
            _cardsPanel.Controls.Add(WrapCardPanel(_lblCardValidation));

            _layout.Controls.Add(_cardsPanel, 0, 1);
        }

        private BeepLabel CreateCard(string title, string value)
        {
            return new BeepLabel
            {
                Text = $"{title}\n{value}",
                Font = new Font("Segoe UI", 8),
                AutoSize = true,
                UseThemeColors = true,
                TextAlign = ContentAlignment.MiddleLeft
            };
        }

        private Panel WrapCardPanel(BeepLabel label)
        {
            var p = new Panel { Width = 155, Height = 40, BackColor = Color.FromArgb(250, 250, 250), Margin = new Padding(0, 0, 3, 0) };
            label.Dock = DockStyle.Fill;
            label.Padding = new Padding(6, 3, 6, 3);
            p.Controls.Add(label);
            return p;
        }

        private void BuildTabs()
        {
            _tabControl = new TabControl { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 0, 8) };

            _tabControl.TabPages.Add(BuildIdentityTab());
            _tabControl.TabPages.Add(BuildEndpointTab());
            _tabControl.TabPages.Add(BuildSecurityTab());
            _tabControl.TabPages.Add(BuildValidationTab());
            _tabControl.TabPages.Add(BuildAdvancedTab());

            _layout.Controls.Add(_tabControl, 0, 2);
        }

        private TabPage BuildIdentityTab()
        {
            var page = new TabPage("Identity");
            var grid = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 4, Padding = new Padding(8) };
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            for (int i = 0; i < 4; i++) grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));

            grid.Controls.Add(new Label { Text = "Connection Name:", TextAlign = ContentAlignment.MiddleRight }, 0, 0);
            _txtConnectionName = new BeepTextBox { Dock = DockStyle.Fill, UseThemeColors = true };
            grid.Controls.Add(_txtConnectionName, 1, 0);

            grid.Controls.Add(new Label { Text = "Category:", TextAlign = ContentAlignment.MiddleRight }, 0, 1);
            _cmbCategory = new BeepComboBox { Dock = DockStyle.Fill };
            foreach (var s in new[] { "RDBMS", "NoSQL", "File", "WebAPI", "InMemory" })
                _cmbCategory.ListItems.Add(new SimpleItem { Text = s, Value = s });
            grid.Controls.Add(_cmbCategory, 1, 1);

            grid.Controls.Add(new Label { Text = "Database Type:", TextAlign = ContentAlignment.MiddleRight }, 0, 2);
            _cmbDatabaseType = new BeepComboBox { Dock = DockStyle.Fill };
            foreach (var s in new[] { "SqlServer", "Oracle", "Mysql", "Postgre", "SqlLite", "MongoDB" })
                _cmbDatabaseType.ListItems.Add(new SimpleItem { Text = s, Value = s });
            grid.Controls.Add(_cmbDatabaseType, 1, 2);

            page.Controls.Add(grid);
            return page;
        }

        private TabPage BuildEndpointTab()
        {
            var page = new TabPage("Endpoint");
            var grid = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 4, Padding = new Padding(8) };
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            for (int i = 0; i < 4; i++) grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));

            grid.Controls.Add(new Label { Text = "Host:", TextAlign = ContentAlignment.MiddleRight }, 0, 0);
            _txtHost = new BeepTextBox { Dock = DockStyle.Fill, UseThemeColors = true };
            grid.Controls.Add(_txtHost, 1, 0);

            grid.Controls.Add(new Label { Text = "Port:", TextAlign = ContentAlignment.MiddleRight }, 0, 1);
            _txtPort = new BeepTextBox { Dock = DockStyle.Fill, UseThemeColors = true, Width = 80 };
            grid.Controls.Add(_txtPort, 1, 1);

            grid.Controls.Add(new Label { Text = "Database:", TextAlign = ContentAlignment.MiddleRight }, 0, 2);
            _txtDatabase = new BeepTextBox { Dock = DockStyle.Fill, UseThemeColors = true };
            grid.Controls.Add(_txtDatabase, 1, 2);

            grid.Controls.Add(new Label { Text = "Schema:", TextAlign = ContentAlignment.MiddleRight }, 0, 3);
            _txtSchema = new BeepTextBox { Dock = DockStyle.Fill, UseThemeColors = true };
            grid.Controls.Add(_txtSchema, 1, 3);

            page.Controls.Add(grid);
            return page;
        }

        private TabPage BuildSecurityTab()
        {
            var page = new TabPage("Security");
            var grid = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 5, Padding = new Padding(8) };
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            for (int i = 0; i < 5; i++) grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));

            _chkWindowsAuth = new BeepCheckBoxBool { Text = "Windows Authentication", Dock = DockStyle.Fill, CurrentValue = false };
            grid.Controls.Add(new Label(), 0, 0);
            grid.Controls.Add(_chkWindowsAuth, 1, 0);

            _chkIntegratedSecurity = new BeepCheckBoxBool { Text = "Integrated Security (SSPI)", Dock = DockStyle.Fill, CurrentValue = false };
            grid.Controls.Add(new Label(), 0, 1);
            grid.Controls.Add(_chkIntegratedSecurity, 1, 1);

            grid.Controls.Add(new Label { Text = "User ID:", TextAlign = ContentAlignment.MiddleRight }, 0, 2);
            _txtUserID = new BeepTextBox { Dock = DockStyle.Fill, UseThemeColors = true };
            grid.Controls.Add(_txtUserID, 1, 2);

            grid.Controls.Add(new Label { Text = "Password:", TextAlign = ContentAlignment.MiddleRight }, 0, 3);
            _txtPassword = new BeepTextBox { Dock = DockStyle.Fill, UseThemeColors = true, PasswordChar = '•' };
            grid.Controls.Add(_txtPassword, 1, 3);

            _chkSavePassword = new BeepCheckBoxBool { Text = "Save Password", Dock = DockStyle.Fill, CurrentValue = true };
            grid.Controls.Add(new Label(), 0, 4);
            grid.Controls.Add(_chkSavePassword, 1, 4);

            page.Controls.Add(grid);
            return page;
        }

        private TabPage BuildValidationTab()
        {
            var page = new TabPage("Validation");
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(8) };

            var lblCs = new Label { Text = "Connection String:", Font = new Font("Segoe UI", 9, FontStyle.Bold), Location = new Point(0, 4), AutoSize = true };
            _txtConnectionString = new BeepTextBox { UseThemeColors = true, Multiline = true, Height = 80, Dock = DockStyle.Top };

            var btnBuild = new Button { Text = "Build Connection String", Width = 150, Height = 28, Location = new Point(0, 112) };
            btnBuild.Click += (_, _) => _txtConnectionString.Text = BuildConnectionString();

            var btnTestTab = new Button { Text = "Test Connection", Width = 120, Height = 28, Location = new Point(158, 112), BackColor = Color.FromArgb(63, 81, 181), ForeColor = Color.White };
            btnTestTab.Click += (_, _) => TestConnection();

            _lblTestResult = new BeepLabel { Text = string.Empty, AutoSize = true, UseThemeColors = false, Location = new Point(0, 148) };

            panel.Controls.Add(lblCs);
            panel.Controls.Add(_txtConnectionString);
            panel.Controls.Add(btnBuild);
            panel.Controls.Add(btnTestTab);
            panel.Controls.Add(_lblTestResult);

            page.Controls.Add(panel);
            return page;
        }

        private TabPage BuildAdvancedTab()
        {
            var page = new TabPage("Advanced");
            var grid = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 5, Padding = new Padding(8) };
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            for (int i = 0; i < 5; i++) grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));

            grid.Controls.Add(new Label { Text = "Timeout (seconds):", TextAlign = ContentAlignment.MiddleRight }, 0, 0);
            _txtTimeout = new BeepTextBox { Dock = DockStyle.Fill, UseThemeColors = true, Width = 80, Text = "30" };
            grid.Controls.Add(_txtTimeout, 1, 0);

            _chkUseSSL = new BeepCheckBoxBool { Text = "Use SSL", Dock = DockStyle.Fill };
            grid.Controls.Add(new Label(), 0, 1);
            grid.Controls.Add(_chkUseSSL, 1, 1);

            _chkTrustCert = new BeepCheckBoxBool { Text = "Trust Server Certificate", Dock = DockStyle.Fill };
            grid.Controls.Add(new Label(), 0, 2);
            grid.Controls.Add(_chkTrustCert, 1, 2);

            _chkEncrypt = new BeepCheckBoxBool { Text = "Encrypt Connection", Dock = DockStyle.Fill };
            grid.Controls.Add(new Label(), 0, 3);
            grid.Controls.Add(_chkEncrypt, 1, 3);

            _chkReadOnly = new BeepCheckBoxBool { Text = "Read Only", Dock = DockStyle.Fill };
            grid.Controls.Add(new Label(), 0, 4);
            grid.Controls.Add(_chkReadOnly, 1, 4);

            page.Controls.Add(grid);
            return page;
        }

        private void BuildFooter()
        {
            _footerPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(0, 4, 0, 0)
            };

            _btnCancel = new BeepButton { Text = "Cancel", Width = 70, Height = 30, Margin = new Padding(8, 0, 0, 0) };
            _btnCancel.Click += (_, _) => { DialogResult = DialogResult.Cancel; Close(); };

            _btnSave = new BeepButton
            {
                Text = _isEditMode ? "Update Connection" : "Save Connection",
                Width = 140,
                Height = 30,
                UseThemeColors = true
            };
            _btnSave.Click += (_, _) => SaveConnection();

            _btnTest = new BeepButton { Text = "Test Connection", Width = 120, Height = 30, Margin = new Padding(0, 0, 8, 0) };
            _btnTest.Click += (_, _) => TestConnection();

            _lblNextStep = new BeepLabel { Text = string.Empty, Font = new Font("Segoe UI", 10), ForeColor = Color.Gray, AutoSize = true, UseThemeColors = false, Margin = new Padding(12, 6, 12, 0) };

            _footerPanel.Controls.Add(_btnCancel);
            _footerPanel.Controls.Add(_btnSave);
            _footerPanel.Controls.Add(_btnTest);
            _footerPanel.Controls.Add(_lblNextStep);

            _layout.Controls.Add(_footerPanel, 0, 3);
        }

        private void LoadConnection()
        {
            if (_isEditMode && _existingConnection != null)
            {
                _txtConnectionName.Text = _existingConnection.ConnectionName ?? "";
                _txtHost.Text = _existingConnection.Host ?? "";
                _txtPort.Text = _existingConnection.Port.ToString();
                _txtDatabase.Text = _existingConnection.Database ?? "";
                _txtSchema.Text = _existingConnection.SchemaName ?? "";
                _chkWindowsAuth.CurrentValue = _existingConnection.UseWindowsAuthentication;
                _chkIntegratedSecurity.CurrentValue = _existingConnection.IntegratedSecurity;
                _txtUserID.Text = _existingConnection.UserID ?? "";
                _txtPassword.Text = _existingConnection.Password ?? "";
                _chkSavePassword.CurrentValue = _existingConnection.SavePassword;
                _txtConnectionString.Text = _existingConnection.ConnectionString ?? "";
                _txtTimeout.Text = _existingConnection.Timeout.ToString();
                _chkUseSSL.CurrentValue = _existingConnection.UseSSL;
                _chkTrustCert.CurrentValue = _existingConnection.TrustServerCertificate;
                _chkEncrypt.CurrentValue = _existingConnection.EncryptConnection;
                _chkReadOnly.CurrentValue = _existingConnection.ReadOnly;

                SelectComboByValue(_cmbCategory, _existingConnection.Category.ToString());
                SelectComboByValue(_cmbDatabaseType, _existingConnection.DatabaseType.ToString());
            }
            else
            {
                if (_cmbCategory.ListItems.Count > 0) _cmbCategory.SelectedIndex = 0;
                if (_cmbDatabaseType.ListItems.Count > 0) _cmbDatabaseType.SelectedIndex = 0;
            }

            UpdateCards();
        }

        private void SelectComboByValue(BeepComboBox cmb, string value)
        {
            if (string.IsNullOrEmpty(value)) { if (cmb.ListItems.Count > 0) cmb.SelectedIndex = 0; return; }
            for (int i = 0; i < cmb.ListItems.Count; i++)
            {
                if (string.Equals(cmb.ListItems[i].Value?.ToString(), value, StringComparison.OrdinalIgnoreCase))
                {
                    cmb.SelectedIndex = i;
                    return;
                }
            }
            if (cmb.ListItems.Count > 0) cmb.SelectedIndex = 0;
        }

        private void UpdateCards()
        {
            var name = _txtConnectionName?.Text ?? "";
            var host = _txtHost?.Text ?? "";
            var port = _txtPort?.Text ?? "";
            var db = _txtDatabase?.Text ?? "";
            var dbType = _cmbDatabaseType?.SelectedItem?.Value?.ToString() ?? "—";

            if (_lblCardIdentity != null) _lblCardIdentity.Text = $"Identity\n{(!string.IsNullOrWhiteSpace(name) ? $"{name} | {dbType}" : "—")}";
            if (_lblCardEndpoint != null) _lblCardEndpoint.Text = $"Endpoint\n{(!string.IsNullOrWhiteSpace(host) ? $"{host}:{port}/{db}" : "—")}";
            if (_lblCardSecurity != null) _lblCardSecurity.Text = $"Security\n{(_chkWindowsAuth?.CurrentValue == true ? "Win Auth" : _chkIntegratedSecurity?.CurrentValue == true ? "SSPI" : !string.IsNullOrWhiteSpace(_txtUserID?.Text) ? $"User: {_txtUserID.Text}" : "—")}";
            if (_lblCardValidation != null) _lblCardValidation.Text = $"Validation\n{(!string.IsNullOrWhiteSpace(_txtConnectionString?.Text) ? "String ready" : "—")}";
        }

        private string GetSelectedComboText(BeepComboBox cmb) => cmb.SelectedItem?.Value?.ToString() ?? string.Empty;

        private async void TestConnection()
        {
            if (_editor == null) { _lblTestResult.Text = "❌ No editor available."; return; }
            _btnTest.Enabled = false;
            _btnTest.Text = "Testing...";
            _lblTestResult.Text = "Testing...";

            try
            {
                var cs = _txtConnectionString.Text;
                if (string.IsNullOrWhiteSpace(cs)) cs = BuildConnectionString();
                if (!string.IsNullOrWhiteSpace(cs)) _txtConnectionString.Text = cs;

                var opened = await System.Threading.Tasks.Task.Run(() => _editor.OpenDataSource(cs));
                if (IsDisposed) return;
                _lblTestResult.Text = opened != null ? "✅ Connected" : "❌ Could not open connection";
                _lblNextStep.Text = opened != null ? "✅ Connected" : "❌ Failed";
            }
            catch (Exception ex)
            {
                if (IsDisposed) return;
                _lblTestResult.Text = $"❌ {ex.Message}";
                _lblNextStep.Text = $"❌ {ex.Message}";
            }
            finally
            {
                if (!IsDisposed)
                {
                    _btnTest.Enabled = true;
                    _btnTest.Text = "Test Connection";
                    UpdateCards();
                }
            }
        }

        private void SaveConnection()
        {
            var name = _txtConnectionName.Text?.Trim() ?? "";
            var host = _txtHost.Text?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(name)) { _lblNextStep.Text = "❌ Connection name is required."; return; }
            if (string.IsNullOrWhiteSpace(host)) { _lblNextStep.Text = "❌ Host is required."; return; }

            Result = new ConnectionProperties
            {
                ConnectionName = name,
                DatabaseType = Enum.TryParse<DataSourceType>(GetSelectedComboText(_cmbDatabaseType), out var dt) ? dt : DataSourceType.SqlServer,
                Category = Enum.TryParse<DatasourceCategory>(GetSelectedComboText(_cmbCategory), out var cat) ? cat : DatasourceCategory.RDBMS,
                Host = host,
                Port = int.TryParse(_txtPort.Text, out var p) ? p : 0,
                Database = _txtDatabase.Text?.Trim() ?? "",
                SchemaName = _txtSchema.Text?.Trim() ?? "",
                UserID = _txtUserID.Text?.Trim() ?? "",
                Password = _txtPassword.Text ?? "",
                SavePassword = _chkSavePassword.CurrentValue,
                ConnectionString = !string.IsNullOrWhiteSpace(_txtConnectionString.Text) ? _txtConnectionString.Text : BuildConnectionString(),
                UseWindowsAuthentication = _chkWindowsAuth.CurrentValue,
                IntegratedSecurity = _chkIntegratedSecurity.CurrentValue,
                Timeout = int.TryParse(_txtTimeout.Text, out var t) ? t : 30,
                UseSSL = _chkUseSSL.CurrentValue,
                TrustServerCertificate = _chkTrustCert.CurrentValue,
                EncryptConnection = _chkEncrypt.CurrentValue,
                ReadOnly = _chkReadOnly.CurrentValue,
            };
            DialogResult = DialogResult.OK;
            Close();
        }

        private string BuildConnectionString()
        {
            var type = GetSelectedComboText(_cmbDatabaseType).ToLowerInvariant();
            var host = _txtHost.Text?.Trim() ?? "";
            var port = _txtPort.Text?.Trim() ?? "";
            var db = _txtDatabase.Text?.Trim() ?? "";
            var user = _txtUserID.Text?.Trim() ?? "";
            var pwd = _txtPassword.Text ?? "";
            var sspi = _chkIntegratedSecurity.CurrentValue;
            var winAuth = _chkWindowsAuth.CurrentValue;
            var ssl = _chkUseSSL.CurrentValue;
            var trust = _chkTrustCert.CurrentValue;

            return type switch
            {
                "sqlserver" => $"Server={host}{(port != "" && port != "1433" ? $",{port}" : "")};Database={db};{(sspi ? "Integrated Security=SSPI" : winAuth ? "Trusted_Connection=True" : $"User Id={user};Password={pwd}")};{(ssl ? $"Encrypt=True;TrustServerCertificate={trust.ToString().ToLower()}" : "")}",
                "oracle" => $"Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST={host})(PORT={port}))(CONNECT_DATA=(SERVICE_NAME={db})));User Id={user};Password={pwd};",
                "mysql" => $"Server={host};Port={port};Database={db};Uid={user};Pwd={pwd};",
                "postgre" => $"Host={host};Port={port};Database={db};Username={user};Password={pwd};",
                "sqllite" => $"Data Source={db};",
                _ => $"Server={host};Database={db};User Id={user};Password={pwd};",
            };
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _tabControl?.Dispose();
                _cmbCategory?.Dispose();
                _cmbDatabaseType?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
