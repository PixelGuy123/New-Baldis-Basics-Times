using BBTimes.CustomComponents;
using BBTimes.CustomComponents.NpcSpecificComponents.EverettTreewood;
using BBTimes.Extensions;
using BBTimes.Manager;
using PixelInternalAPI.Classes;
using PixelInternalAPI.Extensions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BBTimes.CustomContent.NPCs
{
	public class EverettTreewood : NPC, INPCPrefab
	{
		public void SetupPrefab()
		{
			audMan = GetComponent<PropagatedAudioManager>();
			audMovingMan = gameObject.CreatePropagatedAudioManager(audMan.minDistance, audMan.maxDistance);
			audMovingMan.overrideSubtitleColor = true;
			audMovingMan.subtitleColor = audMan.subtitleColor;

			const float pixsPerUnit = 35f;

			walkAnim = this.GetSpriteSheet(5, 3, pixsPerUnit, "EverestWalk.png").ExcludeNumOfSpritesFromSheet(1);
			spriteRenderer[0].sprite = walkAnim[0];
			idleAnim = [walkAnim[0]];
			var angryAnimSprs = this.GetSpriteSheet(5, 3, pixsPerUnit, "AngryTree.png");
			angryAnim = angryAnimSprs.TakeAPair(0, 5);
			spitAnim = angryAnimSprs.ExcludeNumOfSpritesFromSheet(5, false).ExcludeNumOfSpritesFromSheet(1);
			angryWalkAnim = this.GetSpriteSheet(4, 1, pixsPerUnit, "EverestMadWalk.png");

			audWalking = this.GetSound("EverettTreewood_loop.wav", "Vfx_EverettTree_TreeNoises", SoundType.Effect, audMan.subtitleColor);
			audDecorate = this.GetSound("EverettTreewood_decorations.wav", "Vfx_EverettTree_Decoration", SoundType.Voice, audMan.subtitleColor);
			audFinishDecor = this.GetSound("EverettTreewood_finishdecorating.wav", "Vfx_EverettTree_FinishDecor", SoundType.Voice, audMan.subtitleColor);
			audMad = this.GetSound("EverettTreewood_mad.wav", "Vfx_EverettTree_Mad_1", SoundType.Voice, audMan.subtitleColor);
			audMad.additionalKeys = [
				new() { key = "Vfx_EverettTree_HeavyBreathing", time = 0.597f },
				new() { key = "Vfx_EverettTree_Mad_2", time = 0.975f },
				new() { key = "Vfx_EverettTree_HeavyBreathing", time = 1.587f },
				new() { key = "Vfx_EverettTree_Mad_3", time = 1.931f },
				new() { key = "Vfx_EverettTree_Mad_4", time = 2.378f },
				new() { key = "Vfx_EverettTree_Mad_5", time = 3.08f },
				new() { key = "Vfx_EverettTree_Mad_6", time = 3.886f },
				new() { key = "Vfx_EverettTree_Mad_7", time = 4.234f },
				new() { key = "Vfx_EverettTree_Mad_8", time = 4.598f },
				new() { key = "Vfx_EverettTree_Mad_9", time = 5.065f },
				new() { key = "Vfx_EverettTree_Mad_10", time = 6.061f },
				new() { key = "Vfx_EverettTree_Mad_11", time = 6.678f },
				new() { key = "Vfx_EverettTree_Mad_12", time = 7.23f }
				];
			audIdle = [
				this.GetSound("EverettTreewood_idle1.wav", "Vfx_EverettTree_Idle1_0", SoundType.Voice, audMan.subtitleColor),
				this.GetSound("EverettTreewood_idle2.wav", "Vfx_EverettTree_Idle2_0", SoundType.Voice, audMan.subtitleColor)
				];
			audIdle[0].additionalKeys = [
				new() { key = "Vfx_EverettTree_Idle1_1", time = 0.37f },
				new() { key = "Vfx_EverettTree_Idle1_2", time = 0.724f },
				new() { key = "Vfx_EverettTree_HeavyBreathing", time = 1.109f },
				new() { key = "Vfx_EverettTree_Idle1_3", time = 1.428f },
				new() { key = "Vfx_EverettTree_Idle1_4", time = 2.735f },
				new() { key = "Vfx_EverettTree_HeavyBreathing", time = 6.558f },
				new() { key = "Vfx_EverettTree_Idle1_0", time = 6.963f },
				new() { key = "Vfx_EverettTree_Idle1_1", time = 7.313f },
				new() { key = "Vfx_EverettTree_Idle1_2", time = 7.687f },
				];

			audIdle[1].additionalKeys = [
				new() { key = "Vfx_EverettTree_Idle2_1", time = 0.06f },
				new() { key = "Vfx_EverettTree_Idle2_2", time = 0.187f },
				new() { key = "Vfx_EverettTree_Idle2_0", time = 0.306f },
				new() { key = "Vfx_EverettTree_Idle2_1", time = 0.409f },
				new() { key = "Vfx_EverettTree_Idle2_3", time = 0.52f },
				new() { key = "Vfx_EverettTree_Idle2_0", time = 0.568f },
				new() { key = "Vfx_EverettTree_Idle2_1", time = 0.8f },
				new() { key = "Vfx_EverettTree_Idle2_2", time = 1.17f },
				new() { key = "Vfx_EverettTree_Idle2_0", time = 1.552f },
				new() { key = "Vfx_EverettTree_Idle2_1", time = 1.64f },
				new() { key = "Vfx_EverettTree_Idle2_2", time = 1.72f },
				new() { key = "Vfx_EverettTree_Idle2_3", time = 1.779f },
				new() { key = "Vfx_EverettTree_Idle2_4", time = 1.862f },
				new() { key = "Vfx_EverettTree_Idle2_5", time = 1.925f },
				new() { key = "Vfx_EverettTree_Idle2_0", time = 2.191f },
				new() { key = "Vfx_EverettTree_Idle2_2", time = 2.426f },
				new() { key = "Vfx_EverettTree_Idle2_3", time = 2.509f },
				new() { key = "Vfx_EverettTree_Idle2_4", time = 2.648f },
				new() { key = "Vfx_EverettTree_Idle2_5", time = 2.846f },
				new() { key = "Vfx_EverettTree_Idle2_6", time = 3.525f },
				new() { key = "Vfx_EverettTree_Idle2_7", time = 3.879f },
				new() { key = "Vfx_EverettTree_Idle2_6", time = 4.673f },
				new() { key = "Vfx_EverettTree_Idle2_7", time = 5.157f },
				new() { key = "Vfx_EverettTree_Idle2_8", time = 5.526f }
				];

			audSpit = this.GetSound("EverettTreewoodmad_spitornament.wav", "Vfx_EverettTree_Spit", SoundType.Effect, audMan.subtitleColor);

			audDestroyedDecor = this.GetSound("EverettTreewood_destroydecoration.wav", "Vfx_EverettTree_AngryAtBrokenDecor_0", SoundType.Effect, audMan.subtitleColor);
			audDestroyedDecor.additionalKeys = [
				new() { key = "Vfx_EverettTree_AngryAtBrokenDecor_1", time = 1.065f }
				];


			var decorSprites = this.GetSpriteSheet(1, 2, 50f, "christmasDecoration.png");

			decorPre = ObjectCreationExtensions.CreateSpriteBillboard(decorSprites[0]).AddSpriteHolder(out var decorRenderer, 1.6f, LayerStorage.ignoreRaycast).
				gameObject.SetAsPrefab(true).AddComponent<ChristmasDecoration>();
			decorPre.name = "Decoration";
			decorRenderer.name = "DecorationRenderer";

			decorPre.renderer = decorRenderer;
			decorPre.sprBroken = decorSprites[1];
			decorPre.audMan = decorPre.gameObject.CreatePropagatedAudioManager(55f, 75f);
			decorPre.audBreak = this.GetSound("wood_ultimateBreak.wav", "Vfx_EverettTree_DecorBreak", SoundType.Effect, audMan.subtitleColor);
			decorPre.collider = decorPre.gameObject.AddBoxCollider(Vector3.up * 5f, new(4.75f, 10f, 4.75f), true);

			animComp = gameObject.AddComponent<AnimationComponent>();
			animComp.renderers = spriteRenderer;
			animComp.speed = 14f;
			animComp.animation = walkAnim;

			snowPre = ObjectCreationExtensions.CreateSpriteBillboard(this.GetSprite(40f, "EverettTreewood_projectile.png"))
				.AddSpriteHolder(out var snowBallRenderer, 0f, LayerStorage.standardEntities)
				.gameObject.SetAsPrefab(true)
				.AddComponent<ChristmasBall>();
			snowPre.name = "ChristmasBall";
			snowBallRenderer.name = "ChristmasBallSprite";

			snowPre.entity = snowPre.gameObject.CreateEntity(1.5f, 1.5f, snowBallRenderer.transform);
			snowPre.entity.SetGrounded(false);
			snowPre.audMan = snowPre.gameObject.CreatePropagatedAudioManager(35f, 60f);
			snowPre.audWoosh = this.GetSound("christmasBallWoosh.wav", "Sfx_Ben_Gum_Whoosh", SoundType.Effect, audMan.subtitleColor);
			snowPre.audHit = BBTimesManager.man.Get<SoundObject>("audGenericPunch");
			snowPre.slowFactor = 0.48f;

			snowPre.renderer = snowBallRenderer.gameObject;

			snowPre.canvas = ObjectCreationExtensions.CreateCanvas();
			snowPre.canvas.name = "RedCanvas";
			snowPre.canvas.gameObject.SetActive(false);
			snowPre.canvas.transform.SetParent(snowPre.transform);

			ObjectCreationExtensions.CreateImage(snowPre.canvas, TextureExtensions.CreateSolidTexture(480, 360, new(1f, 0f, 0f, 0.45f))).name = "RedScreen";
		}
		public void SetupPrefabPost() { }
		public string Name { get; set; }
		public string Category => "npcs";
		
		public NPC Npc { get; set; }
		[SerializeField] Character[] replacementNPCs; public Character[] GetReplacementNPCs() => replacementNPCs; public void SetReplacementNPCs(params Character[] chars) => replacementNPCs = chars;
		public int ReplacementWeight { get; set; }
		// --------------------------------------------------
		public override void Initialize()
		{
			base.Initialize();
			animComp.Initialize(ec);
			behaviorStateMachine.ChangeState(new EverettTreewood_Wander(this));
		}

		public void LoveChristmas()
		{
			if (!audMan.QueuedAudioIsPlaying && Random.value <= idleLineChance)
				audMan.QueueRandomAudio(audIdle);
		}

		public void DestroyedDecor()
		{
			audMan.FlushQueue(true);
			audMan.QueueAudio(audDestroyedDecor);
		}

		public void PrepareDecor()
		{
			audMan.FlushQueue(true);
			audMan.QueueAudio(audDecorate);
		}

		public void FinishDecorAndSpawnIt()
		{
			audMan.QueueAudio(audFinishDecor);

			var decor = Instantiate(decorPre);
			decor.LinkToEverettTreewood(this);

			var cell = ec.CellFromPosition(transform.position);
			decor.transform.position = cell.FloorWorldPosition;

			decorsSpawned.Add(decor);
			occupiedCells.Add(cell);
		}

		public void CollectDecoration(ChristmasDecoration decor)
		{
			decorsSpawned.Remove(decor);
			occupiedCells.Remove(ec.CellFromPosition(decor.transform.position));
			Destroy(decor.gameObject);
		}

		public void Walk(bool walk, bool angryWalk)
		{
			float speed =
				walk ? 
					angryWalk ? angrySpeedWalk : walkSpeed
				: 0f;
			navigator.maxSpeed = speed;
			navigator.SetSpeed(speed);

			animComp.ResetFrame(true);
			animComp.animation =
				walk ? 
					angryWalk ? angryWalkAnim : walkAnim
				: idleAnim;
			animComp.speed = normalWalkAnimSpeed;

			if (walk)
			{
				audMovingMan.maintainLoop = true;
				audMovingMan.SetLoop(true);
				audMovingMan.QueueAudio(audWalking);
				audMovingMan.pitchModifier = angryWalk ? 1.15f : 1f;
				return;
			}
			audMovingMan.FlushQueue(true);
		}

		public void RunStraight()
		{
			animComp.ResetFrame(true);
			animComp.animation = walkAnim;

			if (!audMovingMan.QueuedAudioIsPlaying)
			{
				audMovingMan.maintainLoop = true;
				audMovingMan.SetLoop(true);
				audMovingMan.QueueAudio(audWalking);
			}
			audMovingMan.pitchModifier = 1.5f;

			navigator.maxSpeed = alertedSpeed;
			navigator.SetSpeed(alertedSpeed);

			animComp.speed = alertedAnimSpeed;
		}

		public void QueueDecor(ChristmasDecoration decor) => decorsToGo.Enqueue(decor);

		public void LectureEntity(Entity e) =>
			StartCoroutine(PunishEntity(e));
		

		IEnumerator PunishEntity(Entity e)
		{
			behaviorStateMachine.ChangeState(new EverettTreewood_AlertedByDecor(this, null));
			behaviorStateMachine.ChangeNavigationState(new NavigationState_DoNothing(this, 0));
			Walk(false, false);

			audMan.FlushQueue(true);
			audMan.QueueAudio(audMad);

			if (e.CompareTag("Player"))
				ec.AddTimeScale(timeScale);

			e.ExternalActivity.moveMods.Add(moveMod);

			animComp.animation = angryAnim;
			animComp.ResetFrame(true);
			animComp.speed = angryAnimationSpeed;
			animComp.StopLastFrameMode();

			while (audMan.QueuedAudioIsPlaying || !animComp.Paused)
				yield return null;

			animComp.animation = spitAnim;
			animComp.speed = spitAnimationSpeed;

			ec.RemoveTimeScale(timeScale);
			if (e)
			{
				e.ExternalActivity.moveMods.Remove(moveMod);
				for (int i = 0; i < shootPerTarget; i++)
				{
					animComp.ResetFrame(true);
					animComp.StopLastFrameMode();
					while (!animComp.Paused)
						yield return null;

					audMan.PlaySingle(audSpit);
					Instantiate(snowPre).Spawn(gameObject, transform.position, (e.transform.position - transform.position).normalized, shootForce, ec);
				}
			}


			behaviorStateMachine.ChangeState(new EverettTreewood_AngryWander(this));
		}
		

		public override void Despawn()
		{
			base.Despawn();
			for (int i = 0; i < decorsSpawned.Count; i++)
				if (decorsSpawned[i])
					Destroy(decorsSpawned[i].gameObject);
		}

		public override void VirtualUpdate()
		{
			base.VirtualUpdate();
			if (decorsToGo.Count != 0 && behaviorStateMachine.CurrentState is not EverettTreewood_AlertedByDecor)
				behaviorStateMachine.ChangeState(new EverettTreewood_RunTowardsDecor(this, decorsToGo.Dequeue()));
		}

		[SerializeField]
		internal PropagatedAudioManager audMan, audMovingMan;

		[SerializeField]
		internal SoundObject[] audIdle;

		[SerializeField]
		internal SoundObject audMad, audDecorate, audFinishDecor, audWalking, audSpit, audDestroyedDecor;

		[SerializeField]
		internal ChristmasDecoration decorPre;

		[SerializeField]
		internal ChristmasBall snowPre; // Literally Snowflake, but with canvas

		[SerializeField]
		internal AnimationComponent animComp;

		[SerializeField]
		internal Sprite[] idleAnim, walkAnim, angryWalkAnim, spitAnim, angryAnim;

		[SerializeField]
		internal float shootDelay = 0.45f, shootForce = 65f, prepareDecorDelay = 6f, wannaBuildDecorCooldown = 20f, delayBeforeScanEveryone = 1.5f, angryWalkDelay = 20f,
			angryAnimationSpeed = 16f, spitAnimationSpeed = 25f, normalWalkAnimSpeed = 14f, alertedAnimSpeed = 35f,
			angrySpeedWalk = 25f, walkSpeed = 18f, alertedSpeed = 75f;

		[SerializeField]
		internal int shootPerTarget = 3;

		[SerializeField]
		[Range(0f, 1f)]
		internal float idleLineChance = 0.15f;

		readonly Queue<ChristmasDecoration> decorsToGo = [];
		readonly List<ChristmasDecoration> decorsSpawned = [];
		readonly HashSet<Cell> occupiedCells = [];

		readonly MovementModifier moveMod = new(Vector3.zero, 0f);
		readonly TimeScaleModifier timeScale = new(0f, 1f, 1f);

		public bool SuitableSpotToPrepareDecoration
		{
			get
			{
				var cell = ec.CellFromPosition(transform.position);
				return !cell.open && (cell.shape == TileShapeMask.Single || cell.shape == TileShapeMask.Corner) && !occupiedCells.Contains(cell);
			}
		}
	}

	internal class EverettTreewood_StateBase(EverettTreewood ev) : NpcState(ev)
	{
		protected EverettTreewood ev = ev;
	}

	internal class EverettTreewood_AlertedByDecor(EverettTreewood ev, ChristmasDecoration decor) : EverettTreewood_StateBase(ev)
	{
		protected ChristmasDecoration decoration = decor;
	}

	internal class EverettTreewood_Wander(EverettTreewood ev) : EverettTreewood_StateBase(ev)
	{
		float cooldown = ev.wannaBuildDecorCooldown, wanderDelay = 1f;
		public override void Enter()
		{
			base.Enter();
			ev.Walk(true, false);
			ChangeNavigationState(new NavigationState_WanderRandom(ev, 0));
		}

		public override void Update()
		{
			base.Update();

			wanderDelay -= ev.TimeScale * Time.deltaTime;
			if (wanderDelay <= 0f)
			{
				ev.LoveChristmas();
				wanderDelay += 1f;
			}

			if (cooldown > 0f)
			{
				cooldown -= ev.TimeScale * Time.deltaTime;
				return;
			}

			if (ev.SuitableSpotToPrepareDecoration)
				ev.behaviorStateMachine.ChangeState(new EverettTreewood_AlignToDecorate(ev));
		}
	}

	internal class EverettTreewood_AngryWander(EverettTreewood ev) : EverettTreewood_StateBase(ev)
	{
		float cooldown = ev.angryWalkDelay;
		public override void Enter()
		{
			base.Enter();
			ev.Walk(true, true);
			ChangeNavigationState(new NavigationState_WanderRandom(ev, 0));
		}

		public override void Update()
		{
			base.Update();

			if (cooldown > 0f)
			{
				cooldown -= ev.TimeScale * Time.deltaTime;
				return;
			}

			ev.behaviorStateMachine.ChangeState(new EverettTreewood_Wander(ev));
		}
	}

	internal class EverettTreewood_AlignToDecorate(EverettTreewood ev) : EverettTreewood_StateBase(ev)
	{
		NavigationState_TargetPosition tarPos;
		public override void Enter()
		{
			base.Enter();
			tarPos = new(ev, 63, ev.ec.CellFromPosition(ev.transform.position).FloorWorldPosition);
			ChangeNavigationState(tarPos);
		}

		public override void DestinationEmpty()
		{
			base.DestinationEmpty();
			if (!ev.SuitableSpotToPrepareDecoration)
				ChangeNavigationState(tarPos);
			else
				ev.behaviorStateMachine.ChangeState(new EverettTreewood_PrepareDecoration(ev));
		}

		public override void Exit()
		{
			base.Exit();
			tarPos.priority = 0;
		}
	}

	internal class EverettTreewood_PrepareDecoration(EverettTreewood ev) : EverettTreewood_StateBase(ev)
	{
		float cooldown = ev.prepareDecorDelay;
		public override void Enter()
		{
			base.Enter();
			ev.Walk(false, false);
			ev.PrepareDecor();
			ChangeNavigationState(new NavigationState_DoNothing(ev, 0));
		}

		public override void Update()
		{
			base.Update();
			cooldown -= ev.TimeScale * Time.deltaTime;
			if (cooldown <= 0f)
			{
				ev.FinishDecorAndSpawnIt();
				ev.behaviorStateMachine.ChangeState(new EverettTreewood_Wander(ev));
			}
		}
	}

	internal class EverettTreewood_RunTowardsDecor(EverettTreewood ev, ChristmasDecoration decor) : EverettTreewood_AlertedByDecor(ev, decor)
	{
		NavigationState_TargetPosition tarPos;
		public override void Enter()
		{
			base.Enter();
			ev.DestroyedDecor();
			ev.RunStraight();
			tarPos = new(ev, 63, decoration.transform.position);
			ChangeNavigationState(tarPos);
		}

		public override void DestinationEmpty()
		{
			base.DestinationEmpty();
			if (ev.transform.position.x != decoration.transform.position.x || ev.transform.position.z != decoration.transform.position.z)
			{
				ChangeNavigationState(tarPos);
				return;
			}

			ev.behaviorStateMachine.ChangeState(new EverettTreewood_GetAngryOverSomeone(ev, decoration));
		}

		public override void Exit()
		{
			base.Exit();
			tarPos.priority = 0;
		}
	}

	internal class EverettTreewood_GetAngryOverSomeone(EverettTreewood ev, ChristmasDecoration decor) : EverettTreewood_AlertedByDecor(ev, decor)
	{
		float cooldown = ev.delayBeforeScanEveryone;
		public override void Enter()
		{
			base.Enter();
			ChangeNavigationState(new NavigationState_DoNothing(ev, 0));
			ev.audMan.FlushQueue(true);
			ev.Walk(false, false);
		}

		public override void Update()
		{
			base.Update();
			if (cooldown > 0f)
			{
				cooldown -= ev.TimeScale * Time.deltaTime;
				return;
			}

			if (ev.Blinded)
			{
				GiveUp();
				return;
			}

			Entity faultyEntity = null;
			float minDistance = ev.looker.distance * 10f;

			for (int i = 0; i < ev.players.Count; i++)
			{
				if (ev.looker.PlayerInSight(ev.players[i]))
				{
					float distance = Vector3.Distance(decoration.transform.position, ev.players[i].transform.position);
					//Debug.Log("Distance to player: " + distance);
					if (distance < minDistance)
					{
						faultyEntity = ev.players[i].plm.Entity;
						minDistance = distance;
						//Debug.Log("Got it!");
					}
				}
			}

			for (int i = 0; i < ev.ec.Npcs.Count; i++)
			{
				if (ev == ev.ec.Npcs[i] || !ev.ec.Npcs[i].Navigator.isActiveAndEnabled)
					continue;

				//Debug.Log($"Trying to raycast npc ({ev.ec.Npcs[i].transform.name}) with distance {Mathf.Min(ev.looker.distance, ev.ec.MaxRaycast)}");
				ev.looker.Raycast(ev.ec.Npcs[i].transform, Mathf.Min((ev.transform.position - ev.ec.Npcs[i].transform.position).magnitude + ev.ec.Npcs[i].Navigator.Velocity.magnitude, ev.looker.distance, ev.ec.MaxRaycast), out bool flag);
				if (!flag)
					continue;

				float distance = Vector3.Distance(decoration.transform.position, ev.ec.Npcs[i].transform.position);
				//Debug.Log($"Distance to npc ({ev.ec.Npcs[i].name}): " + distance);
				if (distance < minDistance)
				{
					faultyEntity = ev.ec.Npcs[i].Navigator.Entity;
					minDistance = distance;
					//Debug.Log("Got it!");
				}

			}

			if (!faultyEntity)
			{
				GiveUp();
				return;
			}

			// Get angry here, idk
			ev.LectureEntity(faultyEntity);
			ev.CollectDecoration(decoration);
		}

		void GiveUp()
		{
			ev.CollectDecoration(decoration);
			ev.behaviorStateMachine.ChangeState(new EverettTreewood_Wander(ev));
		}
	}
}
