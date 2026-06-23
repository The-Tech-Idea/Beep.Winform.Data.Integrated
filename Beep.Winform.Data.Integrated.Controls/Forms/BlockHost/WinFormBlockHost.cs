using System.Windows.Forms;
using TheTechIdea.Beep.Editor.Forms.Hosts;
using TheTechIdea.Beep.Editor.Forms.Models;
using TheTechIdea.Beep.Editor.UOWManager.Models;
using TheTechIdea.Beep.Winform.Controls;
using TheTechIdea.Beep.Winform.Data.Integrated.Forms.FeatureControls;
using TheTechIdea.Beep.Winform.Data.Integrated.Forms.FieldHost;

namespace TheTechIdea.Beep.Winform.Data.Integrated.Forms.BlockHost;

public partial class WinFormBlockHost : UserControl, IBlockView
{
    private readonly List<IFieldPresenter> _presenters = [];
    private readonly HashSet<IFieldPresenter> _ownedPresenters = [];
    private readonly Dictionary<IFieldPresenter, BeepLabel> _presenterLabels = [];
    private readonly TableLayoutPanel _layout;
    private readonly WinFormFieldPresenterRegistry _registry = new();
    private IBeepFormsHost? _formsHost;
    private bool _synchronizing;
    private bool _queryMode;
    private int _currentIndex = -1;
    private bool _disposed;

    public WinFormBlockHost()
    {
        _layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            AutoSize = true,
            ColumnCount = 2
        };
        _layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        _layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        Controls.Add(_layout);
        LovDialogPresenter = dialog =>
        {
            var owner = FindForm();
            return owner is null
                ? dialog.ShowDialog()
                : dialog.ShowDialog(owner);
        };
    }

    public string BlockName { get; set; } = string.Empty;
    public string ManagerBlockName => BlockName.Trim();
    public string EntityName { get; set; } = string.Empty;
    public string ConnectionName { get; set; } = string.Empty;
    public bool IsBound => _formsHost is not null;
    public bool IsQueryMode => _queryMode;
    public bool IsMaster { get; set; }
    public bool AutoGenerateFields { get; set; } = true;
    public Func<WinFormLovDialog, DialogResult> LovDialogPresenter { get; set; }
    public IBeepFormsHost? FormsHost => _formsHost;
    public object View => this;
    public IBlockNavigationBar? NavigationBar { get; set; }
    public object? Definition { get; set; }
    public object ViewState { get; } = new BeepViewState();
    public int RecordCount => _formsHost?.GetBlockRecordCount(ManagerBlockName) ?? 0;
    public int CurrentRecordIndex => _currentIndex;
    public object? CurrentRecord => _formsHost?.GetCurrentBlockItem(ManagerBlockName);
    public IReadOnlyList<IFieldPresenter> FieldPresenters => _presenters;
    public string? ActiveFieldName { get; private set; }

    public event EventHandler<TriggerExecutingEventArgs>? TriggerExecuting;
    public event EventHandler<TriggerExecutedEventArgs>? TriggerExecuted;
    public event EventHandler<TriggerRegisteredEventArgs>? TriggerRegistered;
    public event EventHandler<TriggerUnregisteredEventArgs>? TriggerUnregistered;
    public event EventHandler<BeepUnitOfWorkEventArgs>? UnitOfWorkActivity;

    public void Bind(IBeepFormsHost formsHost)
    {
        ArgumentNullException.ThrowIfNull(formsHost);
        if (string.IsNullOrWhiteSpace(ManagerBlockName))
            throw new InvalidOperationException("BlockName is required.");
        if (!formsHost.IsBlockRegistered(ManagerBlockName))
            throw new InvalidOperationException($"Engine block '{ManagerBlockName}' is not registered.");
        if (_formsHost is not null && !ReferenceEquals(_formsHost, formsHost))
            Unbind();

        _formsHost = formsHost;
        if (AutoGenerateFields) GenerateMissingPresenters();
        foreach (var presenter in _presenters)
        {
            presenter.ValueChanged -= PresenterOnValueChanged;
            presenter.ValueChanged += PresenterOnValueChanged;
        }
        ConnectNavigationBar();
        SyncFromManager();
    }

    public void Unbind()
    {
        DisconnectNavigationBar();
        foreach (var presenter in _presenters)
            presenter.ValueChanged -= PresenterOnValueChanged;
        _formsHost = null;
        _queryMode = false;
        _currentIndex = -1;
        ActiveFieldName = null;
    }

    public IFieldPresenter? FindFieldPresenter(string fieldName) =>
        _presenters.FirstOrDefault(p =>
            string.Equals(p.FieldName, fieldName, StringComparison.OrdinalIgnoreCase));

    public bool FocusField(string fieldName)
    {
        var presenter = FindFieldPresenter(fieldName);
        if (presenter?.View is not Control control ||
            !presenter.IsVisible ||
            !presenter.IsEnabled ||
            !control.Enabled)
        {
            return false;
        }

        ActiveFieldName = presenter.FieldName;
        control.Select();
        control.Focus();
        return true;
    }

    public void AddFieldPresenter(IFieldPresenter presenter)
    {
        ArgumentNullException.ThrowIfNull(presenter);
        if (FindFieldPresenter(presenter.FieldName) is not null)
            throw new InvalidOperationException($"Field '{presenter.FieldName}' already has a presenter.");
        _presenters.Add(presenter);
        presenter.ValueChanged += PresenterOnValueChanged;
        AddPresenterToLayout(presenter);
    }

    public void RemoveFieldPresenter(string fieldName)
    {
        var presenter = FindFieldPresenter(fieldName);
        if (presenter is null) return;
        presenter.ValueChanged -= PresenterOnValueChanged;
        _presenters.Remove(presenter);
        if (presenter.View is Control control)
        {
            DisconnectItemNavigation(control);
            _layout.Controls.Remove(control);
        }
        if (_presenterLabels.Remove(presenter, out var label))
        {
            _layout.Controls.Remove(label);
            label.Dispose();
        }
        if (_ownedPresenters.Remove(presenter) && presenter is IDisposable disposable)
            disposable.Dispose();
    }

    private void GenerateMissingPresenters()
    {
        var fields = _formsHost?.GetBlockFields(ManagerBlockName) ?? [];
        foreach (var field in fields.Where(f => !f.IsHidden).OrderBy(f => f.OrdinalPosition))
        {
            if (FindFieldPresenter(field.FieldName) is not null) continue;
            var presenter = _registry.Create(field, _formsHost!.HasLov(ManagerBlockName, field.FieldName));
            _ownedPresenters.Add(presenter);
            AddFieldPresenter(presenter);
        }
    }

    private void AddPresenterToLayout(IFieldPresenter presenter)
    {
        if (presenter.View is not Control control)
            throw new InvalidOperationException("WinForms presenters must expose a Control.");
        var row = _layout.RowCount++;
        _layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        var label = new BeepLabel { Text = presenter.Label, AutoSize = true };
        _presenterLabels[presenter] = label;
        _layout.Controls.Add(label, 0, row);
        control.Dock = DockStyle.Top;
        ConnectItemNavigation(control);
        _layout.Controls.Add(control, 1, row);
    }

    public void SyncFromManager()
    {
        if (_formsHost is null) return;
        _synchronizing = true;
        try
        {
            _currentIndex = _formsHost.GetCurrentBlockRecordIndex(ManagerBlockName);
            _queryMode = _formsHost.GetBlockMode(ManagerBlockName) == DataBlockMode.EnterQuery;
            foreach (var presenter in _presenters)
            {
                var rawValue = _formsHost.GetFieldValue(
                    ManagerBlockName,
                    presenter.FieldName);
                var security = _formsHost.GetFieldSecurity(
                    ManagerBlockName,
                    presenter.FieldName);
                presenter.SetValue(security?.Masked == true
                    ? _formsHost.GetMaskedFieldValue(
                        ManagerBlockName,
                        presenter.FieldName,
                        rawValue)
                    : rawValue);

                var item = _formsHost.GetItemInfo(
                    ManagerBlockName,
                    presenter.FieldName);
                if (!string.IsNullOrWhiteSpace(item?.PromptText))
                {
                    presenter.Label = item.PromptText;
                    if (_presenterLabels.TryGetValue(presenter, out var label))
                    {
                        label.Text = item.PromptText;
                    }
                }
                presenter.Prompt = item?.HintText;
                presenter.IsVisible = item?.Visible ?? presenter.IsVisible;
                presenter.IsRequired = item?.Required ?? presenter.IsRequired;
                if (item is not null)
                {
                    presenter.IsReadOnly = _queryMode
                        ? !item.QueryAllowed
                        : GetBlockModeForItemAccess() == DataBlockMode.Insert
                            ? !item.InsertAllowed
                            : !item.UpdateAllowed;
                    presenter.ValidationError = item.HasError
                        ? item.ErrorMessage
                        : null;
                    if (presenter.View is Control control)
                    {
                        control.TabIndex = item.TabIndex;
                    }
                }
                presenter.IsEnabled = (item?.Enabled ?? true) &&
                    (_queryMode
                        ? _formsHost.IsFieldQueryAllowed(
                            ManagerBlockName,
                            presenter.FieldName)
                        : true);
            }
            RefreshNavigationBar();
        }
        finally { _synchronizing = false; }
    }

    public void RefreshPresenters() => SyncFromManager();

    private DataBlockMode GetBlockModeForItemAccess() =>
        _formsHost?.GetBlockMode(ManagerBlockName) ?? DataBlockMode.Query;

    private async void PresenterOnValueChanged(object? sender, object? value)
    {
        if (_synchronizing || _formsHost is null || sender is not IFieldPresenter presenter)
            return;
        if (_queryMode)
        {
            presenter.QueryValue = value;
            return;
        }

        if (_formsHost.GetLockOnEdit(ManagerBlockName) &&
            !_formsHost.IsCurrentRecordLocked(ManagerBlockName) &&
            !await _formsHost.LockCurrentRecordAsync(ManagerBlockName))
        {
            _synchronizing = true;
            try
            {
                presenter.SetValue(_formsHost.GetFieldValue(
                    ManagerBlockName,
                    presenter.FieldName));
                presenter.ValidationError = "The current record could not be locked.";
            }
            finally
            {
                _synchronizing = false;
            }
            return;
        }

        var success = _formsHost.SetFieldValue(ManagerBlockName, presenter.FieldName, value);
        presenter.ValidationError = success ? null : $"{presenter.Label} is invalid.";
    }

    public async Task<bool> ExecuteQueryAsync(CancellationToken cancellationToken = default)
    {
        if (_formsHost is null)
        {
            return false;
        }

        if (!_queryMode)
        {
            return await ExecuteAndSyncAsync(
                h => h.ExecuteQueryAsync(ManagerBlockName, cancellationToken));
        }

        var criteria = _presenters
            .Where(p => p.IsQueryEnabled && p.QueryValue is not null)
            .ToDictionary(
                p => p.FieldName,
                p => new QueryCriterion(p.QueryValue, p.QueryOperator),
                StringComparer.OrdinalIgnoreCase);
        var result = await _formsHost.ExecuteQueryByExampleAsync(
            ManagerBlockName,
            criteria,
            cancellationToken);
        if (!result)
        {
            return false;
        }

        foreach (var presenter in _presenters)
        {
            presenter.QueryValue = null;
        }

        _queryMode = false;
        SyncFromManager();
        return true;
    }
    public Task<bool> SaveAsync() =>
        ValidateCurrentRecord(ValidationTiming.OnCommit)
            ? ExecuteAndSyncAsync(h => h.SaveBlockAsync(ManagerBlockName))
            : Task.FromResult(false);
    public Task<bool> RollbackAsync() => ExecuteAndSyncAsync(h => h.RollbackBlockAsync(ManagerBlockName));
    public Task<bool> InsertRecordAsync() => ExecuteAndSyncAsync(h => h.InsertBlockRecordAsync(ManagerBlockName));
    public Task<bool> DeleteCurrentRecordAsync() => ExecuteAndSyncAsync(h => h.DeleteBlockCurrentRecordAsync(ManagerBlockName));
    public Task<bool> ClearAsync(CancellationToken cancellationToken = default) =>
        ExecuteAndSyncAsync(h => h.ClearBlockAsync(ManagerBlockName, cancellationToken));
    public Task<bool> ClearRecordAsync(CancellationToken cancellationToken = default) =>
        ExecuteAndSyncAsync(h => h.ClearRecordAsync(ManagerBlockName, cancellationToken));
    public Task<bool> NavigateFirstAsync() => ExecuteAndSyncAsync(h => h.MoveFirstAsync(ManagerBlockName));
    public Task<bool> NavigateLastAsync() => ExecuteAndSyncAsync(h => h.MoveLastAsync(ManagerBlockName));
    public Task<bool> NavigateNextAsync() => ExecuteAndSyncAsync(h => h.MoveNextAsync(ManagerBlockName));
    public Task<bool> NavigatePreviousAsync() => ExecuteAndSyncAsync(h => h.MovePreviousAsync(ManagerBlockName));
    public Task<bool> NavigateToRecordAsync(int index) =>
        ExecuteAndSyncAsync(h => h.MoveToRecordAsync(ManagerBlockName, index));

    public void EnterQueryMode()
    {
        if (_formsHost is null) return;
        if (_formsHost.EnterQueryModeAsync(ManagerBlockName).GetAwaiter().GetResult())
        {
            _queryMode = true;
            foreach (var presenter in _presenters)
            {
                presenter.Clear();
                presenter.QueryValue = null;
                presenter.IsEnabled = _formsHost.IsFieldQueryAllowed(ManagerBlockName, presenter.FieldName);
            }
            RefreshNavigationBar();
        }
    }

    public void ExitQueryMode()
    {
        if (_formsHost is null) return;
        if (_formsHost.ExitQueryModeAsync(ManagerBlockName).GetAwaiter().GetResult())
        {
            _queryMode = false;
            SyncFromManager();
        }
    }

    private async Task<bool> ExecuteAndSyncAsync(Func<IBeepFormsHost, Task<bool>> operation)
    {
        if (_formsHost is null) return false;
        var result = await operation(_formsHost);
        if (result) SyncFromManager();
        return result;
    }

    public bool ValidateCurrentRecord(ValidationTiming timing = ValidationTiming.Manual)
    {
        if (_formsHost is null) return false;
        var values = _presenters.ToDictionary(
            p => p.FieldName,
            p => p.Value!,
            StringComparer.OrdinalIgnoreCase);
        var result = _formsHost.ValidateBlockRecord(ManagerBlockName, values, timing);
        foreach (var presenter in _presenters)
            presenter.ValidationError = null;
        if (result is null) return true;

        foreach (var pair in result.ItemResults)
        {
            var presenter = FindFieldPresenter(pair.Key);
            if (presenter is null || pair.Value.IsValid) continue;
            presenter.ValidationError = pair.Value.FirstError;
        }
        var firstInvalid = result.InvalidItems.FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(firstInvalid))
            FocusField(firstInvalid);
        return result.IsValid;
    }

    public async Task<bool> ApplyLovSelectionAsync(
        string fieldName,
        object selectedRecord,
        CancellationToken cancellationToken = default)
    {
        if (_formsHost is null) return false;
        var lov = _formsHost.GetLov(ManagerBlockName, fieldName);
        if (lov is null) return false;
        var values = _formsHost.GetLovRelatedFieldValues(lov, selectedRecord);
        if (values is null || values.Count == 0) return false;
        var success = true;
        foreach (var pair in values)
        {
            var isReturnValue = string.Equals(
                pair.Key,
                "__RETURN_VALUE__",
                StringComparison.Ordinal);
            if (!isReturnValue && !lov.AutoPopulateRelatedFields)
                continue;
            success &= _formsHost.SetFieldValue(
                ManagerBlockName,
                isReturnValue ? fieldName : pair.Key,
                pair.Value);
        }
        if (success) SyncFromManager();
        await Task.CompletedTask;
        return success;
    }

    private void ConnectNavigationBar()
    {
        if (NavigationBar is null) return;
        NavigationBar.FirstClicked += NavigationFirstClicked;
        NavigationBar.PreviousClicked += NavigationPreviousClicked;
        NavigationBar.NextClicked += NavigationNextClicked;
        NavigationBar.LastClicked += NavigationLastClicked;
        NavigationBar.RecordIndexChanged += NavigationIndexChanged;
    }

    private void DisconnectNavigationBar()
    {
        if (NavigationBar is null) return;
        NavigationBar.FirstClicked -= NavigationFirstClicked;
        NavigationBar.PreviousClicked -= NavigationPreviousClicked;
        NavigationBar.NextClicked -= NavigationNextClicked;
        NavigationBar.LastClicked -= NavigationLastClicked;
        NavigationBar.RecordIndexChanged -= NavigationIndexChanged;
    }

    private async void NavigationFirstClicked(object? s, EventArgs e) => await NavigateFirstAsync();
    private async void NavigationPreviousClicked(object? s, EventArgs e) => await NavigatePreviousAsync();
    private async void NavigationNextClicked(object? s, EventArgs e) => await NavigateNextAsync();
    private async void NavigationLastClicked(object? s, EventArgs e) => await NavigateLastAsync();
    private async void NavigationIndexChanged(object? s, int e) => await NavigateToRecordAsync(e);
    private void RefreshNavigationBar()
    {
        if (NavigationBar is null) return;
        NavigationBar.CurrentRecordIndex = _currentIndex;
        NavigationBar.RecordCount = RecordCount;
        NavigationBar.IsQueryMode = _queryMode;
        NavigationBar.Refresh();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && !_disposed)
        {
            _disposed = true;
            Unbind();
            foreach (var presenter in _ownedPresenters.OfType<IDisposable>())
                presenter.Dispose();
            _ownedPresenters.Clear();
            foreach (var control in _presenters
                         .Select(presenter => presenter.View)
                         .OfType<Control>())
            {
                DisconnectItemNavigation(control);
            }
            foreach (var label in _presenterLabels.Values)
                label.Dispose();
            _presenterLabels.Clear();
            _presenters.Clear();
        }
        base.Dispose(disposing);
    }
}
