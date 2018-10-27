using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using IllusionPlugin;
using UnityEngine;

namespace JetIslandPowerupsPlugin
{

    //Our Plugin class which implements the IllusionPlugin IPlugin interface.
    public class Plugin : IPlugin
    {
        //Keep track of which level/scene we're in.
        private int _currentLevelId = 0;

        //The Unity prefab we will be loading.
        private GameObject _rocketPowerupPrefab = null;

        //The PowerUp game object we will be creating.
        private GameObject _rocketPowerup = null;

        //Some floats to act as timers.
        private float _powerupDuration = 0f;
        private float _powerupRespawnTime = 0f;

        //Keep track of the players original jet force.
        private float _originalJetForce = 0f;

        //Nice boost to power. :)
        private float _powerupJetForce = 50f;

        //Keep track of the powerup state.
        private bool _powerupActive = false;

        //IllusionPlugin interface - Mod name and version.
        public string Name
        {
            get { return "Powerup Example Plugin"; }
        }
        public string Version
        {
            get { return "1.0"; }
        }

        public void Init()
        {
            //Load our asset bundle.
            AssetBundle assetBundle = AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath, "rocketpower.unity3d"));

            //Load the prefab we created in Unity.
            _rocketPowerupPrefab = assetBundle.LoadAsset<GameObject>("RocketPowerPrefab");

            //Unload the asset bundle without destroying assets from it. Keeps them loaded in the engine for further use.
            assetBundle.Unload(false);

            //Spawn the power up.
            SpawnRocketPowerUp();
        }

        //This isn't really necessary in this case, however it is just to demonstate a way to clean up once the player returns to the menu, etc...
        public void DeInit()
        {
            _rocketPowerup = null;
            _rocketPowerupPrefab = null;
        }

        public void SpawnRocketPowerUp()
        {
            //Check if our object already exists to prevent spawning it multiple times.
            if (_rocketPowerup != null)
                return;

            //Create and position an instance of the prefab we loaded earlier.
            _rocketPowerup = UnityEngine.Object.Instantiate(_rocketPowerupPrefab);

            //This position is right in front of the space ship in the tutorial area.
            _rocketPowerup.transform.position = new Vector3(-85.8f, 121.0f, 9561.8f);

            //Add our MonoBehaviour from PowerUp.cs to the spawned game object, this is basically the same as adding a script to it in the Unity Editor.
            //We have to do it this way because asset bundles can't contain code. (Not entirely true, but this way is easier in my opinion)
            _rocketPowerup.AddComponent<PowerUp>();
        }

        public void PickupPowerup()
        {
            //Destroy our object and set its reference to null;
            UnityEngine.GameObject.DestroyImmediate(_rocketPowerup);
            _rocketPowerup = null;

            //PowerUp will last for 10 seconds.
            _powerupDuration = Time.time + 10f;

            //PowerUp will respawn after 15 seconds;
            _powerupRespawnTime = Time.time + 15f;

            //Save the original jet force so we can set it back later.
            _originalJetForce = PlayerBody.localPlayer.movement.jetForce;

            //Set our new jet force.
            PlayerBody.localPlayer.movement.jetForce = _powerupJetForce;

            //Display a message informing the powerup has been collected.
            PlayerBody.localPlayer.DisplayMessageInFrontOfPlayer("Rocket Power!");

            _powerupActive = true;
        }

        //IllusionPlugin interface method. - "Gets invoked when the application is started."
        public void OnApplicationStart()
        {
        }

        //IllusionPlugin interface method. - "Gets invoked when the application is closed."
        public void OnApplicationQuit()
        {
        }


        //IllusionPlugin interface method. - "Gets invoked whenever a level is loaded."
        public void OnLevelWasLoaded(int level)
        {
            _currentLevelId = level;
        }

        //IllusionPlugin interface method. - "Gets invoked after the first update cycle after a level was loaded."
        public void OnLevelWasInitialized(int level)
        {
            _currentLevelId = level;

            //Level 1 is the main game scene.
            if (_currentLevelId == 1)
            {
                //Only Init if we're single player.
                if(!StartGameScript.playingOnline)
                    Init();
            }
            else
            {
                DeInit();
            }
        }

        //IllusionPlugin interface method. - "Gets invoked on every physics update."
        public void OnFixedUpdate()
        {
        }

        //IllusionPlugin interface method. - "Gets invoked on every graphic update."
        public void OnUpdate()
        {
            if (!StartGameScript.playingOnline)
            {
                if (_currentLevelId == 1)
                {
                    //Make sure our object exists before trying to access it.
                    if(_rocketPowerup != null)
                    {
                        //Instead of messing with collision triggers and what not, we'll just use a simple distance check for the pickup.
                        float distance = Vector3.Distance(PlayerBody.localPlayer.transform.root.position, _rocketPowerup.transform.position);

                        //Units in meters, with a 1:1 VR to real life mapping.
                        if(distance <= 2f)
                        {
                            PickupPowerup();
                        }
                    }

                    //If our powerup duration has run out reset back to the original jet force.
                    if(Time.time >= _powerupDuration && _powerupActive)
                    {
                        PlayerBody.localPlayer.movement.jetForce = _originalJetForce;
                        PlayerBody.localPlayer.DisplayMessageInFrontOfPlayer("Rocket Power Ended");
                        _powerupActive = false;
                    }

                    //Keep setting the powerup jet force incase the game overwrites it.
                    if(_powerupActive)
                        PlayerBody.localPlayer.movement.jetForce = _powerupJetForce;

                    //Check if its time to respawn our power up.
                    if (Time.time >= _powerupRespawnTime && _rocketPowerup == null)
                    {
                        SpawnRocketPowerUp();
                    }

                    //Debug helper to print our position when we squeeze the left trigger.
                    /*
                    if (PlayerBody.localPlayer.input.leftTriggerPulled)
                    {
                        Console.WriteLine(PlayerBody.localPlayer.transform.root.position.ToString());
                    }
                    */
                }
            }
        }
    }
}

