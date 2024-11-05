using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WalkingEnemy : MonoBehaviour
{
	private Animator _animator;

	[Header("Wander")]
	[SerializeField] private float _speedWander;
	[SerializeField] private float _rangeWander;
	[SerializeField] private float _changeDirectionInterval;
	[SerializeField] private float _obstacleDetectionRange;

	[Header("Chase")]
	[SerializeField] private GameObject _player;
	[SerializeField] private float _speedChase;
	[SerializeField] private float _rangeAttack;
	[SerializeField] private float _rangeChase;
	private bool _isChase = false;
	private bool _isAttack = false;
	
	
	private Vector3 _spawnPoint;
	[SerializeField] private Vector3 _currentDirection;
	private float _timer;
	

	void Start() {
		_animator = GetComponent<Animator>();
		_spawnPoint = transform.position;
		SetRandomDirection();
		_timer = _changeDirectionInterval;
	}

	// fix: 구조를 조금 더 깔끔하게 변경
	void Update() {
		float distanceToPlayer = Vector3.Distance(transform.position, _player.transform.position);

		if(!_isChase) {
			if(distanceToPlayer < _rangeChase) { // player firstly detected
				_isChase = true;
				_animator.SetBool("FollowPlayer", true);
			} else { // player not detected
				Wander();
			}
		} else { // player detected
			if(distanceToPlayer < _rangeAttack) { // if close enough
				Attack();
			} else {
				Chase();
			}
		}
	}

	private void Wander() {
		_timer += Time.deltaTime;
		if (_timer > _changeDirectionInterval) {
			// Issue: interval에도 약간의 랜덤성을 주면 좋을 것 같음
			SetRandomDirection();
			_timer = 0f;
		} else if (Vector3.Distance(_spawnPoint, transform.position) > _rangeWander) {
			// 일정 범위를 벗어나지 못하도록
			_currentDirection = (_spawnPoint - transform.position).normalized;
			_timer = 0f;
		} else if (Physics.Raycast(transform.position, _currentDirection, _obstacleDetectionRange)) {
			// 장애물을 만나면 다른 방향으로
			SetRandomDirection();
			_timer = 0f;
		}

		// rotate
		Quaternion targetRotation = Quaternion.LookRotation(_currentDirection);
		transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * _speedWander);
		// move
		transform.Translate(Vector3.forward * _speedWander * Time.deltaTime);
	}

	private void SetRandomDirection() {
		float angle = Random.Range(0, 360);
		_currentDirection = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), 0, Mathf.Sin(angle * Mathf.Deg2Rad)).normalized;
	}

	// fix: isAttack 확인 필요 없을 것으로 보여 삭제
	// fix: 플레이어의 높낮이의 변화가 있으면 문제가 생길 것 같아서 수정
	private void Chase() {
		Vector3 direction = Vector3.Scale(_player.transform.position - transform.position, new Vector3(1, 0, 1)).normalized;
		transform.position += direction * _speedChase * Time.deltaTime;
		transform.LookAt(_player.transform.position);
	}

	private void Attack() {
		if (_isAttack) {
			return;
		}

		_isAttack = true;
		_animator.SetBool("AttackPlayer", true);

		StartCoroutine(ResetAttack());
	}

	// fix: Invoke 대신 Coroutine 사용하는 구조로 변경
	IEnumerator ResetAttack() {
		yield return new WaitForSeconds(1f);

		_isAttack = false;
		_animator.SetBool("AttackPlayer", false);
	}

	// fix: player controller가 유효한지 검사하는 과정 삭제
	public void AttackHitCheck() {
		float distanceToPlayer = Vector3.Distance(transform.position, _player.transform.position);

		if (distanceToPlayer < _rangeAttack) {
			Debug.Log("Attack Successful");
			_player.GetComponent<PlayerController>().OnHit();
		} else {
			Debug.Log("Attack Fail");
		}
	}

	private void OnDrawGizmosSelected() {
		Gizmos.color = Color.red;
		Gizmos.DrawRay(transform.position, _currentDirection * _obstacleDetectionRange);
		Gizmos.color = Color.green;
		Gizmos.DrawWireSphere(_spawnPoint, _rangeWander);
		Gizmos.color = Color.blue;
		Gizmos.DrawWireSphere(transform.position, _rangeChase);
	}
}
