using BBTimes.CustomComponents;
using BBTimes.CustomComponents.CustomDatas;
using PixelInternalAPI.Classes;
using PixelInternalAPI.Components;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BBTimes.CustomContent.NPCs
{
	public class Stunly : NPC
	{
		public override void Initialize()
		{
			base.Initialize();
			behaviorStateMachine.ChangeState(new Stunly_WanderNormal(this));
		}

		protected override void VirtualUpdate()
		{
			base.VirtualUpdate();
			if (angry)
			{
				frame += 10f * TimeScale * Time.deltaTime;
				if (frame >= 4f)
					frame = 1f + (frame % 4f);

				spriteRenderer[0].sprite = dat.storedSprites[Mathf.FloorToInt(frame)];
				return;
			}

			if (shaking)
			{
				frame += 9f * TimeScale * Time.deltaTime;
				if (frame >= 7)
					frame = 4f + (frame % 7f);

				spriteRenderer[0].sprite = dat.storedSprites[Mathf.FloorToInt(frame)];
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
				laughterMan.QueueAudio(dat.soundObjects[2]);

				noiseMan.maintainLoop = true;
				noiseMan.SetLoop(true);
				noiseMan.QueueAudio(dat.soundObjects[0]);
				return;
			}
			laughterMan.FlushQueue(true);
			noiseMan.FlushQueue(true);
			spriteRenderer[0].sprite = dat.storedSprites[0];
		}

		public void GetGuilty() =>
			SetGuilt(4f, "ugliness");

		public void SetBlind(Entity subject, bool blind, bool isPlayer)
		{
			if (blind)
			{
				subject.ExternalActivity.moveMods.Add(moveMod);
				if (!isPlayer)
				{
					subject.GetComponent<NPCAttributesContainer>().AddLookerMod(lookerMod);
					activeStar = Instantiate(stars);
					activeStar.gameObject.SetActive(true);
					activeStar.SetTarget(subject);
				}
				else
				{
					stunlyCanvas.gameObject.SetActive(true);
					stunlyCanvas.worldCamera = Singleton<CoreGameManager>.Instance.GetCamera(subject.GetComponent<PlayerManager>().playerNumber).canvasCam;
					if (stunCor != null)
						StopCoroutine(stunCor);

					stunCor = StartCoroutine(FadeInAndOutBlindness());
					affectedByStunly.Add(this);
				}
			}
			else
			{
				subject.ExternalActivity.moveMods.Remove(moveMod);
				if (!isPlayer)
				{
					subject.GetComponent<NPCAttributesContainer>().RemoveLookerMod(lookerMod);
					Destroy(activeStar.gameObject);
				}
			}
		}

		IEnumerator FadeInAndOutBlindness()
		{
			image.sprite = dat.storedSprites[7];
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

			image.sprite = dat.storedSprites[8];
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
			affectedByStunly.Remove(this);

			yield break;
		}

		public void CancelStunEffect()
		{
			affectedByStunly.Remove(this);
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
		internal StunlyCustomData dat;

		[SerializeField]
		internal AudioManager noiseMan;

		[SerializeField]
		internal AudioManager laughterMan;

		[SerializeField]
		public Canvas stunlyCanvas;

		[SerializeField]
		internal UnityEngine.UI.Image image;

		[SerializeField]
		public StarObject stars;

		StarObject activeStar;

		readonly BaseModifier lookerMod = new(0);

		Coroutine stunCor;

		internal Stunly_Flee stunlyState;

		public static List<Stunly> affectedByStunly = [];

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
					stunly.laughterMan.PlaySingle(stunly.dat.soundObjects[1]);
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
			{
				map.Deactivate();
				stunly.behaviorStateMachine.ChangeState(new Stunly_WanderNormal(stunly));
			}

		}

		public void ForceRemoveEffect()
		{
			stunly.SetBlind(subject, false, wasPlayer);
			removedStun = true;
			stunly.cancelledEffect = false;
		}


		float stuncooldown = 15f, cooldown = 25f;

		bool removedStun = false;

		const float speed = 45f;
	}
}
