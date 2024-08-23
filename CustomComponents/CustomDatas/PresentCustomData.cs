using BBTimes.CustomContent.CustomItems;
using BBTimes.Extensions;

namespace BBTimes.CustomComponents.CustomDatas
{
	public class PresentCustomData : CustomItemData
	{
		protected override SoundObject[] GenerateSoundObjects() =>
			[GetSound("prs_unbox.wav", "Vfx_PRS_Unbox", SoundType.Effect, UnityEngine.Color.white)];

		public override void SetupPrefab()
		{
			base.SetupPrefab();
			GetComponent<ITM_Present>().aud_unbox = soundObjects[0];
		}

		protected override void SetupPrefabPost()
		{
			base.SetupPrefabPost();
			var shops = GameExtensions.GetAllShoppingItems();
			shops.RemoveAll(x => x.itemType == myItmObj.itemType);

			GetComponent<ITM_Present>().items = [.. shops];
		}
	}
}
