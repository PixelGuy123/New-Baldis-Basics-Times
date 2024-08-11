using BBTimes.CustomComponents.EventSpecificComponents;
using BBTimes.ModPatches;
using MTM101BaldAPI.Registers;
using System.Collections.Generic;
using UnityEngine;

namespace BBTimes.CustomContent.Events
{
	public class HologramPastEvent : RandomEvent
	{
		public override void Begin()
		{
			base.Begin();
			
			for (int i = 0; i < ec.Npcs.Count; i++)
			{
				if (ec.Npcs[i].Navigator.isActiveAndEnabled && ec.Npcs[i].GetMeta().flags.HasFlag(NPCFlags.Standard))
				{
					for (int d = 1; d <= pastLayers; d++) 
					{
						var pre = Instantiate(hologramPre);
						pre.Initialize(ec.Npcs[i].spriteRenderer[0], d * timeOffset, ec, 1f / d);
						holos.Add(pre);
					}
				}
			}

			foreach (var player in FindObjectsOfType<PlayerVisual>())
			{
				for (int d = 1; d <= pastLayers; d++)
				{
					var pre = Instantiate(hologramPre);
					pre.Initialize(player.GetComponent<SpriteRenderer>(), d * timeOffset, ec, 1f / (d * 0.95f));
					holos.Add(pre);
				}
			}
		}

		public override void End()
		{
			base.End();
			holos.ForEach(Destroy);
			holos.Clear();
		}

		readonly List<Hologram> holos = [];

		[SerializeField]
		internal Hologram hologramPre;

		[SerializeField]
		internal int pastLayers = 5;

		[SerializeField]
		internal float timeOffset = 8f;
	}
}
