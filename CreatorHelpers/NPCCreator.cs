using BBTimes.CustomComponents.CustomDatas;
using BBTimes.Plugin;
using HarmonyLib;
using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.Reflection;
using System.IO;
using System.Linq;
using UnityEngine;
using static UnityEngine.Object;

namespace BBTimes.Helpers
{
	public static partial class CreatorExtensions
	{
		public static T CreateRuntimeNPC<T, C>(string name, float audioMinDistance, float audioMaxDistance, bool disableLooker = false, float spriteYOffset = 0f) where T : NPC where C : CustomNPCData => CreateNPC<T, C>(name, audioMinDistance, audioMaxDistance, [], [], string.Empty, string.Empty, disableLooker, spriteYOffset);
		public static T CreateNPC<T, C>(string name, float audioMinDistance, float audioMaxDistance, RoomCategory[] rooms, WeightedRoomAsset[] potentialRoomAssets, string posterNameKey, string posterDescKey, bool disableLooker = false, float spriteYOffset = 0f) where T : NPC where C : CustomNPCData
		{
			var npc = Instantiate(GetBeans()).gameObject;
			npc.name = name;
			Destroy(npc.GetComponent<Beans>()); // Removes the component already
			var comp = npc.AddComponent<T>();
			var entity = npc.GetComponent<Entity>();
			var data = npc.AddComponent<C>();
			entity.ReflectionSetVariable("active", false); // Set this to false manually to not affect layers
			npc.SetActive(false);
			DontDestroyOnLoad(npc); // Prefab of course
			Destroy(npc.GetComponent<Animator>()); // Useless


			// Setup the npc component
			AccessTools.Field(typeof(T), "navigator").SetValue(comp, npc.GetComponent<Navigator>());

			var sprites = GetAllNpcSpritesFrom(name);

			var poster = Instantiate((PosterObject)beans.ReflectionGetVariable("poster")); // Setups poster
			poster.textData[0].textKey = posterNameKey;
			poster.textData[1].textKey = posterDescKey;
			poster.baseTexture = sprites[0].texture;
			for (int i = 0; i < poster.material.Length; i++)
			{
				poster.material[i] = new(poster.material[i]) // Materials are the same, better change it
				{
					mainTexture = sprites[0].texture
				};
			}

			AccessTools.Field(typeof(T), "poster").SetValue(comp, poster);
			comp.spriteBase = comp.transform.GetChild(0).gameObject; // Only child anyways
			comp.spriteBase.transform.localPosition += Vector3.up * spriteYOffset;
			comp.spriteRenderer = comp.spriteBase.transform.GetChild(0).GetComponents<SpriteRenderer>(); // Only available sprite render (in an array)
			comp.spriteRenderer[0].sprite = sprites[1]; // Use the main sprite set in the npc's folder
			comp.baseTrigger = npc.GetComponents<CapsuleCollider>(); // More than one collider actually
			comp.looker = npc.GetComponent<Looker>(); // Set looker
			comp.looker.enabled = !disableLooker;
			comp.spawnableRooms = [.. rooms];
			comp.potentialRoomAssets = [.. potentialRoomAssets]; // Temporary too as there aren't custom rooms yet
			comp.ReflectionSetVariable("character", EnumExtensions.ExtendEnum<Character>(name)); // extend enum

			// Setup the entity component
			AccessTools.Field(typeof(Entity), "iEntityTrigger").SetValue(entity, new IEntityTrigger[1] { comp }); // Sets the trigger to itself

			// Setup for navigator component
			var nav = npc.GetComponent<Navigator>();
			nav.npc = comp; // Set npc reference
			AccessTools.Field(typeof(Navigator), "entity").SetValue(nav, entity); // hardly set entity reference
			AccessTools.Field(typeof(Navigator), "collider").SetValue(nav, comp.baseTrigger[0]); // Use by default the first trigger

			// Setup for looker component
			AccessTools.Field(typeof(Looker), "npc").SetValue(comp.looker, comp);

			// Additional field changes
			var man = npc.GetComponent<PropagatedAudioManager>();
			AccessTools.Field(typeof(PropagatedAudioManager), "minDistance").SetValue(man, audioMinDistance);
			AccessTools.Field(typeof(PropagatedAudioManager), "maxDistance").SetValue(man, audioMaxDistance); // Set the audio distance, temporarily disabled

			// Setup for CustomNPCData
			if (sprites.Length > 2)
				data.storedSprites = [.. sprites.Skip(1)]; // Excludes necessary sprites

			data.GetAudioClips(); // Of course
			data.SetupPrefab();


			return comp;
		}

		private static Beans GetBeans()
		{
			beans ??= (Beans)Character.Beans.GetFirstInstance();

			return beans;
		}

		/*private static PosterTextData CloneTextData(this PosterTextData text) =>
			new()
			{
				font = text.font,
				alignment = text.alignment,
				position = text.position,
				color = text.color,
				fontSize = text.fontSize,
				size = text.size,
				style = text.style,
				textKey = text.textKey
			}; Not used (for now, until I'm sure)*/

		static Sprite[] GetAllNpcSpritesFrom(string name)
		{
			var path = Path.Combine(BasePlugin.ModPath, "npcs", name, "Textures");
			if (!Directory.Exists(path))
				return [];

			var files = Directory.GetFiles(path);
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

			text = files.First(x => Path.GetFileName(x).StartsWith(mainSpritePrefix));
#if CHEAT
			Debug.Log("current file: " + Path.GetFileNameWithoutExtension(text));
#endif
			var ar = Path.GetFileNameWithoutExtension(text).Split('_');
			var tex = AssetLoader.TextureFromFile(text);
			sprs[1] = AssetLoader.SpriteFromTexture2D(tex, new((float)tex.width / 2 / tex.width, (float)tex.height / 2 / tex.height), float.Parse(ar[2]));
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


		static Beans beans;
	}
}
