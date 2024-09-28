using GameNetcodeStuff;
using Unity.Netcode;
using UnityEngine;

namespace IXTweaks.MonoBehaviors
{
    public class FakeSprayItem : GrabbableObject
    {

        float remainingSpray = 100f;
        float internalTimer = 0f;
        public Transform fullSprayEffect;
        public ParticleSystem emptySprayEffect;
        public AudioSource spraySFXr;
        public AudioClip spraySFX;
        public AudioClip emptySFX;
        public override void ItemActivate(bool used, bool buttonDown = true) {
            base.ItemActivate(used, buttonDown);

            if (GameNetworkManager.Instance.localPlayerController == null) 
                return;

            if (internalTimer <= 0f) {
                // Check if we have spray remaining
                if(remainingSpray > 0f) {
                    // Play all of the particle systems within the full spray effect
                    foreach(Transform child in fullSprayEffect) {
                        child.GetComponent<ParticleSystem>().Play();
                    }

                    // Play sfx off of SFXer
                    spraySFXr.PlayOneShot(spraySFX);
                    WalkieTalkie.TransmitOneShotAudio(spraySFXr, spraySFX, 100f);
                    RoundManager.Instance.PlayAudibleNoise(transform.position, 10f, 100f, 0, isInElevator && StartOfRound.Instance.hangarDoorsClosed);

                    // Decrease spray by 5%
                    remainingSpray -= 5f;
                }
                else {
                    // Otherwise play empty effects
                    emptySprayEffect.Play();
                    spraySFXr.PlayOneShot(emptySFX);
                }

                // Inc usage timer
                internalTimer += 1f;
            }
        }

        public override void Update()
        {
            base.Update();

            // Decrease internal timer if its above 0
            if(internalTimer > 0f)
                internalTimer -= Time.deltaTime;
        }
    }
}