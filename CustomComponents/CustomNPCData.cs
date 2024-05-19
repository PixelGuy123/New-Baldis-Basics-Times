using BBTimes.Plugin;
using UnityEngine;

namespace BBTimes.CustomComponents.CustomDatas
{
    public class CustomNPCData : CustomBaseData // A basic "mutable" class just for the sole purpose of storing extra info for items
    {
		protected override string SoundPath => System.IO.Path.Combine(BasePlugin.ModPath, "npcs", Name, "Audios");

		protected override string TexturePath => System.IO.Path.Combine(BasePlugin.ModPath, "npcs", Name, "Textures");

		public Character[] npcsBeingReplaced = [];

		public NPC Npc { get => npc; internal set => npc = value; }

		[SerializeField]
		private NPC npc;

		public int replacementWeight = 0;

		// *************** Events that can be triggered with custom npcs *****************
		public virtual void Stabbed() { }
	}
}
