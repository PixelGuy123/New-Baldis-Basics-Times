using BBTimes.CustomComponents.CustomDatas;
using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using System.IO;
using System.Linq;
using BBTimes.Plugin;
using UnityEngine;
using static UnityEngine.Object;
using BBTimes.Manager;

namespace BBTimes.Helpers
{
	public static partial class CreatorExtensions
	{
		public static NPC SetupNPCData<C>(this NPC npc, string name, string posterName, string posterDesc, float spriteYoffset = 0f) where C : CustomNPCData
		{
			var sprites = GetAllNpcSpritesFrom(name);
	
			var data = npc.gameObject.AddComponent<C>();
			
			// Setup for CustomNPCData
			if (sprites.Length >= 2)
				data.storedSprites = [.. sprites.Skip(1)]; // Excludes necessary sprites

			npc.spriteRenderer[0].sprite = sprites[1];
			npc.poster.baseTexture = sprites[0].texture;
			npc.poster.textData[0].textKey = posterName;
			npc.poster.textData[1].textKey = posterDesc;

			npc.spriteBase.transform.Find("Sprite").localPosition = Vector3.up * spriteYoffset;

			data.Name = name;
			data.GetAudioClips(); // Of course
			data.SetupPrefab();
			data.Npc = npc;
			


			return npc;
		}

		public static T InstantiateRuntimeNPC<T>(this T npc, EnvironmentController ec, IntVector2 pos, Vector3 offset) where T : NPC
		{
			ec.SpawnNPC(npc, pos);
			var cnpc = ec.Npcs[ec.Npcs.Count - 1];
			cnpc.transform.position += offset;
			ec.Npcs.RemoveAt(ec.Npcs.Count - 1); // Removes the runtime npc from the list to not be affected by the environment
			return (T)cnpc;
		}

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

			//(PosterObject)_npc_poster.GetValue(npc);
			var poster = Instantiate(npc.poster); // Obviously instantiate it to not affect the og
			poster.baseTexture = sprites[0].texture; // Set posters textures

			foreach (var mat in poster.material)
				mat.mainTexture = sprites[0].texture;

			npc.poster = poster; //_npc_poster.SetValue(npc, poster);

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

			if (!BBTimesManager.replacementNpcs.Contains(comp))
				BBTimesManager.replacementNpcs.Add(comp);

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
			sprs[1] = AssetLoader.SpriteFromTexture2D(tex, Vector2.one / 2f, float.Parse(ar[ar.Length - 1]));
			sprs[1].name = ar[0];
			repeatedOnes[1] = text;

			// The rest (which also follows up a pattern)
			int z = 2;

				for (int i = 0; i < files.Length; i++)
				{
					if (repeatedOnes.Contains(files[i])) continue; // Skip repeated ones

					ar = Path.GetFileNameWithoutExtension(files[i]).Split('_');
					tex = AssetLoader.TextureFromFile(files[i]);
					sprs[z] = AssetLoader.SpriteFromTexture2D(tex, Vector2.one / 2f, float.Parse(ar[1]));
					sprs[z].name = ar[0];
					repeatedOnes[z] = files[i];
					z++; // Increment by 1
				}
			

			return sprs;
		}

		const string posterNamePrefix = "pri_", mainSpritePrefix = "mainSpr_";
	}
}
