using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace AreYouSleeping.Automation;

public class ShutdownAutomation
{
    private ILogger<ShutdownAutomation> _logger;

    public ShutdownAutomation(ILogger<ShutdownAutomation> logger)
    {
        _logger = logger;
    }

    public bool Sleep()
    {
        _logger.LogInformation("Entering sleep mode...");
        return System.Windows.Forms.Application.SetSuspendState(System.Windows.Forms.PowerState.Suspend, true, true);
    }

    public void Shutdown()
    {
        _logger.LogInformation("Shutting down...");
        var psi = new ProcessStartInfo("shutdown", "/s /t 0");
        psi.CreateNoWindow = true;
        psi.UseShellExecute = false;
        Process.Start(psi);
    }
}
