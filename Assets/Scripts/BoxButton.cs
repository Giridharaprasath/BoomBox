using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace BoomBox
{
    [Serializable]
    public class BoxButtonInfo
    {
        public CubeColor CubeColor;
        public int ColumnIndex;
        public int StartRowIndex;
        public int CurrentRowIndex;
        public bool CanShoot;
        public bool IsHit;

        public BoxButtonInfo()
        {
        }

        public BoxButtonInfo(BoxButtonInfo cubeButtonInfo)
        {
            CubeColor = cubeButtonInfo.CubeColor;
            ColumnIndex = cubeButtonInfo.ColumnIndex;
            StartRowIndex = cubeButtonInfo.StartRowIndex;
            CurrentRowIndex = cubeButtonInfo.CurrentRowIndex;
        }
    }

    public class BoxButton : MonoBehaviour
    {
        public BoxButtonInfo BoxButtonInfo;
        public Sprite BoxSprite;

        public Action<BoxButton> OnCanShoot;
        public Action<int> OnAllHit;
        public GameObject OtherBoxObject;
        public List<GameObject> OtherBoxes;
        public int TotalCount;
        public int CurrentCount;

        public ParticleSystem HitEffect;

        private void Start()
        {
            InitCubeButton();
        }

        public void InitCubeButton()
        {
            for (int i = 0; i < TotalCount; i++)
            {
                GameObject box = Instantiate(OtherBoxObject, transform);
                box.transform.localPosition = new Vector3(0f, 30f * i, 0f);
                box.GetComponent<Image>().sprite = BoxSprite;
                OtherBoxes.Add(box);
            }

            transform.localScale = Vector3.zero;
            transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutSine);
        }

        public void SetRowIndex(int index)
        {
            BoxButtonInfo.CurrentRowIndex = index;
            if (BoxButtonInfo.CurrentRowIndex == 0)
            {
                SetCanShoot();
            }
        }

        public void SetCanShoot()
        {
            BoxButtonInfo.CanShoot = true;
            OnCanShoot?.Invoke(this);
        }

        public void OnHit()
        {
            CurrentCount--;
            // OtherBoxes[CurrentCount].SetActive(false);
            AnimateObject(OtherBoxes[CurrentCount]);

            HitEffect.Play();

            if (CurrentCount == 0)
            {
                BoxButtonInfo.IsHit = true;
                BoxButtonInfo.CanShoot = false;
                OnAllHit?.Invoke(BoxButtonInfo.ColumnIndex);
            }
            else
            {
                SetRowIndex(0);
            }
        }

        private void AnimateObject(GameObject gameObject)
        {
            Image img = gameObject.GetComponent<Image>();

            gameObject.transform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.OutSine);
            img.DOFade(0f, 1f).SetEase(Ease.Linear).OnComplete(() =>
            {
                gameObject.SetActive(false);
            });
        }
    }
}
