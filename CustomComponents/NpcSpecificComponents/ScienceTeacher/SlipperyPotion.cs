using UnityEngine;

namespace BBTimes.CustomComponents.NpcSpecificComponents.ScienceTeacher
{
	public class SlipperyPotion : Potion
	{
		protected override void OnEntityEnter(Entity entity)
		{
			base.OnEntityEnter(entity);
			if (SlippingMaterial.SlipEntity(entity, force, acceleration)) // Convenient I guess lol
				audMan.PlaySingle(audSlip);
		}

		[SerializeField]
		internal SoundObject audSlip;

		[SerializeField]
		internal float force = 50f, acceleration = -34.35f;
	}
}
