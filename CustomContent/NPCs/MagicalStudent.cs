using BBTimes.CustomComponents.NpcSpecificComponents;
using System.Collections;
using UnityEngine;

namespace BBTimes.CustomContent.NPCs
{
	public class MagicalStudent : NPC
	{
		public override void Initialize()
		{
			base.Initialize();
			magic = Instantiate(magicPre);
			magic.Initialize(this);
			navigator.maxSpeed = speed;
			navigator.SetSpeed(speed);
			behaviorStateMachine.ChangeState(new MagicalStudent_Wander(this, 0f));
		}

		public void ThrowMagic(PlayerManager pm) =>
			StartCoroutine(Throw(pm.transform));
		

		IEnumerator Throw(Transform target)
		{
			navigator.maxSpeed = 0f;
			navigator.SetSpeed(0f);
			float cool = 4.5f;
			renderer.sprite = throwSprites[1];
			float shakeness = 0f;
			Vector3 pos = renderer.transform.localPosition;
			while (cool > 0f)
			{
				cool -= TimeScale * Time.deltaTime;
				shakeness += TimeScale * Time.deltaTime * 0.05f;
				renderer.transform.localPosition = pos + new Vector3(Random.Range(-shakeness, shakeness), Random.Range(-shakeness, shakeness), Random.Range(-shakeness, shakeness));
				yield return null;
			}
			renderer.transform.localPosition = pos;
			cool = 0.3f;
			renderer.sprite = throwSprites[2];
			audMan.PlaySingle(audThrow);

			magic.Throw((target.position - transform.position).normalized);

			while (cool > 0f)
			{
				cool -= TimeScale * Time.deltaTime;
				yield return null;
			}
			renderer.sprite = throwSprites[0];

			navigator.maxSpeed = speed;
			navigator.SetSpeed(speed);
			behaviorStateMachine.ChangeState(new MagicalStudent_Wander(this, Random.Range(15f, 25f)));

			yield break;
		}

		MagicObject magic;

		[SerializeField]
		internal MagicObject magicPre;

		[SerializeField]
		internal SpriteRenderer renderer;

		[SerializeField]
		internal Sprite[] throwSprites;

		[SerializeField]
		internal PropagatedAudioManager audMan;

		[SerializeField]
		internal SoundObject audThrow;

		const float speed = 15f;
	}

	internal class MagicalStudent_StateBase(MagicalStudent mgs) : NpcState(mgs)
	{
		protected MagicalStudent mgs = mgs;
	}

	internal class MagicalStudent_Wander(MagicalStudent mgs, float waitCooldown) : MagicalStudent_StateBase(mgs)
	{
		float cool = waitCooldown;
		public override void Enter()
		{
			base.Enter();
			ChangeNavigationState(new NavigationState_WanderRounds(mgs, 0));
		}
		public override void DestinationEmpty()
		{
			base.DestinationEmpty();
			ChangeNavigationState(new NavigationState_WanderRounds(mgs, 0));
		}
		public override void PlayerInSight(PlayerManager player)
		{			
			base.PlayerInSight(player);
			if (cool > 0f) return;

			if (!player.Tagged)
				mgs.behaviorStateMachine.ChangeState(new MagicalStudent_ThrowPrep(mgs, player));
		}
		public override void Update()
		{
			base.Update();
			cool -= mgs.TimeScale * Time.deltaTime;
		}
	}

	internal class MagicalStudent_ThrowPrep(MagicalStudent mgs, PlayerManager pm) : MagicalStudent_StateBase(mgs)
	{
		readonly PlayerManager player = pm;
		public override void Enter()
		{
			base.Enter();
			mgs.Navigator.FindPath(mgs.transform.position, player.transform.position);
			ChangeNavigationState(new NavigationState_TargetPosition(mgs, 63, mgs.Navigator.NextPoint));
		}

		public override void DestinationEmpty()
		{
			if (mgs.looker.PlayerInSight() && !player.Tagged)
			{
				base.DestinationEmpty();
				ChangeNavigationState(new NavigationState_DoNothing(mgs, 0));
				mgs.ThrowMagic(player);
				mgs.behaviorStateMachine.ChangeState(new MagicalStudent_StateBase(mgs)); // Who will change state now is Mgs himself
				return;
			}
			mgs.behaviorStateMachine.ChangeState(new MagicalStudent_Wander(mgs, 0f));
		}
	}
}
