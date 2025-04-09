using System.Collections;
using UnityEngine;
using BBTimes.CustomContent.RoomFunctions;

namespace BBTimes.CustomContent.Misc
{
    public class IceRinkWater : MonoBehaviour
    {
		readonly EntityOverrider entityOverrider = new();
		IceWaterFunction func;
		EnvironmentController ec;
		

		[SerializeField]
		internal float pullCooldown = 15f, sinkSpeed = 0.65f;

		bool canPull = false;
		Coroutine pullCooldownCor;

		public void Initialize(IceWaterFunction func, EnvironmentController ec)
		{
			this.func = func;
			canPull = true;
			this.ec = ec;
		}
		public void StartPullingCooldown()
		{
			if (pullCooldownCor != null)
				StopCoroutine(pullCooldownCor);
			pullCooldownCor = StartCoroutine(PullCooldown());
		}
		private void OnTriggerEnter(Collider other)
		{
			if (canPull && other.isTrigger)
			{
				Entity component = other.GetComponent<Entity>();
				if (component != null && component.Grounded && component.Override(entityOverrider))
				{
					canPull = false;
					StartCoroutine(Teleport(component));
				}
			}
		}

		private IEnumerator Teleport(Entity subject)
		{
			IceRinkWater newWater = func.GetPotentialSpot(this);
			StartPullingCooldown(); // Start on myself
			if (!newWater)
				newWater = this; // Teleport to itself then
			else
				newWater.StartPullingCooldown(); // Start at my target

			entityOverrider.SetFrozen(true);
			entityOverrider.SetInteractionState(false);
			
			

			var newPos = newWater.transform.position;

			float sinkPercent = 1f;
			subject?.Teleport(transform.position);
			
			while (sinkPercent > 0.2f)
			{
				if (subject == null)
				{
					yield break;
				}
				sinkPercent -= Time.deltaTime * ec.EnvironmentTimeScale * sinkSpeed;
				entityOverrider.SetHeight(subject.InternalHeight * sinkPercent);
				yield return null;
			}
			if (subject == null)
			{
				yield break;
			}
			sinkPercent = 0.2f;
			entityOverrider.SetHeight(subject.InternalHeight * sinkPercent);
			
			subject?.Teleport(newPos);
			
			while (sinkPercent < 1f)
			{
				if (subject == null)
				{
					yield break;
				}
				sinkPercent += Time.deltaTime * ec.EnvironmentTimeScale * sinkSpeed;
				entityOverrider.SetHeight(subject.InternalHeight * sinkPercent);
				yield return null;
			}

			entityOverrider.Release();
			entityOverrider.SetFrozen(false);
			entityOverrider.SetInteractionState(true);

			yield break;
		}

		IEnumerator PullCooldown()
		{
			canPull = false;
			float cooldown = pullCooldown;
			while (cooldown > 0f)
			{
				cooldown -= ec.EnvironmentTimeScale * Time.deltaTime;
				yield return null;
			}
			canPull = true;
		}
	}
}
