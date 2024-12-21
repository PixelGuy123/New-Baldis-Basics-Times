﻿using BBTimes.Extensions;
using BBTimes.CustomComponents;
using System.Collections.Generic;
using UnityEngine;
using PixelInternalAPI.Extensions;
using PixelInternalAPI.Classes;

namespace BBTimes.CustomContent.NPCs
{
    public class MrKreye : NPC, INPCPrefab
	{
		public void SetupPrefab()
		{
			
			audMan = GetComponent<PropagatedAudioManager>();
			audStatic = this.GetSound("active.wav", "Sfx_Crafters_Intro", SoundType.Effect, Color.white);
			audOpenChest = this.GetSound("open.wav", "Sfx_Effects_Pop", SoundType.Effect, Color.white);

			var sprs = this.GetSpriteSheet(3, 3, 36f, "kreye.png");

			spriteRenderer[0].sprite = sprs[0];

			sprWalk = sprs.ExcludeNumOfSpritesFromSheet(4).MirrorSprites();
			sprOpenEye = sprs.ExcludeNumOfSpritesFromSheet(4, false).ExcludeNumOfSpritesFromSheet(1); // Makes two new arrays, but since GC exists, this can be ignored lol
			sprChestOpen = [sprs[sprs.Length - 1]];
			

			hookPre = ObjectCreationExtensions.CreateSpriteBillboard(this.GetSprite(25f, "hook.png"))
				.AddSpriteHolder(out var hookRenderer, 0f, LayerStorage.standardEntities).gameObject.SetAsPrefab(true)
				.AddComponent<KreyeHook>();

			hookPre.name = "KreyeHook";
			hookRenderer.name = "KreyeHook_Renderer";

			var grapInstance = GenericExtensions.FindResourceObject<ITM_GrapplingHook>();

			hookPre.lineRenderer = hookRenderer.gameObject.AddComponent<LineRenderer>();
			hookPre.lineRenderer.material = new(grapInstance.lineRenderer.material); // Clone material

			hookPre.lineRenderer.material.SetColor("_TextureColor", new(0.44140625f, 0.078125f, 0.0234375f));
			hookPre.lineRenderer.widthMultiplier = 0.75f;

			hookPre.entity = hookPre.gameObject.CreateEntity(2f, 2f, hookRenderer.transform);
			hookPre.audMan = hookPre.gameObject.CreatePropagatedAudioManager(35f, 85f);
			hookPre.audGrab = this.GetSound("hookGrab.wav", "Vfx_Kreye_Grab", SoundType.Effect, Color.white);

			animComp = gameObject.AddComponent<AnimationComponent>();
			animComp.speed = 9f;
			animComp.renderers = spriteRenderer;
			animComp.animation = sprWalk;
		}
		public void SetupPrefabPost() { }
		public string Name { get; set; } public string TexturePath => this.GenerateDataPath("npcs", "Textures");
		public string SoundPath => this.GenerateDataPath("npcs", "Audios");
		public NPC Npc { get; set; }
		[SerializeField] Character[] replacementNPCs; public Character[] GetReplacementNPCs() => replacementNPCs; public void SetReplacementNPCs(params Character[] chars) => replacementNPCs = chars;
		public int ReplacementWeight { get; set; }
		// --------------------------------------------------
		
		public override void Initialize()
		{
			base.Initialize();

			spotsToGo = ec.mainHall.GetTilesOfShape(TileShapeMask.Corner | TileShapeMask.Single | TileShapeMask.End, false);

			hook = Instantiate(hookPre);
			hook.Initialize(ec, this);
			animComp.Initialize(ec);

			WanderAgain();
		}

		public void Walk(bool walk) // If walk = false, implies he's active
		{
			float speed = walk ? this.speed : 0f;
			navigator.maxSpeed = speed;
			navigator.SetSpeed(speed);

			animComp.ResetFrame(true);
			animComp.animation = walk ? sprWalk : sprOpenEye;
			if (!walk)
			{
				animComp.StopLastFrameMode();
				audMan.maintainLoop = true;
				audMan.SetLoop(true);
				audMan.QueueAudio(audStatic);
			}
		}

		public void ThrowHook(Entity target)
		{
			navigator.maxSpeed = 0;
			navigator.SetSpeed(0);

			hook.Throw(target, hookSpeed);
			audMan.FlushQueue(true);
			audMan.QueueAudio(audOpenChest);

			animComp.ResetFrame(true);
			animComp.animation = sprChestOpen;
			animComp.StopLastFrameMode();
		}

		public void SendToDetention(Entity entity)
		{
			if (entity.CompareTag("Player"))
			{
				var pm = entity.GetComponent<PlayerManager>();
				pm.ClearGuilt();
				pm.SendToDetention(detentionTime);

				WanderAgain();
				return;
			}

			// Should be a NPC then
			var n = entity.GetComponent<NPC>();

			int num = Random.Range(0, ec.offices.Count);
			entity.Teleport(ec.offices[num].RandomEntitySafeCellNoGarbage().CenterWorldPosition);
			n.SentToDetention();

			WanderAgain();
		}

		public void WanderAgain() =>
			behaviorStateMachine.ChangeState(new MrKreye_TargetSpot(this));

		public override void Despawn()
		{
			base.Despawn();
			Destroy(hook.gameObject);
		}


		KreyeHook hook;

		[SerializeField]
		internal SoundObject audStatic, audOpenChest;

		[SerializeField]
		internal PropagatedAudioManager audMan;

		[SerializeField]
		internal KreyeHook hookPre;

		[SerializeField]
		internal AnimationComponent animComp;

		[SerializeField]
		internal Sprite[] sprWalk, sprOpenEye, sprChestOpen;

		[SerializeField]
		internal float speed = 20f, hookSpeed = 46f, watchTime = 15f, detentionTime = 30f;

		Cell previousCell = null;

		List<Cell> spotsToGo;

		public Cell GetSpotToGo
		{
			get
			{
				var cell = spotsToGo[Random.Range(0, spotsToGo.Count)];

				while (cell == previousCell || spotsToGo.Count == 1)
					cell = spotsToGo[Random.Range(0, spotsToGo.Count)];

				previousCell = cell;
				return cell;
			}
		}
	}

	internal class MrKreye_StateBase(MrKreye kre) : NpcState(kre)
	{
		protected MrKreye kre = kre;
	}

	internal class MrKreye_TargetSpot(MrKreye kre) : MrKreye_StateBase(kre)
	{
		NavigationState_TargetPosition tarPos;
		readonly Cell target = kre.GetSpotToGo;
		public override void Enter()
		{
			base.Enter();
			kre.Walk(true);
			tarPos = new(kre, 16, target.FloorWorldPosition);
			ChangeNavigationState(tarPos);
		}

		public override void DestinationEmpty()
		{
			base.DestinationEmpty();
			if (kre.ec.CellFromPosition(kre.transform.position) != target)
				ChangeNavigationState(tarPos);
			else
				kre.behaviorStateMachine.ChangeState(new MrKreye_Watch(kre));
		}

		public override void Exit()
		{
			base.Exit();
			tarPos.priority = 0;
		}

	}

	internal class MrKreye_Watch(MrKreye kre) : MrKreye_StateBase(kre)
	{
		readonly Vector3 spotToStayOn = kre.transform.position;
		float watchTime = kre.watchTime;
		public override void Enter()
		{
			base.Enter();
			kre.Walk(false);
			ChangeNavigationState(new NavigationState_DoNothing(kre, 0));
		}

		public override void Update()
		{
			base.Update();
			watchTime -= kre.TimeScale * Time.deltaTime;
			if (watchTime <= 0f || kre.transform.position != spotToStayOn)
			{
				kre.behaviorStateMachine.ChangeState(new MrKreye_TargetSpot(kre));
				return;
			}

			if (kre.Blinded) return;

			for (int i = 0; i < kre.ec.Npcs.Count; i++)
			{
				if (kre != kre.ec.Npcs[i] && kre.ec.Npcs[i].Disobeying && kre.ec.Npcs[i].Navigator.isActiveAndEnabled)
				{
					if (kre.looker.RaycastNPC(kre.ec.Npcs[i]))
					{
						kre.behaviorStateMachine.ChangeState(new MrKreye_ThrowHook(kre, kre.ec.Npcs[i].Navigator.Entity));
						return;
					}
				}
			}
		}

		public override void PlayerInSight(PlayerManager player)
		{
			base.PlayerInSight(player);
			if (!player.Tagged && player.Disobeying)
				kre.behaviorStateMachine.ChangeState(new MrKreye_ThrowHook(kre, player.plm.Entity));
		}
	}

	internal class MrKreye_ThrowHook(MrKreye kre, Entity target) : MrKreye_StateBase(kre)
	{
		readonly Entity target = target;
		public override void Enter()
		{
			ChangeNavigationState(new NavigationState_DoNothing(kre, 0));
			kre.ThrowHook(target);
		}
	}
}
