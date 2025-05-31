using System.Collections;
using System.Collections.Generic;
using BBTimes.CustomComponents;
using BBTimes.Extensions;
using BBTimes.Manager;
using BBTimes.Plugin;
using PixelInternalAPI.Extensions;
using UnityEngine;

namespace BBTimes.CustomContent.NPCs
{
	public class CameraStand : NPC, INPCPrefab
	{
		public void SetupPrefab()
		{
			var storedSprites = this.GetSpriteSheet(4, 4, 25f, "camStand.png");

			spriteRenderer[0].CreateAnimatedSpriteRotator(
				GenericExtensions.CreateRotationMap(16, storedSprites) // Normal first frame of rotation map
				);
			spriteRenderer[0].sprite = storedSprites[0];

			audMan = GetComponent<PropagatedAudioManager>();
			audPic = this.GetSoundNoSub("photo.wav", SoundType.Effect);

			var canvas = ObjectCreationExtensions.CreateCanvas();
			canvas.transform.SetParent(transform);
			canvas.transform.localPosition = Vector3.zero; // I don't know if I really need this but whatever
			canvas.name = "CameraStandOverlay";

			image = ObjectCreationExtensions.CreateImage(canvas, BBTimesManager.man.Get<Sprite>("whiteScreen"));

			stunCanvas = canvas;
			stunCanvas.gameObject.SetActive(false);

			gaugeSprite = this.GetSprite(Storage.GaugeSprite_PixelsPerUnit, "gaugeIcon.png");
		}

		public void SetupPrefabPost() { }
		public string Name { get; set; }
		public string Category => "npcs";

		public NPC Npc { get; set; }
		[SerializeField] Character[] replacementNPCs; public Character[] GetReplacementNPCs() => replacementNPCs; public void SetReplacementNPCs(params Character[] chars) => replacementNPCs = chars;
		public int ReplacementWeight { get; set; }

		// stuff above^^
		public override void Initialize()
		{
			base.Initialize();
			navigator.SetSpeed(0);
			navigator.maxSpeed = 0;
			behaviorStateMachine.ChangeState(new CameraStand_WaitToRespawn(this));
		}

		public void TakePictureOfPlayer(PlayerManager pm)
		{
			audMan.PlaySingle(audPic);
			DisableLatestTimer();

			stunCanvas.gameObject.SetActive(true);
			stunCanvas.worldCamera = Singleton<CoreGameManager>.Instance.GetCamera(pm.playerNumber).canvasCam;
			affectedByCamStand.Add(new(this, pm));


			lastPlayer = pm;
			picTimer = StartCoroutine(PictureTimer(pm));
		}

		public void TakePicture(Entity e) =>
			StartCoroutine(NormalPictureStun(e));


		IEnumerator NormalPictureStun(Entity e)
		{
			e?.ExternalActivity.moveMods.Add(moveMod);
			float cooldown = entityStunTime;
			while (cooldown > 0f)
			{
				cooldown -= TimeScale * Time.deltaTime;
				yield return null;
			}
			e?.ExternalActivity.moveMods.Remove(moveMod);
			yield return null;
		}

		IEnumerator PictureTimer(PlayerManager pm)
		{
			gauge = Singleton<CoreGameManager>.Instance.GetHud(pm.playerNumber).gaugeManager.ActivateNewGauge(gaugeSprite, playerStunDelay);

			Color color = image.color;
			pm.Am.moveMods.Add(moveMod);

			float totalDuration = playerStunDelay; // Total duration of the flash effect
			float fadeInDuration = Singleton<PlayerFileManager>.Instance.reduceFlashing ? totalDuration * 0.25f : 0f; // Duration of the fade-in effect (25% of total duration if reduceFlashing is enabled)
			float fadeOutDuration = totalDuration - fadeInDuration; // Duration of the fade-out effect

			float timer = 0f;

			if (Singleton<PlayerFileManager>.Instance.reduceFlashing)
			{
				color.a = 0f;
				image.color = color;

				while (timer < fadeInDuration)
				{
					timer += TimeScale * Time.deltaTime;
					color.a = Mathf.Clamp01(timer / fadeInDuration); // Gradually increase alpha
					image.color = color;
					yield return null;
				}
			}
			else
			{
				color.a = 1f;
				image.color = color;
			}

			timer = 0f; // Reset timer for fade-out

			while (timer < fadeOutDuration)
			{
				timer += TimeScale * Time.deltaTime;
				color.a = Mathf.Clamp01(1f - (timer / fadeOutDuration)); // Gradually decrease alpha
				gauge.SetValue(fadeOutDuration, fadeOutDuration - timer);
				image.color = color;
				yield return null;
			}

			color.a = 0f;
			image.color = color;

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
					if (gauge)
					{
						gauge.Deactivate();
						gauge = null;
					}
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

		[SerializeField]
		internal Sprite gaugeSprite;

		[SerializeField]
		internal float entityStunTime = 20f, playerStunDelay = 2.5f;

		Coroutine picTimer;
		PlayerManager lastPlayer;
		HudGauge gauge;

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
			cs.transform.RotateSmoothlyToNextPoint(player.transform.position, 0.95f);
			sightDelay -= cs.TimeScale * Time.deltaTime;
			if (sightDelay <= 0f)
			{
				if (!player.plm.Entity.Blinded)
					cs.TakePictureOfPlayer(player);

				if (!cs.Blinded)
				{
					foreach (var npc in cs.ec.Npcs)
						if (npc != cs && !npc.Blinded && npc.Navigator.isActiveAndEnabled && cs.looker.RaycastNPC(npc))
							cs.TakePicture(npc.Navigator.Entity);
				}

				cs.behaviorStateMachine.ChangeState(new CameraStand_WaitToRespawn(cs));
			}
		}
	}
}
