using TheTechIdea.Beep.Editor.Forms.Models;
using TheTechIdea.Beep.Winform.Data.Integrated.Forms.FieldHost;
using TheTechIdea.Beep.Winform.Data.Integrated.Forms.FeatureControls;
using TheTechIdea.Beep.Editor.UOWManager.Helpers;

namespace TheTechIdea.Beep.Winform.Data.Integrated.Forms.BlockHost;

public partial class WinFormBlockHost
{
    public async Task<bool> ExecuteFormsCommandAsync(
        KeyTriggerType command,
        CancellationToken cancellationToken = default)
    {
        if (_formsHost is null)
            return false;

        cancellationToken.ThrowIfCancellationRequested();
        var triggerResult = await _formsHost.FireKeyTriggerAsync(
            command,
            ManagerBlockName);
        if (triggerResult is not TriggerResult.Success and
            not TriggerResult.Skipped)
        {
            return false;
        }

        cancellationToken.ThrowIfCancellationRequested();
        return command switch
        {
            KeyTriggerType.ExecuteQuery =>
                await ExecuteQueryAsync(cancellationToken),
            KeyTriggerType.Commit =>
                await SaveAsync(),
            KeyTriggerType.Rollback =>
                await RollbackAsync(),
            KeyTriggerType.CreateRecord =>
                await InsertRecordAsync(),
            KeyTriggerType.DeleteRecord =>
                await DeleteCurrentRecordAsync(),
            KeyTriggerType.DuplicateRecord =>
                await ExecuteAndSyncAsync(host =>
                    host.DuplicateCurrentRecordAsync(
                        ManagerBlockName,
                        cancellationToken)),
            KeyTriggerType.ClearBlock or KeyTriggerType.ClearForm =>
                await ClearAsync(cancellationToken),
            KeyTriggerType.ClearRecord =>
                await ClearRecordAsync(cancellationToken),
            KeyTriggerType.NextRecord or KeyTriggerType.Down =>
                await NavigateNextAsync(),
            KeyTriggerType.PreviousRecord or KeyTriggerType.Up =>
                await NavigatePreviousAsync(),
            KeyTriggerType.NextItem =>
                FocusAdjacentItem(_formsHost.GetNextItem(
                    ManagerBlockName,
                    ActiveFieldName ?? string.Empty)),
            KeyTriggerType.PreviousItem =>
                FocusAdjacentItem(_formsHost.GetPreviousItem(
                    ManagerBlockName,
                    ActiveFieldName ?? string.Empty)),
            KeyTriggerType.F7 => EnterQueryModeFromCommand(),
            KeyTriggerType.ListValues =>
                ActiveFieldName is not null &&
                await ShowLovAsync(
                    ActiveFieldName,
                    cancellationToken: cancellationToken),
            _ => true,
        };
    }

    public async Task<bool> HandleFormsShortcutAsync(
        Keys keyData,
        CancellationToken cancellationToken = default)
    {
        var command = ResolveFormsShortcut(keyData);
        return command.HasValue &&
            await ExecuteFormsCommandAsync(
                command.Value,
                cancellationToken);
    }

    private static KeyTriggerType? ResolveFormsShortcut(Keys keyData) =>
        keyData switch
        {
            Keys.F4 => KeyTriggerType.DuplicateRecord,
            Keys.F6 => KeyTriggerType.CreateRecord,
            Keys.Shift | Keys.F6 => KeyTriggerType.DeleteRecord,
            Keys.F7 => KeyTriggerType.F7,
            Keys.F8 => KeyTriggerType.ExecuteQuery,
            Keys.F9 => KeyTriggerType.ListValues,
            Keys.F10 => KeyTriggerType.Commit,
            Keys.Control | Keys.R => KeyTriggerType.Rollback,
            Keys.Control | Keys.Delete => KeyTriggerType.ClearRecord,
            Keys.Control | Keys.Down => KeyTriggerType.NextRecord,
            Keys.Control | Keys.Up => KeyTriggerType.PreviousRecord,
            _ => null,
        };

    private bool EnterQueryModeFromCommand()
    {
        EnterQueryMode();
        return _queryMode;
    }

    private bool FocusAdjacentItem(string? fieldName) =>
        !string.IsNullOrWhiteSpace(fieldName) && FocusField(fieldName);

    public async Task<bool> ShowLovAsync(
        string fieldName,
        string? searchText = null,
        CancellationToken cancellationToken = default)
    {
        if (_formsHost is null ||
            string.IsNullOrWhiteSpace(fieldName) ||
            !_formsHost.HasLov(ManagerBlockName, fieldName))
        {
            return false;
        }

        cancellationToken.ThrowIfCancellationRequested();
        using var dialog = new WinFormLovDialog(
            _formsHost,
            ManagerBlockName,
            fieldName);
        var result = await dialog.LoadRecordsAsync(
            searchText,
            cancellationToken);
        if (!result.Success)
            return false;

        cancellationToken.ThrowIfCancellationRequested();
        if (LovDialogPresenter(dialog) != DialogResult.OK ||
            dialog.SelectedRecord is null)
        {
            return false;
        }

        var applied = await dialog.ApplySelectionAsync(cancellationToken);
        if (applied)
        {
            SyncFromManager();
            var definition = _formsHost.GetLov(
                ManagerBlockName,
                fieldName);
            if (definition is not null &&
                FindFieldPresenter(fieldName) is WinFormComboFieldPresenter combo)
            {
                combo.SetLovSelection(
                    RecordPropertyAccessor.GetValue(
                        dialog.SelectedRecord,
                        definition.ReturnField ?? definition.DisplayField),
                    RecordPropertyAccessor.GetValue(
                        dialog.SelectedRecord,
                        definition.DisplayField ?? definition.ReturnField));
            }
        }
        return applied;
    }
}
