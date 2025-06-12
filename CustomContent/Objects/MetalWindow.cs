using System.Collections.Generic;
using PixelInternalAPI.Extensions;
using UnityEngine;

namespace BBTimes.CustomContent.Objects;

public class MetalWindow : MonoBehaviour, IItemAcceptor
{
    [SerializeField]
    public Window window;

    [SerializeField]
    internal float minMagForce = 7f, maxMagForce = 15f, minTorque = 5f, maxTorque = 15f;

    public bool ItemFits(Items item) => window && acceptableItems.Contains(item);
    public void InsertItem(PlayerManager pm, EnvironmentController ec)
    {
        if (!window)
            return;

        // Basically breaks the window without actually calling Break()
        window.Open(true, false);
        window.aTile.Mute(window.direction, block: false);
        window.bTile.Mute(window.direction.GetOpposite(), block: false);
        window.aMapTile.SpriteRenderer.sprite = window.mapOpenSprite;
        window.bMapTile.SpriteRenderer.sprite = window.mapOpenSprite;

        for (int i = 0; i < window.colliders.Length; i++)
            Destroy(window.colliders[i]); // Destroy all colliders from the window

        var rb = window.gameObject.AddComponent<Rigidbody>();
        // Add random force and torque to make the window fly off
        rb.mass = 2f;

        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        // Flies off in a random direction
        Vector3 randomDir = (Vector3.up + Random.onUnitSphere).normalized;
        float forceMagnitude = Random.Range(minMagForce, maxMagForce);
        rb.AddForce(randomDir * forceMagnitude, ForceMode.Impulse);

        // Adds a random rotation to the window
        Vector3 randomTorque = Random.insideUnitSphere * Random.Range(minTorque, maxTorque);
        rb.AddTorque(randomTorque, ForceMode.Impulse);

        Destroy(window); // Destroys the Window itself, since it won't re-appear anymore
        Destroy(this); // Destroy this component since it is useless now
    }

    public static HashSet<Items> acceptableItems = [];
}