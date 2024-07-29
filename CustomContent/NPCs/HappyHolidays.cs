using UnityEngine;

namespace BBTimes.CustomContent.NPCs
{
	public class HappyHolidays : NPC//, IItemAcceptor
	{
		public override void Initialize()
		{
			base.Initialize();
			audMan = GetComponent<AudioManager>();
			behaviorStateMachine.ChangeState(new HappyHolidays_Wondering(this));
		}

		//public void InsertItem(PlayerManager pm, EnvironmentController ec)
		//{
		//	audMan.PlaySingle(GetComponent<HappyHolidaysCustomData>().soundObjects[0]); // First sound object is the merry christmas one
		//}

		//public bool ItemFits(Items item) => item == Items.Scissors;

		internal void GivePlayerItem(PlayerManager pm)
		{
			pm.itm.AddItem(objects[Random.Range(0, objects.Length)]);
			behaviorStateMachine.ChangeState(new HappyHolidays_WaitToRespawn(this));
			audMan.PlaySingle(audHappyHolidays);
		}

		[SerializeField]
		internal ItemObject[] objects = [];

		[SerializeField]
		internal SoundObject audHappyHolidays;

		private AudioManager audMan;
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
			hh.Navigator.speed = 0;
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
