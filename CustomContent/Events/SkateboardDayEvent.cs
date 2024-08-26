﻿using BBTimes.CustomComponents.EventSpecificComponents;
using BBTimes.Extensions;
using MTM101BaldAPI.Registers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BBTimes.CustomContent.Events
{
	public class SkateboardDayEvent : RandomEvent
	{
		public override void Begin()
		{
			base.Begin();
			for (int i = 0; i < ec.Npcs.Count; i++)
			{
				if (ec.Npcs[i].Navigator.isActiveAndEnabled && ec.Npcs[i].GetMeta().flags.HasFlag(NPCFlags.Standard))
				{
					var s = Instantiate(skatePre);
					boards.Add(s);

					s.OverrideNavigator(ec.Npcs[i].Navigator);
					s.Initialize(ec.Npcs[i].Navigator.Entity, ec);
				}
			}

			for (int i = 0; i < ec.Players.Length; i++)
			{
				if (ec.Players[i] != null)
				{
					var s = Instantiate(skatePre);
					boards.Add(s);

					s.Initialize(ec.Players[i].plm.Entity, ec);
				}
			}
		}

		public override void End()
		{
			base.End();
			boards.ForEach(x =>
			{
				StartCoroutine(GameExtensions.TimerToDestroy(x.gameObject, ec, Random.Range(2f, 6f)));

				Destroy(x);
			});
			boards.Clear();
		}

		readonly List<Skateboard> boards = [];

		[SerializeField]
		internal Skateboard skatePre;
	}
}
