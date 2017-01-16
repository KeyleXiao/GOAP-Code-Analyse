using UnityEngine;
using System.Collections.Generic;

#if MOBILE_INPUT

using UnityStandardAssets01.CrossPlatformInput;

#endif

namespace Player
{
    [RequireComponent(typeof(PlayerAtts))]
    public class SetupAndUserInput : MonoBehaviour
    {
        public float sensitivityX = 1, sensitivityY = 1;
        public float Horizontal { get; private set; }
        public float MouseX { get; private set; }
        public float MouseY { get; private set; }
        public float Vertical { get; private set; }
        public float LastInputAt { get; private set; }

        #region Buttons

        public bool CoverDown
        {
            get
            {
                return m_JumpDown;
            }
            private set { m_JumpDown = value; }
        }

        public bool JumpDown
        {
            get
            {
                return m_CoverDown;
            }
            private set { m_CoverDown = value; }
        }

        public bool CameraResetDown
        {
            get
            {
                return m_CameraResetDown;
            }
            private set { m_CameraResetDown = value; }
        }

        public bool Fire1Down
        {
            get
            {
                return m_Fire1Down;
            }
            private set { m_Fire1Down = value; }
        }

        public bool HolsterDown
        {
            get
            {
                return m_HolsterDown;
            }
            private set { m_HolsterDown = value; }
        }

        public bool CrouchDown
        {
            get
            {
                return m_CrouchDown;
            }
            private set { m_CrouchDown = value; }
        }

        public bool FirePress
        {
            get
            {
                return m_FirePress;
            }
            private set { m_FirePress = value; }
        }

        public bool Fire2Press
        {
            get
            {
                return m_Fire2Press;
            }
            set { m_Fire2Press = value; }
        }

        public bool Fire2Down
        {
            get
            {
                return m_Fire2Down;
            }
            set { m_Fire2Down = value; }
        }

        public bool ReloadDown
        {
            get
            {
                return m_ReloadDown;
            }
            private set { m_ReloadDown = value; }
        }

        public bool UseDown
        {
            get
            {
                return m_UseDown;
            }
            private set { m_UseDown = value; }
        }

        public bool FlashLightDown
        {
            get
            {
                return m_FlashLightDown;
            }
            private set { m_FlashLightDown = value; }
        }

        public bool WalkToggleDown
        {
            get
            {
                return m_WalkToggleDown;
            }
            private set { m_WalkToggleDown = value; }
        }

        public bool ModifyWeaponDown
        {
            get
            {
                return m_ChangeGunPartsDown;
            }
            private set { m_ChangeGunPartsDown = value; }
        }

        public bool MenuRightDown
        {
            get
            {
                return m_MenuRightDown;
            }
            set { m_MenuRightDown = value; }
        }

        public bool MenuLeftDown
        {
            get
            {
                return m_MenuLeftDown;
            }
            set { m_MenuLeftDown = value; }
        }

        public bool MenuDownDown
        {
            get
            {
                return m_MenuDownDown;
            }
            set { m_MenuDownDown = value; }
        }

        public bool MenuUpDown
        {
            get
            {
                return m_MenuUpDown;
            }
            set { m_MenuUpDown = value; }
        }

        public bool SecondaryFireDown
        {
            get
            {
                return m_SecondaryFire;
            }
            private set { m_SecondaryFire = value; }
        }

        public bool DropDown
        {
            get
            {
                return m_DropDown;
            }
            private set { m_DropDown = value; }
        }

        public bool ThrowDown
        {
            get
            {
                return m_ThrowDown;
            }
            private set { m_ThrowDown = value; }
        }

        public bool ThrowPress
        {
            get
            {
                return m_ThrowPress;
            }
            private set { m_ThrowPress = value; }
        }

        public bool FirstPersonLookDown
        {
            get
            {
                return m_FirstPersonLookDown;
            }
            private set { m_FirstPersonLookDown = value; }
        }

        public bool SprintPress
        {
            get
            {
                return m_SprintPress;
            }
            private set { m_SprintPress = value; }
        }

        public Transform cameraRig;
        public bool m_HolsterDown;
        public bool m_FirePress;
        public bool m_Fire2Press;
        public bool m_Fire2Down;
        public bool m_CrouchDown;
        public bool m_ReloadDown;
        public bool m_CoverDown;
        public bool m_JumpDown;
        public bool m_CameraResetDown;
        public bool m_FirstPersonLookDown;
        public bool m_UseDown;
        public bool m_DropDown;
        public bool m_SprintPress;
        public bool m_GrenadePress;
        public bool m_ThrowDown;
        public bool m_ThrowPress;
        public bool m_ChangeGunPartsDown;
        public bool m_FlashLightDown;
        public bool m_WalkToggleDown;
        public bool m_MenuLeftDown;
        public bool m_MenuRightDown;
        public bool m_MenuUpDown;
        public bool m_MenuDownDown;
        public bool m_SecondaryFire;
        public bool m_Fire1Down;
        public bool m_ResetButtons;

        #endregion Buttons

#if MOBILE_INPUT
        private bool canGetXY = false;
        private int mouseTouchId;
        private PlayerAtts player;
        private List<TouchingScreen> screenTouchesSwipes = new List<TouchingScreen>();

        public float minSwipeDist = 100;
        public float maxSwipeTime = .5f;
#endif

        private void Awake()
        {
            Animator animator = GetComponent<Animator>();

            if (!cameraRig || !animator)
            {
                string errorStr = "(in " + ToString() + ")Missing :";
                if (!cameraRig) errorStr += "Camera Rig | ";
                if (!animator) errorStr += "Player Animator | ";
                DisablePlayer(errorStr);
                return;
            }
#if MOBILE_INPUT
            player = GetComponent<PlayerAtts>();
#endif
        }

        public void DisablePlayer(string errorMessage)
        {
            Debug.Log("DISABLED PLAYER");
            Debug.LogError(errorMessage);

            if (GetComponent<Animator>())
                GetComponent<Animator>().enabled = false;
            if (GetComponent<PlayerAtts>())
                GetComponent<PlayerAtts>().enabled = false;
            if (cameraRig)
                cameraRig.gameObject.SetActive(false);
            enabled = false;
        }

        private void OnEnable()
        {
            // Find all the SMBs on the animator that inherit from CustomSMB.
            CustomSMB[] allSMBs = GetComponent<Animator>().GetBehaviours<CustomSMB>();
            for (int i = 0; i < allSMBs.Length; i++)
            {
                // For each SMB set it's userInput reference to this instance and run the initialisation function.
                allSMBs[i].userInput = this;
                allSMBs[i].player = GetComponent<PlayerAtts>();
                allSMBs[i].OnEnabled(GetComponent<Animator>());
            }
            LastInputAt = Time.time;
        }

        private void Start()
        {
            foreach (CustomSMB smb in GetComponent<Animator>().GetBehaviours<CustomSMB>())
                smb.OnStart();
        }

        private void Update()
        {
            if (Time.timeScale <= 0)
                return;

#if !MOBILE_INPUT
            Horizontal = Input.GetAxis("Horizontal");
            Vertical = Input.GetAxis("Vertical");
            MouseX = Input.GetAxis("Mouse X") * sensitivityX;
            MouseY = Input.GetAxis("Mouse Y") * sensitivityY;
#else
            
            if (CrossPlatformInputManager.GetButtonDown("Ready") && player.Animator.GetInteger("Weapon Pull") > 0)
                Fire2Press = !Fire2Press;

            if (Input.touchCount > 0 && !canGetXY && (CrossPlatformInputManager.GetButton("Sprint") || CrossPlatformInputManager.GetButton("Fire") || CrossPlatformInputManager.GetButton("RightSide") ||
                CrossPlatformInputManager.GetButton("Throw")))
            {
                canGetXY = true;
                for (int i = 0; i < Input.touchCount; i++)
                {
                    if (Input.GetTouch(i).phase == TouchPhase.Began)
                    {
                        mouseTouchId = Input.GetTouch(i).fingerId;
                        break;
                    }
                }
            }

            if (CrossPlatformInputManager.GetButton("Sprint"))
                SprintPress = true;
            else
                SprintPress = false;

            if (CrossPlatformInputManager.GetButton("Fire"))
                FirePress = true;
            else
                FirePress = false;

            if (CrossPlatformInputManager.GetButton("Throw"))
                ThrowPress = true;
            else
                ThrowPress = false;

            if (CrossPlatformInputManager.GetButtonUp("RightSide") || CrossPlatformInputManager.GetButtonUp("Fire") || CrossPlatformInputManager.GetButtonUp("Throw") || CrossPlatformInputManager.GetButtonUp("Sprint"))
                canGetXY = false;

            if (canGetXY)
            {
                for (int i = 0; i < Input.touchCount; i++)
                    if (Input.GetTouch(i).fingerId == mouseTouchId)
                    {
                        MouseX = Input.GetTouch(i).deltaPosition.x * sensitivityX;
                        MouseY = Input.GetTouch(i).deltaPosition.y * sensitivityY;
                        break;
                    }
            }
            else
            {
                MouseX = 0;
                MouseY = 0;
            }

            Horizontal = CrossPlatformInputManager.GetAxis("HorizontalL");
            Vertical = CrossPlatformInputManager.GetAxis("VerticalL");

#endif
            if (Horizontal != 0 || Vertical != 0)
                LastInputAt = Time.time;

            // If a FixedUpdate has happened since a button was set, then reset it.
            if (m_ResetButtons)
            {
                m_ResetButtons = false;

                HolsterDown = false;
                Fire2Down = false;
                FirstPersonLookDown = false;
                CameraResetDown = false;
                CrouchDown = false;
                ReloadDown = false;
                CoverDown = false;
                UseDown = false;
                FlashLightDown = false;
                WalkToggleDown = false;
                ModifyWeaponDown = false;
                DropDown = false;
                SecondaryFireDown = false;
                Fire1Down = false;
                ThrowDown = false;
                MenuRightDown = false;
                MenuLeftDown = false;
                MenuUpDown = false;
                MenuDownDown = false;
                JumpDown = false;

#if !MOBILE_INPUT
                FirePress = false;
                Fire2Press = false;
                ThrowPress = false;
                SprintPress = false;
#endif
            }

#if !MOBILE_INPUT
            if (Input.GetKeyDown(KeyCode.Mouse1))
            {
                Fire2Down = true;
                LastInputAt = Time.time;
            }
            if (Input.GetKeyDown(KeyCode.Space))
            {
                JumpDown = true;
                LastInputAt = Time.time;
            }
            if (Input.GetKeyDown(KeyCode.G))
            {
                ThrowDown = true;
                LastInputAt = Time.time;
            }
            if (Input.GetKey(KeyCode.G))
            {
                ThrowPress = true;
                LastInputAt = Time.time;
            }
            if (Input.GetKeyDown(KeyCode.B))
            {
                CameraResetDown = true;
                LastInputAt = Time.time;
            }
            if (Input.GetKeyDown(KeyCode.J))
            {
                FirstPersonLookDown = true;
                LastInputAt = Time.time;
            }
            if (Input.GetKeyDown(KeyCode.E))
            {
                UseDown = true;
                LastInputAt = Time.time;
            }
            if (Input.GetKeyDown(KeyCode.F))
            {
                HolsterDown = true;
                LastInputAt = Time.time;
            }
            if (Input.GetKeyDown(KeyCode.C))
            {
                CrouchDown = true;
                LastInputAt = Time.time;
            }
            if (Input.GetKeyDown(KeyCode.H))
            {
                DropDown = true;
                LastInputAt = Time.time;
            }
            if (Input.GetButton("Fire1"))
            {
                FirePress = true;
                LastInputAt = Time.time;
            }
            if (Input.GetKey(KeyCode.Mouse1))
            {
                Fire2Press = true;
                LastInputAt = Time.time;
            }
            if (Input.GetKeyDown(KeyCode.R))
            {
                ReloadDown = true;
                LastInputAt = Time.time;
            }
            if (Input.GetKeyDown(KeyCode.Q))
            {
                CoverDown = true;
                LastInputAt = Time.time;
            }
            if (Input.GetKeyDown(KeyCode.V))
            {
                ModifyWeaponDown = true;
                LastInputAt = Time.time;
            }
            if (Input.GetKeyDown(KeyCode.T))
            {
                FlashLightDown = true;
                LastInputAt = Time.time;
            }
            if (Input.GetKeyDown(KeyCode.LeftAlt))
            {
                WalkToggleDown = true;
                LastInputAt = Time.time;
            }
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                MenuRightDown = true;
                LastInputAt = Time.time;
            }
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                MenuLeftDown = true;
                LastInputAt = Time.time;
            }
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                MenuUpDown = true;
                LastInputAt = Time.time;
            }
            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                MenuDownDown = true;
                LastInputAt = Time.time;
            }
            if (Input.GetKeyDown(KeyCode.Mouse2))
            {
                SecondaryFireDown = true;
                LastInputAt = Time.time;
            }
            if (Input.GetButtonDown("Fire1"))
            {
                Fire1Down = true;
                LastInputAt = Time.time;
            }

            if (Input.GetKey(KeyCode.LeftShift))
            {
                SprintPress = true;
                LastInputAt = Time.time;
            }
#else
            foreach (var touch in Input.touches)
            {
                switch (touch.phase)
                {
                    case TouchPhase.Began:
                        screenTouchesSwipes.Add(new TouchingScreen(touch.fingerId, Time.time, touch.position));
                        break;

                    case TouchPhase.Ended:
                        if (screenTouchesSwipes.Find(x => x.FingerId == touch.fingerId) != null)
                        {
                            SwipingType swipe = screenTouchesSwipes.Find(x => x.FingerId == touch.fingerId).TouchEnded(minSwipeDist, maxSwipeTime, touch.position);
                            screenTouchesSwipes.RemoveAll(x => x.FingerId == touch.fingerId);
                            switch (swipe)
                            {
                                case SwipingType.NoSwipe:
                                    break;

                                case SwipingType.UpSwipe:
                                    MenuUpDown = true;
                                    break;

                                case SwipingType.DownSwipe:
                                    MenuDownDown = true;
                                    break;

                                case SwipingType.LeftSwipe:
                                    MenuLeftDown = true;
                                    break;

                                case SwipingType.RightSwipe:
                                    MenuRightDown = true;
                                    break;

                                default:
                                    break;
                            }
                        }
                        break;
                }
            }

            if (CrossPlatformInputManager.GetButtonDown("Use"))
            {
                UseDown = true;
                LastInputAt = Time.time;
            }
            if (CrossPlatformInputManager.GetButtonDown("Jump"))
            {
                JumpDown = true;
                LastInputAt = Time.time;
            }
            if (CrossPlatformInputManager.GetButtonDown("Ready"))
            {
                Fire2Down = true;
                LastInputAt = Time.time;
            }
            if (CrossPlatformInputManager.GetButtonDown("Holster"))
            {
                HolsterDown = true;
                LastInputAt = Time.time;
            }
            if (CrossPlatformInputManager.GetButtonDown("Crouch"))
            {
                CrouchDown = true;
                LastInputAt = Time.time;
            }
            if (CrossPlatformInputManager.GetButtonDown("Drop"))
            {
                DropDown = true;
                LastInputAt = Time.time;
            }

            if (CrossPlatformInputManager.GetButtonDown("Reload"))
            {
                ReloadDown = true;
                LastInputAt = Time.time;
            }

            if (CrossPlatformInputManager.GetButtonDown("Cover"))
            {
                CoverDown = true;
                LastInputAt = Time.time;
            }
            if (CrossPlatformInputManager.GetButtonDown("Modify"))
            {
                ModifyWeaponDown = true;
                LastInputAt = Time.time;
            }
            if (CrossPlatformInputManager.GetButtonDown("Flashlight"))
            {
                FlashLightDown = true;
                LastInputAt = Time.time;
            }
            if (CrossPlatformInputManager.GetButtonDown("WalkRun"))
            {
                WalkToggleDown = true;
                LastInputAt = Time.time;
            }

            if (CrossPlatformInputManager.GetButtonDown("SecondaryFire"))
            {
                SecondaryFireDown = true;
                LastInputAt = Time.time;
            }
            if (CrossPlatformInputManager.GetButtonDown("Fire"))
            {
                Fire1Down = true;
                LastInputAt = Time.time;
            }
            if (CrossPlatformInputManager.GetButtonDown("Throw"))
            {
                ThrowDown = true;
                LastInputAt = Time.time;
            }
#endif

            bool invokeAnyKeyDown = false;
            invokeAnyKeyDown = LastInputAt == Time.time ? true : invokeAnyKeyDown;
#if !MOBILE_INPUT
            invokeAnyKeyDown = Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0 ? true : invokeAnyKeyDown;
            invokeAnyKeyDown = Mathf.Abs(Input.GetAxisRaw("Vertical")) > 0 ? true : invokeAnyKeyDown;
            invokeAnyKeyDown = (Input.GetAxis("Mouse Y") != 0 || Input.GetAxis("Mouse X") != 0) ? true : invokeAnyKeyDown;
#else
            invokeAnyKeyDown = Mathf.Abs(CrossPlatformInputManager.GetAxisRaw("HorizontalL")) > 0 ? true : invokeAnyKeyDown;
            invokeAnyKeyDown = Mathf.Abs(CrossPlatformInputManager.GetAxisRaw("VerticalL")) > 0 ? true : invokeAnyKeyDown;
            invokeAnyKeyDown = canGetXY ? true : invokeAnyKeyDown;
#endif
            if (invokeAnyKeyDown)
            {
                LastInputAt = Time.time;
                InvokeAnyKeyDown();
            }
        }

        private void FixedUpdate()
        {
            m_ResetButtons = true;
        }

        public bool AnyKeyDown()
        {
            return (Input.anyKeyDown || MouseX != 0 || MouseY != 0);
        }

        public delegate void AnyKeyHandler();

        public event AnyKeyHandler onAnyKeyDown;

        private void InvokeAnyKeyDown()
        {
            if (onAnyKeyDown != null)
                onAnyKeyDown();
        }

#if MOBILE_INPUT

        public enum SwipingType { NoSwipe, UpSwipe, DownSwipe, LeftSwipe, RightSwipe }

        private class TouchingScreen
        {
            public int FingerId { get; private set; }
            public float TouchStartedAt { get; private set; }
            public Vector2 TouchStartPosition { get; private set; }

            public TouchingScreen(int _fingerId, float _touchStartedAt, Vector2 _touchStartPos)
            {
                FingerId = _fingerId;
                TouchStartedAt = _touchStartedAt;
                TouchStartPosition = _touchStartPos;
            }

            public SwipingType TouchEnded(float minSwipeNeeded, float maxTime, Vector2 touchEndPos)
            {
                float swipeDistVertical = (new Vector3(0, touchEndPos.y, 0) - new Vector3(0, TouchStartPosition.y, 0)).magnitude;
                float swipeDistHorizontal = (new Vector3(touchEndPos.x, 0, 0) - new Vector3(TouchStartPosition.x, 0, 0)).magnitude;

                if ((swipeDistVertical < minSwipeNeeded && swipeDistHorizontal < minSwipeNeeded) || Time.time - TouchStartedAt > maxTime)
                    return SwipingType.NoSwipe;

                if (swipeDistVertical > swipeDistHorizontal && swipeDistVertical > minSwipeNeeded)
                {
                    float swipe = Mathf.Sign(TouchStartPosition.x - touchEndPos.x);
                    return swipe > 0 ? SwipingType.UpSwipe : SwipingType.DownSwipe;
                }
                else if (swipeDistHorizontal > swipeDistVertical && swipeDistHorizontal > minSwipeNeeded)
                {
                    float swipe = Mathf.Sign(TouchStartPosition.y - touchEndPos.y);
                    return swipe > 0 ? SwipingType.RightSwipe : SwipingType.LeftSwipe;
                }

                return SwipingType.NoSwipe;
            }
        }

#endif
    }
}