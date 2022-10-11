using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FistVR;


namespace localMP
{
	public class CameraControl : MonoBehaviour
	{
		public Rigidbody CameraRigidbody;
		//public CharacterController controller;
		//public float MouseSpeed = 1;
		//public float MoveSpeed = 1;
		Vector2 rotation = Vector2.zero;
		public float speed = 3;
		private bool inMotion = false;
		public GameObject Projectile;
		public AudioClip firingSound;
		public Transform Muzzle;
		public AudioSource shotSound;
		public float fireRate;
		private float roundsPerSecond;
		public float muzzleVelocityBase;
		public float ProjectileSpread;
		private Vector2 centerScreen;
		//public float SprintSpeed;
		//private float OGMovespeed;
		//public Vector3 MovementDirection;

		// Use this for initialization
		void Start()
		{
			//OGMovespeed = MoveSpeed;
			roundsPerSecond = 1 / (fireRate / 60);
			centerScreen = new Vector2(Screen.width/2, Screen.height/2);
			
		}

		// Update is called once per frame
		void Update()
		{
			if (inMotion)
			{
				CameraRigidbody.drag = .1f;
			}
			else
			{
				CameraRigidbody.drag = 5;
			}
			if (Input.GetKey(KeyCode.W))
			{
				CameraRigidbody.AddRelativeForce(0, 0, 5, ForceMode.Acceleration);
				inMotion = true;
			}

			if (Input.GetKey(KeyCode.S))
			{
				CameraRigidbody.AddRelativeForce(0, 0, -5, ForceMode.Acceleration);
				inMotion = true;
			}
			if (Input.GetKey(KeyCode.A))
			{
				CameraRigidbody.AddRelativeForce(-5, 0, 0, ForceMode.Acceleration);
				inMotion = true;
			}
			if (Input.GetKey(KeyCode.D))
			{
				CameraRigidbody.AddRelativeForce(5, 0, 0, ForceMode.Acceleration);
				inMotion = true;
			}
			if (Input.GetKey(KeyCode.Space))
			{
				if (inMotion)
				{
					CameraRigidbody.AddForce(0, 5, 0, ForceMode.Acceleration);
				}
				if (!inMotion)
				{
					CameraRigidbody.AddForce(0, 25, 0, ForceMode.Acceleration);
				}
			}

			if (Input.GetKey(KeyCode.LeftShift))
			{
				if (inMotion)
				{
					CameraRigidbody.AddForce(0, -5, 0, ForceMode.Acceleration);
				}
				if (!inMotion)
				{
					CameraRigidbody.AddForce(0, -25, 0, ForceMode.Acceleration);
				}
			}
			if (!Input.GetKey(KeyCode.D) && !Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.S) && !Input.GetKey(KeyCode.W))
			{
				inMotion = false;
			}
			if (Input.GetKeyDown(KeyCode.Mouse0))
			{
				StartCoroutine(controlledFireRate());
			}
			else if (Input.GetKeyUp(KeyCode.Mouse0))
			{
				StopCoroutine(controlledFireRate());
			}



			//transform.Rotate(Vector3.up * Input.GetAxis("Mouse X") * MouseSpeed);
			//transform.Rotate(Vector3.right * Input.GetAxis("Mouse Y") * MouseSpeed);
			//Vector3 position = Input.mousePosition;
			//rotation.x += -1 * (position.y - centerScreen.y);
			//rotation.y += position.x - centerScreen.x;
			
			rotation.y += Input.GetAxis("Horizontal");
			//Cursor.lockState = CursorLockMode.Confined;
			Cursor.lockState = CursorLockMode.Locked;
			//centerScreen = Input.mousePosition;
			Debug.Log("MouseXValue is " + Input.GetAxis("Horizontal"));
			rotation.x += -Input.GetAxis("Vertical");			
			transform.eulerAngles = rotation * speed;
			//MovementDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
			//controller.Move(rotation.x, MovementDirection.y, MovementDirection.z * MoveSpeed);

		}
		IEnumerator controlledFireRate()
		{
			while (Input.GetKey(KeyCode.Mouse0))
			{
				GameObject gameObject = Object.Instantiate<GameObject>(this.Projectile, Muzzle.position, this.Muzzle.rotation);
				gameObject.transform.Rotate(new Vector3(Random.Range(this.ProjectileSpread, this.ProjectileSpread), Random.Range(-this.ProjectileSpread, this.ProjectileSpread), 0.0f));
				BallisticProjectile component = gameObject.GetComponent<BallisticProjectile>();
				component.FlightVelocityMultiplier *= 1;
				float muzzleVelocityBase = component.MuzzleVelocityBase;
				//if (this.IsHeldByBot && this.SosigHoldingThis.isDamPowerUp)
				//	muzzleVelocityBase *= this.SosigHoldingThis.BuffIntensity_DamPowerUpDown;
				component.Fire(muzzleVelocityBase, gameObject.transform.forward, (FVRFireArm)null);
				yield return new WaitForSecondsRealtime(roundsPerSecond);
			}       /*shotSound.Play();
		GameObject daBullet = Instantiate(Projectile, Muzzle.transform.position, Muzzle.rotation);
		BallisticProjectile projectile = daBullet.GetComponent<BallisticProjectile>();
		projectile.Awake();
			projectile.Fire(muzzleVelocityBase, gameObject.transform.forward, (FVRFireArm)null);
		*/


		}
	}
}
