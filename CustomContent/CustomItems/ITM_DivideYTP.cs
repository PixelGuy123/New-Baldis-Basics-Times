using UnityEngine;

namespace BBTimes.CustomContent.CustomItems
{
	public class ITM_DivideYTP : Item
	{
		public override bool Use(PlayerManager pm)
		{
			Singleton<CoreGameManager>.Instance.AddPoints(Mathf.Abs(Singleton<CoreGameManager>.Instance.GetPointsThisLevel(pm.playerNumber)) / divider, pm.playerNumber, true);
			Destroy(gameObject);
			return true;
		}

		[SerializeField]
		[Range(1, int.MaxValue)]
		internal int divider = 1;
	}
}
