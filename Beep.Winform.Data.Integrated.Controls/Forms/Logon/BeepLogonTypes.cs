using System;

namespace TheTechIdea.Beep.Winform.Controls.Integrated.Forms.Logon
{
    public enum BeepLogonResult
    {
        Unknown = 0,
        Success = 1,
        Cancelled = 2,
        Failed = 3
    }

    public class BeepLogonRequest
    {
        public string ConnectionName { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool RememberPassword { get; set; }
        public bool AllowConnectionSwitch { get; set; } = true;
        public string Title { get; set; } = "Connect";
        public string Prompt { get; set; } = "Sign in to continue.";
    }

    public class BeepLogonContext
    {
        public BeepLogonRequest Request { get; set; } = new BeepLogonRequest();
        public BeepLogonResult Result { get; set; } = BeepLogonResult.Unknown;
        public string? FailureReason { get; set; }
        public DateTime CompletedAtUtc { get; set; }

        public bool IsSuccess => Result == BeepLogonResult.Success;
    }
}
