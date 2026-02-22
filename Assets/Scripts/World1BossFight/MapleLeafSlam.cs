using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace World1BossFight
{
    public class MapleLeafSlam : MonoBehaviour
    {
        [SerializeField] private Vector2Int bounds;
        [SerializeField] private float dissolveDelay;
        
        private Animator _animator;
        private PlatformData _platformData;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        public void Slam(float delay)
        {
            _platformData = PlatformManager.Instance.FindAndReservePositions(bounds);
            if (!_platformData.IsValid)
            {
                Destroy(gameObject);
                return; 
            }
            transform.position = _platformData.StartPosition + (transform.localScale / 2f);
            StartCoroutine(SlamRoutine(delay));
        }

        private IEnumerator SlamRoutine(float delay)
        {
            yield return new WaitForSeconds(delay);
            _animator.SetTrigger("Slam");
            yield return new WaitForSeconds(dissolveDelay);
            PlatformManager.Instance.UnreservePositions(_platformData.Positions);
            Destroy(gameObject);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                Debug.Log("Damaged Player");
            }
        }
    }
}
