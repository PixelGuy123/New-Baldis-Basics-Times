using BBTimes.CustomComponents;
using UnityEngine;

namespace BBTimes.CustomContent.Objects
{
    public class NotebookMachine : EnvironmentObject
    {
        Notebook linkedNotebook;
        RoomController room;

        bool initialized = false, powered = true;

        [SerializeField]
        internal MeshRenderer boxRenderer;

        [SerializeField]
        internal GameObject capsuleRenderer;

        [SerializeField]
        internal Material[] offMat, onMat;

        [SerializeField]
        internal Sprite spriteMapIcon;

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

            // Disables the capsule if needed
            capsuleRenderer.SetActive(!powered);

            if (!linkedNotebook.collected)
                linkedNotebook.sphereCollider.enabled = powered; // Easiest way to disable Notebook click
        }

        public Notebook LinkedNotebook => linkedNotebook;
    }
}
