using GameNetcodeStuff;
using UnityEngine;

// NOTICE: Credit to EvaisaDev who originally written this code. It serves as just a nice script to put on objects that I want to be able to throw :)
namespace IXTweaks.MonoBehaviors
{
    public class ThrowableItem : GrabbableObject
    {

        public AnimationCurve itemFallCurve;

        public AnimationCurve itemVerticalFallCurve;

        public AnimationCurve itemVerticalFallCurveNoBounce;

        public RaycastHit itemHit;

        public Ray itemThrowRay;
        public override void ItemActivate(bool used, bool buttonDown = true)
        {
            base.ItemActivate(used, buttonDown);
            if (IsOwner)
            {
                playerHeldBy.DiscardHeldObject(placeObject: true, null, GetItemThrowDestination());
            }
        }

        public override void EquipItem()
        {
            EnableItemMeshes(enable: true);
            isPocketed = false;
        }

        public override void FallWithCurve()
        {
            float magnitude = (startFallingPosition - targetFloorPosition).magnitude;
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(itemProperties.restingRotation.x, transform.eulerAngles.y, itemProperties.restingRotation.z), 14f * Time.deltaTime / magnitude);
            transform.localPosition = Vector3.Lerp(startFallingPosition, targetFloorPosition, itemFallCurve.Evaluate(fallTime));
            if (magnitude > 5f)
            {
                transform.localPosition = Vector3.Lerp(new Vector3(transform.localPosition.x, startFallingPosition.y, transform.localPosition.z), new Vector3(transform.localPosition.x, targetFloorPosition.y, transform.localPosition.z), itemVerticalFallCurveNoBounce.Evaluate(fallTime));
            }
            else
            {
                transform.localPosition = Vector3.Lerp(new Vector3(transform.localPosition.x, startFallingPosition.y, transform.localPosition.z), new Vector3(transform.localPosition.x, targetFloorPosition.y, transform.localPosition.z), itemVerticalFallCurve.Evaluate(fallTime));
            }
            fallTime += Mathf.Abs(Time.deltaTime * 12f / magnitude);
        }


        public override void Update()
        {
            Update();
        }

        public Vector3 GetItemThrowDestination()
        {
            Vector3 position = transform.position;
            Debug.DrawRay(playerHeldBy.gameplayCamera.transform.position, playerHeldBy.gameplayCamera.transform.forward, Color.yellow, 15f);
            itemThrowRay = new Ray(playerHeldBy.gameplayCamera.transform.position, playerHeldBy.gameplayCamera.transform.forward);
            position = ((!Physics.Raycast(itemThrowRay, out itemHit, 12f, StartOfRound.Instance.collidersAndRoomMaskAndDefault)) ? itemThrowRay.GetPoint(10f) : itemThrowRay.GetPoint(itemHit.distance - 0.05f));
            Debug.DrawRay(position, Vector3.down, Color.blue, 15f);

            // check if position is inside a collider, and position below is not
            if (Physics.OverlapSphere(position, 0.02f, StartOfRound.Instance.collidersAndRoomMaskAndDefault).Length > 0)
            {
                if(Physics.OverlapSphere(position + (Vector3.down * 0.05f), 0.02f, StartOfRound.Instance.collidersAndRoomMaskAndDefault).Length == 0)
                {
                    // set new position
                    position += (Vector3.down * 0.05f);
                }
            }

            itemThrowRay = new Ray(position, Vector3.down);
            if (Physics.Raycast(itemThrowRay, out itemHit, 30f, StartOfRound.Instance.collidersAndRoomMaskAndDefault))
            {
                return itemHit.point + Vector3.up * 0.05f;
            }
            return itemThrowRay.GetPoint(30f);
        }
    }
}