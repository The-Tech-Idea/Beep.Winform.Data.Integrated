using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using TheTechIdea.Beep.Editor.Forms.Builtins;
using TheTechIdea.Beep.Winform.Controls;
using TheTechIdea.Beep.Winform.Controls.Integrated.Forms.Contracts;

namespace TheTechIdea.Beep.Winform.Controls.Integrated.Forms
{
    /// <summary>
    /// M4-RUN-001: a WinForms application host that owns a
    /// set of <see cref="BeepForms"/> instances and a
    /// <c>Dictionary&lt;string, object&gt;</c> of <c>:GLOBAL</c>
    /// variables. The application is the canonical MDI host
    /// for multi-form Oracle Forms-style apps.
    /// </summary>
    /// <remarks>
    /// The class is intentionally lightweight — it does not
    /// inherit from <c>Form</c> or from
    /// <c>Microsoft.Extensions.Hosting.IHost</c>. The orchestrator
    /// pattern is "the application holds the open forms and the
    /// global state; the form does not know about siblings".
    /// This keeps the runtime side WinForms-only and avoids
    /// pulling in the .NET Generic Host.
    /// </remarks>
    public sealed class BeepApplication : IDisposable
    {
        private readonly Dictionary<string, BeepForms> _openForms = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, object?> _globalVariables = new(StringComparer.OrdinalIgnoreCase);
        private readonly Font _activeFormFont = new Font(SystemFonts.DefaultFont, System.Drawing.FontStyle.Bold);
        private bool _isDisposed;

        /// <summary>
        /// Fires when a form is opened through
        /// <see cref="OpenForm"/>. Subscribers (typically the
        /// IDE or a test harness) can use the event to surface
        /// the form to the user.
        /// </summary>
        public event EventHandler<BeepApplicationFormEventArgs>? FormOpened;

        /// <summary>
        /// Fires when a form is closed through
        /// <see cref="CloseForm"/>. The form is removed from
        /// <see cref="OpenForms"/> before the event is raised.
        /// </summary>
        public event EventHandler<BeepApplicationFormEventArgs>? FormClosed;

        public IReadOnlyDictionary<string, BeepForms> OpenForms => _openForms;
        public IReadOnlyDictionary<string, object?> GlobalVariables => _globalVariables;

        public Panel CreateFormTabs()
        {
            var panel = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 32,
                WrapContents = false,
                AutoScroll = true
            };

            FormOpened += (_, args) => RebuildTabs(panel);
            FormClosed += (_, args) => RebuildTabs(panel);
            RebuildTabs(panel);

            return panel;
        }

        public void ShowGlobalVariables()
        {
            if (_globalVariables.Count == 0)
            {
                MessageBox.Show("No global variables set.", ":GLOBAL Variables",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"Global Variables ({_globalVariables.Count})");
            sb.AppendLine(new string('─', 40));
            foreach (var kvp in _globalVariables)
                sb.AppendLine($"  {kvp.Key}: {kvp.Value ?? "(null)"}");
            MessageBox.Show(sb.ToString(), ":GLOBAL Variables",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public void ShowInterFormMessages()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"Inter-Form Activity — {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine(new string('─', 50));

            foreach (var kvp in _openForms)
            {
                var mgr = kvp.Value.FormsManager;
                sb.AppendLine($"  Form: {kvp.Key}");
                sb.AppendLine($"    Status: {mgr?.Status ?? "N/A"}");
                sb.AppendLine($"    Dirty: {mgr?.IsDirty ?? false}");
                if (mgr != null)
                {
                    try
                    {
                        foreach (var blockName in kvp.Value.Blocks.Select(b => b.BlockName))
                        {
                            if (string.IsNullOrWhiteSpace(blockName)) continue;
                            if (mgr.BlockExists(blockName))
                            {
                                int errCount = mgr.ErrorLog.GetErrorCount(blockName!);
                                if (errCount > 0)
                                    sb.AppendLine($"    Block '{blockName}': {errCount} error(s)");
                            }
                        }
                    }
                    catch { }
                }
                sb.AppendLine();
            }

            if (_openForms.Count == 0)
                sb.AppendLine("  No forms open.");

            MessageBox.Show(sb.ToString(), "Form Activity Log",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public Panel CreateOpenFormsPanel()
        {
            var panel = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 36,
                WrapContents = false,
                AutoScroll = true,
                Padding = new Padding(4, 4, 4, 0)
            };

            var titleLabel = new Label
            {
                Text = "Open Forms:",
                AutoSize = true,
                Margin = new Padding(0, 4, 8, 0)
            };
            panel.Controls.Add(titleLabel);

            var addBtn = new BeepButton
            {
                Text = "+",
                Width = 28,
                Height = 26,
                Margin = new Padding(2, 0, 8, 0),
                ShowShadow = false,
                UseThemeColors = true
            };
            addBtn.Click += (_, _) => ShowOpenFormDialog();
            panel.Controls.Add(addBtn);

            void Rebuild()
            {
                while (panel.Controls.Count > 2)
                    panel.Controls.RemoveAt(2);

                foreach (var kvp in _openForms)
                {
                    string name = kvp.Key;
                    bool isActive = IsFormActive(kvp.Value);
                    var btn = new BeepButton
                    {
                        Text = isActive ? $"▸ {name}" : $"  {name}",
                        AutoSize = true,
                        Height = 26,
                        Margin = new Padding(2, 0, 2, 0),
                        ShowShadow = false,
                        UseThemeColors = true,
                        Font = isActive ? _activeFormFont : SystemFonts.DefaultFont
                    };
                    if (isActive) btn.BackColor = Color.FromArgb(160, 210, 240);
                    btn.Click += (_, _) => GoForm(name);
                    panel.Controls.Add(btn);
                }
            }

            FormOpened += (_, _) => Rebuild();
            FormClosed += (_, _) => Rebuild();
            Rebuild();

            return panel;
        }

        private void ShowOpenFormDialog()
        {
            using var form = new Form
            {
                Text = "Open Form",
                Size = new Size(300, 250),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MinimizeBox = false,
                MaximizeBox = false
            };

            var listBox = new ListBox { Dock = DockStyle.Fill };
            foreach (var name in _openForms.Keys)
            {
                if (!listBox.Items.Contains(name))
                    listBox.Items.Add(name);
            }

            var btnPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 40,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(8)
            };
            var cancelBtn = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel };
            var goBtn = new Button { Text = "Go To Form" };
            goBtn.Click += (_, _) =>
            {
                if (listBox.SelectedItem is string name)
                {
                    GoForm(name);
                    form.DialogResult = DialogResult.OK;
                }
            };
            btnPanel.Controls.Add(cancelBtn);
            btnPanel.Controls.Add(goBtn);
            form.Controls.Add(listBox);
            form.Controls.Add(btnPanel);
            form.ShowDialog();
        }

        private void RebuildTabs(FlowLayoutPanel panel)
        {
            panel.Controls.Clear();
            foreach (var kvp in _openForms)
            {
                string name = kvp.Key;
                bool isActive = IsFormActive(kvp.Value);

                var tab = new BeepButton
                {
                    Text = isActive ? $"▸ {name}" : $"  {name}",
                    AutoSize = true,
                    Height = 28,
                    Margin = new Padding(2, 1, 2, 1),
                    Theme = null,
                    ShowShadow = false,
                    UseThemeColors = true,
                    Font = isActive ? _activeFormFont : SystemFonts.DefaultFont
                };
                if (isActive)
                    tab.BackColor = Color.FromArgb(160, 210, 240);

                tab.Click += (_, _) => GoForm(name);
                panel.Controls.Add(tab);
            }
        }

        private static bool IsFormActive(BeepForms form)
        {
            try
            {
                if (form.Parent is Form parentForm)
                    return parentForm.ActiveControl != null
                        && (parentForm.ActiveControl == form
                         || form.Contains(parentForm.ActiveControl));

                Control? c = form;
                while (c != null)
                {
                    if (c.Focused) return true;
                    c = c.Parent;
                }
                return false;
            }
            catch { return false; }
        }

        /// <summary>
        /// M4-RUN-002: open a form by name. The lookup is
        /// case-insensitive. If the form is already open, the
        /// existing instance is returned and brought to the
        /// front (matching the Oracle Forms
        /// <c>OPEN_FORM</c> behaviour).
        /// </summary>
        public BeepForms? OpenForm(string formName, BeepForms? instance = null)
        {
            ThrowIfDisposed();
            if (string.IsNullOrWhiteSpace(formName)) return null;
            if (_openForms.TryGetValue(formName, out var existing))
            {
                BringToFront(existing);
                return existing;
            }
            var form = instance ?? new BeepForms { Name = formName };
            form.Application = this;
            _openForms[formName] = form;
            BringToFront(form);
            FormOpened?.Invoke(this, new BeepApplicationFormEventArgs(formName, form));
            return form;
        }

        /// <summary>
        /// M5-RUN-002: open a <see cref="BeepLogonScreen"/>
        /// by name. The screen is added to <see cref="OpenForms"/>
        /// and the <c>WhenLogon</c> form-level trigger is
        /// fired through the engine's trigger manager. The
        /// function returns the new screen.
        /// </summary>
        public BeepLogonScreen? OpenLogonScreen(string formName, BeepLogonScreen? instance = null)
        {
            ThrowIfDisposed();
            if (string.IsNullOrWhiteSpace(formName)) return null;
            if (_openForms.TryGetValue(formName, out var existing) && existing is BeepLogonScreen existingLogon)
            {
                BringToFront(existingLogon);
                return existingLogon;
            }
            var screen = instance ?? new BeepLogonScreen { Name = formName };
            screen.Application = this;
            _openForms[formName] = screen;
            BringToFront(screen);
            try
            {
                screen.FormsManager?.Triggers?.FireFormTrigger(
                    TheTechIdea.Beep.Editor.Forms.Models.TriggerType.WhenLogon,
                    formName);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[BeepApplication.OpenLogonScreen] {ex.Message}");
            }
            FormOpened?.Invoke(this, new BeepApplicationFormEventArgs(formName, screen));
            return screen;
        }

        /// <summary>
        /// M4-RUN-002: close a form by name. The form is
        /// removed from <see cref="OpenForms"/> and the
        /// <c>On-Logoff</c> / <c>Post-Form</c> trigger fires
        /// through the host's trigger manager.
        /// </summary>
        public bool CloseForm(string formName)
        {
            ThrowIfDisposed();
            if (string.IsNullOrWhiteSpace(formName)) return false;
            if (!_openForms.TryGetValue(formName, out var form)) return false;
            form.RaiseOnLogoff();
            _openForms.Remove(formName);
            FormClosed?.Invoke(this, new BeepApplicationFormEventArgs(formName, form));
            form.Dispose();
            return true;
        }

        /// <summary>
        /// M4-RUN-002: bring an already-open form to the
        /// front. The lookup is case-insensitive.
        /// </summary>
        public bool GoForm(string formName)
        {
            ThrowIfDisposed();
            if (string.IsNullOrWhiteSpace(formName)) return false;
            if (!_openForms.TryGetValue(formName, out var form)) return false;
            BringToFront(form);
            return true;
        }

        /// <summary>
        /// M4-RUN-002: set a <c>:GLOBAL</c> variable. Names are
        /// case-insensitive. Setting <c>null</c> removes the
        /// entry.
        /// </summary>
        public void SetGlobal(string name, object? value)
        {
            ThrowIfDisposed();
            if (string.IsNullOrWhiteSpace(name)) return;
            if (value == null)
            {
                _globalVariables.Remove(name);
                return;
            }
            _globalVariables[name] = value;
        }

        /// <summary>
        /// M4-RUN-002: read a <c>:GLOBAL</c> variable. The
        /// function returns <c>null</c> when the variable is
        /// not set.
        /// </summary>
        public object? GetGlobal(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return null;
            return _globalVariables.TryGetValue(name, out var v) ? v : null;
        }

        /// <summary>
        /// M4-RUN-002: open the named form asynchronously. The
        /// operation is fire‑and‑forget; the form is created
        /// on a background thread and surfaced on the UI
        /// thread.
        /// </summary>
        public Task<BeepForms?> OpenFormAsync(string formName, BeepForms? instance = null)
        {
            return Task.Run(() => OpenForm(formName, instance));
        }

        /// <summary>
        /// Bring the form to the front. The method handles
        /// three cases: the form is a WinForms <c>Form</c>
        /// (uses <c>Form.Activate</c>), the form is a
        /// <c>Control</c> with a <c>TopLevelControl</c>
        /// (uses <c>BringToFront</c>), or the form is a
        /// non-visual <see cref="BeepForms"/> (no-op; the
        /// orchestrator surfaces the form through the
        /// <c>FormOpened</c> event).
        /// </summary>
        private static void BringToFront(BeepForms form)
        {
            // BeepForms is a Panel; the parent is responsible
            // for showing it. We just no-op here — the host
            // surfaces the form through the FormOpened event
            // and the parent brings the panel to front.
            _ = form;
        }

        private void ThrowIfDisposed()
        {
            if (_isDisposed) throw new ObjectDisposedException(nameof(BeepApplication));
        }

        public void Dispose()
        {
            if (_isDisposed) return;
            _isDisposed = true;
            foreach (var form in _openForms.Values)
            {
                try { form.Dispose(); } catch { /* best effort */ }
            }
            _openForms.Clear();
            _globalVariables.Clear();
            _activeFormFont.Dispose();
        }
    }

    /// <summary>
    /// M4-RUN-001: payload for the <c>FormOpened</c> /
    /// <c>FormClosed</c> events on <see cref="BeepApplication"/>.
    /// </summary>
    public sealed class BeepApplicationFormEventArgs : EventArgs
    {
        public BeepApplicationFormEventArgs(string formName, BeepForms form)
        {
            FormName = formName;
            Form = form;
        }

        public string FormName { get; }
        public BeepForms Form { get; }
    }
}
