using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FistVR;
using BepInEx;
using System.IO;
using System.Reflection;
using BepInEx.Configuration;
using System.Diagnostics;
using Sodalite.Api;
using Sodalite.ModPanel;
using Sodalite.Utilities;
using System;
namespace Arpy.LAMBDA
{

	[BepInPlugin("h3vr.arpy.LocalMP", "DroneSpawner", "1.0.0")]
	public class DroneSpawner : BaseUnityPlugin {
		private static readonly string BasePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
		public static DroneSpawner DS;
		public GameObject DronePrefab;
		public GameObject sosigDronePrefab;		
		private GameObject spawnedDrone;
		private GameObject spawnedSosigDrone;
		public ConfigEntry<bool> CrazyMode;
		//public ConfigEntry<int> PassCode1;
		//public ConfigEntry<int> PassCode2;
		//public ConfigEntry<int> PassCode3;
		public ConfigEntry<bool> SouthPawToggle;
		public ConfigEntry<bool> MotionSicknessToggle;
		public ConfigEntry<float> Sensitivity;
		public ConfigEntry<KeyCode> DroneSpawn;
		public ConfigEntry<KeyCode> PossessionStart;
		public ConfigEntry<KeyCode> PossessionStop;		
		public ConfigEntry<KeyCode> WeaponThrow;
		public ConfigEntry<KeyCode> GrenadeThrow;
		public ConfigEntry<KeyCode> Pickup;
		private LockablePanel _modPanel = null;
		private UniversalModPanel _modPanelComponent;
		private GameObject _modPanelPrefab;
		public static bool BloodBeenGiven = false;
		public GameObject panel;
		public void Awake() {

			/*PassCode1 = Config.Bind("General",
				"Go to Thunderstore and Read the Tutorial",
				1111,
				"READ THE TUTORIAL ON THUNDERSTORE");
			PassCode2 = Config.Bind("General",
				"To enter read the tutorial",
				1111,
				"READ THE TUTORIAL ON THUNDERSTORE");
			PassCode3 = Config.Bind("General",
				"Read the tutorial to enter",
				1111,
				"READ THE TUTORIAL ON THUNDERSTORE");*/
			SouthPawToggle = Config.Bind("General",
				"Southpaw Toggle",
				false,
				"This toggle sets your sosig to be left handed again like all good sosigs (and bad humans) should be");
			MotionSicknessToggle = Config.Bind("General",
				"Motion Sickness Fixer",
				false,
				"This toggle changes the default followpoint from the head of the sosig to the sosig's AI navigation unit thus unlocking your up and down movements from the head");
			Sensitivity = Config.Bind("General",
				"Mouse Sensitivity",
				0.6f,
				"This number changes the sensitivity of the mouse");

			CrazyMode = Config.Bind("General",
				"Crazy Mode",
				false,
				"You're too sane to try this. Only wacky, goofy guys can press this button. Not YOU!!!");
			DroneSpawn = Config.Bind("General",
				"Drone Spawn Button",
				KeyCode.Backslash,
				"Button that spawns the possession drone");
			PossessionStart = Config.Bind("General",
				"Possession Start Button",
				KeyCode.E,
				"Button that starts a posession");
			PossessionStop = Config.Bind("General",
				"Possession End Button",
				KeyCode.Q,
				"Button that ends a posession");
			WeaponThrow = Config.Bind("General",
				"Throw Weapon Button",
				KeyCode.T,
				"Button to throw your weapon");
			GrenadeThrow = Config.Bind("General",
				"Grenade Throw Button",
				KeyCode.G,
				"Button to toss a grenade");
			Pickup = Config.Bind("General",
				"Weapon Pickup Button",
				KeyCode.F,
				"Button to pickup a weapon");
			var bundle = AssetBundle.LoadFromFile(Path.Combine(BasePath, "localMP"));
			if (bundle)
			{
				//DronePrefab = bundle.LoadAsset<GameObject>("SpectatingCameraTest");
				sosigDronePrefab = bundle.LoadAsset<GameObject>("SosigDroneTest");
				_modPanelPrefab = bundle.LoadAsset<GameObject>("PanelForAgreement");

				// Make a new LockablePanel for the console panel
				_modPanel = new LockablePanel();
				_modPanel.Configure += ConfigureModPanel;
				//_modPanel.TextureOverride = SodaliteUtils.LoadTextureFromBytes(Assembly.GetExecutingAssembly().GetResource("LogPanel.png"));
				//WristMenuAPI.Buttons.Add(new WristMenuButton("Spawn Mod Panel", int.MaxValue, SpawnModPanel));
			}
			DS = this;
			
		}
		void Start()
        {
			//we spawn the panel on start no matter what
				SignInBlood();
			
		}
		
		public void updateBlood (bool b) // This is called when the user accepts the disclaimer
        {
			BloodBeenGiven = b;
			//UnityEngine.Debug.Log(BloodBeenGiven);
        }
		void Update()
		{
			//if (Input.GetKeyDown(KeyCode.Keypad8) && spawnedSosigDrone != null)
           // {
			//	Destroy(spawnedSosigDrone);
            //}
			if (BloodBeenGiven && panel != null)
            {
				FVRPhysicalObject pnl = panel.gameObject.GetComponent<FVRPhysicalObject>();
				pnl.m_hand.EndInteractionIfHeld(pnl);
				pnl.ForceBreakInteraction();
				Destroy(panel);
					
            }

			/*if (Input.GetKeyDown(DroneSpawn.Value) && !FindParsec())  //Here we check to make sure that parsec is not running. if it's not spawn the drone
			{
				//UnityEngine.Debug.Log("trying to initialize");
				UnityEngine.Debug.Log("Parsec not found spawning Drone");
				Destroy(spawnedSosigDrone);
				spawnedSosigDrone = Instantiate(sosigDronePrefab, GM.CurrentPlayerBody.Head.position, Quaternion.identity);

			}*/
			if (Input.GetKeyDown(DroneSpawn.Value) && BloodBeenGiven) // Checks if the disclaimer has been signed
			{
				UnityEngine.Debug.Log(" Blood has already been given");
				UnityEngine.Debug.Log("trying to initialize");
				Destroy(spawnedSosigDrone);
				spawnedSosigDrone = Instantiate(sosigDronePrefab, GM.CurrentPlayerBody.Head.position, Quaternion.identity);

			} else if (Input.GetKeyDown(DroneSpawn.Value) && !BloodBeenGiven) //if the disclaimer hasn't been signed then force them to sign it
            {
				UnityEngine.Debug.Log("You have not signed in blood");
				SignInBlood();
            }
			if (panel != null)
            {
				FVRPhysicalObject pnl = panel.gameObject.GetComponent<FVRPhysicalObject>();
				if (pnl.m_hand == null)
                {
					GM.CurrentMovementManager.Hands[0].RetrieveObject(pnl);

				}

			}

		}
		private void LoadAssets()
		{

		}
		public bool FindParsec()		// Here we are looking to see if there's an instance of Parsec running on the computer
        {
			string ParsecFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Parsec\\lock";
			if (File.Exists(ParsecFolder))
            {
				try
				{
					File.Open(ParsecFolder, FileMode.Open, FileAccess.Write, FileShare.None).Close();
					return false;
				}
				catch (IOException e)
				{
					return true;
				}
				
            } else
            {
				return false;
            }
			/*System.Diagnostics.Process[] localByName2 = System.Diagnostics.Process.GetProcesses();
			System.Diagnostics.Process[] localByName = System.Diagnostics.Process.GetProcessesByName("parsecd");
			/*foreach (System.Diagnostics.Process p in localByName2)
			{
				UnityEngine.Debug.Log(p);
			}
			if (localByName.Length > 0)
			{
				UnityEngine.Debug.Log("Parsec was found");
				return true;
			}
			else
			{
				UnityEngine.Debug.Log("Parsec was not found");
				return false;
			}*/

			
			
        }		
		public void SignInBlood() // This Method lets begins the process of spawning and configuring the disclaimer panel

        {
			LockablePanel BloodOathPanel = new LockablePanel();
			BloodOathPanel.Configure += ConfigureModPanel;
			SpawnModPanel();

        }
		private void SpawnModPanel() //This spawns the disclaimer panel in your left hand
		{
			BloodOathPen.timeLeft = 15;
			panel = _modPanel.GetOrCreatePanel();
            GM.CurrentMovementManager.Hands[0].RetrieveObject(panel.GetComponent<FVRPhysicalObject>());
		}

		private void ConfigureModPanel(GameObject panel) // This configures the disclaimer panel with the proper UI
		{
			var canvasTransform = panel.transform.Find("OptionsCanvas_0_Main/Canvas");
			_modPanelComponent = Instantiate(_modPanelPrefab, canvasTransform.position, canvasTransform.rotation, canvasTransform.parent).GetComponent<UniversalModPanel>();			
			Destroy(canvasTransform.gameObject);
		}
	}
}
