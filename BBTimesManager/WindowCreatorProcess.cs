using MTM101BaldAPI.AssetTools;
using System.IO;
using BBTimes.Misc.SelectionHolders;
using BBTimes.Helpers;
using BBTimes.ModPatches.EnvironmentPatches;
using System.Collections.Generic;

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
			((List<WindowObjectHolder>)floorDatas[1].levelObject.GetCustomModValue(plug.Info, "Times_EnvConfig_ExtraWindowsToSpawn")).Add(windowSel);
			((List<WindowObjectHolder>)floorDatas[2].levelObject.GetCustomModValue(plug.Info, "Times_EnvConfig_ExtraWindowsToSpawn")).Add(windowSel);
			((List<WindowObjectHolder>)floorDatas[3].levelObject.GetCustomModValue(plug.Info, "Times_EnvConfig_ExtraWindowsToSpawn")).Add(windowSel);

			// Round Window
			brokenTex = AssetLoader.TextureFromFile(Path.Combine(MiscPath, TextureFolder, GetAssetName("roundWindow.png")));
			tex = brokenTex;
			window = CreatorExtensions.CreateWindow("RoundWindow", tex, brokenTex, AssetLoader.TextureFromFile(Path.Combine(MiscPath, TextureFolder, GetAssetName("roundWindowMask.png"))), true);

			EnvironmentControllerMakeBeautifulOutside.window = window; // Set the metal window, as it is unbreakable

			
		}
	}
}
