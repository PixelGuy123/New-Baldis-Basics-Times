using System.Collections;
using System.Collections.Generic;
using BBTimes.CustomContent.CustomItems;
using UnityEngine;

namespace BBTimes.CustomContent.Objects
{
    public class BasketBallCannon : EnvironmentObject, IItemAcceptor
    {
        public bool ItemFits(Items item) => acceptableItems.Contains(item) && !IsDead && !hasAlreadyAccessedItem;

        public void InsertItem(PlayerManager pm, EnvironmentController ec)
        {
            deadCooldown = deadCooldownDuration;
            StartCoroutine(AddDelay(pm));
        }
        public override void LoadingFinished()
        {
            base.LoadingFinished();
            home = ec.CellFromPosition(transform.position).room;
        }

        void Update()
        {
            for (int i = 0; i < basketBalls.Count; i++)
            {
                if (!basketBalls[i])
                    basketBalls.RemoveAt(i--);
            }

            if (deadCooldown > 0f)
            {
                deadCooldown -= Time.deltaTime * ec.EnvironmentTimeScale;
                return;
            }

            if (turning || basketBalls.Count > maxBasketBalls)
                return;

            cooldownToShoot -= Time.deltaTime * ec.EnvironmentTimeScale;
            if (cooldownToShoot < 0f)
            {
                turnCooldown = Random.Range(turnCooldownMin, turnCooldownMax);
                turnOrientation = Random.value <= 0.5f;
                cooldownToShoot += Random.Range(cooldownToShootMin, cooldownToShootMax);
                turning = true;
                StartCoroutine(ShootSequence());
            }
        }

        IEnumerator ShootSequence()
        {
            float speed = Random.Range(turnSpeedMin, turnSpeedMax);
            audMan.QueueAudio(audTurn);
            audMan.SetLoop(true);
            while (turnCooldown > 0f)
            {
                turnCooldown -= Time.deltaTime * ec.EnvironmentTimeScale;
                transform.Rotate(Vector3.up, (turnOrientation ? speed : -speed) * Time.deltaTime * ec.EnvironmentTimeScale);
                if (IsDead)
                    break;
                yield return null;
            }
            audMan.FlushQueue(true);

            if (IsDead)
                yield break;

            float delay = Random.Range(shootDelayMin, shootDelayMax);
            while (delay > 0f)
            {
                delay -= Time.deltaTime * ec.EnvironmentTimeScale;
                yield return null;
            }

            var b = Instantiate(basketPre);
            b.Setup(ec, transform.forward, transform.position + transform.forward * shootForwardDistance + Vector3.up * shootUpDistance, home, basketballSetupScale);
            audMan.PlaySingle(audBoom);
            basketBalls.Add(b);

            Vector3 pos = cannon.transform.localPosition;
            cannon.transform.localPosition -= cannon.transform.forward * cannonRecoilDistance;
            Vector3 shootPos = cannon.transform.localPosition;
            float t = 0;
            while (true)
            {
                t += Time.deltaTime * ec.EnvironmentTimeScale * cannonRecoilSpeed;
                if (t >= 1f)
                    break;
                cannon.transform.localPosition = Vector3.Lerp(shootPos, pos, t);
                yield return null;
            }

            cannon.transform.localPosition = pos;
            turning = false;
            yield break;
        }

        IEnumerator AddDelay(PlayerManager pm)
        {
            yield return null;
            pm.itm.AddItem(basketItem);
        }

        [SerializeField]
        internal ItemObject basketItem;

        [SerializeField]
        internal ITM_Basketball basketPre;

        [SerializeField]
        internal Transform cannon;

        [SerializeField]
        internal PropagatedAudioManager audMan;

        [SerializeField]
        internal SoundObject audBoom, audTurn;

        public static HashSet<Items> acceptableItems = [];

        float cooldownToShoot = 0f, deadCooldown = 0f, turnCooldown;

        bool turning = false, turnOrientation = false, hasAlreadyAccessedItem = false;

        public bool IsDead => deadCooldown > 0f;

        RoomController home;
        readonly List<ITM_Basketball> basketBalls = [];

        // Asked GPT to replace the hardcoded stuff with "Unity inspector" format and... I think I'll start using Headers to divide some properties too lol (10-06-2025)

        [Header("Cannon Timing & Limits")]
        [SerializeField]
        float deadCooldownDuration = 15f;
        [SerializeField]
        int maxBasketBalls = 7;
        [SerializeField]
        float cooldownToShootMin = 3f, cooldownToShootMax = 8f;
        [SerializeField]
        float turnCooldownMin = 2f, turnCooldownMax = 5f;

        [Header("Cannon Shoot Settings")]
        [SerializeField]
        float turnSpeedMin = 9f, turnSpeedMax = 15f;
        [SerializeField]
        float shootDelayMin = 1f, shootDelayMax = 2f;
        [SerializeField]
        float shootForwardDistance = 7f;
        [SerializeField]
        float shootUpDistance = 5f;
        [SerializeField]
        float basketballSetupScale = 0.8f;
        [SerializeField]
        float cannonRecoilDistance = 1.5f;
        [SerializeField]
        float cannonRecoilSpeed = 2f;
    }
}
