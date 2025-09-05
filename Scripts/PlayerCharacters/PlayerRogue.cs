using UnityEngine;
using DungeonOwner.Data;

namespace DungeonOwner.PlayerCharacters
{
    public class PlayerRogue : BasePlayerCharacter
    {
        [Header("Rogue Specific")]
        [SerializeField] private float backstabDamageMultiplier = 3f;
        [SerializeField] private float stealthDuration = 8f;
        [SerializeField] private float quickStepDistance = 3f;
        [SerializeField] private float criticalChance = 0.25f;
        
        private bool isStealthed = false;
        private float stealthTimer = 0f;
        private bool canBackstab = false;

        protected override void UpdateCharacterBehavior()
        {
            if (currentState == MonsterState.Idle && IsAlive())
            {
                ProcessRogueBehavior();
            }
            
            UpdateStealth();
        }

        protected override void ExecuteAbility()
        {
            // 盗賊のメインアビリティ：クイックステップ
            if (currentMana >= 20f)
            {
                currentMana -= 20f;
                ExecuteQuickStep();
            }
        }

        protected override bool CanUseAbility()
        {
            return currentMana >= 20f && !isDead;
        }

        protected override float GetAbilityCooldown()
        {
            return 5f; // 5秒クールダウン
        }

        private void ProcessRogueBehavior()
        {
            // 盗賊の基本行動：機動力を活かした戦術
            if (currentHealth < MaxHealth * 0.5f && !isStealthed)
            {
                TryActivateStealth();
            }
        }

        private void ExecuteQuickStep()
        {
            // クイックステップ：素早い移動
            Vector2 currentPos = Position;
            Vector2 targetPos = GetQuickStepTarget();
            
            Position = targetPos;
            ShowQuickStepEffect();
            
            // クイックステップ後は短時間バックスタブ可能
            canBackstab = true;
            Invoke(nameof(ResetBackstab), 3f);
            
            Debug.Log($"Rogue {gameObject.name} used Quick Step! Moved from {currentPos} to {targetPos}");
        }

        private Vector2 GetQuickStepTarget()
        {
            // 現在位置から指定距離内のランダムな位置
            Vector2 randomDirection = Random.insideUnitCircle.normalized;
            Vector2 targetPosition = Position + randomDirection * quickStepDistance;
            
            // TODO: 壁や障害物との衝突判定
            return targetPosition;
        }

        public void ActivateStealth()
        {
            if (currentMana >= 35f && !isStealthed)
            {
                currentMana -= 35f;
                TryActivateStealth();
            }
        }

        private void TryActivateStealth()
        {
            if (isStealthed) return;
            
            isStealthed = true;
            stealthTimer = stealthDuration;
            
            ShowStealthEffect();
            Debug.Log($"Rogue {gameObject.name} activated stealth for {stealthDuration} seconds!");
        }

        private void UpdateStealth()
        {
            if (!isStealthed) return;
            
            stealthTimer -= Time.deltaTime;
            
            if (stealthTimer <= 0f)
            {
                DeactivateStealth();
            }
        }

        private void DeactivateStealth()
        {
            if (!isStealthed) return;
            
            isStealthed = false;
            stealthTimer = 0f;
            
            ShowStealthEndEffect();
            Debug.Log($"Rogue {gameObject.name} stealth ended!");
        }

        public void ExecuteBackstab()
        {
            if (!canBackstab) return;
            
            float damage = GetAttackPower() * backstabDamageMultiplier;
            ShowBackstabEffect();
            
            canBackstab = false;
            
            // 実際の攻撃処理は戦闘システムで実装
            Debug.Log($"Rogue {gameObject.name} executed Backstab! Damage: {damage}");
        }

        private void ResetBackstab()
        {
            canBackstab = false;
        }

        public override void TakeDamage(float damage)
        {
            // ステルス中は攻撃を受けにくい（50%回避）
            if (isStealthed && Random.value < 0.5f)
            {
                ShowDodgeEffect();
                Debug.Log($"Rogue {gameObject.name} dodged the attack while stealthed!");
                return;
            }
            
            // ステルス中に攻撃を受けるとステルス解除
            if (isStealthed)
            {
                DeactivateStealth();
            }
            
            // クリティカル回避チェック
            if (Random.value < criticalChance * 0.5f) // 半分の確率でクリティカル回避
            {
                damage *= 0.5f;
                ShowCriticalAvoidEffect();
            }
            
            base.TakeDamage(damage);
        }

        // 毒ダガー攻撃
        public void PoisonDagger()
        {
            if (currentMana >= 25f)
            {
                currentMana -= 25f;
                ExecutePoisonDagger();
            }
        }

        private void ExecutePoisonDagger()
        {
            float damage = GetAttackPower() * 1.5f;
            ShowPoisonDaggerEffect();
            
            // 実際の毒効果は戦闘システムで実装
            Debug.Log($"Rogue {gameObject.name} used Poison Dagger! Damage: {damage} + poison effect");
        }

        // エフェクト表示メソッド
        private void ShowQuickStepEffect()
        {
            // TODO: クイックステップエフェクト
            Debug.Log($"Rogue {gameObject.name} dashes quickly!");
        }

        private void ShowStealthEffect()
        {
            // TODO: ステルスエフェクト
            Debug.Log($"Rogue {gameObject.name} fades into shadows!");
        }

        private void ShowStealthEndEffect()
        {
            // TODO: ステルス終了エフェクト
            Debug.Log($"Rogue {gameObject.name} emerges from shadows!");
        }

        private void ShowBackstabEffect()
        {
            // TODO: バックスタブエフェクト
            Debug.Log($"Rogue {gameObject.name} strikes from behind!");
        }

        private void ShowDodgeEffect()
        {
            // TODO: 回避エフェクト
            Debug.Log($"Rogue {gameObject.name} nimbly dodges!");
        }

        private void ShowCriticalAvoidEffect()
        {
            // TODO: クリティカル回避エフェクト
            Debug.Log($"Rogue {gameObject.name} reduces critical damage!");
        }

        private void ShowPoisonDaggerEffect()
        {
            // TODO: 毒ダガーエフェクト
            Debug.Log($"Rogue {gameObject.name} coats blade with poison!");
        }

        // ゲッター
        public bool IsStealthed()
        {
            return isStealthed;
        }

        public float GetStealthTimeRemaining()
        {
            return stealthTimer;
        }

        public bool CanBackstab()
        {
            return canBackstab;
        }

        public float GetCriticalChance()
        {
            return criticalChance;
        }

        protected override void OnDrawGizmosSelected()
        {
            base.OnDrawGizmosSelected();
            
            // クイックステップ範囲を表示
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, quickStepDistance);
            
            // ステルス状態を表示
            if (isStealthed)
            {
                Gizmos.color = new Color(0, 0, 0, 0.3f);
                Gizmos.DrawSphere(transform.position, 1f);
            }
        }
    }
}