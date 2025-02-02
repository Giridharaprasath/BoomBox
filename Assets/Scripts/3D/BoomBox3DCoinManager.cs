using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BoomBox
{
    public class BoomBox3DCoinManager : MonoBehaviour
    {

        [Header("Coin")]
        public TMP_Text CoinCountText;
        private int CoinCount = 0;

        [Header("Level Complete Page")]
        public GameObject ConfettiEffect;
        public GameObject LevelCompletePage;
        public Image PageBG;
        public GameObject CompletedTextObject;
        public GameObject CoinAddObject;
        public GameObject ContinueButton;
        public TMP_Text ContinueText;

        [Header("Coin Collect Effect")]
        public GameObject CoinMoveParentObject;
        public GameObject[] CoinMoveObjects;
        public Transform CoinTargetPosition;

        private int TempCoinCount;

        private void Start()
        {
            CoinCount = PlayerPrefs.GetInt("CoinCount", 0);
            SetCoinCountText();
        }
        private void OnDestroy()
        {
            DOTween.KillAll();
        }

        public int GetCoinCount()
        {
            return CoinCount;
        }

        public void OnLevelCompleted()
        {
            ShowParticleEffect();
            TempCoinCount = CoinCount;
            SetCoinCount(CoinCount + 10);
        }

        private void ShowParticleEffect()
        {
            ConfettiEffect.SetActive(true);
            LevelCompletePage.SetActive(true);
            Invoke(nameof(ShowCompletedPage), 2f);
        }
        private void ShowCompletedPage()
        {
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
                ShowAddCoinObject();
            }));
            sequence.Append(CompletedTextObject.transform.DOLocalMoveY(1000f, 1f).SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                ShowContinueButton();
            }));
            sequence.Play();
        }

        private void ShowAddCoinObject()
        {
            CoinAddObject.SetActive(true);
            StartCoroutine(AnimateStarCount());
            AnimateMoveCoin();

            Invoke(nameof(PlayVibration), 0.75f);
        }
        private void AnimateMoveCoin()
        {
            CoinMoveParentObject.SetActive(true);
            float scaleDuration = 0.5f;
            float moveDuration = 1f;
            float delay = 0;

            for (int i = 0; i < CoinMoveObjects.Length; i++)
            {
                CoinMoveObjects[i].transform.DOMove(CoinTargetPosition.position, moveDuration).SetDelay(delay + 0.5f).SetEase(Ease.Linear);
                CoinMoveObjects[i].transform.DOScale(Vector3.zero, scaleDuration).SetDelay(delay + 1.8f).SetEase(Ease.Linear);
                delay += 0.1f;
            }
        }
        private void PlayVibration() => VibrationManager.VibrateStarCollect();
        private IEnumerator AnimateStarCount()
        {
            yield return new WaitForSeconds(0.5f);
            while (TempCoinCount < CoinCount)
            {
                yield return new WaitForSeconds(0.1f);
                ++TempCoinCount;
                CoinCountText.text = TempCoinCount.ToString();
            }
        }
        public void SetCoinCount(int count)
        {
            CoinCount = count;
            PlayerPrefs.SetInt("CoinCount", CoinCount);
        }
        public void SetCoinCountText()
        {
            CoinCountText.text = CoinCount.ToString();
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
