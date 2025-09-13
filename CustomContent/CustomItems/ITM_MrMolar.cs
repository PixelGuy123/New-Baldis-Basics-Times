using System.Collections;
using BBTimes.CustomComponents;
using BBTimes.Extensions;
using PixelInternalAPI.Classes;
using PixelInternalAPI.Extensions;
using UnityEngine;

namespace BBTimes.CustomContent.CustomItems
{
	public class ITM_MrMolar : Item, IItemPrefab, IEntityTrigger
	{
		public void SetupPrefab()
		{
			audMan = gameObject.CreatePropagatedAudioManager(115f, 145f);
			audWind = GenericExtensions.FindResourceObject<ITM_AlarmClock>().audWind;
			audActive = this.GetSound("Molar_Loop.wav", "Vfx_MrMolar_Active", SoundType.Effect, Color.white);
			audBite = this.GetSound("Molar_Bite.wav", "Vfx_MrMolar_Bite", SoundType.Effect, Color.white);

			var sprs = this.GetSpriteSheet(12, 1, 45f, "molarEntity.png");
			var renderBase = ObjectCreationExtensions.CreateSpriteBillboard(sprs[0]).AddSpriteHolder(out var render, 0f);
			renderBase.name = "MolarRenderBase";
			render.name = "MolarRenderer";
			renderBase.transform.SetParent(transform);
			renderBase.transform.localPosition = Vector3.zero;

			gameObject.layer = LayerStorage.standardEntities;
			entity = gameObject.CreateEntity(2f, 2f, renderBase.transform);

			sprWinding = sprs.TakeAPair(0, 4);
			sprBite = sprs.TakeAPair(4, 4);

			rotator = render.CreateAnimatedSpriteRotator(
				GenericExtensions.CreateRotationMap(2, [sprs[4], sprs[8]]),
				GenericExtensions.CreateRotationMap(2, [sprs[5], sprs[9]]),
				GenericExtensions.CreateRotationMap(2, [sprs[6], sprs[10]]),
				GenericExtensions.CreateRotationMap(2, [sprs[7], sprs[11]])
				);
			rotator.enabled = false;

			animComp = gameObject.AddComponent<AnimationComponent>();
			animComp.renderers = [render];
			animComp.rotators = [rotator];
			animComp.speed = 8f;
			animComp.animation = sprWinding;

			canvas = ObjectCreationExtensions.CreateCanvas();
			canvas.transform.SetParent(transform);
			canvas.gameObject.SetActive(false);
			canvas.name = "MolarCanvas";

			arrow = new GameObject("Molar_ArrowMinigame").transform;
			arrow.SetParent(canvas.transform);
			arrow.localPosition = new(0f, -73f, 0f);

			var arrowRenderer = ObjectCreationExtensions.CreateImage(canvas, this.GetSprite(65f, "molar_arrow.png"), false);
			arrowRenderer.name = "Molar_ArrowRenderer";
			arrowRenderer.transform.SetParent(arrow);
			arrowRenderer.transform.localScale = new(0.6f, 1f, 1f);
			arrowRenderer.transform.localPosition = new(-5.75f, -73f, 0f);

			var bar = ObjectCreationExtensions.CreateImage(canvas, this.GetSprite(25f, "molar_bar.png"), false);
			bar.name = "Molar_Bar";
			bar.transform.localScale = new(2f, 1f, 1f);
			bar.transform.localPosition = new(-30.78f, -131f);
		}
		public void SetupPrefabPost() { }

		public string Name { get; set; }
		public string Category => "items";

		public ItemObject ItmObj { get; set; }



		public override bool Use(PlayerManager pm)
		{
			if (activeMolars >= 1)
			{
				Destroy(gameObject);
				return false;
			}

			this.pm = pm;
			ec = pm.ec;
			owner = pm.gameObject;
			animComp.Initialize(ec);
			animComp.Pause(true);

			entity.Initialize(ec, pm.transform.position);
			entity.OnEntityMoveInitialCollision += Hit;

			life = lifeTime;

			StartCoroutine(Minigame());
			return true;
		}

		void Hit(RaycastHit hit) =>
			dir = Vector3.Reflect(dir, hit.normal);

		void OnDestroy()
		{
			if (canDecrementMolar)
				activeMolars--;
		}

		IEnumerator Minigame()
		{
			MovementModifier moveMod = new(Vector3.zero, 0.65f);
			pm.Am.moveMods.Add(moveMod);

			canDecrementMolar = true;
			activeMolars++;
			canvas.gameObject.SetActive(true);
			canvas.worldCamera = Singleton<CoreGameManager>.Instance.GetCamera(pm.playerNumber).canvasCam;
			float x = -arrowMaxOffset, offsetCompensation = 0f, speedMultiplier = 1f;
			int direction = 1;
			Vector2 pos = arrow.transform.position;
			int rounds = 0;

			while (true)
			{
				// Molar Position Update
				entity.Teleport(pm.transform.position + (Singleton<CoreGameManager>.Instance.GetCamera(pm.playerNumber).transform.forward * 6f));

				// Minigame's arrow position
				x += offsetCompensation + (direction * Time.deltaTime * ec.EnvironmentTimeScale * 180f * speedMultiplier);
				offsetCompensation = 0f;

				float absX = Mathf.Abs(x);
				if (absX >= arrowMaxOffset)
				{
					direction = -direction;
					x = Mathf.Clamp(x, -arrowMaxOffset, arrowMaxOffset);
					offsetCompensation = absX - arrowMaxOffset;
				}
				pos.x = x;
				arrow.localPosition = pos;


				// Minigame here
				bool click = Singleton<InputManager>.Instance.GetDigitalInput("Interact", true);
				if (click && !clicked && Time.timeScale != 0f)
				{
					audMan.PlaySingle(audWind);
					clicked = true;
					speedMultiplier *= 1.75f;
					x = -arrowMaxOffset;
					int factorReference = 1;

					for (int i = 0; i < offsets.Length; i++)
						if (x >= -offsets[i] && x <= offsets[i])
							factorReference = i + 1;

					bitePower *= defaultIncreasingFactor * factorReference;
					// Debug.Log("MOLAR: updated bite power: " + bitePower);

					if (++rounds >= maxRounds)
						break; // Breaks out of While loop

					animComp.animation = sprWinding;
					animComp.StopLastFrameMode();
					animComp.ResetFrame(true);
				}
				else if (!click)
					clicked = false;

				yield return null;
			}

			canDecrementMolar = false;
			activeMolars--;
			canvas.gameObject.SetActive(false);
			pm.Am.moveMods.Remove(moveMod);

			// Initialize the Entity from here
			active = true;
			dir = Singleton<CoreGameManager>.Instance.GetCamera(pm.playerNumber).transform.forward;
			entity.Teleport(pm.transform.position + (Singleton<CoreGameManager>.Instance.GetCamera(pm.playerNumber).transform.forward * 6f));
			animComp.ResetFrame(true);
			animComp.animation = sprBite;
			animComp.speed = 8f * bitePower;
			rotator.enabled = true;

			audMan.FlushQueue(true);
			audMan.maintainLoop = true;
			audMan.SetLoop(true);
			audMan.QueueAudio(audActive);
		}

		void Update()
		{
			if (!active) return;

			transform.rotation = Quaternion.LookRotation(dir, Vector3.up);

			if (!biting && !attachedEntity)
			{
				entity.UpdateInternalMovement(speed * dir);
				return;
			}

			life -= ec.EnvironmentTimeScale * Time.deltaTime;

			if (!attachedEntity || life <= 0f)
			{
				Destroy(gameObject);
				return;
			}

			entity.UpdateInternalMovement(Vector3.zero);
			entity.Teleport(attachedEntity.transform.position);
			biteDelay -= ec.EnvironmentTimeScale * Time.deltaTime;
			if (biteDelay <= 0f)
			{
				biteDelay += Random.Range(minBiteDelay, maxBiteDelay) / (bitePower * 10f);
				audMan.PlaySingle(audBite);
				attachedEntity.AddForce(new(dir, biteForce * bitePower * 2.15f, -biteForce)); // Intentionally without multiplying the acceleration
			}
		}

		public void EntityTriggerEnter(Collider other, bool validCollision) { }

		public void EntityTriggerExit(Collider other, bool validCollision)
		{
			if (validCollision && owner == other.gameObject)
				owner = null;
		}

		public void EntityTriggerStay(Collider other, bool validCollision)
		{
			if (!validCollision || biting || !active && owner == other.gameObject) return;

			if (other.isTrigger && (other.CompareTag("NPC") || other.CompareTag("Player")))
			{
				var e = other.GetComponent<Entity>();
				if (e)
				{
					biting = true;
					life += lifeTime;
					attachedEntity = e;
					entity.OnEntityMoveInitialCollision -= Hit;
				}
			}
		}


		[SerializeField]
		internal PropagatedAudioManager audMan;

		[SerializeField]
		internal SoundObject audWind, audActive, audBite;

		[SerializeField]
		internal Sprite[] sprWinding, sprBite;

		[SerializeField]
		internal Entity entity;

		[SerializeField]
		internal Canvas canvas;

		[SerializeField]
		internal Transform arrow;

		[SerializeField]
		internal AnimatedSpriteRotator rotator;

		[SerializeField]
		internal int maxRounds = 3;

		[SerializeField]
		internal float arrowMaxOffset = 60.2f, defaultIncreasingFactor = 1.25f, speed = 21f, minBiteDelay = 3.5f, maxBiteDelay = 6f, biteForce = 5f, lifeTime = 30f;

		[SerializeField]
		internal AnimationComponent animComp;

		[SerializeField]
		internal float[] offsets = [50.7f, 40.66f, 28.35f, 13.8f]; // ordered by descending, it looks for the smaller offset from the arrow, relative to the center of the screen.

		bool clicked = false, active = false, biting = false, canDecrementMolar = false;
		Entity attachedEntity;
		GameObject owner;
		Vector3 dir;
		float bitePower = 1f, biteDelay = 0f, life;
		EnvironmentController ec;
		static int activeMolars = 0;
	}
}
