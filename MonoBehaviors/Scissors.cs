using System.Collections.Generic;
using System.Linq;
using GameNetcodeStuff;
using Unity.Netcode;
using UnityEngine;


namespace IXTweaks.MonoBehaviors
{
    public class Scissors : GrabbableObject {
        float internalTimer = 0f;
        public AudioSource sfx;
        public Animator anim;
        public AudioClip snipSFX;
        public override void ItemActivate(bool used, bool buttonDown = true) {
            base.ItemActivate(used, buttonDown);
            Debug.Log("Successful reg item action");
            Debug.Log(sfx);
            Debug.Log(anim);

            if (!IsOwner)
                return;

            if (internalTimer <= 0f) {
                Debug.Log("Pre snip");
                Snip();
                Debug.Log("Post snip");

                if (IsHost)
                    PlayClientEffectRpc();
                else
                    PlayServerEffectRpc();
                Debug.Log("Post effect");

                // Inc usage timer
                internalTimer += 3f;
            }
        }

        public void Snip() {
            if (playerHeldBy == null)
                return;

            RaycastHit[] objectsHitBySnip = Physics.SphereCastAll(playerHeldBy.gameplayCamera.transform.position + playerHeldBy.gameplayCamera.transform.right * -0.25f, 0.6f, playerHeldBy.gameplayCamera.transform.forward, 1.5f, 1084754248);
            RaycastHit[] hits = Physics.SphereCastAll(playerHeldBy.gameplayCamera.transform.position + playerHeldBy.gameplayCamera.transform.right * -0.25f, 0.6f, playerHeldBy.gameplayCamera.transform.forward, 1.5f, 1084754248);
            List<RaycastHit> objectsHitBySnipList = objectsHitBySnip.OrderBy((RaycastHit x) => x.distance).ToList();
            List<EnemyAI> list = new List<EnemyAI>();
            Debug.Log(string.Join(", ", objectsHitBySnipList.Select(hit => hit.transform.gameObject.name).ToList()));
            Debug.Log(string.Join(", ", hits.Select(hit => hit.transform.gameObject.name).ToList()));
            IHittable hittable;
        
            for(int i = 0; i < objectsHitBySnipList.Count; i++) {
                RaycastHit val = objectsHitBySnipList[i];
                val.transform.TryGetComponent<IHittable>(out hittable);
                EnemyAICollisionDetect component = val.transform.GetComponent<EnemyAICollisionDetect>();
                if(component != null) {
                    hittable.Hit(10, val.transform.forward, playerHeldBy, playHitSFX: true, 1);
                }

                PlayerControllerB other = val.transform.GetComponent<PlayerControllerB>();
                Debug.Log(other.gameObject.name + " =? " + playerHeldBy.gameObject.name);
                if (other != null && other != playerHeldBy) {
                    Debug.Log("Kill other player");
                    other.DamagePlayer(1000, true, true, CauseOfDeath.Snipped, 7, false, Vector3.up * 14f);
                }
            }
        }

        [ServerRpc]
        public void PlayServerEffectRpc() {
            PlayClientEffectRpc();
        }

        [ClientRpc]
        public void PlayClientEffectRpc() {
            anim.Play("Base Layer.Snip");
            anim.SetTrigger("Snip");

            sfx.PlayOneShot(snipSFX);
            WalkieTalkie.TransmitOneShotAudio(sfx, snipSFX, 100f);
            RoundManager.Instance.PlayAudibleNoise(transform.position, 10f, 100f, 0, isInElevator && StartOfRound.Instance.hangarDoorsClosed);
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