using BBTimes.CustomContent.CustomItems;
using PixelInternalAPI.Extensions;

namespace BBTimes.CustomComponents.CustomDatas
{
	public class SugarFlavoredZestyBarCustomData : CustomItemData
	{

		protected override void SetupPrefabPost()
		{
			base.SetupPrefabPost();
			GetComponent<ITM_SugarFlavoredZestyBar>().audEat = GenericExtensions.FindResourceObject<ITM_ZestyBar>().audEat;
		}
	}
}
