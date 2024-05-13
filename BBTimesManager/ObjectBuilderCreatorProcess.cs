﻿using BBTimes.CustomComponents.CustomDatas;
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
		static void CreateObjBuilders(BaseUnityPlugin plug)
		{
			// 0 - F1
			// 1 - F2
			// 2 - F3
			// 3 - END

			// Vent Builder
			VentBuilder vent = CreatorExtensions.CreateObjectBuilder<VentBuilder, VentBuilderCustomData>("VentBuilder", "Vent");
			vent.AddMeta(plug);
			vent.minAmount = 3;
			vent.maxAmount = 5;
			floorDatas[1].WeightedObjectBuilders.Add(new() { selection = vent, weight = 65 });
			vent = vent.DuplicatePrefab();
			vent.minAmount = 5;
			vent.maxAmount = 7;
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
			TrapDoorBuilder trapdoor = CreatorExtensions.CreateObjectBuilder<TrapDoorBuilder, TrapdoorBuilderCustomData>("TrapdoorBuilder", "Trapdoor");
			trapdoor.minAmount = 3;
			trapdoor.maxAmount = 5;
			trapdoor.AddMeta(plug);
			floorDatas[1].WeightedObjectBuilders.Add(new() { selection = trapdoor, weight = 35 });

			trapdoor = trapdoor.DuplicatePrefab();
			trapdoor.minAmount = 4;
			trapdoor.maxAmount = 7;

			floorDatas[2].WeightedObjectBuilders.Add(new() { selection = trapdoor, weight = 55 });

			trapdoor = trapdoor.DuplicatePrefab();
			trapdoor.minAmount = 2;
			trapdoor.maxAmount = 5;

			floorDatas[3].WeightedObjectBuilders.Add(new() { selection = trapdoor, weight = 75 });

		}

		

		
	}
}
