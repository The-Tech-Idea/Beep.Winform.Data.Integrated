using System;
using TheTechIdea.Beep.Editor;

namespace TheTechIdea.Beep.Winform.Controls.Integrated.Forms.Models
{
    public enum BeepFormsUnitOfWorkEventKind
    {
        CurrentChanged,
        ItemChanged,
        PreCreate,
        PostCreate,
        PreQuery,
        PostQuery,
        PreInsert,
        PostInsert,
        PreUpdate,
        PostUpdate,
        PostEdit,
        PreDelete,
        PostDelete,
        PreCommit,
        PostCommit
    }

    public sealed class BeepFormsUnitOfWorkEventArgs : EventArgs
    {
        public string BlockName { get; init; } = string.Empty;
        public BeepFormsUnitOfWorkEventKind EventKind { get; init; }
        public IUnitofWork? UnitOfWork { get; init; }
        public UnitofWorkParams? Parameters { get; init; }
        public object? Item { get; init; }
        public string? PropertyName { get; init; }
        public object? CurrentItem { get; init; }

        public bool IsPreEvent => EventKind is
            BeepFormsUnitOfWorkEventKind.PreCreate or
            BeepFormsUnitOfWorkEventKind.PreQuery or
            BeepFormsUnitOfWorkEventKind.PreInsert or
            BeepFormsUnitOfWorkEventKind.PreUpdate or
            BeepFormsUnitOfWorkEventKind.PreDelete or
            BeepFormsUnitOfWorkEventKind.PreCommit;

        public bool IsPostEvent => EventKind is
            BeepFormsUnitOfWorkEventKind.PostCreate or
            BeepFormsUnitOfWorkEventKind.PostQuery or
            BeepFormsUnitOfWorkEventKind.PostInsert or
            BeepFormsUnitOfWorkEventKind.PostUpdate or
            BeepFormsUnitOfWorkEventKind.PostEdit or
            BeepFormsUnitOfWorkEventKind.PostDelete or
            BeepFormsUnitOfWorkEventKind.PostCommit;

        public string ActivityText => EventKind switch
        {
            BeepFormsUnitOfWorkEventKind.CurrentChanged => "Current record changed",
            BeepFormsUnitOfWorkEventKind.ItemChanged => string.IsNullOrWhiteSpace(PropertyName)
                ? "Field changed"
                : $"Field changed: {PropertyName}",
            _ => EventKind.ToString()
        };
    }
}