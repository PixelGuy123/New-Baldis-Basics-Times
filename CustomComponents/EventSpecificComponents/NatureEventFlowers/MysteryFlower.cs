using UnityEngine;

namespace BBTimes.CustomComponents.EventSpecificComponents.NatureEventFlowers
{
	public class MysteryFlower : Plant
	{
		protected override void TriggerEnterNPC(NPC npc)
		{
			base.TriggerEnterNPC(npc);
			npc.Navigator.Entity.Teleport(ec.RandomCell(false, false, true).CenterWorldPosition);
			Teleport();
		}

		protected override void TriggerEnterPlayer(PlayerManager pm)
		{
			base.TriggerEnterPlayer(pm);
			pm.Teleport(ec.RandomCell(false, false, true).CenterWorldPosition);
			Teleport();
		}

		void Teleport()
		{
			audMan.PlaySingle(audTeleport);
			Despawn(true, false);
		}

		[SerializeField]
		internal SoundObject audTeleport;
	}
}
