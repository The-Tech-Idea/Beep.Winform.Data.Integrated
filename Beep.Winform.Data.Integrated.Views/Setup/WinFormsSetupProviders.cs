using System.Threading;
using System.Threading.Tasks;
using TheTechIdea.Beep.SetUp;
using TheTechIdea.Beep.SetUp.Rollback;
using TheTechIdea.Beep.SetUp.Security;

namespace TheTechIdea.Beep.Winform.Default.Views.Setup
{
    /// <summary>
    /// Shared UI-thread marshalling for the setup provider hooks.
    /// </summary>
    /// <remarks>
    /// <c>SchemaSetupStep.Execute</c> calls both providers as
    /// <c>Task.Run(() =&gt; provider.XxxAsync(...)).GetAwaiter().GetResult()</c>, i.e. from a
    /// thread-pool thread. Showing a dialog directly from there would throw or misbehave, so every
    /// prompt is marshalled onto the owning control's thread. That thread is free to pump messages
    /// while this runs, because the pipeline executes on a worker thread inside
    /// <c>SetupWizardAdapterBase.RunAsync</c> — so the Invoke completes rather than deadlocking.
    /// </remarks>
    internal static class SetupPromptMarshal
    {
        public static T OnUiThread<T>(Control owner, Func<T> prompt, T fallback)
        {
            // No usable owner: don't guess an answer for a gate this important.
            if (owner == null || owner.IsDisposed || !owner.IsHandleCreated)
                return fallback;

            try
            {
                return owner.InvokeRequired
                    ? (T)owner.Invoke(prompt)
                    : prompt();
            }
            catch (ObjectDisposedException)
            {
                // The host went away mid-run; fail closed rather than assume consent.
                return fallback;
            }
            catch (InvalidOperationException)
            {
                // Handle destroyed between the check and the Invoke — same reasoning.
                return fallback;
            }
        }
    }

    /// <summary>
    /// Asks a human to approve the schema plan, instead of the framework self-approving it.
    /// </summary>
    /// <remarks>
    /// The solo default (<c>AutoApprovalProvider</c>) grants automatically and records
    /// <c>IsSelfApproved = true</c>. This provider still reports <c>IsSelfApproved = true</c>, because
    /// on a desktop the person clicking Yes *is* the person who started the run — that is a real
    /// self-approval, and laundering it into a two-party sign-off would be a lie. Use
    /// <c>SeparationOfDutyApprovalProvider</c> where a distinct approver is genuinely required.
    /// </remarks>
    internal sealed class WinFormsSetupApprovalProvider : ISetupApprovalProvider
    {
        private readonly Control _owner;

        public WinFormsSetupApprovalProvider(Control owner)
            => _owner = owner ?? throw new ArgumentNullException(nameof(owner));

        public Task<SetupApproval> RequestApprovalAsync(
            SetupContext context, ISetupPrincipal principal, string planHash,
            CancellationToken token = default)
        {
            var id = principal?.Id ?? Environment.UserName ?? "anonymous";
            var label = principal?.DisplayName ?? id;
            var environment = context?.Options?.Environment ?? "Development";

            if (token.IsCancellationRequested)
                return Task.FromResult(Denied(planHash, "Cancelled before approval."));

            var granted = SetupPromptMarshal.OnUiThread(_owner, () =>
                MessageBox.Show(
                    _owner,
                    $"Apply schema changes to the '{environment}' environment?\n\n" +
                    $"Plan: {planHash}\n" +
                    $"Requested by: {label}\n\n" +
                    "This will modify the database schema.",
                    "Approve Schema Migration",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning) == DialogResult.Yes,
                fallback: false);

            if (!granted)
                return Task.FromResult(Denied(planHash, "Approval was declined by the operator."));

            return Task.FromResult(new SetupApproval
            {
                Granted = true,
                ApproverId = id,
                ApproverLabel = label,
                ApprovedAt = DateTimeOffset.UtcNow,
                PlanHash = planHash,
                IsSelfApproved = true,
                Note = $"Approved interactively in the WinForms setup wizard by {label}."
            });
        }

        private static SetupApproval Denied(string planHash, string note) => new()
        {
            Granted = false,
            PlanHash = planHash,
            Note = note
        };
    }

    /// <summary>
    /// Asks a human whether a backup actually exists before a schema change runs.
    /// </summary>
    /// <remarks>
    /// The default (<c>NoBackupConfirmationProvider</c>) always answers false and warns. This asks
    /// rather than assumes, and defaults to <b>false</b> on every uncertain path — an unverified
    /// backup must never be reported as confirmed, because <c>CheckRollbackReadiness</c> trusts this
    /// answer.
    /// </remarks>
    internal sealed class WinFormsBackupConfirmationProvider : IBackupConfirmationProvider
    {
        private readonly Control _owner;

        public WinFormsBackupConfirmationProvider(Control owner)
            => _owner = owner ?? throw new ArgumentNullException(nameof(owner));

        public Task<bool> IsBackupConfirmedAsync(SetupContext context, CancellationToken token = default)
        {
            if (token.IsCancellationRequested)
                return Task.FromResult(false);

            var ds = context?.DataSource?.DatasourceName ?? "the target datasource";

            var confirmed = SetupPromptMarshal.OnUiThread(_owner, () =>
                MessageBox.Show(
                    _owner,
                    $"Has a restorable backup of '{ds}' been taken?\n\n" +
                    "Answer Yes only if a backup genuinely exists — this answer is recorded as " +
                    "rollback-readiness evidence and StrictPolicyMode relies on it.",
                    "Confirm Backup",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning) == DialogResult.Yes,
                fallback: false);

            return Task.FromResult(confirmed);
        }
    }
}
