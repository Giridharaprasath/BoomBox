using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace BoomBox
{
    public class StarManager : MonoBehaviour
    {
        [Header("Star")]
        public Text StarCountText;
        private int StarCount = 0;

        [Header("Level Complete Page")]
        public GameObject LevelCompletePage;
        public Image PageBG;
        public GameObject CompletedTextObject;
        public GameObject StarAddObject;
        public GameObject ContinueButton;
        public Text ContinueText;

        [Header("Star Collect Effect")]
        public GameObject StarMoveParentObject;
        public GameObject[] StarMoveObjects;
        public Transform StarTargetPosition;

        private int TempStarCount;

        private void Start()
        {
            StarCount = PlayerPrefs.GetInt("StarCount", 0);
            StarCountText.text = StarCount.ToString();
        }
        private void OnDestroy()
        {
            DOTween.KillAll();
        }

        public void OnLevelCompleted()
        {
            ShowParticleEffect();
            TempStarCount = StarCount;
            SetStarCount(StarCount + 10);
        }

        private void ShowParticleEffect()
        {
            Invoke(nameof(ShowCompletedPage), 2f);
        }
        private void ShowCompletedPage()
        {
            LevelCompletePage.SetActive(true);
            PageBG.DOFade(0.9f, 0.75f).SetEase(Ease.Linear);
            Invoke(nameof(ShowCompletedText), 1f);
        }

        private void ShowCompletedText()
        {
            LevelCompletePage.SetActive(true);
            CompletedTextObject.SetActive(true);
            AnimateCompletedText();
        }
        private void AnimateCompletedText()
        {
            Sequence sequence = DOTween.Sequence();
            sequence.Append(CompletedTextObject.transform.DOScale(3f, 1f).SetEase(Ease.OutBounce)
            .OnComplete(() =>
            {
                ShowAddStarObject();
            }));
            sequence.Append(CompletedTextObject.transform.DOLocalMoveY(1000f, 1f).SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                ShowContinueButton();
            }));
            sequence.Play();
        }

        private void ShowAddStarObject()
        {
            StarAddObject.SetActive(true);
            StartCoroutine(AnimateStarCount());
            AnimateMoveStar();

            Invoke(nameof(PlayVibration), 0.75f);
        }
        private void AnimateMoveStar()
        {
            StarMoveParentObject.SetActive(true);
            float scaleDuration = 0.5f;
            float moveDuration = 1f;
            Vector3 targetScale = new(2.25f, 2.25f, 2.25f);
            float punchDuration = 1f;
            float delay = 0;

            for (int i = 0; i < StarMoveObjects.Length; i++)
            {
                StarMoveObjects[i].transform.localScale = Vector3.zero;
                StarMoveObjects[i].transform.DOScale(targetScale, scaleDuration).SetDelay(delay).SetEase(Ease.OutBack);
                StarMoveObjects[i].transform.DOMove(StarTargetPosition.position, moveDuration).SetDelay(delay + 0.5f).SetEase(Ease.InBack);
                StarMoveObjects[i].transform.DORotate(new(0f, 0f, 375f), punchDuration, RotateMode.FastBeyond360).SetDelay(delay + 0.5f).SetEase(Ease.Flash);
                StarMoveObjects[i].transform.DOScale(Vector3.zero, scaleDuration).SetDelay(delay + 1.8f).SetEase(Ease.OutBack);
                delay += 0.1f;
            }
        }
        private void PlayVibration() => VibrationManager.VibrateStarCollect();
        private IEnumerator AnimateStarCount()
        {
            yield return new WaitForSeconds(1.25f);
            while(TempStarCount < StarCount)
            {
                yield return new WaitForSeconds(0.1f);
                ++TempStarCount;
                StarCountText.text = TempStarCount.ToString();
            }
        }
        public void SetStarCount(int count)
        {
            StarCount = count;
            PlayerPrefs.SetInt("StarCount", StarCount);
        }
        public void ShowContinueButton()
        {
            ContinueButton.SetActive(true);
            AnimateContinueText();
        }
        private void AnimateContinueText()
        {
            Sequence sequence = DOTween.Sequence();
            sequence.Append(ContinueText.DOFade(1, 1f).SetEase(Ease.Linear));
            sequence.Append(ContinueText.DOFade(0, 1f).SetEase(Ease.Linear));
            sequence.Play().SetLoops(-1);
        }
    }
}
