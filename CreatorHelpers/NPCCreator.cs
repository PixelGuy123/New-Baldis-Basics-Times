using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using System.IO;
using System.Linq;
using BBTimes.Plugin;
using UnityEngine;
using static UnityEngine.Object;
using BBTimes.Manager;
using BBTimes.Extensions;
using MTM101BaldAPI.Registers;
using BBTimes.CustomComponents;

namespace BBTimes.Helpers
{
	public static partial class CreatorExtensions
	{
		public static NPC SetupNPCData(this NPC npc, string name, string posterName, string posterDesc, float spriteYoffset = 0f)
		{
			var sprites = GetAllNpcSpritesFrom(name);
			
			// Setup for CustomNPCData

			
			npc.poster.baseTexture = sprites.texture;
			npc.poster.textData[0].textKey = posterName;
			npc.poster.textData[1].textKey = posterDesc;

			npc.spriteBase.transform.Find("Sprite").localPosition = Vector3.up * spriteYoffset;
			npc.GetComponent<PropagatedAudioManager>().overrideSubtitleColor = false; // Workaround for the overriding being active

			var data = npc.GetComponent<INPCPrefab>();
			data.Npc = npc;
			data.SetupPrefab();

			// npc.spriteRenderer[0].sprite = data.storedSprites[0]; WILL be defined in the setup prefab by the npc, leave this comment as a reminder


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

		public static T CreateCustomNPCFromExistent<T>(Character target, string name, float spriteYOffset = 0f) where T : NPC
		{
			var npc = (T)NPCMetaStorage.Instance.Get(target).value.SafeInstantiate();
			npc.gameObject.name = name;
			npc.gameObject.ConvertToPrefab(true);

			npc.gameObject.layer = LayerMask.NameToLayer("NPCs");
			


			var sprites = GetAllNpcSpritesFrom(name);
			// Set some fields
			npc.spriteBase.transform.position += Vector3.up * spriteYOffset;

			//(PosterObject)_npc_poster.GetValue(npc);
			var poster = Instantiate(npc.poster); // Obviously instantiate it to not affect the og
			poster.baseTexture = sprites.texture; // Set posters textures

			foreach (var mat in poster.material)
				mat.mainTexture = sprites.texture;

			npc.poster = poster; //_npc_poster.SetValue(npc, poster);

			var data = npc.gameObject.GetComponent<INPCPrefab>();
			data.Npc = npc;
			data.SetupPrefab();


			return npc;
		}

		public static T MarkAsReplacement<T>(this T npc, int weight, params Character[] targets) where T : NPC // I think that's what's called "Builder design"
		{
			var comp = npc.GetComponent<INPCPrefab>();
			if (comp != null)
			{
				comp.ReplacementNpcs = targets;
				comp.ReplacementWeight = weight;
			}

			if (!BBTimesManager.replacementNpcs.Contains(comp))
				BBTimesManager.replacementNpcs.Add(comp);

			return npc;
		}


		static Sprite GetAllNpcSpritesFrom(string name)
		{
			var path = Path.Combine(BasePlugin.ModPath, "npcs", name, "Textures");
			if (!Directory.Exists(path))
				return null;

			string[] files = Directory.GetFiles(path);
			string[] repeatedOnes = new string[files.Length];

			// Pre found ones
			var text = files.First(x => Path.GetFileName(x).StartsWith(posterNamePrefix));
#if CHEAT
			Debug.Log("Npc: " + name);
			Debug.Log("Path used for the sprite selection: " + path);
			Debug.Log("Files found in path: " + files.Length);
			Debug.Log("current file: " + Path.GetFileNameWithoutExtension(text));
#endif

			return AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromFile(text), Vector2.zero, 1f);
		}

		const string posterNamePrefix = "pri_";
	}
}
