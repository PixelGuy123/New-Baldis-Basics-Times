using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI;
using System.IO;
using BBTimes.ModPatches.GeneratorPatches;
using BBTimes.Manager.SelectionHolders;
using BBTimes.CreatorHelpers;

namespace BBTimes.Manager
{
	internal static partial class BBTimesManager
	{
		static void CreateWindows()
		{
			var brokenTex = AssetLoader.TextureFromFile(Path.Combine(MiscPath, TextureFolder, "MetalWindow.png"));
			var tex = brokenTex;
			var window = CreatorExtensions.CreateWindow("MetalWindow", tex, brokenTex, unbreakable:true);
			var windowSel = new WindowObjectHolder(window, 75, [RoomCategory.Office]);

			PostRoomCreation.window = window; // Set the metal window, as it is unbreakable

			floorDatas[1].WindowObjects.Add(windowSel);
			floorDatas[2].WindowObjects.Add(windowSel);
			floorDatas[3].WindowObjects.Add(windowSel);
		}
	}
}
