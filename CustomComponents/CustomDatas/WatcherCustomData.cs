using UnityEngine;

namespace BBTimes.CustomComponents.CustomDatas
{
	public class WatcherCustomData : CustomNPCData
	{
		protected override SoundObject[] GenerateSoundObjects() =>
			[GetSound("WCH_ambience.wav", "Vfx_Wch_Idle", SoundType.Voice, new Color(0.8f, 0.8f, 0.8f)),
		GetSound("WCH_see.wav", "Vfx_Wch_See", SoundType.Voice, new Color(0.8f, 0.8f, 0.8f)),
		GetSound("WCH_angered.wav", "Vfx_Wch_Angry", SoundType.Voice, new Color(0.8f, 0.8f, 0.8f)),
		GetSound("WCH_teleport.wav", "Vfx_Wch_Teleport", SoundType.Voice, new Color(0.8f, 0.8f, 0.8f))];

		protected override Sprite[] GenerateSpriteOrder() =>
			[GetSprite(35f, "watcher.png")];


	}
}
