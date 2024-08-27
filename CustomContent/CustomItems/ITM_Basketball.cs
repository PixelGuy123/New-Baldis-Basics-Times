using BBTimes.Extensions;
using BBTimes.CustomComponents;
using BBTimes.Manager;
using PixelInternalAPI.Classes;
using PixelInternalAPI.Extensions;
using System.Collections;
using UnityEngine;

namespace BBTimes.CustomContent.CustomItems
{
    public class ITM_Basketball : Item, IEntityTrigger, IItemPrefab
	{
		public void SetupPrefab()
		{
			var sprs = BBTimesManager.man.Get<Sprite[]>("basketBall");
			var rendererBase = ObjectCreationExtensions.CreateSpriteBillboard(sprs[0]);
			rendererBase.transform.SetParent(transform);
			rendererBase.transform.localPosition = Vector3.zero;
			rendererBase.gameObject.SetActive(true);

			gameObject.layer = LayerStorage.standardEntities;
			entity = gameObject.CreateEntity(2f, 2f, rendererBase.transform);

			audMan = gameObject.CreatePropagatedAudioManager(75, 105);
			audThrow = this.GetSoundNoSub("throw.wav", SoundType.Effect);
			audHit = BBTimesManager.man.Get<SoundObject>("audGenericPunch");
			audBong = this.GetSound("bounce.wav", "BB_Bong", SoundType.Voice, Color.white);
			audPop = BBTimesManager.man.Get<SoundObject>("audPop");
			spriteAnim = sprs;

			renderer = rendererBase;
		}

		public void SetupPrefabPost() { }

		public string Name { get; set; } public string TexturePath => this.GenerateDataPath("items", "Textures");
		public string SoundPath => this.GenerateDataPath("items", "Audios");
		public ItemObject ItmObj { get; set; }







		// Prefab Setup Above^^
		public override bool Use(PlayerManager pm)
		{
			Singleton<CoreGameManager>.Instance.audMan.PlaySingle(audThrow);
			target = pm.gameObject;
			Setup(pm.ec, Singleton<CoreGameManager>.Instance.GetCamera(pm.playerNumber).transform.forward, pm.transform.position, null);

			return true;
		}

		public void Setup(EnvironmentController ec, Vector3 direction, Vector3 pos, RoomController room, float speedDecrease = 0.2f)
		{
			entity.Initialize(ec, pos);
			this.ec = ec;
			dir = direction;
			targetRoom = room;
			moveMod.movementMultiplier = speedDecrease;

			entity.OnEntityMoveInitialCollision += (hit) => // Basically just bounce over
			{
				if (hasHit) return; // stop BONG spam

				dir = Vector3.Reflect(dir, hit.normal); // crazy math I guess
				audMan.PlaySingle(audBong);
			};
		}

		void Update()
		{
			if (hasHit) return;

			entity.UpdateInternalMovement(dir * speed * ec.EnvironmentTimeScale);

			lifeTime -= ec.EnvironmentTimeScale * Time.deltaTime;
			if (lifeTime < 0f || (targetRoom && !ec.CellFromPosition(transform.position).TileMatches(targetRoom)))
			{
				hasHit = true;
				renderer.enabled = false;
				StartCoroutine(PopWait());
			}


			// animation loop
			frame += 8f * ec.EnvironmentTimeScale * Time.deltaTime;
			frame %= spriteAnim.Length;
			renderer.sprite = spriteAnim[Mathf.FloorToInt(frame)];
		}

		public void EntityTriggerEnter(Collider other)
		{
			if (hasHit || other.gameObject == target) return;
			bool isnpc = other.CompareTag("NPC");
			if (other.isTrigger && (isnpc || other.CompareTag("Player")))
			{
				Entity e = other.GetComponent<Entity>();
				if (e)
				{
					if (isnpc && pm) pm.RuleBreak("Bullying", 1f);
					audMan.PlaySingle(audHit);
					renderer.enabled = false;
					hasHit = true;

					e.AddForce(new((other.transform.position - transform.position).normalized, speed * 1.9f, -speed));

					StartCoroutine(Timer(e));
				}
			}

		}
		public void EntityTriggerStay(Collider other)
		{
		}
		public void EntityTriggerExit(Collider other)
		{
			if (other.gameObject == target)
				target = null;
		}

		IEnumerator PopWait()
		{
			audMan.FlushQueue(true);
			audMan.QueueAudio(audPop);
			while (audMan.QueuedAudioIsPlaying)
				yield return null;

			Destroy(gameObject);
			yield break;
		}

		IEnumerator Timer(Entity e)
		{
			e.ExternalActivity.moveMods.Add(moveMod);
			float cooldown = 15f;
			while (cooldown > 0)
			{
				cooldown -= ec.EnvironmentTimeScale * Time.deltaTime;
				yield return null;
			}

			e.ExternalActivity.moveMods.Remove(moveMod);
			Destroy(gameObject);

			yield break;
		}

		GameObject target = null;
		RoomController targetRoom = null;
		float frame = 0f, lifeTime = 160f;
		bool hasHit = false;
		EnvironmentController ec;

		[SerializeField]
		internal Entity entity;

		[SerializeField]
		internal SpriteRenderer renderer;

		[SerializeField]
		internal Sprite[] spriteAnim;

		[SerializeField]
		internal AudioManager audMan;

		[SerializeField]
		internal SoundObject audThrow, audHit, audBong, audPop;

		Vector3 dir;

		const float speed = 25f;

		readonly MovementModifier moveMod = new(Vector3.zero, 0.2f);
	}
}
