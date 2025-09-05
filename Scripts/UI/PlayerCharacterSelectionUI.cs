using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using DungeonOwner.Data;
using DungeonOwner.Managers;
using TMPro;

namespace DungeonOwner.UI
{
    /// <summary>
    /// プレイヤーキャラクター選択UIを管理するクラス
    /// 要件8.1: ゲーム開始時に選択可能な自キャラクタータイプを表示
    /// </summary>
    public class PlayerCharacterSelectionUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject selectionPanel;
        [SerializeField] private Transform characterButtonContainer;
        [SerializeField] private GameObject characterButtonPrefab;
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button cancelButton;
        
        [Header("Character Info Display")]
        [SerializeField] private Image characterPortrait;
        [SerializeField] private TextMeshProUGUI characterNameText;
        [SerializeField] private TextMeshProUGUI characterDescriptionText;
        [SerializeField] private TextMeshProUGUI healthText;
        [SerializeField] private TextMeshProUGUI manaText;
        [SerializeField] private TextMeshProUGUI attackPowerText;
        [SerializeField] private TextMeshProUGUI abilitiesText;
        
        private PlayerCharacterManager playerCharacterManager;
        private List<PlayerCharacterButton> characterButtons = new List<PlayerCharacterButton>();
        private PlayerCharacterType selectedCharacterType = PlayerCharacterType.Warrior;
        private PlayerCharacterData selectedCharacterData;
        
        // イベント
        public System.Action<PlayerCharacterType> OnCharacterSelected;
        public System.Action OnSelectionConfirmed;
        public System.Action OnSelectionCancelled;
        
        private void Awake()
        {
            playerCharacterManager = FindObjectOfType<PlayerCharacterManager>();
            
            if (playerCharacterManager == null)
            {
                Debug.LogError("PlayerCharacterManager not found! PlayerCharacterSelectionUI requires PlayerCharacterManager.");
            }
            
            SetupButtons();
        }
        
        private void Start()
        {
            CreateCharacterButtons();
            UpdateCharacterInfo();
            
            // 初期状態では非表示
            Hide();
        }
        
        private void SetupButtons()
        {
            if (confirmButton != null)
            {
                confirmButton.onClick.AddListener(ConfirmSelection);
            }
            
            if (cancelButton != null)
            {
                cancelButton.onClick.AddListener(CancelSelection);
            }
        }
        
        /// <summary>
        /// キャラクター選択ボタンを作成
        /// </summary>
        private void CreateCharacterButtons()
        {
            if (playerCharacterManager == null || characterButtonPrefab == null || characterButtonContainer == null)
            {
                Debug.LogError("Required components not assigned for character button creation");
                return;
            }
            
            // 既存のボタンをクリア
            ClearCharacterButtons();
            
            // 利用可能なキャラクタータイプを取得
            List<PlayerCharacterType> availableTypes = playerCharacterManager.GetAvailableCharacterTypes();
            
            foreach (PlayerCharacterType characterType in availableTypes)
            {
                CreateCharacterButton(characterType);
            }
            
            // 最初のキャラクターを選択
            if (characterButtons.Count > 0)
            {
                SelectCharacter(availableTypes[0]);
            }
        }
        
        private void CreateCharacterButton(PlayerCharacterType characterType)
        {
            GameObject buttonObject = Instantiate(characterButtonPrefab, characterButtonContainer);
            PlayerCharacterButton characterButton = buttonObject.GetComponent<PlayerCharacterButton>();
            
            if (characterButton == null)
            {
                characterButton = buttonObject.AddComponent<PlayerCharacterButton>();
            }
            
            // キャラクターデータを取得
            PlayerCharacterData characterData = playerCharacterManager.GetCharacterData(characterType);
            if (characterData == null)
            {
                Debug.LogError($"Character data not found for type: {characterType}");
                Destroy(buttonObject);
                return;
            }
            
            // ボタンを設定
            characterButton.Setup(characterData, () => SelectCharacter(characterType));
            characterButtons.Add(characterButton);
        }
        
        private void ClearCharacterButtons()
        {
            foreach (var button in characterButtons)
            {
                if (button != null)
                {
                    Destroy(button.gameObject);
                }
            }
            characterButtons.Clear();
        }
        
        /// <summary>
        /// キャラクターを選択
        /// </summary>
        public void SelectCharacter(PlayerCharacterType characterType)
        {
            selectedCharacterType = characterType;
            selectedCharacterData = playerCharacterManager.GetCharacterData(characterType);
            
            // ボタンの選択状態を更新
            UpdateButtonSelection();
            
            // キャラクター情報を更新
            UpdateCharacterInfo();
            
            OnCharacterSelected?.Invoke(characterType);
            Debug.Log($"Character selected: {characterType}");
        }
        
        private void UpdateButtonSelection()
        {
            foreach (var button in characterButtons)
            {
                if (button != null)
                {
                    button.SetSelected(button.CharacterType == selectedCharacterType);
                }
            }
        }
        
        /// <summary>
        /// キャラクター情報表示を更新
        /// </summary>
        private void UpdateCharacterInfo()
        {
            if (selectedCharacterData == null) return;
            
            // 基本情報
            if (characterPortrait != null)
            {
                characterPortrait.sprite = selectedCharacterData.portrait;
            }
            
            if (characterNameText != null)
            {
                characterNameText.text = selectedCharacterData.displayName;
            }
            
            if (characterDescriptionText != null)
            {
                characterDescriptionText.text = selectedCharacterData.description;
            }
            
            // ステータス情報（レベル1での値）
            if (healthText != null)
            {
                healthText.text = $"HP: {selectedCharacterData.GetHealthAtLevel(1):F0}";
            }
            
            if (manaText != null)
            {
                manaText.text = $"MP: {selectedCharacterData.GetManaAtLevel(1):F0}";
            }
            
            if (attackPowerText != null)
            {
                attackPowerText.text = $"攻撃力: {selectedCharacterData.GetAttackPowerAtLevel(1):F0}";
            }
            
            // アビリティ情報
            if (abilitiesText != null)
            {
                string abilitiesString = "";
                foreach (var ability in selectedCharacterData.abilities)
                {
                    if (!string.IsNullOrEmpty(abilitiesString))
                    {
                        abilitiesString += "\n";
                    }
                    abilitiesString += GetAbilityDescription(ability);
                }
                abilitiesText.text = abilitiesString;
            }
        }
        
        private string GetAbilityDescription(PlayerAbilityType abilityType)
        {
            switch (abilityType)
            {
                case PlayerAbilityType.PowerStrike:
                    return "パワーストライク: 強力な一撃攻撃";
                case PlayerAbilityType.MagicMissile:
                    return "マジックミサイル: 複数の魔法弾を発射";
                case PlayerAbilityType.QuickStep:
                    return "クイックステップ: 素早い移動";
                case PlayerAbilityType.Blessing:
                    return "祝福: パーティの能力値上昇";
                case PlayerAbilityType.Taunt:
                    return "挑発: 敵の注意を引く";
                case PlayerAbilityType.Barrier:
                    return "魔法障壁: ダメージを吸収";
                case PlayerAbilityType.Backstab:
                    return "バックスタブ: 背後からの強力な攻撃";
                case PlayerAbilityType.Sanctuary:
                    return "聖域: 範囲内の味方を回復";
                default:
                    return abilityType.ToString();
            }
        }
        
        /// <summary>
        /// 選択を確定
        /// </summary>
        private void ConfirmSelection()
        {
            if (playerCharacterManager != null)
            {
                playerCharacterManager.SelectCharacterType(selectedCharacterType);
            }
            
            OnSelectionConfirmed?.Invoke();
            Hide();
            
            Debug.Log($"Character selection confirmed: {selectedCharacterType}");
        }
        
        /// <summary>
        /// 選択をキャンセル
        /// </summary>
        private void CancelSelection()
        {
            OnSelectionCancelled?.Invoke();
            Hide();
            
            Debug.Log("Character selection cancelled");
        }
        
        /// <summary>
        /// UIを表示
        /// </summary>
        public void Show()
        {
            if (selectionPanel != null)
            {
                selectionPanel.SetActive(true);
            }
            
            // 最新の情報で更新
            UpdateCharacterInfo();
        }
        
        /// <summary>
        /// UIを非表示
        /// </summary>
        public void Hide()
        {
            if (selectionPanel != null)
            {
                selectionPanel.SetActive(false);
            }
        }
        
        /// <summary>
        /// 現在選択されているキャラクタータイプを取得
        /// </summary>
        public PlayerCharacterType GetSelectedCharacterType()
        {
            return selectedCharacterType;
        }
        
        /// <summary>
        /// 現在選択されているキャラクターデータを取得
        /// </summary>
        public PlayerCharacterData GetSelectedCharacterData()
        {
            return selectedCharacterData;
        }
        
        /// <summary>
        /// 表示状態を取得
        /// </summary>
        public bool IsVisible()
        {
            return selectionPanel != null && selectionPanel.activeInHierarchy;
        }
    }
    
    /// <summary>
    /// プレイヤーキャラクター選択ボタンのコンポーネント
    /// </summary>
    public class PlayerCharacterButton : MonoBehaviour
    {
        [Header("Button Components")]
        [SerializeField] private Button button;
        [SerializeField] private Image characterIcon;
        [SerializeField] private TextMeshProUGUI characterNameText;
        [SerializeField] private GameObject selectionIndicator;
        
        private PlayerCharacterData characterData;
        private System.Action onClickCallback;
        
        public PlayerCharacterType CharacterType => characterData?.type ?? PlayerCharacterType.Warrior;
        
        private void Awake()
        {
            if (button == null)
            {
                button = GetComponent<Button>();
            }
            
            if (button != null)
            {
                button.onClick.AddListener(OnButtonClick);
            }
        }
        
        /// <summary>
        /// ボタンを設定
        /// </summary>
        public void Setup(PlayerCharacterData data, System.Action onClick)
        {
            characterData = data;
            onClickCallback = onClick;
            
            UpdateDisplay();
        }
        
        private void UpdateDisplay()
        {
            if (characterData == null) return;
            
            // アイコンを設定
            if (characterIcon != null && characterData.icon != null)
            {
                characterIcon.sprite = characterData.icon;
            }
            
            // 名前を設定
            if (characterNameText != null)
            {
                characterNameText.text = characterData.displayName;
            }
        }
        
        private void OnButtonClick()
        {
            onClickCallback?.Invoke();
        }
        
        /// <summary>
        /// 選択状態を設定
        /// </summary>
        public void SetSelected(bool selected)
        {
            if (selectionIndicator != null)
            {
                selectionIndicator.SetActive(selected);
            }
            
            // ボタンの色を変更（オプション）
            if (button != null)
            {
                ColorBlock colors = button.colors;
                colors.normalColor = selected ? Color.yellow : Color.white;
                button.colors = colors;
            }
        }
    }
}