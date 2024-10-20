using UnityEngine;

namespace BBTimes.CustomComponents.EventSpecificComponents.NatureEventFlowers
{
	public class YTPFlower : Plant
	{
		protected override void TriggerEnterNPC(NPC npc)
		{
			base.TriggerEnterNPC(npc);
			Despawn(true);
		}

		protected override void TriggerEnterPlayer(PlayerManager pm)
		{
			base.TriggerEnterPlayer(pm);
			Singleton<CoreGameManager>.Instance.AddPoints(value, pm.playerNumber, true);

			if (audPickup)
				audMan.PlaySingle(audPickup);

			Despawn(true);
		}

		[SerializeField]
		internal SoundObject audPickup;

		[SerializeField]
		internal int value;
	}
}
