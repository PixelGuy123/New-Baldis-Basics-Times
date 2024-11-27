using BBTimes.Extensions;
using System.Collections;
using UnityEngine;

namespace BBTimes.CustomContent.Objects
{
	public class Vase : EnvironmentObject
	{
		void OnTriggerEnter(Collider other)
		{
			if (IsBroken) return;

			if (other.isTrigger && (other.CompareTag("NPC") || other.CompareTag("Player")))
			{
				var npc = other.GetComponent<NPC>();
				if (npc)
				{
					if (npc.IsAPrincipal())
						return; // IF it is a principal, it shouldn't be broken

					npc.SetGuilt(ruleBreakTime, "breakingproperty");
					StartCoroutine(TemporarilyFreezeNPC(npc));
				}
				IsBroken = true;
				audMan.PlaySingle(audBreak);
				renderer.sprite = sprBroken;
				ec.CallOutPrincipals(transform.position, whistleCall:false);
				other.GetComponent<PlayerManager>()?.RuleBreak("breakingproperty", ruleBreakTime, 0.5f);
				
				ec.MakeNoise(transform.position, 35); // Not so high because principal can just take you to detention
			}
		}

		IEnumerator TemporarilyFreezeNPC(NPC npc)
		{
			npc?.Navigator.Am.moveMods.Add(moveMod);
			float timer = ruleBreakTime;
			while (timer > 0f)
			{
				timer -= ec.EnvironmentTimeScale * Time.deltaTime;
				yield return null;
			}

			npc?.Navigator.Am.moveMods.Remove(moveMod);
		}

		public bool IsBroken { get; protected set; }

		readonly MovementModifier moveMod = new(Vector3.zero, 0f);

		[SerializeField]
		internal PropagatedAudioManager audMan;

		[SerializeField]
		internal SoundObject audBreak;

		[SerializeField]
		internal SpriteRenderer renderer;

		[SerializeField]
		internal Sprite sprBroken;

		[SerializeField]
		float ruleBreakTime = 5f;
	}
}
