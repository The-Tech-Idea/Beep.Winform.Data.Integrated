using System.Collections.Generic;
using TheTechIdea.Beep.Editor.UOWManager.Models;
using TheTechIdea.Beep.Winform.Controls.Models;

namespace TheTechIdea.Beep.Winform.Data.Integrated.Forms.FormHost
{
    /// <summary>
    /// Maps the engine's <see cref="FormMenuItem"/> tree onto the shared
    /// <see cref="SimpleItem"/> model that both the per-form <c>BeepMenuBar</c> and
    /// the shell-menu merge render. The engine id rides in
    /// <see cref="SimpleItem.Name"/>; no <c>MethodName</c> is set, so the control
    /// never runs its own global-function dispatch. Behaviour lives once in the
    /// engine — callers forward the id to <c>InvokeMenuItemAsync</c>.
    /// </summary>
    internal static class FormMenuBinder
    {
        public static List<SimpleItem> BuildItems(IEnumerable<FormMenuItem> items)
        {
            var result = new List<SimpleItem>();
            if (items == null) return result;

            foreach (var item in items)
            {
                var simple = new SimpleItem
                {
                    Text = item.Label,
                    Name = item.Id,
                    KeyCombination = item.Shortcut ?? string.Empty,
                };

                if (item.Children is { Count: > 0 })
                {
                    foreach (var child in BuildItems(item.Children))
                        simple.Children.Add(child);
                }

                result.Add(simple);
            }

            return result;
        }

        /// <summary>
        /// Every id in the tree, so a shared bar can tell which selections belong
        /// to this form and forward only those to its manager.
        /// </summary>
        public static void CollectIds(IEnumerable<SimpleItem> items, ISet<string> ids)
        {
            if (items == null) return;
            foreach (var item in items)
            {
                if (!string.IsNullOrEmpty(item.Name)) ids.Add(item.Name);
                CollectIds(item.Children, ids);
            }
        }
    }
}
