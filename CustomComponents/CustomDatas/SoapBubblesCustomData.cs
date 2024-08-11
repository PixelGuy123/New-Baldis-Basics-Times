using BBTimes.CustomComponents.NpcSpecificComponents;
using BBTimes.CustomContent.CustomItems;
using BBTimes.CustomContent.NPCs;
using UnityEngine;

namespace BBTimes.CustomComponents.CustomDatas
{
	public class SoapBubblesCustomData : CustomItemData
	{
		public override void SetupPrefab()
		{
			base.SetupPrefab();
			var b = GetComponent<ITM_SoapBubbles>();
			b.bubPre = Resources.FindObjectsOfTypeAll<Bubble>()[0];
			b.audFill = Resources.FindObjectsOfTypeAll<Bubbly>()[0].audFillUp;
			b.audFill = Instantiate(b.audFill);
			b.audFill.color = Color.white;
		}
	}
}
