using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BBTimes.CustomContent.NPCs
{
	public class CameraStand : NPC
	{
		public override void Initialize()
		{
			base.Initialize();
			navigator.SetSpeed(0);
			navigator.maxSpeed = 0;
			behaviorStateMachine.ChangeState(new CameraStand_WaitToRespawn(this));
		}

		public void TakePicture(PlayerManager pm)
		{
			audMan.PlaySingle(audPic);
			DisableLatestTimer();

			stunCanvas.gameObject.SetActive(true);
			stunCanvas.worldCamera = Singleton<CoreGameManager>.Instance.GetCamera(pm.playerNumber).canvasCam;
			affectedByCamStand.Add(new(this, pm));
			

			lastPlayer = pm;
			picTimer = StartCoroutine(PictureTimer(pm));
		}

		IEnumerator PictureTimer(PlayerManager pm)
		{
			Color color = image.color;
			color.a = 1f;
			image.color = color;
			pm.Am.moveMods.Add(moveMod);

			float cooldown = 2.5f;
			while (cooldown > 0f)
			{
				cooldown -= TimeScale * Time.deltaTime;
				yield return null;
			}

			
			while (true)
			{
				color.a -= 0.25f * TimeScale * Time.deltaTime;
				if (color.a <= 0f)
				{
					color.a = 0f;
					break;
				}
				image.color = color;
				yield return null;
			}
			pm.Am.moveMods.Remove(moveMod);
			image.color = color;
			DisableLatestTimer();

			yield break;
		}

		public override void Despawn()
		{
			base.Despawn();
			DisableLatestTimer();
		}

		public void DisableLatestTimer()
		{
			if (picTimer != null)
			{
				StopCoroutine(picTimer);
				if (lastPlayer) 
				{
					lastPlayer.Am.moveMods.Remove(moveMod);
					affectedByCamStand.RemoveAll(x => x.Key == this && x.Value == lastPlayer);
				}
				stunCanvas.gameObject.SetActive(false);
			}
		}

		[SerializeField]
		internal AudioManager audMan;

		[SerializeField]
		internal SoundObject audPic;

		[SerializeField]
		internal Canvas stunCanvas;

		[SerializeField]
		internal UnityEngine.UI.Image image;

		Coroutine picTimer;
		PlayerManager lastPlayer;

		public static List<KeyValuePair<CameraStand, PlayerManager>> affectedByCamStand = [];

		readonly MovementModifier moveMod = new(Vector3.zero, 0.7f);

	}

	internal class CameraStand_StateBase(CameraStand cs) : NpcState(cs)
	{
		protected CameraStand cs = cs;
	}

	internal class CameraStand_WaitToRespawn(CameraStand cs) : CameraStand_StateBase(cs)
	{
		public override void Enter()
		{
			base.Enter();
			ChangeNavigationState(new NavigationState_DoNothing(cs, 0));
			cs.Navigator.Entity.Enable(false);
			
			prevHeight = cs.Navigator.Entity.InternalHeight;
			cs.Navigator.Entity.SetHeight(-15);
		}

		public override void Update()
		{
			base.Update();
			cooldown -= cs.TimeScale * Time.deltaTime;
			if (cooldown < 0f)
				cs.behaviorStateMachine.ChangeState(new CameraStand_AboutToRespawn(cs, prevHeight));

		}

		public override void Exit()
		{
			base.Exit();
			List<Cell> cells = [];
			foreach (var room in cs.ec.rooms)
				if (room.category == RoomCategory.Class)
					cells.AddRange(room.AllEntitySafeCellsNoGarbage());
			
			if (cells.Count > 0)
				cs.transform.position = cells[Random.Range(0, cells.Count)].CenterWorldPosition;

		}

		float prevHeight;
		float cooldown = 3f;//30f;
	}

	internal class CameraStand_AboutToRespawn(CameraStand cs, float height) : CameraStand_StateBase(cs)
	{
		public override void Update()
		{
			base.Update();
			ableOfRespawning -= cs.TimeScale * Time.deltaTime;
			if (ableOfRespawning < 0f)
			{
				cs.Navigator.Entity.Enable(true);
				cs.Navigator.Entity.SetHeight(prevHeight);
				cs.behaviorStateMachine.ChangeState(new CameraStand_Active(cs));
			}
		}

		public override void InPlayerSight(PlayerManager player)
		{
			base.InPlayerSight(player);
			ableOfRespawning = 5f;
		}

		public override void PlayerInSight(PlayerManager player)
		{
			base.InPlayerSight(player);
			ableOfRespawning = 5f;
		}

		readonly float prevHeight = height;

		float ableOfRespawning = 5f;
	}

	internal class CameraStand_Active(CameraStand cs) : CameraStand_StateBase(cs)
	{
		float timeActive = 120f, sightDelay = 1f;
		public override void Update()
		{
			base.Update();
			timeActive -= cs.TimeScale * Time.deltaTime;
			if (timeActive <= 0f)
				cs.behaviorStateMachine.ChangeState(new CameraStand_WaitToRespawn(cs));
		}

		public override void Unsighted()
		{
			base.Unsighted();
			sightDelay = 1f;
		}

		public override void InPlayerSight(PlayerManager player)
		{
			base.InPlayerSight(player);
			sightDelay -= cs.TimeScale * Time.deltaTime;
			if (sightDelay <= 0f)
			{
				cs.TakePicture(player);
				cs.behaviorStateMachine.ChangeState(new CameraStand_WaitToRespawn(cs));
			}
		}
	}
}
