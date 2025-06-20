using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MTM101BaldAPI;
using UnityEngine;
using BBTimes.Manager;
using MTM101BaldAPI.AssetTools;
using BBTimes.CustomContent.Builders;
namespace CustomContent.LevelTypes
{
    public static class SewerFloorType
    {
        static AssetManager AssetMan => BBTimesManager.levelTypeAssetManager;


        public static void ModifyToSewer(string floorname, int floorId, LevelObject toModify)
        {
            toModify.maxPlots = 1;
            toModify.minPlots = 1;
            toModify.maxReplacementHalls = 0;
            toModify.minReplacementHalls = 0;
            toModify.bridgeTurnChance = 0;
            toModify.maxHallsToRemove = 0;
            toModify.minHallsToRemove = 0;
            toModify.prePlotSpecialHallChance = 0;
            toModify.postPlotSpecialHallChance = 0;
            toModify.minPlotSize = 10;
            toModify.hallFloorTexs = new WeightedTexture2D[] {
                new() {
                    selection = AssetMan.Get<Texture2D>("tex_sewer_floor"),
                    weight = 100
                }
            };
            toModify.hallWallTexs = new WeightedTexture2D[] {
                new() {
                    selection = AssetMan.Get<Texture2D>("tex_sewer_wall"),
                    weight = 100
                }
            };
            toModify.hallCeilingTexs = new WeightedTexture2D[] {
                new() {
                    selection = AssetMan.Get<Texture2D>("tex_sewer_ceil"),
                    weight = 100
                }
            };

            toModify.forcedStructures = toModify.forcedStructures.AddToArray(
                new StructureWithParameters()
                {
                    parameters = new()
                    {
                        minMax = [new IntVector2(9, 0)],
                        chance = [0.25f],
                        prefab = new WeightedGameObject[] {
                        new WeightedGameObject() {
                            selection = Resources.FindObjectsOfTypeAll<GameObject>().First(x => x.name == "Door_Swinging"),
                            weight = 99
                        }
                    }
                    },
                    prefab = Resources.FindObjectsOfTypeAll<Structure_HallDoor>().First(x => x.name == "SwingingDoorConstructor")

                }
            );
            toModify.forcedStructures = toModify.forcedStructures.AddToArray(
                new StructureWithParameters()
                {
                    parameters = new()
                    {

                    }
                    ,
                    prefab = AssetMan.Get<Structure_WaterCreator>("Structure_WC")

                }
            );
            toModify.forcedStructures = toModify.forcedStructures.AddToArray(
                new StructureWithParameters()
                {
                    parameters = new()
                    {

                    }
                    ,
                    prefab = AssetMan.Get<Structure_DoorLockerSmth>("Structure_DL")

                }
            );
            
            


        }

        public static void SewerTypeCreator(string floorname, int floorId, SceneObject scene)
        {
            // if not Floor 4/5 then return
            if (!FloorTypeCheck.shouldGenerateFloorType(floorname, floorId, scene, "Sewer")) return;
            CustomLevelObject[] supportedObjects = scene.GetCustomLevelObjects();
            CustomLevelObject factoryLevel = supportedObjects.First(x => x.type == LevelType.Factory);
            if (factoryLevel == null) return;
            CustomLevelObject SewerClone = factoryLevel.MakeClone();
            SewerClone.type = FloorTypeEnums.levelTypesEnum[0];
            SewerClone.name = "Sewer";
            List<StructureWithParameters> structures = SewerClone.forcedStructures.ToList();
            structures.RemoveAll(x => x.prefab is Structure_Rotohalls);
            structures.RemoveAll(x => x.prefab is Structure_ConveyorBelt);
            structures.RemoveAll(x => x.prefab is Structure_LevelBox);
            structures.RemoveAll(x => x.prefab.name == "LockdownDoorConstructor");

            SewerClone.forcedStructures = structures.ToArray();
            ModifyToSewer(floorname, floorId, SewerClone);
            // no forget to change weight to 100
            scene.randomizedLevelObject = scene.randomizedLevelObject.AddToArray(new WeightedLevelObject()
            {
                selection = SewerClone,
                weight = 1000000
            });
        }

    }
}