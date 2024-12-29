namespace BBTimes.CustomComponents
{
	public interface IPrefab
	{
		public string TexturePath { get; }
		public string SoundPath { get; }
		public string Name { get; set; }
	}

	public interface IBuilderPrefab : IObjectPrefab
	{
		StructureWithParameters SetupBuilderPrefabs();
	}

	public interface IObjectPrefab : IPrefab
	{
		void SetupPrefab();
		void SetupPrefabPost();
	}
	public interface INPCPrefab : IObjectPrefab
	{
		Character[] GetReplacementNPCs();
		void SetReplacementNPCs(params Character[] chars);
		public int ReplacementWeight { get; set; }
		public NPC Npc { get; set; }
	}
	public interface IItemPrefab : IObjectPrefab
	{
		public ItemObject ItmObj { get; set; }
	}
}
