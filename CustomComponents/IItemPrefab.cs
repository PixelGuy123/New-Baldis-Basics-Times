using UnityEngine;

namespace BBTimes.CustomComponents
{
	public interface IItemPrefab : IObjectPrefab
	{
		public ItemObject ItmObj { get; set; }
	}
}
