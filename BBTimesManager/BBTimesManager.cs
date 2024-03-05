using BBTimes.Extensions.ObjectCreationExtensions;
using BepInEx;
using MTM101BaldAPI.AssetTools;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BBTimes.Manager
{
    internal static partial class BBTimesManager // basically holds the logic to create everything to the game
    {
        internal static void InitializeContentCreation(BaseUnityPlugin plug)
        {
			SetMaterials();
            CreateNPCs(plug);
			CreateItems(plug);
			CreateEvents(plug);
			CreateObjBuilders(plug);
        }

		static void SetMaterials()
		{
			ObjectCreationExtension.defaultMaterial = Resources.FindObjectsOfTypeAll<Material>().First(x => x.name == "LockerTest"); // Actually a good material, has even lightmap
			ObjectCreationExtension.defaultDustMaterial = Resources.FindObjectsOfTypeAll<Material>().First(x => x.name == "DustTest"); // Actually a good material, has even lightmap
		}

        internal readonly static List<FloorData> floorDatas = [new("F1"), new("F2"), new("F3"), new("END")];

		public readonly static AssetManager man = new();

    }
	// Floor data
    internal class FloorData(string floor = "none")
    {
        public string Floor => _floor;
        readonly string _floor = floor;

        readonly List<WeightedNPC> _npcs = [];
        public List<WeightedNPC> NPCs => _npcs;

        readonly List<WeightedItemObject> _items = [];
        public List<WeightedItemObject> Items => _items;

		readonly List<WeightedRandomEvent> _events = [];
		public List<WeightedRandomEvent> Events => _events;


		// Object Builders

		readonly List<ObjectBuilder> _forcedObjBuilders = [];
		public List<ObjectBuilder> ForcedObjectBuilders => _forcedObjBuilders;

		readonly List<WeightedObjectBuilder> _weightedObjectBlders = [];
		public List<WeightedObjectBuilder> WeightedObjectBuilders => _weightedObjectBlders;

		readonly List<RandomHallBuilder> _hallBuilders = [];
		public List<RandomHallBuilder> HallBuilders => _hallBuilders;

		//readonly List<GenericHallBuilder> _genericHallBuilders = [];
		//public List<GenericHallBuilder> GenericHallBuilders => _genericHallBuilders;
	}
}
