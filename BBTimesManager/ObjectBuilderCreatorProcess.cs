using BBTimes.CustomComponents.CustomDatas;
using BBTimes.CustomContent.Builders;
using BBTimes.Extensions;
using BBTimes.Helpers;
using BepInEx;
using MTM101BaldAPI.Registers;

namespace BBTimes.Manager
{
    internal static partial class BBTimesManager
	{
		static void CreateObjBuilders(BaseUnityPlugin plug)
		{
			// 0 - F1
			// 1 - F2
			// 2 - F3
			// 3 - END
			ObjectBuilder e;

			e = CreatorExtensions.CreateObjectBuilder<VentBuilder, VentBuilderCustomData>("Vent").AddMeta(plug).value;
			floorDatas[1].WeightedObjectBuilders.Add(new() { selection = e, weight = 65 });
			floorDatas[2].WeightedObjectBuilders.Add(new() { selection = e, weight = 105 });
			floorDatas[3].WeightedObjectBuilders.Add(new() { selection = e, weight = 45 });

		}

		

		
	}
}
