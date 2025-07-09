using System.Collections.Generic;
using BBTimes.Manager.InternalClasses.LevelTypeWeights;
using BBTimes.Misc.SelectionHolders;
using MTM101BaldAPI;

namespace BBTimes.Manager.InternalClasses;

// Floor data
internal class FloorData()
{
    public CustomLevelObject[] levelObjects;

    public readonly List<WeightedNPCWithLevelType> NPCs = [];
    public readonly List<WeightedItemObjectWithLevelType> Items = [];
    public readonly List<ItemObjectWithLevelType> ForcedItems = [];
    public readonly List<WeightedItemObject> ShopItems = [];
    public readonly List<WeightedItemObject> FieldTripItems = [];
    public readonly List<WeightedRandomEventWithLevelType> Events = [];
    public readonly List<SchoolTextureHolder> SchoolTextures = [];

    // Rooms
    public readonly List<RoomGroupWithLevelType> RoomAssets = [];
    public readonly List<WeightedRoomAssetWithLevelType> SpecialRooms = [];
    public readonly List<WeightedRoomAsset> Classrooms = [];
    public readonly List<WeightedRoomAsset> Faculties = [];
    public readonly List<WeightedRoomAsset> Offices = [];
    public readonly Dictionary<WeightedRoomAsset, bool> Halls = [];


    // Object Builders
    public readonly List<StructureWithParametersWithLevelType> ForcedObjectBuilders = [];
    public readonly List<WeightedStructureWithParametersWithLevelType> WeightedObjectBuilders = []; // Basically unused at this point

    //readonly List<GenericHallBuilder> _genericHallBuilders = [];
    //public List<GenericHallBuilder> GenericHallBuilders => _genericHallBuilders;
}