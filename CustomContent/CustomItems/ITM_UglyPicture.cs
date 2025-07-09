using System.Collections;
using BBTimes.CustomComponents;
using BBTimes.Extensions;
using PixelInternalAPI.Extensions;
using TMPro;
using UnityEngine;

namespace BBTimes.CustomContent.CustomItems
{
    public class ITM_UglyPicture : Item, IItemPrefab
    {
        public void SetupPrefab()
        {
            canvas = ObjectCreationExtensions.CreateCanvas();
            canvas.name = "UglyCanvas";
            canvas.transform.SetParent(transform);

            image = ObjectCreationExtensions.CreateImage(
                canvas,
                this.GetSprite(1f, "RealUglyPicture.png"), // Tempo color
                true).gameObject;
            image.name = "UglyImage";

            textMesh = ObjectCreationExtensions.CreateTextMeshProUGUI(Color.white);
            textMesh.transform.SetParent(canvas.transform);
            textMesh.transform.localPosition = new(0f, 90f);
            textMesh.rectTransform.sizeDelta = new(400f, 50f);
            textMesh.alignment = TextAlignmentOptions.Center;

            var localizer = textMesh.gameObject.AddComponent<TextLocalizer>();
            localizer.key = "UglyPicture_reaction";

            audCrumple = this.GetSound("crumpling.wav", "Sfx_Crumpledpaper_crumpling", SoundType.Effect, Color.white);
        }

        public void SetupPrefabPost() { }

        public string Name { get; set; }
        public string Category => "items";
        public ItemObject ItmObj { get; set; }

        public override bool Use(PlayerManager pm)
        {
            this.pm = pm;
            canvas.gameObject.SetActive(true);
            canvas.worldCamera = Singleton<CoreGameManager>.Instance.GetCamera(pm.playerNumber).canvasCam;
            StartCoroutine(WaitToCrumble());

            return true;
        }

        IEnumerator WaitToCrumble()
        {
            yield return null;
            pm.itm.AddItem(crumpledPaper); // It is intentionally so fast the player might not even notice the item being added instantly
            yield return new WaitForSecondsRealtime(delayToLeave); // Wait to crumble
            image.SetActive(false);
            Singleton<CoreGameManager>.Instance.audMan.PlaySingle(audCrumple);

            textMesh.gameObject.SetActive(true);
            float t = 0f;
            while (t < textDuration)
            {
                t += pm.ec.EnvironmentTimeScale * Time.deltaTime;
                yield return null;
            }
            t = 0f;
            while (t < alphaDuration)
            {
                t += pm.ec.EnvironmentTimeScale * Time.deltaTime;
                textMesh.color = new(1f, 1f, 1f, Mathf.Lerp(1f, 0f, t / alphaDuration));
                yield return null;
            }

            Destroy(gameObject);
        }

        [SerializeField]
        internal float delayToLeave = 0.002f, alphaDuration = 2.5f, textDuration = 3f;

        [SerializeField]
        internal ItemObject crumpledPaper;

        [SerializeField]
        internal Canvas canvas;

        [SerializeField]
        internal GameObject image;

        [SerializeField]
        internal SoundObject audCrumple;

        [SerializeField]
        internal TextMeshProUGUI textMesh;
    }
}