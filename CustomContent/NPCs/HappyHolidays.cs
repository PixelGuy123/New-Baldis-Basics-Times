using BBTimes.CustomComponents;
using BBTimes.Extensions;
using BBTimes.Extensions.ObjectCreationExtensions;
using UnityEngine;

namespace BBTimes.CustomContent.NPCs
{
    public class HappyHolidays : NPC, INPCPrefab, IClickable<int>
	{
		public void SetupPrefab()
		{
			audMan = GetComponent<AudioManager>();
			var sprites = this.GetSpriteSheet(2, 2, 45f, "happyholidays.png");
			spriteRenderer[0].sprite = sprites[0];
			renderer = spriteRenderer[0];
			unwrapSprites = sprites;

			audHappyHolidays = this.GetSound("HappyHolidays.wav", "Vfx_HapH_MerryChristmas", SoundType.Voice, new(0.796875f, 0f, 0f));
			audUnbox = this.GetSound("unbox.wav", "Vfx_HapH_Unwrap", SoundType.Voice, new(0.796875f, 0f, 0f));

			var myCol = (CapsuleCollider)baseTrigger[0];
			var col = this.CreateClickableLink().gameObject.AddComponent<CapsuleCollider>();
			col.isTrigger = true;
			col.height = myCol.height;
			col.direction = myCol.direction;
			col.radius = myCol.radius;
		}
		public void SetupPrefabPost() =>
			objects = [.. GameExtensions.GetAllShoppingItems()];
		public string Name { get; set; } public string TexturePath => this.GenerateDataPath("npcs", "Textures");
		public string SoundPath => this.GenerateDataPath("npcs", "Audios");
		public NPC Npc { get; set; }
		[SerializeField] Character[] replacementNPCs; public Character[] GetReplacementNPCs() => replacementNPCs; public void SetReplacementNPCs(params Character[] chars) => replacementNPCs = chars;
		public int ReplacementWeight { get; set; }
		// --------------------------------------------------

		public override void Initialize()
		{
			base.Initialize();
			behaviorStateMachine.ChangeState(new HappyHolidays_Wondering(this));
		}

		internal void GivePlayerItem(PlayerManager pm)
		{
			pm.itm.AddItem(Random.value > coalChance ? objects[Random.Range(0, objects.Length)] : itmCoal);
			behaviorStateMachine.ChangeState(new HappyHolidays_WaitToRespawn(this));
			audMan.PlaySingle(audHappyHolidays);
		}

		public void Clicked(int player)
		{
			if (clickDelay > 0f) return;
			audMan.PlaySingle(audUnbox);
			if (++unwraps >= unwrapSprites.Length)
			{
				unwraps = 0;
				GivePlayerItem(Singleton<CoreGameManager>.Instance.GetPlayer(player));
				return;
			}
			renderer.sprite = unwrapSprites[unwraps];

			behaviorStateMachine.ChangeState(
				new HappyHolidays_FleeFromPlayer(this, (HappyHolidays_StateBase)behaviorStateMachine.CurrentState, Singleton<CoreGameManager>.Instance.GetPlayer(player).transform));

		}
		public void ClickableSighted(int player) { }
		public void ClickableUnsighted(int player) { }
		public bool ClickableHidden() => !navigator.Entity.enabled || clickDelay > 0f;
		public bool ClickableRequiresNormalHeight() => true;

		public void ResetSprite() => renderer.sprite = unwrapSprites[0];
		public override void VirtualUpdate()
		{
			base.VirtualUpdate();
			if (clickDelay > 0f)
				clickDelay -= TimeScale * Time.deltaTime;
		}

		[SerializeField]
		internal ItemObject[] objects = [];

		[SerializeField]
		internal SoundObject audHappyHolidays, audUnbox;

		[SerializeField]
		internal SpriteRenderer renderer;

		[SerializeField]
		internal Sprite[] unwrapSprites;

		[SerializeField]
		internal float coalChance = 0.25f;

		[SerializeField]
		internal AudioManager audMan;

		internal static ItemObject itmCoal;

		int unwraps = 0;
		float clickDelay = 2f;
	}

	internal class HappyHolidays_StateBase(HappyHolidays hh) : NpcState(hh) // A default npc state
	{
		protected HappyHolidays hh = hh;
	}

	internal class HappyHolidays_Wondering(HappyHolidays hh) : HappyHolidays_StateBase(hh)
	{
		public override void Enter()
		{
			base.Enter();
			ChangeNavigationState(new NavigationState_WanderRandom(hh, 0));
			hh.Navigator.maxSpeed = speed;
			hh.Navigator.SetSpeed(speed);
		}

		const float speed = 20f;
	}

	internal class HappyHolidays_FleeFromPlayer(HappyHolidays hh, HappyHolidays_StateBase prevState, params Transform[] runningFrom) : HappyHolidays_StateBase(hh)
	{
		float fleeCooldown = 15f;
		readonly HappyHolidays_StateBase prevState = prevState;
		readonly DijkstraMap map = new(hh.ec, PathType.Nav, runningFrom);
		public override void Enter()
		{
			base.Enter();
			hh.Navigator.maxSpeed = 30f;
			hh.Navigator.SetSpeed(30f);
			map.QueueUpdate();
			map.Activate();
			ChangeNavigationState(new NavigationState_WanderFlee(hh, 0, map));
		}

		public override void Update()
		{
			base.Update();
			fleeCooldown -= hh.TimeScale * Time.deltaTime;
			if (fleeCooldown <= 0f)
			{
				hh.behaviorStateMachine.ChangeState(prevState);
			}
		}
	}

	internal class HappyHolidays_WaitToRespawn(HappyHolidays hh) : HappyHolidays_StateBase(hh)
	{
		public override void Enter()
		{
			base.Enter();
			hh.Navigator.Entity.Enable(false);
			hh.Navigator.maxSpeed = 0;
			hh.Navigator.SetSpeed(0);
			ChangeNavigationState(new NavigationState_DoNothing(hh, 0));
			prevHeight = hh.Navigator.Entity.InternalHeight;
			hh.Navigator.Entity.SetHeight(-15);
		}

		public override void Update()
		{
			base.Update();
			cooldown -= hh.TimeScale * Time.deltaTime;
			if (cooldown < 0f)
				hh.behaviorStateMachine.ChangeState(new HappyHolidays_AboutToRespawn(hh, prevHeight));
			
		}

		public override void Exit()
		{
			base.Exit();
			var cells = hh.ec.mainHall.AllTilesNoGarbage(false, false);
			if (cells.Count > 0)
				hh.transform.position = cells[Random.Range(0, cells.Count)].CenterWorldPosition;
			
		}

		float prevHeight;
		float cooldown = 50f;
	}

	internal class HappyHolidays_AboutToRespawn(HappyHolidays hh, float height) : HappyHolidays_StateBase(hh)
	{
		public override void Update()
		{
			base.Update();
			ableOfRespawning -= hh.TimeScale * Time.deltaTime;
			if (ableOfRespawning < 0f)
			{
				hh.Navigator.Entity.Enable(true);
				hh.Navigator.Entity.SetHeight(prevHeight);
				hh.behaviorStateMachine.ChangeState(new HappyHolidays_Wondering(hh));
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

		public override void Exit()
		{
			base.Exit();
			hh.ResetSprite();
		}

		readonly float prevHeight = height;

		float ableOfRespawning = 5f;
	}
}
