using System.Collections.Generic;
using BBTimes.Extensions;
using UnityEngine;

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
				SetIndicatorsToColor(idleColor);

			collider.enabled = on;
			isCameraOn = on;
		}
		public void Setup(List<Direction> dirs, int maximumDistance)
		{
			nextDirections = dirs;
			maxDistance = maximumDistance;
			basePos = ec.CellFromPosition(transform.position).position;

			cooldown = Random.Range(minTurnCool, maxTurnCool);
			spotCooldown = defaultSpotCool;
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
			else if (SawRuleBreaker)
			{
				spotCooldown -= ec.EnvironmentTimeScale * Time.deltaTime;
				SetIndicatorsToColor(spotCooldown < minimumRedSpotThreshold ? detectedColor : suspiciousColor);
				if (spotCooldown < 0f)
				{
					// Set an alarm timer and trigger it
					alarmTime = 15f;
					wasAlarming = true;

					// Alarm
					audMan.FlushQueue(true);
					audMan.QueueAudio(audAlarm);
					audMan.SetLoop(true);

					// Reset rule breaker thing
					foreach (var ruleBreaker in caughtRuleBreakers) // Penalize everyone's guilt in the camera's sight
					{
						if (ruleBreaker.CompareTag("NPC") && ruleBreaker.TryGetComponent<NPC>(out var npc))
							npc.SetGuilt(npc.guiltTime + additionalGuiltTimePenalty, npc.BrokenRule);
						if (ruleBreaker.CompareTag("Player") && ruleBreaker.TryGetComponent<PlayerManager>(out var player))
							player.RuleBreak(player.ruleBreak, player.guiltTime + additionalGuiltTimePenalty, player.GuiltySensitivity);
					}
					caughtRuleBreakers.Clear();
					spotCooldown = defaultSpotCool;

					// Make noise
					if (SawPlayerBreakingRules)
					{
						ec.MakeNoise(transform.position, noiseValue);
						Singleton<BaseGameManager>.Instance.AngerBaldi(angerValue);
					}

					ec.CallOutPrincipals(transform.position);
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
			RoomController myRoom = ec.CellFromPosition(pos).room;
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

				if (cell.HasWallInDirection(nextDirections[dirIndex]) ||
					!ec.CellFromPosition(pos + nextDirections[dirIndex].ToIntVector2()).TileMatches(myRoom))
					break;
			}

			collider.size = new Vector3(3f, 10f, size * 9f);
			collider.center = new Vector3(0f, 1f, (5 * collider.size.z / 9f) + 5f); // Linear function lol
		}

		void OnTriggerEnter(Collider other)
		{
			if (alarmTime > 0f)
				return;

			if (other.isTrigger)
			{
				if (other.CompareTag("Player"))
				{
					var pm = other.GetComponent<PlayerManager>();
					if (pm && !caughtRuleBreakers.Contains(pm.plm.Entity) && // Is not registered already
					!pm.Tagged && !pm.Invisible && // Is visible and normal
					pm.Disobeying) // IS breaking rules
					{
						caughtRuleBreakers.Add(pm.plm.Entity);
						spottedPlayersBreakingRules++;
					}
				}

				if (other.CompareTag("NPC"))
				{
					var npc = other.GetComponent<NPC>();
					if (npc && !caughtRuleBreakers.Contains(npc.Entity) && npc.Disobeying) // Checks whether they are still set as "caughtable"
					{
						caughtRuleBreakers.Add(npc.Entity);
					}
				}

				if (!SawRuleBreaker) // In the end, to make sure it resets properly
					SetIndicatorsToColor(suspiciousColor);
			}

		}

		void OnTriggerStay(Collider other)
		{
			if (wasAlarming || !SawRuleBreaker)
				return;

			if (other.isTrigger)
			{
				if (other.CompareTag("Player"))
				{
					var pm = other.GetComponent<PlayerManager>();
					if (pm && caughtRuleBreakers.Contains(pm.plm.Entity) && (pm.Tagged || pm.Invisible || !pm.Disobeying)) // Checks whether they are still set as "caughtable"
					{
						if (--spottedPlayersBreakingRules < 0)
							spottedPlayersBreakingRules = 0;
						caughtRuleBreakers.Remove(pm.plm.Entity);
					}
				}

				if (other.CompareTag("NPC"))
				{
					var npc = other.GetComponent<NPC>();
					if (npc && caughtRuleBreakers.Contains(npc.Entity) && !npc.Disobeying) // Checks whether they are still set as "caughtable"
						caughtRuleBreakers.Remove(npc.Entity);

				}

				if (!SawRuleBreaker) // In the end, to make sure it resets properly
					SetIndicatorsToColor(idleColor);
			}
		}

		void OnTriggerExit(Collider other)
		{
			if (wasAlarming) return;

			if (other.isTrigger && other.TryGetComponent<Entity>(out var e))
			{
				caughtRuleBreakers.Remove(e);
				if (other.CompareTag("Player"))
				{
					if (--spottedPlayersBreakingRules < 0)
						spottedPlayersBreakingRules = 0;
				}
			}

			if (!SawRuleBreaker) // In the end, to make sure it resets properly
			{
				SetIndicatorsToColor(idleColor);
				spotCooldown = defaultSpotCool;
			}
		}

		void SetIndicatorsToColor(Color color)
		{
			if (color != currentColor)
			{
				currentColor = color;
				indicators.ForEach(x => x.color = color);
				audMan.PlaySingle(audDetect);
			}
		}

		List<Direction> nextDirections;

		int dirIndex = 0, maxDistance;

		IntVector2 basePos;

		float cooldown, spotCooldown, alarmTime = 0f;
		int spottedPlayersBreakingRules = 0;
		bool wasAlarming = false, isCameraOn = true;

		Color currentColor = Color.blue;

		[SerializeField]
		internal float maxTurnCool = 30f, minTurnCool = 15f, defaultSpotCool = 2.5f, minimumRedSpotThreshold = 1.25f,
		angerValue = 2f, additionalGuiltTimePenalty = 7f;

		[SerializeField]
		internal int noiseValue = 81;

		[SerializeField]
		internal SpriteRenderer visionIndicatorPre;

		[SerializeField]
		internal BoxCollider collider;

		[SerializeField]
		internal PropagatedAudioManager audMan;

		[SerializeField]
		internal SoundObject audTurn, audDetect, audAlarm;

		[SerializeField]
		internal Color idleColor = Color.blue, suspiciousColor = Color.yellow, detectedColor = Color.red;

		public bool SawRuleBreaker => caughtRuleBreakers.Count != 0;
		public bool SawPlayerBreakingRules => spottedPlayersBreakingRules != 0;

		readonly List<SpriteRenderer> indicators = [];
		readonly HashSet<Entity> caughtRuleBreakers = [];
	}
}
