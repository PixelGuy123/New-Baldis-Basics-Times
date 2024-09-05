using UnityEngine;

namespace BBTimes.CustomComponents
{
	public class TimedFountain : GenericFountain, IClickable<int>
	{
		public new void Clicked(int player)
		{
			if (disabled) return;

			if (audSip)
				audMan.PlaySingle(audSip);

			Singleton<CoreGameManager>.Instance.GetPlayer(player).plm.AddStamina(
				refillAll ? Singleton<CoreGameManager>.Instance.GetPlayer(player).plm.staminaMax : refillValue, true);

			disabled = true;
			cooldown = usageCooldown;
			renderer.sprite = sprDisabled;
		}
		public new bool ClickableHidden() => disabled;

		void Update()
		{
			if (cooldown > 0f)
				cooldown -= ec.EnvironmentTimeScale * Time.deltaTime;
			else if (disabled)
			{
				renderer.sprite = sprEnabled;
				disabled = false;
			}
			
		}

		float cooldown = 0f;
		bool disabled = false;
		[SerializeField]
		internal SpriteRenderer renderer;

		[SerializeField]
		internal Sprite sprDisabled, sprEnabled;

		[SerializeField]
		internal float usageCooldown = 120f;
	}
}
