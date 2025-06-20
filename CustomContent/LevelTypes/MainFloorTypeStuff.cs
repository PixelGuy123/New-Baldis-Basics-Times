
using System.Collections.Generic;
using BBTimes;
using MTM101BaldAPI;

namespace CustomContent.LevelTypes
{
    public static class FloorTypeCheck
    {
        public static bool shouldGenerateFloorType(string LevelName, int LevelId, SceneObject scene, string typeName)
        {
            if (LevelName != "F4"
            && LevelName != "F5"
            && BasePlugin.config.Bind<bool>("Level types settings", typeName, true, "Enables the floor type " + typeName + " disabling it will make it not spawn on F4 and F5").Value) return false;
            return true;
        }
    }

    
    public static class FloorTypeEnums
    {
        public static List<LevelType> levelTypesEnum = new List<LevelType>();
        public static void GenerateEnums()
        {
            levelTypesEnum.Add(EnumExtensions.ExtendEnum<LevelType>("Sewer"));

        }
    }


    public static class MainFloorTypeStuff
    {


        public static void FloorTypesHandler(string floorname, int floorId, SceneObject scene)
        {
            SewerFloorType.SewerTypeCreator(floorname, floorId, scene);
        }


    }
}