using UnityEngine;

namespace BBTimes.CustomComponents.NpcSpecificComponents.ScienceTeacher
{
	public class SlipperyPotion : Potion
	{
		protected override void OnEntityEnter(Entity entity)
		{
			base.OnEntityEnter(entity);
			SlippingMaterial.SlipEntity(entity, force, acceleration); // Convenient I guess lol
		}

		[SerializeField]
		internal SoundObject audSlip;

		[SerializeField]
		internal float force = 50f, acceleration = -37.35f;
	}
}
