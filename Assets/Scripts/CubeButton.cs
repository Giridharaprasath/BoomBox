using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

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

        private void Start()
        {
            InitCubeButton();
        }

        public void InitCubeButton()
        {
            if (CubeButtonInfo.CurrentRowIndex == 0)
            {
                CubeButtonInfo.CanSelect = true;
            }

            Image.color = CubeButtonInfo.CubeColor switch
            {
                CubeColor.Yellow => Color.yellow,
                CubeColor.Red => Color.red,
                CubeColor.Green => Color.green,
                CubeColor.Blue => Color.blue,
                CubeColor.Pink => new Color(1f, 0.75f, 0.8f),
                CubeColor.Orange => new Color(1f, 0.5f, 0f),
                _ => Color.white,
            };

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
    }
}
