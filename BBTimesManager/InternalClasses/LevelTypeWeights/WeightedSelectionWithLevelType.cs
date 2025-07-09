using System.Collections.Generic;
using MTM101BaldAPI;

namespace BBTimes.Manager.InternalClasses.LevelTypeWeights;

// ******************************************************
// ************* Abstract Class for this ****************
// ******************************************************
internal abstract class WeightedSelectionWithLevelType<T, C> where T : class where C : class // should limit the scope of the generics in a way
{
    internal WeightedSelectionWithLevelType(C selection, int weight, params LevelType[] levelTypes)
    {
        if (levelTypes.Length != 0)
        {
            acceptedLevelTypes = [.. levelTypes];
            usesAllLevelType = false;
        }
        this.selection = selection;
        this.weight = weight;
    }
    public abstract T GetWeightedSelection();
    public bool AcceptsLevelType(LevelType type) =>
        usesAllLevelType ? WeightedSelectionWithLevelType_AllStorage.All.Contains(type) : acceptedLevelTypes.Contains(type);


    public C selection;
    public int weight = 100;

    protected bool usesAllLevelType = true;

    readonly HashSet<LevelType> acceptedLevelTypes;
}

internal static class WeightedSelectionWithLevelType_AllStorage // Workaround to not have a static collection inside a generic class for weird reasons?
{
    internal static HashSet<LevelType> All;
}

// ******************************************************
// ******************* Weight LevelType Types *****************
// ******************************************************

internal class WeightedNPCWithLevelType(NPC selection, int weight, params LevelType[] levelTypes) : WeightedSelectionWithLevelType<WeightedNPC, NPC>(selection, weight, levelTypes)
{
    public override WeightedNPC GetWeightedSelection() => new() { selection = selection, weight = weight };
}

internal class WeightedItemObjectWithLevelType(ItemObject selection, int weight, params LevelType[] levelTypes) : WeightedSelectionWithLevelType<WeightedItemObject, ItemObject>(selection, weight, levelTypes)
{
    public override WeightedItemObject GetWeightedSelection() => new() { selection = selection, weight = weight };
}

internal class WeightedTexture2DWithLevelType(UnityEngine.Texture2D selection, int weight, params LevelType[] levelTypes) : WeightedSelectionWithLevelType<WeightedTexture2D, UnityEngine.Texture2D>(selection, weight, levelTypes)
{
    public override WeightedTexture2D GetWeightedSelection() => new() { selection = selection, weight = weight };
}

internal class WeightedRandomEventWithLevelType(RandomEvent selection, int weight, params LevelType[] levelTypes) : WeightedSelectionWithLevelType<WeightedRandomEvent, RandomEvent>(selection, weight, levelTypes)
{
    public override WeightedRandomEvent GetWeightedSelection() => new() { selection = selection, weight = weight };
}

internal class WeightedRoomAssetWithLevelType(RoomAsset selection, int weight, params LevelType[] levelTypes) : WeightedSelectionWithLevelType<WeightedRoomAsset, RoomAsset>(selection, weight, levelTypes)
{
    public override WeightedRoomAsset GetWeightedSelection() => new() { selection = selection, weight = weight };
}

internal class WeightedStructureWithParametersWithLevelType(StructureWithParameters selection, int weight, params LevelType[] levelTypes) : WeightedSelectionWithLevelType<WeightedStructureWithParameters, StructureWithParameters>(selection, weight, levelTypes)
{
    public override WeightedStructureWithParameters GetWeightedSelection() => new() { selection = selection, weight = weight };
}

// ******************************************************
// ************* Forced LevelType Types ******************
// ******************************************************

internal class StructureWithParametersWithLevelType(StructureWithParameters selection, params LevelType[] levelTypes) : WeightedSelectionWithLevelType<StructureWithParameters, StructureWithParameters>(selection, 100, levelTypes)
{
    public override StructureWithParameters GetWeightedSelection() => selection;
}

internal class RoomGroupWithLevelType(RoomGroup selection, params LevelType[] levelTypes) : WeightedSelectionWithLevelType<RoomGroup, RoomGroup>(selection, 100, levelTypes)
{
    public override RoomGroup GetWeightedSelection() => selection;
}

internal class ItemObjectWithLevelType(ItemObject selection, params LevelType[] levelTypes) : WeightedSelectionWithLevelType<ItemObject, ItemObject>(selection, 100, levelTypes)
{
    public override ItemObject GetWeightedSelection() => selection;
}