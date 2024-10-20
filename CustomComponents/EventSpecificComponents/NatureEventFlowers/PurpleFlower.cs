using UnityEngine;

namespace BBTimes.CustomComponents.EventSpecificComponents.NatureEventFlowers
{
	public class PurpleFlower : Plant
	{
		protected override void TriggerEnterNPC(NPC npc)
		{
			base.TriggerEnterNPC(npc);
			Push(npc.Navigator.Entity);
		}

		protected override void TriggerEnterPlayer(PlayerManager pm)
		{
			base.TriggerEnterPlayer(pm);
			Push(pm.plm.Entity);
		}

		void Push(Entity e)
		{
			e?.AddForce(new((e.transform.position - transform.position).normalized, pushForce, acceleration));
			audMan.PlaySingle(audPush);
			Despawn(true);
		}

		[SerializeField]
		internal float pushForce = 45f, acceleration = -25f;

		[SerializeField]
		internal SoundObject audPush;
	}
}
