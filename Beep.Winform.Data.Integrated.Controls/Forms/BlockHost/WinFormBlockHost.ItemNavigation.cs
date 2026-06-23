using TheTechIdea.Beep.Editor.Forms.Hosts;
using TheTechIdea.Beep.Editor.Forms.Models;
using TheTechIdea.Beep.Editor.UOWManager.Models;

namespace TheTechIdea.Beep.Winform.Data.Integrated.Forms.BlockHost;

public partial class WinFormBlockHost
{
    private void ConnectItemNavigation(Control control)
    {
        control.Enter -= EditorOnEnter;
        control.Enter += EditorOnEnter;
        control.KeyDown -= EditorOnKeyDown;
        control.KeyDown += EditorOnKeyDown;
    }

    private void DisconnectItemNavigation(Control control)
    {
        control.Enter -= EditorOnEnter;
        control.KeyDown -= EditorOnKeyDown;
    }

    private void EditorOnEnter(object? sender, EventArgs e)
    {
        var presenter = _presenters.FirstOrDefault(
            candidate => ReferenceEquals(candidate.View, sender));
        if (presenter is not null)
            ActiveFieldName = presenter.FieldName;
    }

    private async void EditorOnKeyDown(object? sender, KeyEventArgs e)
    {
        var keyData = e.KeyCode | e.Modifiers;
        if (ResolveFormsShortcut(keyData).HasValue)
        {
            e.Handled = true;
            e.SuppressKeyPress = true;
            try
            {
                await HandleFormsShortcutAsync(keyData);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception exception)
            {
                _formsHost?.ShowError(exception.Message);
            }
            return;
        }

        if (e.KeyCode is not Keys.Tab and not Keys.Enter)
            return;
        var presenter = _presenters.FirstOrDefault(
            candidate => ReferenceEquals(candidate.View, sender));
        if (presenter is null)
            return;

        e.Handled = true;
        e.SuppressKeyPress = true;
        if (e.Shift)
            await MoveToPreviousItemAsync(presenter.FieldName);
        else
            await MoveToNextItemAsync(presenter.FieldName);
    }

    public Task<bool> MoveToNextItemAsync(string currentFieldName) =>
        MoveToAdjacentItemAsync(
            currentFieldName,
            KeyTriggerType.NextItem,
            host => host.GetNextItem(ManagerBlockName, currentFieldName));

    public Task<bool> MoveToPreviousItemAsync(string currentFieldName) =>
        MoveToAdjacentItemAsync(
            currentFieldName,
            KeyTriggerType.PreviousItem,
            host => host.GetPreviousItem(ManagerBlockName, currentFieldName));

    private async Task<bool> MoveToAdjacentItemAsync(
        string currentFieldName,
        KeyTriggerType keyTrigger,
        Func<IBeepFormsHost, string?> resolveTarget)
    {
        if (_formsHost is null || string.IsNullOrWhiteSpace(currentFieldName))
            return false;

        if (!await ValidateItemExitAsync(currentFieldName))
            return false;
        if (!await FireItemTriggerAsync(
                TriggerType.PostTextItem,
                currentFieldName))
        {
            return false;
        }

        var triggerResult = await _formsHost.FireKeyTriggerAsync(
            keyTrigger,
            ManagerBlockName);
        if (triggerResult is not TriggerResult.Success and
            not TriggerResult.Skipped)
        {
            return false;
        }

        var targetField = resolveTarget(_formsHost);
        if (string.IsNullOrWhiteSpace(targetField))
            return false;
        if (!await FireItemTriggerAsync(TriggerType.PreTextItem, targetField))
            return false;
        if (!await FireItemTriggerAsync(
                TriggerType.WhenNewItemInstance,
                targetField))
        {
            return false;
        }

        return FocusField(targetField);
    }

    public async Task<bool> ValidateItemExitAsync(
        string fieldName,
        CancellationToken cancellationToken = default)
    {
        if (_formsHost is null)
            return false;
        var presenter = FindFieldPresenter(fieldName);
        if (presenter is null)
            return false;

        cancellationToken.ThrowIfCancellationRequested();
        if (!presenter.Validate())
        {
            FocusField(fieldName);
            return false;
        }

        var validation = _formsHost.ValidateItem(
            ManagerBlockName,
            presenter.FieldName,
            presenter.Value,
            ValidationTiming.OnBlur);
        if (validation is not null && !validation.IsValid)
        {
            presenter.ValidationError =
                validation.FirstError ?? $"{presenter.Label} is invalid.";
            FocusField(fieldName);
            return false;
        }

        var triggerResult = await _formsHost.FireBlockTriggerAsync(
            TriggerType.WhenValidateItem,
            ManagerBlockName,
            TriggerContext.ForValidation(
                ManagerBlockName,
                presenter.FieldName,
                presenter.Value),
            cancellationToken);
        if (triggerResult is not TriggerResult.Success and
            not TriggerResult.Skipped)
        {
            FocusField(fieldName);
            return false;
        }

        presenter.ValidationError = null;
        return true;
    }

    private async Task<bool> FireItemTriggerAsync(
        TriggerType type,
        string fieldName,
        CancellationToken cancellationToken = default)
    {
        if (_formsHost is null)
            return false;
        var presenter = FindFieldPresenter(fieldName);
        if (presenter is null)
            return false;

        var result = await _formsHost.FireBlockTriggerAsync(
            type,
            ManagerBlockName,
            TriggerContext.ForItem(
                type,
                ManagerBlockName,
                presenter.FieldName,
                presenter.Value,
                presenter.Value),
            cancellationToken);
        return result is TriggerResult.Success or TriggerResult.Skipped;
    }
}
