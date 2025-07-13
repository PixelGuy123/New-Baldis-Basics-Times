using System.Collections.Generic;
using System.IO;
using System.Linq;
using BBTimes.CustomContent.Objects;
using BBTimes.CustomContent.RoomFunctions;
using BBTimes.Extensions;
using BBTimes.Extensions.ObjectCreationExtensions;
using BBTimes.Helpers;
using HarmonyLib;
using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.Registers;
using PixelInternalAPI.Classes;
using PixelInternalAPI.Extensions;
using UnityEngine;

namespace BBTimes.Manager
{
	internal static partial class BBTimesManager
	{
		static void CreateRoomFunctions() // This is specifically for base game rooms, custom rooms can add their room functions by other ways
		{
			var classicWindow = CreatorExtensions.CreateWindow("classicWindow",
				AssetLoader.TextureFromFile(Path.Combine(MiscPath, TextureFolder, GetAssetName("ClassicWindow.png"))),
				AssetLoader.TextureFromFile(Path.Combine(MiscPath, TextureFolder, GetAssetName("ClassicWindow_Broken.png"))),
				AssetLoader.TextureFromFile(Path.Combine(MiscPath, TextureFolder, GetAssetName("ClassicWindow_Mask.png"))));
			// Random Window Function for cafe
			RandomWindowFunction windowFunc;
			if (TryAddFunctionToEveryRoom(CafeteriaPrefix, out windowFunc))
			{
				windowFunc.window = classicWindow;
			}
			if (TryAddFunctionToEveryRoom(PlaygroundPrefix, out windowFunc))
			{
				windowFunc.window = classicWindow;
			}

			// Random poster functon for class and office
			PosterObject[] poster = [man.Get<PosterObject>("WallClock")];
			var posterFuncAssignment = new System.Action<RandomPosterFunction>((posterFunc) => posterFunc.posters = poster);

			if (TryAddFunctionToEveryRoom(RoomCategory.Class, out List<RandomPosterFunction> posterFuncs))
				posterFuncs.ForEach(posterFuncAssignment);

			if (TryAddFunctionToEveryRoom(RoomCategory.Office, out posterFuncs))
				posterFuncs.ForEach(posterFuncAssignment);

			if (TryAddFunctionToEveryRoom(CafeteriaPrefix, out RandomPosterFunction posterFunc))
			{
				posterFunc.posters = [ObjectCreators.CreatePosterObject([AssetLoader.TextureFromFile(GetRoomAsset("Cafeteria", "CafeteriaRules.png"))]),
					ObjectCreators.CreatePosterObject([AssetLoader.TextureFromFile(GetRoomAsset("Cafeteria", "food_allergy_.png"))])];
			}

			if (TryAddFunctionToEveryRoom<RandomItemSpawnFunction>(RoomCategory.Office, out var officeItemFuncs))
			{
				var yearBook = ItemMetaStorage.Instance.FindByEnum(EnumExtensions.GetFromExtendedName<Items>("BaldiYearbook"));
				if (yearBook != null)
					officeItemFuncs.ForEach(func => func.itemToSpawn = yearBook.value);
			}

			// High ceiling function
			if (TryAddFunctionToEveryRoom(CafeteriaPrefix, out HighCeilingRoomFunction highCeil))
			{
				highCeil.ceilingHeight = 5;
				highCeil.customCeiling = ObjectCreationExtension.blackTex;
				highCeil.customWallProximityToCeil = [AssetLoader.TextureFromFile(GetRoomAsset("Cafeteria", "wallFadeInBlack.png"))];
				highCeil.chanceToHappen = 0.8f;
				highCeil.customLight = man.Get<GameObject>("prefab_cafeHangingLight").transform;
			}

			if (TryAddFunctionToEveryRoom(LibraryPrefix, out highCeil))
			{
				highCeil.targetTransformNamePrefix = "Bookshelf";
				highCeil.targetTransformOffset = 9f;
				highCeil.customLight = man.Get<GameObject>("prefab_libraryHangingLight").transform;
				highCeil.usesSingleCustomWall = true;
				highCeil.customWallProximityToCeil = [AssetLoader.TextureFromFile(GetRoomAsset("Library", GetAssetName("libraryWallSheet.png")))];
			}

			// -------------------- DUST SHROOM CREATION ----------------------------

			WeightedTransform[] transforms = [.. man.Get<List<WeightedTransform>>("prefabs_cornerLamps")];


			if (TryAddFunctionToEveryRoom<CornerObjectSpawner>(RoomCategory.Faculty, out var cos))
			{
				cos.ForEach(func =>
				{
					func.lightPower = 0;
					func.minAmount = 2;
					func.maxAmount = 3;
					func.randomObjs = transforms;
					func.randomChance = 0.6f;
					func.nonSafeEntityCell = true;
				});
			}

			if (TryAddFunctionToEveryRoom<CornerObjectSpawner>(RoomCategory.Office, out cos))
			{
				cos.ForEach(func =>
				{
					func.lightPower = 0;
					func.minAmount = 1;
					func.maxAmount = 3;
					func.randomObjs = transforms;
					func.nonSafeEntityCell = true;
				});
			}

			// ******* Dust Shroom Creation ********

			var sprites = TextureExtensions.LoadSpriteSheet(2, 1, 33f, BasePlugin.ModPath, "objects", "DustShroom", GetAssetName("shroom.png"));
			var shroomObject = ObjectCreationExtensions.CreateSpriteBillboard(sprites[0]).AddSpriteHolder(out var shroomRenderer, 1.25f, LayerStorage.ignoreRaycast);

			var shroom = shroomObject.gameObject.AddComponent<DustShroom>();
			shroom.name = "DustShroom";
			shroomRenderer.name = "DustShroomRenderer";
			shroom.gameObject.AddBoxCollider(Vector3.up * 5f, new(1f, 10f, 1f), true); // Touching hitbox
			shroom.gameObject.AddObjectToEditor();

			shroom.renderer = shroomRenderer;

			shroom.audMan = shroom.gameObject.CreatePropagatedAudioManager(15f, 65f);
			shroom.audUsed = ObjectCreators.CreateSoundObject(
				AssetLoader.AudioClipFromFile(Path.Combine(BasePlugin.ModPath, "objects", "DustShroom", GetAssetName("deflate.wav"))),
				"Vfx_DustShroom_Deflate", SoundType.Voice, Color.white);

			shroom.raycastBlockingCollider = new GameObject("RaycastBlocker").AddBoxCollider(Vector3.up * 5f, new(4f, 10f, 4f), false);
			shroom.raycastBlockingCollider.gameObject.layer = LayerStorage.blockRaycast;
			shroom.raycastBlockingCollider.transform.SetParent(shroom.transform);
			shroom.raycastBlockingCollider.transform.localPosition = Vector3.zero;
			shroom.raycastBlockingCollider.enabled = false;

			shroom.sprUsed = sprites[1];
			shroom.sprActive = sprites[0];

			var system = GameExtensions.GetNewParticleSystem();
			system.name = "Dusts";
			system.transform.SetParent(shroom.transform);
			system.transform.localPosition = Vector3.up * shroom.renderer.transform.localPosition.y;
			system.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
			var partsRenderer = system.GetComponent<ParticleSystemRenderer>();
			partsRenderer.material = new Material(ObjectCreationExtension.defaultDustMaterial) { mainTexture = AssetLoader.TextureFromFile(Path.Combine(BasePlugin.ModPath, "objects", "DustShroom", GetAssetName("dust.png"))) };

			var main = system.main;
			main.gravityModifierMultiplier = -0.65f;
			main.startLifetimeMultiplier = 5f;
			main.startSpeedMultiplier = 2f;
			main.simulationSpace = ParticleSystemSimulationSpace.World;
			main.startSize = 1f;
			main.startRotation = new(1f, 90f);

			var size = system.sizeOverLifetime;
			size.enabled = true;

			var minMaxCurve = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(
				new Keyframe(0f, 1f),
				new Keyframe(0.15f, 2.5f),
				new Keyframe(0.3f, 3.5f),
				new Keyframe(5f, 8f)
				));

			size.x = minMaxCurve;
			size.y = minMaxCurve;
			size.z = minMaxCurve;


			var emission = system.emission;
			emission.rateOverTimeMultiplier = 35f;
			emission.enabled = false;

			var vel = system.velocityOverLifetime;
			vel.enabled = true;
			vel.space = ParticleSystemSimulationSpace.World;
			vel.x = new(-3f, 3f);
			vel.y = new(4f, 12f);
			vel.z = new(-3f, 3f);

			var rot = system.rotationOverLifetime;
			rot.enabled = true;
			rot.x = new(2f, 5f);

			shroom.particles = system;

			var rendCont = shroom.GetComponent<RendererContainer>();
			rendCont.renderers = rendCont.renderers.AddToArray(partsRenderer);

			// Actual implementation of dust shroom
			if (TryAddFunctionToEveryRoom<RandomObjectSpawner>(PlaygroundPrefix, out var rngSpawner))
			{
				rngSpawner.objectPlacer = new ObjectPlacer()
				{
					prefab = shroomObject.gameObject,
					coverage = CellCoverage.Down,
					eligibleShapes = TileShapeMask.Open | TileShapeMask.Corner | TileShapeMask.Straight | TileShapeMask.Single,
					min = 4,
					max = 8,
					useOpenDir = true,
					useWallDir = false,
					includeOpen = true
				};
			}
		}

		static bool TryAddFunctionToEveryRoom<R>(string prefix, out R roomFunction) where R : RoomFunction
		{
			var r = Resources.FindObjectsOfTypeAll<RoomAsset>().FirstOrDefault(x => x.name.StartsWith(prefix));
			if (!r)
			{
				roomFunction = null;
				return false;
			}
			roomFunction = r.roomFunctionContainer.gameObject.AddComponent<R>();
			r.roomFunctionContainer.AddFunction(roomFunction);
			return true;
		}

		static bool TryAddFunctionToEveryRoom<R>(RoomCategory category, out List<R> roomFunctions) where R : RoomFunction
		{
			_usedContainers.Clear();
			List<R> functions = [];

			foreach (var room in Resources.FindObjectsOfTypeAll<RoomAsset>())
			{
				if (room.category != category || _usedContainers.Contains(room.roomFunctionContainer))
					continue;

				_usedContainers.Add(room.roomFunctionContainer);
				var roomFunction = room.roomFunctionContainer.gameObject.AddComponent<R>();

				functions.Add(roomFunction);
			}

			if (functions.Count == 0)
			{
				roomFunctions = null;
				return false;
			}

			roomFunctions = functions;
			return true;
		}

		internal static List<R> AddFunctionToEverythingExcept<R>(params RoomCategory[] exceptions) where R : RoomFunction =>
			AddFunctionToEverythingExcept<R>((x) => true, exceptions);
		internal static List<R> AddFunctionToEverythingExcept<R>(System.Predicate<RoomAsset> predicate, params RoomCategory[] exceptions) where R : RoomFunction
		{
			List<R> l = [];
			foreach (var room in Resources.FindObjectsOfTypeAll<RoomAsset>())
			{
				if (room.roomFunctionContainer && !exceptions.Contains(room.category) && !room.roomFunctionContainer.GetComponent<R>() && predicate(room))
				{
					var comp = room.roomFunctionContainer.gameObject.AddComponent<R>();
					room.roomFunctionContainer.AddFunction(comp);
					l.Add(comp);
				}
			}
			return l;
		}

		const string CafeteriaPrefix = "Cafeteria", LibraryPrefix = "Library", PlaygroundPrefix = "Playground";

		static readonly HashSet<RoomFunctionContainer> _usedContainers = [];
	}
}
