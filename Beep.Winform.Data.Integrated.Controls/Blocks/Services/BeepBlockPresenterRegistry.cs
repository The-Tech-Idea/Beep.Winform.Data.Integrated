using System;
using System.Collections.Generic;
using System.Linq;
using TheTechIdea.Beep.Winform.Controls.Integrated.Blocks.Contracts;
using TheTechIdea.Beep.Winform.Controls.Integrated.Blocks.Models;
using TheTechIdea.Beep.Winform.Controls.Integrated.Blocks.Services.Presenters;

namespace TheTechIdea.Beep.Winform.Controls.Integrated.Blocks.Services
{
    public sealed class BeepBlockPresenterRegistry
    {
        private readonly Dictionary<string, IBeepFieldPresenter> _presenters = new(StringComparer.OrdinalIgnoreCase);
        private readonly ReflectiveControlBeepFieldPresenter _reflectivePresenter = new();

        public IReadOnlyCollection<IBeepFieldPresenter> Presenters => _presenters.Values.ToList().AsReadOnly();

        public void Register(IBeepFieldPresenter presenter)
        {
            if (presenter == null || string.IsNullOrWhiteSpace(presenter.Key))
            {
                return;
            }

            _presenters[presenter.Key] = presenter;
        }

        public void RegisterAlias(string key, IBeepFieldPresenter presenter)
        {
            if (presenter == null || string.IsNullOrWhiteSpace(key))
            {
                return;
            }

            _presenters[key] = presenter;
        }

        public void RegisterDefaults()
        {
            var comboPresenter = new ComboBeepFieldPresenter();

            Register(new TextBeepFieldPresenter());
            Register(new NumericBeepFieldPresenter());
            Register(new DateBeepFieldPresenter());
            Register(new CheckboxBeepFieldPresenter());
            Register(comboPresenter);
            RegisterAlias("lov", comboPresenter);
            RegisterAlias("option", comboPresenter);
        }

        public IBeepFieldPresenter? Resolve(BeepFieldDefinition fieldDefinition)
        {
            if (fieldDefinition == null)
            {
                return null;
            }

            if (_reflectivePresenter.CanPresent(fieldDefinition))
            {
                return _reflectivePresenter;
            }

            if (!string.IsNullOrWhiteSpace(fieldDefinition.EditorKey) &&
                _presenters.TryGetValue(fieldDefinition.EditorKey, out var directMatch))
            {
                return directMatch;
            }

            return _presenters.Values.FirstOrDefault(x => x.CanPresent(fieldDefinition));
        }
    }
}