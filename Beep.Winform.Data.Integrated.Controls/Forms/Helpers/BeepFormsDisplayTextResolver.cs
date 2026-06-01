using System.Collections.Generic;
using TheTechIdea.Beep.Winform.Controls.Integrated.Forms.Contracts;
using TheTechIdea.Beep.Winform.Controls.Integrated.Forms.Models;

namespace TheTechIdea.Beep.Winform.Controls.Integrated.Forms.Helpers
{
    internal static class BeepFormsDisplayTextResolver
    {
        public static string ResolveTitle(IBeepFormsHost? formsHost)
        {
            if (formsHost == null)
            {
                return "BeepForms";
            }

            if (!string.IsNullOrWhiteSpace(formsHost.Definition?.Title))
            {
                return formsHost.Definition.Title.Trim();
            }

            if (!string.IsNullOrWhiteSpace(formsHost.FormName))
            {
                return formsHost.FormName.Trim();
            }

            if (!string.IsNullOrWhiteSpace(formsHost.Definition?.FormName))
            {
                return formsHost.Definition.FormName.Trim();
            }

            return "BeepForms";
        }

        public static string ResolveContext(IBeepFormsHost? formsHost, bool includeActiveBlock, bool includeStateSummary)
        {
            if (!includeActiveBlock && !includeStateSummary)
            {
                return string.Empty;
            }

            if (formsHost == null)
            {
                return "No BeepForms host attached.";
            }

            var parts = new List<string>();

            if (includeActiveBlock)
            {
                if (!string.IsNullOrWhiteSpace(formsHost.ViewState.ActiveBlockName))
                {
                    parts.Add($"Block: {formsHost.ViewState.ActiveBlockName}");
                }
                else if (formsHost.Blocks.Count > 0)
                {
                    parts.Add("Block: none selected");
                }
            }

            if (includeStateSummary)
            {
                if (formsHost.ViewState.IsQueryMode)
                {
                    parts.Add("Query mode");
                }

                if (formsHost.ViewState.IsDirty)
                {
                    parts.Add("Pending changes");
                }
            }

            return parts.Count == 0 ? "Ready" : string.Join(" | ", parts);
        }

        public static string ResolveStatusText(IBeepFormsHost? formsHost)
        {
            if (formsHost == null)
            {
                return "No BeepForms host attached.";
            }

            if (!string.IsNullOrWhiteSpace(formsHost.ViewState.StatusText))
            {
                return formsHost.ViewState.StatusText.Trim();
            }

            if (formsHost.ViewState.IsDirty)
            {
                return "Pending changes.";
            }

            if (formsHost.ViewState.IsQueryMode)
            {
                return "Query mode active.";
            }

            return "Ready.";
        }

        public static string ResolveQueryTargetCaption(IBeepFormsHost? formsHost)
        {
            return ResolveQueryTargetCaption(formsHost, BeepFormsQueryShelfCaptionMode.TargetPlusMode);
        }

        public static string ResolveQueryTargetCaption(IBeepFormsHost? formsHost, BeepFormsQueryShelfCaptionMode captionMode)
        {
            if (captionMode == BeepFormsQueryShelfCaptionMode.TitleOnly)
            {
                return "Query Actions";
            }

            if (formsHost == null)
            {
                return captionMode == BeepFormsQueryShelfCaptionMode.TargetOnly
                    ? "Query target unavailable."
                    : "Query target: no BeepForms host attached.";
            }

            if (string.IsNullOrWhiteSpace(formsHost.ViewState.ActiveBlockName))
            {
                return formsHost.Blocks.Count > 0
                    ? "Query target: no active block selected."
                    : "Query target: no blocks available.";
            }

            if (captionMode == BeepFormsQueryShelfCaptionMode.TargetOnly)
            {
                return $"Query target: {formsHost.ViewState.ActiveBlockName}";
            }

            return formsHost.ViewState.IsQueryMode
                ? $"Query target: {formsHost.ViewState.ActiveBlockName} | Query mode active"
                : $"Query target: {formsHost.ViewState.ActiveBlockName} | Ready to enter query mode";
        }
    }
}