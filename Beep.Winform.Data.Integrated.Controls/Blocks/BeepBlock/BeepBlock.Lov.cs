using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using TheTechIdea.Beep.Editor.UOWManager.Interfaces;
using TheTechIdea.Beep.Editor.UOWManager.Models;
using TheTechIdea.Beep.Winform.Controls;
using TheTechIdea.Beep.Winform.Controls.Buttons;
using TheTechIdea.Beep.Winform.Controls.Models;
using TheTechIdea.Beep.Winform.Controls.ThemeManagement;

namespace TheTechIdea.Beep.Winform.Controls.Integrated.Blocks
{
    public partial class BeepBlock
    {
        private const int LovPopupSearchDebounceMs = 250;
        private readonly Dictionary<string, List<SimpleItem>> _lovRecentSelections = new(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> _activeLovPopupFields = new(StringComparer.OrdinalIgnoreCase);

        private Control CreateRecordEditorHost(Control editor, Models.BeepFieldDefinition fieldDefinition)
        {
            if (editor == null)
            {
                throw new ArgumentNullException(nameof(editor));
            }

            editor.Height = 30;

            if (editor is not BeepComboBox comboBox || !ShouldUseLovPicker(fieldDefinition))
            {
                editor.Dock = DockStyle.Bottom;
                return editor;
            }

            comboBox.Dock = DockStyle.Fill;
            comboBox.Margin = new Padding(0);
            AttachLovPickerHandler(comboBox);

            TableLayoutPanel contentHost = new()
            {
                Dock = DockStyle.Bottom,
                Height = 30,
                ColumnCount = 2,
                RowCount = 1,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };
            contentHost.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            contentHost.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 44f));

            contentHost.Controls.Add(comboBox, 0, 0);
            contentHost.Controls.Add(CreateLovPickerButton(comboBox, fieldDefinition), 1, 0);

            return contentHost;
        }

        private bool ShouldUseLovPicker(Models.BeepFieldDefinition fieldDefinition)
        {
            if (fieldDefinition == null || string.IsNullOrWhiteSpace(fieldDefinition.FieldName))
            {
                return false;
            }

            if (fieldDefinition.Options != null && fieldDefinition.Options.Count > 0)
            {
                return false;
            }

            return TryGetLovContext(fieldDefinition.FieldName, out _);
        }

        private BeepButton CreateLovPickerButton(BeepComboBox comboBox, Models.BeepFieldDefinition fieldDefinition)
        {
            string fieldLabel = ResolveLovFieldLabel(fieldDefinition);

            BeepButton pickerButton = new()
            {
                Dock = DockStyle.Fill,
                Text = "...",
                Theme = Theme,
                UseThemeColors = true,
                IsChild = true,
                ShowShadow = false,
                HideText = false,
                Margin = new Padding(8, 0, 0, 0),
                ToolTipText = $"Show list of values for {fieldLabel} (F9)",
                Enabled = comboBox.Enabled
            };

            pickerButton.Click += async (_, _) =>
                await OpenLovPickerAsync(comboBox, fieldDefinition, comboBox.Text).ConfigureAwait(true);

            return pickerButton;
        }

        private void AttachLovPickerHandler(BeepComboBox comboBox)
        {
            comboBox.KeyDown -= LovComboBox_KeyDown;
            comboBox.KeyDown += LovComboBox_KeyDown;
        }

        private async void LovComboBox_KeyDown(object? sender, KeyEventArgs e)
        {
            if (sender is not BeepComboBox comboBox || e.KeyCode != Keys.F9)
            {
                return;
            }

            e.Handled = true;
            e.SuppressKeyPress = true;
            await OpenLovPickerAsync(comboBox, ResolveLovFieldDefinition(comboBox.DataSourceProperty), comboBox.Text).ConfigureAwait(true);
        }

        private async Task OpenLovPickerAsync(BeepComboBox comboBox, Models.BeepFieldDefinition? fieldDefinition, string? preloadSearch)
        {
            if (comboBox == null || comboBox.IsDisposed)
            {
                return;
            }

            string fieldName = fieldDefinition?.FieldName;
            if (string.IsNullOrWhiteSpace(fieldName))
            {
                fieldName = comboBox.DataSourceProperty;
            }

            if (string.IsNullOrWhiteSpace(fieldName) || !TryGetLovContext(fieldName, out var lov))
            {
                return;
            }

            if (!_activeLovPopupFields.Add(fieldName))
            {
                return;
            }

            CancellationTokenSource? searchReloadCts = null;
            EventHandler<string>? searchChanged = null;
            BeepLovPopup? popup = null;

            try
            {
                _formsHost?.TrySetActiveBlock(BlockName);
                ClearFieldError(fieldName);

                popup = CreateLovPopup(lov, fieldDefinition, fieldName);
                using (popup)
                {
                    if (_lovRecentSelections.TryGetValue(fieldName, out var recentItems))
                    {
                        popup.RecentItems = recentItems;
                    }

                    Form? owner = FindForm();
                    if (owner != null)
                    {
                        popup.Owner = owner;
                    }

                    Point origin = comboBox.PointToScreen(new Point(0, comboBox.Height));
                    List<SimpleItem> loadedItems = new();
                    string searchText = preloadSearch?.Trim() ?? string.Empty;

                    popup.ShowAt(new List<SimpleItem>(), origin, comboBox.Width, searchText);
                    Task<SimpleItem?> selectionTask = WaitForLovSelectionAsync(popup);

                    searchChanged = async (_, changedSearchText) =>
                    {
                        if (popup.IsDisposed || !popup.Visible)
                        {
                            return;
                        }

                        searchReloadCts?.Cancel();
                        searchReloadCts?.Dispose();
                        searchReloadCts = new CancellationTokenSource();

                        try
                        {
                            loadedItems = await QueueLovPopupSearchReloadAsync(
                                popup,
                                lov,
                                fieldName,
                                changedSearchText,
                                searchReloadCts.Token).ConfigureAwait(true);
                        }
                        catch (OperationCanceledException)
                        {
                        }
                    };

                    popup.SearchChanged += searchChanged;

                    await popup.LoadItemsAsync(async token =>
                    {
                        loadedItems = await LoadLovPopupItemsAsync(lov, fieldName, searchText, token, useShowLov: true).ConfigureAwait(true);
                        return loadedItems;
                    }, searchText).ConfigureAwait(true);

                    SimpleItem? selectedItem = await selectionTask.ConfigureAwait(true);
                    if (selectedItem == null || comboBox.IsDisposed)
                    {
                        return;
                    }

                    if (popup.RecentItems.Count > 0)
                    {
                        _lovRecentSelections[fieldName] = popup.RecentItems;
                    }

                    if (loadedItems.Count > 0)
                    {
                        comboBox.ListItems = new BindingList<SimpleItem>(loadedItems);
                    }

                    comboBox.SelectedItem = ResolveMatchingSimpleItem(comboBox.ListItems, selectedItem) ?? selectedItem;
                    ClearFieldError(fieldName);
                    comboBox.Focus();
                }
            }
            catch (Exception ex)
            {
                if (!comboBox.IsDisposed)
                {
                    SetFieldError(fieldName, ex.Message);
                }
            }
            finally
            {
                if (popup != null && searchChanged != null)
                {
                    popup.SearchChanged -= searchChanged;
                }

                searchReloadCts?.Cancel();
                searchReloadCts?.Dispose();
                _activeLovPopupFields.Remove(fieldName);
            }
        }

        private async Task<List<SimpleItem>> QueueLovPopupSearchReloadAsync(
            BeepLovPopup popup,
            LOVDefinition lov,
            string fieldName,
            string? searchText,
            CancellationToken cancellationToken)
        {
            await Task.Delay(LovPopupSearchDebounceMs, cancellationToken).ConfigureAwait(true);
            cancellationToken.ThrowIfCancellationRequested();

            List<SimpleItem> loadedItems = new();
            await popup.LoadItemsAsync(async popupToken =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                popupToken.ThrowIfCancellationRequested();

                loadedItems = await LoadLovPopupItemsAsync(lov, fieldName, searchText, popupToken, useShowLov: false).ConfigureAwait(true);
                return loadedItems;
            }, searchText ?? string.Empty).ConfigureAwait(true);

            cancellationToken.ThrowIfCancellationRequested();
            return loadedItems;
        }

        private static Task<SimpleItem?> WaitForLovSelectionAsync(BeepLovPopup popup)
        {
            var completion = new TaskCompletionSource<SimpleItem?>(TaskCreationOptions.RunContinuationsAsynchronously);

            EventHandler<SimpleItem>? itemAccepted = null;
            EventHandler? cancelled = null;
            EventHandler? visibilityChanged = null;
            EventHandler? disposed = null;

            void DetachHandlers()
            {
                popup.ItemAccepted -= itemAccepted;
                popup.Cancelled -= cancelled;
                popup.VisibleChanged -= visibilityChanged;
                popup.Disposed -= disposed;
            }

            itemAccepted = (_, item) =>
            {
                DetachHandlers();
                completion.TrySetResult(item);
            };

            cancelled = (_, _) =>
            {
                DetachHandlers();
                completion.TrySetResult(null);
            };

            visibilityChanged = (_, _) =>
            {
                if (popup.Visible || completion.Task.IsCompleted)
                {
                    return;
                }

                DetachHandlers();
                completion.TrySetResult(null);
            };

            disposed = (_, _) =>
            {
                if (completion.Task.IsCompleted)
                {
                    return;
                }

                DetachHandlers();
                completion.TrySetResult(null);
            };

            popup.ItemAccepted += itemAccepted;
            popup.Cancelled += cancelled;
            popup.VisibleChanged += visibilityChanged;
            popup.Disposed += disposed;

            return completion.Task;
        }

        private async Task<List<SimpleItem>> LoadLovPopupItemsAsync(
            LOVDefinition lov,
            string fieldName,
            string? searchText,
            CancellationToken cancellationToken,
            bool useShowLov)
        {
            if (string.IsNullOrWhiteSpace(ManagerBlockName) || _formsHost == null)
            {
                return new List<SimpleItem>();
            }

            string? effectiveSearchText = string.IsNullOrWhiteSpace(searchText) ? null : searchText.Trim();
            cancellationToken.ThrowIfCancellationRequested();

            LOVResult result = useShowLov
                ? await _formsHost.ShowLovAsync(ManagerBlockName, fieldName, searchText: effectiveSearchText, ct: cancellationToken).ConfigureAwait(true)
                : await _formsHost.LoadLovDataAsync(ManagerBlockName, fieldName, effectiveSearchText).ConfigureAwait(true);

            cancellationToken.ThrowIfCancellationRequested();
            if (!result.Success)
            {
                throw new InvalidOperationException(string.IsNullOrWhiteSpace(result.ErrorMessage)
                    ? $"Unable to load LOV data for {ManagerBlockName}.{fieldName}."
                    : result.ErrorMessage);
            }

            return BuildLovItems(lov, result.Records).ToList();
        }

        private BeepLovPopup CreateLovPopup(LOVDefinition lov, Models.BeepFieldDefinition? fieldDefinition, string fieldName)
        {
            return new BeepLovPopup
            {
                LovTitle = ResolveLovPopupTitle(lov, fieldDefinition, fieldName),
                LovTheme = Theme,
                UseThemeColors = UseThemeColors,
                CurrentTheme = BeepThemesManager.CurrentTheme,
                MaxPopupHeight = lov?.Height > 0 ? lov.Height : 360,
                LovColumns = CreateLovPopupColumns(lov)
            };
        }

        private static List<BeepColumnConfig> CreateLovPopupColumns(LOVDefinition? lov)
        {
            var columns = new List<BeepColumnConfig>();
            if (lov?.Columns == null || lov.Columns.Count == 0)
            {
                return columns;
            }

            string displayField = string.IsNullOrWhiteSpace(lov.DisplayField)
                ? lov.ReturnField ?? string.Empty
                : lov.DisplayField;
            string returnField = string.IsNullOrWhiteSpace(lov.ReturnField)
                ? displayField
                : lov.ReturnField;
            var supplementalFields = ResolveLovSupplementalFields(lov, displayField, returnField);

            foreach (var column in lov.Columns)
            {
                if (column == null || !column.Visible || string.IsNullOrWhiteSpace(column.FieldName))
                {
                    continue;
                }

                string? propertyName = MapLovFieldToSimpleItemProperty(column.FieldName, displayField, returnField, supplementalFields);
                if (string.IsNullOrWhiteSpace(propertyName))
                {
                    continue;
                }

                columns.Add(new BeepColumnConfig
                {
                    ColumnName = propertyName,
                    ColumnCaption = string.IsNullOrWhiteSpace(column.DisplayName) ? column.FieldName : column.DisplayName,
                    Width = column.Width > 0 ? column.Width : 100,
                    MinWidth = 40,
                    ReadOnly = true
                });
            }

            return columns;
        }

        private static IReadOnlyList<string> ResolveLovSupplementalFields(LOVDefinition? lov, string displayField, string returnField)
        {
            if (lov?.Columns == null || lov.Columns.Count == 0)
            {
                return Array.Empty<string>();
            }

            return lov.Columns
                .Where(column => column != null && column.Visible && !string.IsNullOrWhiteSpace(column.FieldName))
                .Select(column => column.FieldName)
                .Where(fieldName =>
                    !string.Equals(fieldName, displayField, StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(fieldName, returnField, StringComparison.OrdinalIgnoreCase))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Take(3)
                .ToList();
        }

        private static string? MapLovFieldToSimpleItemProperty(string? fieldName, string displayField, string returnField, IReadOnlyList<string> supplementalFields)
        {
            if (string.IsNullOrWhiteSpace(fieldName))
            {
                return null;
            }

            if (string.Equals(fieldName, returnField, StringComparison.OrdinalIgnoreCase))
            {
                return nameof(SimpleItem.Value);
            }

            if (string.Equals(fieldName, displayField, StringComparison.OrdinalIgnoreCase))
            {
                return nameof(SimpleItem.Text);
            }

            for (int index = 0; index < supplementalFields.Count; index++)
            {
                if (!string.Equals(supplementalFields[index], fieldName, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                return index switch
                {
                    0 => nameof(SimpleItem.SubText),
                    1 => nameof(SimpleItem.SubText2),
                    2 => nameof(SimpleItem.SubText3),
                    _ => null
                };
            }

            return null;
        }

        private Models.BeepFieldDefinition? ResolveLovFieldDefinition(string? fieldName)
        {
            if (string.IsNullOrWhiteSpace(fieldName))
            {
                return null;
            }

            return EffectiveDefinition?.Fields?
                .FirstOrDefault(field => string.Equals(field.FieldName, fieldName, StringComparison.OrdinalIgnoreCase));
        }

        private string ResolveLovPopupTitle(LOVDefinition? lov, Models.BeepFieldDefinition? fieldDefinition, string fieldName)
        {
            if (!string.IsNullOrWhiteSpace(lov?.Title))
            {
                return lov.Title;
            }

            string fieldLabel = ResolveLovFieldLabel(fieldDefinition);
            if (!string.IsNullOrWhiteSpace(fieldLabel))
            {
                return $"Select {fieldLabel}";
            }

            return $"Select {fieldName}";
        }

        private static string ResolveLovFieldLabel(Models.BeepFieldDefinition? fieldDefinition)
        {
            if (!string.IsNullOrWhiteSpace(fieldDefinition?.Label))
            {
                return fieldDefinition.Label;
            }

            return fieldDefinition?.FieldName ?? string.Empty;
        }

        private bool TryGetLovContext(string fieldName, out LOVDefinition lov)
        {
            lov = null!;

            if (_formsHost == null || string.IsNullOrWhiteSpace(ManagerBlockName) || string.IsNullOrWhiteSpace(fieldName))
            {
                return false;
            }

            if (!_formsHost.IsBlockRegistered(ManagerBlockName) || !_formsHost.HasLov(ManagerBlockName, fieldName))
            {
                return false;
            }

            lov = _formsHost.GetLov(ManagerBlockName, fieldName)!;
            return lov != null;
        }

        private static SimpleItem? ResolveMatchingSimpleItem(BindingList<SimpleItem>? items, SimpleItem? selectedItem)
        {
            if (items == null || selectedItem == null)
            {
                return null;
            }

            string? selectedValue = Convert.ToString(selectedItem.Value);
            return items.FirstOrDefault(item =>
                ReferenceEquals(item.Item, selectedItem.Item) ||
                Equals(item.Value, selectedItem.Value) ||
                (!string.IsNullOrWhiteSpace(selectedValue) && string.Equals(Convert.ToString(item.Value), selectedValue, StringComparison.OrdinalIgnoreCase)) ||
                string.Equals(item.Text, selectedItem.Text, StringComparison.OrdinalIgnoreCase));
        }
    }
}