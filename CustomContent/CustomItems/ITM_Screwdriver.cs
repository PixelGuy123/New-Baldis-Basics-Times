using UnityEngine;

namespace BBTimes.CustomContent.CustomItems
{
	public class ITM_Screwdriver : Item
	{
		public override bool Use(PlayerManager pm)
		{
			Destroy(gameObject);

			if (Physics.Raycast(pm.transform.position, Singleton<CoreGameManager>.Instance.GetCamera(pm.playerNumber).transform.forward, out var hit, pm.pc.reach, pm.pc.ClickLayers))
			{
				var math = hit.transform.GetComponentInParent<MathMachine>();
				if (math && !math.IsCompleted)
				{
					Singleton<CoreGameManager>.Instance.audMan.PlaySingle(audScrew);
					math.Completed(pm.playerNumber, true, math);
					return true;
				}
			}

			return false;
		}

		[SerializeField]
		internal SoundObject audScrew;
	}
}
