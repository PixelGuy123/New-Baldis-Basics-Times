using UnityEngine;

namespace BBTimes.CustomComponents.NpcSpecificComponents.ScienceTeacher
{
	public class AcidPotion : Potion
	{
		protected override void OnEntityStay(Entity entity)
		{
			base.OnEntityStay(entity);
			entity.AddForce(new((entity.transform.position - transform.position).normalized, pushForce, pushAcceleration));
		}

		protected override void OnEntityEnter(Entity entity)
		{
			base.OnEntityEnter(entity);
			audMan.PlaySingle(audAcidicEffect);
		}

		[SerializeField]
		internal SoundObject audAcidicEffect;

		[SerializeField]
		internal float pushForce = 7f, pushAcceleration = -5f;
	}
}
