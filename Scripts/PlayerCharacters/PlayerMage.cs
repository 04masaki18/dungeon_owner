using UnityEngine;
using DungeonOwner.Data;

namespace DungeonOwner.PlayerCharacters
{
    public class PlayerMage : BasePlayerCharacter
    {
        [Header("Mage Specific")]
        [SerializeField] private float magicMissileRange = 6f;
        [SerializeField] private float barrierDuration = 10f;
        [SerializeField] private float barrierAbsorption = 50f;
        [SerializeField] private int maxMissiles = 3;
        
        private bool hasBarrier = false;
        private float barrierHealth = 0f;
        private float barrierTimer = 0f;

        protected override void UpdateCharacterBehavior()
        {
            if (currentState == MonsterState.Idle && IsAlive())
            {
                ProcessMageBehavior();
            }
            
            UpdateBarrier();
        }

        protected override void ExecuteAbility()
        {
            // 魔法使いのメインアビリティ：マジックミサイル
            if (currentMana >= 25f)
            {
                currentMana -= 25f;
                ExecuteMagicMissile();
            }
        }

        protected override bool CanUseAbility()
        {
            return currentMana >= 25f && !isDead;
        }

        protected override float GetAbilityCooldown()
        {
            return 6f; // 6秒クールダウン
        }

        private void ProcessMageBehavior()
        {
            // 魔法使いの基本行動：後方支援と魔法障壁
            if (currentHealth < MaxHealth * 0.4f && !hasBarrier)
            {
                TryCreateBarrier();
            }
        }

        private void ExecuteMagicMissile()
        {
            // マジックミサイル：複数の魔法弾を発射
            ShowMagicMissileEffect();
            
            // 実際の攻撃処理は戦闘システムで実装
            float damage = GetAttackPower() * 1.2f;
            Debug.Log($"Mage {gameObject.name} cast Magic Missile! {maxMissiles} missiles with {damage} damage each");
        }

        public void CreateBarrier()
        {
            if (currentMana >= 30f && !hasBarrier)
            {
                currentMana -= 30f;
                TryCreateBarrier();
            }
        }

        private void TryCreateBarrier()
        {
            if (hasBarrier) return;
            
            hasBarrier = true;
            barrierHealth = barrierAbsorption;
            barrierTimer = barrierDuration;
            
            ShowBarrierEffect();
            Debug.Log($"Mage {gameObject.name} created magic barrier! Absorption: {barrierAbsorption}");
        }

        private void UpdateBarrier()
        {
            if (!hasBarrier) return;
            
            barrierTimer -= Time.deltaTime;
            
            if (barrierTimer <= 0f || barrierHealth <= 0f)
            {
                RemoveBarrier();
            }
        }

        private void RemoveBarrier()
        {
            if (!hasBarrier) return;
            
            hasBarrier = false;
            barrierHealth = 0f;
            barrierTimer = 0f;
            
            ShowBarrierEndEffect();
            Debug.Log($"Mage {gameObject.name} barrier expired!");
        }

        public override void TakeDamage(float damage)
        {
            // 魔法障壁がある場合は先に障壁がダメージを吸収
            if (hasBarrier && barrierHealth > 0f)
            {
                float absorbedDamage = Mathf.Min(damage, barrierHealth);
                barrierHealth -= absorbedDamage;
                damage -= absorbedDamage;
                
                ShowBarrierAbsorbEffect();
                
                if (barrierHealth <= 0f)
                {
                    RemoveBarrier();
                }
            }
            
            // 残りダメージを本体に適用
            if (damage > 0f)
            {
                base.TakeDamage(damage);
            }
        }

        // 範囲回復スペル
        public void CastAreaHeal()
        {
            if (currentMana >= 40f)
            {
                currentMana -= 40f;
                ExecuteAreaHeal();
            }
        }

        private void ExecuteAreaHeal()
        {
            float healAmount = GetAttackPower() * 0.8f;
            float healRange = 4f;
            
            ShowAreaHealEffect();
            
            // パーティメンバーを回復
            if (currentParty != null)
            {
                foreach (var member in currentParty.Members)
                {
                    if (member != this && Vector2.Distance(Position, member.Position) <= healRange)
                    {
                        member.Heal(healAmount);
                    }
                }
            }
            
            Debug.Log($"Mage {gameObject.name} cast Area Heal! Healing: {healAmount} in range {healRange}");
        }

        // エフェクト表示メソッド
        private void ShowMagicMissileEffect()
        {
            // TODO: マジックミサイルエフェクト
            Debug.Log($"Mage {gameObject.name} launches magic missiles!");
        }

        private void ShowBarrierEffect()
        {
            // TODO: 魔法障壁エフェクト
            Debug.Log($"Mage {gameObject.name} creates a shimmering barrier!");
        }

        private void ShowBarrierEndEffect()
        {
            // TODO: 魔法障壁終了エフェクト
            Debug.Log($"Mage {gameObject.name} barrier fades away!");
        }

        private void ShowBarrierAbsorbEffect()
        {
            // TODO: 魔法障壁ダメージ吸収エフェクト
            Debug.Log($"Mage {gameObject.name} barrier absorbs damage!");
        }

        private void ShowAreaHealEffect()
        {
            // TODO: 範囲回復エフェクト
            Debug.Log($"Mage {gameObject.name} casts healing light!");
        }

        // ゲッター
        public bool HasBarrier()
        {
            return hasBarrier;
        }

        public float GetBarrierHealth()
        {
            return barrierHealth;
        }

        public float GetBarrierTimeRemaining()
        {
            return barrierTimer;
        }

        public float GetMagicMissileRange()
        {
            return magicMissileRange;
        }

        protected override void OnDrawGizmosSelected()
        {
            base.OnDrawGizmosSelected();
            
            // マジックミサイル射程を表示
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, magicMissileRange);
            
            // 魔法障壁を表示
            if (hasBarrier)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(transform.position, 1.5f);
            }
        }
    }
}