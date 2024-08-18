using BBTimes.CustomComponents.EventSpecificComponents;
using MTM101BaldAPI.Registers;
using System.Collections.Generic;
using UnityEngine;

namespace BBTimes.CustomContent.Events
{
	public class ShufflingChaos : RandomEvent
	{
		public override void Begin()
		{
			base.Begin();
			telCooldown = 6;

			foreach (var npc in ec.Npcs)
				if (npc.Navigator.isActiveAndEnabled && npc.GetMeta().flags.HasFlag(NPCFlags.Standard))
					entities.Add(npc.Navigator.Entity);

			foreach (var player in ec.Players)
				if (player != null)
					entities.Add(player.plm.Entity);

			foreach (var item in ec.items)
				if (item.free && item.item.itemType != Items.None)
					pickups.Add(item);
		}

		void Update()
		{
			if (!active) return;

			telCooldown -= ec.EnvironmentTimeScale * Time.deltaTime;
			if (telCooldown <= 0f)
			{
				telCooldown += Random.Range(minWaitDelay, maxWaitDelay);
				for (int i = 0; i < entities.Count; i++)
				{
					if (entities[i])
					{
						var t = Instantiate(entShufPre);
						t.Initialize(entities[i], ec, Random.Range(minTeleportDelay, maxTeleportDelay), [.. entities]);
					}
					else entities.RemoveAt(i--);
				}
				for (int i = 0; i < pickups.Count; i++)
				{
					if (pickups[i].item.itemType != Items.None)
					{
						var t = Instantiate(pickShufPre);
						t.Initialize(pickups[i], ec, Random.Range(minTeleportDelay, maxTeleportDelay), [.. pickups]);
					}
					else pickups.RemoveAt(i--);
				}
			}
		}

		public override void End()
		{
			base.End();
			entities.Clear();
			pickups.Clear();
		}

		readonly List<Entity> entities = [];

		readonly List<Pickup> pickups = [];

		float telCooldown;

		[SerializeField]
		internal float minWaitDelay = 15f, maxWaitDelay = 20f, minTeleportDelay = 4f, maxTeleportDelay = 6f;

		[SerializeField]
		internal PickupShuffler pickShufPre;

		[SerializeField]
		internal EntityShuffler entShufPre;
		
	}
}
