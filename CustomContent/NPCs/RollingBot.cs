using System.Collections;
using System.Collections.Generic;
using BBTimes.CustomComponents;
using BBTimes.CustomComponents.NpcSpecificComponents;
using BBTimes.Extensions;
using BBTimes.Extensions.ObjectCreationExtensions;
using BBTimes.Manager;
using MTM101BaldAPI.Registers;
using PixelInternalAPI.Extensions;
using UnityEngine;



namespace BBTimes.CustomContent.NPCs
{
	public class RollingBot : NPC, INPCPrefab, IItemAcceptor, IClickable<int>
	{
		public void SetupPrefab()
		{
			Color capColor = new(0.7f, 0.7f, 0.7f);

			Sprite[] storedSprites = this.GetSpriteSheet(4, 4, 25f, "rollBotSheet.png");

			// npc setup
			audError = this.GetSound("roll_error.wav", "Vfx_Rollbot_Error", SoundType.Voice, capColor);
			audWarning = [
				this.GetSound("roll_warn1.wav", "Vfx_Rollbot_Warning1", SoundType.Voice, capColor),
				this.GetSound("roll_warn2.wav", "Vfx_Rollbot_Warning2", SoundType.Voice, capColor),
				this.GetSound("roll_warn3.wav", "Vfx_Rollbot_Warning3", SoundType.Voice, capColor)
				];
			audThanks = this.GetSound("roll_fix.wav", "Vfx_Rollbot_Fix", SoundType.Voice, capColor);
			audIdle = [
				this.GetSound("roll_idle1.wav", "Vfx_Rollbot_Idle1", SoundType.Voice, capColor),
				this.GetSound("roll_idle2.wav", "Vfx_Rollbot_Idle2", SoundType.Voice, capColor)
				];
			audRechargeItem = this.GetSoundNoSub("roll_RechargeItem.wav", SoundType.Effect);
			audMan = GetComponent<PropagatedAudioManager>();

			gameObject.CreatePropagatedAudioManager(10f, 115f)
				.AddStartingAudiosToAudioManager(true,
				this.GetSound("motor.wav", "Sfx_1PR_Motor", SoundType.Effect, capColor));

			spriteRenderer[0].CreateAnimatedSpriteRotator(
				GenericExtensions.CreateRotationMap(storedSprites.Length, storedSprites)
				);
			spriteRenderer[0].sprite = storedSprites[0];

			eletricityPre = BBTimesManager.man.Get<Eletricity>("EletricityPrefab");

			this.CreateClickableLink()
				.CopyColliderAttributes((CapsuleCollider)baseTrigger[0]);
		}
		public void SetupPrefabPost() { }
		public string Name { get; set; }
		public string Category => "npcs";

		public NPC Npc { get; set; }
		[SerializeField] Character[] replacementNPCs; public Character[] GetReplacementNPCs() => replacementNPCs; public void SetReplacementNPCs(params Character[] chars) => replacementNPCs = chars;
		public int ReplacementWeight { get; set; }
		// --------------------------------------------------
		public override void Initialize()
		{
			base.Initialize();
			navigator.maxSpeed = 14f;
			navigator.SetSpeed(14f);
			behaviorStateMachine.ChangeState(new RollingBot_Wandering(this));
		}

		public override void VirtualUpdate()
		{
			base.VirtualUpdate();
			transform.RotateSmoothlyToNextPoint(navigator.NextPoint, 0.7f);
		}

		public override void Despawn()
		{
			base.Despawn();
			while (eletricities.Count != 0)
				DestroyLastEletricity();
		}

		internal void AnnounceWarning()
		{
			audMan.FlushQueue(true);
			audMan.PlayRandomAudio(audWarning);
		}
		internal void AnnounceError()
		{
			audMan.FlushQueue(true);
			audMan.PlaySingle(audError);
		}

		internal void Idle()
		{
			if (audMan.QueuedAudioIsPlaying)
				return;
			if (Random.value <= idleChance)
				audMan.QueueRandomAudio(audIdle);
		}

		internal void SpawnEletricity(Cell cell)
		{
			var eletricity = Instantiate(eletricityPre);
			eletricity.Initialize(gameObject, cell.FloorWorldPosition, 1.25f, ec);
			eletricity.eletricityForce = electricityShakingForce;
			eletricities.Add(eletricity);
		}

		internal void DestroyEletricity() =>
			StartCoroutine(EletricityDestroy());


		IEnumerator EletricityDestroy()
		{
			while (eletricities.Count > 0)
			{
				DestroyLastEletricity();

				float delay = 0.5f;
				while (delay > 0f)
				{
					delay -= TimeScale * Time.deltaTime;
					yield return null;
				}
				yield return null;
			}

			yield break;
		}

		internal void DestroyLastEletricity()
		{
			Destroy(eletricities[0].gameObject);
			eletricities.RemoveAt(0);
		}

		// Item fits feature: accept items to fix bot

		public bool ItemFits(Items itm)
		{
			if (IsMalfunctioning && itemsThatFixMe.Contains(itm))
				return true;
			return false;
		}

		public void InsertItem(PlayerManager pm, EnvironmentController ec)
		{
			if (behaviorStateMachine.CurrentState is RollingBot_Error col)
			{
				col.cooldown = -1f;
				audMan.PlaySingle(audThanks);
			}
		}

		// IClickable feature: recharge items
		public void Clicked(int player)
		{
			if (IsItemRechargeable(Singleton<CoreGameManager>.Instance.GetPlayer(player).itm, true))
			{
				audMan.PlaySingle(audRechargeItem);
			}
		}

		public void ClickableSighted(int player) { }
		public void ClickableUnsighted(int player) { }

		public bool ClickableHidden() => !IsItemRechargeable(Singleton<CoreGameManager>.Instance.GetPlayer(0).itm, false);

		public bool ClickableRequiresNormalHeight() => false;

		bool IsItemRechargeable(ItemManager itm, bool rechargeIfTrue)
		{
			var item = itm.items[itm.selectedItem];
			if (item.itemType == Items.None)
				return false;
			var meta = item.GetMeta();
			// If Item has 0 meta registered, OR simply isn't multiple use, it cannot be recharged in any way
			if (meta == null || !meta.flags.HasFlag(ItemFlags.MultipleUse) || meta.value == item) // If the max value is the item itself, it can't be recharged
				return false;

			if (rechargeIfTrue)
				itm.SetItem(meta.value, itm.selectedItem); // meta.value will always store the "max value" of the item, which means it is a recharge
			return true;
		}

		[SerializeField]
		internal SoundObject audThanks, audError, audRechargeItem;

		[SerializeField]
		internal SoundObject[] audWarning, audIdle;

		[SerializeField]
		internal PropagatedAudioManager audMan;

		[SerializeField]
		internal Eletricity eletricityPre;

		[SerializeField]
		[Range(0f, 1f)]
		internal float idleChance = 0.005f;

		[SerializeField]
		internal float minErrorCooldown = 25f, maxErrorCooldown = 40f, errorThreshold = 10f,
			errorMinActiveCooldown = 15f, errorMaxActiveCooldown = 30f, electricityShakingForce = 1.25f;

		readonly List<Eletricity> eletricities = [];

		internal int EletricitiesCreated => eletricities.Count;
		public bool IsMalfunctioning => behaviorStateMachine.CurrentState is RollingBot_Error col && col.cooldown > 0f;

		readonly static HashSet<Items> itemsThatFixMe = [];

		public static void AddFixableItem(Items i) => itemsThatFixMe.Add(i);
	}

	internal class RollingBot_StateBase(RollingBot bot) : NpcState(bot)
	{
		protected RollingBot bot = bot;
	}

	internal class RollingBot_Wandering(RollingBot bot) : RollingBot_StateBase(bot)
	{
		float errorCooldown = Random.Range(bot.minErrorCooldown, bot.maxErrorCooldown);
		bool errorAnnounced = false;

		public override void Enter()
		{
			base.Enter();
			ChangeNavigationState(new NavigationState_WanderRandom(bot, 0));
		}

		public override void Update()
		{
			base.Update();
			if (!errorAnnounced)
				bot.Idle();

			errorCooldown -= bot.TimeScale * Time.deltaTime;
			if (errorCooldown <= bot.errorThreshold)
			{
				if (!errorAnnounced)
				{
					bot.AnnounceWarning();
					errorAnnounced = true;
				}
				if (errorCooldown <= 0f)
					bot.behaviorStateMachine.ChangeState(new RollingBot_Error(bot));
			}
		}
	}

	internal class RollingBot_Error(RollingBot bot) : RollingBot_StateBase(bot)
	{
		Cell currentCell = null;
		List<Cell> usedCells = [];

		internal float cooldown = Random.Range(bot.errorMinActiveCooldown, bot.errorMaxActiveCooldown);

		const int eletricityLimit = 12;
		public override void Initialize()
		{
			base.Initialize();
			bot.audMan.FlushQueue(true);
			bot.AnnounceError();
		}

		public override void Update()
		{
			base.Update();
			var c = bot.ec.CellFromPosition(bot.transform.position);
			if (c != currentCell && !usedCells.Contains(c))
			{
				currentCell = c;
				usedCells.Add(c);
				bot.SpawnEletricity(c);
				if (bot.EletricitiesCreated > eletricityLimit)
				{
					bot.DestroyLastEletricity();
					usedCells.RemoveAt(0);
				}
			}

			cooldown -= bot.TimeScale * Time.deltaTime;
			if (cooldown <= 0f)
				bot.behaviorStateMachine.ChangeState(new RollingBot_Wandering(bot));

		}

		public override void Exit()
		{
			base.Exit();
			bot.DestroyEletricity();
			usedCells = null; // clear it I guess
		}
	}

}
