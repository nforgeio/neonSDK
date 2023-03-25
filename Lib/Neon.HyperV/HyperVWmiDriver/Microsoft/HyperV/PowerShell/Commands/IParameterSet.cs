namespace Microsoft.HyperV.PowerShell.Commands;

internal interface IParameterSet
{
	bool CurrentParameterSetIs(string parameterSetName);
}
