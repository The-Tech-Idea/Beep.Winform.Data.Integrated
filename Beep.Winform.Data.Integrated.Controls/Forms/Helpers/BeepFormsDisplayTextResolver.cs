using System.Collections.Generic;
using TheTechIdea.Beep.Editor.UOWManager.Models;
using TheTechIdea.Beep.Winform.Controls.Integrated.Forms.Contracts;
using TheTechIdea.Beep.Editor.Forms.Models;
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
                return string.Empty;

            if (formsHost == null)
                return "No BeepForms host attached.";

            var parts = new List<string>();

            if (includeActiveBlock)
            {
                int blockCount = formsHost.Blocks.Count;
                if (blockCount > 0)
                {
                    parts.Add($"{blockCount} block(s)");
                    if (!string.IsNullOrWhiteSpace(formsHost.ViewState.ActiveBlockName))
                        parts.Add($"Active: {formsHost.ViewState.ActiveBlockName}");
                }
                else
                {
                    parts.Add("No blocks");
                }
            }

            if (includeStateSummary)
            {
                if (formsHost.ViewState.IsQueryMode)
                    parts.Add("Query mode");

                if (formsHost.ViewState.IsDirty)
                    parts.Add("Unsaved changes");

                if (formsHost.ViewState.ErrorCount > 0)
                    parts.Add($"{formsHost.ViewState.ErrorCount} error(s)");
            }

            return parts.Count == 0 ? "Ready" : string.Join("  ·  ", parts);
        }

        public static string ResolveStatusText(IBeepFormsHost? formsHost)
        {
            if (formsHost == null)
            {
                return "No BeepForms host attached.";
            }

            var parts = new List<string>();

            string mode = ResolveModeText(formsHost);
            if (!string.IsNullOrWhiteSpace(mode))
                parts.Add(mode);

            if (!string.IsNullOrWhiteSpace(formsHost.ViewState.ActiveBlockName))
                parts.Add(formsHost.ViewState.ActiveBlockName);

            if (!string.IsNullOrWhiteSpace(formsHost.ViewState.RecordPositionText))
                parts.Add(string.Concat("Rec ", formsHost.ViewState.RecordPositionText));

            if (formsHost.ViewState.ErrorCount > 0)
                parts.Add(string.Concat(formsHost.ViewState.ErrorCount, " err"));

            if (formsHost.ViewState.IsDirty)
                parts.Add("Dirty");

            if (!string.IsNullOrWhiteSpace(formsHost.ViewState.AggregateText))
                parts.Add(formsHost.ViewState.AggregateText);

            if (!string.IsNullOrWhiteSpace(formsHost.ViewState.ConnectionName))
                parts.Add(formsHost.ViewState.ConnectionName);

            if (!string.IsNullOrWhiteSpace(formsHost.ViewState.StatusText))
                parts.Add(formsHost.ViewState.StatusText.Trim());

            return parts.Count > 0 ? string.Join("  |  ", parts) : "Ready.";
        }

        private static string ResolveModeText(IBeepFormsHost? formsHost)
        {
            if (formsHost == null)
                return string.Empty;

            if (formsHost.ViewState.BootstrapState == BootstrapState.Running)
                return "Loading";

            if (formsHost.ViewState.IsQueryMode)
            {
                if (formsHost.FormsManager != null
                    && !string.IsNullOrWhiteSpace(formsHost.ActiveBlockName)
                    && formsHost.FormsManager.BlockExists(formsHost.ActiveBlockName))
                {
                    var mode = formsHost.FormsManager.GetBlock(formsHost.ActiveBlockName)?.Mode;
                    if (mode == DataBlockMode.Query)
                        return "Enter Query";
                    if (mode == DataBlockMode.Insert)
                        return "Insert";
                }
                return "Query";
            }

            return "Normal";
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