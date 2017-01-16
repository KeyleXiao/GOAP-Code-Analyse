using UnityEngine;

namespace Player
{
    public class HudEventCatcher : MonoBehaviour
    {
        public delegate void OnZeroAlpha();

        public event OnZeroAlpha onZeroAlphaTotalAmmo;

        public event OnZeroAlpha onZeroAlphaCurrentClipAmmo;

        public event OnZeroAlpha onInfoTextFadedOut;

        public void OnSetZeroTotalAmmo()
        {
            if (onZeroAlphaTotalAmmo != null)
                onZeroAlphaTotalAmmo();
        }

        public void OnSetZeroCurrentClipAmmo()
        {
            if (onZeroAlphaCurrentClipAmmo != null)
                onZeroAlphaCurrentClipAmmo();
        }

        public void OnTextFadedOut()
        {
            if (onInfoTextFadedOut != null)
                onInfoTextFadedOut();
            Destroy(gameObject);
        }

        public void OnTextFadedOutWorldText()
        {
            if (onInfoTextFadedOut != null)
                onInfoTextFadedOut();
            Destroy(transform.parent.gameObject);
        }
    }
}