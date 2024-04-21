using PixelInternalAPI.Extensions;

namespace BBTimes.CustomContent.CustomItems
{
	public class ITM_GoldenQuarter : Item
	{
		public override bool Use(PlayerManager pm)
		{
			Destroy(gameObject);
			if (pm.itm.SlotsAvailable() >= amount)
			{
				pm.itm.SetItem(quarter, pm.itm.selectedItem);
				for (int i = 0; i < amount; i++)
					pm.itm.AddItem(quarter);
			}
				
			

			return false;
		}

		const int amount = 2;

		static internal ItemObject quarter;
	}
}
