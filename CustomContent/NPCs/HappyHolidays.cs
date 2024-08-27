
using BBTimes.CustomComponents;
using BBTimes.Extensions;
using UnityEngine;

namespace BBTimes.CustomContent.NPCs
{
    public class HappyHolidays : NPC, INPCPrefab
	{
		public void SetupPrefab()
		{
			spriteRenderer[0].sprite = this.GetSprite(65f, "happyholidays.png");
			audHappyHolidays = this.GetSound("HappyHolidays.wav", "Vfx_HapH_MerryChristmas", SoundType.Voice, new(0.796875f, 0f, 0f));
		}
		public void SetupPrefabPost() =>
			objects = [.. GameExtensions.GetAllShoppingItems()];
		public string Name { get; set; } public string TexturePath => this.GenerateDataPath("npcs", "Textures");
		public string SoundPath => this.GenerateDataPath("npcs", "Audios");
		public NPC Npc { get; set; }
		public Character[] ReplacementNpcs { get; set; }
		public int ReplacementWeight { get; set; }
		// --------------------------------------------------

		public override void Initialize()
		{
			base.Initialize();
			audMan = GetComponent<AudioManager>();
			behaviorStateMachine.ChangeState(new HappyHolidays_Wondering(this));
		}

		internal void GivePlayerItem(PlayerManager pm)
		{
			pm.itm.AddItem(Random.value > coalChance ? objects[Random.Range(0, objects.Length)] : itmCoal);
			behaviorStateMachine.ChangeState(new HappyHolidays_WaitToRespawn(this));
			audMan.PlaySingle(audHappyHolidays);
		}

		[SerializeField]
		internal ItemObject[] objects = [];

		[SerializeField]
		internal SoundObject audHappyHolidays;

		[SerializeField]
		internal float coalChance = 0.5f;

		private AudioManager audMan;

		internal static ItemObject itmCoal;
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

		public override void OnStateTriggerEnter(Collider other)
		{
			base.OnStateTriggerEnter(other);
			if (other.CompareTag("Player"))
				hh.GivePlayerItem(other.GetComponent<PlayerManager>());
			
		}

		const float speed = 20f;
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

		readonly float prevHeight = height;

		float ableOfRespawning = 5f;
	}
}
