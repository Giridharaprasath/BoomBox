using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;
using System.Collections;

namespace BoomBox
{
    [Serializable]
    public class CubeButtonInfo
    {
        public CubeColor CubeColor;
        public int Count;
        public bool CanSelect;
        public int ColumnIndex;
        public int StartRowIndex;
        public int CurrentRowIndex;
        public int CurrentSpotIndex = -1;

        public CubeButtonInfo(CubeButtonInfo cubeButtonInfo)
        {
            CubeColor = cubeButtonInfo.CubeColor;
            Count = cubeButtonInfo.Count;
            CanSelect = cubeButtonInfo.CanSelect;
            ColumnIndex = cubeButtonInfo.ColumnIndex;
            StartRowIndex = cubeButtonInfo.StartRowIndex;
            CurrentRowIndex = cubeButtonInfo.CurrentRowIndex;
            CurrentSpotIndex = cubeButtonInfo.CurrentSpotIndex;
        }
    }

    public class CubeButton : MonoBehaviour, IPointerClickHandler
    {
        public CubeButtonInfo CubeButtonInfo;

        public Image Image;
        public Text CountText;

        public Action<CubeButton> OnClicked;

        private Sequence sequence;
        private bool IsAnimating;
        public ParticleSystem ShootPS;

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
            if (CubeButtonInfo.CurrentRowIndex == 0)
            {
                CubeButtonInfo.CanSelect = true;
            }

            CountText.text = CubeButtonInfo.Count.ToString();

            transform.localScale = new Vector3(1.25f, 1.25f, 1.25f);
            transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutSine);
        }

        public void SetSpotIndex(int index)
        {
            CubeButtonInfo.CurrentSpotIndex = index;
            CubeButtonInfo.CurrentRowIndex = -1;
            CubeButtonInfo.CanSelect = false;
        }

        public void SetRowIndex(int index)
        {
            CubeButtonInfo.CurrentRowIndex = index;
            if (CubeButtonInfo.CurrentRowIndex == 0)
            {
                CubeButtonInfo.CanSelect = true;
            }
        }

        public void OnPointerClick(PointerEventData eventData)
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

            transform.DOShakeRotation(1f, new Vector3(0f, 0f, 15f)).SetEase(Ease.OutSine).SetDelay(0.1f).OnComplete(() =>
            {
                IsAnimating = false;
            });
        }
        public void StartAnimating()
        {
            CountText.DOFade(1f, 0.5f).SetEase(Ease.OutSine);

            sequence = DOTween.Sequence();
            sequence.Append(transform.DOScale(1.025f, 0.5f).SetEase(Ease.OutSine));
            sequence.Append(transform.DOScale(1f, 0.5f).SetEase(Ease.OutSine));
            sequence.Play().SetLoops(-1);
        }
        public void StopAnimating()
        {
            sequence.Kill();
            transform.localScale = Vector3.one;
        }

        public void ShowShootEffect()
        {
            ShootPS.Emit(1);
            ShootPS.Play();
        }
    }
}
