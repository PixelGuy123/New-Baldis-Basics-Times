

namespace BBTimes.CustomComponents
{
	public interface INPCPrefab : IObjectPrefab
	{
		Character[] GetReplacementNPCs();
		void SetReplacementNPCs(params Character[] chars); 
		public int ReplacementWeight { get; set; }
		public NPC Npc { get; set; }
	}
}
