using System;
using System.Collections.Generic;
using System.Linq;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Editor.UOWManager.Models;
using TheTechIdea.Beep.Winform.Controls.Integrated.Blocks.Models;

namespace TheTechIdea.Beep.Winform.Controls.Integrated.Blocks
{
    public partial class BeepBlock
    {
        // BeepBlock is UI-only.  All entity structure, field metadata, datasource operations,
        // and business logic live in FormsManager.  BeepBlock reads from FormsManager only.
        // It never accepts EntityStructure directly; it never calls IDataSource.

        private BeepBlockDefinition? _runtimeDefinition;

        private BeepBlockDefinition? EffectiveDefinition => _runtimeDefinition ?? _definition;

        private void RefreshRuntimeDefinition(DataBlockInfo? blockInfo)
        {
            if (blockInfo == null && _definition == null)
            {
                _runtimeDefinition = null;
                UpdateEntityViewState(new BeepBlockEntityDefinition());
                return;
            }

            var baseDefinition = _definition ?? CreateDefinitionShell();
            var entityDefinition = ResolveEntityDefinition(blockInfo, baseDefinition.Entity);
            bool hasExplicitFields = BeepBlockFieldDefinitionStateHelper.HasExplicitFieldDefinitions(baseDefinition);
            string resolvedBlockName = string.IsNullOrWhiteSpace(blockInfo?.BlockName) ? baseDefinition.BlockName : blockInfo.BlockName;

            UpdateEntityViewState(entityDefinition);

            _runtimeDefinition = new BeepBlockDefinition
            {
                Id = string.IsNullOrWhiteSpace(baseDefinition.Id) ? resolvedBlockName : baseDefinition.Id,
                BlockName = string.IsNullOrWhiteSpace(baseDefinition.BlockName) ? resolvedBlockName : baseDefinition.BlockName,
                ManagerBlockName = string.IsNullOrWhiteSpace(baseDefinition.ManagerBlockName) ? resolvedBlockName : baseDefinition.ManagerBlockName,
                Caption = string.IsNullOrWhiteSpace(baseDefinition.Caption)
                    ? ResolveCaption(entityDefinition, resolvedBlockName)
                    : baseDefinition.Caption,
                PresentationMode = baseDefinition.PresentationMode,
                Entity = entityDefinition.Clone(),
                Navigation = baseDefinition.Navigation?.Clone() ?? new BeepBlockNavigationDefinition(),
                Metadata = new Dictionary<string, string>(baseDefinition.Metadata ?? new Dictionary<string, string>(), StringComparer.OrdinalIgnoreCase),
                Fields = hasExplicitFields
                    ? baseDefinition.Fields
                        .Where(x => x != null)
                        .Select(x => x.Clone())
                        .ToList()
                    : entityDefinition.CreateFieldDefinitions()
            };
        }

        private static IEntityStructure? ResolveEntityStructure(DataBlockInfo? blockInfo)
        {
            if (blockInfo?.UnitOfWork?.EntityStructure != null)
            {
                return blockInfo.UnitOfWork.EntityStructure;
            }

            return blockInfo?.EntityStructure;
        }

        public IEnumerable<string> GetAvailableEntityNames()
        {
            var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var entityStructure = ResolveEntityStructure(TryGetManagerBlockInfo());
            if (!string.IsNullOrWhiteSpace(entityStructure?.EntityName))
            {
                names.Add(entityStructure.EntityName);
            }

            if (!string.IsNullOrWhiteSpace(_definition?.Entity?.EntityName))
            {
                names.Add(_definition.Entity.EntityName);
            }

            return names.OrderBy(static name => name).ToArray();
        }

        public IEnumerable<string> GetAvailableConnectionNames()
        {
            var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var blockInfo = TryGetManagerBlockInfo();
            if (!string.IsNullOrWhiteSpace(blockInfo?.DataSourceName))
            {
                names.Add(blockInfo.DataSourceName);
            }

            if (!string.IsNullOrWhiteSpace(_definition?.Entity?.ConnectionName))
            {
                names.Add(_definition.Entity.ConnectionName);
            }

            var connections = _formsHost?.GetAvailableConnectionNames();
            if (connections != null)
            {
                foreach (var name in connections)
                {
                    if (!string.IsNullOrWhiteSpace(name))
                    {
                        names.Add(name);
                    }
                }
            }

            return names.OrderBy(static name => name).ToArray();
        }

        private DataBlockInfo? TryGetManagerBlockInfo()
        {
            if (_formsHost == null || string.IsNullOrWhiteSpace(ManagerBlockName) || !_formsHost.IsBlockRegistered(ManagerBlockName))
            {
                return null;
            }

            return _formsHost.GetBlockInfo(ManagerBlockName);
        }

        private static string ResolveCaption(BeepBlockEntityDefinition entityDefinition, string fallbackBlockName)
        {
            if (!string.IsNullOrWhiteSpace(entityDefinition.Caption))
            {
                return entityDefinition.Caption;
            }

            if (!string.IsNullOrWhiteSpace(entityDefinition.EntityName))
            {
                return entityDefinition.EntityName;
            }

            return fallbackBlockName;
        }

        private static BeepBlockEntityDefinition ResolveEntityDefinition(DataBlockInfo? blockInfo, BeepBlockEntityDefinition? persistedEntity)
        {
            var entityStructure = ResolveEntityStructure(blockInfo);
            if (entityStructure != null)
            {
                return CreateEntityDefinition(blockInfo, entityStructure);
            }

            return persistedEntity?.Clone() ?? new BeepBlockEntityDefinition();
        }

        // Phase 7B: promoted to internal so design-time and bootstrapper code can call it.
        internal static BeepBlockEntityDefinition CreateEntityDefinition(DataBlockInfo? blockInfo, IEntityStructure entityStructure)
        {
            var primaryKeyNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (entityStructure.PrimaryKeys != null)
            {
                foreach (var primaryKey in entityStructure.PrimaryKeys)
                {
                    if (!string.IsNullOrWhiteSpace(primaryKey?.FieldName))
                    {
                        primaryKeyNames.Add(primaryKey.FieldName);
                    }
                }
            }

            var entityDefinition = new BeepBlockEntityDefinition
            {
                ConnectionName = blockInfo?.DataSourceName ?? string.Empty,
                EntityName = entityStructure.EntityName ?? string.Empty,
                DatasourceEntityName = entityStructure.DatasourceEntityName ?? string.Empty,
                Caption = entityStructure.Caption ?? string.Empty,
                Description = entityStructure.Description ?? string.Empty,
                DataSourceId = entityStructure.DataSourceID ?? string.Empty,
                IsMasterBlock = blockInfo?.IsMasterBlock ?? false,
                MasterBlockName = blockInfo?.MasterBlockName ?? string.Empty,
                MasterKeyField = blockInfo?.MasterKeyField ?? string.Empty,
                ForeignKeyField = blockInfo?.ForeignKeyField ?? string.Empty
            };

            var fields = entityStructure.Fields;
            if (fields != null)
            {
                for (int index = 0; index < fields.Count; index++)
                {
                    var field = fields[index];
                    if (field == null || string.IsNullOrWhiteSpace(field.FieldName))
                    {
                        continue;
                    }

                    entityDefinition.Fields.Add(new BeepBlockEntityFieldDefinition
                    {
                        FieldName = field.FieldName,
                        Label = string.IsNullOrWhiteSpace(field.Caption) ? field.FieldName : field.Caption,
                        Description = field.Description ?? string.Empty,
                        DataType = field.Fieldtype ?? string.Empty,
                        Category = field.FieldCategory,
                        Order = field.OrdinalPosition > 0 ? field.OrdinalPosition : index,
                        Size = field.Size,
                        NumericPrecision = field.NumericPrecision,
                        NumericScale = field.NumericScale,
                        IsRequired = field.IsRequired,
                        AllowDBNull = field.AllowDBNull,
                        IsPrimaryKey = field.IsKey || primaryKeyNames.Contains(field.FieldName),
                        IsUnique = field.IsUnique,
                        IsIndexed = field.IsIndexed,
                        IsAutoIncrement = field.IsAutoIncrement,
                        IsReadOnly = field.IsReadOnly,
                        IsCheck = field.IsCheck,
                        // Phase 7A: lossless fields
                        IsIdentity = field.IsIdentity,
                        IsHidden = field.IsHidden,
                        IsLong = field.IsLong,
                        IsRowVersion = field.IsRowVersion,
                        DefaultValue = field.DefaultValue ?? string.Empty
                    });
                }
            }

            return entityDefinition;
        }

        private void UpdateEntityViewState(BeepBlockEntityDefinition entityDefinition)
        {
            var snapshot = entityDefinition?.Clone() ?? new BeepBlockEntityDefinition();
            _viewState.Entity = snapshot;
            _viewState.EntityName = snapshot.EntityName ?? string.Empty;
            _viewState.ConnectionName = snapshot.ConnectionName ?? string.Empty;
            _viewState.FieldCount = snapshot.Fields?.Count ?? 0;
        }
    }
}