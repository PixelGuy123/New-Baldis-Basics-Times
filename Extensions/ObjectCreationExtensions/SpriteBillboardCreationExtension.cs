using MTM101BaldAPI.Components;
using BBTimes.Manager;
using PixelInternalAPI.Classes;
using UnityEngine;
using static UnityEngine.Object;

namespace BBTimes.Extensions.ObjectCreationExtensions
{
	public static partial class ObjectCreationExtension
	{
		public static T CreateSpriteAnimator<T>(string name) where T : CustomSpriteAnimator
		{
			var obj = Instantiate(BBTimesManager.man.Get<GameObject>("SpriteBillboardTemplate"));
			var animator = obj.AddComponent<T>();
			animator.spriteRenderer = obj.GetComponent<SpriteRenderer>();

			DontDestroyOnLoad(obj);
			obj.name = name;
			return animator;
		}

		public static GameObject CreateSpriteBillboard(Sprite sprite, bool isPrefab = true)
		{
			var obj = Instantiate(BBTimesManager.man.Get<GameObject>("SpriteBillboardTemplate"));
			var comp = obj.GetComponent<SpriteRenderer>();
			comp.sprite = sprite;

			if (isPrefab)
				BBTimesManager.prefabs.Add(obj);

			DontDestroyOnLoad(obj);
			obj.name = "SpriteBillboard_" + sprite.name;
			obj.AddComponent<RendererContainer>().renderers = [comp];
			obj.SetActive(false);
			return obj;
		}

		public static GameObject CreateSpriteBillboard(Sprite sprite, float yoffset, bool isPrefab = true)
		{
			var obj = CreateSpriteBillboard(sprite, false);
			obj.SetActive(true);

			var parent = new GameObject("SpriteBillBoardHolder_" + sprite.name, typeof(RendererContainer));
			obj.transform.SetParent(parent.transform);
			obj.transform.localPosition = Vector3.up * yoffset;
			Destroy(obj.GetComponent<RendererContainer>()); // The parent should hold it
			DontDestroyOnLoad(parent);
			parent.SetActive(false);
			parent.GetComponent<RendererContainer>().renderers = [obj.GetComponent<SpriteRenderer>()];
			parent.layer = LayerStorage.billboardLayer;

			if (isPrefab)
				BBTimesManager.prefabs.Add(parent);

			return parent;
		}

		public static GameObject CreateSpriteBillboard(Sprite sprite, float yoffset, LayerMask layer, bool isPrefab = true)
		{
			var obj = CreateSpriteBillboard(sprite, yoffset, isPrefab);
			obj.layer = layer;
			return obj;
		}

		public static GameObject CreateSpriteBillboard(Sprite sprite, LayerMask layer, bool isPrefab = true)
		{
			var obj = CreateSpriteBillboard(sprite, isPrefab);
			obj.layer = layer;
			return obj;
		}
	}
}
