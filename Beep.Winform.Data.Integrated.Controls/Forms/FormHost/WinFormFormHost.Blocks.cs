using TheTechIdea.Beep.Editor.Forms.Hosts;

namespace TheTechIdea.Beep.Winform.Data.Integrated.Forms.FormHost;

public partial class WinFormFormHost
{
    public bool RegisterBlock(object blockView)
    {
        var result = false;
        RunOnUi(() => result = RegisterBlockCore(blockView));
        return result;
    }

    private bool RegisterBlockCore(object blockView)
    {
        if (blockView is not IBlockView block)
        {
            throw new ArgumentException(
                "The registered object must implement IBlockView.",
                nameof(blockView));
        }

        if (block.View is not Control)
        {
            throw new ArgumentException(
                "The block view must be a WinForms Control.",
                nameof(blockView));
        }

        var blockName = NormalizeBlockName(block.BlockName);
        if (_blocks.ContainsKey(blockName))
        {
            throw new InvalidOperationException(
                $"A block named '{blockName}' is already registered.");
        }

        var previousActiveBlockName = _activeBlockName;
        var wasBound = block.IsBound;
        var bindAttempted = false;
        _blocks.Add(blockName, block);
        try
        {
            if (_formsManager?.BlockExists(blockName) == true)
            {
                bindAttempted = true;
                block.Bind(this);
            }

            if (_activeBlockName is null)
            {
                SetActiveBlock(blockName);
            }
        }
        catch (Exception originalException)
        {
            try
            {
                if (bindAttempted && !wasBound)
                {
                    block.Unbind();
                }
            }
            catch (Exception cleanupException)
            {
                AttachCleanupException(
                    originalException,
                    "RegistrationBindCleanup",
                    cleanupException);
            }
            finally
            {
                _blocks.Remove(blockName);
                _activeBlockName = previousActiveBlockName;
            }

            throw;
        }

        return true;
    }

    public bool UnregisterBlock(string blockName)
    {
        var result = false;
        RunOnUi(() => result = UnregisterBlockCore(blockName));
        return result;
    }

    private bool UnregisterBlockCore(string blockName)
    {
        if (!TryNormalizeBlockName(blockName, out var normalizedName) ||
            !_blocks.TryGetValue(normalizedName, out var block))
        {
            return false;
        }

        var previousActiveBlockName = _activeBlockName;
        var wasBound = block.IsBound;
        try
        {
            block.Unbind();
            _blocks.Remove(normalizedName);

            if (string.Equals(
            _activeBlockName,
            normalizedName,
            StringComparison.OrdinalIgnoreCase))
            {
                SetActiveBlock(null);
            }

            return true;
        }
        catch
        {
            _blocks[normalizedName] = block;
            _activeBlockName = previousActiveBlockName;
            if (wasBound && !block.IsBound)
            {
                block.Bind(this);
            }

            throw;
        }
    }

    public bool TrySetActiveBlock(string blockName)
    {
        var result = false;
        RunOnUi(() => result = TrySetActiveBlockCore(blockName));
        return result;
    }

    private bool TrySetActiveBlockCore(string blockName)
    {
        if (!TryNormalizeBlockName(blockName, out var normalizedName) ||
            !_blocks.TryGetValue(normalizedName, out var block))
        {
            return false;
        }

        SetActiveBlock(NormalizeBlockName(block.BlockName));
        return true;
    }

    public bool IsBlockRegistered(string blockName)
    {
        var result = false;
        RunOnUi(() =>
            result = TryNormalizeBlockName(blockName, out var normalizedName) &&
                     _blocks.ContainsKey(normalizedName));
        return result;
    }

    public object? GetCurrentBlockItem(string blockName) =>
        TryReadManager(
            manager => manager.GetUnitOfWork(
                NormalizeBlockName(blockName))?.CurrentItem,
            default(object));

    public int GetCurrentBlockRecordIndex(string blockName) =>
        TryReadManager(
            manager => (int?)manager.GetUnitOfWork(
                NormalizeBlockName(blockName))?.Units?.CurrentIndex ?? -1,
            -1);

    public object? GetFieldValue(string blockName, string fieldName) =>
        TryReadManager(
            manager =>
            {
                var record = manager.GetUnitOfWork(
                    NormalizeBlockName(blockName))?.CurrentItem;
                return record is null
                    ? null
                    : manager.GetFieldValue(record, fieldName);
            },
            default(object));

    public bool SetFieldValue(string blockName, string fieldName, object? value) =>
        TryReadManager(
            manager =>
            {
                var normalizedBlockName = NormalizeBlockName(blockName);
                var record = manager.GetUnitOfWork(
                    normalizedBlockName)?.CurrentItem;
                if (record is null ||
                    !manager.SetFieldValue(record, fieldName, value))
                {
                    return false;
                }

                return manager.ValidateField(
                    normalizedBlockName,
                    fieldName,
                    value!);
            },
            false);

    private void SetActiveBlock(string? blockName)
    {
        if (string.Equals(
            _activeBlockName,
            blockName,
            StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var previousActiveBlockName = _activeBlockName;
        _activeBlockName = blockName;
        try
        {
            ActiveBlockChanged?.Invoke(this, EventArgs.Empty);
        }
        catch
        {
            _activeBlockName = previousActiveBlockName;
            throw;
        }
    }

    private static string NormalizeBlockName(string blockName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(blockName);
        return blockName.Trim();
    }

    private static bool TryNormalizeBlockName(
        string? blockName,
        out string normalizedName)
    {
        if (string.IsNullOrWhiteSpace(blockName))
        {
            normalizedName = string.Empty;
            return false;
        }

        normalizedName = blockName.Trim();
        return true;
    }
}
