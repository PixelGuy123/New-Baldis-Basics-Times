using UnityEngine;
using System.Collections.Generic;
using HarmonyLib;

namespace BBTimes.CustomContent.Objects
{
	public class SecurityCamera : EnvironmentObject
	{
		public void TurnMe(bool on)
		{
			if (!on)
			{
				audMan.FlushQueue(true);
				spotCooldown = defaultSpotCool;
				alarmTime = 0f;
				wasAlarming = false;
				SetIndicatorsToColor(Color.clear);
			}
			else
				SetIndicatorsToColor(Color.blue);

			collider.enabled = on;
			isCameraOn = on;
		}
		public void Setup(List<Direction> dirs, int maximumDistance)
		{
			nextDirections = dirs;
			maxDistance = maximumDistance;
			basePos = ec.CellFromPosition(transform.position).position;
		}

		void Start() =>
			UpdateVision();

		void Update()
		{
			if (!isCameraOn)
				return;
			
			if (alarmTime > 0f)
			{
				alarmTime -= ec.EnvironmentTimeScale * Time.deltaTime;
				return;
			}
			else if (sawPlayer)
			{
				spotCooldown -= ec.EnvironmentTimeScale * Time.deltaTime;
				SetIndicatorsToColor(spotCooldown < 2f ? Color.red : Color.yellow);
				if (spotCooldown < 0f)
				{
					alarmTime = 15f;
					audMan.FlushQueue(true);
					audMan.QueueAudio(audAlarm);
					audMan.SetLoop(true);
					ec.MakeNoise(transform.position, 112);
					sawPlayer = false;
					spotCooldown = defaultSpotCool;
					wasAlarming = true;
					Singleton<BaseGameManager>.Instance.AngerBaldi(2f);
				}
				return;
			}

			if (wasAlarming)
			{
				audMan.FlushQueue(true);
				SetIndicatorsToColor(Color.blue);
				wasAlarming = false;
			}
			

			cooldown -= ec.EnvironmentTimeScale * Time.deltaTime;
			if (cooldown < 0f)
			{
				cooldown = Random.Range(minTurnCool, maxTurnCool);
				dirIndex = ++dirIndex % nextDirections.Count;
				UpdateVision();
			}

			
		}

		void UpdateVision()
		{
			audMan.PlaySingle(audTurn);
			while (indicators.Count != 0)
			{
				Destroy(indicators[0].gameObject);
				indicators.RemoveAt(0);
			}

			transform.rotation = Quaternion.Euler(0f, nextDirections[dirIndex].ToDegrees(), 0f);
			IntVector2 pos = basePos;
			int size = 0;
			for (int i = 0; i < maxDistance; i++)
			{
				pos += nextDirections[dirIndex].ToIntVector2();
				var cell = ec.CellFromPosition(pos);

				var indicator = Instantiate(visionIndicatorPre, transform, true);
				indicator.transform.position = cell.FloorWorldPosition + Vector3.up * 0.1f;
				indicator.transform.rotation = Quaternion.Euler(90f, nextDirections[dirIndex].ToDegrees(), 0f);
				indicator.color = currentColor;
				indicators.Add(indicator);

				size++;

				if (cell.HasWallInDirection(nextDirections[dirIndex]))
					break;
			}

			collider.size = new Vector3(3f, 10f, size * 9f);
			collider.center = new Vector3(0f, 1f, size * 6.7f);
		}

		void OnTriggerEnter(Collider other)
		{
			if (alarmTime > 0f)
				return;

			if (other.isTrigger && other.CompareTag("Player"))
			{
				var pm = other.GetComponent<PlayerManager>();
				if (pm && !pm.Tagged &&! pm.Invisible)
				{
					SetIndicatorsToColor(Color.yellow);
					sawPlayer = true;
				}
			}
			
		}

		void OnTriggerStay(Collider other)
		{
			if (wasAlarming || !sawPlayer)
				return;

			if (other.isTrigger && other.CompareTag("Player"))
			{
				var pm = other.GetComponent<PlayerManager>();
				if (pm && ( pm.Tagged || pm.Invisible))
				{
					sawPlayer = false;
					SetIndicatorsToColor(Color.blue);
				}
			}
		}

		void OnTriggerExit(Collider other)
		{
			if (wasAlarming) return;

			if (other.isTrigger && other.CompareTag("Player"))
			{
				spotCooldown = defaultSpotCool;
				sawPlayer = false;
				SetIndicatorsToColor(Color.blue);
			}
			
		}

		void SetIndicatorsToColor(Color color)
		{
			if (color != currentColor)
			{
				currentColor = color;
				indicators.Do(x => x.color = color);
				audMan.PlaySingle(audDetect);
			}
		}

		List<Direction> nextDirections;

		int dirIndex = 0, maxDistance;

		IntVector2 basePos;

		float cooldown = Random.Range(minTurnCool, maxTurnCool), spotCooldown = defaultSpotCool, alarmTime = 0f;

		bool sawPlayer = false, wasAlarming = false, isCameraOn = true;

		Color currentColor = Color.blue;

		const float maxTurnCool = 30f, minTurnCool = 15f, defaultSpotCool = 4f;

		[SerializeField]
		internal SpriteRenderer visionIndicatorPre;

		[SerializeField]
		internal BoxCollider collider;

		[SerializeField]
		internal PropagatedAudioManager audMan;

		[SerializeField]
		internal SoundObject audTurn, audDetect, audAlarm;

		readonly List<SpriteRenderer> indicators = [];
	}
}
