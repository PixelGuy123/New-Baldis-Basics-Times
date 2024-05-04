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
using PixelInternalAPI;

namespace BBTimes.Manager
{
	internal static partial class BBTimesManager
	{
		static void CreateSpriteBillboards() // Solo sprite bill boards btw, that isn't added to other categorized stuff like Room Function (that would be in RoomFunctionCreatorProcess)
		{
			// Decorations outside
			EnvironmentControllerMakeBeautifulOutside.decorations = [ObjectCreationExtensions.CreateSpriteBillboard(AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromFile(Path.Combine(MiscPath, TextureFolder, "flower.png")), 15f)).AddSpriteHolder(2.6f).transform.parent.gameObject.SetAsPrefab().AddAsGeneratorPrefab()];
			// Fire Object
			SchoolFire.anim = new([AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromFile(Path.Combine(MiscPath, TextureFolder, "SchoolFire.png")), 15f), AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromFile(Path.Combine(MiscPath, TextureFolder, "SchoolFire2.png")), 15f)], 2f);
			var fire = ObjectCreationExtensions.CreateSpriteBillboard(null).AddSpriteAnimator<SchoolFire>(out var fireAnimator).gameObject.SetAsPrefab();
			fire.name = "Fire";
			fireAnimator.spriteRenderer.material.SetTexture("_LightMap", null); // Don't get affected by reddish from schoolhouse
			MainGameManagerPatches.fire = fire;

			// Elevator exit signs
			GenericExtensions.FindResourceObjects<Elevator>().Do((x) =>
			{
				var exit = ObjectCreationExtensions.CreateSpriteBillboard(AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromFile(Path.Combine(BasePlugin.ModPath, "objects", "Elevator", "ExitSignSprite.png")), 25f)).AddSpriteHolder(8.75f);
				exit.material.SetTexture("Texture2D_0ebe02d67a8a4acb8705243366af66aa", AssetLoader.TextureFromFile(Path.Combine(BasePlugin.ModPath, "objects", "Elevator", "ExitSign_LightMap.png"))); // Why does it have to be a so fucking long name
				exit.transform.parent.SetParent(x.transform);
				exit.transform.parent.localPosition = x.transform.forward * LayerStorage.TileBaseOffset;
			});

			// Hanging ceiling light for cafeteria
			var hangingLight = ObjectCreationExtensions.CreateSpriteBillboard(AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromFile(Path.Combine(MiscPath, TextureFolder, "cafeHangingLight.png")), 25f)).AddSpriteHolder(40f);

			hangingLight.transform.localScale = Vector3.one * 1.4f;

			man.Add("prefab_cafeHangingLight", hangingLight.transform.parent.gameObject.SetAsPrefab().AddAsGeneratorPrefab());
		}
	}
}
