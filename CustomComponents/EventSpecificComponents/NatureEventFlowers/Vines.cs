using System.Collections.Generic;
using UnityEngine;

namespace BBTimes.CustomComponents.EventSpecificComponents.NatureEventFlowers
{
	public class Vines : Plant
	{
		protected override void TriggerEnterNPC(NPC npc)
		{
			base.TriggerEnterNPC(npc);
			Catch(npc.Navigator.Am);
		}

		protected override void TriggerEnterPlayer(PlayerManager pm)
		{
			base.TriggerEnterPlayer(pm);
			Catch(pm.Am);
		}

		protected override void TriggerExitNPC(NPC npc)
		{
			base.TriggerExitNPC(npc);
			Release(npc.Navigator.Am);
		}

		protected override void TriggerExitPlayer(PlayerManager pm)
		{
			base.TriggerExitPlayer(pm);
			Release(pm.Am);
		}

		void Catch(ActivityModifier actMod)
		{
			actMods.Add(actMod);
			actMod.moveMods.Add(moveMod);
			audMan.PlaySingle(audCatch);
			catches++;
		}
		void Release(ActivityModifier actMod)
		{
			if (actMod)
			{
				actMods.Remove(actMod);
				actMod.moveMods.Remove(moveMod);
			}

			if (catches > maxCatches)
			{
				actMods.ForEach(x => x?.moveMods.Remove(moveMod));
				Despawn(true);
			}
		}
			

		readonly List<ActivityModifier> actMods = [];

		readonly MovementModifier moveMod = new(Vector3.zero, 0.5f);

		int catches = 0;

		[SerializeField]
		internal SoundObject audCatch;

		[SerializeField]
		internal int maxCatches = 4;
	}
}
