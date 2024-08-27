using BBTimes.CustomComponents;
using BBTimes.Extensions;
using PixelInternalAPI.Classes;
using PixelInternalAPI.Extensions;
using UnityEngine;

namespace BBTimes.CustomContent.CustomItems
{
	public class ITM_CherryBsoda : ITM_BSODA, IItemPrefab
	{
		public void SetupPrefab()
		{
			var nbsoda = GetComponent<ITM_BSODA>();
			var bsoda = gameObject.GetComponent<ITM_CherryBsoda>();

			bsoda.spriteRenderer = nbsoda.spriteRenderer;
			bsoda.sound = nbsoda.sound;
			bsoda.entity = nbsoda.entity;
			bsoda.time = nbsoda.time;
			bsoda.moveMod = nbsoda.moveMod;
			bsoda.audMan = gameObject.CreatePropagatedAudioManager(65, 125);
			bsoda.audHit = this.GetSoundNoSub("hit.wav", SoundType.Voice);

			bsoda.spriteRenderer.sprite = this.GetSprite(bsoda.spriteRenderer.sprite.pixelsPerUnit, "spray.png");
			bsoda.speed = 45f;
			bsoda.entity.collisionLayerMask = LayerStorage.entityCollisionMask; // Default entity collision mask
			Destroy(bsoda.transform.Find("RendereBase").Find("Particles").gameObject);

			Destroy(nbsoda);
		}

		public void SetupPrefabPost() { }

		public string Name { get; set; } public string TexturePath => this.GenerateDataPath("items", "Textures");
		public string SoundPath => this.GenerateDataPath("items", "Audios");
		public ItemObject ItmObj { get; set; }

		public override bool Use(PlayerManager pm)
		{
			dir = Singleton<CoreGameManager>.Instance.GetCamera(pm.playerNumber).transform.forward;
			entity.OnEntityMoveInitialCollision += (hit) => // Basically just bounce over
			{
				dir = Vector3.Reflect(dir, hit.normal);
				transform.forward = dir;
				audMan.PlaySingle(audHit);
				time -= 0.5f;
			};
			return base.Use(pm);
		}

		Vector3 dir;

		[SerializeField]
		internal PropagatedAudioManager audMan;

		[SerializeField]
		internal SoundObject audHit;
	}
}
