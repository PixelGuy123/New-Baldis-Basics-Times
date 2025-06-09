using System.IO;
using BBTimes.CustomContent.Builders;
using BBTimes.Extensions;
using BBTimes.Helpers;
using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;

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
			StructureWithParameters vent = CreatorExtensions.CreateObjectBuilder<Structure_Duct>("DuctBuilder", out _, "Duct");
			floorDatas[F3].ForcedObjectBuilders.Add(new(vent));

			// Wall Bell Builder
			vent = CreatorExtensions.CreateObjectBuilder<RandomForcedPostersBuilder>("ForcedPosterBuilder", out var forcedPosterBuilder);
			forcedPosterBuilder.allowedShape = TileShapeMask.Single | TileShapeMask.Corner;
			vent.parameters.chance[0] = 0.35f;
			forcedPosterBuilder.posters = [
				new WeightedPosterObject() {selection = ObjectCreators.CreatePosterObject([AssetLoader.TextureFromFile(Path.Combine(MiscPath, TextureFolder, GetAssetName("wallbell.png")))]), weight = 100}
				];
			foreach (var fld in floorDatas)
				fld.Value.ForcedObjectBuilders.Add(new(vent));

			// Trapdoor Builder
			vent = CreatorExtensions.CreateObjectBuilder<Structure_Trapdoor>("Structure_Trapdoor", out _, "Trapdoor");
			floorDatas[F4].ForcedObjectBuilders.Add(new(vent, LevelType.Maintenance));

			vent = CloneParameter(vent);
			vent.parameters.minMax[0] = new(4, 5);
			vent.parameters.chance[0] = 0.35f;

			floorDatas[F5].ForcedObjectBuilders.Add(new(vent, LevelType.Maintenance));


			// Camera Builder
			vent = CreatorExtensions.CreateObjectBuilder<Structure_Camera>("Structure_Camera", out _, "SecurityCamera");
			vent.parameters.minMax[0] = new(3, 5);

			//floorDatas[F1].ForcedObjectBuilders.Add(vent);

			floorDatas[F2].ForcedObjectBuilders.Add(new(vent));
			floorDatas[END].ForcedObjectBuilders.Add(new(vent));
			vent = CloneParameter(vent);
			vent.parameters.minMax[0] = new(5, 7);
			vent.parameters.minMax[1] = new(12, 15);

			floorDatas[F3].ForcedObjectBuilders.Add(new(vent));
			vent = CloneParameter(vent);
			vent.parameters.minMax[0] = new(3, 4);
			vent.parameters.minMax[1] = new(9, 13);

			floorDatas[F4].ForcedObjectBuilders.Add(new(vent, LevelType.Laboratory));
			floorDatas[F5].ForcedObjectBuilders.Add(new(vent, LevelType.Laboratory));


			// Squisher builder
			vent = CreatorExtensions.CreateObjectBuilder<Structure_Squisher>("Structure_Squisher", out _, "Squisher");

			vent.parameters.minMax[0].z = 3;
			vent.parameters.chance[0] = 0.15f;
			floorDatas[F4].ForcedObjectBuilders.Add(new(vent, LevelType.Laboratory));

			vent = CloneParameter(vent);
			vent.parameters.minMax[0] = new(2, 5);
			vent.parameters.minMax[1] = new(12, 16);
			vent.parameters.minMax[2] = new(4, 6);
			vent.parameters.chance[0] = 0.6f;

			floorDatas[F5].ForcedObjectBuilders.Add(new(vent, LevelType.Laboratory));


			// Small Door builder
			vent = CreatorExtensions.CreateObjectBuilder<Structure_SmallDoor>("Structure_SmallDoor", out _, "SmallDoor");
			foreach (var fld in floorDatas)
				fld.Value.ForcedObjectBuilders.Add(new(vent, LevelType.Schoolhouse, LevelType.Maintenance));

			// ItemAlarm Builder
			vent = CreatorExtensions.CreateObjectBuilder<Structure_ItemAlarm>("Structure_ItemAlarm", out _, "ItemAlarm");

			floorDatas[F3].ForcedObjectBuilders.Add(new(vent, LevelType.Laboratory));
			floorDatas[F4].ForcedObjectBuilders.Add(new(vent, LevelType.Laboratory));

			vent = CloneParameter(vent);
			vent.parameters.minMax[0] = new(6, 9);
			floorDatas[F5].ForcedObjectBuilders.Add(new(vent, LevelType.Laboratory));


			// SecretButton Builder
			vent = CreatorExtensions.CreateObjectBuilder<Structure_SecretButton>("Structure_SecretTimesButton", out _, "SecretButton");
			floorDatas[F5].ForcedObjectBuilders.Add(new(vent));

			static StructureWithParameters CloneParameter(StructureWithParameters bld) =>
				new() { prefab = bld.prefab, parameters = new() { chance = bld.parameters.chance.CopyArray(), minMax = bld.parameters.minMax.CopyArray(), prefab = bld.parameters.prefab.CopyObjArray() } };

		}

		static T[] CopyArray<T>(this T[] ogAr)
		{
			if (ogAr == null)
				return null;

			T[] newAr = new T[ogAr.Length];
			for (int i = 0; i < ogAr.Length; i++) // Useful for arrays that contains structs
				newAr[i] = ogAr[i];
			return newAr;
		}

		static WeightedGameObject[] CopyObjArray(this WeightedGameObject[] ogAr)
		{
			if (ogAr == null)
				return null;

			WeightedGameObject[] newAr = new WeightedGameObject[ogAr.Length];
			for (int i = 0; i < ogAr.Length; i++) // Useful for arrays that contains structs
				newAr[i] = new() { selection = ogAr[i].selection, weight = ogAr[i].weight };
			return newAr;
		}




	}
}
