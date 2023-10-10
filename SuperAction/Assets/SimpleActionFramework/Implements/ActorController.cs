using CMF;
using UnityEngine;

namespace SimpleActionFramework.Implements
{

    //A very simplified controller script;
	//This script is an example of a very simple walker controller that covers only the basics of character movement;
    public class ActorController : Controller
    {
        private Mover mover;
        float currentVerticalSpeed = 0f;
        bool isGrounded;
        public float movementSpeed = 7f;
        public float jumpSpeed = 10f;
        public float gravity = 10f;
        public float gravityFactor = 1f;

		Vector3 lastVelocity = Vector3.zero;
		Vector3 innerVelocity = Vector3.zero;
		Vector3 overridenVelocity = Vector3.zero;

		public bool useGravity = true;
		public bool useCharacterInput = true;
		
		public Transform cameraTransform;
		public CharacterInput CharacterInput { get; set; }

        Transform tr;

        // Use this for initialization
        void Start()
        {
            tr = transform;
            mover = GetComponent<Mover>();
            CharacterInput = GetComponent<CharacterInput>();
        }

        void FixedUpdate()
        {
            //Run initial mover ground check;
            mover.CheckForGround();

            //If character was not grounded int the last frame and is now grounded, call 'OnGroundContactRegained' function;
            if(isGrounded == false && mover.IsGrounded() == true)
                OnGroundContactRegained(lastVelocity);

            //Check whether the character is grounded and store result;
            isGrounded = mover.IsGrounded();

            Vector3 _velocity = Vector3.zero;

            if (useCharacterInput)
            {
	            //Add player movement to velocity;
	            _velocity += CalculateMovementDirection() * movementSpeed;
	            
	            //Handle jumping;
	            if ((CharacterInput != null) && isGrounded && CharacterInput.IsJumpKeyPressed())
	            {
		            OnJumpStart();
		            currentVerticalSpeed = jumpSpeed;
		            isGrounded = false;
	            }
	            
            }

            //Handle gravity;
            if (!isGrounded && useGravity)
            {
	            currentVerticalSpeed -= gravity * gravityFactor * Time.deltaTime;
            }
            else
            {
	            if (currentVerticalSpeed <= 0f)
		            currentVerticalSpeed = 0f;
            }

            //Add vertical velocity;
            _velocity += tr.up * currentVerticalSpeed;

            //Save current velocity for next frame;
            if (isGrounded)
            {
	            if (_velocity.y < 0f)
		            _velocity.y = 0f;
	            if (overridenVelocity.y < 0f)
		            overridenVelocity.y = 0f;
	            if (innerVelocity.y < 0f)
		            innerVelocity.y = 0f;
            }
            
            _velocity += innerVelocity;
            lastVelocity = _velocity;

            mover.SetExtendSensorRange(isGrounded);
            mover.SetVelocity(_velocity + overridenVelocity);
            
            overridenVelocity *= 0.7f;
            if (overridenVelocity.magnitude < 0.1f)
			{
	            overridenVelocity = Vector3.zero;
			}
            
            innerVelocity = Vector3.zero;
        }

        private Vector3 CalculateMovementDirection()
        {
            //If no character input script is attached to this object, return no input;
			if(CharacterInput == null || !useCharacterInput)
				return Vector3.zero;

			Vector3 _direction = Vector3.zero;

			//If no camera transform has been assigned, use the character's transform axes to calculate the movement direction;
			if(cameraTransform == null)
			{
				_direction += tr.right * CharacterInput.GetHorizontalMovementInput();
				_direction += tr.forward * CharacterInput.GetVerticalMovementInput();
			}
			else
			{
				//If a camera transform has been assigned, use the assigned transform's axes for movement direction;
				//Project movement direction so movement stays parallel to the ground;
				_direction += Vector3.ProjectOnPlane(cameraTransform.right, tr.up).normalized * CharacterInput.GetHorizontalMovementInput();
				_direction += Vector3.ProjectOnPlane(cameraTransform.forward, tr.up).normalized * CharacterInput.GetVerticalMovementInput();
			}

			//If necessary, clamp movement vector to magnitude of 1f;
			if(_direction.magnitude > 1f)
				_direction.Normalize();

			return _direction;
        }

        //This function is called when the controller has landed on a surface after being in the air;
		void OnGroundContactRegained(Vector3 _collisionVelocity)
		{
			//Call 'OnLand' delegate function;
			if(OnLand != null)
				OnLand(_collisionVelocity);
		}

        //This function is called when the controller has started a jump;
        void OnJumpStart()
        {
            //Call 'OnJump' delegate function;
            if(OnJump != null)
                OnJump(lastVelocity);
        }

        //Return the current velocity of the character;
        public override Vector3 GetVelocity()
        {
            return lastVelocity;
        }

        //Return only the current movement velocity (without any vertical velocity);
        public override Vector3 GetMovementVelocity()
        {
            return lastVelocity;
        }

        //Return only the current movement velocity (without any vertical velocity);
        public Vector3 GetOverridenVelocity()
        {
            return overridenVelocity;
        }

        //Return whether the character is currently grounded;
        public override bool IsGrounded()
        {
            return isGrounded;
        }

        public void SetVerticalSpeed(float speed)
		{
			currentVerticalSpeed = speed;
		}

        public void AddExternalVelocity(Vector3 velocity)
        {
	        overridenVelocity += velocity;
        }

        public void SetExternalVelocity(Vector3 velocity)
        {
	        overridenVelocity = velocity;
        }
        
        public void AddVerticalSpeed(float spd)
		{
	        currentVerticalSpeed += spd;
		}

        public void SetGravityFactor(float factor)
        {
	        gravityFactor = factor;
        }
        
        public void ToggleGravity(bool toggle)
		{
			useGravity = toggle;
		}
        
        public void ToggleCharacterInput(bool toggle)
		{
			useCharacterInput = toggle;
		}

        public void SetMovementVelocity(Vector3 velocity)
        {
	        //Save current velocity for next frame;
	        innerVelocity = velocity;
        }
    }

}