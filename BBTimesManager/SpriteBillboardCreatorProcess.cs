using BBTimes.CustomContent.Misc;
using BBTimes.ModPatches;
using BBTimes.ModPatches.EnvironmentPatches;
using MTM101BaldAPI.AssetTools;
using BBTimes.Extensions.ObjectCreationExtensions;
using System.IO;
using UnityEngine;
using HarmonyLib;
using BBTimes.Plugin;

namespace BBTimes.Manager
{
	internal static partial class BBTimesManager
	{
		static void CreateSpriteBillboards() // Solo sprite bill boards btw, that isn't added to other categorized stuff like Room Function (that would be in RoomFunctionCreatorProcess)
		{
			// Decorations outside
			EnvironmentControllerMakeBeautifulOutside.decorations = [ObjectCreationExtension.CreateSpriteBillboard(AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromFile(Path.Combine(MiscPath, TextureFolder, "flower.png")), 15f), 2.6f)];
			// Fire Object
			SchoolFire.anim = new([AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromFile(Path.Combine(MiscPath, TextureFolder, "SchoolFire.png")), 15f), AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromFile(Path.Combine(MiscPath, TextureFolder, "SchoolFire2.png")), 15f)], 2f);
			var fire = ObjectCreationExtension.CreateSpriteAnimator<SchoolFire>("SchoolFire");
			fire.gameObject.SetActive(false);
			fire.spriteRenderer.material.SetTexture("_LightMap", null); // Don't get affected by reddish from schoolhouse
			MainGameManagerPatches.fire = fire.gameObject;

			// Elevator exit signs
			Resources.FindObjectsOfTypeAll<Elevator>().Do((x) => {
				var exit = ObjectCreationExtension.CreateSpriteBillboard(AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromFile(Path.Combine(BasePlugin.ModPath, "objects", "Elevator", "ExitSignSprite.png")), 25f), 8.75f);
				exit.GetComponent<RendererContainer>().renderers[0].material.SetTexture("Texture2D_0ebe02d67a8a4acb8705243366af66aa", AssetLoader.TextureFromFile(Path.Combine(BasePlugin.ModPath, "objects", "Elevator", "ExitSign_LightMap.png"))); // Why does it have to be a so fucking long name
				exit.transform.SetParent(x.transform);
				exit.transform.localPosition = x.transform.forward * TileBaseOffset;
			});

			// Hanging ceiling light for cafeteria
			var hangingLight = ObjectCreationExtension.CreateSpriteBillboard(AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromFile(Path.Combine(MiscPath, TextureFolder, "cafeHangingLight.png")), 25f), 40f);

			hangingLight.GetComponent<RendererContainer>().renderers[0].transform.localScale = Vector3.one * 1.4f;
			hangingLight.SetActive(false);
			Object.DontDestroyOnLoad(hangingLight);



			prefabs.Add(hangingLight);
			man.Add("prefab_cafeHangingLight", hangingLight);
		}	
	}
}
