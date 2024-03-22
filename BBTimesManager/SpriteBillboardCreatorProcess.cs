using BBTimes.CustomContent.Misc;
using BBTimes.ModPatches;
using BBTimes.ModPatches.EnvironmentPatches;
using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.Components;
using System.IO;
using UnityEngine;
using static UnityEngine.Object;

namespace BBTimes.Manager
{
	internal static partial class BBTimesManager
	{
		static void CreateSpriteBillboards()
		{
			// Decorations outside
			EnvironmentControllerMakeBeautifulOutside.decorations = [CreateSpriteBillboard(AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromFile(Path.Combine(MiscPath, TextureFolder, "flower.png")), 15f), 2.6f)];
			// Fire Object
			SchoolFire.anim = new([AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromFile(Path.Combine(MiscPath, TextureFolder, "SchoolFire.png")), 15f), AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromFile(Path.Combine(MiscPath, TextureFolder, "SchoolFire2.png")), 15f)], 2f);
			var fire = CreateSpriteAnimator<SchoolFire>("SchoolFire");
			fire.gameObject.SetActive(false);
			fire.spriteRenderer.material.SetTexture("_LightMap", null); // Don't get affected by reddish from schoolhouse
			MainGameManagerPatches.fire = fire.gameObject;
		}

		static T CreateSpriteAnimator<T>(string name) where T : CustomSpriteAnimator
		{
			var obj = Instantiate(man.Get<GameObject>("SpriteBillboardTemplate"));
			var animator = obj.AddComponent<T>();
			animator.spriteRenderer = obj.GetComponent<SpriteRenderer>();

			DontDestroyOnLoad(obj);
			obj.name = name;
			return animator;
		}

		static GameObject CreateSpriteBillboard(Sprite sprite)
		{
			var obj = Instantiate(man.Get<GameObject>("SpriteBillboardTemplate"));
			var comp = obj.GetComponent<SpriteRenderer>();
			comp.sprite = sprite;

			prefabs.Add(obj);
			
			DontDestroyOnLoad(obj);
			obj.name = "SpriteBillboard_" + sprite.name;
			obj.AddComponent<RendererContainer>().renderers = [comp];
			return obj;
		}

		static GameObject CreateSpriteBillboard(Sprite sprite, float yoffset)
		{
			var obj = CreateSpriteBillboard(sprite);
			prefabs.RemoveAt(prefabs.Count - 1);
			obj.SetActive(true);

			var parent = new GameObject("SpriteBillBoardHolder_" + sprite.name, typeof(RendererContainer));
			obj.transform.SetParent(parent.transform);
			obj.transform.localPosition = Vector3.up * yoffset;
			DontDestroyOnLoad(parent);
			parent.SetActive(false);
			parent.GetComponent<RendererContainer>().renderers = [obj.GetComponent<SpriteRenderer>()];
			parent.layer = LayerMask.NameToLayer("Billboard");

			
			prefabs.Add(parent);

			return parent;
		}
	}
}
