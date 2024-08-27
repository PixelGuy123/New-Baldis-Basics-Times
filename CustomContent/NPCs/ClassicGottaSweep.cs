using BBTimes.Extensions;
using BBTimes.CustomComponents;
using UnityEngine;

namespace BBTimes.CustomContent.NPCs
{
    public class ClassicGottaSweep : MonoBehaviour, INPCPrefab
	{
		public void SetupPrefab()
		{
			var gs = GetComponent<GottaSweep>();
			if (!gs) return;

			gs.spriteRenderer[0].sprite = this.GetSprite(gs.spriteRenderer[0].sprite.pixelsPerUnit, "oldsweep.png");
			gs.maxActive = 60f;
			gs.minActive = 35f;
			gs.minDelay = 45f;
			gs.maxDelay = 60f;
			gs.speed = 75f;
			gs.moveMod = new(Vector3.zero, 0.5f);
			gs.moveModMultiplier = 1f;
			gs.audMan.audioDevice.dopplerLevel = 2f; // I wonder why I didn't set it in here
		}
		public void SetupPrefabPost() { }
		public string Name { get; set; } public string TexturePath => this.GenerateDataPath("npcs", "Textures");
		public string SoundPath => this.GenerateDataPath("npcs", "Audios");
		public NPC Npc { get; set; }
		public Character[] ReplacementNpcs { get; set; }
		public int ReplacementWeight { get; set; }
	}
}
