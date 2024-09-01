using MTM101BaldAPI.Registers;
using StackableItems;
using MTM101BaldAPI;
using BBTimes.Manager;

namespace BBTimes.CompatibilityModule
{
	internal class StackableItemsCompat
	{
		internal static void Loadup()
		{
			Items en = EnumExtensions.GetFromExtendedName<Items>("GrabGun");
			StackableItemsPlugin.NonStackableItems.Add(ItemMetaStorage.Instance.Find(x => x.info == BBTimesManager.plug.Info && x.value.itemType == en).value);
		}
	}
}
