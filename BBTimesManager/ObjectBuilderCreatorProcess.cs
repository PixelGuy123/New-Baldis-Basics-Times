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
			StructureWithParameters vent = CreatorExtensions.CreateObjectBuilder<Structure_Duct>("DuctBuilder", out _, "Duct");
			floorDatas[1].WeightedObjectBuilders.Add(new() { selection = vent, weight = 65 });
			floorDatas[2].WeightedObjectBuilders.Add(new() { selection = vent, weight = 105 });
			floorDatas[3].WeightedObjectBuilders.Add(new() { selection = vent, weight = 45 });

			// Wall Bell Builder
			vent = CreatorExtensions.CreateObjectBuilder<RandomForcedPostersBuilder>("ForcedPosterBuilder", out var forcedPosterBuilder);
			forcedPosterBuilder.allowedShape = TileShapeMask.Single | TileShapeMask.Corner;
			vent.parameters.chance[0] = 0.35f;
			forcedPosterBuilder.posters = [
				new WeightedPosterObject() {selection = ObjectCreators.CreatePosterObject([AssetLoader.TextureFromFile(Path.Combine(MiscPath, TextureFolder, GetAssetName("wallbell.png")))]), weight = 100}
				];
			floorDatas.ForEach(x => x.ForcedObjectBuilders.Add(vent));

			// Trapdoor Builder
			vent = CreatorExtensions.CreateObjectBuilder<Structure_Trapdoor>("Structure_Trapdoor", out _, "Trapdoor");
			floorDatas[1].WeightedObjectBuilders.Add(new() { selection = vent, weight = 35 });
			floorDatas[3].WeightedObjectBuilders.Add(new() { selection = vent, weight = 75 });

			vent = CloneParameter(vent);
			vent.parameters.minMax[0] = new(6, 9);
			vent.parameters.chance[0] = 0.35f;

			floorDatas[2].WeightedObjectBuilders.Add(new() { selection = vent, weight = 55 });
			

			// Camera Builder
			vent = CreatorExtensions.CreateObjectBuilder<Structure_Camera>("Structure_Camera", out _, "SecurityCamera");
			vent.parameters.minMax[0] = new(3, 5);

			//floorDatas[0].ForcedObjectBuilders.Add(vent);

			floorDatas[1].WeightedObjectBuilders.Add(new() { selection = vent, weight = 35 });
			vent = CloneParameter(vent);
			vent.parameters.minMax[0] = new(5, 7);
			vent.parameters.minMax[1] = new(12, 15);

			floorDatas[2].WeightedObjectBuilders.Add(new() { selection = vent, weight = 45 });
			vent = CloneParameter(vent);
			vent.parameters.minMax[0] = new(3, 4);
			vent.parameters.minMax[1] = new(9, 13);

			floorDatas[3].WeightedObjectBuilders.Add(new() { selection = vent, weight = 25 });


			// Squisher builder
			vent = CreatorExtensions.CreateObjectBuilder<Structure_Squisher>("Structure_Squisher", out _, "Squisher");

			vent.parameters.minMax[0].z = 3;
			vent.parameters.chance[0] = 0.15f;
			floorDatas[1].WeightedObjectBuilders.Add(new() { selection = vent, weight = 25 });
			floorDatas[3].WeightedObjectBuilders.Add(new() { selection = vent, weight = 35 });

			vent = CloneParameter(vent);
			vent.parameters.minMax[0] = new(2, 5);
			vent.parameters.minMax[1] = new(12, 16);
			vent.parameters.minMax[2] = new(4, 6);
			vent.parameters.chance[0] = 0.6f;

			floorDatas[2].WeightedObjectBuilders.Add(new() { selection = vent, weight = 45 });
			

			// Small Door builder
			vent = CreatorExtensions.CreateObjectBuilder<Structure_SmallDoor>("Structure_SmallDoor", out _, "SmallDoor");
			floorDatas.ForEach(x => x.ForcedObjectBuilders.Add(vent));

			// ItemAlarm Builder
			vent = CreatorExtensions.CreateObjectBuilder<Structure_ItemAlarm>("Structure_ItemAlarm", out _, "ItemAlarm");

			floorDatas[1].WeightedObjectBuilders.Add(new() { selection = vent, weight = 35 });
			floorDatas[3].WeightedObjectBuilders.Add(new() { selection = vent, weight = 25 });

			vent = CloneParameter(vent);
			vent.parameters.minMax[0] = new(6, 9);
			floorDatas[2].WeightedObjectBuilders.Add(new() { selection = vent, weight = 55 });
			

			// SecretButton Builder
			vent = CreatorExtensions.CreateObjectBuilder<Structure_SecretButton>("Structure_SecretTimesButton", out _, "SecretButton");
			floorDatas[2].ForcedObjectBuilders.Add(vent); // debugging purposes

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
