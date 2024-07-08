using UnityEngine;

namespace BBTimes.CustomContent.CustomItems
{
	public class ITM_CherryBsoda : ITM_BSODA
	{
		public override bool Use(PlayerManager pm)
		{
			dir = Singleton<CoreGameManager>.Instance.GetCamera(pm.playerNumber).transform.forward;
			entity.OnEntityMoveInitialCollision += (hit) => // Basically just bounce over
			{
				dir = Vector3.Reflect(dir, hit.normal);
				transform.forward = dir;
				audMan.PlaySingle(audHit);
			};
			return base.Use(pm);
		}

		Vector3 dir;

		[SerializeField]
		internal PropagatedAudioManager audMan;

		[SerializeField]
		internal SoundObject audHit;
	}
}
