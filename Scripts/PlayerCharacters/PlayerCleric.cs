using UnityEngine;
using DungeonOwner.Data;

namespace DungeonOwner.PlayerCharacters
{
    public class PlayerCleric : BasePlayerCharacter
    {
        [Header("Cleric Specific")]
        [SerializeField] private float blessingDuration = 15f;
        [SerializeField] private float blessingBonus = 0.3f;
        [SerializeField] private float sanctuaryRange = 5f;
        [SerializeField] private float sanctuaryDuration = 12f;
        [SerializeField] private float healPower = 1.5f;
        
        private bool hasBlessingActive = false;
        private float blessingTimer = 0f;
        private bool hasSanctuaryActive = false;
        private float sanctuaryTimer = 0f;
        private Vector2 sanctuaryPosition;

        protected override void UpdateCharacterBehavior()
        {
            if (currentState == MonsterState.Idle && IsAlive())
            {
                ProcessClericBehavior();
            }
            
            UpdateBlessings();
            UpdateSanctuary();
        }

        protected override void ExecuteAbility()
        {
            // 僧侶のメインアビリティ：祝福
            if (currentMana >= 30f)
            {
                currentMana -= 30f;
                ExecuteBlessing();
            }
        }

        protected override bool CanUseAbility()
        {
            return currentMana >= 30f && !isDead;
        }

        protected override float GetAbilityCooldown()
        {
            return 10f; // 10秒クールダウン
        }

        private void ProcessClericBehavior()
        {
            // 僧侶の基本行動：支援と回復
            if (currentParty != null)
            {
                CheckPartyMembersHealth();
            }
            
            // 自分の体力が低い場合は聖域を作成
            if (currentHealth < MaxHealth * 0.4f && !hasSanctuaryActive)
            {
                TryCreateSanctuary();
            }
        }

        private void CheckPartyMembersHealth()
        {
            // パーティメンバーの体力をチェックして回復
            foreach (var member in currentParty.Members)
            {
                if (member != this && member.Health < member.MaxHealth * 0.6f)
                {
                    if (Vector2.Distance(Position, member.Position) <= 4f)
                    {
                        TryHealMember(member);
                        break; // 一度に一人だけ回復
                    }
                }
            }
        }

        private void TryHealMember(ICharacter member)
        {
            if (currentMana >= 15f)
            {
                currentMana -= 15f;
                float healAmount = GetAttackPower() * healPower;
                member.Heal(healAmount);
                ShowHealEffect();
                
                Debug.Log($"Cleric {gameObject.name} healed {member} for {healAmount} HP");
            }
        }

        private void ExecuteBlessing()
        {
            // 祝福：パーティメンバーの能力値上昇
            if (hasBlessingActive) return;
            
            hasBlessingActive = true;
            blessingTimer = blessingDuration;
            
            ShowBlessingEffect();
            
            // パーティメンバーに祝福効果を適用
            if (currentParty != null)
            {
                foreach (var member in currentParty.Members)
                {
                    ApplyBlessingToMember(member);
                }
            }
            
            Debug.Log($"Cleric {gameObject.name} cast Blessing! Duration: {blessingDuration}s");
        }

        private void ApplyBlessingToMember(ICharacter member)
        {
            // 祝福効果の適用（実際の効果は戦闘システムで実装）
            Debug.Log($"Blessing applied to {member}");
        }

        public void CreateSanctuary()
        {
            if (currentMana >= 50f && !hasSanctuaryActive)
            {
                currentMana -= 50f;
                TryCreateSanctuary();
            }
        }

        private void TryCreateSanctuary()
        {
            if (hasSanctuaryActive) return;
            
            hasSanctuaryActive = true;
            sanctuaryTimer = sanctuaryDuration;
            sanctuaryPosition = Position;
            
            ShowSanctuaryEffect();
            Debug.Log($"Cleric {gameObject.name} created Sanctuary at {sanctuaryPosition}! Duration: {sanctuaryDuration}s");
        }

        private void UpdateBlessings()
        {
            if (!hasBlessingActive) return;
            
            blessingTimer -= Time.deltaTime;
            
            if (blessingTimer <= 0f)
            {
                RemoveBlessing();
            }
        }

        private void RemoveBlessing()
        {
            if (!hasBlessingActive) return;
            
            hasBlessingActive = false;
            blessingTimer = 0f;
            
            ShowBlessingEndEffect();
            
            // パーティメンバーから祝福効果を除去
            if (currentParty != null)
            {
                foreach (var member in currentParty.Members)
                {
                    RemoveBlessingFromMember(member);
                }
            }
            
            Debug.Log($"Cleric {gameObject.name} blessing effect ended!");
        }

        private void RemoveBlessingFromMember(ICharacter member)
        {
            // 祝福効果の除去（実際の効果は戦闘システムで実装）
            Debug.Log($"Blessing removed from {member}");
        }

        private void UpdateSanctuary()
        {
            if (!hasSanctuaryActive) return;
            
            sanctuaryTimer -= Time.deltaTime;
            
            // 聖域内での回復処理
            ProcessSanctuaryHealing();
            
            if (sanctuaryTimer <= 0f)
            {
                RemoveSanctuary();
            }
        }

        private void ProcessSanctuaryHealing()
        {
            // 聖域範囲内のパーティメンバーを回復
            if (currentParty != null)
            {
                foreach (var member in currentParty.Members)
                {
                    if (Vector2.Distance(sanctuaryPosition, member.Position) <= sanctuaryRange)
                    {
                        float healAmount = GetAttackPower() * 0.3f * Time.deltaTime;
                        member.Heal(healAmount);
                    }
                }
            }
        }

        private void RemoveSanctuary()
        {
            if (!hasSanctuaryActive) return;
            
            hasSanctuaryActive = false;
            sanctuaryTimer = 0f;
            
            ShowSanctuaryEndEffect();
            Debug.Log($"Cleric {gameObject.name} sanctuary expired!");
        }

        // 復活スペル
        public void Resurrect(ICharacter target)
        {
            if (currentMana >= 60f && target.Health <= 0)
            {
                currentMana -= 60f;
                ExecuteResurrection(target);
            }
        }

        private void ExecuteResurrection(ICharacter target)
        {
            float reviveHealth = target.MaxHealth * 0.3f;
            target.Heal(reviveHealth);
            
            ShowResurrectionEffect();
            Debug.Log($"Cleric {gameObject.name} resurrected {target} with {reviveHealth} HP!");
        }

        public override void TakeDamage(float damage)
        {
            // 祝福効果中はダメージ軽減
            if (hasBlessingActive)
            {
                damage *= (1f - blessingBonus * 0.5f);
                ShowBlessingDefenseEffect();
            }
            
            base.TakeDamage(damage);
        }

        // エフェクト表示メソッド
        private void ShowHealEffect()
        {
            // TODO: 回復エフェクト
            Debug.Log($"Cleric {gameObject.name} channels healing light!");
        }

        private void ShowBlessingEffect()
        {
            // TODO: 祝福エフェクト
            Debug.Log($"Cleric {gameObject.name} calls upon divine blessing!");
        }

        private void ShowBlessingEndEffect()
        {
            // TODO: 祝福終了エフェクト
            Debug.Log($"Cleric {gameObject.name} blessing fades away!");
        }

        private void ShowBlessingDefenseEffect()
        {
            // TODO: 祝福防御エフェクト
            Debug.Log($"Cleric {gameObject.name} blessing reduces damage!");
        }

        private void ShowSanctuaryEffect()
        {
            // TODO: 聖域エフェクト
            Debug.Log($"Cleric {gameObject.name} creates a holy sanctuary!");
        }

        private void ShowSanctuaryEndEffect()
        {
            // TODO: 聖域終了エフェクト
            Debug.Log($"Cleric {gameObject.name} sanctuary dissipates!");
        }

        private void ShowResurrectionEffect()
        {
            // TODO: 復活エフェクト
            Debug.Log($"Cleric {gameObject.name} channels resurrection magic!");
        }

        // ゲッター
        public bool HasBlessingActive()
        {
            return hasBlessingActive;
        }

        public float GetBlessingTimeRemaining()
        {
            return blessingTimer;
        }

        public bool HasSanctuaryActive()
        {
            return hasSanctuaryActive;
        }

        public float GetSanctuaryTimeRemaining()
        {
            return sanctuaryTimer;
        }

        public Vector2 GetSanctuaryPosition()
        {
            return sanctuaryPosition;
        }

        public float GetSanctuaryRange()
        {
            return sanctuaryRange;
        }

        protected override void OnDrawGizmosSelected()
        {
            base.OnDrawGizmosSelected();
            
            // 聖域範囲を表示
            if (hasSanctuaryActive)
            {
                Gizmos.color = Color.white;
                Gizmos.DrawWireSphere(sanctuaryPosition, sanctuaryRange);
            }
            
            // 祝福効果を表示
            if (hasBlessingActive)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(transform.position, 2f);
            }
        }
    }
}