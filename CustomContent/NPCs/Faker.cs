using BBTimes.Extensions;
using MTM101BaldAPI.Components;
using PixelInternalAPI.Extensions;
using System.Collections.Generic;
using UnityEngine;

namespace BBTimes.CustomContent.NPCs
{
	public class Faker : NPC
	{
		public override void Initialize()
		{
			base.Initialize();
			ChangeRandomState();
		}

		public override void VirtualUpdate()
		{
			base.VirtualUpdate();
			if (!audMan.QueuedAudioIsPlaying && IsActive)
				PlayRandomAudio();
			
		}

		public void PlayRandomAudio()
		{
			audMan.FlushQueue(true);
			audMan.pitchModifier = Random.Range(0.35f, 1.5f);
			audMan.QueueRandomAudio(soundsToEmit);
		}

		public void ChangeRandomState()
		{
			int rng = Random.Range(0, 3);
			behaviorStateMachine.ChangeState(new Faker_Spawn(this, rng == 0 ? new Faker_BlueVariant(this) : rng == 1 ? new Faker_RedVariant(this) : new Faker_GreenVariant_Idle(this)));
		}

		internal bool IsActive { get; set; } = false;

		[SerializeField]
		internal SoundObject[] soundsToEmit;

		[SerializeField]
		internal PropagatedAudioManager audMan;

		[SerializeField]
		internal Sprite[] forms;

		[SerializeField]
		internal SpriteRenderer renderer;
	}

	internal class Faker_StateBase(Faker f) : NpcState(f)
	{
		protected Faker f = f;
	}

	internal class Faker_ActiveState(Faker f) : Faker_StateBase(f)
	{
		float despawnCooldown = 60f;

		protected bool CanDespawn { 
			get => _canDespawn; 
			set
			{
				if (value)
				{
					_despawns++;
					_canDespawn = true;
					return;
				}
				_despawns--;
				if (_despawns <= 0)
				{
					_despawns = 0;
					_canDespawn = false;
				}
			} 
		}
		bool _canDespawn = true;
		int _despawns = 0;

		public override void Update()
		{
			base.Update();
			if (!_canDespawn) return;

			despawnCooldown -= f.TimeScale * Time.deltaTime;
			if (despawnCooldown <= 0f)
				f.ChangeRandomState();
		}
	}

	internal class Faker_Spawn(Faker f, Faker_StateBase stateToChange) : Faker_StateBase(f)
	{
		float prevHeight;
		float spawnCooldown = 5f;
		public override void Enter()
		{
			base.Enter();
			f.IsActive = false;
			f.audMan.FlushQueue(true);
			f.Navigator.Entity.Enable(false);
			f.Navigator.speed = 0;
			f.Navigator.SetSpeed(0);
			ChangeNavigationState(new NavigationState_DoNothing(f, 0));
			prevHeight = f.Navigator.Entity.Height;
			f.Navigator.Entity.SetHeight(-15);
			var cells = f.ec.mainHall.AllTilesNoGarbage(false, false);
			f.Navigator.Entity.Teleport(cells[Random.Range(0, cells.Count)].CenterWorldPosition);
		}
		public override void Update()
		{
			base.Update();
			spawnCooldown -= f.TimeScale * Time.deltaTime;
			if (spawnCooldown <= 0f)
			{
				f.IsActive = true;
				f.behaviorStateMachine.ChangeState(stateToChange);
			}
		}

		public override void Exit()
		{
			base.Exit();
			f.Navigator.Entity.Enable(true);
			f.Navigator.speed = 25;
			f.Navigator.SetSpeed(25);
			f.Navigator.Entity.SetHeight(prevHeight);
		}

		public override void InPlayerSight(PlayerManager player)
		{
			base.InPlayerSight(player);
			spawnCooldown = 5f;
		}

		public override void PlayerInSight(PlayerManager player)
		{
			base.PlayerInSight(player);
			spawnCooldown = 5f;
		}
	}

	internal class Faker_BlueVariant(Faker f) : Faker_ActiveState(f)
	{
		public override void Enter()
		{
			base.Enter();
			f.renderer.sprite = f.forms[2];
			f.Navigator.maxSpeed = 0;
			f.Navigator.SetSpeed(0);
			ChangeNavigationState(new NavigationState_DoNothing(f, 0));
		}

		public override void InPlayerSight(PlayerManager player)
		{
			base.InPlayerSight(player);
			
			if (!players.ContainsKey(player))
			{
				var val = new ValueModifier();
				players.Add(player, val);
				player.GetCustomCam().SlideFOVAnimation(val, -25f);
				player.Am.moveMods.Add(moveMod);
				CanDespawn = false;
			}
		}

		public override void PlayerLost(PlayerManager player)
		{
			base.PlayerLost(player);
			if (players.ContainsKey(player))
			{
				player.Am.moveMods.Remove(moveMod);
				player.GetCustomCam().ResetSlideFOVAnimation(players[player]);
				players.Remove(player);
			}
			if (players.Count == 0)
				CanDespawn = true;
		}

		readonly Dictionary<PlayerManager, ValueModifier> players = [];

		readonly MovementModifier moveMod = new(Vector3.zero, 0.5f);
	}

	internal class Faker_RedVariant(Faker f) : Faker_ActiveState(f)
	{
		public override void Enter()
		{
			base.Enter();
			f.Navigator.maxSpeed = 0;
			f.Navigator.SetSpeed(0);
			f.renderer.sprite = f.forms[1];
			ChangeNavigationState(new NavigationState_DoNothing(f, 0));
		}

		public override void PlayerInSight(PlayerManager player)
		{
			base.PlayerInSight(player);
			player.transform.RotateSmoothlyToNextPoint(f.transform.position, 1.2f);
		}
		public override void PlayerSighted(PlayerManager player)
		{
			base.PlayerSighted(player);
			CanDespawn = false;
		}
		public override void PlayerLost(PlayerManager player)
		{
			base.PlayerLost(player);
			CanDespawn = true;
		}
	}

	internal class Faker_GreenVariant_Idle(Faker f) : Faker_ActiveState(f)
	{
		public override void Enter()
		{
			base.Enter();
			f.Navigator.maxSpeed = 0;
			f.Navigator.SetSpeed(0);
			f.renderer.sprite = f.forms[0];
			ChangeNavigationState(new NavigationState_DoNothing(f, 0));
			CanDespawn = true;
		}
		public override void PlayerSighted(PlayerManager player)
		{
			base.PlayerSighted(player);
			f.behaviorStateMachine.ChangeState(new Faker_GreenVariant_Follow(f, player, this));
		}
	}
	internal class Faker_GreenVariant_Follow(Faker f, PlayerManager pm, Faker_GreenVariant_Idle prev) : Faker_ActiveState(f)
	{
		int sighted = 0;
		NavigationState_TargetPlayer target;
		public override void Enter()
		{
			CanDespawn = false;
			target = new(f, 64, pm.transform.position);
			ChangeNavigationState(target);
		}

		public override void DestinationEmpty()
		{
			base.DestinationEmpty();
			ChangeNavigationState(target);
		}
		public override void PlayerInSight(PlayerManager player)
		{
			base.PlayerInSight(player);
			if (player == pm)
				target.UpdatePosition(player.transform.position);
		}
		public override void InPlayerSight(PlayerManager player)
		{
			base.InPlayerSight(player);
			f.Navigator.maxSpeed = 0;
			f.Navigator.SetSpeed(0);
		}
		public override void Sighted()
		{
			base.Sighted();
			sighted++;
		}
		public override void Unsighted()
		{
			base.Unsighted();
			f.Navigator.maxSpeed = 25;
			f.Navigator.SetSpeed(25);
			sighted--;
		}
		public override void OnStateTriggerEnter(Collider other)
		{
			base.OnStateTriggerEnter(other);
			if (sighted <= 0 && other.isTrigger && other.gameObject == pm.gameObject)
			{
				pm.itm.RemoveRandomItem();
				f.PlayRandomAudio();
				f.ChangeRandomState();
			}
		}
		public override void PlayerLost(PlayerManager player)
		{
			base.PlayerLost(player);
			f.behaviorStateMachine.ChangeState(prev);
		}
	}
}
