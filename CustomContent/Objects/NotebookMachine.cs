using System.Collections.Generic;
using BBTimes.CustomComponents;
using UnityEngine;

namespace BBTimes.CustomContent.Objects
{
    public class NotebookMachine : EnvironmentObject, IClickable<int>, IItemAcceptor
    {
        Notebook linkedNotebook;
        RoomController room;

        bool initialized = false, powered = true, pressed = false;

        public bool IsClickable => initialized && !pressed && powered && linkedNotebook && !linkedNotebook.collected;

        [SerializeField]
        internal MeshRenderer boxRenderer;

        [SerializeField]
        internal Material[] offMat, onMat;

        [SerializeField]
        internal AudioManager audMan;

        [SerializeField]
        internal SoundObject audClick;

        [SerializeField]
        internal Sprite spriteMapIcon;
        readonly public static HashSet<Items> unlockableItems = [];

        public void LinkToNotebook(Notebook notebook, EnvironmentController ec)
        {
            Ec = ec;
            linkedNotebook = notebook;
            notebook.icon.spriteRenderer.sprite = spriteMapIcon;
            room = ec.CellFromPosition(notebook.transform.position).room;
            initialized = true;

            Destroy(notebook.sprite.GetComponent<PickupBob>()); // Overrides the PickupBob
            notebook.sprite.gameObject.AddComponent<OffsettledPickupBob>().offset = 1.25f;

            UpdateState();
            RevealNotebook(false);
        }

        void Update()
        {
            if (!initialized) return;

            if (room.Powered != powered)
            {
                UpdateState();
            }
        }

        void UpdateState()
        {
            powered = room.Powered;

            // Updates electricity state
            boxRenderer.materials = powered ? onMat : offMat;
        }

        public void RevealNotebook(bool reveal)
        {
            if (!linkedNotebook.collected)
            {
                linkedNotebook.sphereCollider.enabled = reveal; // Easiest way to disable Notebook click
                linkedNotebook.sprite.enabled = reveal;
            }
        }

        public void Pressed()
        {
            if (pressed)
                return;

            audMan.PlaySingle(audClick);
            RevealNotebook(true);
            pressed = true;
        }

        public Notebook LinkedNotebook => linkedNotebook;

        public void Clicked(int player)
        {
            if (IsClickable)
                Pressed();
        }

        public void ClickableSighted(int player) { }

        public void ClickableUnsighted(int player) { }

        public bool ClickableHidden() => !IsClickable;


        public bool ClickableRequiresNormalHeight() => true;


        public void InsertItem(PlayerManager player, EnvironmentController ec) =>
            Pressed();

        public bool ItemFits(Items item) => IsClickable && unlockableItems.Contains(item);

    }
}
