using System.Collections.Generic;
using System.IO;
using System.Linq;
using BBTimes.CustomContent.Objects;
using BBTimes.Helpers;
using BBTimes.Misc.SelectionHolders;
using BBTimes.ModPatches.EnvironmentPatches;
using MTM101BaldAPI.AssetTools;

namespace BBTimes.Manager
{
	internal static partial class BBTimesManager
	{
		static void CreateWindows()
		{
			// Metal Window
			var brokenTex = AssetLoader.TextureFromFile(Path.Combine(MiscPath, TextureFolder, GetAssetName("MetalWindow.png")));
			var tex = brokenTex;
			var window = CreatorExtensions.CreateWindow("MetalWindow", tex, brokenTex, unbreakable: true);
			window.windowPre.gameObject.AddComponent<MetalWindow>().window = window.windowPre;
			var windowSel = new WindowObjectHolder(window, 75, [RoomCategory.Office]);

			AddWindowsToFloor(F4, windowSel, LevelType.Maintenance, LevelType.Laboratory);
			AddWindowsToFloor(F5, windowSel, LevelType.Maintenance, LevelType.Laboratory);
			AddWindowsToFloor(END, windowSel);

			// Round Window
			brokenTex = AssetLoader.TextureFromFile(Path.Combine(MiscPath, TextureFolder, GetAssetName("roundWindow.png")));
			tex = brokenTex;
			window = CreatorExtensions.CreateWindow("RoundWindow", tex, brokenTex, AssetLoader.TextureFromFile(Path.Combine(MiscPath, TextureFolder, GetAssetName("roundWindowMask.png"))), true);

			EnvironmentControllerMakeBeautifulOutside.window = window; // Set the metal window, as it is unbreakable


			// Add Windows to all level objects from a sceneObject
			static void AddWindowsToFloor(string floorName, WindowObjectHolder window, params LevelType[] floorTypes)
			{
				foreach (var ld in floorDatas[floorName].levelObjects)
				{
					if (floorTypes.Length == 0 || floorTypes.Contains(ld.type))
						((List<WindowObjectHolder>)ld.GetCustomModValue(plug.Info, "Times_EnvConfig_ExtraWindowsToSpawn")).Add(window);
				}
			}
		}
	}
}
