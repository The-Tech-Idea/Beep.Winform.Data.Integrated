using System;
using System.Windows.Forms;

namespace TheTechIdea.Beep.Winform.Controls.Integrated.Blocks
{
    public partial class BeepBlock
    {
        // ── Keyboard shortcuts (Oracle Forms KEY- triggers) ──────────────────
        //
        // Oracle Forms has standard key-triggers that map to function keys.
        // Beep routes them as follows when the block has focus:
        //
        //   F1     → Query button (ENTER_QUERY)
        //   F8     → Execute button (EXECUTE_QUERY)
        //   F10    → Save button (COMMIT / POST)
        //   F11    → Rollback button
        //   Ctrl+N → CreateRecord
        //   Ctrl+D → DeleteCurrentRecord
        //   Ctrl+S → Commit
        //   Ctrl+Q → EnterQuery
        //   Ctrl+E → ExecuteQuery
        //   Esc    → ClearRecord (when block is in query mode)
        //
        // The mapping is exposed via the <see cref="FormsKeyboardShortcuts"/>
        // so hosts can override individual bindings.

        /// <summary>
        /// Centralised configuration of the keyboard shortcut map for
        /// <see cref="BeepBlock"/>. Hosts can replace individual bindings
        /// to match the developer's local conventions.
        /// </summary>
        public BeepFormsKeyboardShortcuts FormsKeyboardShortcuts { get; } = new BeepFormsKeyboardShortcuts();

        /// <summary>
        /// Intercept function-key presses (and the related Ctrl+ shortcuts)
        /// so the block can fire the matching built-in. Mirrors Oracle
        /// Forms' built-in KEY- triggers.
        /// </summary>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            Keys key = keyData & Keys.KeyCode;
            Keys modifiers = keyData & Keys.Modifiers;

            try
            {
                // F-keys (no modifiers)
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
                        if (IsNavigatorCommandEnabled("save")) { _ = CommitAsync(); return true; }
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
                        if (IsNavigatorCommandEnabled("delete")) { _ = DeleteCurrentRecordAsync(); return true; }
                    }
                    else if (key == Keys.Escape && ViewState.IsQueryMode)
                    {
                        if (IsNavigatorCommandEnabled("query")) { _ = ClearBlockAsync(); return true; }
                    }
                }

                // Ctrl+ shortcuts
                if (modifiers == Keys.Control)
                {
                    if (key == FormsKeyboardShortcuts.CtrlSave) { _ = CommitAsync(); return true; }
                    if (key == FormsKeyboardShortcuts.CtrlQuery) { _ = EnterQueryAsync(); return true; }
                    if (key == FormsKeyboardShortcuts.CtrlExecute) { _ = ExecuteQueryAsync(); return true; }
                    if (key == FormsKeyboardShortcuts.CtrlNew) { _ = CreateRecordAsync(); return true; }
                    if (key == FormsKeyboardShortcuts.CtrlDelete) { _ = DeleteCurrentRecordAsync(); return true; }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[BeepBlock.ProcessCmdKey] {ex.GetType().Name}: {ex.Message}");
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }
    }

    /// <summary>
    /// Customizable mapping of physical keys to Oracle Forms
    /// built-in operations. Reuse the same instance across a form so all
    /// blocks respond identically to a given function key.
    /// </summary>
    public sealed class BeepFormsKeyboardShortcuts
    {
        public Keys EnterQueryKey { get; set; } = Keys.F1;
        public Keys ExecuteQueryKey { get; set; } = Keys.F8;
        public Keys CommitKey { get; set; } = Keys.F10;
        public Keys RollbackKey { get; set; } = Keys.F11;
        public Keys NewRecordKey { get; set; } = Keys.F3;
        public Keys DeleteRecordKey { get; set; } = Keys.F4;
        public Keys CtrlSave { get; set; } = Keys.S;
        public Keys CtrlQuery { get; set; } = Keys.Q;
        public Keys CtrlExecute { get; set; } = Keys.E;
        public Keys CtrlNew { get; set; } = Keys.N;
        public Keys CtrlDelete { get; set; } = Keys.D;

        public BeepFormsKeyboardShortcuts Clone()
        {
            return new BeepFormsKeyboardShortcuts
            {
                EnterQueryKey = EnterQueryKey,
                ExecuteQueryKey = ExecuteQueryKey,
                CommitKey = CommitKey,
                RollbackKey = RollbackKey,
                NewRecordKey = NewRecordKey,
                DeleteRecordKey = DeleteRecordKey,
                CtrlSave = CtrlSave,
                CtrlQuery = CtrlQuery,
                CtrlExecute = CtrlExecute,
                CtrlNew = CtrlNew,
                CtrlDelete = CtrlDelete
            };
        }
    }
}
