using System;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;

public class JumpPlugin : MonoBehaviour
{
    static JumpPlugin _instance;

    public string userId;

    public bool initialized = false;

    public event EventHandler Initialized;

    private void Start()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            _instance = this;

            DontDestroyOnLoad(gameObject);

            if(!Application.isEditor)
            {
                Application.ExternalCall("jump.start");
            }

        }
    }

    public void Exit()
    {
        Application.ExternalCall("jump.exit");
    }

    public void Init(string message)
    {
        userId = message;
        initialized = true;

        var handler = Initialized;
        if (handler != null)
        {
            var args = new EventArgs();
            handler(this, args);
        }
    }

}
