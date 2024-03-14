using BBTimes.CustomComponents.CustomDatas;
using BBTimes.CustomContent.Builders;
using BBTimes.Extensions;
using BBTimes.Helpers;
using BepInEx;
using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI;
using MTM101BaldAPI.Registers;
using System.IO;

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

			// Vent Builder
			VentBuilder vent = CreatorExtensions.CreateObjectBuilder<VentBuilder, VentBuilderCustomData>("Vent");
			vent.AddMeta(plug);
			floorDatas[1].WeightedObjectBuilders.Add(new() { selection = vent, weight = 65 });
			floorDatas[2].WeightedObjectBuilders.Add(new() { selection = vent, weight = 105 });
			floorDatas[3].WeightedObjectBuilders.Add(new() { selection = vent, weight = 45 });

			// Wall Bell Builder
			RandomForcedPostersBuilder forcedPosterBuilder = CreatorExtensions.CreateObjectBuilder<RandomForcedPostersBuilder>();
			forcedPosterBuilder.AddMeta(plug);
			forcedPosterBuilder.allowedShapes = [TileShape.Single, TileShape.Corner];
			forcedPosterBuilder.chance = 0.6f;
			forcedPosterBuilder.posters = [
				new WeightedPosterObject() {selection = ObjectCreators.CreatePosterObject([AssetLoader.TextureFromFile(Path.Combine(MiscPath, TextureFolder, "wallbell.png"))]), weight = 100}
				];
			floorDatas.ForEach(x => x.ForcedObjectBuilders.Add(forcedPosterBuilder));

		}

		

		
	}
}
