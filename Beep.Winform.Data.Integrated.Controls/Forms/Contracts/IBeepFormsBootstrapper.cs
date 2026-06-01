using System;
using System.Threading;
using System.Threading.Tasks;

namespace TheTechIdea.Beep.Winform.Controls.Integrated.Forms.Contracts
{
    /// <summary>
    /// Populates <see cref="Beep.Winform.Controls.Integrated.Blocks.BeepBlock"/> instances owned by a
    /// <see cref="BeepForms"/> host with <see cref="TheTechIdea.Beep.DataBase.EntityStructure"/> metadata
    /// from a live datasource.
    /// </summary>
    public interface IBeepFormsBootstrapper
    {
        /// <summary>
        /// Apply schema metadata for all blocks declared in <paramref name="forms"/> whose
        /// <see cref="Forms.Models.BeepFormsDefinition"/> carries a valid ConnectionName and EntityName.
        /// Implementations must not throw; swallow and surface failures via the return value.
        /// </summary>
        /// <param name="forms">The <see cref="BeepForms"/> host to bootstrap.</param>
        /// <param name="cancellationToken">Token to observe for cancellation.</param>
        /// <returns><see langword="true"/> if all blocks were populated; <see langword="false"/> if any block failed or was skipped.</returns>
        Task<bool> BootstrapAsync(BeepForms forms, CancellationToken cancellationToken = default);
    }
}
