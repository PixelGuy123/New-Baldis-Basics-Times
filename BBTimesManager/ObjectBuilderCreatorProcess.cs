using BBTimes.CustomContent.Builders;
using BBTimes.Extensions;
using BBTimes.Helpers;
using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI;
using MTM101BaldAPI.Registers;
using System.IO;

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
			var meta = vent.AddMeta(plug);
			floorDatas[1].WeightedObjectBuilders.Add(new() { selection = vent, weight = 65 });
			floorDatas[2].WeightedObjectBuilders.Add(new() { selection = vent, weight = 105 });
			floorDatas[3].WeightedObjectBuilders.Add(new() { selection = vent, weight = 45 });

			// Wall Bell Builder
			RandomForcedPostersBuilder forcedPosterBuilder = CreatorExtensions.CreateObjectBuilder<RandomForcedPostersBuilder>();
			forcedPosterBuilder.AddMeta(plug);
			forcedPosterBuilder.allowedShapes = [TileShape.Single, TileShape.Corner];
			forcedPosterBuilder.chance = 0.6f;
			forcedPosterBuilder.posters = [
				new WeightedPosterObject() {selection = ObjectCreators.CreatePosterObject([AssetLoader.TextureFromFile(Path.Combine(MiscPath, TextureFolder, GetAssetName("wallbell.png")))]), weight = 100}
				];
			floorDatas.ForEach(x => x.ForcedObjectBuilders.Add(forcedPosterBuilder));

			// Trapdoor Builder
			TrapDoorBuilder trapdoor = CreatorExtensions.CreateObjectBuilder<TrapDoorBuilder>("TrapdoorBuilder", "Trapdoor");
			trapdoor.AddMeta(plug);
			floorDatas[1].WeightedObjectBuilders.Add(new() { selection = trapdoor, weight = 35 });
			floorDatas[2].WeightedObjectBuilders.Add(new() { selection = trapdoor, weight = 55 });
			floorDatas[3].WeightedObjectBuilders.Add(new() { selection = trapdoor, weight = 75 });

			// Camera Builder
			CameraBuilder cams = CreatorExtensions.CreateObjectBuilder<CameraBuilder>("CameraBuilder", "SecurityCamera");
			cams.AddMeta(plug);
			AddBuilderAtIdx(cams, 35, 1, 3);
			AddBuilderAtIdx(cams, 55, 2, 5);
			AddBuilderAtIdx(cams, 25, 3, 4);

			// Squisher builder
			SquisherBuilder squish = CreatorExtensions.CreateObjectBuilder<SquisherBuilder>("SquisherBuilder", "Squisher");
			squish.AddMeta(plug);
			AddBuilderAtIdx(squish, 25, 1, 2);
			AddBuilderAtIdx(squish, 45, 2, 6);
			AddBuilderAtIdx(squish, 35, 3, 3);

			// Small Door builder
			SmallDoorBuilder smalDor = CreatorExtensions.CreateObjectBuilder<SmallDoorBuilder>("SmallDoor", "SmallDoor");
			smalDor.AddMeta(plug);
			floorDatas.ForEach(x => x.ForcedObjectBuilders.Add(smalDor));

			// ItemAlarm Builder
			ItemAlarmBuilder alarm = CreatorExtensions.CreateObjectBuilder<ItemAlarmBuilder>("ItemAlarmBuilder", "ItemAlarm");
			alarm.AddMeta(plug);
			AddBuilderAtIdx(alarm, 35, 1, 4);
			AddBuilderAtIdx(alarm, 60, 2, 5);
			AddBuilderAtIdx(alarm, 15, 3, 2);

			// SecretButton Builder
			SecretButtonBuilder secBut = CreatorExtensions.CreateObjectBuilder<SecretButtonBuilder>("SecretButtonBuilder", "SecretButton");
			secBut.AddMeta(plug);
			floorDatas[2].ForcedObjectBuilders.Add(secBut); // debugging purposes

			static void AddBuilderAtIdx(ObjectBuilder bld, int weight, int index, int n)
			{
				for (int i = 0; i < n; i++)
					floorDatas[index].WeightedObjectBuilders.Add(new() { selection = bld, weight = weight });
			}

		}

		

		
	}
}
