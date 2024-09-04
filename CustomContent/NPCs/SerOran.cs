using UnityEngine;
using BBTimes.Extensions;
using BBTimes.CustomComponents;
using PixelInternalAPI.Extensions;

namespace BBTimes.CustomContent.NPCs
{
	public class SerOran : NPC, INPCPrefab
	{
		public void SetupPrefab()
		{
			audMan = GetComponent<PropagatedAudioManager>();
			renderer = spriteRenderer[0];
		
		}
		public void SetupPrefabPost() { }
		public string Name { get; set; }
		public string TexturePath => this.GenerateDataPath("npcs", "Textures");
		public string SoundPath => this.GenerateDataPath("npcs", "Audios");
		public NPC Npc { get; set; }
		public Character[] ReplacementNpcs { get; set; }
		public int ReplacementWeight { get; set; }
		// --------------------------------------------------

		[SerializeField]
		internal PropagatedAudioManager audMan;

		[SerializeField]
		internal SpriteRenderer renderer;

		[SerializeField]
		internal Sprite sprs;
	}
}
