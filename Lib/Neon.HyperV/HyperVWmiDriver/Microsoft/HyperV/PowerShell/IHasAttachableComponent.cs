namespace Microsoft.HyperV.PowerShell;

internal interface IHasAttachableComponent<TComponentSetting>
{
	string DescriptionForAttach { get; }

	bool HasComponent();

	TComponentSetting GetComponentSetting(UpdatePolicy policy);

	void FinishAttachingComponentSetting(TComponentSetting componentSetting);
}
