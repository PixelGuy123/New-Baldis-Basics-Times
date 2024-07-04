using HarmonyLib;
using System.Collections;
using UnityEngine;

namespace BBTimes.CustomComponents.EventSpecificComponents
{
	public class Curtains : MonoBehaviour
	{
		public void AttachToWindow(Window window) =>
			attachedWindow = window;
		public void Close(bool close)
		{
			ThrowIfNoAttach();

			audMan.PlaySingle(close ? audClose : audOpen);
			renderers.Do(x => x.sprite = close ? sprClosed : sprOpen);
			collider.enabled = close;
			attachedWindow.aTile.Block(attachedWindow.direction, close);
			attachedWindow.bTile.Block(attachedWindow.direction.GetOpposite(), close);

			StopAllCoroutines();
		}

		public void TimedClose(bool close, float timer)
		{
			ThrowIfNoAttach();
			StartCoroutine(CloseTimer(close, timer));
		}

		IEnumerator CloseTimer(bool close, float timer)
		{ 
			while (timer > 0f)
			{
				timer -= Time.deltaTime * attachedWindow.ec.EnvironmentTimeScale;
				yield return null;
			}

			Close(close);
			yield break;
		}

		void ThrowIfNoAttach()
		{
			if (!attachedWindow)
				throw new System.ArgumentNullException("The curtain doesn\'t have a window attached to function");
		}

		Window attachedWindow;

		[SerializeField]
		internal Sprite sprClosed, sprOpen;

		[SerializeField]
		internal SpriteRenderer[] renderers;

		[SerializeField]
		internal BoxCollider collider;

		[SerializeField]
		internal PropagatedAudioManager audMan;

		[SerializeField]
		internal SoundObject audOpen, audClose;
	}
}
