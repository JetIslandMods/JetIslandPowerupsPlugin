using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


namespace JetIslandPowerupsPlugin
{

    //This class is just like any script you would attach to a game object in Unity. Notice how it inherits from MonoBehaviour.

    class PowerUp : MonoBehaviour
    {
        private float _yOffset = 0f;

        void Start()
        {

        }

        //Spin to win!
        void Update()
        {
            gameObject.transform.Rotate(new Vector3(0f, 90f * Time.deltaTime, 0f));

            _yOffset = Mathf.Sin(Time.time * 2f) * 0.5f * Time.deltaTime;
            gameObject.transform.position += new Vector3(0f, _yOffset, 0f);
        }
    }
}
