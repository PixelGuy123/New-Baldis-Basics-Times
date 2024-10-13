using MTM101BaldAPI.AssetTools;
using System.IO;
using BBTimes.ModPatches.GeneratorPatches;
using BBTimes.Misc.SelectionHolders;
using BBTimes.Helpers;

namespace BBTimes.Manager
{
	internal static partial class BBTimesManager
	{
		static void CreateWindows()
		{
			// Metal Window
			var brokenTex = AssetLoader.TextureFromFile(Path.Combine(MiscPath, TextureFolder, GetAssetName("MetalWindow.png")));
			var tex = brokenTex;
			var window = CreatorExtensions.CreateWindow("MetalWindow", tex, brokenTex, unbreakable:true);
			var windowSel = new WindowObjectHolder(window, 75, [RoomCategory.Office]);
			floorDatas[1].WindowObjects.Add(windowSel);
			floorDatas[2].WindowObjects.Add(windowSel);
			floorDatas[3].WindowObjects.Add(windowSel);

			// Round Window
			brokenTex = AssetLoader.TextureFromFile(Path.Combine(MiscPath, TextureFolder, GetAssetName("roundWindow.png")));
			tex = brokenTex;
			window = CreatorExtensions.CreateWindow("RoundWindow", tex, brokenTex, AssetLoader.TextureFromFile(Path.Combine(MiscPath, TextureFolder, GetAssetName("roundWindowMask.png"))), true);

			PostRoomCreation.window = window; // Set the metal window, as it is unbreakable

			
		}
	}
}
