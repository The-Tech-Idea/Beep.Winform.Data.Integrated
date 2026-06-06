using System;
using System.Drawing;
using System.Windows.Forms;
using TheTechIdea.Beep.Winform.Controls;

namespace TheTechIdea.Beep.Winform.Controls.Integrated.Forms.Logon
{
    public class SimpleLoginForm : Form
    {
        private BeepTextBox _txtUsername = null!;
        private BeepTextBox _txtPassword = null!;
        private BeepLabel _lblError = null!;
        private BeepLabel _lblTitle = null!;
        private BeepLabel _lblSubtitle = null!;
        private BeepButton _btnLogin = null!;
        private BeepButton _btnCancel = null!;
        private TableLayoutPanel _layout = null!;

        public string? Username => _txtUsername?.Text;
        public string? Password => _txtPassword?.Text;

        public string Title
        {
            get => _lblTitle?.Text ?? "Login";
            set { if (_lblTitle != null) _lblTitle.Text = value; }
        }

        public string Subtitle
        {
            get => _lblSubtitle?.Text ?? string.Empty;
            set { if (_lblSubtitle != null) { _lblSubtitle.Text = value; _lblSubtitle.Visible = !string.IsNullOrWhiteSpace(value); } }
        }

        public event EventHandler<(string Username, string Password)>? LoginClicked;
        public event EventHandler? Cancelled;

        public SimpleLoginForm()
        {
            InitializeComponent();
            Title = "Login";
        }

        private void InitializeComponent()
        {
            Text = "Login";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Size = new Size(400, 300);

            _layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 7,
                Padding = new Padding(24)
            };
            _layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
            _layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
            _layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
            _layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));
            _layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));
            _layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
            _layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            _lblTitle = new BeepLabel
            {
                Dock = DockStyle.Fill,
                Text = "Login",
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font(FontFamily.GenericSansSerif, 16f, FontStyle.Bold),
                UseThemeColors = true
            };

            _lblSubtitle = new BeepLabel
            {
                Dock = DockStyle.Fill,
                Text = string.Empty,
                TextAlign = ContentAlignment.MiddleCenter,
                Visible = false,
                UseThemeColors = true
            };

            _lblError = new BeepLabel
            {
                Dock = DockStyle.Fill,
                Text = string.Empty,
                ForeColor = Color.Red,
                TextAlign = ContentAlignment.MiddleLeft,
                UseThemeColors = false
            };

            _txtUsername = new BeepTextBox
            {
                Dock = DockStyle.Fill,
                PlaceholderText = "Username",
                Height = 36,
                UseThemeColors = true
            };
            _txtUsername.KeyDown += (_, e) => { if (e.KeyCode == Keys.Enter) Login(); };

            _txtPassword = new BeepTextBox
            {
                Dock = DockStyle.Fill,
                PlaceholderText = "Password",
                UseSystemPasswordChar = true,
                Height = 36,
                UseThemeColors = true
            };
            _txtPassword.KeyDown += (_, e) => { if (e.KeyCode == Keys.Enter) Login(); };

            var buttonPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(0, 8, 0, 0)
            };

            _btnLogin = new BeepButton
            {
                Text = "Login",
                Width = 120,
                Height = 36,
                UseThemeColors = true
            };
            _btnLogin.Click += (_, _) => Login();

            _btnCancel = new BeepButton
            {
                Text = "Cancel",
                Width = 100,
                Height = 36,
                UseThemeColors = true
            };
            _btnCancel.Click += (_, _) => Cancel();

            buttonPanel.Controls.Add(_btnLogin);
            buttonPanel.Controls.Add(_btnCancel);

            _layout.Controls.Add(_lblTitle, 0, 0);
            _layout.Controls.Add(_lblSubtitle, 0, 1);
            _layout.Controls.Add(_lblError, 0, 2);
            _layout.Controls.Add(_txtUsername, 0, 3);
            _layout.Controls.Add(_txtPassword, 0, 4);
            _layout.Controls.Add(buttonPanel, 0, 5);

            Controls.Add(_layout);
            //AcceptButton = _btnLogin;
            //CancelButton = _btnCancel;
        }

        private void Login()
        {
            _lblError.Text = string.Empty;

            if (string.IsNullOrWhiteSpace(_txtUsername.Text))
            {
                _lblError.Text = "Username is required.";
                _txtUsername.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(_txtPassword.Text))
            {
                _lblError.Text = "Password is required.";
                _txtPassword.Focus();
                return;
            }

            _btnLogin.IsLoading = true;
            _btnLogin.Enabled = false;
            try
            {
                LoginClicked?.Invoke(this, (_txtUsername.Text, _txtPassword.Text));
            }
            finally
            {
                _btnLogin.IsLoading = false;
                _btnLogin.Enabled = true;
            }
        }

        private void Cancel()
        {
            _txtUsername.Text = string.Empty;
            _txtPassword.Text = string.Empty;
            _lblError.Text = string.Empty;
            Cancelled?.Invoke(this, EventArgs.Empty);
        }

        public void ShowError(string message)
        {
            _lblError.Text = message;
            _lblError.Visible = true;
        }

        public void ClearFields()
        {
            _txtUsername.Text = string.Empty;
            _txtPassword.Text = string.Empty;
            _lblError.Text = string.Empty;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                LoginClicked = null;
                Cancelled = null;
            }
            base.Dispose(disposing);
        }
    }
}
