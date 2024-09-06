using GameNetcodeStuff;
using UnityEngine;

namespace IXTweaks.MonoBehaviors
{
    public class InvinsibleEnemyWrapper : MonoBehaviour
    {
        int internalHealth;
        int startHealth = 3;

        void Awake() {
            internalHealth = startHealth;
        }

        public void DamageWrapper(int damage, PlayerControllerB source = null) {
            // Subtract the damage dealt from the wrapper
            internalHealth -= damage;

            // If the internal health is 0, kill this enemy
            if (internalHealth <= 0) {
                GetComponent<EnemyAI>().KillEnemy(true);
                return;
            }

            // Switch on what AI this enemy has
            if (GetComponent<SpringManAI>() != null)  {
                // Check if the coil is retracted
                SpringManAI self = GetComponent<SpringManAI>();
                if (self.setOnCooldown) {
                    self.SetCoilheadOnCooldownServerRpc(false);
                }
                else {
                    source.DamagePlayer(1000, hasDamageSFX: true, callRPC: true, CauseOfDeath.Mauling, 2);
                }

            }
        }
    }
}