using BBTimes.CustomComponents;
using BBTimes.Extensions;
using MTM101BaldAPI.Registers;
using PixelInternalAPI.Extensions;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BBTimes.CustomContent.NPCs
{
	public class SerOran : NPC, INPCPrefab, IClickable<int>
	{
		public void SetupPrefab()
		{
			gameObject.layer = LayerMask.NameToLayer("ClickableEntities");
			audMan = GetComponent<PropagatedAudioManager>();
			angryAudMan = gameObject.CreatePropagatedAudioManager(audMan.minDistance + 25f, audMan.maxDistance + 25f);
			audWhiteNoise = this.GetSound("whiteNoise.wav", "Vfx_Oran_Sucking", SoundType.Voice, new(0.79609375f, 0.39765625f, 0f));
			audItemCall = new SoundObject[9];
			for (int i = 0; i < audItemCall.Length; i++)
				audItemCall[i] = this.GetSound($"{i + 1}item.wav", $"Vfx_Oran_Item{i + 1}", SoundType.Voice, new(0.99609375f, 0.59765625f, 0f));
			audAngryCall = [this.GetSound("angryCall1.wav", "Vfx_Oran_Angry1", SoundType.Voice, new(0.79609375f, 0.39765625f, 0f)),
			this.GetSound("angryCall2.wav", "Vfx_Oran_Angry2", SoundType.Voice, new(0.79609375f, 0.39765625f, 0f))];
			audCrunch = this.GetSound("crunch.wav", "Vfx_Oran_Crunch", SoundType.Voice, new(0.99609375f, 0.59765625f, 0f));
			audRefuseItem = this.GetSound("refuseItem.wav", "Vfx_Oran_DontWant", SoundType.Voice, new(0.99609375f, 0.59765625f, 0f));
			audRequireItem = this.GetSound("requireItem.wav", "Vfx_Oran_RequireItem", SoundType.Voice, new(0.99609375f, 0.59765625f, 0f));
			audThanks = this.GetSound("thanks1.wav", "Vfx_Oran_Thanks1", SoundType.Voice, new(0.99609375f, 0.59765625f, 0f));
			audWonder = new SoundObject[3];
			for (int i = 0; i < audWonder.Length; i++)
				audWonder[i] = this.GetSound($"wonder{i + 1}.wav", $"Vfx_Oran_Wander{i + 1}", SoundType.Voice, new(0.99609375f, 0.59765625f, 0f));
			audNoticeChase = this.GetSound("noticeChase1.wav", "Vfx_Oran_Notice1", SoundType.Voice, new(0.99609375f, 0.59765625f, 0f));
			audLetPlayerOut = this.GetSound("letPlayerOut.wav", "Vfx_Oran_Dirt", SoundType.Voice, new(0.99609375f, 0.59765625f, 0f));
			audHeavyCrunch = this.GetSound("heavyCrunch.wav", "Sfx_Misc_ChipCrunch", SoundType.Voice, new(0.99609375f, 0.59765625f, 0f));

			var sprites = this.GetSpriteSheet(8, 1, 25f, "orange.png");
			happyTalk = [sprites[0], sprites[1]];
			botheredTalk = [sprites[2], sprites[3]];
			eatYou = [sprites[4], sprites[5], sprites[6], sprites[7]];

			canvas = ObjectCreationExtensions.CreateCanvas();
			ObjectCreationExtensions.CreateImage(canvas, this.GetSprite(1f, "mouth.png"));

			canvas.transform.SetParent(transform);
			canvas.gameObject.SetActive(false);

			animComp = gameObject.AddComponent<AnimationComponent>();
			animComp.Pause(true);
			animComp.animation = happyTalk;
			animComp.renderer = spriteRenderer[0];
			spriteRenderer[0].sprite = happyTalk[0];
		}
		public void SetupPrefabPost() { }
		public string Name { get; set; }
		public string TexturePath => this.GenerateDataPath("npcs", "Textures");
		public string SoundPath => this.GenerateDataPath("npcs", "Audios");
		public NPC Npc { get; set; }
		[SerializeField] Character[] replacementNPCs; public Character[] GetReplacementNPCs() => replacementNPCs; public void SetReplacementNPCs(params Character[] chars) => replacementNPCs = chars;
		public int ReplacementWeight { get; set; }
		// --------------------------------------------------

		[SerializeField]
		internal AnimationComponent animComp;

		[SerializeField]
		internal SoundObject[] audWonder, audAngryCall, audItemCall;

		[SerializeField]
		internal SoundObject audThanks, audRequireItem, audRefuseItem, audCrunch, audNoticeChase, audLetPlayerOut, audHeavyCrunch, audWhiteNoise;

		[SerializeField]
		internal PropagatedAudioManager audMan, angryAudMan;

		[SerializeField]
		internal Sprite[] happyTalk, botheredTalk, eatYou;

		[SerializeField]
		internal Canvas canvas;

		[SerializeField]
		[Range(0f, 1f)]
		internal float wanderChance = 0.65f;

		readonly EntityOverrider overrider = new();

		public EntityOverrider Overrider => overrider;

		public override void Initialize()
		{
			base.Initialize();
			animComp.Initialize(ec);
			behaviorStateMachine.ChangeState(new Oran_Wondering(this));
		}

		public void Wander() => audMan.QueueRandomAudio(audWonder);
		public void SpotPlayer()
		{
			audMan.FlushQueue(true);
			audMan.QueueAudio(audNoticeChase);
		}

		public void UpsetMe()
		{
			requiredSlot = -1;
			animComp.speed = 10f;
			audMan.FlushQueue(true);
			audMan.QueueRandomAudio(audAngryCall);

			angryAudMan.maintainLoop = true;
			angryAudMan.SetLoop(true);
			angryAudMan.QueueAudio(audWhiteNoise);

			animComp.animation = eatYou;
			animComp.StopLastFrameMode();
			TalkingMood = false;
		}

		public void QueueSlotRequirement(int slot)
		{
			audMan.FlushQueue(true);
			audMan.QueueAudio(audRequireItem);
			audMan.QueueAudio(audItemCall[slot]);
		}

		public void EnableClickable(int waitingSlot) =>
			requiredSlot = waitingSlot;

		public void EatPlayer(PlayerManager pm)
		{
			angryAudMan.FadeOut(1.3f);
			audMan.PlaySingle(audHeavyCrunch);
			canvas.worldCamera = Singleton<CoreGameManager>.Instance.GetCamera(pm.playerNumber).canvasCam;
			canvas.gameObject.SetActive(true);
		}

		public void TakePlayerOut()
		{
			canvas.gameObject.SetActive(false);
			TalkingMood = true;
			behaviorStateMachine.ChangeState(new NpcState(this));
			navigator.maxSpeed = 0f;
			navigator.SetSpeed(0f);
			StartCoroutine(TasteLikeDirt());
		}

		public void Clicked(int player)
		{
			if (requiredSlot == -1) return;
			if (Singleton<CoreGameManager>.Instance.GetPlayer(player).itm.selectedItem == requiredSlot)
			{
				animComp.animation = happyTalk;
				Singleton<CoreGameManager>.Instance.GetPlayer(player).itm.RemoveItem(requiredSlot);
				behaviorStateMachine.ChangeState(new NpcState(this)); // Doing nothing basically
				requiredSlot = -1;
				StartCoroutine(Eating());
				return;
			}

			audMan.PlaySingle(audRefuseItem);
			animComp.animation = botheredTalk;
		}
		public void ClickableSighted(int player) { }
		public void ClickableUnsighted(int player) { }
		public bool ClickableHidden() => requiredSlot == -1;
		public bool ClickableRequiresNormalHeight() => false;

		IEnumerator Eating()
		{
			audMan.FlushQueue(true);
			audMan.QueueAudio(audCrunch);
			while (audMan.QueuedAudioIsPlaying) yield return null;

			audMan.QueueAudio(audThanks);
			behaviorStateMachine.ChangeState(new Oran_Wondering(this, 20f));

			yield break;
		}

		IEnumerator TasteLikeDirt()
		{
			animComp.animation = botheredTalk;
			audMan.PlaySingle(audLetPlayerOut);

			while (audMan.AnyAudioIsPlaying) yield return null;

			behaviorStateMachine.ChangeState(new Oran_Wondering(this, 60f));
			animComp.animation = happyTalk;

			yield break;
		}


		public override void VirtualUpdate()
		{
			base.VirtualUpdate();
			if (talkingMood)
			{
				if (audMan.AnyAudioIsPlaying)
				{
					if (animComp.Paused)
						animComp.Pause(false);
				}
				else if (!animComp.Paused)
				{
					animComp.Pause(true);
					animComp.ResetFrame();
				}
			}
		}
		bool talkingMood = true;
		public bool TalkingMood
		{
			get => talkingMood;
			set
			{
				talkingMood = value;
				animComp.ResetFrame();
				while (animComp.Paused)
					animComp.Pause(false);
				if (talkingMood)
					animComp.speed = 5;
			}
		}

		public const int slotsItCanAccept = 9;
		int requiredSlot = -1;
	}

	internal class Oran_StateBase(SerOran or) : NpcState(or)
	{
		protected SerOran or = or;
	}

	internal class Oran_Wondering(SerOran or, float cooldown = 0f) : Oran_StateBase(or)
	{
		float wanderCooldown = 5f, cooldown = cooldown;
		public override void Enter()
		{
			base.Enter();
			or.Navigator.maxSpeed = 13f;
			or.Navigator.SetSpeed(13f);
			ChangeNavigationState(new NavigationState_WanderRandom(or, 0));
		}

		public override void PlayerInSight(PlayerManager player)
		{
			base.PlayerInSight(player);
			if (cooldown <= 0f && !player.Tagged && player.itm.items.Any(x => x.GetMeta().tags.Any(x => { string n = x.ToLower(); return n == "food" || n == "drink"; }))) // player.itm.HasItem()
				or.behaviorStateMachine.ChangeState(new Oran_ChasePlayer(or, player, this));
		}

		public override void Update()
		{
			base.Update();
			if (!or.audMan.QueuedAudioIsPlaying)
				wanderCooldown -= or.TimeScale * Time.deltaTime;
			if (wanderCooldown <= 0f)
			{
				if (Random.value <= or.wanderChance)
					or.Wander();
				wanderCooldown += 5f;
			}

			if (cooldown > 0f)
				cooldown -= or.TimeScale * Time.deltaTime;
		}
	}

	internal class Oran_ChasePlayer(SerOran or, PlayerManager pm, Oran_StateBase prevState) : Oran_StateBase(or)
	{
		readonly PlayerManager pm = pm;
		readonly Oran_StateBase prevState = prevState;
		NavigationState_TargetPlayer tar;

		public override void Enter()
		{
			base.Enter();
			or.Navigator.maxSpeed = 18f;
			or.Navigator.SetSpeed(18f);
			or.SpotPlayer();
			tar = new NavigationState_TargetPlayer(or, 63, pm.transform.position);
			ChangeNavigationState(tar);
		}

		public override void DestinationEmpty()
		{
			base.DestinationEmpty();
				or.behaviorStateMachine.ChangeState(prevState);
		}

		public override void PlayerInSight(PlayerManager player)
		{
			base.PlayerInSight(player);
			if (pm == player && !player.Tagged)
			{
				tar.UpdatePosition(player.transform.position);
				if (!player.itm.HasItem())
					or.behaviorStateMachine.ChangeState(prevState);
			}
		}

		public override void OnStateTriggerEnter(Collider other)
		{
			base.OnStateTriggerEnter(other);
			if (other.gameObject == pm.gameObject && !pm.Tagged)
{
  if (pm.itm.items.Any(x => x.GetMeta().tags.Any(x => { string n = x.ToLower(); return n == "food" || n == "drink"; })))
				or.behaviorStateMachine.ChangeState(new Oran_AskForItem(or, pm));
else
or.behaviorStateMachine.ChangeState(new prevState);
}
		}

public override void Exit() {
base.Exit();
tar.priority = 0;
}
	}

	internal class Oran_AskForItem(SerOran or, PlayerManager pm) : Oran_StateBase(or)
	{
		readonly PlayerManager pm = pm;
		int chosenSlot = 0;
		Items wantedItem = Items.None;

		public override void Enter()
		{
			base.Enter();
			or.Navigator.maxSpeed = 0f;
			or.Navigator.SetSpeed(0f);
			ChooseSlot();
			or.QueueSlotRequirement(chosenSlot);
			or.EnableClickable(chosenSlot);
		}

		void ChooseSlot()
		{
			List<int> slots = [];
			int max = Mathf.Min(SerOran.slotsItCanAccept, pm.itm.maxItem);
			for (int i = 0; i <= max; i++)
				if (!pm.itm.IsSlotLocked(i) && pm.itm.items[i].itemType != Items.None && pm.itm.items[i].GetMeta().tags.Any(x => { string n = x.ToLower(); return n == "food" || n == "drink"; }))
					slots.Add(i);

			if (slots.Count == 0)
			{
				Debug.LogWarning("BBTimes: Ser Oran failed to find available items for consume");
				or.behaviorStateMachine.ChangeState(new Oran_Wondering(or));
				return;
			}

			chosenSlot = slots[Random.Range(0, chosenSlot)];
			wantedItem = pm.itm.items[chosenSlot].itemType;
		}

		public override void Update()
		{
			base.Update();
			if (pm.itm.items[chosenSlot].itemType != wantedItem || Vector3.Distance(or.transform.position, pm.transform.position) >= 22f)
				or.behaviorStateMachine.ChangeState(new Oran_Angry(or, pm));
		}
	}

	internal class Oran_Angry(SerOran or, PlayerManager pm) : Oran_StateBase(or)
	{
		readonly PlayerManager pm = pm;
		NavigationState_TargetPlayer tar;

		public override void Enter()
		{
			base.Enter();
			or.UpsetMe();
			or.Navigator.SetSpeed(0f);
			or.Navigator.maxSpeed = 36f;
			tar = new NavigationState_TargetPlayer(or, 63, pm.transform.position);
			ChangeNavigationState(tar);
		}

		public override void PlayerInSight(PlayerManager player)
		{
			base.PlayerInSight(player);
			if (pm == player)
			{
				tar.priority = 63;
				ChangeNavigationState(tar);
				tar.UpdatePosition(player.transform.position);
			}
		}

		public override void DestinationEmpty()
		{
			base.DestinationEmpty();
			if (or.behaviorStateMachine.CurrentNavigationState == tar)
			{
				tar.priority = 0;
				ChangeNavigationState(new NavigationState_WanderRandom(or, 0));
			}
		}

		public override void Hear(Vector3 position, int value)
		{
			base.Hear(position, value);
			tar.priority = 63;
			ChangeNavigationState(tar);
			tar.UpdatePosition(position);
		}

		public override void OnStateTriggerEnter(Collider other)
		{
			base.OnStateTriggerEnter(other);
			if (other.gameObject == pm.gameObject && pm.plm.Entity.Override(or.Overrider))
				or.behaviorStateMachine.ChangeState(new Oran_EatenPlayer(or, pm, 10f));
		}
		public override void Exit()
		{
			base.Exit();
			tar.priority = 0;
		}
	}

	internal class Oran_EatenPlayer(SerOran or, PlayerManager pm, float eatCooldown) : Oran_StateBase(or)
	{
		readonly PlayerManager pm = pm;
		readonly float baseHeight = pm.plm.Entity.InternalHeight;
		float eatCooldown = eatCooldown;

		public override void Enter()
		{
			base.Enter();
			ChangeNavigationState(new NavigationState_WanderRandom(or, 0));
			or.Navigator.maxSpeed = 14;
			or.Navigator.SetSpeed(14f);

			or.EatPlayer(pm);
			or.Overrider.SetHeight(baseHeight - 3f);
			or.Overrider.SetInteractionState(false);
			or.Overrider.SetFrozen(true);
		}

		public override void Update()
		{
			base.Update();
			eatCooldown -= or.TimeScale * Time.deltaTime;
			if (eatCooldown <= 0f)
				or.TakePlayerOut();

			pm.Teleport(or.transform.position + (or.Navigator.NextPoint - or.transform.position).normalized * 2f);
		}

		public override void Exit()
		{
			base.Exit();
			or.Overrider.SetHeight(baseHeight);
			or.Overrider.SetInteractionState(true);
			or.Overrider.SetFrozen(false);
			or.Overrider.Release();
		}
	}
}
