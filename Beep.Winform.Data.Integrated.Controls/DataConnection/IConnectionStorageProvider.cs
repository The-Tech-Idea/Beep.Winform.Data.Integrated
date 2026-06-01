using System.Collections.Generic;
using TheTechIdea.Beep.ConfigUtil;

namespace TheTechIdea.Beep.Winform.Controls
{
    public interface IConnectionStorageProvider
    {
        IReadOnlyList<ConnectionProperties> LoadConnections(
            ConnectionStorageScope scope,
            string profileName,
            bool includePrecedenceChain);

        bool SaveConnections(
            ConnectionStorageScope scope,
            string profileName,
            IReadOnlyList<ConnectionProperties> connections);

        bool AddOrUpdate(
            ConnectionStorageScope scope,
            string profileName,
            ConnectionProperties connection,
            bool persist);

        bool Remove(
            ConnectionStorageScope scope,
            string profileName,
            string connectionName,
            bool persist);

        bool Promote(
            ConnectionStorageScope sourceScope,
            ConnectionStorageScope targetScope,
            string profileName,
            ConnectionConflictPolicy conflictPolicy,
            out string message);

        bool ExportPackage(
            ConnectionStorageScope scope,
            string profileName,
            string packagePath,
            bool includeEncryptedSecretsOnly,
            out string message);

        bool ImportPackage(
            ConnectionStorageScope targetScope,
            string profileName,
            string packagePath,
            ConnectionConflictPolicy conflictPolicy,
            bool importWhenEmptyOnly,
            out string message);
    }
}
