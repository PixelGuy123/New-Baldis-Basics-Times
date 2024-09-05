using System.Collections;
using UnityEngine;

namespace BBTimes.CustomComponents
{
	public class GenericDoor : EnvironmentObject, IClickable<int>
	{
		public void Open(float timer, bool makeNoise)
		{
			if (openTimer != null)
				StopCoroutine(openTimer);

			openTimer = StartCoroutine(OpenTimer(timer));
			if (makeNoise)
				ec.MakeNoise(transform.position, 1);
		}
		public void Close()
		{
			if (openTimer != null)
				StopCoroutine(openTimer);

			renderer.sprite = closed;
			if (audClose && opened)
				audMan.PlaySingle(audClose);
			opened = false;

			for (int i = 0; i < colliders.Length; i++)
				colliders[i].enabled = true;
		}
		IEnumerator OpenTimer(float timer)
		{
			for (int i = 0; i < colliders.Length; i++)
				colliders[i].enabled = false;


			if (audOpen && !opened)
				audMan.PlaySingle(audOpen);
			renderer.sprite = open;
			opened = true;

			while (timer > 0f)
			{
				timer -= ec.EnvironmentTimeScale * Time.deltaTime;
				yield return null;
			}

			Close();
		}
		public void Clicked(int player) =>
			Open(defaultOpenTimer, true);

		private void OnTriggerEnter(Collider other)
		{
			if (other.isTrigger)
			{
				if (other.CompareTag("NPC") || (other.CompareTag("Player") && other.GetComponent<ActivityModifier>().ForceTrigger))
					Open(defaultOpenTimer, false);
			}
		}

		public void ClickableSighted(int player) { }
		public void ClickableUnsighted(int player) { }
		public bool ClickableHidden() => false;
		public bool ClickableRequiresNormalHeight() => false;

		Coroutine openTimer = null;
		bool opened = false;
		public bool IsOpen => opened;

		[SerializeField]
		internal AudioManager audMan;

		[SerializeField]
		internal SoundObject audOpen, audClose;

		[SerializeField]
		internal SpriteRenderer renderer;

		[SerializeField]
		internal Sprite closed, open;

		[SerializeField]
		internal Collider[] colliders;

		[SerializeField]
		internal float defaultOpenTimer = 3f;

		public float DefaultOpenTimer => defaultOpenTimer;
	}
}
