using System.Windows.Forms;
using TheTechIdea.Beep.Editor.Forms.Hosts;
using TheTechIdea.Beep.Winform.Controls;
using TheTechIdea.Beep.Winform.Controls.TextFields;

namespace TheTechIdea.Beep.Winform.Data.Integrated.Forms.BlockHost;

public sealed class WinFormBlockNavigationBar : UserControl, IBlockNavigationBar
{
    private readonly BeepButton _first = new() { Text = "|<" };
    private readonly BeepButton _previous = new() { Text = "<" };
    private readonly BeepTextBox _position = new() { Text = "0" };
    private readonly BeepButton _next = new() { Text = ">" };
    private readonly BeepButton _last = new() { Text = ">|" };
    private int _currentRecordIndex = -1;
    private int _recordCount;
    private bool _refreshing;

    public WinFormBlockNavigationBar()
    {
        var layout = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true };
        foreach (Control control in new Control[] { _first, _previous, _position, _next, _last })
            layout.Controls.Add(control);
        Controls.Add(layout);
        _first.Click += (_, _) => FirstClicked?.Invoke(this, EventArgs.Empty);
        _previous.Click += (_, _) => PreviousClicked?.Invoke(this, EventArgs.Empty);
        _next.Click += (_, _) => NextClicked?.Invoke(this, EventArgs.Empty);
        _last.Click += (_, _) => LastClicked?.Invoke(this, EventArgs.Empty);
        _position.TextChanged += (_, _) =>
        {
            if (!_refreshing && int.TryParse(_position.Text, out var oneBased))
                RecordIndexChanged?.Invoke(this, Math.Max(0, oneBased - 1));
        };
    }

    public int CurrentRecordIndex { get => _currentRecordIndex; set => _currentRecordIndex = value; }
    public int RecordCount { get => _recordCount; set => _recordCount = Math.Max(0, value); }
    public bool IsQueryMode { get; set; }
    public object View => this;

    public event EventHandler? FirstClicked;
    public event EventHandler? PreviousClicked;
    public event EventHandler? NextClicked;
    public event EventHandler? LastClicked;
    public event EventHandler<int>? RecordIndexChanged;

    public new void Refresh()
    {
        _refreshing = true;
        try
        {
            _position.Text = _recordCount == 0 ? "0" : (_currentRecordIndex + 1).ToString();
            var canMove = !IsQueryMode && _recordCount > 0;
            _first.Enabled = _previous.Enabled = canMove && _currentRecordIndex > 0;
            _next.Enabled = _last.Enabled = canMove && _currentRecordIndex < _recordCount - 1;
        }
        finally { _refreshing = false; }
        base.Refresh();
    }
}
