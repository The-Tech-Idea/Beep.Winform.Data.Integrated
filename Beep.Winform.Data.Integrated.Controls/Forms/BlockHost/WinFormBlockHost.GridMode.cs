using System.Windows.Forms;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Editor.Forms.Helpers;
using TheTechIdea.Beep.Editor.Forms.Models;
using TheTechIdea.Beep.Winform.Controls.GridX;
using TheTechIdea.Beep.Winform.Controls.Models;

namespace TheTechIdea.Beep.Winform.Data.Integrated.Forms.BlockHost;

// Multi-record (tabular) block mode.
//
// An Oracle Forms block is routinely multi-record: a scrolling region showing N
// rows with the current record highlighted. The commonest Forms screen is a
// single-record master over a tabular detail, so without this that layout
// cannot be built at all.
//
// This HOSTS BeepGridPro. It does not draw a grid, and no DataGridView appears
// anywhere — the Beep control carries the theming, DPI and editor work. The
// host's only job is translation in both directions:
//
//     block items          -> BeepColumnConfig
//     GetBlockData         -> BeepGridPro.DataSource
//     grid row selection   -> MoveToRecordAsync
//
// Behaviour stays in FormsManager. Nothing here decides anything about records;
// it reflects engine state and forwards intent.
public partial class WinFormBlockHost
{
    private BeepGridPro? _grid;
    private bool _gridSyncing;

    /// <summary>
    /// Renders the block as a scrolling table rather than one record at a time.
    /// </summary>
    /// <remarks>
    /// Defaults from <c>Definition.PresentationMode</c> — the value the IDE's
    /// block editor writes — so a block declared multi-record at design time
    /// comes up that way with no extra wiring.
    /// </remarks>
    public bool IsMultiRecord
    {
        get => _isMultiRecord ??= DeclaredMultiRecord();
        set => _isMultiRecord = value;
    }

    private bool? _isMultiRecord;

    private bool DeclaredMultiRecord()
    {
        var mode = Definition?.PresentationMode;
        if (string.IsNullOrWhiteSpace(mode)) return false;

        return mode.Trim() switch
        {
            var m when m.Equals("Grid", StringComparison.OrdinalIgnoreCase) => true,
            var m when m.Equals("Table", StringComparison.OrdinalIgnoreCase) => true,
            var m when m.Equals("Tabular", StringComparison.OrdinalIgnoreCase) => true,
            var m when m.Equals("MultiRecord", StringComparison.OrdinalIgnoreCase) => true,
            _ => false
        };
    }

    /// <summary>The hosted grid, or null while the block is in record mode.</summary>
    public BeepGridPro? Grid => _grid;

    /// <summary>
    /// Builds the grid and swaps it in for the field layout.
    /// </summary>
    private void BuildGrid()
    {
        if (_grid is not null) return;

        _grid = new BeepGridPro { Dock = DockStyle.Fill };
        _grid.RowSelectionChanged += GridOnRowSelectionChanged;

        _layout.Visible = false;
        Controls.Add(_grid);
        _grid.BringToFront();
    }

    private void TeardownGrid()
    {
        if (_grid is null) return;

        _grid.RowSelectionChanged -= GridOnRowSelectionChanged;
        Controls.Remove(_grid);
        _grid.Dispose();
        _grid = null;
        _layout.Visible = true;
    }

    /// <summary>
    /// Describes the block's fields as grid columns.
    /// </summary>
    /// <remarks>
    /// The editor for each column comes from
    /// <see cref="FieldHost.WinFormFieldPresenterRegistry.ResolveColumnType"/> —
    /// the same registry that picks the single-record presenter, so one field
    /// looks and behaves the same either way. This deliberately does not carry
    /// its own switch.
    /// </remarks>
    private void ConfigureGridColumns()
    {
        if (_grid is null || _formsHost is null) return;

        var fields = _formsHost.GetBlockFields(ManagerBlockName);
        if (fields is null || fields.Count == 0) return;

        _grid.Columns.Clear();

        var index = 0;
        foreach (var field in fields)
        {
            if (string.IsNullOrWhiteSpace(field.FieldName)) continue;

            var item = _formsHost.GetItemInfo(ManagerBlockName, field.FieldName);
            var hasLov = _formsHost.HasLov(ManagerBlockName, field.FieldName);

            _grid.Columns.Add(new BeepColumnConfig
            {
                ColumnName = field.FieldName,
                ColumnCaption = string.IsNullOrWhiteSpace(item?.PromptText)
                    ? field.FieldName
                    : item.PromptText,
                Index = index++,
                Visible = item?.Visible ?? true,
                ReadOnly = !(item?.UpdateAllowed ?? true),
                CellEditor = _registry.ResolveColumnType(field, hasLov),
            });
        }
    }

    /// <summary>
    /// Pushes the engine's rows and current-record pointer into the grid.
    /// </summary>
    private void SyncGridFromManager()
    {
        if (_grid is null || _formsHost is null) return;

        _gridSyncing = true;
        try
        {
            if (_grid.Columns.Count == 0) ConfigureGridColumns();

            _grid.DataSource = _formsHost.GetBlockData(ManagerBlockName);

            // SelectCell, not a row API — BeepGridPro tracks selection by cell
            // and exposes the row through CurrentRowIndex.
            var index = _formsHost.GetCurrentBlockRecordIndex(ManagerBlockName);
            if (index >= 0 && index != _grid.CurrentRowIndex) _grid.SelectCell(index, 0);
        }
        finally
        {
            _gridSyncing = false;
        }
    }

    /// <summary>
    /// Grid selection moves the engine's record pointer, not the other way
    /// round — the engine owns which record is current, and every other surface
    /// (detail blocks, the navigation bar, dirty state) follows from it.
    /// </summary>
    private async void GridOnRowSelectionChanged(object? sender, BeepRowSelectedEventArgs e)
    {
        if (_gridSyncing || _formsHost is null) return;
        if (e.RowIndex < 0) return;
        if (e.RowIndex == _formsHost.GetCurrentBlockRecordIndex(ManagerBlockName)) return;

        try
        {
            await _formsHost.MoveToRecordAsync(ManagerBlockName, e.RowIndex);
            SyncFromManager();
        }
        catch (Exception ex)
        {
            // House rule: report, never swallow. A failed move must not leave
            // the grid showing a row the engine is not on.
            _formsHost.SetMessage(
                $"Could not move to record {e.RowIndex + 1}: {ex.Message}",
                MessageLevel.Error);
            SyncGridFromManager();
        }
    }
}
