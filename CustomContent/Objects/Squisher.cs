using BBTimes.Extensions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BBTimes.CustomContent.Objects
{
	public class Squisher : EnvironmentObject, IButtonReceiver
	{
		public void ButtonPressed(bool val)
		{
			if (!active)
			{
				forceSquish = true;
				cooldown = 0f;
			}
		}

		public void TurnMe(bool on)
		{
			waitingForSquish = on;
			cooldown = Random.Range(5f, 10f);
			if (!on)
			{
				StopAllCoroutines();
				StartCoroutine(ResetPosition());
			}
		}
		
		public void Setup(float speed)
		{
			ogPos = transform.position;
			this.speed = speed;
		}
		
		void Update()
		{
			if (!waitingForSquish)
				return;

			cooldown -= ec.EnvironmentTimeScale * Time.deltaTime;
			if (cooldown < 0f)
			{
				waitingForSquish = false;
				StartCoroutine(SquishSequence());
			}
		}

		void OnTriggerEnter(Collider other)
		{
			if (canSquish && other.isTrigger)
			{
				var e = other.GetComponent<Entity>();
				if (e)
				{
					if (!e.Squished) // Should fix the noclip bug
						e.Squish(12f);
					e.SetFrozen(true);
					squishedEntities.Add(e);
				}
				
			}
		}

		IEnumerator ResetPosition()
		{
			blockCollider.enabled = false;
			ec.BlockAllDirs(ogPos.ZeroOutY(), false);
			canSquish = false;
			audMan.FlushQueue(true);
			Vector3 pos = transform.position;
			float t = 0f;
			while (true) // *squishing*
			{
				t += ec.EnvironmentTimeScale * Time.deltaTime * speed;

				if (t >= 1f)
					break;

				transform.position = Vector3.Lerp(pos, ogPos, t);
				yield return null;
			}
			transform.position = ogPos;
			yield break;
		}

		IEnumerator SquishSequence()
		{
			active = true;
			float cool = forceSquish ? Random.Range(0.5f, 1.5f) : Random.Range(3f, 5f);
			forceSquish = false;

			audMan.QueueAudio(audPrepare);
			audMan.SetLoop(true);

			while (cool > 0f) // preparing to squish
			{
				if (Time.timeScale > 0f)
					transform.position = new(ogPos.x + Random.Range(-0.2f, 0.2f), ogPos.y, ogPos.z + Random.Range(-0.2f, 0.2f));
				cool -= ec.EnvironmentTimeScale * Time.deltaTime;
				yield return null;
			}
			

			audMan.FlushQueue(true);
			audMan.QueueAudio(audRun);
			audMan.SetLoop(true);

			transform.position = ogPos;
			float t = 0;
			Vector3 squishPos = ogPos.ZeroOutY();
			ec.BlockAllDirs(squishPos, true);
			collider.enabled = true;
			canSquish = true;

			while (true) // *squishing*
			{
				t += ec.EnvironmentTimeScale * Time.deltaTime * speed;

				if (t >= 1f)
					break;

				transform.position = Vector3.Lerp(ogPos, squishPos, t);
				yield return null;
			}
			transform.position = squishPos;
			canSquish = false;
			blockCollider.enabled = true; // This should be called after any frozen entity is done
			collider.enabled = false;
			cool = Random.Range(2f, 3.5f);
			audMan.FlushQueue(true);
			audMan.QueueAudio(audHit);

			while (cool > 0f) // stay in the ground for a while
			{
				cool -= ec.EnvironmentTimeScale * Time.deltaTime;
				yield return null;
			}

			blockCollider.enabled = false;
			ec.BlockAllDirs(squishPos, false);
			while (squishedEntities.Count != 0)
			{
				squishedEntities[0].SetFrozen(false);
				squishedEntities.RemoveAt(0);
			}

			audMan.QueueAudio(audPrepare);
			audMan.SetLoop(true);

			t = 0f;
			while (true) // go back up
			{
				t += ec.EnvironmentTimeScale * Time.deltaTime * (speed * 0.5f);

				if (t >= 1f)
					break;

				transform.position = Vector3.Lerp(squishPos, ogPos, t);
				yield return null;
			}
			transform.position = ogPos;
			waitingForSquish = true;
			cooldown = Random.Range(5f, 10f);
			audMan.FlushQueue(true);
			active = false;

			yield break;
		}

		[SerializeField]
		internal BoxCollider collider, blockCollider;

		[SerializeField]
		internal PropagatedAudioManager audMan;

		[SerializeField]
		internal SoundObject audPrepare, audRun, audHit;

		float cooldown = Random.Range(5f, 10f);
		float speed;
		bool waitingForSquish = true, canSquish = false, forceSquish = false, active = false;

		readonly List<Entity> squishedEntities = [];

		Vector3 ogPos;
	}
}
