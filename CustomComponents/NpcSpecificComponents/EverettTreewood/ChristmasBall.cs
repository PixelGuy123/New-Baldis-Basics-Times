using UnityEngine;

namespace BBTimes.CustomComponents.NpcSpecificComponents.EverettTreewood
{
	internal class ChristmasBall : Snowflake
	{
		protected override void AffectEntity(Entity e, PlayerManager pm)
		{
			base.AffectEntity(e, pm);
			if (pm)
			{
				canvas.gameObject.SetActive(true);
				canvas.worldCamera = Singleton<CoreGameManager>.Instance.GetCamera(pm.playerNumber).canvasCam;
			}
		}

		[SerializeField]
		internal Canvas canvas;
	}
}
