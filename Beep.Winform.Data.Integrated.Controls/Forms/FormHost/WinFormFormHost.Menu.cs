using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using TheTechIdea.Beep.Editor.UOWManager.Interfaces;
using TheTechIdea.Beep.Editor.UOWManager.Models;
using TheTechIdea.Beep.Winform.Controls;
using TheTechIdea.Beep.Winform.Controls.Models;

namespace TheTechIdea.Beep.Winform.Data.Integrated.Forms.FormHost
{
    /// <summary>
    /// The form's own menu bar (Oracle Forms menu module). Pure presentation: it
    /// maps the engine's <see cref="FormMenuDefinition"/> tree onto a
    /// <see cref="BeepMenuBar"/> and forwards every leaf click, by id, to the
    /// engine's <see cref="IUnitofWorksManager.InvokeMenuItemAsync"/>. What an
    /// item *does* lives once in the engine — the host never dispatches itself.
    /// The WPF host does the exact same thing (<c>BeepWpfForms.Menu.cs</c>).
    /// </summary>
    public partial class WinFormFormHost
    {
        private BeepMenuBar? _menuBar;
        private FormMenuShellAdapter? _shellAdapter;

        /// <summary>
        /// The form's rendered menu bar, or null when the form has no menu. The
        /// opt-in shell merge reads this to fold the items into the shell menu;
        /// it is also the surface a click drives.
        /// </summary>
        public BeepMenuBar? MenuBar => _menuBar;

        /// <summary>
        /// Rebuilds the menu bar from the current <c>Definition.Menu</c> against
        /// the bound manager. The bar is normally built when the manager is
        /// assigned; call this after setting <c>Definition.Menu</c> on an
        /// already-bound host.
        /// </summary>
        public void RefreshMenu() => RunOnUi(() => RenderMenu(_formsManager));

        /// <summary>Re-renders when the engine's menu manager reports a change.</summary>
        private void ManagerMenuChanged(object? sender, System.EventArgs e) =>
            RunOnUi(() => RenderMenu(_formsManager));

        /// <summary>
        /// (Re)builds the top-docked menu bar from <c>Definition.Menu</c>. A null
        /// manager or an empty menu removes the bar, so a form without a menu
        /// keeps the exact layout it had.
        /// </summary>
        private void RenderMenu(IUnitofWorksManager? manager)
        {
            if (_menuBar is not null)
            {
                Controls.Remove(_menuBar);
                _menuBar.Dispose();
                _menuBar = null;
                Padding = new Padding(Padding.Left, 0, Padding.Right, Padding.Bottom);
            }

            // The runtime menu the emitter registered wins; Definition.Menu is the
            // design-time fallback for a host that has one set but not yet registered.
            var def = manager?.Menu?.GetMenu() ?? Definition?.Menu;
            if (manager is null || def?.Items is null || def.Items.Count == 0)
                return;

            var bar = new BeepMenuBar
            {
                Dock = DockStyle.Top,
                MenuItems = new BindingList<SimpleItem>(FormMenuBinder.BuildItems(def.Items)),
            };
            bar.SelectedItemChanged += (_, e) =>
            {
                var id = e?.SelectedItem?.Name;
                if (!string.IsNullOrWhiteSpace(id))
                    _ = manager.InvokeMenuItemAsync(id);
            };

            Controls.Add(bar);
            bar.BringToFront();                 // dock above designer-parented content
            // Reserve the bar's height so Fill-docked content sits below it.
            Padding = new Padding(Padding.Left, bar.Height, Padding.Right, Padding.Bottom);
            _menuBar = bar;
        }

        /// <summary>
        /// Opt-in: fold this form's menu into a shell menu bar (typically
        /// <c>AppManager.MenuStrip</c>) while the form is active in the Beep
        /// desktop shell, and remove it again with <see cref="UnmergeMenuFromShell"/>.
        /// The per-form bar still renders on the host; this is the additional
        /// shell-level surface. Dispatch stays in the engine — merged items forward
        /// to the same <c>InvokeMenuItemAsync</c>.
        /// </summary>
        public bool MergeMenuIntoShell(object shellMenu)
        {
            UnmergeMenuFromShell();
            if (_formsManager is null) return false;
            _shellAdapter = FormMenuShellAdapter.TryCreate(shellMenu, _formsManager);
            return _shellAdapter?.Attach() == true;
        }

        /// <summary>Removes this form's items from the shell menu bar.</summary>
        public void UnmergeMenuFromShell()
        {
            _shellAdapter?.Detach();
            _shellAdapter = null;
        }
    }
}
