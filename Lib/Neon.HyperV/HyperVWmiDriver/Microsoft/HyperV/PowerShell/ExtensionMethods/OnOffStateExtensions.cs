namespace Microsoft.HyperV.PowerShell.ExtensionMethods;

internal static class OnOffStateExtensions
{
    internal static bool ToBool(this OnOffState onOffState)
    {
        return onOffState == OnOffState.On;
    }

    internal static OnOffState ToOnOffState(this bool value)
    {
        if (!value)
        {
            return OnOffState.Off;
        }
        return OnOffState.On;
    }
}
