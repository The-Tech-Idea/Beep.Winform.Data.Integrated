using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace TheTechIdea.Beep.Winform.Controls.Integrated.Forms.Helpers
{
    internal static class BeepFormsHostResolver
    {
        private static readonly ConditionalWeakTable<Control, BeepForms?> s_cache = new();

        public static BeepForms? Find(Control? source)
        {
            if (source == null) return null;

            if (s_cache.TryGetValue(source, out var cached))
                return cached;

            var result = FindUncached(source);
            s_cache.Add(source, result);
            return result;
        }

        public static void Invalidate(Control? source)
        {
            if (source != null) s_cache.Remove(source);
        }

        private static BeepForms? FindUncached(Control? source)
        {
            Control? current = source?.Parent;
            while (current != null)
            {
                BeepForms? resolved = FindDescendantFormsHost(current, source);
                if (resolved != null)
                    return resolved;

                current = current.Parent;
            }

            return null;
        }

        private static BeepForms? FindDescendantFormsHost(Control parent, Control? exclude)
        {
            var stack = new Stack<Control>();
            stack.Push(parent);

            while (stack.Count > 0)
            {
                Control current = stack.Pop();
                if (!ReferenceEquals(current, exclude) && current is BeepForms formsHost)
                    return formsHost;

                for (int index = current.Controls.Count - 1; index >= 0; index--)
                    stack.Push(current.Controls[index]);
            }

            return null;
        }
    }
}
