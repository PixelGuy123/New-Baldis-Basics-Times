using BBTimes.CustomContent.Misc;
using BBTimes.ModPatches;
using BBTimes.ModPatches.EnvironmentPatches;
using MTM101BaldAPI.AssetTools;
using System.IO;
using UnityEngine;
using HarmonyLib;
using BBTimes.Plugin;
using PixelInternalAPI.Classes;
using PixelInternalAPI.Extensions;
using BBTimes.Extensions;
using MTM101BaldAPI;

namespace BBTimes.Manager
{
	internal static partial class BBTimesManager
	{
		static void CreateSpriteBillboards() // Solo sprite bill boards btw, that isn't added to other categorized stuff like Room Function (that would be in RoomFunctionCreatorProcess)
		{
			// Decorations outside
			EnvironmentControllerMakeBeautifulOutside.decorations = [ObjectCreationExtensions.CreateSpriteBillboard(AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromFile(Path.Combine(MiscPath, TextureFolder, "flower.png")), 15f)).AddSpriteHolder(2.6f).transform.parent.gameObject.SetAsPrefab(true)];
			// Fire Object
			var fire = ObjectCreationExtensions.CreateSpriteBillboard(null);
			fire.gameObject.ConvertToPrefab(true);
			// 
			fire.name = "Fire";
			fire.material.SetTexture("_LightMap", null); // Don't get affected by reddish from schoolhouse
			var fireAnim = fire.gameObject.AddComponent<SchoolFire>();
			fireAnim.animation = TextureExtensions.LoadSpriteSheet(2, 1, 15f, MiscPath, TextureFolder, "schoolFire.png");
			fireAnim.speed = 2f;
			fireAnim.renderer = fire;

			MainGameManagerPatches.fire = fire.gameObject;

			// Elevator exit signs
			GenericExtensions.FindResourceObjects<Elevator>().Do((x) =>
			{
				var exit = Object.Instantiate(GenericExtensions.FindResourceObjectByName<RendererContainer>("Decor_ExitSign"));
				exit.transform.SetParent(x.transform);
				exit.transform.localPosition = x.transform.forward * LayerStorage.TileBaseOffset + Vector3.up * 10f;
			});

			// Hanging ceiling light for cafeteria
			var hangingLight = ObjectCreationExtensions.CreateSpriteBillboard(AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromFile(Path.Combine(MiscPath, TextureFolder, "cafeHangingLight.png")), 25f)).AddSpriteHolder(40f);

			hangingLight.transform.localScale = Vector3.one * 1.4f;

			man.Add("prefab_cafeHangingLight", hangingLight.transform.parent.gameObject.SetAsPrefab(true));

			// Hanging light for library
			man.Add("prefab_libraryHangingLight", ObjectCreationExtensions.CreateSpriteBillboard(AssetLoader.SpriteFromTexture2D(GenericExtensions.FindResourceObjectByName<Texture2D>("StandardHangingLight"), 25f))
				.AddSpriteHolder(18.1f).transform.parent.gameObject.SetAsPrefab(true));
		}
	}
}
