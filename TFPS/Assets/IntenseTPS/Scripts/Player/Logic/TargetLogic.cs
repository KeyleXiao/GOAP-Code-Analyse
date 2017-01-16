using UnityEngine;

namespace Player
{
    public class TargetLogic : MonoBehaviour
    {
        private float noHitZ = 30f;

        private Vector3 defLocalPos;
        private bool immutedSignX = false;
        private bool immutedSignY = false;
        private float immutedTargetX, immutedTargetY;
        private Vector2 cImmuted;
        private float eps = .02f;
        private PlayerAtts plAtts;
        private WeaponCSMB smbFire;
        private LookIKCSMB smbLook;
        private Transform camTransform;
        private Transform fireRef;

        public bool IsHit { get; private set; }

        private void Start()
        {
            defLocalPos = transform.localPosition;
            GameObject plGo = GameObject.FindGameObjectWithTag("Player");

            if (plGo)
                plAtts = plGo.GetComponent<PlayerAtts>();
            if (plAtts)
            {
                camTransform = plAtts.GetComponent<SetupAndUserInput>().cameraRig;
                fireRef = transform.FindChild("Fire Reference");
                smbFire = plAtts.SmbWeapon;
                smbLook = plAtts.SmbLookIK;
            }

            if (!plGo || !plAtts || !smbFire || !smbLook || !fireRef)
            {
                Debug.Log("needed references not found on :" + ToString());
                Destroy(this);
                return;
            }
        }

        private void Update()
        {
            GunAtt cGunAtt = smbFire.GetCurrentWeaponScript();
            if (cGunAtt && smbFire.IsAiming)
            {
                if (Mathf.Abs(immutedTargetX - cImmuted.x) < eps)
                {
                    immutedTargetX = (immutedSignX ? -1 : 1) * Random.Range(0, smbFire.CFireProps.immutedWeaponSpreadAgentMultiplier.x * cGunAtt.immutedSpreadMax.x);
                    immutedSignX = !immutedSignX;
                }
                if (Mathf.Abs(immutedTargetY - cImmuted.y) < eps)
                {
                    immutedTargetY = (immutedSignY ? -1 : 1) * Random.Range(0, smbFire.CFireProps.immutedWeaponSpreadAgentMultiplier.y * cGunAtt.immutedSpreadMax.y);
                    immutedSignY = !immutedSignY;
                }
                Vector2 immutedTarget = new Vector2(immutedTargetX, immutedTargetY);
                cImmuted = Vector2.Lerp(cImmuted, immutedTarget, Time.deltaTime * smbFire.CFireProps.immutedSpreadChangeSpeed);

                transform.localPosition = Vector3.Lerp(transform.localPosition, defLocalPos + new Vector3(cImmuted.x, cImmuted.y, 0), cGunAtt.spreadRecoverSpeed * smbFire.CFireProps.weaponSpreadRecoverAgentMultiplier);

                Vector3 camPosWoOffset = camTransform.position - camTransform.right * camTransform.GetComponent<PlayerCamera>().defaultCameraOffset.x;
                Vector3 plHeadPos = smbFire.cGunAtt.transform.position;

                float fixOffset = Vector3.Distance(camPosWoOffset, plHeadPos);

                RaycastHit hit;
                if (Physics.Raycast(camTransform.position + camTransform.forward * fixOffset, (-camTransform.position + transform.position).normalized, out hit, 999, smbFire.CFireProps.rayCBulletLayerMask))
                {
                    fireRef.position = hit.point;
                    IsHit = true;
                }
                else
                {
                    fireRef.position = camTransform.position + camTransform.forward * noHitZ;
                    IsHit = false;
                }
            }
        }
    }
}