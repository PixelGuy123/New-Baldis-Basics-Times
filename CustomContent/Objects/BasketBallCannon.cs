using BBTimes.CustomContent.CustomItems;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BBTimes.CustomContent.Objects
{
    public class BasketBallCannon : EnvironmentObject
    {
        public override void LoadingFinished()
        {
            base.LoadingFinished();
            home = ec.CellFromPosition(transform.position).room;
        }

        void Update()
        {
            for (int i = 0; i < basketBalls.Count; i++)
                if (!basketBalls[i])
                    basketBalls.RemoveAt(i--);

            if (turning || basketBalls.Count > 7)
                return;

            cooldownToShoot -= Time.deltaTime * ec.EnvironmentTimeScale;
            if (cooldownToShoot < 0f)
            {
                turnCooldown = Random.Range(2f, 5f);
                turnOrientation = Random.value <= 0.5f;
                cooldownToShoot += Random.Range(3f, 8f);
                turning = true;
                StartCoroutine(ShootSequence());
            }
        }

        IEnumerator ShootSequence()
        {
            float speed = Random.Range(9f, 15f);
            audMan.QueueAudio(audTurn);
            audMan.SetLoop(true);
            while (turnCooldown > 0f)
            {
                turnCooldown -= Time.deltaTime * ec.EnvironmentTimeScale;
                transform.Rotate(Vector3.up, (turnOrientation ? speed : -speed) * Time.deltaTime * ec.EnvironmentTimeScale);
                yield return null;
            }
            audMan.FlushQueue(true);

            float delay = Random.Range(1f, 2f);
            while (delay > 0f)
            {
                delay -= Time.deltaTime * ec.EnvironmentTimeScale;
                yield return null;
            }

            var b = Instantiate(basketPre);
            b.Setup(ec, transform.forward, transform.position + transform.forward * 7f + Vector3.up * 5f, home, 0.8f);
            audMan.PlaySingle(audBoom);
            basketBalls.Add(b);

            Vector3 pos = cannon.transform.localPosition;
            cannon.transform.localPosition -= cannon.transform.forward * 1.5f;
            Vector3 shootPos = cannon.transform.localPosition;
            float t = 0;
            while (true)
            {
                t += Time.deltaTime * ec.EnvironmentTimeScale * 2f;
                if (t >= 1f)
                    break;
                cannon.transform.localPosition = Vector3.Lerp(shootPos, pos, t);
                yield return null;
            }

            cannon.transform.localPosition = pos;
            turning = false;
            yield break;
        }


        [SerializeField]
        internal ITM_Basketball basketPre;

        [SerializeField]
        internal Transform cannon;

        [SerializeField]
        internal PropagatedAudioManager audMan;

        [SerializeField]
        internal SoundObject audBoom, audTurn;

        float cooldownToShoot = 0f, turnCooldown;

        bool turning = false, turnOrientation = false;

        RoomController home;
        readonly List<ITM_Basketball> basketBalls = [];
    }
}
