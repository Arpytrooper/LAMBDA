using System.Collections.Generic;
using UnityEngine;
using FistVR;
using UnityEngine.UI;
using System.Collections;

namespace Arpy.LAMBDA
{
	public class SosigDroneCameraController : MonoBehaviour
	{
		public bool IsMotionSick = false;
		public Camera droneCam;
		private float MaxFOV = 81;
		public Rigidbody CameraRigidbody;
		//public CharacterController controller;
		//public float MouseSpeed = 1;
		//public float MoveSpeed = 1;
		private bool isControllingSosig = false;
		private Vector2 rotation = Vector2.zero;
		public float lookdirSpeed = .2f;
		public float speed = 3;
		public float sensitivity = 1;
		private float CameraSpeed = 5;
		public float cameraSpeed = 5;
		public float fastCameraSpeed = 10;
		//private bool inMotion = false;
		public AudioClip firingSound;
		public Transform Muzzle;
		public AudioSource shotSound;
		public Sosig ControllingSosig;
		public LayerMask mask;
		public Transform MFHolder;
		public Transform MF;
		public float MFDistance = 1;
		private Transform actualMovePoint;
		public Transform MeleePoint;
		public Transform AimTorwardsPoint;
		public GameObject Crosshair;
		public GameObject Crosshair2;
		private float storedwalkingspeed = 0;
		private float storedSightRange = 0;
		private float storedHearingRange = 0;
		public LayerMask environment;
		public LayerMask EnvironmentOnly;
		public LayerMask GunPickup;
		
		public float ClampRange;
		private GameObject RedSquare;
		private SosigWeapon currentGun;
		private SosigWeaponPlayerInterface currentGunInterface;
		private SosigHand currentHand;
		//private int CurInventoryIndex = 0;
		public List<SosigWeapon> sosigWeps;
		public float CrossHairScale = .25f;
		private bool semiAutoFired = false;
		//private Vector2 centerScreen;
		public float throwVelocity = 10;
		public float throwForce = 20;
		public GameObject ControllingParticleSystem;
		[Header("Object Controlling Stuffs")]
		public bool isControllingObject = false;
		private FVRPhysicalObject currentObject;
		private Rigidbody CRB;
		public float objectForce = 20;
		public float objectJump = 20;
		private float distanceToObject = 1;
		public float maxOBJSpeed = 10;
		public GameObject otherAimPoint;
		[Header("UI stuff")]
		public Text playerHealth;
		public Text AmmoCounter;
		public Text GunName;
		public GameObject arrowPointer;
		public Image reloadingImage;
		private float showHealthTime;
		public Color neutral;
		public Color Friend;
		public Color Enemy;
		public LayerMask iffCheckMask;
		public Material ReticleMaterial;
		private int sosigIFF;
		[Header("Health UI")]
		//0-3 count from head down
		//0 = head, 1= torso, 2 = legs, 3 = feet
		//FOR LINK TEXTS ONLY 4 = mustard
		public List<Image> LinkImages;
		public List<float> LinkIntegrityLastFrame;
		public List<Text> LinkTexts;
		private float MustardAmountLastFrame;
		public float healthShowTime = 4;
		public Gradient HealthGradient;
		public Gradient HealthAlphaKey;
		
		private KeyCode DroneSpawn;
		private KeyCode PossessionStart;
		private KeyCode PossessionStop;
		private KeyCode WeaponThrow;
		private KeyCode GrenadeThrow;
		private KeyCode Pickup;
		private bool isReloading = false;
		private Transform sosigBodyFollower;
		private Vector3 storedSosigAimPoint;
		private float StoredSosigHeadAngularDrag = 0.25f;
		private bool IsLeftie = false;
		

		void Start()
		{
#if UNITY_EDITOR
			DroneSpawn = KeyCode.Backslash;
			PossessionStart = KeyCode.E;
			PossessionStop = KeyCode.Q;
            WeaponThrow = KeyCode.T;
			GrenadeThrow = KeyCode.G;
			Pickup = KeyCode.F;
#endif
#if !UNITY_EDITOR
			DroneSpawn = DroneSpawner.DS.DroneSpawn.Value;
			PossessionStart = DroneSpawner.DS.PossessionStart.Value;
			PossessionStop = DroneSpawner.DS.PossessionStop.Value;
            WeaponThrow = DroneSpawner.DS.WeaponThrow.Value;
			GrenadeThrow = DroneSpawner.DS.GrenadeThrow.Value;
			Pickup = DroneSpawner.DS.Pickup.Value;
			IsMotionSick = DroneSpawner.DS.MotionSicknessToggle.Value;
			IsLeftie = DroneSpawner.DS.SouthPawToggle.Value;
			sensitivity = DroneSpawner.DS.Sensitivity.Value;			
			
#endif
			//OGMovespeed = MoveSpeed;
			sosigWeps = new List<SosigWeapon>();
			LinkIntegrityLastFrame = new List<float>();
			LinkIntegrityLastFrame.Add(100);
			LinkIntegrityLastFrame.Add(100);
			LinkIntegrityLastFrame.Add(100);
			LinkIntegrityLastFrame.Add(100);
			//centerScreen = new Vector2(Screen.width / 2, Screen.height / 2);
			RedSquare = new GameObject("aimingStart");
			RedSquare.transform.parent = transform;
			RedSquare.transform.localPosition = new Vector3(0, 0, .35f);
			MaxFOV = droneCam.fieldOfView;
			actualMovePoint = new GameObject().transform;
			//sosigBodyFollower = new GameObject().transform;
			storedSosigAimPoint = new Vector3();
			MFHolder.transform.parent = null;

		}

		// Update is called once per frame
		void Update()
		{
			//MF.transform.position = transform.position;
			//MF.transform.right = transform.right
			//THIS IS ALL CODE FOR MOVING THE MF (Move F*cker) POINT
			
			playerHealth.text = ((int)GM.CurrentPlayerBody.Health).ToString();
#region MF Code
			if (MF.localPosition.z != 0 && !Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.S))
			{
				MF.localPosition = new Vector3(MF.localPosition.x, MF.localPosition.y, 0);
			}
			if (MF.localPosition.x != 0 && !Input.GetKey(KeyCode.D) && !Input.GetKey(KeyCode.A))
			{
				MF.localPosition = new Vector3(0, MF.localPosition.y, MF.localPosition.z);

			}
			if (Input.GetKeyDown(KeyCode.W)) //moves the patrol point forwards
			{
				MF.localPosition = new Vector3(MF.localPosition.x, MF.localPosition.y, MF.localPosition.z + MFDistance);
				//MF.position += Vector3.forward;

			}
			else if (Input.GetKeyUp(KeyCode.W))
			{
				MF.localPosition = new Vector3(MF.localPosition.x, MF.localPosition.y, MF.localPosition.z - MFDistance);
				//MF.position -= Vector3.forward;

			}

			if (Input.GetKeyDown(KeyCode.S)) // Moves the patrol point backwards
			{
				MF.localPosition = new Vector3(MF.localPosition.x, MF.localPosition.y, MF.localPosition.z - MFDistance);
			}
			else if (Input.GetKeyUp(KeyCode.S))
			{
				MF.localPosition = new Vector3(MF.localPosition.x, MF.localPosition.y, MF.localPosition.z + MFDistance);
			}

			if (Input.GetKeyDown(KeyCode.D)) //Moves the patrol point right
			{
				MF.localPosition = new Vector3(MF.localPosition.x + MFDistance, MF.localPosition.y, MF.localPosition.z);
			}
			else if (Input.GetKeyUp(KeyCode.D))
			{
				MF.localPosition = new Vector3(MF.localPosition.x - MFDistance, MF.localPosition.y, MF.localPosition.z);
			}

			if (Input.GetKeyDown(KeyCode.A)) //Moves the patrol point left
			{
				MF.localPosition = new Vector3(MF.localPosition.x - MFDistance, MF.localPosition.y, MF.localPosition.z);
			}
			else if (Input.GetKeyUp(KeyCode.A))
			{
				MF.localPosition = new Vector3(MF.localPosition.x + MFDistance, MF.localPosition.y, MF.localPosition.z);
			}
			MF.localPosition = new Vector3(MF.localPosition.x, 0, MF.localPosition.z);
			//MFHolder.transform.rotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(transform.forward, Vector3.up));
			RaycastHit MoveDown;
			//Vector3 newDownPoint = otherAimPoint.transform.position;

			{
				if (Physics.Raycast(MF.position, Vector3.down, out MoveDown, 50, EnvironmentOnly))
				{
					//MF.position = new Vector3(MF.position.x, MF.position.y - MoveDown.distance, MF.position.z);
					actualMovePoint.position = MoveDown.point;
					//newDownPoint = MoveDown.point;
				}

			}
			#endregion
			//THUS ENDS THE MF POINT CODE
			if (Input.GetKeyDown(PossessionStop) && isControllingObject && !DroneSpawner.DS.CrazyMode.Value)
			{

			}
			if (isControllingSosig && ControllingSosig == null)
			{
				ReleaseSosig();
			}
			if (isControllingSosig && ControllingSosig.BodyState == Sosig.SosigBodyState.Dead)
			{
				ReleaseSosig();
			}
			if (currentObject == null && isControllingObject)
			{
				ReleaseObject();
			}
			if (ControllingSosig != null && ControllingSosig.BodyState != Sosig.SosigBodyState.Dead)
			{
				// Here we decide if we're gonna make the camera react as if it's in motion sick mode
				if (!IsMotionSick)
                {					
					//transform.position = sosigBodyFollower.position;
					Vector3 newpos = new Vector3(ControllingSosig.Links[0].transform.position.x, ControllingSosig.Links[0].transform.position.y + .15f, ControllingSosig.Links[0].transform.position.z);
					transform.position = newpos;
				}				
				else
				{
					Vector3 newpos = new Vector3(ControllingSosig.Links[1].transform.position.x, ControllingSosig.transform.position.y + 1.65f, ControllingSosig.Links[1].transform.position.z);
					transform.position = newpos;
				}
			}
			RaycastHit hitt;
			Vector3 hittPoint = otherAimPoint.transform.position;

			{
				if (Physics.Raycast(RedSquare.transform.position, transform.forward, out hitt, 1000f, environment))
				{
					hittPoint = hitt.point;
				}

			}
			Crosshair2.transform.position = hittPoint;//hitt.point;
			float crosshair2Size = (transform.position - Crosshair2.transform.position).magnitude;
			Crosshair2.transform.localScale = new Vector3(crosshair2Size, crosshair2Size, crosshair2Size);
			//Crosshair2.transform.localScale = new Vector3(CrossHairScale * hitt.distance, CrossHairScale * hitt.distance, CrossHairScale * hitt.distance);


			if (Input.GetKeyDown(PossessionStart)) //DroneSpawner.DS.PossessionStart.Value))
			{
				SosigCheck();
			}

#region Sosig Controls
			if (isControllingSosig)
			{
				//this changes the color of the reticle depending on if you're looking at a friendly or an enemy
				
				//this decides where the sosig's head link ooks
				//ControllingSosig.Links[0].transform.rotation = Quaternion.LookRotation(transform.forward, transform.up);
				ControllingSosig.Links[0].transform.rotation = Quaternion.RotateTowards(ControllingSosig.Links[0].transform.rotation, transform.rotation, 180);
				if (ControllingSosig == null)
				{
					ReleaseSosig();
				}

				if (arrowPointer.activeSelf)
					arrowPointer.SetActive(false);
				// Here we are seeing if the player is using the scroll wheel. That determines the zoom of the camera
				if (Input.mouseScrollDelta.y < 0)
				{
					droneCam.fieldOfView += 5;
				}
				else if (Input.mouseScrollDelta.y > 0)
				{
					droneCam.fieldOfView -= 5;
				}
				droneCam.fieldOfView = Mathf.Clamp(droneCam.fieldOfView, 15, MaxFOV);

				//These are killbinds

				if (Input.GetKeyDown(KeyCode.Keypad0))
				{
					ControllingSosig.Vaporize(ControllingSosig.DamageFX_Vaporize, ControllingSosig.GetIFF());
				}
				if (Input.GetKeyDown(KeyCode.Keypad1))
				{
					ControllingSosig.Links[0].LinkExplodes(Damage.DamageClass.Abstract);

				}
				if (Input.GetKeyDown(KeyCode.Keypad2))
				{
					ControllingSosig.Links[0].LinkExplodes(Damage.DamageClass.Abstract);
					ControllingSosig.Links[1].LinkExplodes(Damage.DamageClass.Abstract);
					ControllingSosig.Links[2].LinkExplodes(Damage.DamageClass.Abstract);
					ControllingSosig.Links[3].LinkExplodes(Damage.DamageClass.Abstract);
				}
				if (Input.GetKeyDown(KeyCode.Keypad3))
				{
					ControllingSosig.BreakBack(true);
				}
				//throwing things
				if (Input.GetKeyDown(WeaponThrow))
				{
					ThrowTheCheese();
				}
				if (Input.GetKeyDown(GrenadeThrow))
				{
					NadeOut();
				}
				//ShitPosting Audio 
				if (Input.GetKeyDown(KeyCode.Alpha1))
				{
					ControllingSosig.Speak_State(ControllingSosig.Speech.OnSkirmish);
				}
				if (Input.GetKeyDown(KeyCode.Alpha2))
				{
					ControllingSosig.Speak_State(ControllingSosig.Speech.OnWander);
				}
				if (Input.GetKeyDown(KeyCode.Alpha3))
				{
					ControllingSosig.Speak_State(ControllingSosig.Speech.OnInvestigate);
				}
				if (Input.GetKeyDown(Pickup)) // DroneSpawner.DS.Pickup.Value))
				{
					changeWeapons();
				}
				RaycastHit hit;

				//RELOADING
				if (currentGun != null && Input.GetKeyDown(KeyCode.R) || currentGun!= null && currentGun.m_shotsLeft == 0)
				{
					ReloadCurrentWeapon();
				}
				//idk what I'm doing here but I think I am making sure stuff isn't broken
				if (currentGun != null && currentGun != ControllingSosig.Hand_Primary.HeldObject)
				{
					currentGun = ControllingSosig.Hand_Primary.HeldObject;
					if (currentGun != null)
					{
						SosigWeaponPlayerInterface j = currentGun.GetComponent<SosigWeaponPlayerInterface>();
						if (j != null)
						{
							currentGunInterface = j;
						}
					}					
					
						currentHand = ControllingSosig.Hand_Primary;
					

				}
				//Crosshair stuff
				Vector3 hitPoint = transform.position + transform.forward * 1000f;

				if (currentGun != null && currentGun.Type == SosigWeapon.SosigWeaponType.Gun)
				{

					if (Physics.Raycast(currentGun.Muzzle.position, currentGun.Muzzle.transform.forward, out hit, 1000f, environment))
					{
						hitPoint = hit.point;
					}
				}
				else
				{
					if (Physics.Raycast(transform.position, transform.forward, out hit, 1000f, environment))
					{
						hitPoint = hit.point;
					}
				}

				Crosshair.transform.position = hit.point;
				//Setup what gun the player can use
				if (ControllingSosig.Hand_Primary.IsHoldingObject && currentGun == null)
				{
					currentGun = ControllingSosig.Hand_Primary.HeldObject;
					SosigWeaponPlayerInterface j = currentGun.GetComponent<SosigWeaponPlayerInterface>();
					if (j != null)
					{
						currentGunInterface = j;
					}
					currentHand = currentGun.HandHoldingThis;

				}

				/*if (Input.GetKey(KeyCode.Mouse0))
				{
					CancelReload();
					if (currentGun != null && currentGun.Type == SosigWeapon.SosigWeaponType.Gun)
					{
						if (currentGun.isFullAuto)
						{
							currentGun.FireGun(1);
						}
						if (!currentGun.isFullAuto && !semiAutoFired)
						{
							currentGun.FireGun(1);
							semiAutoFired = true;
						}

					}

				}*/
				//SEMI AUTO STUFF
				

				//setting melee stuff
				if (currentGun != null && currentGun.Type == SosigWeapon.SosigWeaponType.Melee && currentHand.m_posedToward != currentHand.Point_ShieldHold)
				{
					currentHand.PoseToward(currentHand.Point_ShieldHold);
				}
				if (Input.GetKey(KeyCode.Mouse0))
				{
					if (currentGun != null && currentGun.Type == SosigWeapon.SosigWeaponType.Melee)
					{
						currentGun.StartNewAttackAnim();// (MeleePoint.position);
					}
				}
				if (Input.GetKey(KeyCode.Mouse1)) //Sets aiming parameters
				{
					if (currentHand != null && currentGun != null)
					{
						currentHand.PoseToward(currentHand.Point_Aimed);
						currentGun.transform.LookAt(AimTorwardsPoint.position);						

					} //else if (currentGun != null)
                    //{
						//currentGun.Muzzle.transform.rotation = Quaternion.identity;	
                    //}

				}
				else if (Input.GetKeyUp(KeyCode.Mouse1) && currentHand != null)
				{
					currentHand.PoseToward(currentHand.Point_HipFire);
				}

				if (Input.GetKey(KeyCode.LeftShift)) //sets sprinting parameters
				{
					ControllingSosig.m_assaultSpeed = Sosig.SosigMoveSpeed.Running;
					ControllingSosig.Speed_Walk = ControllingSosig.Speed_Run;
				}
				else if (ControllingSosig.Speed_Walk != storedwalkingspeed)
				{
					ControllingSosig.Speed_Walk = storedwalkingspeed;
				}
				if (Input.GetKeyDown(PossessionStop) || ControllingSosig.BodyState == Sosig.SosigBodyState.Dead) //releases the sosig you're controlling
				{
					ReleaseSosig();
				}

				if (ControllingSosig != null && Input.GetAxis("Horizontal") > 0 || ControllingSosig != null && Input.GetAxis("Horizontal") < 0 && ControllingSosig.BodyState != Sosig.SosigBodyState.Dead) //makes sure that the sosig doesn't have a seizure
				{
					ControllingSosig.SetNewWeightedLookDir(lookdirSpeed, transform.forward);
				}



				if (ControllingSosig != null && Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D))
				{
					ControllingSosig.CommandPathTo(new List<Transform> { actualMovePoint }, 0, new Vector2(0, 0), 0.2f, Sosig.SosigMoveSpeed.Walking, Sosig.PathLoopType.Once, null, 0, 0, false, 0);
				}
				else if (ControllingSosig != null)
				{
					ControllingSosig.CommandGuardPoint(ControllingSosig.transform.position, true);
				}


			}
#endregion
#region Drone Controls
			if (!isControllingSosig && !isControllingObject)
			{
				if (ControllingParticleSystem.activeSelf == true)
				{
					ControllingParticleSystem.SetActive(false);

				}
				if (!arrowPointer.activeSelf)
					arrowPointer.SetActive(true);
				arrowPointer.transform.LookAt(GM.CurrentPlayerBody.transform.position);

				
				Vector3 velocityVector = new Vector3(0,0,0);
				if (Input.GetKey(KeyCode.LeftShift))
                {
					CameraSpeed = fastCameraSpeed;
                } else
                {
					CameraSpeed = cameraSpeed;
                }
				if (Input.GetKey(KeyCode.W))
				{
					velocityVector += transform.forward * CameraSpeed;					
				}

				if (Input.GetKey(KeyCode.S))
				{
					velocityVector += -transform.forward * CameraSpeed;
					
				}
				if (Input.GetKey(KeyCode.A))
				{
					velocityVector += -transform.right * CameraSpeed;
					
				}
				if (Input.GetKey(KeyCode.D))
				{
					velocityVector += transform.right * CameraSpeed;
					
				}
				if (Input.GetKey(KeyCode.Space))
				{
					velocityVector += Vector3.up * CameraSpeed;
				}

				if (Input.GetKey(KeyCode.LeftControl))
				{
					velocityVector +=  Vector3.down * CameraSpeed;					
				}
				CameraRigidbody.velocity = velocityVector;
				/*if (!Input.GetKey(KeyCode.D) && !Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.S) && !Input.GetKey(KeyCode.W))
				{
					inMotion = false;
				}*/
				
				

				/*//THIS IS THE ORIGINAL CODE FOR MOVEMENT
				if (!Input.GetKey(KeyCode.D) && !Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.S) && !Input.GetKey(KeyCode.W))
				{
					inMotion = false;
				}
				if (CameraRigidbody.velocity.z > cameraSpeed)
                {

                }
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
				}*/

			}
#endregion
			//OBJECT CONTROLLING UPDATE STUFF
#region Object Controls
			if (isControllingObject && DroneSpawner.DS.CrazyMode.Value)
			{

				if (Input.GetKey(KeyCode.Mouse1))
				{
					currentObject.transform.LookAt(Crosshair2.transform.position);
				}
				if (arrowPointer.activeSelf)
					arrowPointer.SetActive(false);
				if (Input.mouseScrollDelta.y < 0)
				{
					distanceToObject += .1f;
				}
				else if (Input.mouseScrollDelta.y > 0)
				{
					distanceToObject -= .1f;
				}
				distanceToObject = Mathf.Clamp(distanceToObject, 0, 100);
				//Movement of the object
				if (CRB != null)
				{
					//WACKY PHYSICAL OBJECT STUFF
					if (Input.GetKeyDown(KeyCode.Mouse0) && currentObject != null)
					{
						//primes a grenade
						PinnedGrenade G = currentObject.GetComponent<PinnedGrenade>();
						if (G != null && !G.m_isPinPulled)
						{
							foreach (PinnedGrenadeRing ring in G.m_rings)
								ring.ForcePopOut();
							if (!G.IsLeverReleased())
							{
								G.ReleaseLever();
							}
						}
						//Shoots a cartridge
						FVRFireArmRound Round = currentObject.GetComponent<FVRFireArmRound>();
						if (Round != null)
						{
							Round.Fire();
						}
						SosigWeapon SW = currentObject.GetComponent<SosigWeapon>();
						if (SW != null)
						{

							SW.FireGun(1);
						}
						OpenBoltReceiver OBR = currentObject.GetComponent<OpenBoltReceiver>();
						if (OBR != null)
						{							
							OBR.Fire();
						}
						ClosedBoltWeapon CBW = currentObject.GetComponent<ClosedBoltWeapon>();
						if (CBW != null)
						{
							CBW.Fire();
						}
						Handgun HG = currentObject.GetComponent<Handgun>();
						if (HG != null)
						{
							HG.Fire();
						}
						LeverActionFirearm LV = currentObject.GetComponent<LeverActionFirearm>();
						if (LV != null)
						{
							LV.Fire();
						}
						BoltActionRifle BR = currentObject.GetComponent<BoltActionRifle>();
						if (BR != null)
						{
							BR.Fire();
						}
						Revolver R = currentObject.GetComponent<Revolver>();
						if (R != null)
						{
							R.Fire();
						}
						RevolvingShotgun RS = currentObject.GetComponent<RevolvingShotgun>();
						if (RS != null)
						{
							RS.Fire();
						}
						SingleActionRevolver SAR = currentObject.GetComponent<SingleActionRevolver>();
						if (SAR != null)
						{
							SAR.Fire();
						}
						TubeFedShotgun TBS = currentObject.GetComponent<TubeFedShotgun>();
						if (TBS != null)
						{
							TBS.Fire();
						}
						Flaregun FG = currentObject.GetComponent<Flaregun>();
						if (FG != null)
						{
							FG.Fire();
						}




					}
					
					if (Input.GetKey(KeyCode.W))
					{
						Vector3 desiredDirection = Vector3.Normalize(new Vector3(transform.forward.x, 0, transform.forward.z));


						CRB.AddForce(desiredDirection * objectForce, ForceMode.VelocityChange);

						//CRB.AddForce(transform.forward.x, 0, transform.forward.z * objectForce, ForceMode.Force);
					}

					if (Input.GetKey(KeyCode.S))
					{

						Vector3 desiredDirection = Vector3.Normalize(new Vector3(transform.forward.x, 0, transform.forward.z));

						CRB.AddForce(-desiredDirection * objectForce, ForceMode.VelocityChange);
					}
					if (Input.GetKey(KeyCode.A))
					{

						CRB.AddForce(-transform.right * objectForce, ForceMode.VelocityChange);
					}
					if (Input.GetKey(KeyCode.D))
					{
						//if (CRB.velocity.magnitude <= maxOBJSpeed)
						CRB.AddForce(transform.right * objectForce, ForceMode.VelocityChange);
					}
					if (Input.GetKeyDown(KeyCode.Space))
					{
						CRB.AddForce(Vector3.up * objectJump, ForceMode.VelocityChange);
					}



				}
				//Camera stuff for object control
				if (currentObject != null)
				{
					Vector3 newPos = new Vector3(currentObject.transform.position.x, currentObject.transform.position.y + distanceToObject, currentObject.transform.position.z);
					transform.position = newPos;

				}
				if (Input.GetKeyDown(PossessionStop)) //releases the object you're controlling
				{
					ReleaseObject();
				}
				if (currentObject == null)
				{
					ReleaseObject();
				}
			}
#endregion
			rotation.y += Input.GetAxis("Horizontal") * sensitivity;
			Cursor.lockState = CursorLockMode.Locked;
			rotation.x += -Input.GetAxis("Vertical") * sensitivity;
			rotation.x = Mathf.Clamp(rotation.x, -ClampRange, ClampRange);
			transform.eulerAngles = rotation * speed;

		}
		void LateUpdate()
        {
			MFHolder.transform.position =transform.position;
			MFHolder.transform.rotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(transform.forward, Vector3.up));
			UIUpdate();
			//Reticle color changing code
			ReticleMaterial.color = neutral;
			RaycastHit iffCheck;
			if (Physics.Raycast(transform.position, transform.forward, out iffCheck, Mathf.Infinity, iffCheckMask))
			{
				FVRPlayerBody player = iffCheck.collider.gameObject.GetComponent<FVRPlayerBody>();
				Sosig hitSosig = iffCheck.collider.gameObject.GetComponentInParent<SosigLink>().S;
				if (player != null)
				{
					IFFShow(GM.CurrentPlayerBody.GetPlayerIFF());
				}
				else if (hitSosig != null)
				{
					IFFShow(hitSosig.GetIFF());
				}
			}
			#region LU Sosig stuff
			if (ControllingSosig)
			{
				
				if (Input.GetKey(KeyCode.Mouse0))
				{
					if (isReloading && currentGun != null && currentGun.m_shotsLeft > 0)
					{
						CancelReload();
					}
					if (currentGun != null && currentGun.Type == SosigWeapon.SosigWeaponType.Gun)
					{
						
						if (currentGun.isFullAuto)
						{
							currentGun.FireGun(1);
						}
						if (!currentGun.isFullAuto && !semiAutoFired)
						{
							currentGun.FireGun(1);
							semiAutoFired = true;
						}

					}

				}
				if (Input.GetKeyUp(KeyCode.Mouse0) && semiAutoFired)
				{
					semiAutoFired = false;
				}
			}
			#endregion
			
        }

        public void SosigCheck()
		{
			RaycastHit hit;
			if (Physics.Raycast(transform.position, transform.forward, out hit, 100, mask))
			{
				SosigLink L = hit.collider.gameObject.GetComponent<SosigLink>();
				if (L != null && !isControllingObject)
				{


					if (L.S.BodyState != Sosig.SosigBodyState.Dead)
					{
						ReleaseSosig();
						ControllingSosig = L.S;
					}
					Debug.Log("Attempting");
					if (ControllingSosig != null)
						SetupSosigCamera(ControllingSosig);
				}
				else
				{
					if (!ControllingSosig && DroneSpawner.DS.CrazyMode.Value)
					{
						setupObject();
					}
				}
			}
			else
			{
				Debug.Log("nope");
			}

		}
		public void SetupSosigCamera(Sosig S)
		{
			// add new shit at the end boi
			ControllingParticleSystem.SetActive(true);
			transform.localPosition = new Vector3(0, 0, 0);
			isControllingSosig = true;
			CameraRigidbody.isKinematic = true;
			S.IsAllowedToSpeak = false;
			storedwalkingspeed = S.Speed_Walk;
			storedSightRange = S.MaxSightRange;
			S.MaxSightRange = 0;
			storedHearingRange = S.MaxHearingRange;
			S.MaxHearingRange = 0;
			ControllingSosig.Priority.m_targetPoint = Crosshair.transform.position;
			ControllingSosig.m_randomLookTickRange = new Vector2(9999999, 9999999);
			if (ControllingSosig.Hand_Primary.HeldObject != null)
			{
				sosigWeps.Add(ControllingSosig.Hand_Primary.HeldObject);
			}
			if (ControllingSosig.Hand_Secondary.HeldObject != null)
			{
				sosigWeps.Add(ControllingSosig.Hand_Secondary.HeldObject);
			}
			for (int i = 0; i < ControllingSosig.Inventory.Slots.Count; i++)
			{
				if (ControllingSosig.Inventory.Slots[i].HeldObject != null)
				{
					sosigWeps.Add(ControllingSosig.Inventory.Slots[i].HeldObject);
				}
			}
			ControllingSosig.CanPickup_Melee = false;
			ControllingSosig.CanPickup_Ranged = false;
			ControllingSosig.CanPickup_Other = false;
			ControllingSosig.m_tickDownToRandomLook = 100000;
			if (ControllingSosig.Links[0].m_wearables.Count > 0)
			{
				foreach (SosigWearable wearba in ControllingSosig.Links[0].m_wearables)
				{			
					wearba.gameObject.layer = 11;
					Transform[] ExtraItems = wearba.gameObject.GetComponentsInChildren<Transform>();
					for (int i = 0; i <= ExtraItems.Length - 1; i++)
					{
						GameObject jeff = ExtraItems[i].gameObject;
						if (jeff != null)
						{
							jeff.layer = 11;
						}
					}

				}
			}
			ControllingSosig.Links[0].gameObject.GetComponentInChildren<MeshRenderer>().gameObject.layer = 11;
			sosigBodyFollower = new GameObject().transform;
			sosigBodyFollower.SetParent(ControllingSosig.Links[1].transform);
			sosigBodyFollower.localPosition = new Vector3(0, .55f, 0);
			SetupHealthMeter();
			StoredSosigHeadAngularDrag = ControllingSosig.Links[0].GetComponent<Rigidbody>().angularDrag;
			ControllingSosig.Links[0].GetComponent<Rigidbody>().angularDrag = 100;
			//this is the code for changing the inventory slots and aim point of the sosigs. It parents the aim point to the head link so it can rotate with it
			storedSosigAimPoint = ControllingSosig.Hand_Primary.Point_Aimed.transform.localPosition;
			if (!IsLeftie)
			{
				foreach (SosigHand hand in ControllingSosig.Hands)
				{
					
					hand.Point_Aimed.transform.localPosition = new Vector3(-hand.Point_Aimed.transform.localPosition.x, hand.Point_Aimed.transform.localPosition.y + .17f, hand.Point_Aimed.transform.localPosition.z);
					hand.Point_HipFire.transform.localPosition = new Vector3(-hand.Point_HipFire.transform.localPosition.x, hand.Point_HipFire.transform.localPosition.y, hand.Point_HipFire.transform.localPosition.z);
					hand.Point_AtRest.transform.localPosition = new Vector3(-hand.Point_AtRest.transform.localPosition.x, hand.Point_AtRest.transform.localPosition.y, hand.Point_AtRest.transform.localPosition.z);
					hand.Point_ShieldHold.transform.localPosition = new Vector3(-hand.Point_ShieldHold.transform.localPosition.x, hand.Point_ShieldHold.transform.localPosition.y, hand.Point_ShieldHold.transform.localPosition.z);

				}
				foreach(SosigInventory.Slot inven in ControllingSosig.Inventory.Slots)
                {
					inven.Target.transform.localPosition = new Vector3(-inven.Target.transform.localPosition.x, inven.Target.transform.localPosition.y, inven.Target.transform.localPosition.z);
                }
			}
			else
			{
				foreach (SosigHand hand in ControllingSosig.Hands)
				{
					hand.Point_Aimed.transform.localPosition = new Vector3(-hand.Point_Aimed.transform.localPosition.x, hand.Point_Aimed.transform.localPosition.y + .17f, hand.Point_Aimed.transform.localPosition.z);

				}
			}
            ControllingSosig.Hand_Primary.Point_Aimed.transform.SetParent(ControllingSosig.Links[0].transform);
			sosigIFF = ControllingSosig.GetIFF();
		}
		public void ReleaseSosig()
		{
			//transform.SetParent(null, true);
			CancelReload();
			droneCam.fieldOfView = MaxFOV;
			isControllingSosig = false;
			CameraRigidbody.isKinematic = false;
			currentGun = null;
			currentGunInterface = null;
			currentHand = null;
			ControllingParticleSystem.SetActive(false);
			if (ControllingSosig != null)
			{//Add new shit at the beginning of this
                ControllingSosig.Hand_Primary.Point_Aimed.transform.SetParent(ControllingSosig.Links[1].transform.Find("HandRoot").transform);
				if (!IsLeftie)
				{
					foreach (SosigHand hand in ControllingSosig.Hands)
					{
						hand.Point_Aimed.transform.localPosition = new Vector3(-hand.Point_Aimed.transform.localPosition.x, hand.Point_Aimed.transform.localPosition.y- .17f, hand.Point_Aimed.transform.localPosition.z);
						hand.Point_HipFire.transform.localPosition = new Vector3(-hand.Point_HipFire.transform.localPosition.x, hand.Point_HipFire.transform.localPosition.y, hand.Point_HipFire.transform.localPosition.z);
						hand.Point_AtRest.transform.localPosition = new Vector3(-hand.Point_AtRest.transform.localPosition.x, hand.Point_AtRest.transform.localPosition.y, hand.Point_AtRest.transform.localPosition.z);
						hand.Point_ShieldHold.transform.localPosition = new Vector3(-hand.Point_ShieldHold.transform.localPosition.x, hand.Point_ShieldHold.transform.localPosition.y, hand.Point_ShieldHold.transform.localPosition.z);
					}
					foreach (SosigInventory.Slot inven in ControllingSosig.Inventory.Slots)
					{
						inven.Target.transform.localPosition = new Vector3(-inven.Target.transform.localPosition.x, inven.Target.transform.localPosition.y, inven.Target.transform.localPosition.z);
					}
				}
				else
                {
					foreach (SosigHand hand in ControllingSosig.Hands)
					{
						hand.Point_Aimed.transform.localPosition = new Vector3(hand.Point_Aimed.transform.localPosition.x, hand.Point_Aimed.transform.localPosition.y - .17f, hand.Point_Aimed.transform.localPosition.z);
					}
				}
				ControllingSosig.Hand_Primary.Point_Aimed.transform.localPosition = storedSosigAimPoint;
				ControllingSosig.Links[0].GetComponent<Rigidbody>().angularDrag = StoredSosigHeadAngularDrag;
				ControllingSosig.IsAllowedToSpeak = true;
				ControllingSosig.Links[0].gameObject.GetComponentInChildren<MeshRenderer>().gameObject.layer = 20;
				ControllingSosig.MaxHearingRange = storedHearingRange;
				ControllingSosig.MaxSightRange = storedSightRange;
				ControllingSosig.CanPickup_Melee = true;
				ControllingSosig.CanPickup_Ranged = true;
				ControllingSosig.CanPickup_Other = true;
				ControllingSosig.m_tickDownToRandomLook = 20;
				sosigWeps.Clear();
				if (ControllingSosig.Links[0].m_wearables.Count > 0)
				{
					foreach (SosigWearable wearba in ControllingSosig.Links[0].m_wearables)
					{
						wearba.gameObject.layer = 0;
					}
				}
				ControllingSosig = null;
			}

		}
		public void NadeOut()
		{
			if (sosigWeps != null)
			{
				if (currentGun != null && currentGun.Type == SosigWeapon.SosigWeaponType.Grenade)
				{
					ThrowTheCheese();
				}
				else
				{
					foreach (SosigWeapon wep in sosigWeps)
					{
						if (wep.Type == SosigWeapon.SosigWeaponType.Grenade && wep != currentGun)
						{
							ControllingSosig.Inventory.SwapObjectFromHandToObjectInInventory(currentGun, wep);
							currentGun = wep;
							SosigWeaponPlayerInterface j = currentGun.GetComponent<SosigWeaponPlayerInterface>();
							if (j != null)
							{
								currentGunInterface = j;
							}
							ThrowTheCheese();
							break;
						}
					}
				}
			}
		}
		public void ThrowTheCheese()
		{
			if (currentGun != null && sosigWeps != null && currentGun.Type != SosigWeapon.SosigWeaponType.Melee)
			{
				foreach (SosigWeapon wep in sosigWeps)
				{
					if (wep.Type == SosigWeapon.SosigWeaponType.Melee && wep != currentGun)
					{
						if (currentGun != null)
						{
							ControllingSosig.Inventory.SwapObjectFromHandToObjectInInventory(currentGun, wep);
							currentGun = wep;
							SosigWeaponPlayerInterface j = currentGun.GetComponent<SosigWeaponPlayerInterface>();
							if (j != null)
							{
								currentGunInterface = j;
							}
						}
						else
						{
							ControllingSosig.ForceEquip(wep);
							currentGun = wep;
							SosigWeaponPlayerInterface j = currentGun.GetComponent<SosigWeaponPlayerInterface>();
							if (j != null)
							{
								currentGunInterface = j;
							}
						}
						break;
					}
				}
			}

			if (currentGun != null)
			{
				Vector3 targetPoint = Crosshair2.transform.position;
				Vector3 vector3_1 = targetPoint - currentGun.transform.position;
				Vector3 s0 = vector3_1.normalized * throwForce;
				Vector3 s1;
				if (currentGun.Pin != null)
					currentGun.Pin.ForceExpelPin();
				currentHand.ThrowObject(Vector3.ClampMagnitude(s0 + Random.onUnitSphere, Mathf.Max(s0.magnitude - 3f, 2f)) * 1f, Crosshair2.transform.position);
				sosigWeps.Remove(currentGun);
				currentGun = null;
				currentGunInterface = null;
			}
		}
		public void changeWeapons()
		{
			RaycastHit hit;
			SosigWeapon swappingWep = null;
			if (currentHand == null && ControllingSosig != null) 
            {
				currentHand = ControllingSosig.Hand_Primary;
				Debug.Log("the grab fix fired");
            }
			if (Physics.Raycast(transform.position, transform.forward, out hit, 3f, GunPickup))
			{
				swappingWep = hit.collider.gameObject.GetComponentInParent<SosigWeapon>();
				//Debug.Log(hit.transform.parent.gameObject.name);
			}
			if (currentGun != null && swappingWep != null && !swappingWep.IsHeldByBot && !swappingWep.IsInBotInventory)
			{
				sosigWeps.Remove(currentGun);
				currentHand.DropHeldObject();
				currentHand.PickUp(swappingWep);
				currentGun = swappingWep;
				SosigWeaponPlayerInterface j = currentGun.GetComponent<SosigWeaponPlayerInterface>();
				if (j != null)
				{
					currentGunInterface = j;
				}
			}
			else if (currentGun == null && swappingWep != null && !swappingWep.IsHeldByBot && !swappingWep.IsInBotInventory)
			{

				currentHand.PickUp(swappingWep);
				currentGun = swappingWep;
				SosigWeaponPlayerInterface j = currentGun.GetComponent<SosigWeaponPlayerInterface>();
				if (j != null)
				{
					currentGunInterface = j;
				}
			}


		}
		public void setupObject()
		{
			RaycastHit hit;
			if (Physics.Raycast(transform.position, transform.forward, out hit, 100, mask))
			{
				FVRPhysicalObject o = hit.collider.gameObject.GetComponentInParent<FVRPhysicalObject>();
				if (o != null)
				{
					isControllingObject = true;
					currentObject = o;
					CRB = o.GetComponent<Rigidbody>();

				}
			}

		}
		public void ReleaseObject()
		{
			isControllingObject = false;
			currentObject = null;
			CRB = null;
			if (currentObject != null)
			{
			}
		}
		public void UIUpdate()
		{
			//this is the setup for the ammo counter
			if (currentGun != null)
			{
				string ammocount = currentGun.GetShotsLeft().ToString() + "/" + currentGun.ShotsPerLoad;
				AmmoCounter.text = ammocount;
				GunName.text = currentGunInterface.ObjectWrapper.DisplayName;
			}
			else if (currentGun == null)
			{
				string ammocount = "0/0";
				AmmoCounter.text = ammocount;
				GunName.text = "";
			}
			if (ControllingSosig != null)
			{
				sosigHealthMeterUpdate();
			}
		}
		public void ReloadCurrentWeapon()
		{
			if (!isReloading && currentGun != null && currentGun.Type == SosigWeapon.SosigWeaponType.Gun)
			{
				StartCoroutine("reloadingGun");
			}
		}
		IEnumerator reloadingGun()
		{
			
			if (currentGun != null && currentGun.Type == SosigWeapon.SosigWeaponType.Gun)
			{
				isReloading = true;
				IEnumerator reloadMeterCoroutine = reloadMeter(currentGun.ReloadTime);
				StartCoroutine(reloadMeterCoroutine);
				yield return new WaitForSeconds(currentGun.ReloadTime);
				isReloading = false;
				//currentGun.InstaReload();
				currentGun.m_shotsLeft = currentGun.ShotsPerLoad;
			} else
            {
				CancelReload();				
			}

		}
		//this is to make the reloading icon turn while you're reloading
		IEnumerator reloadMeter(float ReloadTime)
		{
			reloadingImage.gameObject.SetActive(true);
			float ReloadTimeLeft = 0;
			float multiplier = 1 / ReloadTime;
			while (ReloadTimeLeft < ReloadTime)
			{
				if (currentGun != null)
				{
					ReloadTimeLeft += Time.deltaTime;
					reloadingImage.fillAmount = ReloadTimeLeft * multiplier;
					yield return null;
				}
				else
                {
					ReloadTimeLeft = ReloadTime;
					reloadingImage.fillAmount = 0;
					yield return null;
                }

			}
			reloadingImage.gameObject.SetActive(false);
			yield break;

		}
		//stops all reloading coroutines and deactivates teh reloading meter gameobject
		public void CancelReload()
		{
			StopCoroutine("reloadingGun");
			StopCoroutine("reloadMeter");
			reloadingImage.gameObject.SetActive(false);
		}

		//the health meter update sets all the specifics for what the health meter looks like.
		//This is not what is called to show the health meter
		public void sosigHealthMeterUpdate()
		{
			bool hasChanged = false;
			for (int i = 0; i <= 3; i++)
			{
				//Debug.Log("first if clause" + i);				
				if (LinkIntegrityLastFrame[i] != Mathf.Floor(ControllingSosig.Links[i].m_integrity))
				{
					LinkIntegrityLastFrame[i] = Mathf.Floor(ControllingSosig.Links[i].m_integrity);
					hasChanged = true;
				}
			}
			if (MustardAmountLastFrame != ControllingSosig.Mustard)
			{
				//Debug.Log("Mustard one");
				hasChanged = true;
				MustardAmountLastFrame = ControllingSosig.Mustard;
			}
			if (hasChanged)
			{
				//Debug.Log("HasChanged = true I geuss");
				for (int i = 0; i <= 3; i++)
				{
					//Debug.Log("2nd if clause " + i);
					LinkImages[i].color = HealthGradient.Evaluate(LinkIntegrityLastFrame[i]/100);
					//Debug.Log("Link Integrity of link " + i + " is " + ( 1 / 100 * (100-LinkIntegrityLastFrame[i])));
				}
				for (int i = 0; i <= 3; i++)
				{
					//Debug.Log("3rd if clause " + i);					
					LinkTexts[i].text = Mathf.Floor(ControllingSosig.Links[i].m_integrity).ToString();
				}
				LinkTexts[4].text = "Mustard: " + Mathf.Floor(ControllingSosig.Mustard).ToString();
				LinkTexts[4].color = HealthGradient.Evaluate(ControllingSosig.Mustard/100);
				showHealthTime = healthShowTime;
				StopCoroutine("ShowCurrentHealth");
				StartCoroutine("ShowCurrentHealth");
			}

		}
		public void SetupHealthMeter()
		{
			for (int i = 0; i <= 3; i++)
			{
				LinkIntegrityLastFrame[i] = ControllingSosig.Links[i].m_integrity;				
			}
			MustardAmountLastFrame = ControllingSosig.Mustard;
		}
		//this CoRoutine will show the health for a few seconds then fade it out over one second
		IEnumerator ShowCurrentHealth()
		{
			float timeSinceStart = 0;
			while(showHealthTime > 0) 
			{ 
					showHealthTime -= Time.deltaTime + 0.01f;
					timeSinceStart += Time.deltaTime + 0.01f;
			
				//Debug.Log(showHealthTime);
				if (ControllingSosig != null)
				{
					for (int i = 0; i <= 3; i++)
					{
						LinkImages[i].color = new Vector4(LinkImages[i].color.r, LinkImages[i].color.g, LinkImages[i].color.b, HealthAlphaKey.Evaluate((1 / healthShowTime) * timeSinceStart).a);
						LinkTexts[i].color = HealthAlphaKey.Evaluate((1 / healthShowTime) * timeSinceStart);
					}
					LinkTexts[4].color = new Vector4(LinkTexts[4].color.r, LinkTexts[4].color.g, LinkTexts[4].color.b, HealthAlphaKey.Evaluate((1 / healthShowTime) * timeSinceStart).a);
				} else
                {
					for (int i = 0; i <= 3; i++)
					{
						LinkImages[i].color = new Vector4(LinkImages[i].color.r, LinkImages[i].color.g, LinkImages[i].color.b, HealthAlphaKey.Evaluate(1).a);
						LinkTexts[i].color = HealthAlphaKey.Evaluate(1);
					}
					LinkTexts[4].color = HealthAlphaKey.Evaluate(1);
				}
				//showHealthTime -= 0.01f;
				//timeSinceStart += 0.01f;
				yield return new WaitForSeconds(.01f);

			}
			/*else if (showHealthTime <= 0)
            {
				showHealthTime = 0;
				yield break;
            }*/
			yield return null;
			

		}
		public void IFFShow(int iff)
        {
			if (iff == sosigIFF)
            {
				ReticleMaterial.color = Friend;
            } else
            {
				ReticleMaterial.color = Enemy;
            }
        }
	}
}