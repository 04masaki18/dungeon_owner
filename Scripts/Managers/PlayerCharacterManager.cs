using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using DungeonOwner.Data;
using DungeonOwner.PlayerCharacters;
using DungeonOwner.Interfaces;

namespace DungeonOwner.Managers
{
    /// <summary>
    /// プレイヤーキャラクターの管理を行うマネージャー
    /// 選択、配置、蘇生、レベル管理を担当
    /// </summary>
    public class PlayerCharacterManager : MonoBehaviour
    {
        [Header("Player Character Configuration")]
        [SerializeField] private List<PlayerCharacterData> availableCharacters = new List<PlayerCharacterData>();
        [SerializeField] private Transform playerCharacterParent;
        
        [Header("Current Player Character")]
        [SerializeField] private PlayerCharacterType selectedCharacterType = PlayerCharacterType.Warrior;
        [SerializeField] private BasePlayerCharacter currentPlayerCharacter;
        [SerializeField] private int playerCharacterLevel = 1;
        
        [Header("Revive Settings")]
        [SerializeField] private float baseReviveTime = 30f;
        [SerializeField] private bool isReviving = false;
        [SerializeField] private float reviveTimer = 0f;
        
        // イベント
        public System.Action<PlayerCharacterType> OnCharacterSelected;
        public System.Action<BasePlayerCharacter> OnCharacterSpawned;
        public System.Action<BasePlayerCharacter> OnCharacterDied;
        public System.Action<BasePlayerCharacter> OnCharacterRevived;
        public System.Action<int> OnLevelChanged;
        
        // プロパティ
        public PlayerCharacterType SelectedCharacterType => selectedCharacterType;
        public BasePlayerCharacter CurrentPlayerCharacter => currentPlayerCharacter;
        public int PlayerCharacterLevel => playerCharacterLevel;
        public bool IsReviving => isReviving;
        public float ReviveTimeRemaining => isReviving ? reviveTimer : 0f;
        
        private ShelterManager shelterManager;
        private FloorSystem floorSystem;
        
        private void Awake()
        {
            // 親オブジェクトが設定されていない場合は作成
            if (playerCharacterParent == null)
            {
                GameObject parent = new GameObject("PlayerCharacters");
                parent.transform.SetParent(transform);
                playerCharacterParent = parent.transform;
            }
            
            LoadAvailableCharacters();
        }
        
        private void Start()
        {
            shelterManager = FindObjectOfType<ShelterManager>();
            floorSystem = FindObjectOfType<FloorSystem>();
            
            if (shelterManager == null)
            {
                Debug.LogError("ShelterManager not found! PlayerCharacterManager requires ShelterManager.");
            }
            
            if (floorSystem == null)
            {
                Debug.LogError("FloorSystem not found! PlayerCharacterManager requires FloorSystem.");
            }
        }
        
        private void Update()
        {
            UpdateReviveTimer();
            CheckPlayerCharacterStatus();
        }
        
        /// <summary>
        /// 利用可能なプレイヤーキャラクターデータを読み込み
        /// </summary>
        private void LoadAvailableCharacters()
        {
            if (availableCharacters.Count == 0)
            {
                // Resourcesフォルダから自動読み込み
                PlayerCharacterData[] characters = Resources.LoadAll<PlayerCharacterData>("Data/PlayerCharacters");
                availableCharacters.AddRange(characters);
                
                if (availableCharacters.Count == 0)
                {
                    Debug.LogWarning("No PlayerCharacterData found in Resources/Data/PlayerCharacters");
                }
            }
        }
        
        /// <summary>
        /// プレイヤーキャラクタータイプを選択
        /// </summary>
        public void SelectCharacterType(PlayerCharacterType characterType)
        {
            if (selectedCharacterType == characterType) return;
            
            selectedCharacterType = characterType;
            OnCharacterSelected?.Invoke(characterType);
            
            Debug.Log($"Player character type selected: {characterType}");
        }
        
        /// <summary>
        /// 選択されたプレイヤーキャラクターを指定位置にスポーン
        /// </summary>
        public BasePlayerCharacter SpawnPlayerCharacter(Vector2 position)
        {
            // 既存のプレイヤーキャラクターがいる場合は削除
            if (currentPlayerCharacter != null)
            {
                DestroyPlayerCharacter();
            }
            
            PlayerCharacterData characterData = GetCharacterData(selectedCharacterType);
            if (characterData == null)
            {
                Debug.LogError($"PlayerCharacterData not found for type: {selectedCharacterType}");
                return null;
            }
            
            // プレハブからインスタンス生成
            GameObject characterObject = Instantiate(characterData.prefab, position, Quaternion.identity, playerCharacterParent);
            currentPlayerCharacter = characterObject.GetComponent<BasePlayerCharacter>();
            
            if (currentPlayerCharacter == null)
            {
                Debug.LogError($"BasePlayerCharacter component not found on prefab: {characterData.prefab.name}");
                Destroy(characterObject);
                return null;
            }
            
            // データとレベルを設定
            currentPlayerCharacter.SetCharacterData(characterData);
            currentPlayerCharacter.Level = playerCharacterLevel;
            
            // イベント通知
            OnCharacterSpawned?.Invoke(currentPlayerCharacter);
            
            Debug.Log($"Player character spawned: {selectedCharacterType} at level {playerCharacterLevel}");
            return currentPlayerCharacter;
        }
        
        /// <summary>
        /// プレイヤーキャラクターを削除
        /// </summary>
        public void DestroyPlayerCharacter()
        {
            if (currentPlayerCharacter != null)
            {
                Destroy(currentPlayerCharacter.gameObject);
                currentPlayerCharacter = null;
            }
        }
        
        /// <summary>
        /// プレイヤーキャラクターのレベルを設定
        /// </summary>
        public void SetPlayerCharacterLevel(int level)
        {
            int newLevel = Mathf.Max(1, level);
            if (playerCharacterLevel == newLevel) return;
            
            playerCharacterLevel = newLevel;
            
            // 現在のプレイヤーキャラクターにも適用
            if (currentPlayerCharacter != null)
            {
                currentPlayerCharacter.Level = playerCharacterLevel;
            }
            
            OnLevelChanged?.Invoke(playerCharacterLevel);
            Debug.Log($"Player character level set to: {playerCharacterLevel}");
        }
        
        /// <summary>
        /// プレイヤーキャラクターのレベルを上げる
        /// </summary>
        public void LevelUpPlayerCharacter()
        {
            SetPlayerCharacterLevel(playerCharacterLevel + 1);
        }
        
        /// <summary>
        /// プレイヤーキャラクターが死亡した時の処理
        /// </summary>
        public void OnPlayerCharacterDeath(BasePlayerCharacter character)
        {
            if (character != currentPlayerCharacter) return;
            
            Debug.Log($"Player character died: {character.Type}");
            OnCharacterDied?.Invoke(character);
            
            // 蘇生タイマー開始
            StartReviveProcess();
        }
        
        /// <summary>
        /// 蘇生プロセスを開始
        /// </summary>
        private void StartReviveProcess()
        {
            if (isReviving) return;
            
            isReviving = true;
            
            // レベルに応じた蘇生時間を計算
            PlayerCharacterData characterData = GetCharacterData(selectedCharacterType);
            if (characterData != null)
            {
                reviveTimer = characterData.GetReviveTime(playerCharacterLevel);
            }
            else
            {
                reviveTimer = baseReviveTime;
            }
            
            Debug.Log($"Revive process started. Time remaining: {reviveTimer:F1}s");
        }
        
        /// <summary>
        /// 蘇生タイマーの更新
        /// </summary>
        private void UpdateReviveTimer()
        {
            if (!isReviving) return;
            
            reviveTimer -= Time.deltaTime;
            
            if (reviveTimer <= 0f)
            {
                RevivePlayerCharacter();
            }
        }
        
        /// <summary>
        /// プレイヤーキャラクターを蘇生
        /// </summary>
        private void RevivePlayerCharacter()
        {
            if (!isReviving) return;
            
            isReviving = false;
            reviveTimer = 0f;
            
            // 退避スポットで蘇生
            if (currentPlayerCharacter != null && shelterManager != null)
            {
                currentPlayerCharacter.Revive();
                shelterManager.AddPlayerCharacterToShelter(currentPlayerCharacter);
                
                OnCharacterRevived?.Invoke(currentPlayerCharacter);
                Debug.Log($"Player character revived in shelter: {currentPlayerCharacter.Type}");
            }
        }
        
        /// <summary>
        /// プレイヤーキャラクターの状態をチェック
        /// </summary>
        private void CheckPlayerCharacterStatus()
        {
            if (currentPlayerCharacter == null) return;
            
            // 死亡チェック
            if (!currentPlayerCharacter.IsAlive() && !isReviving)
            {
                OnPlayerCharacterDeath(currentPlayerCharacter);
            }
        }
        
        /// <summary>
        /// プレイヤーキャラクターを退避スポットから配置
        /// </summary>
        public bool DeployPlayerCharacterFromShelter(Vector2 position, int floorIndex)
        {
            if (currentPlayerCharacter == null || !currentPlayerCharacter.IsInShelter())
            {
                Debug.LogWarning("No player character in shelter to deploy");
                return false;
            }
            
            if (floorSystem == null)
            {
                Debug.LogError("FloorSystem not available for deployment");
                return false;
            }
            
            // 配置可能かチェック
            if (!floorSystem.CanPlaceCharacter(floorIndex, position))
            {
                Debug.LogWarning($"Cannot deploy player character at position {position} on floor {floorIndex}");
                return false;
            }
            
            // 退避スポットから除去
            if (shelterManager != null)
            {
                shelterManager.RemovePlayerCharacterFromShelter(currentPlayerCharacter);
            }
            
            // 指定位置に配置
            currentPlayerCharacter.DeployFromShelter(position);
            
            Debug.Log($"Player character deployed from shelter to floor {floorIndex} at {position}");
            return true;
        }
        
        /// <summary>
        /// 指定タイプのキャラクターデータを取得
        /// </summary>
        public PlayerCharacterData GetCharacterData(PlayerCharacterType characterType)
        {
            return availableCharacters.FirstOrDefault(data => data.type == characterType);
        }
        
        /// <summary>
        /// 利用可能なキャラクタータイプのリストを取得
        /// </summary>
        public List<PlayerCharacterType> GetAvailableCharacterTypes()
        {
            return availableCharacters.Select(data => data.type).ToList();
        }
        
        /// <summary>
        /// プレイヤーキャラクターが存在するかチェック
        /// </summary>
        public bool HasPlayerCharacter()
        {
            return currentPlayerCharacter != null;
        }
        
        /// <summary>
        /// プレイヤーキャラクターが生きているかチェック
        /// </summary>
        public bool IsPlayerCharacterAlive()
        {
            return currentPlayerCharacter != null && currentPlayerCharacter.IsAlive();
        }
        
        /// <summary>
        /// セーブデータ用の情報を取得
        /// </summary>
        public PlayerCharacterSaveData GetSaveData()
        {
            return new PlayerCharacterSaveData
            {
                selectedType = selectedCharacterType,
                level = playerCharacterLevel,
                isAlive = IsPlayerCharacterAlive(),
                isInShelter = currentPlayerCharacter?.IsInShelter() ?? false,
                currentHealth = currentPlayerCharacter?.Health ?? 0f,
                currentMana = currentPlayerCharacter?.Mana ?? 0f,
                position = currentPlayerCharacter?.Position ?? Vector2.zero,
                isReviving = isReviving,
                reviveTimeRemaining = reviveTimer
            };
        }
        
        /// <summary>
        /// セーブデータから復元
        /// </summary>
        public void LoadFromSaveData(PlayerCharacterSaveData saveData)
        {
            selectedCharacterType = saveData.selectedType;
            playerCharacterLevel = saveData.level;
            isReviving = saveData.isReviving;
            reviveTimer = saveData.reviveTimeRemaining;
            
            if (saveData.isAlive)
            {
                BasePlayerCharacter character = SpawnPlayerCharacter(saveData.position);
                if (character != null)
                {
                    character.TakeDamage(character.MaxHealth - saveData.currentHealth);
                    // マナの設定は直接アクセスできないため、回復で調整
                    float manaDiff = saveData.currentMana - character.Mana;
                    if (manaDiff > 0)
                    {
                        // マナ回復処理（実装が必要）
                    }
                    
                    if (saveData.isInShelter && shelterManager != null)
                    {
                        character.MoveToShelter();
                        shelterManager.AddPlayerCharacterToShelter(character);
                    }
                }
            }
            
            Debug.Log($"Player character data loaded: {selectedCharacterType} Level {playerCharacterLevel}");
        }
    }
    
    /// <summary>
    /// プレイヤーキャラクターのセーブデータ
    /// </summary>
    [System.Serializable]
    public class PlayerCharacterSaveData
    {
        public PlayerCharacterType selectedType;
        public int level;
        public bool isAlive;
        public bool isInShelter;
        public float currentHealth;
        public float currentMana;
        public Vector2 position;
        public bool isReviving;
        public float reviveTimeRemaining;
    }
}