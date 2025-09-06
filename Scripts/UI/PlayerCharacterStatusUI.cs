using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DungeonOwner.Managers;
using DungeonOwner.PlayerCharacters;

namespace DungeonOwner.UI
{
    /// <summary>
    /// プレイヤーキャラクターの状態を表示するUIクラス
    /// HP、MP、レベル、蘇生タイマーなどを表示
    /// </summary>
    public class PlayerCharacterStatusUI : MonoBehaviour
    {
        [Header("Status Display")]
        [SerializeField] private GameObject statusPanel;
        [SerializeField] private Image characterIcon;
        [SerializeField] private TextMeshProUGUI characterNameText;
        [SerializeField] private TextMeshProUGUI levelText;
        
        [Header("Health and Mana")]
        [SerializeField] private Slider healthSlider;
        [SerializeField] private TextMeshProUGUI healthText;
        [SerializeField] private Slider manaSlider;
        [SerializeField] private TextMeshProUGUI manaText;
        
        [Header("Status Indicators")]
        [SerializeField] private GameObject aliveIndicator;
        [SerializeField] private GameObject deadIndicator;
        [SerializeField] private GameObject shelterIndicator;
        [SerializeField] private GameObject revivingIndicator;
        
        [Header("Revive Timer")]
        [SerializeField] private GameObject reviveTimerPanel;
        [SerializeField] private Slider reviveTimerSlider;
        [SerializeField] private TextMeshProUGUI reviveTimerText;
        
        [Header("Abilities")]
        [SerializeField] private Transform abilityContainer;
        [SerializeField] private GameObject abilityIconPrefab;
        
        private PlayerCharacterManager playerCharacterManager;
        private BasePlayerCharacter currentPlayerCharacter;
        private bool isInitialized = false;
        
        private void Awake()
        {
            playerCharacterManager = FindObjectOfType<PlayerCharacterManager>();
            
            if (playerCharacterManager == null)
            {
                Debug.LogError("PlayerCharacterManager not found! PlayerCharacterStatusUI requires PlayerCharacterManager.");
            }
        }
        
        private void Start()
        {
            Initialize();
        }
        
        private void Update()
        {
            if (isInitialized)
            {
                UpdateStatusDisplay();
            }
        }
        
        private void Initialize()
        {
            if (playerCharacterManager == null) return;
            
            // イベント登録
            playerCharacterManager.OnCharacterSpawned += OnPlayerCharacterSpawned;
            playerCharacterManager.OnCharacterDied += OnPlayerCharacterDied;
            playerCharacterManager.OnCharacterRevived += OnPlayerCharacterRevived;
            playerCharacterManager.OnLevelChanged += OnLevelChanged;
            
            // 現在のプレイヤーキャラクターを取得
            currentPlayerCharacter = playerCharacterManager.CurrentPlayerCharacter;
            
            UpdateCharacterInfo();
            isInitialized = true;
        }
        
        private void OnDestroy()
        {
            if (playerCharacterManager != null)
            {
                playerCharacterManager.OnCharacterSpawned -= OnPlayerCharacterSpawned;
                playerCharacterManager.OnCharacterDied -= OnPlayerCharacterDied;
                playerCharacterManager.OnCharacterRevived -= OnPlayerCharacterRevived;
                playerCharacterManager.OnLevelChanged -= OnLevelChanged;
            }
        }
        
        private void OnPlayerCharacterSpawned(BasePlayerCharacter character)
        {
            currentPlayerCharacter = character;
            UpdateCharacterInfo();
        }
        
        private void OnPlayerCharacterDied(BasePlayerCharacter character)
        {
            if (character == currentPlayerCharacter)
            {
                UpdateStatusIndicators();
            }
        }
        
        private void OnPlayerCharacterRevived(BasePlayerCharacter character)
        {
            if (character == currentPlayerCharacter)
            {
                UpdateStatusIndicators();
            }
        }
        
        private void OnLevelChanged(int newLevel)
        {
            UpdateLevelDisplay();
        }
        
        /// <summary>
        /// キャラクター情報を更新
        /// </summary>
        private void UpdateCharacterInfo()
        {
            if (playerCharacterManager == null) return;
            
            var characterData = playerCharacterManager.GetCharacterData(playerCharacterManager.SelectedCharacterType);
            if (characterData == null) return;
            
            // キャラクターアイコンと名前
            if (characterIcon != null && characterData.icon != null)
            {
                characterIcon.sprite = characterData.icon;
            }
            
            if (characterNameText != null)
            {
                characterNameText.text = characterData.displayName;
            }
            
            UpdateLevelDisplay();
            UpdateAbilityIcons(characterData);
        }
        
        /// <summary>
        /// レベル表示を更新
        /// </summary>
        private void UpdateLevelDisplay()
        {
            if (levelText != null && playerCharacterManager != null)
            {
                levelText.text = $"Lv.{playerCharacterManager.PlayerCharacterLevel}";
            }
        }
        
        /// <summary>
        /// アビリティアイコンを更新
        /// </summary>
        private void UpdateAbilityIcons(DungeonOwner.Data.PlayerCharacterData characterData)
        {
            if (abilityContainer == null || abilityIconPrefab == null) return;
            
            // 既存のアイコンをクリア
            foreach (Transform child in abilityContainer)
            {
                Destroy(child.gameObject);
            }
            
            // アビリティアイコンを作成
            foreach (var ability in characterData.abilities)
            {
                GameObject iconObject = Instantiate(abilityIconPrefab, abilityContainer);
                // TODO: アビリティアイコンの設定
            }
        }
        
        /// <summary>
        /// ステータス表示を更新
        /// </summary>
        private void UpdateStatusDisplay()
        {
            UpdateHealthAndMana();
            UpdateStatusIndicators();
            UpdateReviveTimer();
        }
        
        /// <summary>
        /// HP/MP表示を更新
        /// </summary>
        private void UpdateHealthAndMana()
        {
            if (currentPlayerCharacter == null) return;
            
            // HP表示
            if (healthSlider != null)
            {
                healthSlider.maxValue = currentPlayerCharacter.MaxHealth;
                healthSlider.value = currentPlayerCharacter.Health;
            }
            
            if (healthText != null)
            {
                healthText.text = $"{currentPlayerCharacter.Health:F0}/{currentPlayerCharacter.MaxHealth:F0}";
            }
            
            // MP表示
            if (manaSlider != null)
            {
                manaSlider.maxValue = currentPlayerCharacter.MaxMana;
                manaSlider.value = currentPlayerCharacter.Mana;
            }
            
            if (manaText != null)
            {
                manaText.text = $"{currentPlayerCharacter.Mana:F0}/{currentPlayerCharacter.MaxMana:F0}";
            }
        }
        
        /// <summary>
        /// 状態インジケーターを更新
        /// </summary>
        private void UpdateStatusIndicators()
        {
            bool isAlive = currentPlayerCharacter != null && currentPlayerCharacter.IsAlive();
            bool isInShelter = currentPlayerCharacter != null && currentPlayerCharacter.IsInShelter();
            bool isReviving = playerCharacterManager != null && playerCharacterManager.IsReviving;
            
            if (aliveIndicator != null)
            {
                aliveIndicator.SetActive(isAlive && !isInShelter);
            }
            
            if (deadIndicator != null)
            {
                deadIndicator.SetActive(!isAlive && !isReviving);
            }
            
            if (shelterIndicator != null)
            {
                shelterIndicator.SetActive(isInShelter);
            }
            
            if (revivingIndicator != null)
            {
                revivingIndicator.SetActive(isReviving);
            }
        }
        
        /// <summary>
        /// 蘇生タイマーを更新
        /// </summary>
        private void UpdateReviveTimer()
        {
            if (playerCharacterManager == null) return;
            
            bool isReviving = playerCharacterManager.IsReviving;
            float timeRemaining = playerCharacterManager.ReviveTimeRemaining;
            
            if (reviveTimerPanel != null)
            {
                reviveTimerPanel.SetActive(isReviving);
            }
            
            if (isReviving)
            {
                if (reviveTimerSlider != null)
                {
                    // 蘇生時間の総時間を取得（概算）
                    float totalReviveTime = 30f; // デフォルト値
                    var characterData = playerCharacterManager.GetCharacterData(playerCharacterManager.SelectedCharacterType);
                    if (characterData != null)
                    {
                        totalReviveTime = characterData.GetReviveTime(playerCharacterManager.PlayerCharacterLevel);
                    }
                    
                    reviveTimerSlider.maxValue = totalReviveTime;
                    reviveTimerSlider.value = totalReviveTime - timeRemaining;
                }
                
                if (reviveTimerText != null)
                {
                    reviveTimerText.text = $"蘇生まで: {timeRemaining:F1}秒";
                }
            }
        }
        
        /// <summary>
        /// UIの表示/非表示を切り替え
        /// </summary>
        public void SetVisible(bool visible)
        {
            if (statusPanel != null)
            {
                statusPanel.SetActive(visible);
            }
        }
        
        /// <summary>
        /// プレイヤーキャラクターが存在するかチェック
        /// </summary>
        public bool HasPlayerCharacter()
        {
            return currentPlayerCharacter != null;
        }
        
        /// <summary>
        /// 現在のプレイヤーキャラクターを取得
        /// </summary>
        public BasePlayerCharacter GetCurrentPlayerCharacter()
        {
            return currentPlayerCharacter;
        }
    }
}