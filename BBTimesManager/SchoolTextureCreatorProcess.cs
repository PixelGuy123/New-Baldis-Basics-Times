using BBTimes.Misc;
using BBTimes.Misc.SelectionHolders;
using BBTimes.Plugin;
using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using System;
using System.IO;
using UnityEngine;

namespace BBTimes.Manager
{
	internal static partial class BBTimesManager
	{
		static void CreateSchoolTextures()
		{
			// 0 - F1
			// 1 - F2
			// 2 - F3
			// 3 - END

			foreach (var rootDir in Directory.GetDirectories(Path.Combine(BasePlugin.ModPath, "rooms")))
			{
				RoomCategory cat;
				string dirName = Path.GetFileName(rootDir); // Yes, file name. Get Directory name isn't as it looks
				try
				{
					if (Enum.TryParse(dirName, out RoomCategory c))
						cat = c;
					else
						cat = EnumExtensions.GetFromExtendedName<RoomCategory>(dirName);

				}
				catch
				{
					Debug.LogWarning("BB TIMES: Failed to load texture for the room category, it doesn\'t exist: " + dirName);
					continue;
				}

				foreach (var file in Directory.GetFiles(rootDir))
				{
					string[] data = Path.GetFileNameWithoutExtension(file).Split('_');
					if (data.Length < 4) // Not a data file
						continue;

					Texture2D tex = AssetLoader.TextureFromFile(file);
					SchoolTexture texType = data[1].GetSchoolTextureFromName(); // 1 expected to be the type
					if (texType == SchoolTexture.Null)
					{
						Debug.LogWarning("BB TIMES: Invalid data in SchoolTexture: " + data[1]);
						continue;
					}

					if (int.TryParse(data[2], out int weight))
					{
						var holder = new SchoolTextureHolder(tex, weight, cat, texType);

						for (int i = 3; i < data.Length; i++)
						{
							switch (data[i].ToLower())
							{
								case "f1": floorDatas[0].SchoolTextures.Add(holder); break;
								case "f2": floorDatas[1].SchoolTextures.Add(holder); break;
								case "f3": floorDatas[2].SchoolTextures.Add(holder); break;
								case "end": floorDatas[3].SchoolTextures.Add(holder); break;
							}
						}
					}
					else
						Debug.LogWarning("BB TIMES: Invalid data in Weight: " + data[2]);

				}
			}
			

		}
	}
}
