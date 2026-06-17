using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using TheTechIdea.Beep.Winform.Controls.Integrated.Blocks.Contracts;
using TheTechIdea.Beep.Winform.Controls.Integrated.Blocks.Models;
using TheTechIdea.Beep.Editor.Forms.Models;
using TheTechIdea.Beep.Winform.Controls.Integrated.Forms.Models;

namespace TheTechIdea.Beep.Winform.Controls.Integrated.Forms;

internal sealed class CommandButtonStrategy
{
    public BeepFormsCommandBarButtons Flag { get; init; }
    public string Caption { get; init; } = string.Empty;
    public int MinWidth { get; init; } = 42;
    public Func<BeepForms, bool> CanExecute { get; init; } = _ => true;
    public Func<BeepForms, Task> ExecuteAsync { get; init; } = _ => Task.CompletedTask;
    public bool IsAsync { get; init; }
}

internal static class BeepFormsCommandBarStrategies
{
    public static IReadOnlyList<CommandButtonStrategy> All { get; } = new List<CommandButtonStrategy>
    {
        new() { Flag = BeepFormsCommandBarButtons.FirstBlock,  Caption = "|◀",  CanExecute = CanNavigateBlock, ExecuteAsync = f => SyncAction(f, h => h.Builtins?.FirstBlock()) },
        new() { Flag = BeepFormsCommandBarButtons.PreviousBlock, Caption = "◀",   CanExecute = CanNavigateBlock, ExecuteAsync = f => SyncAction(f, h => h.Builtins?.PreviousBlock()) },
        new() { Flag = BeepFormsCommandBarButtons.NextBlock,  Caption = "▶",   CanExecute = CanNavigateBlock, ExecuteAsync = f => SyncAction(f, h => h.Builtins?.NextBlock()) },
        new() { Flag = BeepFormsCommandBarButtons.LastBlock,   Caption = "▶|",  CanExecute = CanNavigateBlock, ExecuteAsync = f => SyncAction(f, h => h.Builtins?.LastBlock()) },
        new() { Flag = BeepFormsCommandBarButtons.InsertRecord, Caption = "+",  MinWidth = 42, CanExecute = CanInsert, ExecuteAsync = f => RunHostAsync(f, h => h.InsertRecordAsync()), IsAsync = true },
        new() { Flag = BeepFormsCommandBarButtons.DeleteRecord, Caption = "−",  MinWidth = 42, CanExecute = CanDelete, ExecuteAsync = f => RunHostAsync(f, h => h.DeleteCurrentRecordAsync()), IsAsync = true },
        new() { Flag = BeepFormsCommandBarButtons.DuplicateRecord, Caption = "Dup", MinWidth = 52, CanExecute = HasActiveBlock, ExecuteAsync = f => RunHostAsync(f, h => h.DuplicateCurrentRecordAsync()), IsAsync = true },
        new() { Flag = BeepFormsCommandBarButtons.ClearRecord, Caption = "Clr Rec", MinWidth = 76, CanExecute = HasActiveBlock, ExecuteAsync = f => RunHostAsync(f, h => h.ClearRecordAsync()), IsAsync = true },
        new() { Flag = BeepFormsCommandBarButtons.ClearBlock, Caption = "Clr Blk", MinWidth = 76, CanExecute = HasActiveBlock, ExecuteAsync = f => RunHostAsync(f, h => h.ClearBlockAsync()), IsAsync = true },
        new() { Flag = BeepFormsCommandBarButtons.ShowLOV, Caption = "LOV", MinWidth = 54, CanExecute = HasLovOnActiveItem, ExecuteAsync = ShowLovAsync, IsAsync = true },
        new() { Flag = BeepFormsCommandBarButtons.RefreshBlock, Caption = "⟳",  MinWidth = 42, CanExecute = HasActiveBlock, ExecuteAsync = f => SyncAction(f, h => h.SyncFromManager()) },
        new() { Flag = BeepFormsCommandBarButtons.Undo, Caption = "↩",  MinWidth = 42, CanExecute = CanUndo, ExecuteAsync = f => SyncAction(f, h => h.FormsManager?.UndoBlock(h.ActiveBlockName!)) },
        new() { Flag = BeepFormsCommandBarButtons.Redo, Caption = "↪",  MinWidth = 42, CanExecute = CanRedo, ExecuteAsync = f => SyncAction(f, h => h.FormsManager?.RedoBlock(h.ActiveBlockName!)) },
        new() { Flag = BeepFormsCommandBarButtons.ExportJson, Caption = "⤓",  MinWidth = 42, CanExecute = HasActiveBlock, ExecuteAsync = ExportAsync, IsAsync = true },
        new() { Flag = BeepFormsCommandBarButtons.ImportJson, Caption = "⤒",  MinWidth = 42, CanExecute = HasActiveBlock, ExecuteAsync = ImportAsync, IsAsync = true },
        new() { Flag = BeepFormsCommandBarButtons.JumpToFirstError, Caption = "⟶Err", MinWidth = 62, CanExecute = HasErrors, ExecuteAsync = f => { f.ShowErrorSummary(); return Task.CompletedTask; } },
        new() { Flag = BeepFormsCommandBarButtons.ToggleGridView, Caption = "⊞",  MinWidth = 42, CanExecute = HasActiveBlock, ExecuteAsync = f => { ToggleGrid(f); return Task.CompletedTask; } },
        new() { Flag = BeepFormsCommandBarButtons.Sync, Caption = "Sync", MinWidth = 88, CanExecute = HasHost, ExecuteAsync = f => { f.SyncFromManager(); f.ShowInfo("Fresh-start host synchronized from FormsManager."); return Task.CompletedTask; } },
    };

    private static bool HasHost(BeepForms f) => f != null;
    private static bool HasActiveBlock(BeepForms f) => !string.IsNullOrWhiteSpace(f?.ActiveBlockName);
    private static bool CanNavigateBlock(BeepForms f) => f != null && f.Blocks.Count > 1;
    private static bool CanInsert(BeepForms f) => HasActiveBlock(f) && f.TryGetBlockProperty(f.ActiveBlockName!, "InsertAllowed", out object? v) && v is true;
    private static bool CanDelete(BeepForms f) => HasActiveBlock(f) && f.TryGetBlockProperty(f.ActiveBlockName!, "DeleteAllowed", out object? v) && v is true;
    private static bool HasLovOnActiveItem(BeepForms f) => HasActiveBlock(f) && !string.IsNullOrWhiteSpace(f.ActiveItemName) && f.HasLov(f.ActiveBlockName!, f.ActiveItemName!);
    private static bool CanUndo(BeepForms f) { try { return HasActiveBlock(f) && f.FormsManager?.CanUndoBlock(f.ActiveBlockName!) == true; } catch { return false; } }
    private static bool CanRedo(BeepForms f) { try { return HasActiveBlock(f) && f.FormsManager?.CanRedoBlock(f.ActiveBlockName!) == true; } catch { return false; } }
    private static bool HasErrors(BeepForms f) => f != null && f.ViewState.ErrorCount > 0;

    private static Task RunHostAsync(BeepForms f, Func<BeepForms, Task> action)
    {
        try { return action(f); }
        catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"[CommandBar] {ex.Message}"); return Task.CompletedTask; }
    }

    private static Task SyncAction(BeepForms f, Action<BeepForms> action)
    {
        try { action(f); f.SyncFromManager(); }
        catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"[CommandBar] {ex.Message}"); }
        return Task.CompletedTask;
    }

    private static async Task ShowLovAsync(BeepForms f)
    {
        if (string.IsNullOrWhiteSpace(f.ActiveBlockName) || string.IsNullOrWhiteSpace(f.ActiveItemName)) return;
        await f.ShowLovAsync(f.ActiveBlockName, f.ActiveItemName);
        f.SyncFromManager();
    }

    private static async Task ExportAsync(BeepForms f)
    {
        string blockName = f.ActiveBlockName!;
        var conn = f.DataConnection;
        string defaultName = conn?.CurrentConnection?.ConnectionName ?? "export";
        using var dialog = new SaveFileDialog { FileName = $"{defaultName}_{blockName}.json", Filter = "JSON files (*.json)|*.json", Title = $"Export {blockName} as JSON" };
        if (dialog.ShowDialog() == DialogResult.OK)
        {
            using var stream = new System.IO.FileStream(dialog.FileName, System.IO.FileMode.Create);
            await f.FormsManager!.ExportBlockToJsonAsync(blockName, stream);
            f.ShowSuccess($"Exported '{blockName}' to {dialog.FileName}.");
        }
    }

    private static async Task ImportAsync(BeepForms f)
    {
        string blockName = f.ActiveBlockName!;
        using var dialog = new OpenFileDialog { Filter = "JSON files (*.json)|*.json|CSV files (*.csv)|*.csv", Title = $"Import into {blockName}" };
        if (dialog.ShowDialog() == DialogResult.OK)
        {
            bool confirmed = await f.ConfirmAsync("Import Data", $"Import data from {System.IO.Path.GetFileName(dialog.FileName)} into '{blockName}'?");
            if (!confirmed) return;
            bool isJson = dialog.FileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase);
            using var stream = new System.IO.FileStream(dialog.FileName, System.IO.FileMode.Open);
            if (isJson) await f.FormsManager!.ImportBlockFromJsonAsync(blockName, stream);
            else await f.FormsManager!.ImportBlockFromCsvAsync(blockName, stream);
            f.SyncFromManager();
            f.ShowSuccess($"Imported into '{blockName}' from {System.IO.Path.GetFileName(dialog.FileName)}.");
        }
    }

    private static void ToggleGrid(BeepForms f)
    {
        var block = f.Blocks.FirstOrDefault(b => string.Equals(b.BlockName, f.ActiveBlockName, StringComparison.OrdinalIgnoreCase));
        if (block == null) return;
        bool isGrid = block is IBeepBlockView view && view.Definition?.PresentationMode == BeepBlockPresentationMode.Grid;
        if (block.Definition == null) block.Definition = new BeepBlockDefinition { PresentationMode = BeepBlockPresentationMode.Grid };
        else if (isGrid) block.Definition.PresentationMode = BeepBlockPresentationMode.Record;
        else block.Definition.PresentationMode = BeepBlockPresentationMode.Grid;
        f.SyncFromManager();
    }

    public static bool IsGridViewActive(BeepForms f)
    {
        if (f == null || string.IsNullOrWhiteSpace(f.ActiveBlockName)) return false;
        var block = f.Blocks.FirstOrDefault(b => string.Equals(b.BlockName, f.ActiveBlockName, StringComparison.OrdinalIgnoreCase));
        return block is IBeepBlockView view && view.Definition?.PresentationMode == BeepBlockPresentationMode.Grid;
    }

    public static string? GetGridToggleCaption(BeepForms f) => IsGridViewActive(f) ? "⊟" : "⊞";
}
