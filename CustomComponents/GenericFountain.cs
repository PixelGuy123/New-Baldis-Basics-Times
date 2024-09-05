using UnityEngine;

namespace BBTimes.CustomComponents
{
	public class GenericFountain : EnvironmentObject, IClickable<int>
	{
		public void Clicked(int player)
		{
			if (audSip)
				audMan.PlaySingle(audSip);

			Singleton<CoreGameManager>.Instance.GetPlayer(player).plm.AddStamina(
				refillAll ? Singleton<CoreGameManager>.Instance.GetPlayer(player).plm.staminaMax : refillValue, true);
		}


		public void ClickableSighted(int player) { }
		public void ClickableUnsighted(int player) { }
		public bool ClickableHidden() => false;
		public bool ClickableRequiresNormalHeight() => requiresNormalHeight;

		[SerializeField]
		internal bool requiresNormalHeight = false, refillAll = false;

		[SerializeField]
		internal float refillValue = 25f;

		[SerializeField]
		internal AudioManager audMan;

		[SerializeField]
		internal SoundObject audSip;
	}
}
