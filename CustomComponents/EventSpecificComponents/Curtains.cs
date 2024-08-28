using HarmonyLib;
using System.Collections;
using UnityEngine;

namespace BBTimes.CustomComponents.EventSpecificComponents
{
	public class Curtains : MonoBehaviour
	{
		public void AttachToCell(EnvironmentController ec, Cell cell, Direction dir)
		{
			this.ec = ec;
			this.dir = dir;
			cellA = cell;
			cellB = ec.CellFromPosition(cell.position + dir.ToIntVector2());
		}
		public void Close(bool close)
		{
			ThrowIfNoAttach();

			audMan.PlaySingle(close ? audClose : audOpen);
			renderer.sprite = close ? sprClosed : sprOpen;
			cellA.Mute(dir, close);
			cellB.Mute(dir.GetOpposite(), close);
			collider.enabled = close;

			if (closeTimer != null)
				StopCoroutine(closeTimer);
		}

		public void TimedClose(bool close, float timer)
		{
			ThrowIfNoAttach();
			if (closeTimer != null)
				StopCoroutine(closeTimer);
			closeTimer = StartCoroutine(CloseTimer(close, timer));
		} 

		IEnumerator CloseTimer(bool close, float timer)
		{ 
			while (timer > 0f)
			{
				timer -= Time.deltaTime * ec.EnvironmentTimeScale;
				yield return null;
			}

			Close(close);
			yield break;
		}

		void ThrowIfNoAttach()
		{
			if (cellA == null || cellB == null)
				throw new System.ArgumentNullException("The curtain doesn\'t have a cell pair attached to function");
		}

		Coroutine closeTimer;
		Cell cellA, cellB;
		Direction dir;
		EnvironmentController ec;

		[SerializeField]
		internal Collider collider;

		[SerializeField]
		internal Sprite sprClosed, sprOpen;

		[SerializeField]
		internal SpriteRenderer renderer;

		[SerializeField]
		internal PropagatedAudioManager audMan;

		[SerializeField]
		internal SoundObject audOpen, audClose;
	}
}
