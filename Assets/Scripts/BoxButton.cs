using System;
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
        public Image Image;

        public Action<BoxButton> OnCanShoot;

        private void Start()
        {
            InitCubeButton();
        }

        public void InitCubeButton()
        {
            Image.color = BoxButtonInfo.CubeColor switch
            {
                CubeColor.Yellow => Color.yellow,
                CubeColor.Red => Color.red,
                CubeColor.Green => Color.green,
                CubeColor.Blue => Color.blue,
                CubeColor.Pink => new Color(1f, 0.75f, 0.8f),
                CubeColor.Orange => new Color(1f, 0.5f, 0f),
                _ => Color.white,
            };

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
    }
}
