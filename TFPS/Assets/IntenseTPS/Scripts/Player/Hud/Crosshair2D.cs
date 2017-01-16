using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Player
{
    public class Crosshair2D : MonoBehaviour
    {
        public Sprite defaultCrosshair2D;
        public int defaultCrosshairCount = 4;

        [Space]
        public float maxSpace = 50f;

        public float spaceMultiplierPerShot = .2f;
        public float spaceDecreaseSpeedMultiplier = .3f;

        [Space]
        public float fadeInSpeed = .5f;

        public float fadeOutSpeed = .5f;

        [Range(0, 1)]
        public float maxFadeInAlpha = 1f;

        [Space]
        public float crosshairScale = 10f;

        public float crosshairStartSpace = 5f;
        public float immutedSpaceMultiplier = 1f;

        private WeaponCSMB smb;
        private Sprite cCrosshairSprite;
        private float alphaTarget = 0;
        private float cAlpha = 0;
        private bool chEnabled = false;
        private List<Image> images;
        private int cCrosshairCount;
        private float additionalFireSpace;
        private float cImmutedSpread;
        private float gunRecoverSpeed;

        private void Start()
        {
            if (GameObject.FindGameObjectWithTag("Player"))
                if (GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerAtts>())
                    smb = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerAtts>().SmbWeapon;

            if (smb == null)
            {
                gameObject.SetActive(false);
                Debug.Log("Needed reference not found on:" + ToString());
                return;
            }

            smb.Events.onWeaponFire += OnWeaponFire;
            smb.Events.onWeaponPullOut += OnWeaponPullOut;
            smb.Events.onWeaponHolster += OnWeaponHolster;
            smb.Events.onDropWeapon += OnWeaponDrop;
            smb.Events.onWeaponAim += OnWeaponAim;
            smb.Events.onWeaponUnAim += OnWeaponUnAim;

            cCrosshairCount = defaultCrosshairCount;

            images = new List<Image>();

            if (cCrosshairCount <= 1)
                additionalFireSpace = 0;
        }

        private void Update()
        {
            if (!cCrosshairSprite || !chEnabled)
                return;

            if (cAlpha < .01f && alphaTarget == 0)
            {
                SetAlphaColor(0);
                chEnabled = false;
                return;
            }

            cAlpha = Mathf.Lerp(cAlpha, alphaTarget, (alphaTarget > .5f ? fadeInSpeed : fadeOutSpeed) * Time.deltaTime);
            float space = gunRecoverSpeed * spaceDecreaseSpeedMultiplier;
            space = space > maxSpace ? maxSpace : space;
            additionalFireSpace = Mathf.Lerp(additionalFireSpace, 0, Time.deltaTime * space);
            SetAlphaColor(cAlpha > maxFadeInAlpha ? maxFadeInAlpha : (cAlpha > .99f ? 1 : cAlpha));
            SetPosOfAllImages();
        }

        private void SetAlphaColor(float alpha)
        {
            foreach (var img in images)
            {
                Color clr = img.color;
                clr = new Color(clr.r, clr.g, clr.b, alpha);
                img.color = clr;
            }
        }

        private void DestroyImgs()
        {
            if (images != null && images.Count > 0)
                foreach (var img in images)
                {
                    Destroy(img.gameObject);
                }
            images.Clear();
        }

        private void CreateImgs()
        {
            if (!cCrosshairSprite)
                return;

            for (int i = 0; i < cCrosshairCount; i++)
            {
                GameObject go = new GameObject();
                go.transform.SetParent(transform);
                go.AddComponent<Image>();
                go.name = "Crosshair:" + i;
                go.GetComponent<Image>().sprite = cCrosshairSprite;
                images.Add(go.GetComponent<Image>());

                go.transform.localPosition = Vector3.zero;
                go.GetComponent<RectTransform>().sizeDelta = new Vector2(crosshairScale, crosshairScale);
                SetPosOfImg(go.transform, i);
            }
        }

        private void SetPosOfAllImages()
        {
            if (images.Count == 1)
            {
                return;
            }

            for (int i = 0; i < cCrosshairCount; i++)
            {
                SetPosOfImg(images[i].transform, i);
            }
        }

        private void SetPosOfImg(Transform imgTransform, int i)
        {
            Quaternion quat = Quaternion.AngleAxis(i * 360 / (cCrosshairCount <= 0 ? 1 : cCrosshairCount), Vector3.forward);
            imgTransform.localRotation = quat;
            imgTransform.position = transform.position + transform.TransformDirection(quat * Vector3.up) * (crosshairStartSpace + cImmutedSpread * immutedSpaceMultiplier + additionalFireSpace);
        }

        private void OnWeaponFire(GunAtt gunAtt)
        {
            if (cCrosshairCount <= 1)
            {
                additionalFireSpace = 0;
            }
            else
            {
                additionalFireSpace += gunAtt.spreadAmount * spaceMultiplierPerShot;
            }
        }

        private void OnWeaponPullOut(GunAtt gunAtt)
        {
            gunRecoverSpeed = gunAtt.spreadRecoverSpeed;
            if (gunAtt.crosshair2D)
            {
                cCrosshairSprite = gunAtt.crosshair2D;
                cCrosshairCount = gunAtt.crosshairCount;
            }
            else
            {
                cCrosshairSprite = defaultCrosshair2D;
                cCrosshairCount = defaultCrosshairCount;
            }
            cImmutedSpread = gunAtt.immutedSpreadMax.x > gunAtt.immutedSpreadMax.y ? gunAtt.immutedSpreadMax.x : gunAtt.immutedSpreadMax.y;

            if (cCrosshairCount <= 1)
            {
                cImmutedSpread = 0;

                additionalFireSpace = 0;
            }
            CreateImgs();
            cAlpha = 0;
            chEnabled = false;
            SetAlphaColor(0);
        }

        private void OnWeaponHolster(GunAtt gunAtt)
        {
            cCrosshairSprite = defaultCrosshair2D;
            cCrosshairCount = defaultCrosshairCount;
            DestroyImgs();
            chEnabled = false;
        }
        private void OnWeaponDrop(GunAtt g1, GunAtt g2)
        {
            OnWeaponHolster(g2);
        }

        private void OnWeaponAim(GunAtt gunAtt)
        {
            chEnabled = true;
            additionalFireSpace = 0;
            alphaTarget = 1;
        }

        private void OnWeaponUnAim(GunAtt gunAtt)
        {
            alphaTarget = 0;
        }
    }
}