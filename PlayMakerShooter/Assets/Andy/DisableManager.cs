using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class DisableManager : MonoBehaviour {
    [SerializeField] private vp_FPInput vp_FPInput;
    [SerializeField] private vp_FPCamera vp_FPCamera;

    public void DisablePlayer()
    {
        vp_FPInput.MouseCursorForced = true;
        vp_FPCamera.enabled = false;

    }

    public void EnablePlayer()
    {
        vp_FPInput.MouseCursorForced = false;
        vp_FPCamera.enabled = true;
    }

}
