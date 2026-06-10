using System;
using System.Threading.Tasks;
using TheTechIdea.Beep.SetUp;

namespace TheTechIdea.Beep.Winform.Default.Views.Setup
{
    /// <summary>
    /// WinForms-side wrapper around BeepDM's <see cref="IFirstRunDetector"/>.
    /// Resolves the detector from an <see cref="IServiceProvider"/> if available,
    /// otherwise falls back to <see cref="FileBasedFirstRunDetector"/> bound to
    /// the supplied <c>IDMEEditor</c>.
    /// </summary>
    public class BootstrapState : IDisposable
    {
        private readonly IFirstRunDetector _detector;
        private bool _disposed;

        public BootstrapState(IFirstRunDetector detector)
        {
            _detector = detector ?? throw new ArgumentNullException(nameof(detector));
        }

        public static BootstrapState Resolve(IServiceProvider? services, TheTechIdea.Beep.Editor.IDMEEditor editor)
        {
            if (services != null)
            {
                var resolved = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions
                    .GetService<IFirstRunDetector>(services);
                if (resolved != null) return new BootstrapState(resolved);
            }
            return new BootstrapState(new FileBasedFirstRunDetector(editor));
        }

        public bool IsFirstRun => !_detector.WasSetupCompleted;
        public bool CheckComplete { get; private set; }

        public event Func<Task>? OnStateChanged;

        public async Task InitializeAsync()
        {
            CheckComplete = false;
            await _detector.IsFirstRunAsync();
            CheckComplete = true;
            if (OnStateChanged != null) await OnStateChanged.Invoke();
        }

        public async Task MarkSetupCompleteAsync()
        {
            await _detector.MarkSetupCompleteAsync();
            if (OnStateChanged != null) await OnStateChanged.Invoke();
        }

        public async Task ResetAsync()
        {
            await _detector.ClearSetupFlagAsync();
            if (OnStateChanged != null) await OnStateChanged.Invoke();
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            OnStateChanged = null;
        }
    }
}
