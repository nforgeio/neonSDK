using System.ComponentModel;

namespace Microsoft.Virtualization.Client.Management;

[TypeConverter(typeof(EnumResourceConverter<BandwidthReservationMode>))]
internal enum BandwidthReservationMode : uint
{
	Default,
	Weight,
	Absolute,
	None
}
