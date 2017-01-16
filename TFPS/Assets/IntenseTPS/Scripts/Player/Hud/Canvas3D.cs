using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Player
{
	public class Canvas3D : MonoBehaviour
	{
		public float positionLerpSpeed = 7f;
		public bool useCamDistanceResizer = true;
		public float maxScaleAtDist = 2.5f;
		public float maxScale = .005f;
		public float minScaleAtDist = .75f;
		public float minScale = .002f;
		public GameObject informationTextPrefab;
		public GameObject worldInformationTextPrefab;
		public float worldInfoTextScale = .05f;

		[Space]
		public float rightFix;

		public float leftFix;
		public float upFix;
		public HumanBodyBones bone = HumanBodyBones.Head;

		#region Private

		private PlayerCamera playerCamera;
		private Transform followBone;
		private Text totalAmmoText0;
		private Text totalAmmoText1;
		private Text totalAmmoText2;

		private Text currentClipText0;
		private Text currentClipText1;

		private Text secTotalAmmoText0;
		private Text secTotalAmmoText1;
		private Text secTotalAmmoText2;

		private Text secCurrentClipText0;
		private Text secCurrentClipText1;

		private Text throwableCountText0;
		private Text throwableCountText1;

		private Animator cClipAnimator;
		private Animator tAmmoAnimator;
		private Animator throwableCountAnimator;
		private Animator secCClipAnimator;
		private Animator secTAmmoAnimator;

		private Image bulletImage;
		private Text bulletName;

		private Image secBulletImage;
		private Text secBulletName;

		private Image weaponImage;
		private Text weaponName;

		private Image throwableImage;
		private Text throwableName;

		private PlayerAtts player;
		private WeaponCSMB smbWeapon;

		private string toPrintTotalAmmo, toPrintCurrentClip, toPrintSecTotalAmmo, toPrintSecCurrentClip, toPrintThrowableCount;

		private Transform infoTextParentPanel;
		private List<Small3dHudInfoText> infoTexts;

		private List<Small3dHudInfoText> worldInfoTexts;
		private Vector3 targetPosition;

		#endregion Private

		private void Start()
		{
			toPrintThrowableCount = "00";
			toPrintCurrentClip = "00";
			toPrintTotalAmmo = "000";
			toPrintSecCurrentClip = "00";
			toPrintSecTotalAmmo = "000";

			infoTexts = new List<Small3dHudInfoText>();
			worldInfoTexts = new List<Small3dHudInfoText>();

			if (GameObject.FindGameObjectWithTag("Player"))
			{
				player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerAtts>();
				smbWeapon = player.SmbWeapon;
				smbWeapon.Events.onWeaponFire += OnFire;
				smbWeapon.Events.onReloadDone += OnReload;
				smbWeapon.Events.onWeaponPullOut += OnWeaponPullOut;
				smbWeapon.Events.onWeaponSwitch += OnWeaponChange;
				smbWeapon.Events.onDropWeapon += OnWeaponChange;
				smbWeapon.Events.onWeaponCollect += OnWeaponCollect;
				smbWeapon.Events.onSupplyCollect += SetAllCounts;

				player.SmbThrow.Events.onSwitch += OnThrowableSwitch;
				player.SmbThrow.Events.onThrowableExit += OnThrowableExit;
			}

			if (!player)
			{
				Debug.Log("Needed reference not found..." + ToString());
				gameObject.SetActive(false);

				return;
			}
			playerCamera = player.GetComponent<SetupAndUserInput>().cameraRig.GetComponent<PlayerCamera>();

			foreach (var catcher in gameObject.GetComponentsInChildren<HudEventCatcher>())
			{
				catcher.onZeroAlphaTotalAmmo += AnimEventOnTotalAmmoZeroAlpha;
				catcher.onZeroAlphaCurrentClipAmmo += AnimEventOnCurrentAmmoZeroAlpha;
			}

			foreach (var trns in transform.GetComponentsInChildren<Transform>())
			{
				if (trns.name == "Current Clip Capacity")
					cClipAnimator = trns.GetComponent<Animator>();
				else if (trns.name == "Total Ammo")
					tAmmoAnimator = trns.GetComponent<Animator>();
				else if (trns.name == "SecCurrent Clip Capacity")
					secCClipAnimator = trns.GetComponent<Animator>();
				else if (trns.name == "SecTotal Ammo")
					secTAmmoAnimator = trns.GetComponent<Animator>();
				else if (trns.name == "Weapon Image")
					weaponImage = trns.GetComponent<Image>();
				else if (trns.name == "Weapon Name")
					weaponName = trns.GetComponent<Text>();
				else if (trns.name == "Projectile Image")
					bulletImage = trns.GetComponent<Image>();
				else if (trns.name == "Projectile Name")
					bulletName = trns.GetComponent<Text>();
				else if (trns.name == "SecProjectile Image")
					secBulletImage = trns.GetComponent<Image>();
				else if (trns.name == "SecProjectile Name")
					secBulletName = trns.GetComponent<Text>();
				else if (trns.name == "Throwable Count")
					throwableCountAnimator = trns.GetComponent<Animator>();
				else if (trns.name == "Throwable Name")
					throwableName = trns.GetComponent<Text>();
				else if (trns.name == "Throwable Image")
					throwableImage = trns.GetComponent<Image>();
				else if (trns.name == "InfoTextParentPanel")
					infoTextParentPanel = trns;
			}

			foreach (var trns in cClipAnimator.GetComponentsInChildren<Transform>())
			{
				if (trns.name == "Text0")
					currentClipText0 = trns.GetComponent<Text>();
				else if (trns.name == "Text1")
					currentClipText1 = trns.GetComponent<Text>();
			}

			foreach (var trns in tAmmoAnimator.GetComponentsInChildren<Transform>())
			{
				if (trns.name == "Text0")
					totalAmmoText0 = trns.GetComponent<Text>();
				else if (trns.name == "Text1")
					totalAmmoText1 = trns.GetComponent<Text>();
				else if (trns.name == "Text2")
					totalAmmoText2 = trns.GetComponent<Text>();
			}
			if (secCClipAnimator)
				foreach (var trns in secCClipAnimator.GetComponentsInChildren<Transform>())
				{
					if (trns.name == "Text0")
						secCurrentClipText0 = trns.GetComponent<Text>();
					else if (trns.name == "Text1")
						secCurrentClipText1 = trns.GetComponent<Text>();
				}
			if (secTAmmoAnimator)
				foreach (var trns in secTAmmoAnimator.GetComponentsInChildren<Transform>())
				{
					if (trns.name == "Text0")
						secTotalAmmoText0 = trns.GetComponent<Text>();
					else if (trns.name == "Text1")
						secTotalAmmoText1 = trns.GetComponent<Text>();
					else if (trns.name == "Text2")
						secTotalAmmoText2 = trns.GetComponent<Text>();
				}

			foreach (var trns in throwableCountAnimator.GetComponentsInChildren<Transform>())
			{
				if (trns.name == "Text0")
					throwableCountText0 = trns.GetComponent<Text>();
				else if (trns.name == "Text1")
					throwableCountText1 = trns.GetComponent<Text>();
			}

			currentClipText0.text = "0";
			currentClipText1.text = "0";

			totalAmmoText0.text = "0";
			totalAmmoText1.text = "0";
			totalAmmoText2.text = "0";

			if (secCurrentClipText0)
				secCurrentClipText0.text = "0";
			if (secCurrentClipText0)
				secCurrentClipText1.text = "0";

			if (secTotalAmmoText0)
				secTotalAmmoText0.text = "0";
			if (secTotalAmmoText1)
				secTotalAmmoText1.text = "0";
			if (secTotalAmmoText2)
				secTotalAmmoText2.text = "0";

			throwableCountText0.text = "0";
			throwableCountText1.text = "1";

			SetWeaponImage(player.SmbWeapon.GetCurrentWeaponSprite());
			SetWeaponName(player.SmbWeapon.GetCurrentWeaponName());
			SetBulletImage(player.SmbWeapon.GetCurrentBulletSprite());
			SetBulletnName(player.SmbWeapon.GetCurrentBulletName());
			SetSecBulletImage(player.SmbWeapon.GetSecCurrentBulletSprite());
			SetSecBulletnName(player.SmbWeapon.GetSecCurrentBulletName());

			CClipTexter(player.SmbWeapon.GetCurrentClip());
			TotalClipTexter(player.SmbWeapon.GetTotalAmmoCount());
			SecCClipTexter(player.SmbWeapon.GetSecCurrentClip());
			SecTotalClipTexter(player.SmbWeapon.GetSecTotalAmmoCount());

			SetThrowableImage(player.SmbThrow.GetThrowableSprite());
			SetThrowableName(player.SmbThrow.GetThrowableName());
			ThrowableCountTexter(player.SmbThrow.GetThrowableCount());

			followBone = player.GetComponent<Animator>().GetBoneTransform(bone);
		}

		public void PrintInformationText(string id, float timer, string text)
		{
			if (infoTexts.Find(x => x.Id == id) != null)
				infoTexts.Find(x => x.Id == id).SetTimerAndText(timer, text);
			else if (informationTextPrefab && infoTextParentPanel)
			{
				GameObject textClone = Instantiate(informationTextPrefab, Vector3.zero, Quaternion.identity) as GameObject;
				textClone.transform.FindChild("InfoText").GetComponent<Text>().text = text;
				textClone.transform.SetParent(infoTextParentPanel, false);

				infoTexts.Add(new Small3dHudInfoText(id, textClone.GetComponent<Animator>(), timer, textClone.transform.FindChild("InfoText").GetComponent<Text>()));
			}
		}

		public void PrintWorldInformationText(string id, float timer, string text, Vector3 position, Quaternion rotation)
		{
			if (worldInfoTexts.Find(x => x.Id == id) != null)
			{
				worldInfoTexts.Find(x => x.Id == id).SetTimerAndText(timer, text);

				worldInfoTexts.Find(x => x.Id == id).Animator.transform.parent.transform.position = position;
				worldInfoTexts.Find(x => x.Id == id).Animator.transform.parent.rotation = rotation;
			}
			else if (worldInformationTextPrefab)
			{
				GameObject textClone = Instantiate(worldInformationTextPrefab, position, rotation) as GameObject;
				textClone.GetComponent<RectTransform>().position = position;
				textClone.transform.FindChild("InfoWorldTextPanelPrefab").FindChild("InfoText").GetComponent<Text>().text = text;
				textClone.transform.SetParent(null, true);
				textClone.transform.localScale = worldInfoTextScale * Vector3.one;
				worldInfoTexts.Add(new Small3dHudInfoText(id, textClone.transform.FindChild("InfoWorldTextPanelPrefab").GetComponent<Animator>(), timer,
					textClone.transform.FindChild("InfoWorldTextPanelPrefab").FindChild("InfoText").GetComponent<Text>()
					));
			}
		}

		private void Update()
		{
			HudTextUpdate(infoTexts);
			HudTextUpdate(worldInfoTexts);

			ManageSize();
		}

		private void HudTextUpdate(List<Small3dHudInfoText> textList)
		{
			for (int i = 0; i < textList.Count; i++)
			{
				textList[i].DecrementTimer();
				if (textList[i].IsDisabled)
					textList.RemoveAt(i);
			}
		}

		Vector3 lastFrame;

		private void ManageSize()
		{
			if (!useCamDistanceResizer)
				return;

			targetPosition = followBone.position + playerCamera.transform.right * (playerCamera.currentOffset.x >= 0 ? 1 * rightFix : -1 * (leftFix)) + Vector3.up * upFix;
			transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * positionLerpSpeed);
			//transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, playerCamera.transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);

			float yAngle = Mathf.LerpAngle(transform.eulerAngles.y, playerCamera.transform.rotation.eulerAngles.y, Time.deltaTime * positionLerpSpeed / 2);
			transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, yAngle, transform.rotation.eulerAngles.z);


			float fov = Vector3.Distance(playerCamera.transform.position, transform.position);
			float maxDif = maxScaleAtDist - minScaleAtDist;
			float cDif = fov - minScaleAtDist;

			float scale = Mathf.Lerp(minScale, maxScale, cDif / maxDif);
			transform.localScale = Vector3.one * scale;

		}

		#region Event Triggered

		private void OnWeaponChange(GunAtt gunAttOld, GunAtt gunAttNew)
		{
			SetWeaponImage(player.SmbWeapon.GetCurrentWeaponSprite());
			SetWeaponName(player.SmbWeapon.GetCurrentWeaponName());
			SetBulletImage(player.SmbWeapon.GetCurrentBulletSprite());
			SetBulletnName(player.SmbWeapon.GetCurrentBulletName());
			CClipTexter(player.SmbWeapon.GetCurrentClip());
			TotalClipTexter(player.SmbWeapon.GetTotalAmmoCount());

			SetSecBulletImage(player.SmbWeapon.GetSecCurrentBulletSprite());
			SetSecBulletnName(player.SmbWeapon.GetSecCurrentBulletName());
			SecCClipTexter(player.SmbWeapon.GetSecCurrentClip());
			SecTotalClipTexter(player.SmbWeapon.GetSecTotalAmmoCount());
		}

		private void OnWeaponCollect(GunAtt gunAtt)
		{
			SetWeaponImage(player.SmbWeapon.GetCurrentWeaponSprite());
			SetWeaponName(player.SmbWeapon.GetCurrentWeaponName());
			SetBulletImage(player.SmbWeapon.GetCurrentBulletSprite());
			SetBulletnName(player.SmbWeapon.GetCurrentBulletName());
			CClipTexter(player.SmbWeapon.GetCurrentClip());
			TotalClipTexter(player.SmbWeapon.GetTotalAmmoCount());

			SetSecBulletImage(player.SmbWeapon.GetSecCurrentBulletSprite());
			SetSecBulletnName(player.SmbWeapon.GetSecCurrentBulletName());
			SecCClipTexter(player.SmbWeapon.GetSecCurrentClip());
			SecTotalClipTexter(player.SmbWeapon.GetSecTotalAmmoCount());
		}

		private void OnWeaponPullOut(GunAtt gunAtt)
		{
			CClipTexter(player.SmbWeapon.GetCurrentClip());
			TotalClipTexter(player.SmbWeapon.GetTotalAmmoCount());
			SetWeaponImage(gunAtt.hudSprite);
			SetWeaponName(gunAtt.weaponName);
			SetBulletImage(player.SmbWeapon.GetCurrentBulletSprite());
			SetBulletnName(player.SmbWeapon.GetCurrentBulletName());

			SetSecBulletImage(player.SmbWeapon.GetSecCurrentBulletSprite());
			SetSecBulletnName(player.SmbWeapon.GetSecCurrentBulletName());
			SecCClipTexter(player.SmbWeapon.GetSecCurrentClip());
			SecTotalClipTexter(player.SmbWeapon.GetSecTotalAmmoCount());
		}

		private void SetAllCounts()
		{
			CClipTexter(player.SmbWeapon.GetCurrentClip());
			TotalClipTexter(player.SmbWeapon.GetTotalAmmoCount());
			SecCClipTexter(player.SmbWeapon.GetSecCurrentClip());
			SecTotalClipTexter(player.SmbWeapon.GetSecTotalAmmoCount());
			ThrowableCountTexter(player.SmbThrow.GetThrowableCount());
		}

		private void OnFire(GunAtt gunAtt)
		{
			CClipTexter(player.SmbWeapon.GetCurrentClip());

			SecCClipTexter(player.SmbWeapon.GetSecCurrentClip());
		}

		private void OnReload(GunAtt gunAtt)
		{
			TotalClipTexter(player.SmbWeapon.GetTotalAmmoCount());
			CClipTexter(player.SmbWeapon.GetCurrentClip());

			SecCClipTexter(player.SmbWeapon.GetSecCurrentClip());
			SecTotalClipTexter(player.SmbWeapon.GetSecTotalAmmoCount());
		}

		private void OnThrowableSwitch(Exploder oldExp, Exploder newExp)
		{
			SetThrowableImage(player.SmbThrow.GetThrowableSprite());
			SetThrowableName(player.SmbThrow.GetThrowableName());
			ThrowableCountTexter(player.SmbThrow.GetThrowableCount());
		}

		private void OnThrowableExit(Exploder exp)
		{
			SetThrowableImage(player.SmbThrow.GetThrowableSprite());
			SetThrowableName(player.SmbThrow.GetThrowableName());
			ThrowableCountTexter(player.SmbThrow.GetThrowableCount());
		}

		#endregion Event Triggered

		#region UI Assign

		private void SetWeaponImage(Sprite sprite)
		{
			weaponImage.sprite = sprite;
			if (!sprite) weaponImage.color = new Color(weaponImage.color.r, weaponImage.color.g, weaponImage.color.b, 0);
			else weaponImage.color = new Color(weaponImage.color.r, weaponImage.color.g, weaponImage.color.b, 1);
		}

		private void SetWeaponName(string name)
		{
			weaponName.text = name;
		}

		private void SetBulletImage(Sprite sprite)
		{
			bulletImage.sprite = sprite;
			if (!sprite) bulletImage.color = new Color(bulletImage.color.r, bulletImage.color.g, bulletImage.color.b, 0);
			else bulletImage.color = new Color(bulletImage.color.r, bulletImage.color.g, bulletImage.color.b, 1);
		}

		private void SetBulletnName(string name)
		{
			bulletName.text = name;
		}

		private void SetSecBulletImage(Sprite sprite)
		{
			if (!secBulletImage)
				return;
			secBulletImage.sprite = sprite;
			if (!sprite) secBulletImage.color = new Color(secBulletImage.color.r, secBulletImage.color.g, secBulletImage.color.b, 0);
			else secBulletImage.color = new Color(secBulletImage.color.r, secBulletImage.color.g, secBulletImage.color.b, 1);
		}

		private void SetSecBulletnName(string name)
		{
			if (!secBulletName)
				return;
			secBulletName.text = name;
		}

		private void SetThrowableImage(Sprite sprite)
		{
			throwableImage.sprite = sprite;
			if (!sprite) throwableImage.color = new Color(throwableImage.color.r, throwableImage.color.g, throwableImage.color.b, 0);
			else throwableImage.color = new Color(throwableImage.color.r, throwableImage.color.g, throwableImage.color.b, 1);
		}

		private void SetThrowableName(string name)
		{
			throwableName.text = name;
		}

		private void CClipTexter(int count)
		{
			if (currentClipText0.text == "")
				currentClipText0.text = "0";
			if (currentClipText1.text == "")
				currentClipText1.text = "0";

			toPrintCurrentClip = count > 99 ? "99" : count.ToString().PadLeft(2, '0');
			if (currentClipText0.text[0] != toPrintCurrentClip[0])
			{
				cClipAnimator.SetTrigger("Text0Change");
				cClipAnimator.ResetTrigger("Text1Change");
			}
			else if (currentClipText1.text[0] != toPrintCurrentClip[1])
			{
				cClipAnimator.ResetTrigger("Text0Change");
				cClipAnimator.SetTrigger("Text1Change");
			}
		}

		private void TotalClipTexter(int count)
		{
			if (totalAmmoText0.text == "")
				totalAmmoText0.text = "0";
			if (totalAmmoText1.text == "")
				totalAmmoText1.text = "0";
			if (totalAmmoText2.text == "")
				totalAmmoText2.text = "0";

			int countAmmo = count;

			toPrintTotalAmmo = countAmmo > 999 ? "999" : countAmmo.ToString().PadLeft(3, '0');

			if (totalAmmoText0.text[0] != toPrintTotalAmmo[0])
			{
				tAmmoAnimator.SetTrigger("Text0Change");
				tAmmoAnimator.ResetTrigger("Text1Change");
				tAmmoAnimator.ResetTrigger("Text2Change");
			}
			else if (totalAmmoText1.text[0] != toPrintTotalAmmo[1])
			{
				tAmmoAnimator.ResetTrigger("Text0Change");
				tAmmoAnimator.SetTrigger("Text1Change");
				tAmmoAnimator.ResetTrigger("Text2Change");
			}
			else if (totalAmmoText2.text[0] != toPrintTotalAmmo[2])
			{
				tAmmoAnimator.ResetTrigger("Text0Change");
				tAmmoAnimator.ResetTrigger("Text1Change");
				tAmmoAnimator.SetTrigger("Text2Change");
			}
		}

		private void SecCClipTexter(int count)
		{
			if (!secCurrentClipText0 || !secCurrentClipText1 || !secCClipAnimator)
				return;
			if (secCurrentClipText0.text == "")
				secCurrentClipText0.text = "0";
			if (secCurrentClipText1.text == "")
				secCurrentClipText1.text = "0";

			toPrintSecCurrentClip = count > 99 ? "99" : count.ToString().PadLeft(2, '0');
			if (secCurrentClipText0.text[0] != toPrintSecCurrentClip[0])
			{
				secCClipAnimator.SetTrigger("Text0Change");
				secCClipAnimator.ResetTrigger("Text1Change");
			}
			else if (secCurrentClipText1.text[0] != toPrintSecCurrentClip[1])
			{
				secCClipAnimator.ResetTrigger("Text0Change");
				secCClipAnimator.SetTrigger("Text1Change");
			}
		}

		private void SecTotalClipTexter(int count)
		{
			if (!secTotalAmmoText0 || !secTotalAmmoText1 || !secTotalAmmoText2 || !secTAmmoAnimator)
				return;
			if (secTotalAmmoText0.text == "")
				secTotalAmmoText0.text = "0";
			if (secTotalAmmoText1.text == "")
				secTotalAmmoText1.text = "0";
			if (secTotalAmmoText2.text == "")
				secTotalAmmoText2.text = "0";

			int countAmmo = count;

			toPrintSecTotalAmmo = countAmmo > 999 ? "999" : countAmmo.ToString().PadLeft(3, '0');
			//}

			if (secTotalAmmoText0.text[0] != toPrintSecTotalAmmo[0])
			{
				secTAmmoAnimator.SetTrigger("Text0Change");
				secTAmmoAnimator.ResetTrigger("Text1Change");
				secTAmmoAnimator.ResetTrigger("Text2Change");
			}
			else if (secTotalAmmoText1.text[0] != toPrintSecTotalAmmo[1])
			{
				secTAmmoAnimator.ResetTrigger("Text0Change");
				secTAmmoAnimator.SetTrigger("Text1Change");
				secTAmmoAnimator.ResetTrigger("Text2Change");
			}
			else if (secTotalAmmoText2.text[0] != toPrintSecTotalAmmo[2])
			{
				secTAmmoAnimator.ResetTrigger("Text0Change");
				secTAmmoAnimator.ResetTrigger("Text1Change");
				secTAmmoAnimator.SetTrigger("Text2Change");
			}
		}

		private void ThrowableCountTexter(int count)
		{
			if (throwableCountText0.text == "")
				throwableCountText0.text = "0";
			if (throwableCountText1.text == "")
				throwableCountText1.text = "0";

			toPrintThrowableCount = "00";
			int haveC = count;
			toPrintThrowableCount = haveC > 999 ? "99" : haveC.ToString().PadLeft(2, '0');

			if (throwableCountText0.text[0] != toPrintThrowableCount[0])
			{
				throwableCountAnimator.SetTrigger("Text0Change");
				throwableCountAnimator.ResetTrigger("Text1Change");
			}
			else if (throwableCountText1.text[0] != toPrintThrowableCount[1])
			{
				throwableCountAnimator.ResetTrigger("Text0Change");
				throwableCountAnimator.SetTrigger("Text1Change");
			}
		}

		private void AnimEventOnTotalAmmoZeroAlpha()
		{
			totalAmmoText0.text = toPrintTotalAmmo[0].ToString();
			totalAmmoText1.text = toPrintTotalAmmo[1].ToString();
			totalAmmoText2.text = toPrintTotalAmmo[2].ToString();

			if (secTotalAmmoText0)
				secTotalAmmoText0.text = toPrintSecTotalAmmo[0].ToString();
			if (secTotalAmmoText1)
				secTotalAmmoText1.text = toPrintSecTotalAmmo[1].ToString();
			if (secTotalAmmoText2)
				secTotalAmmoText2.text = toPrintSecTotalAmmo[2].ToString();
		}

		private void AnimEventOnCurrentAmmoZeroAlpha()
		{
			currentClipText0.text = toPrintCurrentClip[0].ToString();
			currentClipText1.text = toPrintCurrentClip[1].ToString();

			if (secCurrentClipText0)
				secCurrentClipText0.text = toPrintSecCurrentClip[0].ToString();
			if (secCurrentClipText1)
				secCurrentClipText1.text = toPrintSecCurrentClip[1].ToString();

			throwableCountText0.text = toPrintThrowableCount[0].ToString();
			throwableCountText1.text = toPrintThrowableCount[1].ToString();
		}

		#endregion UI Assign

		private class Small3dHudInfoText
		{
			public Animator Animator { get; private set; }
			public Text textComp { get; private set; }
			public float Timer { get; private set; }
			public string Id { get; private set; }

			public bool IsDisabled { get; private set; }

			public Small3dHudInfoText(string id, Animator _animator, float _timer, Text _textComp)
			{
				Id = id; Animator = _animator; Timer = _timer; textComp = _textComp;
				IsDisabled = false;
			}

			public void SetTimerAndText(float time, string text)
			{
				if (IsDisabled)
					return;
				textComp.text = text;
				Timer = time;
			}

			public void DecrementTimer(bool setFadeOutOnZero = true)
			{
				Timer -= Time.deltaTime;
				if (Timer < 0 && Animator)
				{
					Animator.SetBool("FadeOut", true);
					IsDisabled = true;
				}
			}
		}
	}
}