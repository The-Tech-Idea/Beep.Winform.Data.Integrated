using TheTechIdea.Beep.Editor.Forms.Models;
using TheTechIdea.Beep.Editor.UOWManager.Interfaces;
using TheTechIdea.Beep.Winform.Controls;

namespace TheTechIdea.Beep.Winform.Data.Integrated.Forms.FeatureControls;

/// <summary>
/// Beep-control implementation of the Forms engine alert provider.
/// Inject this provider when constructing FormsManager.
/// </summary>
public sealed class WinFormAlertProvider(
    Func<IWin32Window?>? ownerProvider = null) : IAlertProvider
{
    public Task<AlertResult> ShowAlertAsync(
        string title,
        string message,
        AlertStyle style = AlertStyle.None,
        string button1Text = "OK",
        string? button2Text = null,
        string? button3Text = null,
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
        {
            return Task.FromResult(AlertResult.None);
        }

        var owner = ownerProvider?.Invoke();
        if (owner is Control control &&
            control.InvokeRequired)
        {
            var completion = new TaskCompletionSource<AlertResult>(
                TaskCreationOptions.RunContinuationsAsynchronously);
            control.BeginInvoke(new Action(async () =>
            {
                try
                {
                    completion.TrySetResult(await ShowCoreAsync(
                        owner, title, message, style,
                        button1Text, button2Text, button3Text, ct));
                }
                catch (Exception ex)
                {
                    completion.TrySetException(ex);
                }
            }));
            return completion.Task;
        }

        return ShowCoreAsync(
            owner, title, message, style,
            button1Text, button2Text, button3Text, ct);
    }

    private static Task<AlertResult> ShowCoreAsync(
        IWin32Window? owner,
        string title,
        string message,
        AlertStyle style,
        string button1Text,
        string? button2Text,
        string? button3Text,
        CancellationToken ct)
    {
        using var dialog = new Form
        {
            Text = title,
            Width = 480,
            Height = 220,
            MinimizeBox = false,
            MaximizeBox = false,
            ShowInTaskbar = false,
            StartPosition = FormStartPosition.CenterParent,
            FormBorderStyle = FormBorderStyle.FixedDialog
        };
        var messageLabel = new BeepLabel
        {
            Text = message,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            UseThemeColors = true
        };
        var actions = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 52,
            FlowDirection = FlowDirection.RightToLeft,
            Padding = new Padding(8)
        };
        AddButton(actions, dialog, button3Text, AlertResult.Button3);
        AddButton(actions, dialog, button2Text, AlertResult.Button2);
        AddButton(actions, dialog, button1Text, AlertResult.Button1);
        dialog.Controls.Add(messageLabel);
        dialog.Controls.Add(actions);
        dialog.Tag = AlertResult.None;

        using var registration = ct.Register(() =>
        {
            if (!dialog.IsDisposed)
            {
                dialog.BeginInvoke(new Action(dialog.Close));
            }
        });
        if (owner is null)
        {
            dialog.ShowDialog();
        }
        else
        {
            dialog.ShowDialog(owner);
        }

        return Task.FromResult(
            dialog.Tag is AlertResult result
                ? result
                : AlertResult.None);
    }

    private static void AddButton(
        Control parent,
        Form dialog,
        string? text,
        AlertResult result)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        var button = new BeepButton
        {
            Text = text,
            Width = 100,
            Height = 34,
            UseThemeColors = true
        };
        button.Click += (_, _) =>
        {
            dialog.Tag = result;
            dialog.Close();
        };
        parent.Controls.Add(button);
    }
}
