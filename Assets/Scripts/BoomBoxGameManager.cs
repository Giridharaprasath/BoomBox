using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace BoomBox
{
    public enum CubeColor
    {
        Yellow,
        Red,
        Green,
        Blue,
        Pink,
        Orange,
    }

    public class BoomBoxGameManager : MonoBehaviour
    {
        [Header("Level Objects")]
        public GameObject CubePlaceSpotParentObject;
        public GameObject CubeButtonParentObject;
        public List<GameObject> CubeButtonsParentObject;
        public GameObject BoxButtonParentObject;
        public List<GameObject> BoxButtonsParentObject;

        public GameObject CubePlaceSpotPrefab;
        public GameObject CubeButtonRowPrefab;
        public GameObject CubeButtonColumnPrefab;
        public CubeButton CubeButtonPrefab;

        public GameObject BoxButtonRowPrefab;
        public GameObject BoxButtonColumnPrefab;
        public BoxButton BoxButtonPrefab;

        public GameObject CubeLeftSpot, CubeRightSpot;

        public List<Sprite> CubeColorSprites;

        [Header("Level Objects Created")]
        public List<GameObject> CubePlaceSpots;
        public List<CubeButton> CubeButtons;
        public List<BoxButton> BoxButtons;

        public List<BoomBoxLevels> AllBoomBoxLevels;
        public BoomBoxLevels CurrentBoomBoxLevel { get; set; }
        public int CurrentLevelNumber;

        public List<bool> IsCubePlaceSpotOccupied { get; set; }

        public List<CubeButton> CubeButtonsInPlaceSpots;

        public List<BoxButton> BoxButtonToShoot;

        [Header("Level Complete")]
        public StarManager starManager;

        [Header("Reload Button")]
        public Button ReloadButton;
        public Transform ReloadButtonStartPoint, ReloadButtonEndPoint;

        [Header("Progress Bar")]
        public GameObject ProgressPanel;
        public Image ProgressBar;
        public Text[] LevelText;
        public Transform ProgressPanelStartPoint, ProgressPanelEndPoint;

        [Header("Add Extra Place Slot")]
        private const int maxSlots = 7;
        public int CurrentSlotCount;
        public Button GetExtraSlot;
        public Text NextSlotRateText;

        private int BoxButtonShootCount = 0;
        private bool CheckShoot = false;

        private void Start()
        {
            VibrationManager.Init();
            DOTween.SetTweensCapacity(2000, 100);

            CurrentLevelNumber = PlayerPrefs.GetInt("CurrentLevelNumber", 1);

            CurrentBoomBoxLevel = AllBoomBoxLevels[CurrentLevelNumber - 1];

            CreateCubePlaceSpots();
            CreateBoxButtons();
            CreateCubeButtons();

            IsCubePlaceSpotOccupied = new List<bool>(new bool[CubePlaceSpots.Count]);
            CurrentSlotCount = CubePlaceSpots.Count;

            ReloadButton.onClick.AddListener(LoadScene);
            ShowReloadButton();

            foreach (var item in LevelText)
            {
                item.text = "Level " + CurrentLevelNumber;
            }
            ShowProgressBar();

            GetExtraSlot.onClick.AddListener(BuyExtraSlotProcess);
            NextSlotRateText.text = CurrentSlotCount.ToString();

            if (CurrentLevelNumber == 1)
                GetExtraSlot.gameObject.SetActive(false);
        }
        private void OnDestroy()
        {
            DOTween.KillAll();
        }

        public void SetCurrentLevel(string changeValue)
        {
            if (changeValue.Equals("+"))
            {
                CurrentLevelNumber++;
            }
            else
            {
                CurrentLevelNumber--;
            }

            if (CurrentLevelNumber < 1) CurrentLevelNumber = AllBoomBoxLevels.Count;
            if (CurrentLevelNumber > AllBoomBoxLevels.Count) CurrentLevelNumber = 1;

            LoadScene();
        }
        public void LoadScene()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        #region Level Objects Creation
        private void CreateCubePlaceSpots()
        {
            for (int i = 0; i < CurrentBoomBoxLevel.CubePlaceSlotCount; i++)
            {
                GameObject cubePlaceSpot = Instantiate(CubePlaceSpotPrefab, CubePlaceSpotParentObject.transform);
                cubePlaceSpot.name = $"Cube PS{i}";

                CubePlaceSpots.Add(cubePlaceSpot);
            }
        }

        private void CreateBoxButtons()
        {
            for (int i = 0; i < CurrentBoomBoxLevel.BoxButtonRowCount; i++)
            {
                GameObject boxButtonRowObject = Instantiate(BoxButtonRowPrefab, BoxButtonParentObject.transform);
                boxButtonRowObject.name = $"Box BR{i}";

                for (int j = 0; j < CurrentBoomBoxLevel.BoxButtonColumnCount; j++)
                {
                    GameObject boxButtonColumnObject = Instantiate(BoxButtonColumnPrefab, boxButtonRowObject.transform);
                    boxButtonColumnObject.name = $"Box BC{j}";

                    BoxButtonsParentObject.Add(boxButtonColumnObject);
                }

                boxButtonRowObject.transform.SetAsFirstSibling();
            }

            for (int i = 0; i < CurrentBoomBoxLevel.BoxButtonInfos.Count; i++)
            {
                BoomBoxLevels.BoxButtonRowInfo item = CurrentBoomBoxLevel.BoxButtonInfos[i];

                for (int j = 0; j < item.BoxButtonSetRows.Count; j++)
                {
                    BoomBoxLevels.BoxButtonSetRow boxButtonSetRow = item.BoxButtonSetRows[j];

                    for (int k = boxButtonSetRow.StartColumnIndex; k <= boxButtonSetRow.EndColumnIndex; k++)
                    {
                        BoxButton boxButton = Instantiate(BoxButtonPrefab, BoxButtonsParentObject[i * CurrentBoomBoxLevel.BoxButtonColumnCount + k].transform);
                        boxButton.name = $"Box B{i * CurrentBoomBoxLevel.BoxButtonColumnCount + k}";

                        boxButton.BoxButtonInfo = new()
                        {
                            CubeColor = boxButtonSetRow.BoxColor,
                            ColumnIndex = k,
                            StartRowIndex = i,
                            CurrentRowIndex = i,
                        };

                        boxButton.BoxSprite = boxButtonSetRow.BoxColor switch
                        {
                            CubeColor.Yellow => CubeColorSprites[0],
                            CubeColor.Red => CubeColorSprites[1],
                            CubeColor.Green => CubeColorSprites[2],
                            CubeColor.Blue => CubeColorSprites[3],
                            CubeColor.Pink => CubeColorSprites[4],
                            CubeColor.Orange => CubeColorSprites[5],
                            _ => CubeColorSprites[6],
                        };

                        boxButton.TotalCount = boxButtonSetRow.Height;
                        boxButton.CurrentCount = boxButtonSetRow.Height;

                        if (boxButton.BoxButtonInfo.CurrentRowIndex == 0)
                        {
                            boxButton.BoxButtonInfo.CanShoot = true;
                            AddBoxButtonToBeShot(boxButton);
                        }

                        BoxButtons.Add(boxButton);
                        boxButton.OnCanShoot += AddBoxButtonToBeShot;
                        boxButton.OnAllHit += MoveBoxColumnDown;
                    }
                }
            }
        }

        private void CreateCubeButtons()
        {
            for (int i = 0; i < CurrentBoomBoxLevel.CubeButtonRowCount; i++)
            {
                GameObject cubeButtonRowObject = Instantiate(CubeButtonRowPrefab, CubeButtonParentObject.transform);
                cubeButtonRowObject.name = $"Cube BR{i}";

                for (int j = 0; j < CurrentBoomBoxLevel.CubeButtonColumnCount; j++)
                {
                    GameObject cubeButtonColumnObject = Instantiate(CubeButtonColumnPrefab, cubeButtonRowObject.transform);
                    cubeButtonColumnObject.name = $"Cube BC{j}";

                    CubeButtonsParentObject.Add(cubeButtonColumnObject);
                }
            }

            for (int i = 0; i < CurrentBoomBoxLevel.CubeButtonCount; i++)
            {
                CubeButton cubeButton = Instantiate(CubeButtonPrefab, CubeButtonsParentObject[i].transform);
                cubeButton.name = $"Cube B{i}";

                cubeButton.CubeButtonInfo = new CubeButtonInfo(CurrentBoomBoxLevel.CubeButtonInfos[i])
                {
                    ColumnIndex = i % CurrentBoomBoxLevel.CubeButtonColumnCount,
                    StartRowIndex = i / CurrentBoomBoxLevel.CubeButtonColumnCount,
                    CurrentRowIndex = i / CurrentBoomBoxLevel.CubeButtonColumnCount,
                };

                cubeButton.Image.sprite = cubeButton.CubeButtonInfo.CubeColor switch
                {
                    CubeColor.Yellow => CubeColorSprites[0],
                    CubeColor.Red => CubeColorSprites[1],
                    CubeColor.Green => CubeColorSprites[2],
                    CubeColor.Blue => CubeColorSprites[3],
                    CubeColor.Pink => CubeColorSprites[4],
                    CubeColor.Orange => CubeColorSprites[5],
                    _ => CubeColorSprites[6],
                };

                CubeButtons.Add(cubeButton);
                cubeButton.OnClicked += AddCubeButtonToCubePlaceSpot;
            }
        }
        #endregion

        #region Cube Button Movement
        public void AddCubeButtonToCubePlaceSpot(CubeButton cubeButton)
        {
            if (!cubeButton.CubeButtonInfo.CanSelect)
            {
                cubeButton.OnClickShake();
                VibrationManager.VibrateNope();
                return;
            }

            int freeSpotIndex = GetFirstFreeCubePlaceSpot();
            if (freeSpotIndex == -1)
            {
                cubeButton.OnClickShake();
                VibrationManager.VibrateNope();
                return;
            }

            VibrationManager.VibratePeek();

            IsCubePlaceSpotOccupied[freeSpotIndex] = true;

            cubeButton.SetSpotIndex(freeSpotIndex);
            cubeButton.transform.DOJump(CubePlaceSpots[freeSpotIndex].transform.position, 1.5f, 2, 0.75f).SetEase(Ease.OutQuad).OnComplete(() =>
            {
                cubeButton.transform.SetParent(CubePlaceSpots[freeSpotIndex].transform);
                cubeButton.transform.localPosition = Vector3.zero;

                AddCubeButtonToSpot(cubeButton);
                cubeButton.StartAnimating();
            });

            MoveCubeColumnUp(cubeButton.CubeButtonInfo.ColumnIndex);
        }

        private void MoveCubeColumnUp(int ColumnIndex)
        {
            for (int i = 0; i < CubeButtons.Count; i++)
            {
                if (CubeButtons[i].CubeButtonInfo.ColumnIndex != ColumnIndex) continue;

                if (CubeButtons[i].CubeButtonInfo.CurrentRowIndex == -1) continue;

                int index = i;
                CubeButton currentButton = CubeButtons[index];
                currentButton.SetRowIndex(currentButton.CubeButtonInfo.CurrentRowIndex - 1);
                int nextPos = index - (CurrentBoomBoxLevel.CubeButtonColumnCount * (currentButton.CubeButtonInfo.StartRowIndex - currentButton.CubeButtonInfo.CurrentRowIndex));
                currentButton.transform.DOMove(CubeButtonsParentObject[nextPos].transform.position, 0.5f).SetEase(Ease.OutSine).OnComplete(() =>
                {
                    currentButton.transform.SetParent(CubeButtonsParentObject[nextPos].transform);
                    currentButton.transform.localPosition = Vector3.zero;
                });
            }
        }

        private int GetFirstFreeCubePlaceSpot()
        {
            for (int i = 0; i < IsCubePlaceSpotOccupied.Count; i++)
            {
                if (!IsCubePlaceSpotOccupied[i])
                {
                    return i;
                }
            }

            return -1;
        }
        #endregion

        #region Cube Place Spots Shotting
        public void AddCubeButtonToSpot(CubeButton cubeButton)
        {
            if (CubeButtonsInPlaceSpots.Contains(cubeButton)) return;

            CubeButtonsInPlaceSpots.Add(cubeButton);
            IsCubePlaceSpotOccupied[cubeButton.CubeButtonInfo.CurrentSpotIndex] = true;
            OnCubePlaceSpotUpdated();
        }
        public void RemoveCubeButtonFromSpot(CubeButton cubeButton)
        {
            if (!CubeButtonsInPlaceSpots.Contains(cubeButton)) return;

            CubeButtonsInPlaceSpots.Remove(cubeButton);
            IsCubePlaceSpotOccupied[cubeButton.CubeButtonInfo.CurrentSpotIndex] = false;
            OnCubePlaceSpotUpdated();
        }
        public void OnCubePlaceSpotUpdated()
        {
            // Debug.Log($"GG : Cube Place Spot Updated");
            CheckShoot = CubeButtonsInPlaceSpots.Count != 0;
        }
        public void AddBoxButtonToBeShot(BoxButton boxButton)
        {
            if (BoxButtonToShoot.Contains(boxButton)) return;

            BoxButtonToShoot.Add(boxButton);
        }

        private void FixedUpdate()
        {
            if (!CheckShoot) return;

            if (CubeButtonsInPlaceSpots.Count == 0)
            {
                CheckShoot = false;
                return;
            }

            for (int i = 0; i < CubeButtonsInPlaceSpots.Count; i++)
            {
                CubeButton item = CubeButtonsInPlaceSpots[i];
                if (item == null) continue;

                if (item.CubeButtonInfo.Count == 0) continue;

                CheckShootForCube(item);
            }
        }

        private void CheckShootForCube(CubeButton cubeButton)
        {
            foreach (BoxButton box in BoxButtonToShoot)
            {
                if (!box.BoxButtonInfo.CanShoot) continue;

                if (box.BoxButtonInfo.CubeColor != cubeButton.CubeButtonInfo.CubeColor) continue;

                BoxButtonShootCount++;
                // Debug.Log($"GG : {(float)BoxButtonShootCount / CurrentBoomBoxLevel.BoxButtonCount}");
                ProgressBar.fillAmount = (float)BoxButtonShootCount / CurrentBoomBoxLevel.BoxButtonCount;

                cubeButton.DecreaseCount();

                int mul = cubeButton.CubeButtonInfo.CurrentSpotIndex < IsCubePlaceSpotOccupied.Count / 2 ? -1 : 1;
                cubeButton.transform.DOLocalRotate(new Vector3(0f, 0f, Random.Range(25 * mul, 50 * mul)), 0.03f).OnComplete(() => cubeButton.transform.rotation = Quaternion.identity);

                cubeButton.ShowShootEffect();
                if (cubeButton.CubeButtonInfo.Count == 0)
                {
                    AnimateBoxOutOfSpot(cubeButton);
                }

                BoxButtonToShoot.Remove(box);
                box.OnHit();

                VibrationManager.VibratePop();

                if (BoxButtonShootCount == CurrentBoomBoxLevel.BoxButtonCount)
                {
                    OnLevelCompleted();
                    CheckShoot = false;
                }

                return;
            }
        }
        #endregion

        #region Box Button Movement
        public void MoveBoxColumnDown(int ColumnIndex)
        {
            for (int i = 0; i < BoxButtons.Count; i++)
            {
                if (BoxButtons[i].BoxButtonInfo.ColumnIndex != ColumnIndex) continue;

                if (BoxButtons[i].BoxButtonInfo.CurrentRowIndex == 0) continue;

                int index = i;
                BoxButton currentButton = BoxButtons[index];
                currentButton.SetRowIndex(currentButton.BoxButtonInfo.CurrentRowIndex - 1);
                int nextPos = index + (CurrentBoomBoxLevel.BoxButtonColumnCount * (currentButton.BoxButtonInfo.CurrentRowIndex - currentButton.BoxButtonInfo.StartRowIndex));
                currentButton.transform.DOMove(BoxButtonsParentObject[nextPos].transform.position, 0.5f).SetEase(Ease.OutSine).OnComplete(() =>
                {
                    currentButton.transform.SetParent(BoxButtonsParentObject[nextPos].transform);
                    currentButton.transform.localPosition = Vector3.zero;
                });
            }
        }
        private void AnimateBoxOutOfSpot(CubeButton cubeButton)
        {
            Transform endTransform = cubeButton.CubeButtonInfo.CurrentSpotIndex < IsCubePlaceSpotOccupied.Count / 2 ? CubeLeftSpot.transform : CubeRightSpot.transform;

            cubeButton.StopAnimating();
            cubeButton.transform.SetParent(endTransform, true);

            Sequence sequence = DOTween.Sequence();
            sequence.Append(cubeButton.transform.DOScale(0.75f, 0.5f).SetEase(Ease.Linear)
                .OnComplete(() =>
                {
                    RemoveCubeButtonFromSpot(cubeButton);
                }));
            sequence.Append(cubeButton.transform.DOJump(endTransform.position, 1.5f, 2, 1f).SetEase(Ease.OutSine));
            sequence.Play().OnComplete(() =>
            {
                cubeButton.transform.SetParent(endTransform);
                cubeButton.transform.localPosition = Vector3.zero;
                cubeButton.transform.localScale = Vector3.one;
                cubeButton.gameObject.SetActive(false);
            });
        }
        #endregion

        #region Reload Button
        private void ShowReloadButton()
        {
            ReloadButton.transform.DOMove(ReloadButtonEndPoint.position, 0.5f).SetEase(Ease.InSine).OnComplete(() =>
            {
                ReloadButton.enabled = true;
            });
        }
        private void HideReloadButton()
        {
            ReloadButton.enabled = false;
            ReloadButton.transform.DOMove(ReloadButtonStartPoint.position, 0.5f).SetEase(Ease.InSine);
        }
        #endregion

        #region Show Progress Bar
        public void ShowProgressBar()
        {
            ProgressPanel.transform.DOMove(ProgressPanelEndPoint.position, 0.5f).SetEase(Ease.InOutSine);
        }
        public void HideProgressBar()
        {
            ProgressPanel.transform.DOMove(ProgressPanelStartPoint.position, 0.5f).SetEase(Ease.OutBack);
        }
        #endregion

        #region Extra Slot Process
        private void BuyExtraSlotProcess()
        {
            if (CurrentSlotCount == maxSlots) return;

            int nextRate = CurrentSlotCount;
            int currentStarCount = starManager.GetStarCount();

            if (currentStarCount < nextRate)
            {
                VibrationManager.VibrateNope();
                return;
            }

            CurrentSlotCount++;
            IsCubePlaceSpotOccupied.Add(false);
            GameObject cubePlaceSpot = Instantiate(CubePlaceSpotPrefab, CubePlaceSpotParentObject.transform);
            cubePlaceSpot.name = $"Cube PS{CurrentSlotCount}";

            CubePlaceSpots.Add(cubePlaceSpot);

            starManager.SetStarCount(currentStarCount - nextRate);
            starManager.SetStarCountText();

            NextSlotRateText.text = CurrentSlotCount.ToString();

            if (CurrentSlotCount == maxSlots)
            {
                GetExtraSlot.gameObject.SetActive(false);
            }
        }
        #endregion

        private void OnLevelCompleted()
        {
            Debug.Log($"GG : Level Completed");
            starManager.OnLevelCompleted();
            HideReloadButton();
            HideProgressBar();

            SaveLevelProgress();
        }

        private void SaveLevelProgress()
        {
            CurrentLevelNumber++;

            if (CurrentLevelNumber > AllBoomBoxLevels.Count) CurrentLevelNumber = 1;

            PlayerPrefs.SetInt("CurrentLevelNumber", CurrentLevelNumber);
        }
    }
}
