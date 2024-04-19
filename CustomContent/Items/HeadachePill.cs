using BBTimes.CustomContent.NPCs;
using UnityEngine;

namespace BBTimes.CustomContent.CustomItems
{
	public class HeadachePill : Item
	{
		public override bool Use(PlayerManager pm)
		{
			Destroy(gameObject);
			if (Stunly.affectedByStunly.Count > 0)
			{
				Singleton<CoreGameManager>.Instance.audMan.PlaySingle(audSwallow);
				while (Stunly.affectedByStunly.Count > 0)
					Stunly.affectedByStunly[0].CancelStunEffect();				
				return true;
			}
			return false;
		}

		[SerializeField]
		internal SoundObject audSwallow;
	}
}
