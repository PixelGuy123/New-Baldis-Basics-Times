using BBTimes.CustomComponents;
using BBTimes.Extensions;
using BBTimes.Extensions.ObjectCreationExtensions;
using BBTimes.Manager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BBTimes.CustomContent.NPCs
{
	public class CheeseMan : NPC, INPCPrefab, IClickable<int>
	{
		public void SetupPrefab()
		{
			audMan = GetComponent<AudioManager>();
			this.CreateClickableLink().CopyColliderAttributes((CapsuleCollider)baseTrigger[0]);

			animComp = gameObject.AddComponent<AnimationComponent>();
			var sprs = this.GetSpriteSheet(3, 4, 55f, "cheeseMan.png");
			sprsWalking = [sprs[0], sprs[1], sprs[2], sprs[1]];
			sprsWalkHumming = [sprs[3], sprs[4], sprs[5], sprs[4]];
			sprHit = [sprs[6]];
			sprStunned = [sprs[7]];
			sprsOfferYTP = [sprs[8], sprs[9]]; // Reversed, to end at the mouth closed, not open
			sprBlankFace = [sprs[10]];
			sprAngryTalk = [sprs[11]];

			animComp.renderers = [spriteRenderer[0]];
			animComp.animation = sprsWalking;
			animComp.speed = 6.5f;
			spriteRenderer[0].sprite = sprsWalking[0];

			Color subColor = new(0.796875f, 0.796875f, 0f);

			audHitted = new SoundObject[5];
			for (int i = 0; i < 5; i++)
				audHitted[i] = this.GetSound($"CMS_Hit{i+1}.wav", $"Vfx_CheeseMan_Hit{i+1}", SoundType.Voice, subColor);

			audHumming = this.GetSound("CMS_Hum.wav", "Vfx_CheeseMan_Hum", SoundType.Voice, subColor);

			audYTPOffer = new SoundObject[4];

			for (int i = 0; i < 4; i++)
				audYTPOffer[i] = this.GetSound($"CMS_YTPs{i + 1}.wav", $"Vfx_CheeseMan_YTPs{i + 1}", SoundType.Voice, subColor);

			audStunned = this.GetSound("CMS_Head.wav", "Vfx_CheeseMan_AfterHit", SoundType.Voice, subColor);

			audSorry = new SoundObject[5];
			for (int i = 0; i < 5; i++)
				audSorry[i] = this.GetSound($"CMS_Sorry{i+1}.wav", $"Vfx_CheeseMan_Sorry{i+1}", SoundType.Voice, subColor);

			audAngry = new SoundObject[3];
			for (int i = 0; i < 3; i++)
				audAngry[i] = this.GetSound($"CMS_Watch{i+1}.wav", $"Vfx_CheeseMan_Watch{i+1}", SoundType.Voice, subColor);

			audBumpNoise = BBTimesManager.man.Get<SoundObject>("audGenericPunch");
		}
		public void SetupPrefabPost() { }
		public string Name { get; set; }
		public string Category => "npcs";
		
		public NPC Npc { get; set; }
		[SerializeField] Character[] replacementNPCs; public Character[] GetReplacementNPCs() => replacementNPCs; public void SetReplacementNPCs(params Character[] chars) => replacementNPCs = chars;
		public int ReplacementWeight { get; set; }
		// --------------------------------------------------

		[SerializeField]
		internal AudioManager audMan;

		[SerializeField]
		internal SoundObject[] audHitted, audAngry, audYTPOffer, audSorry;

		[SerializeField]
		internal SoundObject audHumming, audStunned, audBumpNoise;

		[SerializeField]
		internal int minYTPGain = 15, maxYTPGain = 30;

		[SerializeField]
		internal float minWaitForHummingCooldown = 30f, maxWaitForHummingCooldown = 40f, minStunDelay = 11f, maxStunDelay = 14f, offerToleranceBuffer = 25f,
			bumpForce = 17.8f, bumpAcceleration = -14.6f;

		[SerializeField]
		[Range(0f, 1f)]
		internal float stunSlownessFactor = 0.45f;

		[SerializeField]
		internal AnimationComponent animComp;

		[SerializeField]
		internal Sprite[] sprsWalking, sprsWalkHumming, sprHit, sprStunned, sprsOfferYTP, sprBlankFace, sprAngryTalk;

		readonly MovementModifier moveMod = new(Vector3.zero, 1f);


		public override void Initialize()
		{
			base.Initialize();
			moveMod.movementMultiplier = stunSlownessFactor;
			animComp.Initialize(ec);
			behaviorStateMachine.ChangeState(new CheeseMan_Walking(this));
		}

		public void Clicked(int player)
		{
			if (!offeringYTP) return;
			Singleton<CoreGameManager>.Instance.AddPoints(Random.Range(minYTPGain, maxYTPGain + 1), player, true);
			behaviorStateMachine.ChangeState(new CheeseMan_Walking(this));
			offeringYTP = false;
		}
		public bool ClickableHidden() => !offeringYTP;
		public bool ClickableRequiresNormalHeight() => false;
		public void ClickableSighted(int player) { }
		public void ClickableUnsighted(int player) { }
		public void Walk(bool humming)
		{
			navigator.maxSpeed = 17.5f;
			navigator.SetSpeed(17.5f);

			animComp.animation = humming ? sprsWalkHumming : sprsWalking;
			
			if (humming)
			{
				audMan.FlushQueue(true);
				audMan.maintainLoop = true;
				audMan.SetLoop(true);
				audMan.QueueAudio(audHumming);
			}
		}
		public void Bump(Entity e, bool wasPlayer)
		{
			navigator.maxSpeed = 0f;
			navigator.SetSpeed(0f);
			behaviorStateMachine.ChangeState(new NpcState(this));
			behaviorStateMachine.ChangeNavigationState(new NavigationState_DoNothing(this, 0));

			navigator.Entity.AddForce(new((transform.position - e.transform.position).normalized, bumpForce, bumpAcceleration));
			e.AddForce(new((e.transform.position - transform.position).normalized, bumpForce, bumpAcceleration));

			audMan.FlushQueue(true);
			audMan.PlaySingle(audBumpNoise);
			audMan.PlayRandomAudio(audHitted);

			animComp.animation = sprHit;
			if (sorrySequ != null)
				StopCoroutine(sorrySequ);
			sorrySequ = StartCoroutine(SorrySequence(wasPlayer ? e : null));

			StartCoroutine(StunEntity(e));
		}
		IEnumerator SorrySequence(Entity playerToOffer)
		{
			float delay = 1f;
			while (delay > 0f)
			{
				delay -= TimeScale * Time.deltaTime;
				yield return null;
			}
			while (audMan.QueuedAudioIsPlaying) yield return null;
			audMan.QueueAudio(audStunned);
			animComp.animation = sprStunned;
			while (audMan.QueuedAudioIsPlaying) yield return null;
			if (playerToOffer)
			{
				animComp.animation = sprsOfferYTP;
				audMan.QueueRandomAudio(audSorry);
				audMan.QueueRandomAudio(audYTPOffer);
				offeringYTP = true;
				bool speaking = true;
				bool angry = false;
				while (offeringYTP)
				{
					if (speaking && !audMan.QueuedAudioIsPlaying)
					{
						animComp.StopLastFrameMode();
						speaking = false;
					}
					if (Vector3.Distance(playerToOffer.transform.position, transform.position) > offerToleranceBuffer)
					{
						offeringYTP = false;
						angry = true;
						break;
					}
					yield return null;
				}
				animComp.ResetFrame(true);
				audMan.FlushQueue(true);
				if (!angry)
					yield break;
				
				animComp.animation = sprBlankFace;

				delay = 0.35f;
				while (delay > 0f)
				{
					delay -= TimeScale * Time.deltaTime;
					yield return null;
				}

				animComp.animation = sprAngryTalk;
				audMan.QueueRandomAudio(audAngry);

				while (audMan.QueuedAudioIsPlaying) yield return null;

				behaviorStateMachine.ChangeState(new CheeseMan_Walking(this));

				yield break;
			}

			audMan.QueueRandomAudio(audSorry);
			behaviorStateMachine.ChangeState(new CheeseMan_Walking(this));

			yield break;
		}
		IEnumerator StunEntity(Entity e)
		{
			affectedActMods.Add(e.ExternalActivity);
			e.ExternalActivity.moveMods.Add(moveMod);
			float time = Random.Range(minStunDelay, maxStunDelay);
			while (time > 0f)
			{
				time -= TimeScale * Time.deltaTime;
				yield return null;
			}
			if (e && e.ExternalActivity)
			{
				e.ExternalActivity.moveMods.Remove(moveMod);
				affectedActMods.Remove(e.ExternalActivity);
			}
		}
		public override void Despawn()
		{
			base.Despawn();
			while (affectedActMods.Count != 0)
			{
				affectedActMods[0]?.moveMods.Remove(moveMod);
				affectedActMods.RemoveAt(0);
			}
		}

		readonly List<ActivityModifier> affectedActMods = [];
		bool offeringYTP = false;
		public float WaitForHummingCooldown => Random.Range(minWaitForHummingCooldown, maxWaitForHummingCooldown);
		Coroutine sorrySequ;
	}

	internal class CheeseMan_StateBase(CheeseMan chm) : NpcState(chm)
	{
		protected CheeseMan chm = chm;
	}

	internal class CheeseMan_Walking(CheeseMan chm) : CheeseMan_StateBase(chm)
	{
		float humCool = chm.WaitForHummingCooldown;
		public override void Enter()
		{
			base.Enter();
			chm.Walk(false);
			ChangeNavigationState(new NavigationState_WanderRandom(chm, 0));
		}
		public override void Update()
		{
			base.Update();
			humCool -= chm.TimeScale * Time.deltaTime;
			if (humCool <= 0f)
				chm.behaviorStateMachine.ChangeState(new CheeseMan_WalkingHumming(chm));
			
		}
	}

	internal class CheeseMan_WalkingHumming(CheeseMan chm) : CheeseMan_StateBase(chm)
	{
		public override void Enter()
		{
			base.Enter();
			chm.Walk(true);
		}

		public override void OnStateTriggerEnter(Collider other)
		{
			base.OnStateTriggerEnter(other);
			if (other.isTrigger)
			{
				var isPlayer = other.CompareTag("Player");
				if (other.CompareTag("NPC") || isPlayer)
				{
					var e = other.GetComponent<Entity>();
					if (e)
					{
						if (isPlayer)
						{
							var pm = other.GetComponent<PlayerManager>();
							if (pm)
							{
								bool wasRunning = pm.plm.running && pm.plm.stamina > 0f;
								if (wasRunning)
								{
									if (pm.itm.HasItem())
										pm.itm.RemoveRandomItem();
									else
										wasRunning = false;
								}
								chm.Bump(e, wasRunning);
							}
							return;
						}
						chm.Bump(e, false);
					}
				}
			}
		}
	}

}
