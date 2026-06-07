using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

using TheTechIdea.Beep.Winform.Controls;
using TheTechIdea.Beep.Winform.Controls.Layouts.Helpers;
namespace TheTechIdea.Beep.Winform.Controls.Integrated.Forms.Logon
{
    public class BeepLogonDialog : Form, IBeepLogonDialog
    {
        private readonly BeepDataConnection? _dataConnection;
        private readonly List<ConnectionProperties> _connections;

        private BeepLogin _login = null!;
        private ComboBox _connectionCombo = null!;
        private Label _connectionLabel = null!;
        private Button _okButton = null!;
        private Button _cancelButton = null!;
        private Button _testButton = null!;
        private Label _statusLabel = null!;
        private TableLayoutPanel _layout = null!;

        public event EventHandler<BeepLogonContext>? LoggedIn;
        public event EventHandler<BeepLogonEventArgs>? OnLogin;

        public BeepLogonContext? LastContext { get; private set; }

        /// <summary>
        /// Underlying <c>BeepLogin</c> control. Exposed for advanced
        /// subscribers that want to wire social-login handlers, theming
        /// callbacks, or other <c>BeepLogin</c>-level events.
        /// </summary>
        public BeepLogin LoginControl => _login;

        public BeepLogonDialog()
            : this(null)
        {
        }

        public BeepLogonDialog(BeepDataConnection? dataConnection)
        {
            _dataConnection = dataConnection;
            _connections = dataConnection?.GetConnectionsSnapshot(includeRepository: true)?.ToList()
                          ?? new List<ConnectionProperties>();

            InitializeComponent();
            PopulateConnections();
        }

        public Task<BeepLogonContext> PromptAsync(BeepLogonRequest request)
        {
            ApplyRequest(request);
            var tcs = new TaskCompletionSource<BeepLogonContext>(TaskCreationOptions.RunContinuationsAsynchronously);

            EventHandler<BeepLogonContext>? handler = null;
            handler = (_, ctx) =>
            {
                LoggedIn -= handler;
                tcs.TrySetResult(ctx);
            };
            LoggedIn += handler;

            FormClosed += (_, _) =>
            {
                LoggedIn -= handler;
                if (!tcs.Task.IsCompleted)
                {
                    tcs.TrySetResult(new BeepLogonContext
                    {
                        Request = request,
                        Result = BeepLogonResult.Cancelled,
                        CompletedAtUtc = DateTime.UtcNow
                    });
                }
            };

            try
            {
                if (ShowDialog() == DialogResult.OK)
                {
                    var ctx = BuildContext(request, BeepLogonResult.Success);
                    LastContext = ctx;
                    LoggedIn?.Invoke(this, ctx);
                    return Task.FromResult(ctx);
                }

                var cancelled = BuildContext(request, BeepLogonResult.Cancelled);
                LastContext = cancelled;
                return Task.FromResult(cancelled);
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"[BeepLogonDialog.PromptAsync] {ex.GetType().Name}: {ex.Message}");
                var failed = BuildContext(request, BeepLogonResult.Failed, ex.Message);
                LastContext = failed;
                return Task.FromResult(failed);
            }
        }

        private void ApplyRequest(BeepLogonRequest request)
        {
            if (request == null)
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(request.Title))
            {
                Text = request.Title;
            }

            _login.Username = request.UserName ?? string.Empty;
            _login.Password = request.Password ?? string.Empty;
            _login.RememberMe = request.RememberPassword;

            if (!string.IsNullOrWhiteSpace(request.ConnectionName))
            {
                int idx = _connections.FindIndex(c =>
                    string.Equals(c.ConnectionName, request.ConnectionName, StringComparison.OrdinalIgnoreCase));
                if (idx >= 0)
                {
                    _connectionCombo.SelectedIndex = idx;
                }
            }

            _connectionCombo.Enabled = request.AllowConnectionSwitch && _connections.Count > 1;
        }

        private void PopulateConnections()
        {
            _connectionCombo.Items.Clear();
            foreach (var c in _connections)
            {
                _connectionCombo.Items.Add(c.ConnectionName);
            }

            if (_connectionCombo.Items.Count > 0)
            {
                _connectionCombo.SelectedIndex = 0;
            }

            _connectionCombo.Enabled = _connections.Count > 1;
        }

        private BeepLogonContext BuildContext(BeepLogonRequest request, BeepLogonResult result, string? reason = null)
        {
            return new BeepLogonContext
            {
                Request = new BeepLogonRequest
                {
                    ConnectionName = _connectionCombo.SelectedItem?.ToString() ?? request.ConnectionName,
                    UserName = _login.Username,
                    Password = _login.Password,
                    RememberPassword = _login.RememberMe,
                    AllowConnectionSwitch = request.AllowConnectionSwitch,
                    Title = request.Title,
                    Prompt = request.Prompt
                },
                Result = result,
                FailureReason = reason,
                CompletedAtUtc = DateTime.UtcNow
            };
        }

        private void TestButton_Click(object? sender, EventArgs e)
        {
            string name = _connectionCombo.SelectedItem?.ToString() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(name))
            {
                _statusLabel.Text = "Select a connection first.";
                return;
            }

            var match = _connections.FirstOrDefault(c =>
                string.Equals(c.ConnectionName, name, StringComparison.OrdinalIgnoreCase));
            if (match == null)
            {
                _statusLabel.Text = $"Connection '{name}' not found.";
                return;
            }

            _statusLabel.Text = $"Ready: {match.ConnectionName} ({match.DatabaseType})";
        }

        private void OkButton_Click(object? sender, EventArgs e)
        {
            string name = _connectionCombo.SelectedItem?.ToString() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(name))
            {
                _statusLabel.Text = "Select a connection.";
                DialogResult = DialogResult.None;
                return;
            }

            if (string.IsNullOrWhiteSpace(_login.Username))
            {
                _statusLabel.Text = "Enter a user name.";
                DialogResult = DialogResult.None;
                return;
            }

            DialogResult = DialogResult.OK;
        }

        private void LoginControl_LoginClick(object? sender, EventArgs e)
        {
            string name = _connectionCombo.SelectedItem?.ToString() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(name))
            {
                _statusLabel.Text = "Select a connection.";
                return;
            }

            if (string.IsNullOrWhiteSpace(_login.Username))
            {
                _statusLabel.Text = "Enter a user name.";
                return;
            }

            var candidate = new BeepLogonContext
            {
                Request = new BeepLogonRequest
                {
                    ConnectionName = name,
                    UserName = _login.Username,
                    Password = _login.Password,
                    RememberPassword = _login.RememberMe,
                    AllowConnectionSwitch = _connectionCombo.Enabled,
                    Title = Text
                },
                Result = BeepLogonResult.Unknown,
                CompletedAtUtc = DateTime.MinValue
            };

            var args = new BeepLogonEventArgs(candidate);
            OnLogin?.Invoke(this, args);
            if (args.Cancel)
            {
                _statusLabel.Text = "Login cancelled by handler.";
                return;
            }

            DialogResult = DialogResult.OK;
        }

        private void InitializeComponent()
        {
            SuspendLayout();

            Text = "Connect";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MinimizeBox = false;
            MaximizeBox = false;
            ShowInTaskbar = false;
            ClientSize = BeepLayoutMetrics.DialogMedium.ScaleSize(this);
            Font = new Font("Segoe UI", BeepLayoutMetrics.BodyFontSize);

            _layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 4,
                Padding = BeepLayoutMetrics.DialogPadding.ScalePadding(this)
            };
            _layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100F));
            _layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            _layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            _layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            _layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            _layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            _connectionLabel = new Label
            {
                Text = "Connection:",
                AutoSize = true,
                Anchor = AnchorStyles.Left,
                Margin = new Padding(0, BeepLayoutMetrics.ButtonGap.ScaleValue(this), 0, 0)
            };
            _connectionCombo = new ComboBox
            {
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Margin = new Padding(0, 4, 0, 4)
            };

            _login = new BeepLogin
            {
                Dock = DockStyle.Fill,
                ViewType = LoginViewType.Simple
            };
            _login.LoginClick += LoginControl_LoginClick;

            _statusLabel = new Label
            {
                Text = string.Empty,
                AutoSize = true,
                ForeColor = SystemColors.ControlDarkDark,
                Margin = new Padding(0, 4, 0, 0)
            };

            var buttonRow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                AutoSize = true,
                Margin = new Padding(0, BeepLayoutMetrics.ButtonGap.ScaleValue(this), 0, 0)
            };

            _okButton = new Button { Text = "OK", Width = BeepLayoutMetrics.ButtonSmall.ScaleSize(this).Width, Margin = new Padding(BeepLayoutMetrics.SmallGap.ScaleValue(this), 0, 0, 0) };
            _okButton.Click += OkButton_Click;

            _cancelButton = new Button { Text = "Cancel", Width = BeepLayoutMetrics.ButtonSmall.ScaleSize(this).Width, Margin = new Padding(BeepLayoutMetrics.SmallGap.ScaleValue(this), 0, 0, 0), DialogResult = DialogResult.Cancel };

            _testButton = new Button { Text = "Test", Width = BeepLayoutMetrics.ButtonSmall.ScaleSize(this).Width, Margin = new Padding(BeepLayoutMetrics.SmallGap.ScaleValue(this), 0, BeepLayoutMetrics.SmallGap.ScaleValue(this), 0) };
            _testButton.Click += TestButton_Click;

            buttonRow.Controls.Add(_okButton);
            buttonRow.Controls.Add(_cancelButton);
            buttonRow.Controls.Add(_testButton);

            _layout.Controls.Add(_connectionLabel, 0, 0);
            _layout.Controls.Add(_connectionCombo, 1, 0);
            _layout.SetColumnSpan(_login, 2);
            _layout.Controls.Add(_login, 0, 1);
            _layout.SetColumnSpan(_statusLabel, 2);
            _layout.Controls.Add(_statusLabel, 0, 2);
            _layout.SetColumnSpan(buttonRow, 2);
            _layout.Controls.Add(buttonRow, 0, 3);

            AcceptButton = _okButton;
            CancelButton = _cancelButton;

            Controls.Add(_layout);

            ResumeLayout(false);
        }
    }
}
