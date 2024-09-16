using BBTimes.CustomContent.Misc;
using BBTimes.ModPatches;
using BBTimes.ModPatches.EnvironmentPatches;
using MTM101BaldAPI.AssetTools;
using System.IO;
using UnityEngine;
using HarmonyLib;
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
			var flowerSprites = TextureExtensions.LoadSpriteSheet(8, 1, 25f, MiscPath, TextureFolder, "flowers.png");

			GameObject[] flowers = new GameObject[flowerSprites.Length];
			for (int i = 0; i < flowers.Length; i++)
				flowers[i] = ObjectCreationExtensions.CreateSpriteBillboard(flowerSprites[i]).AddSpriteHolder(2.6f).transform.parent.gameObject.SetAsPrefab(true);

			EnvironmentControllerMakeBeautifulOutside.decorations = flowers;
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

			// Misc Decorations
			AddDecoration("SecretBread", "bread.png", 35f, Vector3.up * 1.3f);

			static void AddDecoration(string name, string fileName, float pixelsPerUnit, Vector3 offset)
			{
				var bred = ObjectCreationExtensions.CreateSpriteBillboard(AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromFile(Path.Combine(MiscPath, TextureFolder, fileName)), pixelsPerUnit)).AddSpriteHolder(offset);
				bred.transform.parent.name = name;
				bred.name = name;
				bred.transform.parent.gameObject.AddObjectToEditor();
				//"editorPrefab_"
			}
		}
	}
}
