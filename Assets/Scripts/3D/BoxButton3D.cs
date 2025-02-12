using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace BoomBox
{
    public class BoxButton3D : MonoBehaviour
    {
        public BoxButtonInfo BoxButtonInfo;

        public GameObject OtherBoxObject;
        public List<GameObject> OtherBoxes;
        public Material BoxColor;
        public int TotalCount;
        public int CurrentCount;
        
        public Action<BoxButton3D> OnCanShoot;
        public Action<int> OnAllHit;

        private void Start()
        {
            InitBoxButton();
        }

        private void OnDestroy()
        {
            DOTween.Kill(this);
        }

        public void InitBoxButton()
        {
            for (int i = 0; i < TotalCount; i++)
            {
                GameObject box = Instantiate(OtherBoxObject, transform);
                box.transform.localPosition = new Vector3(0f, 0f, -i);
                box.GetComponent<Renderer>().material = BoxColor;
                OtherBoxes.Add(box);
            }

            if (BoxButtonInfo.CurrentRowIndex < 10)
            {
                transform.localPosition = new Vector3(0f, 0f, -15f);
                transform.DOLocalMove(Vector3.zero, 0.5f).SetEase(Ease.OutQuad);
            }
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

        public void StartShrinking()
        {
            CurrentCount--;
            AnimateObject(OtherBoxes[CurrentCount]);
        }

        private void AnimateObject(GameObject gameObject)
        {
            gameObject.transform.DOScale(Vector3.zero, 0.25f).SetEase(Ease.Linear).OnComplete(() =>
            {
                gameObject.SetActive(false);
            });
        }
    }
}
