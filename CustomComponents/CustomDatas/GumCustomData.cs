using UnityEngine;
using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using System.IO;
using BBTimes.CustomContent.CustomItems;
using PixelInternalAPI.Classes;
using PixelInternalAPI.Extensions;

namespace BBTimes.CustomComponents.CustomDatas
{
	public class GumCustomData : CustomItemData
	{
		protected override SoundObject[] GenerateSoundObjects() =>
			[ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(Path.Combine(SoundPath, "gum_spit.wav")), "Vfx_GUM_spit", SoundType.Effect, new Color(1, 0.2039f, 0.8863f))];
		public override void SetupPrefab()
		{
			base.SetupPrefab();

			var comp = GetComponent<GumItem>();
			gameObject.layer = LayerStorage.standardEntities;

			comp.audMan = gameObject.CreateAudioManager(55, 75, true);
			comp.aud_fly = GenericExtensions.FindResourceObjectByName<SoundObject>("Ben_Gum_Whoosh");
			comp.aud_splash = GenericExtensions.FindResourceObjectByName<SoundObject>("Ben_Splat");
			comp.aud_spit = soundObjects[0];

			comp.rendererBase = Instantiate(Resources.FindObjectsOfTypeAll<Gum>()[0].transform.Find("RendererBase"));
			comp.rendererBase.SetParent(transform);
			comp.rendererBase.localPosition = Vector3.zero;

			comp.flyingSprite = comp.rendererBase.Find("Sprite_Flying");
			comp.groundedSprite = comp.rendererBase.Find("Sprite_Grounded");

			comp.entity = gameObject.CreateEntity(1f, 1f, out var collider, out _, comp.rendererBase, [comp]).SetEntityCollisionLayerMask(2113537); // 2113537 is the layermask of the gum, I can't figure out what are the layers because Unity sucks
			collider.height = 4;
			
		}
	}
}
