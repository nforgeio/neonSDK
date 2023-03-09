namespace Microsoft.HyperV.PowerShell;

internal static class CriticalStateConverter
{
	public static VMState GetCriticalState(VMState state)
	{
		VMState result = state;
		switch (state)
		{
		case VMState.Running:
			result = VMState.RunningCritical;
			break;
		case VMState.Off:
			result = VMState.OffCritical;
			break;
		case VMState.Stopping:
			result = VMState.StoppingCritical;
			break;
		case VMState.Saved:
			result = VMState.SavedCritical;
			break;
		case VMState.Paused:
			result = VMState.PausedCritical;
			break;
		case VMState.Starting:
			result = VMState.StartingCritical;
			break;
		case VMState.Reset:
			result = VMState.ResetCritical;
			break;
		case VMState.Saving:
			result = VMState.SavingCritical;
			break;
		case VMState.Pausing:
			result = VMState.PausingCritical;
			break;
		case VMState.Resuming:
			result = VMState.ResumingCritical;
			break;
		case VMState.FastSaved:
			result = VMState.FastSavedCritical;
			break;
		case VMState.FastSaving:
			result = VMState.FastSavingCritical;
			break;
		}
		return result;
	}
}
