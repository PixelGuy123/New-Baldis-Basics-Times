using MTM101BaldAPI.Registers;
using PixelInternalAPI.Extensions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BBTimes.CustomContent.CustomItems
{
	public class ITM_ThrowableTeleporter : Item
	{
		public override bool Use(PlayerManager pm)
		{
			Throw(pm.transform.position, Singleton<CoreGameManager>.Instance.GetCamera(pm.playerNumber).transform.forward, pm.ec);
			return true;
		}

		public void Throw(Vector3 pos, Vector3 dir, EnvironmentController ec)
		{
			this.ec = ec;
			audMan.PlaySingle(audThrow);
			entity.Initialize(ec, pos);
			entity.AddForce(new(dir, 9f, -7f));

			StartCoroutine(ThrowAnimation());
		}

		IEnumerator ThrowAnimation()
		{
			float height = entity.InternalHeight;
			float time = 0f;

			while (true)
			{
				time += ec.EnvironmentTimeScale * Time.deltaTime;
				entity.SetHeight(height + GenericExtensions.QuadraticEquation(time, -0.5f, 1, 0));
				if (time >= 2f)
				{
					entity.SetHeight(height);
					break;
				}
				yield return null;
			}

			float cooldown = Random.Range(5f, 10f);
			while (cooldown > 0f)
			{
				cooldown -= ec.EnvironmentTimeScale * Time.deltaTime;
				yield return null;
			}

			List<NPC> npcs = [];
			for (int i = 0; i < ec.Npcs.Count; i++)
			{
				if (ec.Npcs[i] && ec.Npcs[i].Navigator && ec.Npcs[i].Navigator.isActiveAndEnabled && ec.Npcs[i].GetMeta().flags.HasFlag(NPCFlags.Standard))
					npcs.Add(ec.Npcs[i]);
			}

			if (npcs.Count != 0)
			{
				audMan.PlaySingle(audTeleport);
				npcs[Random.Range(0, npcs.Count)].Navigator.Entity.Teleport(transform.position);
			}

			cooldown = Random.Range(3f, 6f);
			while (audMan.AnyAudioIsPlaying || cooldown > 0f)
			{
				cooldown -= ec.EnvironmentTimeScale * Time.deltaTime;
				yield return null;
			}

			Destroy(gameObject);

			yield break;
		}

		EnvironmentController ec;

		Dictionary<Entity, MovementModifier> touchedEntities = [];

		[SerializeField]
		internal Entity entity;

		[SerializeField]
		internal PropagatedAudioManager audMan;

		[SerializeField]
		internal SoundObject audThrow, audTeleport;

		[SerializeField]
		internal float maxForce = 55f;


	}
}
