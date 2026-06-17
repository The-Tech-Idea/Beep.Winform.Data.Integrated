using System;
using System.Threading;
using System.Windows.Forms;

namespace TheTechIdea.Beep.Winform.Controls.Integrated.Blocks;

public partial class BeepBlock
{
    public BeepFormsKeyboardShortcuts FormsKeyboardShortcuts { get; } = new BeepFormsKeyboardShortcuts();

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        Keys key = keyData & Keys.KeyCode;
        Keys modifiers = keyData & Keys.Modifiers;

        try
        {
            if (modifiers == Keys.None)
            {
                if (key == FormsKeyboardShortcuts.EnterQueryKey)
                {
                    if (IsNavigatorCommandEnabled("query")) { _ = EnterQueryAsync(); return true; }
                }
                else if (key == FormsKeyboardShortcuts.ExecuteQueryKey)
                {
                    if (IsNavigatorCommandEnabled("execute")) { _ = ExecuteQueryAsync(); return true; }
                }
                else if (key == FormsKeyboardShortcuts.CommitKey)
                {
                    if (IsNavigatorCommandEnabled("save")) { _ = CommitAsync(CancellationToken.None); return true; }
                }
                else if (key == FormsKeyboardShortcuts.RollbackKey)
                {
                    if (IsNavigatorCommandEnabled("rollback")) { _ = RollbackAsync(); return true; }
                }
                else if (key == FormsKeyboardShortcuts.NewRecordKey)
                {
                    if (IsNavigatorCommandEnabled("new")) { _ = CreateRecordAsync(); return true; }
                }
                else if (key == FormsKeyboardShortcuts.DeleteRecordKey)
                {
                    if (IsNavigatorCommandEnabled("delete")) { _ = DeleteCurrentRecordAsync(CancellationToken.None); return true; }
                }
                else if (key == Keys.Escape && ViewState.IsQueryMode)
                {
                    if (IsNavigatorCommandEnabled("query")) { _ = ClearBlockAsync(CancellationToken.None); return true; }
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[BeepBlock.ProcessCmdKey] {ex.GetType().Name}: {ex.Message}");
        }

        return base.ProcessCmdKey(ref msg, keyData);
    }
}

public sealed class BeepFormsKeyboardShortcuts
{
    public Keys EnterQueryKey { get; set; } = Keys.F1;
    public Keys ExecuteQueryKey { get; set; } = Keys.F8;
    public Keys CommitKey { get; set; } = Keys.F10;
    public Keys RollbackKey { get; set; } = Keys.F11;
    public Keys NewRecordKey { get; set; } = Keys.F3;
    public Keys DeleteRecordKey { get; set; } = Keys.F4;

    public BeepFormsKeyboardShortcuts Clone()
    {
        return new BeepFormsKeyboardShortcuts
        {
            EnterQueryKey = EnterQueryKey,
            ExecuteQueryKey = ExecuteQueryKey,
            CommitKey = CommitKey,
            RollbackKey = RollbackKey,
            NewRecordKey = NewRecordKey,
            DeleteRecordKey = DeleteRecordKey
        };
    }
}
