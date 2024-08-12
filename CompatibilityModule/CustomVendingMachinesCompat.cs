using BBTimes.Plugin;

namespace BBTimes.CompatibilityModule
{
	internal class CustomVendingMachinesCompat
	{
		internal static void Loadup() =>
			CustomVendingMachines.CustomVendingMachinesPlugin.AddDataFromDirectory(System.IO.Path.Combine(BasePlugin.ModPath, "objects", "VendingMachines"));
	}
}
