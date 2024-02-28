using UnityEngine;

namespace BBTimes.CustomItems
{
	public class Hammer : Item
	{
		public override bool Use(PlayerManager pm)
		{
			Destroy(gameObject);
			if (Physics.Raycast(pm.transform.position, Singleton<CoreGameManager>.Instance.GetCamera(pm.playerNumber).transform.forward, out var raycastHit, pm.pc.reach, LayerMask.NameToLayer("Window"), QueryTriggerInteraction.Collide) && raycastHit.transform.CompareTag("Window"))
			{
				raycastHit.transform.GetComponent<Window>().Break(true);
				return true;
			}
			return false;
		}
	}
}
