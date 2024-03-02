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
        }

		static void SetMaterials()
		{
			ObjectCreationExtension.defaultMaterial = Resources.FindObjectsOfTypeAll<Material>().First(x => x.name == "LockerTest"); // Actually a good material, has even lightmap
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
	}
}
