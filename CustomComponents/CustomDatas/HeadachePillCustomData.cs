using BBTimes.CustomContent.CustomItems;
using UnityEngine;

namespace BBTimes.CustomComponents.CustomDatas
{
	internal class HeadachePillCustomData : CustomItemData
	{
		protected override SoundObject[] GenerateSoundObjects() =>
			[GetSound("swallow.wav", "HDP_Swallow", SoundType.Effect, Color.white)];

		public override void SetupPrefab()
		{
			base.SetupPrefab();
			GetComponent<ITM_HeadachePill>().audSwallow = soundObjects[0];
		}
	}
}
