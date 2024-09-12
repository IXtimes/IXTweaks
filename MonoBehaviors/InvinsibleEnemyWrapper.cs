using GameNetcodeStuff;
using UnityEngine;
using MoreShipUpgrades.UpgradeComponents.TierUpgrades.Enemies;
using MoreShipUpgrades.Managers;
using System.Linq;
using Unity.Netcode;

namespace IXTweaks.MonoBehaviors
{
    public class InvinsibleEnemyWrapper : MonoBehaviour
    {
        public int internalHealth;

        public void DamageWrapper(int damage, PlayerControllerB source = null) {
            // Subtract the damage dealt from the wrapper
            internalHealth -= damage;
            Debug.Log(gameObject.name + " HP: " + internalHealth);

            // If the internal health is 0, kill this enemy
            if (internalHealth <= 0) {
                // Check if this is a master clay surgeon
                ClaySurgeonAI self = GetComponent<ClaySurgeonAI>();
                /*
                if(self != null && self.isMaster) {
                    // Determine a successor
                    ClaySurgeonAI successor;
                    ClaySurgeonAI[] array = FindObjectsByType<ClaySurgeonAI>(0);
                    int hits = array.Count();
                    int i = 0;
                    if (hits > 1) {
                        do {
                            successor = array[i];
                            i++;
                        } while (successor != self);

                        successor.ChooseMasterSurgeon();
                    }
                }
                */

                if(self)
                    if (MoreShipUpgrades.Misc.Upgrades.BaseUpgrade.GetActiveUpgrade(Hunter.UPGRADE_NAME) && Hunter.CanHarvest("barber")) {
                        ItemManager.Instance.SpawnSample("barber", transform.position);
                }

                GetComponent<EnemyAI>().KillEnemy(true);
                return;
            }

            // Is this a coil head?
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

            // Is this a jester?
            if (GetComponent<JesterAI>() != null)  {
                // Check if the jester is in its retracted form
                JesterAI self = GetComponent<JesterAI>();
                if (self.currentBehaviourStateIndex == 0 || self.currentBehaviourStateIndex == 1) {
                    self.creatureAnimator.SetBool("turningCrank", true);
                    self.currentBehaviourStateIndex = 1;
                    self.popUpTimer = Time.deltaTime * 3f;
                }
            }

            // Is this a clay surgeon?
            if (GetComponent<ClaySurgeonAI>() != null)  {
                // Speed up the surgeons
                /*
                ClaySurgeonAI self = GetComponent<ClaySurgeonAI>();
                if (self.isMaster) {
                    self.currentInterval *= 1.5f;
                    self.beatTimer = self.currentInterval;
                } else {
                    self.master.currentInterval *= 1.5f;
                    self.master.beatTimer = self.master.currentInterval;
                }
                */
            }
        }
    }
}