using BBTimes.Extensions;
using BBTimes.CustomComponents;
using MTM101BaldAPI.Components;
using PixelInternalAPI.Components;
using PixelInternalAPI.Extensions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BBTimes.CustomContent.NPCs
{
    public class Stunly : NPC, INPCPrefab
	{
		public void SetupPrefab()
		{
			allSprites = [.. this.GetSpriteSheet(7, 1, 35f, "stunly.png"), .. this.GetSpriteSheet(2, 1, 1f, "stunlyScreen.png"), this.GetSprite(30f, "StunningStars.png")];
			spriteRenderer[0].sprite = allSprites[0];
			allSounds = [this.GetSoundNoSub("stunly_noises.wav", SoundType.Effect),
		this.GetSound("stunly_stun.wav", "Vfx_Stunly_Stun", SoundType.Effect, Color.white),
		this.GetSound("StunlyChaseLaughter.wav", "Vfx_Stunly_Laughter", SoundType.Voice, Color.white)]; ;

			noiseMan = GetComponent<PropagatedAudioManager>();

			laughterMan = gameObject.CreatePropagatedAudioManager(75f, 100f);

			stunlyCanvas = ObjectCreationExtensions.CreateCanvas();
			stunlyCanvas.transform.SetParent(transform);
			stunlyCanvas.transform.localPosition = Vector3.zero; // I don't know if I really need this but whatever
			stunlyCanvas.name = "stunlyOverlay";

			image = ObjectCreationExtensions.CreateImage(stunlyCanvas, allSprites[7]);
			stunlyCanvas.gameObject.SetActive(false);

			var billboard = ObjectCreationExtensions.CreateSpriteBillboard(allSprites[9]);
			billboard.transform.SetParent(transform);
			billboard.gameObject.SetActive(false);
			stars = billboard.gameObject.AddComponent<StarObject>();
		}
		public void SetupPrefabPost() { }
		public string Name { get; set; } public string Category => "npcs";
		
		public NPC Npc { get; set; }
		[SerializeField] Character[] replacementNPCs; public Character[] GetReplacementNPCs() => replacementNPCs; public void SetReplacementNPCs(params Character[] chars) => replacementNPCs = chars;
		public int ReplacementWeight { get; set; }
		// --------------------------------------------------

		public override void Initialize()
		{
			base.Initialize();
			behaviorStateMachine.ChangeState(new Stunly_WanderNormal(this));
		}

		public override void VirtualUpdate()
		{
			base.VirtualUpdate();
			if (angry)
			{
				frame += 10f * TimeScale * Time.deltaTime;
				if (frame >= 4f)
					frame = 1f + (frame % 4f);

				spriteRenderer[0].sprite = allSprites[Mathf.FloorToInt(frame)];
				return;
			}

			if (shaking)
			{
				frame += 9f * TimeScale * Time.deltaTime;
				if (frame >= 7)
					frame = 4f + (frame % 7f);

				spriteRenderer[0].sprite = allSprites[Mathf.FloorToInt(frame)];
			}

		}

		public void SetAngry(bool angry)
		{
			this.angry = angry;
			if (angry)
			{
				frame = 1f;
				laughterMan.maintainLoop = true;
				laughterMan.SetLoop(true);
				laughterMan.QueueAudio(allSounds[2]);

				noiseMan.maintainLoop = true;
				noiseMan.SetLoop(true);
				noiseMan.QueueAudio(allSounds[0]);
				return;
			}
			laughterMan.FlushQueue(true);
			noiseMan.FlushQueue(true);
			spriteRenderer[0].sprite = allSprites[0];
		}

		public void GetGuilty() =>
			SetGuilt(4f, "ugliness");

		public void SetBlind(Entity subject, bool blind, bool isPlayer)
		{
			if (!subject)
				return;
			

			if (blind)
			{
				subject.ExternalActivity.moveMods.Add(moveMod);
				if (!isPlayer)
				{
					var comp = subject.GetComponent<NPCAttributesContainer>();
					if (comp)
						comp.AddLookerMod(lookerMod);
					activeStar = Instantiate(stars);
					activeStar.gameObject.SetActive(true);
					activeStar.SetTarget(subject);
				}
				else
				{
					var pm = subject.GetComponent<PlayerManager>();
					if (pm)
					{
						stunlyCanvas.gameObject.SetActive(true);
						stunlyCanvas.worldCamera = Singleton<CoreGameManager>.Instance.GetCamera(pm.playerNumber).canvasCam;
						if (stunCor != null)
							StopCoroutine(stunCor);

						stunCor = StartCoroutine(FadeInAndOutBlindness());
						affectedByStunly.Add(new(this, pm));
					}
				}
			}
			else
			{
				subject.ExternalActivity.moveMods.Remove(moveMod);
				if (!isPlayer)
				{
					var comp = subject.GetComponent<NPCAttributesContainer>();
					if (comp)
						comp.RemoveLookerMod(lookerMod);
					if (activeStar)
						Destroy(activeStar.gameObject);
				}
			}
		}

		IEnumerator FadeInAndOutBlindness()
		{
			image.sprite = allSprites[7];
			var color = image.color;
			color.a = 0f;
			while (true)
			{
				color.a += 4f * TimeScale * Time.deltaTime;
				if (color.a >= 1f)
				{
					color.a = 1f;
					break;
				}
				image.color = color;
				yield return null;
			}

			image.color = color;

			float cooldown = 2f;

			while (cooldown > 0f)
			{
				cooldown -= TimeScale * Time.deltaTime;
				yield return null;
			}

			image.sprite = allSprites[8];
			color.a = 1f;
			while (true)
			{
				color.a -= 0.07f * TimeScale * Time.deltaTime;
				if (color.a <= 0f)
				{
					color.a = 0f;
					break;
				}
				image.color = color;
				yield return null;
			}

			image.color = color;
			stunlyCanvas.gameObject.SetActive(false);
			affectedByStunly.RemoveAll(x => x.Key == this);

			yield break;
		}

		public void CancelStunEffect()
		{
			affectedByStunly.RemoveAll(x => x.Key == this);
			if (stunCor != null)
				StopCoroutine(stunCor);
			stunlyCanvas.gameObject.SetActive(false);
			cancelledEffect = true;
		}

		public override void Despawn()
		{
			stunlyState?.ForceRemoveEffect();
			if (activeStar != null)
				Destroy(activeStar);
			base.Despawn();
		}

		bool angry;

		bool shaking;

		internal bool IsShaking { get => shaking; set { if (value) frame = 4f; shaking = value; } }

		float frame = 1f;

		readonly MovementModifier moveMod = new(Vector3.zero, 0.25f, 0);

		[SerializeField]
		internal Sprite[] allSprites;

		[SerializeField]
		internal SoundObject[] allSounds;

		[SerializeField]
		internal AudioManager noiseMan;

		[SerializeField]
		internal AudioManager laughterMan;

		[SerializeField]
		public Canvas stunlyCanvas;

		[SerializeField]
		internal UnityEngine.UI.Image image;

		[SerializeField]
		internal StarObject stars;

		StarObject activeStar;

		readonly ValueModifier lookerMod = new(0);

		Coroutine stunCor;

		internal Stunly_Flee stunlyState;

		public static List<KeyValuePair<Stunly, PlayerManager>> affectedByStunly = [];

		internal bool cancelledEffect = false;

		internal const float speed = 13f;
	}

	internal class Stunly_StateBase(Stunly st) : NpcState(st)
	{
		protected Stunly stunly = st;
	}

	internal class Stunly_WaitBeforeActive(Stunly st) : Stunly_StateBase(st)
	{
		public override void Enter()
		{
			base.Enter();
			stunly.Navigator.maxSpeed = 0f;
			stunly.Navigator.SetSpeed(0f);
			stunly.IsShaking = true;
			ChangeNavigationState(new NavigationState_DoNothing(stunly, 99));
		}

		public override void Update()
		{
			base.Update();
			cooldown -= stunly.TimeScale * Time.deltaTime;
			if (cooldown < 0f)
			{
				stunly.IsShaking = false;
				stunly.behaviorStateMachine.ChangeState(new Stunly_Active(stunly));
			}

		}

		float cooldown = 5f;
	}

	internal class Stunly_WanderNormal(Stunly st) : Stunly_StateBase(st)
	{
		public override void Enter()
		{
			base.Enter();
			stunly.Navigator.maxSpeed = Stunly.speed;
			stunly.Navigator.SetSpeed(Stunly.speed);
			ChangeNavigationState(new NavigationState_WanderRandom(stunly, 0));
		}

		public override void Update()
		{
			base.Update();
			cooldown -= stunly.TimeScale * Time.deltaTime;
			if (cooldown < 0f)
				stunly.behaviorStateMachine.ChangeState(new Stunly_WaitBeforeActive(stunly));
			
		}

		float cooldown = 20f;
	}

	internal class Stunly_Active(Stunly st) : Stunly_StateBase(st)
	{
		public override void Enter()
		{
			base.Enter();
			targetState = new NavigationState_TargetPlayer(stunly, 63, Vector3.zero); // Doing what principal code does, seems more optimized
			stunly.SetAngry(true);
			stunly.Navigator.maxSpeed = Stunly.speed + 3;
			stunly.Navigator.SetSpeed(Stunly.speed + 3);
			ChangeNavigationState(new NavigationState_WanderRandom(stunly, 0));
		}

		public override void PlayerInSight(PlayerManager player)
		{
			base.PlayerInSight(player);
			stunly.Navigator.maxSpeed = maxSpeed;
			stunly.Navigator.SetSpeed(maxSpeed);
			ChangeNavigationState(targetState);
			targetState.UpdatePosition(player.transform.position);
		}

		public override void PlayerLost(PlayerManager player)
		{
			base.PlayerLost(player);
			Directions.ReverseList(stunly.Navigator.currentDirs);
			stunly.Navigator.maxSpeed = Stunly.speed + 3;
			stunly.Navigator.SetSpeed(Stunly.speed + 3);
			ChangeNavigationState(new NavigationState_WanderRandom(stunly, 0));
		}

		public override void OnStateTriggerEnter(Collider other)
		{
			base.OnStateTriggerEnter(other);
			bool isPlayer = other.CompareTag("Player");
			if ((other.CompareTag("NPC") && stunly.looker.PlayerInSight()) || isPlayer)
			{
				var e = other.GetComponent<Entity>();
				if (e)
				{
					stunly.SetBlind(e, true, isPlayer);

					stunly.SetAngry(false);
					stunly.laughterMan.PlaySingle(stunly.allSounds[1]);
					stunly.behaviorStateMachine.ChangeState(new Stunly_Flee(stunly, e, isPlayer));
					stunly.GetGuilty();
				}
			}
		}

		NavigationState_TargetPlayer targetState;

		const float maxSpeed = 22f;
	}

	internal class Stunly_Flee(Stunly st, Entity fleeSubject, bool npcOrPlayer) : Stunly_StateBase(st) // npc = false, player = true
	{
		readonly bool wasPlayer = npcOrPlayer;
		readonly Entity subject = fleeSubject;
		readonly DijkstraMap map = new(st.ec, PathType.Nav, fleeSubject.transform);

		public override void Enter()
		{
			base.Enter();
			stunly.Navigator.maxSpeed = speed;
			stunly.Navigator.SetSpeed(speed);
			stunly.stunlyState = this;
			map.Activate();
			map.QueueUpdate(); // Omg there's these methods
			ChangeNavigationState(new NavigationState_WanderFlee(stunly, 63, map));
		}

		public override void Update() // Notes: make subjects blind, make the star effect above npcs
		{
			base.Update();
			if (!removedStun)
			{
				stuncooldown -= stunly.TimeScale * Time.deltaTime;
				if (stuncooldown < 0f || stunly.cancelledEffect)
				{
					ForceRemoveEffect();
				}
			}

			cooldown -= stunly.TimeScale * Time.deltaTime;
			if (cooldown < 0f)
				stunly.behaviorStateMachine.ChangeState(new Stunly_WanderNormal(stunly));

		}

		public void ForceRemoveEffect()
		{
			stunly.SetBlind(subject, false, wasPlayer);
			removedStun = true;
			stunly.cancelledEffect = false;
		}

		public override void Exit()
		{
			base.Exit();
			map.Deactivate();
			stunly.stunlyState = null;
		}


		float stuncooldown = 15f, cooldown = 25f;

		bool removedStun = false;

		const float speed = 45f;
	}
}
