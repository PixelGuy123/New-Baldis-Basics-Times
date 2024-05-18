using BBTimes.CustomContent.CustomItems;
using PixelInternalAPI.Extensions;

namespace BBTimes.CustomComponents.CustomDatas
{
	public class PencilCustomData : CustomItemData
	{
		public override void SetupPrefab()
		{
			base.SetupPrefab();
			GetComponent<ITM_Pencil>().audMan = gameObject.CreatePropagatedAudioManager(65f, 85f);
		}
	}
}
