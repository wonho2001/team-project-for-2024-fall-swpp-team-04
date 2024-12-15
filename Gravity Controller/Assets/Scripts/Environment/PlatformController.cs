using UnityEngine;

public class PlatformController : MonoBehaviour
{
	[SerializeField] private float _descentSpeed = 1f; // �ʴ� �ϰ� �ӵ�
	private float _initialY; // �ʱ� Y��ǥ
	private float _targetY; // 7��ŭ ������ ��ǥ Y��ǥ
	private bool _isPlayerOnPlatform = false; // �÷��̾ ���� ���� �ִ��� ����
	private bool _isMoving = false; // ���� �̵� ����
	private Rigidbody _rb;

	private ParticleSystem[] _dust;
	[SerializeField] private float _unitParticleRate = 100;
	[SerializeField] private float _emissionPower;
	[SerializeField] private float _unitSpeed = 1;
	[Header("Audio")]
	[SerializeField] private float _crackingSfxMaxVolume;
	[SerializeField] private AudioClip _crackingSfx;
	[SerializeField] private AudioSource _breakingAudioSource;
	[SerializeField] private float _breakingSfxMaxVolume;
	[SerializeField] private AudioClip _breakingSfx;
	private AudioSource _audioSource;


	// �÷����� ���� ��ġ�� �����ϱ� ���� ����
	private Vector3 _previousPosition;

	void Start()
	{
		_initialY = transform.position.y; // �ʱ� Y��ǥ ����
		_targetY = _initialY - 7f; // ��ǥ Y��ǥ ���

		// Rigidbody ������Ʈ �������� �Ǵ� �߰�
		_rb = GetComponent<Rigidbody>();
		if (_rb == null)
		{
			_rb = gameObject.AddComponent<Rigidbody>();
			_rb.isKinematic = true; // Kinematic���� ����
		}

		// Rigidbody ���� Ȱ��ȭ (�� �ε巯�� �������� ����)
		_rb.interpolation = RigidbodyInterpolation.Interpolate;

		_previousPosition = _rb.position;

		_dust = transform.Find("Dust").GetComponentsInChildren<ParticleSystem>();

		_audioSource = GetComponent<AudioSource>();
	}

	void OnCollisionEnter(Collision collision)
	{
		if (collision.collider.CompareTag("Player"))
		{
			_isPlayerOnPlatform = true;
			_isMoving = true;
			if(!_audioSource.isPlaying) {
				_audioSource.clip = _crackingSfx;
				_audioSource.volume = _crackingSfxMaxVolume * GameManager.Instance.GetSFXVolume();
				_audioSource.Play();
			}
		}
	}

	void OnCollisionExit(Collision collision)
	{
		if (collision.collider.CompareTag("Player"))
		{
			_isPlayerOnPlatform = false;
			_isMoving = false;
			if(_audioSource.isPlaying) {
				_audioSource.Stop();
			}
		}
	}

	void Update()
	{
		if (_isPlayerOnPlatform && _isMoving)
		{
			foreach(ParticleSystem dustParticle in _dust)
			{
				dustParticle.Play();
			}
			MovePlatform();
		}
		else
		{
			foreach (ParticleSystem dustParticle in _dust)
			{
				dustParticle.Stop();
			}
		}

		_previousPosition = _rb.position;
	}

	private void MovePlatform()
	{
		float step = _descentSpeed * Time.deltaTime; 
		Vector3 targetPosition = new Vector3(transform.position.x, _targetY, transform.position.z);
		Vector3 newPosition = Vector3.MoveTowards(_rb.position, targetPosition, step);
		_rb.MovePosition(newPosition);

		foreach (ParticleSystem dustParticle in _dust)
		{
			var main = dustParticle.main;
			main.startColor = IsEmergency() ? (Color) new Color32(0x60, 0x37, 0x3e, 0xff) : (Color) new Color32(0x67, 0x63, 0x5d, 0xff);

			var mult = main.startSpeedMultiplier;
			var em = dustParticle.emission;
			mult = _unitSpeed * (7 + _targetY - transform.position.y);
			em.rateOverTime = _unitParticleRate * Mathf.Pow((7 + _targetY - transform.position.y), _emissionPower);
		}

		if (newPosition.y <= _targetY)
		{
			_rb.position = targetPosition;
			_isMoving = false;
			
			var sparkle = transform.parent.Find("SparkleDust");
			sparkle.position = newPosition;
			var main = sparkle.GetComponent<ParticleSystem>().main;
			main.startColor = IsEmergency() ? (Color)new Color32(0x60, 0x37, 0x3e, 0xff) : (Color)new Color32(0x67, 0x63, 0x5d, 0xff);
			sparkle.gameObject.SetActive(true);

			_breakingAudioSource.volume = _breakingSfxMaxVolume * GameManager.Instance.GetSFXVolume();
			_breakingAudioSource.PlayOneShot(_breakingSfx);

			gameObject.SetActive(false);
		}
	}

	private bool IsEmergency()
	{
		return GameObject.Find("Stage_third").transform.Find("FUSION_CORE").GetComponent<CoreInteraction>().IsEmergency;
	}
}
