# Phase 9: Reflection Cleanup & Hardening

**Priority:** Medium | **Est. Tasks:** 5

## Goal

Replace reflection-based savepoint operations in `BeepForms.WorkflowShell.cs` with direct interface calls. Remove tech debt.

## Current Problem

`BeepForms.WorkflowShell.cs` lines 271-333 use runtime reflection (`MethodInfo.Invoke`) to call `CreateBlockSavepoint` and `RollbackToSavepointAsync` because those methods were not guaranteed to exist on `IUnitofWorksManager`. The methods now exist on the `ISavepointManager` interface (accessible via `_formsManager.Savepoints`).

## Code Before (Reflection)

```csharp
// WorkflowShell.cs:271-291
private string? TryCreateBlockSavepoint(string blockName, string? savepointName)
{
    if (_formsManager == null) return null;

    MethodInfo? wrapperMethod = _formsManager.GetType().GetMethod(
        "CreateBlockSavepoint",
        BindingFlags.Instance | BindingFlags.Public,
        binder: null,
        types: new[] { typeof(string), typeof(string) },
        modifiers: null);

    if (wrapperMethod != null)
    {
        try
        {
            return wrapperMethod.Invoke(_formsManager, new object?[] { blockName, savepointName }) as string;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"...");
        }
    }

    return _formsManager.Savepoints.CreateSavepoint(blockName, savepointName);
}
```

```csharp
// WorkflowShell.cs:293-333
private async Task<bool> TryRollbackToSavepointViaManagerAsync(
    string blockName, string savepointName, CancellationToken cancellationToken)
{
    if (_formsManager == null) return false;

    MethodInfo? wrapperMethod = _formsManager.GetType().GetMethod(
        "RollbackToSavepointAsync",
        BindingFlags.Instance | BindingFlags.Public,
        binder: null,
        types: new[] { typeof(string), typeof(string), typeof(CancellationToken) },
        modifiers: null);

    if (wrapperMethod == null) return false;

    try
    {
        object? taskObject = wrapperMethod.Invoke(_formsManager, 
            new object?[] { blockName, savepointName, cancellationToken });
        if (taskObject is Task<bool> typedTask)
            return await typedTask.ConfigureAwait(true);
        if (taskObject is Task untypedTask)
        {
            await untypedTask.ConfigureAwait(true);
            return true;
        }
        return false;
    }
    catch (Exception ex) { ... return false; }
}
```

## Code After (Direct Interface Call)

```csharp
private string? TryCreateBlockSavepoint(string blockName, string? savepointName)
{
    if (_formsManager?.Savepoints == null) return null;

    try
    {
        return _formsManager.Savepoints.CreateSavepoint(blockName, savepointName);
    }
    catch (Exception ex)
    {
        System.Diagnostics.Debug.WriteLine(
            $"[BeepForms.TryCreateBlockSavepoint] {ex.GetType().Name} - {ex.Message}");
        return null;
    }
}
```

```csharp
private async Task<bool> TryRollbackToSavepointViaManagerAsync(
    string blockName, string savepointName, CancellationToken cancellationToken)
{
    if (_formsManager?.Savepoints == null) return false;

    try
    {
        return await _formsManager.Savepoints.RollbackToSavepointAsync(
            blockName, savepointName, cancellationToken).ConfigureAwait(true);
    }
    catch (Exception ex)
    {
        System.Diagnostics.Debug.WriteLine(
            $"[BeepForms.TryRollbackToSavepointViaManagerAsync] {ex.GetType().Name} - {ex.Message}");
        return false;
    }
}
```

## Implementation Checklist

- [ ] Replace `TryCreateBlockSavepoint` reflection with direct `_formsManager.Savepoints.CreateSavepoint()` call
- [ ] Replace `TryRollbackToSavepointViaManagerAsync` reflection with direct `_formsManager.Savepoints.RollbackToSavepointAsync()` call
- [ ] Add null-guard for `_formsManager?.Savepoints` (check both null before access)
- [ ] Verify savepoint create/list/release/rollback still work after removal
- [ ] Remove `using System.Reflection;` from `WorkflowShell.cs` if no longer needed (check other usages)
- [ ] Verify no other reflection calls exist in the Forms directory that could be replaced

## Verification

1. Attach a FormsManager with `Savepoints` property set
2. Call `BeepForms.CreateSavepoint("TEST_BLOCK")` in a test form
3. Verify savepoint is created (returns a non-null name)
4. Call `BeepForms.ListSavepoints("TEST_BLOCK")` — verify savepoint appears
5. Call `BeepForms.RollbackToSavepointAsync("savepointName", "TEST_BLOCK")` — verify rollback succeeds
6. Call `BeepForms.ReleaseSavepoint("savepointName", "TEST_BLOCK")` — verify release succeeds
7. Call `BeepForms.ReleaseAllSavepoints("TEST_BLOCK")` — verify all released
8. Attach null Savepoints — verify graceful null handling (no NRE)

## Risks

- **Very Low:** The reflection code already fell back to `_formsManager.Savepoints.CreateSavepoint` when reflection failed. The replacement is strictly simpler and uses the same path.
- The only risk is if `ISavepointManager.RollbackToSavepointAsync` has a different signature than the one used in reflection. Verify: `RollbackToSavepointAsync(string blockName, string savepointName, CancellationToken cancellationToken)` returns `Task<bool>`. The engine's `SavepointManager` has this exact signature.
