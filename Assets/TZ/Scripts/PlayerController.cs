using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CubeSurfer
{
	public class PlayerController : MonoBehaviour
	{
		private List<Cube> cubeCollection = new List<Cube>();
		private List<Rigidbody> mouseRagdoll = new List<Rigidbody>();


		private bool _isLive = true;

		[Header("UI")]
		[SerializeField] private GameObject takeCubeUI;
		[SerializeField] private TextMeshProUGUI scoreUI;
		[SerializeField] private TextMeshProUGUI highScoreUI;
		[SerializeField] private float _timeUI;


		[Header("Input")]
		[SerializeField] private float _speed;
		[SerializeField] private float _timeSpeed= 0;
		[SerializeField] private float _horizontalSpeed;

		[SerializeField] private float _positionX;
		[Header("Component")]
		[SerializeField] private GameObject magicCircle;
		[SerializeField] private GameObject mouse;
		[SerializeField] private ParticleSystem takeEffect;
		private ParticleSystem[] takeEffects;

		[Header("Seting cube")]
		[SerializeField] private float _rotationSpeed = 5f;
		[SerializeField] private float _horizontalLimit = 2f;
		[SerializeField] private float _heightCube = 1f;
		[SerializeField] private float _lineTower;

		[Header("Force mouse")]
		[SerializeField] private float _force = 10f;

		[Header("Audio")]
		[SerializeField] private AudioSource audioSourceMusic;
		[SerializeField] private AudioSource audioSourceFX;

		[SerializeField] private AudioClip takeAudio;
		[SerializeField] private AudioClip endAudio;



		private int _score = 0;
		private int _highScore = 0;
		private void Awake()
		{
			foreach(Rigidbody rb in mouse.GetComponentsInChildren<Rigidbody>())
			{
				rb.isKinematic = true;
				mouseRagdoll.Add(rb);
			}

			takeEffects = takeEffect.GetComponentsInChildren<ParticleSystem>();
			
		}
		private void Start()
		{
			if (PlayerPrefs.GetInt("_highScore") <= _highScore)
				PlayerPrefs.SetInt("_highScore", _highScore);

			_highScore = PlayerPrefs.GetInt("_highScore");

			highScoreUI.text = $"Highscore : " + _highScore.ToString();


			EventManager.EventTakeCube += OnEventTakeCube;
			EventManager.EventLostCube += OnEventLostCube;
			EventManager.EventInput += OnEventInput;

		}

		private void OnEventInput(Vector2 vector)
		{
			_positionX = vector.x;
		}

		private void OnEventLostCube(Cube cube)
		{
			cubeCollection.Remove(cube);
			audioSourceFX.PlayOneShot(endAudio);
			takeEffect.transform.position = cube.transform.position;
			takeEffect.Emit(1);
			foreach (ParticleSystem partical in takeEffects)
			{
				partical.Emit(3);
			}
		}

		private void OnEventTakeCube(Cube cube)
		{
			
			//Take cube and start time ui
			takeCubeUI.SetActive(true);
			StartCoroutine(ShowUI(_timeUI));

			_score++;
			scoreUI.text = $"Score : " + _score.ToString();
			
			transform.position = new Vector3(transform.position.x,transform.position.y + _heightCube,transform.position.z);
			cube.transform.position = new Vector3(transform.position.x,_heightCube / 2, transform.position.z);
			cube.transform.SetParent(transform);

			takeEffect.transform.position = cube.transform.position;
			takeEffect.Emit(1);
			foreach (ParticleSystem partical in takeEffects)
			{
				partical.Emit(1);
			}
			cubeCollection.Add(cube);
			audioSourceFX.PlayOneShot(takeAudio);
		}

		private void Update()
		{
			_timeSpeed += Time.deltaTime;
			if (_timeSpeed > 15)
			{
				_speed += 5;
				_timeSpeed += 15;
			}
			if (_isLive)
			{
				foreach (var cube in cubeCollection)
				{
					cube.transform.position = Vector3.Lerp(cube.transform.position, new Vector3(transform.position.x, cube.transform.position.y, transform.position.z), _rotationSpeed * Time.deltaTime);
					cube.transform.rotation = Quaternion.Lerp(cube.transform.rotation, Quaternion.identity, _rotationSpeed * Time.deltaTime);
				}
				float positionX = transform.position.x + _positionX * _horizontalSpeed * Time.deltaTime;
				positionX = Mathf.Clamp(positionX, -_horizontalLimit, _horizontalLimit);
				transform.position = new Vector3(positionX, transform.position.y, transform.position.z);

				transform.Translate(Vector3.forward * _speed * Time.deltaTime);
				magicCircle.transform.Translate(Vector3.down * _speed * Time.deltaTime);

				if (transform.position.y < cubeCollection.Count-1)
				{
					GameOver();
				}
				if (transform.position.z / 100 == 0) _speed++;
			}
			if(transform.position.z / 100 == 0)
			{
				_speed ++;
			}
		}
		private void OnTriggerEnter(Collider other)
		{
			if(other.tag == "Obstacle")
			{
				GameOver();
			}
		}
		private IEnumerator ShowUI(float timeUI)
		{
			yield return new WaitForSeconds(timeUI);
			takeCubeUI.SetActive(false);
		}
		private void GameOver()
		{
			if (_score > _highScore)
				PlayerPrefs.SetInt("_highScore",_score);

			audioSourceMusic.Stop();
			audioSourceFX.PlayOneShot(endAudio);


			EventManager.EventGameOver?.Invoke();
			_isLive = false;
			mouse.transform.parent = null;
			
			mouse.GetComponent<Animator>().enabled = false;

			foreach (Rigidbody rb in mouseRagdoll)
			{
				rb.isKinematic = false;
			}
			mouse.transform.position += Vector3.up;
			mouse.GetComponent<Rigidbody>().AddForce(Vector3.forward * _force, ForceMode.Impulse);
		}
		private void OnDestroy()
		{
			EventManager.EventTakeCube -= OnEventTakeCube;
			EventManager.EventLostCube -= OnEventLostCube;
			EventManager.EventInput -= OnEventInput;
		}
	}
}