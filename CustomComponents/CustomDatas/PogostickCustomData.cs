using BBTimes.CustomContent.CustomItems;
using PixelInternalAPI.Extensions;
using PixelInternalAPI.Classes;
using UnityEngine;

namespace BBTimes.CustomComponents.CustomDatas
{
	public class PogostickCustomData : CustomItemData
	{
		protected override SoundObject[] GenerateSoundObjects() =>
			[GetSound("boing.wav", "POGST_Boing", SoundType.Effect, Color.white)];

		public override void SetupPrefab()
		{
			base.SetupPrefab();
			var po = GetComponent<ITM_Pogostick>();
			po.audBoing = soundObjects[0];
			var falseRenderer = new GameObject("PogoStickRendererBase");
			falseRenderer.transform.SetParent(transform);
			falseRenderer.transform.localPosition = Vector3.zero;

			
			po.entity = gameObject.CreateEntity(2f, rendererBase: falseRenderer.transform);
			gameObject.layer = LayerStorage.ignoreRaycast;

			po.rendererBase = falseRenderer.transform;
		}
	}
}
