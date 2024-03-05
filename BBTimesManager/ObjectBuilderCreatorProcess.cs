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
			floorDatas[0].ForcedObjectBuilders.Add(e);
			floorDatas[1].ForcedObjectBuilders.Add(e);
			floorDatas[2].ForcedObjectBuilders.Add(e);
			floorDatas[3].ForcedObjectBuilders.Add(e);

		}

		

		
	}
}
