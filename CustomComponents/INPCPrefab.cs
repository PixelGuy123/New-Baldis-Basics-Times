using UnityEngine;

namespace BBTimes.CustomComponents
{
	public interface INPCPrefab : IObjectPrefab
	{
		public Character[] ReplacementNpcs { get; set; }
		public int ReplacementWeight { get; set; }
		public NPC Npc { get; set; }
	}
}
