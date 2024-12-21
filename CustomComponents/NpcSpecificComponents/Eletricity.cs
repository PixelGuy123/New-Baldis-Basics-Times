using UnityEngine;

namespace BBTimes.CustomComponents.NpcSpecificComponents
{
	public class Eletricity : GlueObject
	{
		protected override void Initialize()
		{
			base.Initialize();
			ani.Initialize(ec);
		}

		protected override void VirtualUpdate()
		{
			base.VirtualUpdate();
			if (++frameDelay >= 3)
			{
				moveMod.movementAddend.x = Random.Range(-eletricityForce, eletricityForce);
				moveMod.movementAddend.z = Random.Range(-eletricityForce, eletricityForce);
				frameDelay = 0;
			}
		}
		[SerializeField]
		internal AnimationComponent ani;

		[SerializeField]
		internal float eletricityForce = 5f;

		int frameDelay = 0;
	}
}
