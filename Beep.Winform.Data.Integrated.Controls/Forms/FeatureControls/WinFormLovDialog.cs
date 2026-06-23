using System.ComponentModel;
using TheTechIdea.Beep.Editor.Forms.Hosts;
using TheTechIdea.Beep.Editor.UOWManager.Models;
using TheTechIdea.Beep.Winform.Controls;
using TheTechIdea.Beep.Winform.Controls.Models;

namespace TheTechIdea.Beep.Winform.Data.Integrated.Forms.FeatureControls;

public sealed class WinFormLovDialog : Form
{
    private readonly IBeepFormsHost _host;
    private readonly string _blockName;
    private readonly string _fieldName;
    private readonly BeepTextBox _searchBox;
    private readonly BeepListBox _recordList;
    private bool _accepted;

    public WinFormLovDialog(
        IBeepFormsHost host,
        string blockName,
        string fieldName)
    {
        _host = host ?? throw new ArgumentNullException(nameof(host));
        _blockName = string.IsNullOrWhiteSpace(blockName)
            ? throw new ArgumentException("Block name is required.", nameof(blockName))
            : blockName.Trim();
        _fieldName = string.IsNullOrWhiteSpace(fieldName)
            ? throw new ArgumentException("Field name is required.", nameof(fieldName))
            : fieldName.Trim();

        var definition = _host.GetLov(_blockName, _fieldName);
        Text = definition?.Title ?? $"Select {_fieldName}";
        Width = definition?.Width ?? 600;
        Height = definition?.Height ?? 400;
        StartPosition = FormStartPosition.CenterParent;

        _searchBox = new BeepTextBox
        {
            Dock = DockStyle.Top,
            PlaceholderText = "Search",
            Visible = definition?.AllowSearch != false,
            UseThemeColors = true
        };
        _recordList = new BeepListBox
        {
            Dock = DockStyle.Fill,
            MultiSelect = definition?.AllowMultiSelect == true,
            UseThemeColors = true
        };

        var acceptButton = new BeepButton
        {
            Text = "Select",
            Dock = DockStyle.Right,
            UseThemeColors = true
        };
        var cancelButton = new BeepButton
        {
            Text = "Cancel",
            Dock = DockStyle.Right,
            UseThemeColors = true
        };
        var actions = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            FlowDirection = FlowDirection.RightToLeft,
            Height = 48
        };
        actions.Controls.Add(cancelButton);
        actions.Controls.Add(acceptButton);
        Controls.Add(_recordList);
        Controls.Add(_searchBox);
        Controls.Add(actions);

        acceptButton.Click += (_, _) =>
        {
            if (_recordList.SelectedItem?.Item is not null)
            {
                AcceptSelection(_recordList.SelectedItem.Item);
                DialogResult = DialogResult.OK;
            }
        };
        cancelButton.Click += (_, _) =>
        {
            CancelSelection();
            DialogResult = DialogResult.Cancel;
        };
        _searchBox.KeyDown += async (_, args) =>
        {
            if (args.KeyCode == Keys.Enter)
            {
                await LoadRecordsAsync(_searchBox.Text);
            }
        };
    }

    public object? SelectedRecord { get; private set; }

    public async Task<LOVResult> LoadRecordsAsync(
        string? searchText = null,
        CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        var result = await _host.LoadLovDataAsync(
            _blockName,
            _fieldName,
            searchText);
        if (!result.Success)
        {
            return result;
        }

        var definition = _host.GetLov(_blockName, _fieldName);
        _recordList.ClearItems();
        _recordList.AddItems(result.Records.Select(record => new SimpleItem
        {
            Text = GetDisplayText(record, definition),
            Item = record,
            Value = record
        }));
        _recordList.RefreshItems();
        return result;
    }

    public void AcceptSelection(object selectedRecord)
    {
        SelectedRecord = selectedRecord ?? throw new ArgumentNullException(nameof(selectedRecord));
        _accepted = true;
    }

    public void CancelSelection()
    {
        _accepted = false;
        SelectedRecord = null;
    }

    public Task<bool> ApplySelectionAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        if (!_accepted || SelectedRecord is null)
        {
            return Task.FromResult(false);
        }

        var definition = _host.GetLov(_blockName, _fieldName);
        var values = definition is null
            ? null
            : _host.GetLovRelatedFieldValues(definition, SelectedRecord);
        if (values is null || values.Count == 0)
        {
            return Task.FromResult(false);
        }

        var success = true;
        foreach (var pair in values)
        {
            var isReturnValue = string.Equals(
                pair.Key,
                "__RETURN_VALUE__",
                StringComparison.Ordinal);
            if (!isReturnValue && !definition!.AutoPopulateRelatedFields)
                continue;
            success &= _host.SetFieldValue(
                _blockName,
                isReturnValue ? _fieldName : pair.Key,
                pair.Value);
        }

        return Task.FromResult(success);
    }

    private static string GetDisplayText(object record, LOVDefinition? definition)
    {
        var fieldName = definition?.DisplayField;
        if (string.IsNullOrWhiteSpace(fieldName))
        {
            return record.ToString() ?? string.Empty;
        }

        var property = TypeDescriptor.GetProperties(record)
            .Find(fieldName, ignoreCase: true);
        return property?.GetValue(record)?.ToString()
            ?? record.ToString()
            ?? string.Empty;
    }
}
