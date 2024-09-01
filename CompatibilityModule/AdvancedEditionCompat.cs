using BBTimes.Manager;
using BaldisBasicsPlusAdvanced.API;
using System.Collections.Generic;

namespace BBTimes.CompatibilityModule
{
	internal class AdvancedEditionCompat
	{
		internal static void Loadup()
		{
			List<string> strs = [];
			for (int i = 1; i <= elvTips; i++)
				strs.Add($"times_elv_tip{i}");
			ApiManager.AddNewTips(BBTimesManager.plug.Info, [.. strs]);
		}
		const int elvTips = 9;
	}
}
