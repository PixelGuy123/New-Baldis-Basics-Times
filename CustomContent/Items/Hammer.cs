using BBTimes.CustomComponents;
using UnityEngine;
using BBTimes.Manager;

namespace BBTimes.CustomContent.Items
{
    public class Hammer : Item
    {
        public override bool Use(PlayerManager pm)
        {
            Destroy(gameObject);
            if (Physics.Raycast(pm.transform.position, Singleton<CoreGameManager>.Instance.GetCamera(pm.playerNumber).transform.forward, out var raycastHit, pm.pc.reach, BBTimesManager.windowLayer, QueryTriggerInteraction.Collide) && raycastHit.transform.CompareTag("Window"))
            {
                raycastHit.transform.GetComponent<Window>().Break(true);
				bool broken = !raycastHit.transform.GetComponent<CustomWindowComponent>()?.unbreakable ?? true;
				if (broken)
					pm.RuleBreak("breakingproperty", 3f, 0.15f);
				return broken;
            }
            return false;
        }

	}
}
