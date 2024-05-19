using BBTimes.CustomComponents;
using BBTimes.Extensions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace BBTimes.CustomContent.NPCs
{
	public class RollingBot : NPC
	{
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
			while (eletricities.Count > 0)
				DestroyLastEletricity();
			
		}

		internal void AnnounceWarning() =>
			audMan.PlaySingle(audWarning);
		internal void AnnounceError() =>
			audMan.PlaySingle(audError);

		internal void SpawnEletricity(Cell cell)
		{
			var eletricity = Instantiate(eletricityPre);
			eletricity.Initialize(ec, cell, this);
			eletricities.Add(eletricity.transform);
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

		[SerializeField]
		internal SoundObject audError, audWarning;

		[SerializeField]
		internal PropagatedAudioManager audMan;

		[SerializeField]
		internal Eletricity eletricityPre;

		readonly List<Transform> eletricities = [];

		internal int EletricitiesCreated => eletricities.Count;
	}

	internal class RollingBot_StateBase(RollingBot bot) : NpcState(bot)
	{
		protected RollingBot bot = bot;
	}

	internal class RollingBot_Wandering(RollingBot bot) : RollingBot_StateBase(bot)
	{
		float errorCooldown = Random.Range(25f, 40f);
		bool errorAnnounced = false;

		public override void Enter()
		{
			base.Enter();
			ChangeNavigationState(new NavigationState_WanderRandom(bot, 0));
		}

		public override void Update()
		{
			base.Update();
			errorCooldown -= bot.TimeScale * Time.deltaTime;
			if (errorCooldown <= 10f)
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

		float cooldown = Random.Range(15f, 30f);

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
			if (c != currentCell)
			{
				currentCell = c;
				bot.SpawnEletricity(c);
				if (bot.EletricitiesCreated > eletricityLimit)
					bot.DestroyLastEletricity();
			}

			cooldown -= bot.TimeScale * Time.deltaTime;
			if (cooldown <= 0f)
				bot.behaviorStateMachine.ChangeState(new RollingBot_Wandering(bot));
			
		}

		public override void Exit()
		{
			base.Exit();
			bot.DestroyEletricity();
		}
	}

}
