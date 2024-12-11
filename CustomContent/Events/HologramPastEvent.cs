using BBTimes.CustomComponents;
using BBTimes.CustomComponents.EventSpecificComponents;
using BBTimes.ModPatches;
using MTM101BaldAPI.Registers;
using PixelInternalAPI.Extensions;
using System.Collections.Generic;
using UnityEngine;
using MTM101BaldAPI;
using BBTimes.Extensions;


namespace BBTimes.CustomContent.Events
{
    public class HologramPastEvent : RandomEvent, IObjectPrefab
	{
		public void SetupPrefab()
		{
			eventIntro = this.GetSound("hologramEv.wav", "Event_PastHolograms0", SoundType.Voice, Color.green);
			eventIntro.additionalKeys = [new() { time = 2.168f, key = "Event_PastHolograms1" },
				new() { time = 6.584f, key = "Event_PastHolograms2" },
				new() { time = 11.379f, key = "Event_PastHolograms3" },
				];

			var rend = ObjectCreationExtensions.CreateSpriteBillboard(null);
			rend.name = "HologramRenderer";
			rend.gameObject.ConvertToPrefab(true);

			hologramPre = rend.gameObject.AddComponent<Hologram>();
			hologramPre.renderer = rend;
		}
		public void SetupPrefabPost() { }
		public string Name { get; set; } public string TexturePath => this.GenerateDataPath("events", "Textures");
		public string SoundPath => this.GenerateDataPath("events", "Audios");
		// ---------------------------------------------------

		public override void Begin()
		{
			base.Begin();
			
			for (int i = 0; i < ec.Npcs.Count; i++)
			{
				if (ec.Npcs[i].Navigator.isActiveAndEnabled && ec.Npcs[i].spriteRenderer.Length != 0 && ec.Npcs[i].spriteRenderer[0] && ec.Npcs[i].GetMeta().flags.HasFlag(NPCFlags.Standard))
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
			while (holos.Count != 0)
			{
				if (holos[0])
					Destroy(holos[0].gameObject);
				holos.RemoveAt(0);
			}
		}

		readonly List<Hologram> holos = [];

		[SerializeField]
		internal Hologram hologramPre;

		[SerializeField]
		internal int pastLayers = 7;

		[SerializeField]
		internal float timeOffset = 8f;
	}
}
