using BBTimes.CustomComponents.NpcSpecificComponents;
using System.Collections.Generic;
using UnityEngine;

namespace BBTimes.CustomContent.NPCs
{
	public class Mopper : NPC
	{
		void Start()
		{
			home = ec.CellFromPosition(transform.position);
		}

		public override void Initialize()
		{
			base.Initialize();
			//behaviorStateMachine.ChangeState(new ZeroPrize_Wait(this, SleepingCooldown, false));
		}

		internal void StartSweeping()
		{
			audMan.PlaySingle(audStartSweep);

			navigator.maxSpeed = speed;
			navigator.SetSpeed(speed);

		}

		internal void StopSweeping()
		{
			navigator.SetSpeed(0f);
			navigator.maxSpeed = 0f;
		}

		public override void VirtualUpdate()
		{
			base.VirtualUpdate();
		}

		public override void Despawn()
		{
			base.Despawn();

		}

		internal void SpawnSlipper()
		{
			var slip = Instantiate(slipMatPre);
			slip.transform.position = ec.CellFromPosition(transform.position).FloorWorldPosition;
			slip.StartCoroutine(Extensions.GameExtensions.TimerToDestroy(slip.gameObject, ec, 15f));
		}

		internal bool IsHome => home == ec.CellFromPosition(transform.position);
		internal float ActiveCooldown => Random.Range(minActive, maxActive);
		internal float WaitCooldown => Random.Range(minWait, maxWait);
		internal Cell home;

		[SerializeField]
		internal SoundObject audSweep, audStartSweep;

		[SerializeField]
		internal Sprite activeSprite, deactiveSprite;

		[SerializeField]
		internal AudioManager audMan;

		[SerializeField]
		internal SlippingMaterial slipMatPre;

		[SerializeField]
		internal float minActive = 30f, maxActive = 50f, minWait = 40f, maxWait = 60f, speed = 80f;
	}
}
