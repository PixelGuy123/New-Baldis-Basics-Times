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

			walkAnim = this.GetSpriteSheet(3, 2, pixsPerUnit, "EverestWalk.png");
			spriteRenderer[0].sprite = walkAnim[0];
			idleAnim = [walkAnim[0]];
			var angryAnimSprs = this.GetSpriteSheet(4, 3, pixsPerUnit, "EverestAngryTree.png");
			angryAnim = angryAnimSprs.ExcludeNumOfSpritesFromSheet(2);
			spitAnim = [angryAnimSprs[angryAnimSprs.Length - 2]];

			audWalking = this.GetSound("christmasornament_loop.wav", "Vfx_EverettTree_TreeNoises", SoundType.Effect, Color.white);
			audDecorate = this.GetSound("EverettTreewood_decorations.wav", "Vfx_EverettTree_Decoration", SoundType.Voice, Color.white);
			audFinishDecor = this.GetSound("EverettTreewood_finishdecorating.wav", "Vfx_EverettTree_FinishDecor", SoundType.Voice, Color.white);
			audMad = this.GetSound("EverettTreewood_mad.wav", "Vfx_EverettTree_Mad_1", SoundType.Voice, Color.white);
			audMad.additionalKeys = [
				new() { key = "Vfx_EverettTree_Mad_2", time = 3.125f },
				new() { key = "Vfx_EverettTree_Mad_3", time = 7.36f }
				];
			audIdle = [this.GetSound("EverettTreewood_idle1.wav", "Vfx_EverettTree_Idle1_0", SoundType.Voice, Color.white)];
			audIdle[0].additionalKeys = [
				new() { key = "Vfx_EverettTree_Idle1_1", time = 0.469f },
				new() { key = "Vfx_EverettTree_Idle1_2", time = 1.032f },
				new() { key = "Vfx_EverettTree_Idle1_3", time = 2.044f },
				new() { key = "Vfx_EverettTree_Idle1_0", time = 5.092f },
				new() { key = "Vfx_EverettTree_Idle1_1", time = 5.501f },
				new() { key = "Vfx_EverettTree_Idle1_2", time = 5.879f },
				];

			audSpit = this.GetSound("EverettTreewoodmad_spitornament.wav", "Vfx_EverettTree_Spit", SoundType.Effect, Color.white);

			audDestroyedDecor = this.GetSound("EverettTreewood_destroydecoration.wav", "Vfx_EverettTree_AngryAtBrokenDecor_0", SoundType.Effect, Color.white);
			audDestroyedDecor.additionalKeys = [
				new() { key = "Vfx_EverettTree_AngryAtBrokenDecor_1", time = 0.81f },
				new() { key = "Vfx_EverettTree_AngryAtBrokenDecor_2", time = 2.267f },
				new() { key = "Vfx_EverettTree_AngryAtBrokenDecor_3", time = 4.718f },
				];


			var decorSprites = this.GetSpriteSheet(1, 2, 50f, "christmasDecoration.png");

			decorPre = ObjectCreationExtensions.CreateSpriteBillboard(decorSprites[0]).AddSpriteHolder(out var decorRenderer, 1.6f, LayerStorage.ignoreRaycast).
				gameObject.SetAsPrefab(true).AddComponent<ChristmasDecoration>();
			decorPre.name = "Decoration";
			decorRenderer.name = "DecorationRenderer";

			decorPre.renderer = decorRenderer;
			decorPre.sprBroken = decorSprites[1];
			decorPre.audMan = decorPre.gameObject.CreatePropagatedAudioManager(55f, 75f);
			decorPre.audBreak = this.GetSound("wood_ultimateBreak.wav", "Vfx_EverettTree_DecorBreak", SoundType.Effect, Color.white);
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
			snowPre.audMan = snowPre.gameObject.CreatePropagatedAudioManager(35f, 60f);
			snowPre.audWoosh = this.GetSound("christmasBallWoosh.wav", "Sfx_Ben_Gum_Whoosh", SoundType.Effect, Color.white);
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
		public string TexturePath => this.GenerateDataPath("npcs", "Textures");
		public string SoundPath => this.GenerateDataPath("npcs", "Audios");
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

		public void Walk(bool walk)
		{
			float speed = walk ? 18f : 0f;
			navigator.maxSpeed = speed;
			navigator.SetSpeed(speed);

			animComp.ResetFrame(true);
			animComp.animation = walk ? walkAnim : idleAnim;
			animComp.speed = 14f;

			if (walk)
			{
				audMovingMan.maintainLoop = true;
				audMovingMan.SetLoop(true);
				audMovingMan.QueueAudio(audWalking);
				audMovingMan.pitchModifier = 1f;
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

			navigator.maxSpeed = 75f;
			navigator.SetSpeed(75f);

			animComp.speed = 35f;
		}

		public void QueueDecor(ChristmasDecoration decor) => decorsToGo.Enqueue(decor);

		public void LectureEntity(Entity e) =>
			StartCoroutine(PunishEntity(e));
		

		IEnumerator PunishEntity(Entity e)
		{
			behaviorStateMachine.ChangeState(new EverettTreewood_AlertedByDecor(this, null));
			behaviorStateMachine.ChangeNavigationState(new NavigationState_DoNothing(this, 0));
			Walk(false);

			audMan.FlushQueue(true);
			audMan.QueueAudio(audMad);

			if (e.CompareTag("Player"))
				ec.AddTimeScale(timeScale);

			e.ExternalActivity.moveMods.Add(moveMod);

			animComp.animation = angryAnim;
			animComp.ResetFrame(true);
			animComp.speed = 16f;
			animComp.StopLastFrameMode();

			while (audMan.QueuedAudioIsPlaying || !animComp.Paused)
				yield return null;

			animComp.animation = spitAnim;
			animComp.ResetFrame(true);

			ec.RemoveTimeScale(timeScale);
			if (e)
			{
				float delay = 0f;
				e.ExternalActivity.moveMods.Remove(moveMod);
				for (int i = 0; i < shootPerTarget; i++)
				{
					while (delay > 0f)
					{
						delay -= TimeScale * Time.deltaTime;
						yield return null;
					}

					audMan.PlaySingle(audSpit);
					Instantiate(snowPre).Spawn(gameObject, transform.position, (e.transform.position - transform.position).normalized, shootForce, ec);

					delay = shootDelay;
				}
			}


			behaviorStateMachine.ChangeState(new EverettTreewood_Wander(this));
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
		internal Sprite[] idleAnim, walkAnim, spitAnim, angryAnim;

		[SerializeField]
		internal float shootDelay = 0.45f, shootForce = 65f, prepareDecorDelay = 6f, wannaBuildDecorCooldown = 20f, delayBeforeScanEveryone = 1.5f;

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
			ev.Walk(true);
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
			ev.Walk(false);
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
			ev.Walk(false);
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
