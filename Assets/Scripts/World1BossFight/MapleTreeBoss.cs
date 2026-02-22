using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

namespace World1BossFight
{
    public class MapleTreeBoss : MonoBehaviour
    {
        [Header("Health & Staging")]
        [SerializeField] private int maxHealth;
        [Range(0,1)] [SerializeField] private float mediumStageUpperPercentage;
        [Range(0,1)] [SerializeField] private float hardStageUpperPercentage;
        [Space]
        [SerializeField] private GameObject bossHeartPrefab;
        
        [Header("Rolling Log Attack")]
        [SerializeField] private GameObject rollingLogPrefab;
        [SerializeField] private Vector3Int rollingLogStageCount;
        [SerializeField] private Vector3 rollingLogStageSpeed;
        [SerializeField] private Vector3 rollingLogStageAttackSpeed;
        [Space]
        [SerializeField] private Transform leftRollingLogSpawnPoint;
        [SerializeField] private Transform rightRollingLogSpawnPoint;
        [SerializeField] private float rollingLogSpawnPointHeight;
        
        [Header("Branch Strike Attack")]
        [SerializeField] private GameObject branchStrikePrefab;
        [SerializeField] private Vector3Int branchStrikeStageCount;
        [SerializeField] private Vector3 branchStrikeStageSpeed;
        [SerializeField] private Vector3 branchStrikeStageAttackSpeed;
        
        [Header("Maple Leaf Slam Attack")]
        [SerializeField] private GameObject mapleLeafSlamPrefab;
        [SerializeField] private Vector3Int mapleLeafSlamStageCount;
        [SerializeField] private Vector3 mapleLeafSlamStageDelay;
        [SerializeField] private Vector3 mapleLeafSlamStageAttackSpeed;
        
        [Header("Hedge Split Attack")]
        [SerializeField] private GameObject hedgeSplitPrefab;
        [SerializeField] private Vector3 hedgeSplitStageQuestionDuration;

        private int _health;

        private void Awake()
        {
            _health = maxHealth;
            //PerformRollingLogAttack();
            PerformMapleLeafSlamAttack();
        }

        private float GetStageValue(Vector3 value)
        {
            var ratio = _health / maxHealth;
            if (ratio < hardStageUpperPercentage) return value.z;
            return ratio < mediumStageUpperPercentage ? value.y : value.x;
        }

        public void PerformRollingLogAttack()
        {
            StartCoroutine(RollingLogAttackRoutine());
        }

        private IEnumerator RollingLogAttackRoutine()
        {
            var count = GetStageValue(rollingLogStageCount);
            var speed = GetStageValue(rollingLogStageSpeed);
            var attackSpeed = GetStageValue(rollingLogStageAttackSpeed);
            for (var i = 0; i < count; i++)
            {
                var spawnLeft = Random.Range(0, 2) == 1;
                var spawnTransform = spawnLeft ? leftRollingLogSpawnPoint : rightRollingLogSpawnPoint;
                var spawnOffset = (int)Random.Range(-rollingLogSpawnPointHeight, rollingLogSpawnPointHeight);
                var direction = spawnLeft ? Vector3.right : Vector3.left;
                var spawnPosition = spawnTransform.position + Vector3.up * spawnOffset;
                
                var rollingLogGameObject = Instantiate(rollingLogPrefab, spawnPosition, Quaternion.identity);
                var rollingLog = rollingLogGameObject.GetComponent<RollingLog>();
                rollingLog.ThrowUpAndRoll(direction, speed);
                yield return new WaitForSeconds(attackSpeed);
            }
        }

        public void PerformBranchStrikeAttack()
        {
            
        }
        
        private IEnumerator BranchStrikeAttackRoutine()
        {
            yield return null;
        }

        public void PerformMapleLeafSlamAttack()
        {
            StartCoroutine(MapleLeafSlamAttackRoutine());
        }
        
        private IEnumerator MapleLeafSlamAttackRoutine()
        {
            var count = GetStageValue(mapleLeafSlamStageCount);
            var delay = GetStageValue(mapleLeafSlamStageDelay);
            var attackSpeed = GetStageValue(mapleLeafSlamStageAttackSpeed);
            for (var i = 0; i < count; i++)
            {
                var mapleLeafSlamGameObject = Instantiate(mapleLeafSlamPrefab);
                var mapleLeafSlam = mapleLeafSlamGameObject.GetComponent<MapleLeafSlam>();
                mapleLeafSlam.Slam(delay);
                yield return new WaitForSeconds(attackSpeed);
            }
        }

        public void PerformHedgeSplitAttack()
        {
            
        }
        
        private IEnumerator HedgeSplitAttackRoutine()
        {
            yield return null;
        }
    }
}