using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

namespace JustAwake
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(PlayerInput))]
    public class PlayerController : MonoBehaviour
    {
		public GameObject GameManager;
        [Header("Player")]
		[Tooltip("Move speed of the character in m/s")]
		public float MoveSpeed = 4.0f;
        [Tooltip("Sprint speed of the character in m/s")]
		public float SprintSpeed = 6.0f;
        [Tooltip("Rotation speed of the character")]
		public float RotationSpeed = 1.0f;
        [Tooltip("Acceleration and deceleration")]
		public float SpeedChangeRate = 10.0f;

        [Space(10)]
		[Tooltip("The height the player can jump")]
		public float JumpHeight = 1.2f;
        [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
		public float Gravity = -15.0f;

        [Space(10)]
		[Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
		public float JumpTimeout = 0.1f;
        [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
		public float FallTimeout = 0.15f;
		[Tooltip("Time required to pass before being able to deploy when falling.")]
		public float DeployTimeout = 0.8f;

        [Header("Player Grounded")]
		[Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
		public bool Grounded = true;
        [Tooltip("Useful for rough ground")]
		public float GroundedOffset = -0.14f;
        [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
		public float GroundedRadius = 0.5f;
        [Tooltip("What layers the character uses as ground")]
		public LayerMask GroundLayers;

        [Header("Player OnBorder")]
		[Tooltip("If the character is on the borders or not.")]
		public bool OnBorder;
        [Tooltip("What layers the character uses as border")]
		public LayerMask BorderLayers;

		[Header("Health, Damage and Score")]
		[Tooltip("Player health")]
        public float Health;
        [Tooltip("Damage that player takes")]
		public float Damage;
        [Tooltip("Score accumulated")]
		public float Score;
		[Tooltip("Player is dead or not")]
		public bool Dead;

		[Header("DeployChance, ...")]
		[Tooltip("If having a chance to deploy while falling or not")]
		public bool DeployChance;

        [Header("Cinemachine")]
		[Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
		public GameObject CinemachineCameraTarget;
        [Tooltip("How far in degrees can you move the camera up")]
		public float TopClamp = 90.0f;
        [Tooltip("How far in degrees can you move the camera down")]
		public float BottomClamp = -90.0f;

		[Header("GameUI")]
		[Tooltip("Height Text")]
		public TextMeshProUGUI HeightText;
		public TextMeshProUGUI HeightTextShadow;
		[Tooltip("Score Text")]
		public TextMeshProUGUI ScoreText;
		public TextMeshProUGUI ScoreTextShadow;
		[Tooltip("ScorePlus Text")]
		public TextMeshProUGUI ScorePlusText;

		[Header("Animator Parameters")]
		public Animator ScoreAnim;
		public string ScoreChangeAnim;
		public Animator ScorePlusAnim;
		public string ScorePlusFadeOut;
		public Animator BloodyPanelAnim;
		public string BloodyPanelFadeOut;
		public string BloodyPanelFadeIn;
		public Animator DeployRemindAnim;
		public string DeployRemindFadeOut;
		public string CloseRemindFadeOut;

		[Header("Audio Source")]
		public AudioSource JumpSoundEffect;
		public AudioSource ScorePlusSoundEffect;
		public AudioSource LargeScorePlusSoundEffect;
		public AudioSource DeploySoundEffect;
		public AudioSource HurtSoundEffect;


        // cinemachine
		private float _cinemachineTargetPitch;
		private bool _cinemachineTargetSelfMove;
		private Vector3 _cinemachineTargetSelfMoveTargetPosition;
		private float _cinemachineTargetSelfMoveSpeed;
		private float _cinemachineTargetSelfMoveTimeoutDelta = 0.8f;
		private bool _cinemachineTargetSelfRotate;
		private float _cinemachineTargetSelfRotateAngle;
		private float _cinemachineTargetSelfRotateSpeed;
		private float _cinemachineTargetSelfRotateTimeoutDelta = 0.8f;

		// player
        private float _speed;
		private float _rotationVelocity;
		private float _verticalVelocity;
		private float _terminalVelocity = 53.0f;
		// private bool speedReduce = false; // Whether to limit the horizontal velocity input

        // timeout deltatime
		private float _jumpTimeoutDelta;
		private float _fallTimeoutDelta;
		private float _deployTimeoutDelta;

		// Sound effect parameters
		private bool _jumpSoundEffect;


		private PlayerInput _playerInput;
		private CharacterController _controller;
		private InputSetting _input;
		private GameObject _mainCamera;
		private GameManage _gameManage;

		private const float _threshold = 0.01f;

		private void Awake()
		{
			// get a reference to our main camera
			if (_mainCamera == null)
			{
				_mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
			}

			// get GameManage component from Gamemanager
			_gameManage = GameManager.GetComponent<GameManage>();
		}

		private void Start()
		{
			_controller = GetComponent<CharacterController>();
			_input = GetComponent<InputSetting>();
			_playerInput = GetComponent<PlayerInput>();


			// reset player status
			Health = 100;
			Damage = 0;
			Score = 0;
			DeployChance = true;
			Dead = false;

			// reset cinemachineTarget parameters
			_cinemachineTargetSelfMove = false;
			_cinemachineTargetSelfRotate = false;

			// reset our timeouts on start
			_jumpTimeoutDelta = JumpTimeout;
			_fallTimeoutDelta = FallTimeout;
			_deployTimeoutDelta = DeployTimeout;
		}

		private void Update()
		{
			if(!_gameManage.Paused)
			{
				Peek();
				JumpAndGravity();
				GroundedCheck();
				Deploy();
				Move();
				HealthMethod();
				ScoreMethod();

				//GameUI;
				HeightText.text = ((int)transform.position.y).ToString();
				HeightTextShadow.text = ((int)transform.position.y).ToString();

				string scoreText;
				if(Score >= 1000000f)
				{
					scoreText = (Score/1000000f).ToString("F1")+"M";
				}
				else if(Score >= 1000f)
				{
					scoreText = (Score/1000f).ToString("F1")+"k";
				}
				else
				{
					scoreText = ((int)Score).ToString();
				}
				ScoreText.text = scoreText;
				ScoreTextShadow.text = scoreText;
			}
		}

		private void LateUpdate()
		{
			CameraMovement();
			CameraRotation();
		}

		private void GroundedCheck()
		{
			// set sphere position, with offset
			Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z);
			Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers, QueryTriggerInteraction.Ignore);
			OnBorder = Physics.CheckSphere(spherePosition, GroundedRadius, BorderLayers, QueryTriggerInteraction.Ignore);
		}

		private void CameraMovement()
		{
			Vector3 CinemachineTargetLocalPosition = CinemachineCameraTarget.transform.localPosition;
			if(_cinemachineTargetSelfMove)
			{
				Vector3 cinemachineTargetLocalPosition = Vector3.Lerp(CinemachineTargetLocalPosition, _cinemachineTargetSelfMoveTargetPosition, Time.deltaTime*_cinemachineTargetSelfMoveSpeed);
				CinemachineCameraTarget.transform.localPosition = cinemachineTargetLocalPosition;
				_cinemachineTargetSelfMoveTimeoutDelta -= Time.deltaTime;
				if(_cinemachineTargetSelfMoveTimeoutDelta <= 0f) _cinemachineTargetSelfMove = false;
			}
			else
			{
				float _cinemachineTargetSelfBackSpeed = Vector3.Distance(CinemachineTargetLocalPosition, new Vector3(0f, 2f, 0f))*10f;
				Vector3 cinemachineTargetLocalPosition = Vector3.Lerp(CinemachineTargetLocalPosition, new Vector3(0f, 2f, 0f), Time.deltaTime*_cinemachineTargetSelfBackSpeed);
				CinemachineCameraTarget.transform.localPosition = cinemachineTargetLocalPosition;
			}
		}

		private void CameraRotation()
		{
			//Don't multiply mouse input by Time.deltaTime
			float deltaTimeMultiplier = 1.0f;

			// if there is an input
			if (_input.look.sqrMagnitude >= _threshold)
			{
				_rotationVelocity = _input.look.x * RotationSpeed * deltaTimeMultiplier;

				// rotate the player left and right
				transform.Rotate(Vector3.up * _rotationVelocity);
			}

			if(_cinemachineTargetSelfRotate)
			{
				_cinemachineTargetPitch = Mathf.Lerp(_cinemachineTargetPitch, _cinemachineTargetSelfRotateAngle, Time.deltaTime*_cinemachineTargetSelfRotateSpeed);

				CinemachineCameraTarget.transform.localRotation = Quaternion.Euler(_cinemachineTargetPitch, 0.0f, 0.0f);

				_cinemachineTargetSelfRotateTimeoutDelta -= Time.deltaTime;
				if(_cinemachineTargetSelfRotateTimeoutDelta <= 0f && _input.look.sqrMagnitude >= _threshold) _cinemachineTargetSelfRotate = false;
			}
			else if (_input.look.sqrMagnitude >= _threshold)
			{
				_cinemachineTargetPitch += _input.look.y * RotationSpeed * deltaTimeMultiplier;

				// clamp our pitch rotation
				_cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

				// Update Cinemachine camera target pitch
				CinemachineCameraTarget.transform.localRotation = Quaternion.Euler(_cinemachineTargetPitch, 0.0f, 0.0f);
			}
		}

		private void Move()
		{
			// set target speed based on move speed, sprint speed and if sprint is pressed
			float targetSpeed = _input.sprint ? SprintSpeed : MoveSpeed;

			// a simplistic acceleration and deceleration designed to be easy to remove, replace, or iterate upon

			// note: Vector2's == operator uses approximation so is not floating point error prone, and is cheaper than magnitude
			// if there is no input, set the target speed to 0
			if (_input.move == Vector2.zero) targetSpeed = 0.0f;

			// a reference to the players current horizontal velocity
			float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

			float speedOffset = 0.1f;
			float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

			// accelerate or decelerate to target speed
			if (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset)
			{
				// creates curved result rather than a linear one giving a more organic speed change
				// note T in Lerp is clamped, so we don't need to clamp our speed
				_speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude, Time.deltaTime * SpeedChangeRate);

				// round speed to 3 decimal places
				_speed = Mathf.Round(_speed * 1000f) / 1000f;
			}
			else
			{
				_speed = targetSpeed;
			}

			// normalise input direction
			Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

			// note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
			// if there is a move input rotate player when the player is moving
			if (_input.move != Vector2.zero)
			{
				// move
				inputDirection = transform.right * _input.move.x + transform.forward * _input.move.y;
			}

			/*Lock the horizontal velocity when falling
			if (!Grounded) 
			{
				Vector3 currentHorizontalVelocity = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z);
				inputDirection = currentHorizontalVelocity + transform.right * _input.move.x * Mathf.Sqrt(0.005f) + transform.forward * _input.move.y * Mathf.Sqrt(0.005f);
				_speed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;
				if (speedReduce)
				{
					_speed *= 0.2f;
					speedReduce = false;
				}
			}
			*/

			// move the player
			_controller.Move(inputDirection.normalized * (_speed * Time.deltaTime) + new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);

			/*Let player won't get fallen out of the border while peeking
			if(_input.peek && OnBorder)
			{
				Vector3 controllerPosition = transform.position;
				_controller.Move(inputDirection.normalized * (_speed * Time.deltaTime) + new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);
				GroundedCheck();
				transform.position = !Grounded ? controllerPosition : transform.position;
			}
			else
			{
				_controller.Move(inputDirection.normalized * (_speed * Time.deltaTime) + new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);
			}
			*/
		}

		private void JumpAndGravity()
		{
			if (Grounded)
			{
				if(!_input.jump) _jumpSoundEffect = true;

				// reset the fall timeout timer
				_fallTimeoutDelta = FallTimeout;

				// stop our velocity dropping infinitely when grounded
				if (_verticalVelocity < 0.0f)
				{
					_verticalVelocity = -2f;
				}

				// Jump
				if (_input.jump && _jumpTimeoutDelta <= 0.0f && !(_input.peek && OnBorder))
				{
					if (_jumpSoundEffect) 
					{
						JumpSoundEffect.Play();
						_jumpSoundEffect = false;
					}
					// the square root of H * -2 * G = how much velocity needed to reach desired height
					_verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);
				}

				// jump timeout
				if (_jumpTimeoutDelta >= 0.0f)
				{
					_jumpTimeoutDelta -= Time.deltaTime;
				}
			}
			else
			{
				// reset the jump timeout timer
				_jumpTimeoutDelta = JumpTimeout;

				// fall timeout
				if (_fallTimeoutDelta >= 0.0f)
				{
					_fallTimeoutDelta -= Time.deltaTime;
				}

				// if we are not grounded, do not jump
				_input.jump = false;

				// Use 'Gravity == 0f' to represent the player has deployed, not exactly
				if(Gravity == 0f)
				{
					_verticalVelocity = Mathf.Lerp(_verticalVelocity, -40f/3f, Time.deltaTime);
				}
			}

			// apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
			if (_verticalVelocity < _terminalVelocity)
			{
				_verticalVelocity += Gravity * Time.deltaTime;
			}
		}

		private void Peek()
		{
			if(OnBorder)
			{
				if(_input.peek)
				{
					_input.jump = false;

					// LocalPosition of cinemachine smoothly add 1f on z axis
					_cinemachineTargetSelfMove = true;
					_cinemachineTargetSelfMoveTargetPosition = new Vector3(0f, 2f, 1f);
					_cinemachineTargetSelfMoveSpeed = 1f;
					_cinemachineTargetSelfMoveTimeoutDelta = 0f;
				}
			}
		}

		private void Deploy()
		{
			if(Grounded)
			{
				Gravity = -15.0f;
				_deployTimeoutDelta = DeployTimeout;

				// Bad landing
				if(_verticalVelocity < -10f)
				{
					_cinemachineTargetSelfRotate = true;
					_cinemachineTargetSelfRotateAngle = Mathf.Abs(_cinemachineTargetPitch) < 60f ? 20f : 30f;
					_cinemachineTargetSelfRotateSpeed = 2f;
					_cinemachineTargetSelfRotateTimeoutDelta = 0.8f;
				}
				else
				{
					// Perfect landing
					if(!DeployChance)
					{
						_cinemachineTargetSelfMove = true;
						_cinemachineTargetSelfMoveTargetPosition = new Vector3(0f, CinemachineCameraTarget.transform.localPosition.y - 0.5f, -0.5f);
						_cinemachineTargetSelfMoveSpeed = 10f;
						_cinemachineTargetSelfMoveTimeoutDelta = 0.8f;
						_cinemachineTargetSelfRotate = true;
						_cinemachineTargetSelfRotateAngle = 20f;
						_cinemachineTargetSelfRotateSpeed = 2f;
						_cinemachineTargetSelfRotateTimeoutDelta = 0.8f;

						// Reward
						Score += 30000;

						LargeScorePlusSoundEffect.Play();
						ScoreAnim.Play(ScoreChangeAnim);
						ScorePlusText.text = "+30000";
						ScorePlusAnim.Play(ScorePlusFadeOut);
					}
				}

				DeployChance = true;
				DeployRemindAnim.Play("New State");
			}
			else
			{
				if(_input.deploy)
				{
					if(_deployTimeoutDelta <= 0.0f){
						if(DeployChance)
						{
							// speedReduce = true;
							_verticalVelocity = -2f;
							Gravity = 0f;
							DeployChance = false;

							_cinemachineTargetSelfRotate = true;
							_cinemachineTargetSelfRotateAngle = 60f;
							_cinemachineTargetSelfRotateSpeed = 1f;
							_cinemachineTargetSelfRotateTimeoutDelta = 0.8f;

							DeploySoundEffect.Play();
							DeployRemindAnim.Play(CloseRemindFadeOut);
						}
						else
						{
							Gravity = -15f;
							DeployRemindAnim.Play("New State");
						}
					}

					_input.deploy = false;
				}

				if (_deployTimeoutDelta >= 0.0f)
				{
					_deployTimeoutDelta -= Time.deltaTime;
				}
				else
				{
					if(DeployChance)
					{
						DeployRemindAnim.Play(DeployRemindFadeOut);
					}
				}
			}
		}

		private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
		{
			if (lfAngle < -360f) lfAngle += 360f;
			if (lfAngle > 360f) lfAngle -= 360f;
			return Mathf.Clamp(lfAngle, lfMin, lfMax);
		}

		private void OnDrawGizmosSelected()
		{
			Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
			Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

			if (Grounded) Gizmos.color = transparentGreen;
			else Gizmos.color = transparentRed;

			// when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
			Gizmos.DrawSphere(new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z), GroundedRadius);
		}

		private void HealthMethod()
		{
			if(!Grounded)
			{
				if(_verticalVelocity <= -20)
				{
					Damage = (0.27f*Mathf.Pow(_verticalVelocity,2)-108f)/10f;
				}
				else
				{
					Damage = 0f;
				}
			}

			if(Damage != 0 && Grounded)
			{
				Health -= Damage;

				HurtSoundEffect.Play();
				BloodyPanelAnim.Play(BloodyPanelFadeOut);

				Damage = 0;
			}

			if(Health < 0)
			{
				Health = 0;
				Dead = true;
				Cursor.lockState = CursorLockMode.None;
				BloodyPanelAnim.Play(BloodyPanelFadeIn);
			}
		}

		private void ScoreMethod()
		{
			float nowScore = Score;

			if(!Grounded)
			{
				if(_verticalVelocity < -56f)
				{
					Score += _verticalVelocity*-4.0f;
				}
				else if(_verticalVelocity < -44f)
				{
					Score += _verticalVelocity*-2.5f;
				}
				else if(_verticalVelocity < -32f)
				{
					Score += _verticalVelocity*-1.8f;
				}
				else if(_verticalVelocity < -24f)
				{
					Score += _verticalVelocity*-1.3f;
				}
				else if(_verticalVelocity < -18f)
				{
					Score += _verticalVelocity*-0.8f;
				}
				else if(_verticalVelocity < -14f)
				{
					Score += _verticalVelocity*-0.6f;
				}
			}

			if(nowScore != Score)
			{
				ScorePlusSoundEffect.Play();
				// trimming the sound effect
				if (ScorePlusSoundEffect.time >= 1f) ScorePlusSoundEffect.time = 0f;
				ScoreAnim.Play(ScoreChangeAnim);
				ScorePlusText.text = "+"+(Score - nowScore).ToString("0");
				ScorePlusAnim.Play(ScorePlusFadeOut);
			}
			else
			{
				ScoreAnim.Play("New State");
			}
		}
    }
}