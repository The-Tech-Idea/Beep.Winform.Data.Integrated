using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using TheTechIdea.Beep.Editor.UOWManager.Interfaces;
using TheTechIdea.Beep.Winform.Controls;
using TheTechIdea.Beep.Winform.Controls.Models;

namespace TheTechIdea.Beep.Winform.Data.Integrated.Forms.FormHost
{
    /// <summary>
    /// The opt-in shell merge: folds a form's menu into a shell
    /// <see cref="BeepMenuBar"/> (typically <c>AppManager.MenuStrip</c>) while the
    /// form is active, and removes it again on <see cref="Detach"/>. Oracle Forms
    /// runs one menu bar for the whole application; the Beep shell keeps its own
    /// and lets the active form contribute to it.
    /// <para>
    /// This is presentation glue only. The form's items map onto the shared
    /// <see cref="SimpleItem"/> model, and a selection of one of them forwards, by
    /// id, to the form's <see cref="IUnitofWorksManager.InvokeMenuItemAsync"/> —
    /// the same one-place dispatch the per-form bar uses. The adapter forwards only
    /// selections whose id it owns, so the shell's own menu items are untouched.
    /// </para>
    /// </summary>
    public sealed class FormMenuShellAdapter : IDisposable
    {
        private readonly BeepMenuBar _shellBar;
        private readonly IUnitofWorksManager _manager;
        private readonly List<SimpleItem> _merged = new();
        private readonly HashSet<string> _ids = new(StringComparer.OrdinalIgnoreCase);
        private bool _attached;

        public FormMenuShellAdapter(BeepMenuBar shellBar, IUnitofWorksManager manager)
        {
            _shellBar = shellBar ?? throw new ArgumentNullException(nameof(shellBar));
            _manager = manager ?? throw new ArgumentNullException(nameof(manager));
        }

        /// <summary>
        /// Builds an adapter when the shell menu is a <see cref="BeepMenuBar"/>
        /// (the concrete type behind <c>AppManager.MenuStrip</c>), or null when it
        /// is some other component — so the caller can pass <c>MenuStrip</c>
        /// directly without a cast.
        /// </summary>
        public static FormMenuShellAdapter TryCreate(object shellMenu, IUnitofWorksManager manager)
            => shellMenu is BeepMenuBar bar && manager != null
                ? new FormMenuShellAdapter(bar, manager)
                : null;

        /// <summary>Appends the form's menu to the shell bar. Returns false when the form has no menu.</summary>
        public bool Attach()
        {
            if (_attached) return true;

            var menu = _manager.Menu?.GetMenu();
            if (menu?.Items == null || menu.Items.Count == 0) return false;

            _merged.Clear();
            _ids.Clear();
            _merged.AddRange(FormMenuBinder.BuildItems(menu.Items));
            FormMenuBinder.CollectIds(_merged, _ids);

            // Reassigning MenuItems is what makes the bar recompute its hit areas
            // and repaint — mutating the existing BindingList in place would not.
            var combined = _shellBar.MenuItems.Concat(_merged).ToList();
            _shellBar.MenuItems = new BindingList<SimpleItem>(combined);
            _shellBar.SelectedItemChanged += OnShellItemSelected;

            _attached = true;
            return true;
        }

        /// <summary>Removes this form's items from the shell bar, leaving the shell's own intact.</summary>
        public void Detach()
        {
            if (!_attached) return;

            _shellBar.SelectedItemChanged -= OnShellItemSelected;
            var remaining = _shellBar.MenuItems.Where(item => !_merged.Contains(item)).ToList();
            _shellBar.MenuItems = new BindingList<SimpleItem>(remaining);

            _merged.Clear();
            _ids.Clear();
            _attached = false;
        }

        private void OnShellItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            var id = e?.SelectedItem?.Name;
            if (!string.IsNullOrWhiteSpace(id) && _ids.Contains(id))
                _ = _manager.InvokeMenuItemAsync(id);
        }

        public void Dispose() => Detach();
    }
}
