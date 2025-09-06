using UnityEngine;
using DungeonOwner.Data;
using DungeonOwner.Managers;
using DungeonOwner.PlayerCharacters;

namespace DungeonOwner.Core
{
    /// <summary>
    /// プレイヤーキャラクターシステムのテスト用クラス
    /// 要件8.1-8.4の動作確認を行う
    /// </summary>
    public class PlayerCharacterSystemTester : MonoBehaviour
    {
        [Header("Test Configuration")]
        [SerializeField] private bool runTestsOnStart = true;
        [SerializeField] private bool enableDebugLogs = true;
        [SerializeField] private KeyCode testKey = KeyCode.P;
        [SerializeField] private KeyCode spawnKey = KeyCode.O;
        [SerializeField] private KeyCode killKey = KeyCode.K;
        [SerializeField] private KeyCode levelUpKey = KeyCode.L;
        
        [Header("Test Settings")]
        [SerializeField] private Vector2 spawnPosition = new Vector2(0, 0);
        [SerializeField] private int testFloorIndex = 1;
        
        private PlayerCharacterManager playerCharacterManager;
        private ShelterManager shelterManager;
        private FloorSystem floorSystem;
        
        private void Start()
        {
            InitializeComponents();
            
            if (runTestsOnStart)
            {
                Invoke(nameof(RunAllTests), 1f); // 1秒後にテスト実行
            }
        }
        
        private void Update()
        {
            HandleInput();
        }
        
        private void InitializeComponents()
        {
            playerCharacterManager = FindObjectOfType<PlayerCharacterManager>();
            shelterManager = FindObjectOfType<ShelterManager>();
            floorSystem = FindObjectOfType<FloorSystem>();
            
            if (playerCharacterManager == null)
            {
                Debug.LogError("PlayerCharacterManager not found!");
            }
            
            if (shelterManager == null)
            {
                Debug.LogError("ShelterManager not found!");
            }
            
            if (floorSystem == null)
            {
                Debug.LogError("FloorSystem not found!");
            }
        }
        
        private void HandleInput()
        {
            if (Input.GetKeyDown(testKey))
            {
                RunAllTests();
            }
            
            if (Input.GetKeyDown(spawnKey))
            {
                TestSpawnPlayerCharacter();
            }
            
            if (Input.GetKeyDown(killKey))
            {
                TestKillPlayerCharacter();
            }
            
            if (Input.GetKeyDown(levelUpKey))
            {
                TestLevelUp();
            }
        }
        
        /// <summary>
        /// 全テストを実行
        /// </summary>
        public void RunAllTests()
        {
            DebugLog("=== プレイヤーキャラクターシステムテスト開始 ===");
            
            TestCharacterSelection();
            TestCharacterSpawning();
            TestLevelSystem();
            TestReviveSystem();
            TestShelterIntegration();
            
            DebugLog("=== プレイヤーキャラクターシステムテスト完了 ===");
        }
        
        /// <summary>
        /// 要件8.1: キャラクター選択システムのテスト
        /// </summary>
        private void TestCharacterSelection()
        {
            DebugLog("--- キャラクター選択システムテスト ---");
            
            if (playerCharacterManager == null) return;
            
            // 利用可能なキャラクタータイプを取得
            var availableTypes = playerCharacterManager.GetAvailableCharacterTypes();
            DebugLog($"利用可能なキャラクタータイプ数: {availableTypes.Count}");
            
            foreach (var type in availableTypes)
            {
                DebugLog($"- {type}");
                
                // キャラクターデータを取得
                var characterData = playerCharacterManager.GetCharacterData(type);
                if (characterData != null)
                {
                    DebugLog($"  HP: {characterData.GetHealthAtLevel(1)}, MP: {characterData.GetManaAtLevel(1)}, 攻撃力: {characterData.GetAttackPowerAtLevel(1)}");
                }
            }
            
            // 各キャラクタータイプを選択してテスト
            foreach (var type in availableTypes)
            {
                playerCharacterManager.SelectCharacterType(type);
                DebugLog($"キャラクタータイプ選択: {type} -> 現在の選択: {playerCharacterManager.SelectedCharacterType}");
                
                if (playerCharacterManager.SelectedCharacterType == type)
                {
                    DebugLog($"✓ {type} の選択成功");
                }
                else
                {
                    DebugLog($"✗ {type} の選択失敗");
                }
            }
        }
        
        /// <summary>
        /// 要件8.2: キャラクタースポーンシステムのテスト
        /// </summary>
        private void TestCharacterSpawning()
        {
            DebugLog("--- キャラクタースポーンシステムテスト ---");
            
            if (playerCharacterManager == null) return;
            
            // 戦士を選択してスポーン
            playerCharacterManager.SelectCharacterType(PlayerCharacterType.Warrior);
            BasePlayerCharacter spawnedCharacter = playerCharacterManager.SpawnPlayerCharacter(spawnPosition);
            
            if (spawnedCharacter != null)
            {
                DebugLog($"✓ プレイヤーキャラクタースポーン成功: {spawnedCharacter.Type}");
                DebugLog($"  位置: {spawnedCharacter.Position}");
                DebugLog($"  レベル: {spawnedCharacter.Level}");
                DebugLog($"  HP: {spawnedCharacter.Health}/{spawnedCharacter.MaxHealth}");
                DebugLog($"  MP: {spawnedCharacter.Mana}/{spawnedCharacter.MaxMana}");
            }
            else
            {
                DebugLog("✗ プレイヤーキャラクタースポーン失敗");
            }
        }
        
        /// <summary>
        /// 要件8.4: レベル引き継ぎシステムのテスト
        /// </summary>
        private void TestLevelSystem()
        {
            DebugLog("--- レベルシステムテスト ---");
            
            if (playerCharacterManager == null) return;
            
            int initialLevel = playerCharacterManager.PlayerCharacterLevel;
            DebugLog($"初期レベル: {initialLevel}");
            
            // レベルアップテスト
            for (int i = 1; i <= 3; i++)
            {
                playerCharacterManager.LevelUpPlayerCharacter();
                int newLevel = playerCharacterManager.PlayerCharacterLevel;
                DebugLog($"レベルアップ {i}: {newLevel}");
                
                // 現在のプレイヤーキャラクターがいる場合、ステータス確認
                var currentCharacter = playerCharacterManager.CurrentPlayerCharacter;
                if (currentCharacter != null)
                {
                    DebugLog($"  キャラクターレベル: {currentCharacter.Level}");
                    DebugLog($"  HP: {currentCharacter.MaxHealth}");
                    DebugLog($"  MP: {currentCharacter.MaxMana}");
                }
            }
            
            // レベル設定テスト
            playerCharacterManager.SetPlayerCharacterLevel(10);
            DebugLog($"レベル10設定後: {playerCharacterManager.PlayerCharacterLevel}");
        }
        
        /// <summary>
        /// 要件8.3: 蘇生システムのテスト
        /// </summary>
        private void TestReviveSystem()
        {
            DebugLog("--- 蘇生システムテスト ---");
            
            if (playerCharacterManager == null) return;
            
            var currentCharacter = playerCharacterManager.CurrentPlayerCharacter;
            if (currentCharacter == null)
            {
                // キャラクターがいない場合はスポーン
                currentCharacter = playerCharacterManager.SpawnPlayerCharacter(spawnPosition);
            }
            
            if (currentCharacter != null)
            {
                DebugLog($"テスト対象キャラクター: {currentCharacter.Type}");
                DebugLog($"初期状態 - 生存: {currentCharacter.IsAlive()}, 退避中: {currentCharacter.IsInShelter()}");
                
                // キャラクターを撃破
                float maxHealth = currentCharacter.MaxHealth;
                currentCharacter.TakeDamage(maxHealth + 100f); // 確実に撃破
                
                DebugLog($"撃破後 - 生存: {currentCharacter.IsAlive()}, HP: {currentCharacter.Health}");
                DebugLog($"蘇生中: {playerCharacterManager.IsReviving}");
                
                if (playerCharacterManager.IsReviving)
                {
                    float reviveTime = playerCharacterManager.ReviveTimeRemaining;
                    DebugLog($"蘇生時間: {reviveTime:F1}秒");
                    DebugLog("✓ 蘇生プロセス開始成功");
                }
                else
                {
                    DebugLog("✗ 蘇生プロセス開始失敗");
                }
            }
        }
        
        /// <summary>
        /// 退避スポット統合テスト
        /// </summary>
        private void TestShelterIntegration()
        {
            DebugLog("--- 退避スポット統合テスト ---");
            
            if (playerCharacterManager == null || shelterManager == null) return;
            
            var currentCharacter = playerCharacterManager.CurrentPlayerCharacter;
            if (currentCharacter == null)
            {
                currentCharacter = playerCharacterManager.SpawnPlayerCharacter(spawnPosition);
            }
            
            if (currentCharacter != null)
            {
                DebugLog($"テスト対象キャラクター: {currentCharacter.Type}");
                
                // 退避スポットに移動
                bool shelterResult = shelterManager.AddPlayerCharacterToShelter(currentCharacter);
                DebugLog($"退避スポット移動: {shelterResult}");
                
                if (shelterResult)
                {
                    DebugLog($"退避後 - 退避中: {currentCharacter.IsInShelter()}");
                    DebugLog($"退避スポット内プレイヤーキャラクター数: {shelterManager.GetPlayerCharacterCount()}");
                    
                    // 退避スポットから配置
                    Vector2 deployPosition = spawnPosition + Vector2.right * 2f;
                    bool deployResult = shelterManager.DeployPlayerCharacter(currentCharacter, testFloorIndex, deployPosition);
                    DebugLog($"退避スポットから配置: {deployResult}");
                    
                    if (deployResult)
                    {
                        DebugLog($"配置後 - 退避中: {currentCharacter.IsInShelter()}, 位置: {currentCharacter.Position}");
                        DebugLog("✓ 退避スポット統合テスト成功");
                    }
                    else
                    {
                        DebugLog("✗ 退避スポットから配置失敗");
                    }
                }
                else
                {
                    DebugLog("✗ 退避スポット移動失敗");
                }
            }
        }
        
        /// <summary>
        /// 手動テスト: プレイヤーキャラクタースポーン
        /// </summary>
        public void TestSpawnPlayerCharacter()
        {
            if (playerCharacterManager == null) return;
            
            // 現在選択されているタイプでスポーン
            BasePlayerCharacter character = playerCharacterManager.SpawnPlayerCharacter(spawnPosition);
            if (character != null)
            {
                DebugLog($"手動スポーン成功: {character.Type} at {character.Position}");
            }
            else
            {
                DebugLog("手動スポーン失敗");
            }
        }
        
        /// <summary>
        /// 手動テスト: プレイヤーキャラクター撃破
        /// </summary>
        public void TestKillPlayerCharacter()
        {
            if (playerCharacterManager == null) return;
            
            var character = playerCharacterManager.CurrentPlayerCharacter;
            if (character != null && character.IsAlive())
            {
                character.TakeDamage(character.MaxHealth + 100f);
                DebugLog($"プレイヤーキャラクター撃破: {character.Type}");
            }
            else
            {
                DebugLog("撃破対象のプレイヤーキャラクターが存在しません");
            }
        }
        
        /// <summary>
        /// 手動テスト: レベルアップ
        /// </summary>
        public void TestLevelUp()
        {
            if (playerCharacterManager == null) return;
            
            int oldLevel = playerCharacterManager.PlayerCharacterLevel;
            playerCharacterManager.LevelUpPlayerCharacter();
            int newLevel = playerCharacterManager.PlayerCharacterLevel;
            
            DebugLog($"レベルアップ: {oldLevel} -> {newLevel}");
        }
        
        private void DebugLog(string message)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"[PlayerCharacterSystemTester] {message}");
            }
        }
        
        /// <summary>
        /// テスト結果の詳細情報を表示
        /// </summary>
        public void ShowDetailedStatus()
        {
            DebugLog("=== プレイヤーキャラクターシステム詳細状態 ===");
            
            if (playerCharacterManager != null)
            {
                DebugLog($"選択キャラクタータイプ: {playerCharacterManager.SelectedCharacterType}");
                DebugLog($"プレイヤーキャラクターレベル: {playerCharacterManager.PlayerCharacterLevel}");
                DebugLog($"蘇生中: {playerCharacterManager.IsReviving}");
                DebugLog($"蘇生残り時間: {playerCharacterManager.ReviveTimeRemaining:F1}秒");
                
                var character = playerCharacterManager.CurrentPlayerCharacter;
                if (character != null)
                {
                    DebugLog($"現在のキャラクター: {character.Type}");
                    DebugLog($"  レベル: {character.Level}");
                    DebugLog($"  HP: {character.Health:F1}/{character.MaxHealth:F1}");
                    DebugLog($"  MP: {character.Mana:F1}/{character.MaxMana:F1}");
                    DebugLog($"  位置: {character.Position}");
                    DebugLog($"  生存: {character.IsAlive()}");
                    DebugLog($"  退避中: {character.IsInShelter()}");
                    DebugLog($"  状態: {character.State}");
                }
                else
                {
                    DebugLog("現在のキャラクター: なし");
                }
            }
            
            if (shelterManager != null)
            {
                DebugLog($"退避スポット内プレイヤーキャラクター数: {shelterManager.GetPlayerCharacterCount()}");
            }
        }
    }
}