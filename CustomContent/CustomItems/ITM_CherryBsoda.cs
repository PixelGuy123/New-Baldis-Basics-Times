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

			spriteRenderer = nbsoda.spriteRenderer;
			sound = nbsoda.sound;
			entity = nbsoda.entity;
			entity.collisionLayerMask = LayerStorage.gumCollisionMask;
			time = nbsoda.time;
			moveMod = nbsoda.moveMod;
			audMan = gameObject.CreatePropagatedAudioManager(65, 125);
			audHit = this.GetSoundNoSub("hit.wav", SoundType.Voice);

			spriteRenderer.sprite = this.GetSprite(spriteRenderer.sprite.pixelsPerUnit, "spray.png");
			speed = 45f;
			Destroy(transform.Find("RendereBase").Find("Particles").gameObject);

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
