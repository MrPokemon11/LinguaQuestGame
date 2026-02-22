using System;
using System.Collections;
using UnityEngine;

namespace World1BossFight
{
    public class RollingLog : MonoBehaviour
    {
        private Rigidbody2D _rigidbody2D;
        private Animator _animator;

        private bool _isRolling;

        private void Awake()
        {
            _rigidbody2D = GetComponent<Rigidbody2D>();
            _animator = GetComponent<Animator>();
        }

        public void ThrowUpAndRoll(Vector2 direction, float speed)
        {
            _animator.SetTrigger("ThrowUp");
            _rigidbody2D.linearVelocity = direction;
            StartCoroutine(RollRoutine(direction, speed));
        }

        private void Roll(Vector2 direction, float speed)
        {
            _rigidbody2D.linearVelocity = direction * speed;
            _isRolling = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!_isRolling) return;
            if (other.CompareTag("Player"))
            {
                //var player = other.GetComponent<PlayerExploring>();
                // Damage Player
                Debug.Log("Damaged Player");
            }
            else if (other.CompareTag("Log"))
            {
                _rigidbody2D.linearVelocity = -_rigidbody2D.linearVelocity;
            }
            else if (other.CompareTag("Void"))
            {
                _animator.SetTrigger("FallDown");
            }
        }

        private IEnumerator RollRoutine(Vector2 direction, float speed)
        {
            yield return new WaitForSeconds(1);
            Roll(direction, speed);
        }
    }
}
