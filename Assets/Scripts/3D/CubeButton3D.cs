using System;
using DG.Tweening;
using UnityEngine;
using TMPro;
using System.Collections.Generic;

namespace BoomBox
{
    public class CubeButton3D : MonoBehaviour
    {
        public CubeButtonInfo CubeButtonInfo;
        public TMP_Text CountText;
        public Animator CharacterAnimator;
        public GameObject BulletSpawnPoint;

        public Material CubeColor;

        private Sequence sequence;
        private bool IsAnimating;
        public Action<CubeButton3D> OnClicked;
        public Action<CubeButton3D, BoxButton3D> OnHitSuccess;

        public Renderer CharacterHead;
        public Renderer CharacterLeg;

        public Collider boxCollider;

        public GameObject CharacterObject;
        public GameObject CharacterHeadObject;

        public List<GameObject> Bullets;
        public GameObject BulletPrefab;

        private void Start()
        {
            InitCubeButton();
        }

        private void OnDestroy()
        {
            sequence.Kill();
        }

        public void InitCubeButton()
        {
            CharacterHead.material = CubeColor;
            CharacterLeg.materials[1].color = CubeColor.color;

            CountText.text = CubeButtonInfo.Count.ToString();
            CountText.color = new(1f, 1f, 1f, 0.5f);

            transform.localScale = new Vector3(15f, 15f, 15f);
            transform.DOScale(new Vector3(12f, 12f, 12f), 0.5f).SetEase(Ease.OutSine);

            CheckRowIndex();
            SpawnAllBullets();
        }

        private void SpawnAllBullets()
        {
            for (int i = 0; i < CubeButtonInfo.Count; i++)
            {
                GameObject bullet = Instantiate(BulletPrefab, BulletSpawnPoint.transform);
                bullet.SetActive(false);

                Bullets.Add(bullet);
            }
        }

        public void SetSpotIndex(int index)
        {
            CubeButtonInfo.CurrentSpotIndex = index;
            CubeButtonInfo.CurrentRowIndex = -1;
            CubeButtonInfo.CanSelect = false;

            SetDefaultLayer();
        }

        public void SetRowIndex(int index)
        {
            CubeButtonInfo.CurrentRowIndex = index;
        }
        public void CheckRowIndex()
        {
            if (CubeButtonInfo.CurrentRowIndex == 0)
            {
                CubeButtonInfo.CanSelect = true;
                SetCharacterLayer();
                CountText.color = new(1f, 1f, 1f, 1f);
            }
        }

        private void OnMouseDown()
        {
            OnClicked?.Invoke(this);
        }

        public void DecreaseCount()
        {
            CubeButtonInfo.Count--;
            CountText.text = CubeButtonInfo.Count.ToString();
        }

        public void OnClickShake()
        {
            if (IsAnimating) return;

            CharacterHeadObject.transform.DOShakeRotation(1f, new Vector3(0f, 30f, 0f)).SetEase(Ease.OutSine).SetDelay(0.1f).OnComplete(() =>
            {
                IsAnimating = false;
            });
        }

        private void SetCharacterLayer()
        {
            int layerIndex = LayerMask.NameToLayer("Character");
            CharacterObject.layer = layerIndex;
            CharacterLeg.gameObject.layer = layerIndex;
        }

        private void SetDefaultLayer()
        {
            int layerIndex = LayerMask.NameToLayer("Default");
            CharacterObject.layer = layerIndex;
            CharacterLeg.gameObject.layer = layerIndex;
        }

        public void DisableCollider()
        {
            boxCollider.enabled = false;
        }

        public void StartMoving()
        {
            CharacterAnimator.SetFloat("Speed", 1f);
            boxCollider.enabled = false;
        }

        public void StopMoving()
        {
            CharacterAnimator.SetFloat("Speed", 0f);
            boxCollider.enabled = true;
        }

        public void StartShootingAt(BoxButton3D boxButton)
        {
            DecreaseCount();

            int BulletIndex = CubeButtonInfo.Count;
            GameObject bullet = Bullets[BulletIndex];
            CharacterHeadObject.transform.LookAt(boxButton.transform.position);
            bullet.SetActive(true);

            bullet.transform.DOMove(boxButton.transform.position, 0.2f).SetEase(Ease.OutQuad).OnComplete(() =>
            {
                bullet.SetActive(false);
                CharacterHeadObject.transform.localRotation = Quaternion.identity;
                OnHitSuccess?.Invoke(this, boxButton);
            });
        }

        public void AnimateColor()
        {
            // Color oldColor = CharacterHead.material.color;

            // CharacterHead.material.DOColor(Color.white, 0.2f).SetEase(Ease.Linear).OnComplete(() =>
            // {
            //     CharacterHead.material.DOColor(oldColor, 0.2f);
            // });
        }
    }
}
