using BBTimes.ModPatches.EnvironmentPatches;
using MTM101BaldAPI.AssetTools;
using System.IO;
using UnityEngine;
using static UnityEngine.Object;

namespace BBTimes.Manager
{
	internal static partial class BBTimesManager
	{
		static void CreateSpriteBillboards()
		{
			EnvironmentControllerMakeBeautifulOutside.decorations = [CreateSpriteBillboard(AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromFile(Path.Combine(MiscPath, TextureFolder, "flower.png")), 15f), 2.6f)];

			
			
		}

		static GameObject CreateSpriteBillboard(Sprite sprite)
		{
			var obj = Instantiate(man.Get<GameObject>("SpriteBillboardTemplate"));
			var comp = obj.GetComponent<SpriteRenderer>();
			comp.sprite = sprite;
			obj.SetActive(false);
			DontDestroyOnLoad(obj);
			obj.name = "SpriteBillboard_" + sprite.name;
			obj.AddComponent<RendererContainer>().renderers = [comp];
			return obj;
		}

		static GameObject CreateSpriteBillboard(Sprite sprite, float yoffset)
		{
			var obj = CreateSpriteBillboard(sprite);
			obj.SetActive(true);

			var parent = new GameObject("SpriteBillBoardHolder_" + sprite.name, typeof(RendererContainer));
			obj.transform.SetParent(parent.transform);
			obj.transform.localPosition = Vector3.up * yoffset;
			DontDestroyOnLoad(parent);
			parent.SetActive(false);
			parent.GetComponent<RendererContainer>().renderers = [obj.GetComponent<SpriteRenderer>()];
			parent.layer = LayerMask.NameToLayer("Billboard");

			return parent;
		}
	}
}
