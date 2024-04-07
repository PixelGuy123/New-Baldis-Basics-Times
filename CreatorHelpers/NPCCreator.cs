using BBTimes.CustomComponents.CustomDatas;
using BBTimes.Plugin;
using HarmonyLib;
using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using static UnityEngine.Object;

namespace BBTimes.Helpers
{
	public static partial class CreatorExtensions
	{
		// Some fields that the api misses
		readonly static FieldInfo _npc_ignorePlayerOnSpawn = AccessTools.Field(typeof(NPC), "ignorePlayerOnSpawn");
		readonly static FieldInfo _npc_ignoreBelts = AccessTools.Field(typeof(NPC), "ignoreBelts");
		readonly static FieldInfo _npc_poster = AccessTools.Field(typeof(NPC), "poster");
		readonly static FieldInfo _nav_avoidRooms = AccessTools.Field(typeof(Navigator), "avoidRooms");

		public static T CreateNPC<T, C>(string name, float audioMinDistance, float audioMaxDistance, RoomCategory[] rooms, WeightedRoomAsset[] potentialRoomAssets, string posterNameKey, string posterDescKey, bool disableLooker = false, float spriteYOffset = 0f, bool ignorePlayerOnSpawn = false, bool usesHeatMap = false, bool hasTrigger = true, bool ignoreBelts = false, float lookerDistance = int.MaxValue, bool avoidRooms = true) where T : NPC where C : CustomNPCData
		{
			var sprites = GetAllNpcSpritesFrom(name);
			var npc = ObjectCreators.CreateNPC<T>(name, EnumExtensions.ExtendEnum<Character>(name), ObjectCreators.CreateCharacterPoster(sprites[0].texture, posterNameKey, posterDescKey), !disableLooker, usesHeatMap, hasTrigger, audioMinDistance, audioMaxDistance, rooms);

			// Set some other npc parameters
			npc.potentialRoomAssets = potentialRoomAssets;
			_npc_ignorePlayerOnSpawn.SetValue(npc, ignorePlayerOnSpawn);
			npc.spriteRenderer[0].sprite = sprites[1]; // Sets to default sprite
			_npc_ignoreBelts.SetValue(npc, ignoreBelts);

			npc.spriteBase.transform.Find("Sprite").localPosition = Vector3.up * spriteYOffset; // I HATE ENTITY CLASS JUST MESSING UP WITH SPRITE BASE Y

			npc.looker.distance = lookerDistance;

			_nav_avoidRooms.SetValue(npc.Navigator, avoidRooms);

			var data = npc.gameObject.AddComponent<C>();
			
			// Setup for CustomNPCData
			if (sprites.Length >= 2)
				data.storedSprites = [.. sprites.Skip(1)]; // Excludes necessary sprites

			data.GetAudioClips(); // Of course
			data.SetupPrefab();
			

			return npc;
		}

		public static T CreateRuntimeNPC<T, C>(string name, float audioMinDistance, float audioMaxDistance, bool disableLooker = false, float spriteYOffset = 0f, bool usesHeatMap = false, bool hasTrigger = true, bool ignoreBelts = false, float lookerDistance = int.MaxValue, bool avoidRooms = true) where T : NPC where C : CustomNPCData =>
			CreateNPC<T, C>(name, audioMinDistance, audioMaxDistance, [], [], string.Empty, string.Empty, disableLooker, spriteYOffset, true, usesHeatMap, hasTrigger, ignoreBelts, lookerDistance, avoidRooms);

		public static T InstantiateRuntimeNPC<T>(this T npc, EnvironmentController ec, IntVector2 pos, Vector3 offset) where T : NPC
		{
			ec.SpawnNPC(npc, pos);
			var cnpc = ec.Npcs[ec.Npcs.Count - 1];
			cnpc.transform.position += offset;
			ec.Npcs.RemoveAt(ec.Npcs.Count - 1); // Removes the runtime npc from the list to not be affected by the environment
			return (T)cnpc;
		}
		public static T InstantiateRuntimeNPC<T>(this T npc, EnvironmentController ec, IntVector2 pos) where T : NPC => InstantiateRuntimeNPC(npc, ec, pos, Vector3.zero);

		public static T CreateCustomNPCFromExistent<T, C>(Character target, string name, float spriteYOffset = 0f) where T : NPC where C : CustomNPCData
		{
			var npc = Instantiate((T)target.GetFirstInstance());
			npc.gameObject.name = name;
			DontDestroyOnLoad(npc.gameObject);
			npc.GetComponent<Entity>().SetActive(false); //disable the entity
			npc.gameObject.SetActive(false); // Some copy paste from the API, because we aren't changing the npc itself
			npc.gameObject.layer = LayerMask.NameToLayer("NPCs");

			// handle audio manager stuff
			PropagatedAudioManager audMan = npc.GetComponent<PropagatedAudioManager>();
			Destroy(audMan.audioDevice.gameObject);
			audMan.sourceId = 0; //reset source id
			AudioManager.totalIds--; //decrement total ids
			


			var sprites = GetAllNpcSpritesFrom(name);
			// Set some fields
			npc.spriteRenderer[0].sprite = sprites[1];
			npc.spriteBase.transform.position += Vector3.up * spriteYOffset;

			var poster = (PosterObject)_npc_poster.GetValue(npc);
			poster = Instantiate(poster); // Obviously instantiate it to not affect the og
			poster.baseTexture = sprites[0].texture; // Set posters textures

			foreach (var mat in poster.material)
				mat.mainTexture = sprites[0].texture;

			_npc_poster.SetValue(npc, poster);

			var data = npc.gameObject.AddComponent<C>();

			// Setup for CustomNPCData
			if (sprites.Length >= 2)
				data.storedSprites = [.. sprites.Skip(1)]; // Excludes necessary sprites

			data.GetAudioClips(); // Of course
			data.SetupPrefab();



			return npc;
		}

		public static T MarkAsReplacement<T>(this T npc, params Character[] targets) where T : NPC // I think that's what's called "Builder design"
		{
			var comp = npc.GetComponent<CustomNPCData>();
			if (comp != null)
				comp.npcsBeingReplaced = targets;
			
			return npc;
		}


		static Sprite[] GetAllNpcSpritesFrom(string name)
		{
			var path = Path.Combine(BasePlugin.ModPath, "npcs", name, "Textures");
			if (!Directory.Exists(path))
				return [];

			string[] files = [.. Directory.GetFiles(path).OrderBy(Path.GetFileNameWithoutExtension)]; // Guarantee the order of the files
			var sprs = new Sprite[files.Length];
			string[] repeatedOnes = new string[files.Length];

			// Pre found ones
			var text = files.First(x => Path.GetFileName(x).StartsWith(posterNamePrefix));
#if CHEAT
			Debug.Log("Npc: " + name);
			Debug.Log("Path used for the sprite selection: " + path);
			Debug.Log("Files found in path: " + files.Length);
			Debug.Log("current file: " + Path.GetFileNameWithoutExtension(text));


#endif
			sprs[0] = AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromFile(text), Vector2.zero, 1f);
			repeatedOnes[0] = text;

			text = files.FirstOrDefault(x => Path.GetFileName(x).StartsWith(mainSpritePrefix));
			if (text == default)
			{
				text = files.First(x => !Path.GetFileName(x).StartsWith(posterNamePrefix)); // First sprite in the folder (that is not a poster)
#if CHEAT
				Debug.Log("Default sprite used: " + text);
#endif
			}
#if CHEAT
			Debug.Log("current file: " + Path.GetFileNameWithoutExtension(text));
#endif
			var ar = Path.GetFileNameWithoutExtension(text).Split('_');
			var tex = AssetLoader.TextureFromFile(text);
			sprs[1] = AssetLoader.SpriteFromTexture2D(tex, new((float)tex.width / 2 / tex.width, (float)tex.height / 2 / tex.height), float.Parse(ar[ar.Length - 1]));
			sprs[1].name = ar[0];
			repeatedOnes[1] = text;

			// The rest (which also follows up a pattern)
			int z = 2;

				for (int i = 0; i < files.Length; i++)
				{
					if (repeatedOnes.Contains(files[i])) continue; // Skip repeated ones

					ar = Path.GetFileNameWithoutExtension(files[i]).Split('_');
					tex = AssetLoader.TextureFromFile(files[i]);
					sprs[z] = AssetLoader.SpriteFromTexture2D(tex, new((float)tex.width / 2 / tex.width, (float)tex.height / 2 / tex.height), float.Parse(ar[1]));
					sprs[z].name = ar[0];
					repeatedOnes[z] = files[i];
					z++; // Increment by 1
				}
			

			return sprs;
		}

		const string posterNamePrefix = "pri_", mainSpritePrefix = "mainSpr_";
	}
}
