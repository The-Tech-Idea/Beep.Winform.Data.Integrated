using System.Collections.Generic;
using System.Windows.Forms;

namespace TheTechIdea.Beep.Winform.Controls.Integrated.Forms.Helpers
{
    internal static class BeepFormsHostResolver
    {
        public static BeepForms? Find(Control? source)
        {
            Control? current = source?.Parent;
            while (current != null)
            {
                BeepForms? resolved = FindDescendantFormsHost(current, source);
                if (resolved != null)
                {
                    return resolved;
                }

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
                {
                    return formsHost;
                }

                for (int index = current.Controls.Count - 1; index >= 0; index--)
                {
                    stack.Push(current.Controls[index]);
                }
            }

            return null;
        }
    }
}