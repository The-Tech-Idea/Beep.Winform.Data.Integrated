using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using TheTechIdea.Beep.DragDrop;
using TheTechIdea.Beep.Winform.Controls.Integrated.Blocks;
using TheTechIdea.Beep.Winform.Controls.Integrated.Blocks.Contracts;
using TheTechIdea.Beep.Winform.Controls.Integrated.Blocks.Models;

namespace TheTechIdea.Beep.Winform.Controls.Integrated.Forms;

public partial class BeepForms
{
    private bool _enableDragDrop = true;

    [System.ComponentModel.Browsable(true)]
    [System.ComponentModel.Category("Behavior")]
    [System.ComponentModel.Description("Allow drag-drop of entity definitions from the IDE Beep Data Navigator.")]
    [System.ComponentModel.DefaultValue(true)]
    public bool EnableDragDrop
    {
        get => _enableDragDrop;
        set
        {
            _enableDragDrop = value;
            AllowDrop = value && !DesignMode;
        }
    }

    private void InitializeDragDrop()
    {
        AllowDrop = _enableDragDrop && !DesignMode;
        DragEnter += OnDragEnter;
        DragDrop += OnDragDrop;
    }

    private void OnDragEnter(object? sender, DragEventArgs e)
    {
        if (DesignMode) return;
        if (_formsManager == null) return;

        if (e.Data != null && e.Data.GetDataPresent(typeof(BeepEntityDragPayload)))
        {
            e.Effect = DragDropEffects.Copy;
        }
        else
        {
            e.Effect = DragDropEffects.None;
        }
    }

    private void OnDragDrop(object? sender, DragEventArgs e)
    {
        if (DesignMode) return;

        try
        {
            if (e.Data == null || !e.Data.GetDataPresent(typeof(BeepEntityDragPayload)))
                return;

            var payload = e.Data.GetData(typeof(BeepEntityDragPayload)) as BeepEntityDragPayload;
            if (payload == null || string.IsNullOrWhiteSpace(payload.EntityName))
                return;

            CreateBlockFromPayload(payload);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[BeepForms.DragDrop] {ex.Message}");
        }
    }

    private void CreateBlockFromPayload(BeepEntityDragPayload payload)
    {
        string blockName = !string.IsNullOrWhiteSpace(payload.PreferredBlockName)
            ? payload.PreferredBlockName
            : payload.EntityName;

        if (_blocks.Any(b => string.Equals(b.BlockName, blockName, StringComparison.OrdinalIgnoreCase)))
        {
            blockName = GetUniqueBlockName(blockName);
        }

        var block = new BeepBlock
        {
            BlockName = blockName
        };

        var entityDef = new BeepBlockEntityDefinition
        {
            EntityName = payload.EntityName,
            ConnectionName = payload.ConnectionName,
            IsMasterBlock = payload.IsMaster
        };

        if (!string.IsNullOrWhiteSpace(payload.MasterKeyField))
            entityDef.MasterKeyField = payload.MasterKeyField;

        foreach (var field in payload.Fields)
        {
            entityDef.Fields.Add(new BeepBlockEntityFieldDefinition
            {
                FieldName = field.FieldName,
                DataType = field.DataType,
                IsPrimaryKey = field.IsPrimaryKey,
                IsRequired = field.IsRequired,
                Size = field.FieldSize
            });
        }

        block.Definition = new BeepBlockDefinition
        {
            BlockName = blockName,
            Entity = entityDef
        };

        if (!RegisterBlock(block))
        {
            System.Diagnostics.Debug.WriteLine($"[BeepForms.DragDrop] Failed to register block '{blockName}'");
            return;
        }

        if (_formsManager != null)
        {
            _formsManager.CurrentBlockName = blockName;
            _ = _formsManager.SetupBlockAsync(blockName, payload.ConnectionName!, payload.EntityName, payload.IsMaster).ContinueWith(_ => SyncFromManager(), TaskScheduler.Default);
        }

        SyncFromManager();
        ShowSuccess($"Block '{blockName}' created from {payload.EntityName} ({payload.Fields.Count} fields).");
    }

    private string GetUniqueBlockName(string baseName)
    {
        var existing = new System.Collections.Generic.HashSet<string>(
            _blocks.Select(b => b.BlockName).Where(n => !string.IsNullOrWhiteSpace(n))!,
            StringComparer.OrdinalIgnoreCase);

        if (!existing.Contains(baseName)) return baseName;

        int suffix = 2;
        while (existing.Contains($"{baseName}{suffix}")) suffix++;
        return $"{baseName}{suffix}";
    }
}
