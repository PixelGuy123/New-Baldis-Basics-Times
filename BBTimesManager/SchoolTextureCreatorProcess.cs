using System;
using System.IO;
using BBTimes.Misc;
using BBTimes.Misc.SelectionHolders;
using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
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
				bool modded = false;
				string dirName = Path.GetFileName(rootDir); // Yes, file name. Get Directory name isn't as it looks
#if CHEAT
				Debug.Log($"Loading texture from dir: {dirName}");
#endif
				try
				{
					if (Enum.TryParse(dirName, out RoomCategory c))
					{
						cat = c;
						modded = c == RoomCategory.Special;
					}
					else
					{
						cat = EnumExtensions.GetFromExtendedName<RoomCategory>(dirName);
						modded = true;
					}

				}
				catch
				{
					//Debug.LogWarning("BB TIMES: Failed to load texture for the room category, it doesn\'t exist: " + dirName);
					continue;
				}

				foreach (var file in Directory.GetFiles(rootDir))
				{
					string[] data = Path.GetFileNameWithoutExtension(file).Split('_');
					if (data.Length < (modded ? 2 : 3)) // Not a data file
						continue;

#if CHEAT
					Debug.Log("Loading texture file: " + data[0]);
#endif


					SchoolTexture texType = data[1].GetSchoolTextureFromName(); // 1 expected to be the type
					if (texType == SchoolTexture.Null)
					{
						Debug.LogWarning("BB TIMES: Invalid data in SchoolTexture: " + data[1]);
						continue;
					}
					Texture2D tex = AssetLoader.TextureFromFile(file);

					if (modded)
						AddTextureToEditor(data[0], tex);

					Debug.Log($"Texture size is {tex.width}x{tex.height}");

					int weight = 50;
					if (data.Length < 3 || int.TryParse(data[2], out weight))
					{
						var holder = new SchoolTextureHolder(tex, weight, cat, texType);

						if (!modded)
							return;
						{
							foreach (var fData in floorDatas)
								fData.Value.SchoolTextures.Add(holder);
							_moddedAssets.ForEach((x) =>
							{
								if (x.category == cat)
								{
									switch (holder.TextureType)
									{
										case SchoolTexture.Ceiling:
											x.ceilTex = holder.Selection.selection;
											break;
										case SchoolTexture.Floor:
											x.florTex = holder.Selection.selection;
											break;
										case SchoolTexture.Wall:
											x.wallTex = holder.Selection.selection;
											break;
										default:
											break;
									}
								}
							});
						}
					}
					else
						Debug.LogWarning("BB TIMES: Invalid data in Weight: " + data[2]);

				}
			}


		}
	}
}
