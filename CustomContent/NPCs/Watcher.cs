using MTM101BaldAPI.Components;
using MTM101BaldAPI.Registers;
using PixelInternalAPI.Components;
using PixelInternalAPI.Extensions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BBTimes.CustomContent.NPCs
{
	public class Watcher : NPC
	{
		public override void Initialize()
		{
			base.Initialize();
			audMan.maintainLoop = true;
			screenAudMan.maintainLoop = true;

			behaviorStateMachine.ChangeState(new Watcher_WaitBelow(this));
		}

		public void GetAngry()
		{
			navigator.maxSpeed = 500f;
			navigator.SetSpeed(0f);
			navigator.accel = 35f;
		}

		public void Hide(bool hide)
		{
			spriteToHide.enabled = !hide;
			for (int i = 0; i < baseTrigger.Length; i++)
				baseTrigger[i].enabled = !hide;

			if (hide)
			{
				audMan.FlushQueue(true);
				return;
			}
			audMan.SetLoop(true);
			audMan.QueueAudio(audAmbience);			
		}

		public void SetFrozen(bool freeze)
		{
			if (freeze)
			{
				if (!navigator.Entity.ExternalActivity.moveMods.Contains(moveMod))
					navigator.Entity.ExternalActivity.moveMods.Add(moveMod);
			}
			else
				navigator.Entity.ExternalActivity.moveMods.Remove(moveMod);
			
		}

		public void GoToRandomSpot()
		{
			List<Cell> cells = [];
			for (int i = 0; i < ec.levelSize.x; i++)
			{
				for (int j = 0; j < ec.levelSize.z; j++)
				{
					if (!ec.cells[i, j].Null && ec.cells[i, j].room.type == RoomType.Hall && (ec.cells[i, j].shape == TileShape.Corner || ec.cells[i, j].shape == TileShape.End) && !ec.cells[i, j].HasAnyHardCoverage)
						cells.Add(ec.cells[i, j]);
				}
			}

			navigator.Entity.Teleport(cells[Random.Range(0, cells.Count)].CenterWorldPosition);
			StartCoroutine(SpawnDelay());
		}

		public void TeleportPlayer(PlayerManager pm)
		{
			pm.GetCustomCam().ReverseSlideFOVAnimation(new ValueModifier(), 115f, 4f);
			List<NPC> npcs = new(ec.Npcs);
			npcs.RemoveAll(x => x == this || !x.GetMeta().flags.HasFlag(NPCFlags.Standard) || !x.Navigator.Entity);

			if (npcs.Count != 0)
				StartCoroutine(TeleportDelay(pm, npcs[Random.Range(0, npcs.Count)]));
		}

		IEnumerator TeleportDelay(PlayerManager pm, NPC npc)
		{
			pm.plm.Entity.IgnoreEntity(npc.Navigator.Entity, true);
			Vector3 pos = npc.transform.position;
			npc.Navigator.Entity.Teleport(pm.transform.position);
			pm.plm.Entity.Teleport(pos);

			yield return null;

			pm.plm.Entity.IgnoreEntity(npc.Navigator.Entity, false);

			yield break;
		}

		IEnumerator SpawnDelay()
		{
			EntityOverrider overrider = new();
			navigator.Entity.Override(overrider);
			float normHeight = navigator.Entity.InternalHeight;
			overrider.SetHeight(0f);
			float curHeight = 0f;
			float tar = normHeight - 0.05f;
			while (true)
			{
				curHeight += (normHeight - curHeight) / 3f * TimeScale * Time.deltaTime * 15f;
				if (curHeight >= tar)
					break;
				
				overrider.SetHeight(curHeight);
				yield return null;
			}
			overrider.SetHeight(normHeight);
			overrider.Release();

			yield break;
		}

		[SerializeField]
		internal SpriteRenderer spriteToHide;

		[SerializeField]
		internal PropagatedAudioManager audMan;

		[SerializeField]
		internal AudioManager screenAudMan;

		[SerializeField]
		internal SoundObject audAmbience, audAngry, audSpot, audTeleport;

		readonly MovementModifier moveMod = new(Vector3.zero, 0f);
	}

	internal class Watcher_StateBase(Watcher w) : NpcState(w)
	{
		protected Watcher w = w;
	}

	internal class Watcher_WaitBelow(Watcher w) : Watcher_StateBase(w)
	{
		float cooldown = Random.Range(20f, 40f);

		public override void Enter()
		{
			base.Enter();
			ChangeNavigationState(new NavigationState_DoNothing(w, 0));
			w.Hide(true);
		}

		public override void Update()
		{
			base.Update();
			cooldown -= w.TimeScale * Time.deltaTime;
			if (cooldown <= 0f)
				w.behaviorStateMachine.ChangeState(new Watcher_Active(w));
		}
	}

	internal class Watcher_Active(Watcher w) : Watcher_StateBase(w)
	{
		public override void Initialize()
		{
			base.Initialize();			
			w.GoToRandomSpot();
			w.SetFrozen(true);
			w.Hide(false);
			w.screenAudMan.FlushQueue(true);
			w.screenAudMan.Pause(true);
		}

		public override void Sighted()
		{
			base.Sighted();
			if (!hasPlayed)
			{
				hasPlayed = true;

				w.screenAudMan.SetLoop(true);
				w.screenAudMan.QueueAudio(w.audSpot);
			}
			w.screenAudMan.Pause(false);
			stillInSight = true;
		}

		public override void Unsighted()
		{
			base.Unsighted();
			w.screenAudMan.Pause(true);
			mod.addend = 0;
			stillInSight = false;
		}

		public override void InPlayerSight(PlayerManager player)
		{
			base.InPlayerSight(player);
			spotStrength += w.TimeScale * Time.deltaTime * 6.5f;
			if (Time.timeScale > 0)
				mod.addend = spotStrength * (-1f + Random.value * 2f) * 2f;
			if (spotStrength > strengthLimit)
			{
				player.GetCustomCam().RemoveModifier(mod);
				w.behaviorStateMachine.ChangeState(new Watcher_Attack(w, player));
			}
		}

		public override void PlayerSighted(PlayerManager player)
		{
			base.PlayerSighted(player);
			mod.addend = 0;
			player.GetCustomCam().AddModifier(mod);
		}

		public override void PlayerLost(PlayerManager player)
		{
			base.PlayerLost(player);
			player.GetCustomCam().RemoveModifier(mod);
		}

		public override void Update()
		{
			base.Update();
			if (stillInSight) return;

			leaveCooldown -= w.TimeScale * Time.deltaTime;
			if (leaveCooldown <= 0f)
				w.behaviorStateMachine.ChangeState(new Watcher_WaitBelow(w));
		}

		bool hasPlayed = false, stillInSight = false;


		float spotStrength = 0f, leaveCooldown = Random.Range(30f, 60f);
		const float strengthLimit = 12f;
		readonly ValueModifier mod = new();

	}

	internal class Watcher_Attack(Watcher w, PlayerManager pm) : Watcher_StateBase(w)
	{
		readonly PlayerManager target = pm;
		CustomPlayerCameraComponent comp;
		NavigationState_TargetPlayer tar;

		public override void Initialize()
		{
			base.Initialize();
			comp = target.GetCustomCam();
			comp.AddModifier(mod);

			w.SetFrozen(false);
			w.GetAngry();
			w.screenAudMan.Pause(false);
			w.screenAudMan.FlushQueue(true);
			w.screenAudMan.SetLoop(true);
			w.screenAudMan.QueueAudio(w.audAngry);
			tar = new NavigationState_TargetPlayer(w, 127, target.transform.position, true);
			ChangeNavigationState(tar);
		}

		public override void Update()
		{
			base.Update();
			ChangeNavigationState(tar);
			tar.UpdatePosition(target.transform.position);
			if (Time.timeScale > 0)
				mod.addend = 25f * (-1f + Random.value * 2f);
		}

		public override void OnStateTriggerEnter(Collider other)
		{
			base.OnStateTriggerEnter(other);
			if (other.gameObject == target.gameObject)
			{
				w.screenAudMan.FlushQueue(true);
				w.screenAudMan.PlaySingle(w.audTeleport);
				w.TeleportPlayer(target);

				w.behaviorStateMachine.ChangeState(new Watcher_WaitBelow(w));
			}
		}

		public override void Exit()
		{
			base.Exit();
			comp.RemoveModifier(mod);
		}

		readonly ValueModifier mod = new();
	}
}
