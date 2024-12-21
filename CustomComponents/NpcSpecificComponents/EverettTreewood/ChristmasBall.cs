using UnityEngine;

namespace BBTimes.CustomComponents.NpcSpecificComponents.EverettTreewood
{
	internal class ChristmasBall : Snowflake
	{
		protected override void Initialized()
		{
			base.Initialized();
			audMan.maintainLoop = true;
			audMan.SetLoop(true);
			audMan.QueueAudio(audWoosh);
		}
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

		[SerializeField]
		internal SoundObject audWoosh;
	}
}
