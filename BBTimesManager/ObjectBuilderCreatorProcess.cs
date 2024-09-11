using BBTimes.CustomContent.Builders;
using BBTimes.Extensions;
using BBTimes.Helpers;
using BepInEx;
using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI;
using MTM101BaldAPI.Registers;
using System.IO;
using PixelInternalAPI.Extensions;

namespace BBTimes.Manager
{
    internal static partial class BBTimesManager
	{
		static void CreateObjBuilders()
		{
			// 0 - F1
			// 1 - F2
			// 2 - F3
			// 3 - END

			// Vent Builder
			VentBuilder vent = CreatorExtensions.CreateObjectBuilder<VentBuilder>("VentBuilder", "Vent");
			vent.AddMeta(plug);
			vent.minAmount = 3;
			vent.maxAmount = 5;
			floorDatas[1].WeightedObjectBuilders.Add(new() { selection = vent, weight = 65 });
			vent = vent.DuplicatePrefab();
			vent.minAmount = 4;
			vent.maxAmount = 6;
			floorDatas[2].WeightedObjectBuilders.Add(new() { selection = vent, weight = 105 });
			vent = vent.DuplicatePrefab();
			vent.minAmount = 4;
			vent.maxAmount = 5;
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

			// Trapdoor Builder
			TrapDoorBuilder trapdoor = CreatorExtensions.CreateObjectBuilder<TrapDoorBuilder>("TrapdoorBuilder", "Trapdoor");
			trapdoor.minAmount = 3;
			trapdoor.maxAmount = 5;
			trapdoor.AddMeta(plug);
			floorDatas[1].WeightedObjectBuilders.Add(new() { selection = trapdoor, weight = 35 });

			trapdoor = trapdoor.DuplicatePrefab();
			trapdoor.minAmount = 3;
			trapdoor.maxAmount = 5;

			floorDatas[2].WeightedObjectBuilders.Add(new() { selection = trapdoor, weight = 55 });

			trapdoor = trapdoor.DuplicatePrefab();
			trapdoor.minAmount = 2;
			trapdoor.maxAmount = 5;

			floorDatas[3].WeightedObjectBuilders.Add(new() { selection = trapdoor, weight = 75 });

			// Camera Builder
			CameraBuilder cams = CreatorExtensions.CreateObjectBuilder<CameraBuilder>("CameraBuilder", "SecurityCamera");
			cams.AddMeta(plug);
			cams.minAmount = 1;
			cams.maxAmount = 3;

			floorDatas[1].WeightedObjectBuilders.Add(new() { selection = cams, weight = 35 });

			cams = cams.DuplicatePrefab();
			cams.minAmount = 3;
			cams.maxAmount = 5;

			floorDatas[2].WeightedObjectBuilders.Add(new() { selection = cams, weight = 55 });

			cams = cams.DuplicatePrefab();
			cams.minAmount = 1;
			cams.maxAmount = 4;

			floorDatas[3].WeightedObjectBuilders.Add(new() { selection = cams, weight = 25 });

			// Squisher builder
			SquisherBuilder squish = CreatorExtensions.CreateObjectBuilder<SquisherBuilder>("SquisherBuilder", "Squisher");
			squish.AddMeta(plug);

			squish.minAmount = 1;
			squish.maxAmount = 2;

			floorDatas[1].WeightedObjectBuilders.Add(new() { selection = squish, weight = 25 });

			squish = squish.DuplicatePrefab();
			squish.minAmount = 2;
			squish.maxAmount = 6;

			floorDatas[2].WeightedObjectBuilders.Add(new() { selection = squish, weight = 45 });

			squish = squish.DuplicatePrefab();
			squish.minAmount = 1;
			squish.maxAmount = 3;

			floorDatas[3].WeightedObjectBuilders.Add(new() { selection = squish, weight = 35 });

		}

		

		
	}
}
