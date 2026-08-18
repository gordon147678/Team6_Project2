using System;
using UnityEngine;

namespace Team6.Project2.ObstaclePacks
{
    [DisallowMultipleComponent]
    public sealed class CheesePickup : MonoBehaviour
    {
        [SerializeField] private AudioClip pickupSound;
        [Range(0f, 1f)]
        [SerializeField] private float pickupSoundVolume = 1f;

        private bool collected;

        public static event Action<CheesePickup> Collected;

        private void OnTriggerEnter(Collider other)
        {
            if (collected || other.attachedRigidbody == null)
                return;

            collected = true;

            if (pickupSound != null)
            {
                AudioSource.PlayClipAtPoint(
                    pickupSound,
                    transform.position,
                    pickupSoundVolume);
            }

            Collected?.Invoke(this);
            Destroy(gameObject);
        }
    }
}
