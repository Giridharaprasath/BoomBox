using DG.Tweening;
using UnityEngine;

namespace BoomBox
{
    public class BoomBox3DExtraSlotButton : MonoBehaviour
    {
        public BoomBox3DGameManager GameManager;
        public GameObject ButtonObject;
        private bool bIsAnimating;

        private void OnMouseDown()
        {
            DoButtonClickAnimation();
            GameManager.BuyExtraSlotProcess();
        }

        private void DoButtonClickAnimation()
        {
            if (bIsAnimating) return;

            bIsAnimating = true;

            ButtonObject.transform.DOLocalMoveY(-0.1f, 0.25f).SetEase(Ease.OutQuad).SetLoops(1, LoopType.Yoyo).OnComplete(() =>
            {
                ButtonObject.transform.localPosition = Vector3.zero;
                bIsAnimating = false;
            });
        }
    }
}
