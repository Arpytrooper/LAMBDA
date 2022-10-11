using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace Arpy.LAMBDA
{
	public class BloodOathPen : MonoBehaviour
	{
		public GameObject initialDisclaimer; //the disclaimer that tells the palyer it will run a check for the Parsec lock file.
		public GameObject actualOath;
		public static float timeLeft = 15;
		public Text timeBox;
		void Update()
        {
			if (timeLeft > 0)
            {
				timeLeft -= Time.deltaTime;
            }
			if (timeLeft < 0)
            {
				timeLeft = 0;
            }
			int ceiling = Mathf.CeilToInt(timeLeft);
			timeBox.text = ceiling.ToString();
        }
		public void RightsRemover()
        {
			if (DroneSpawner.DS.FindParsec())
			{
				initialDisclaimer.SetActive(false);
				actualOath.SetActive(true);
				timeLeft = 15;
			}
			else
			{
				DroneSpawner.DS.updateBlood(true);
			}
        }
		public void Signature()
        {
			if (timeLeft <= 0)
			{
				DroneSpawner.DS.updateBlood(true);
			}

        }

        }
}
