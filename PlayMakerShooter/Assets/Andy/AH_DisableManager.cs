using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class AH_DisableManager : MonoBehaviour {
    [SerializeField] private vp_FPInput vp_FPInput;

    public void DisablePlayer()
    {

        vp_FPInput.MouseCursorForced = true;
        vp_FPInput.MouseCursorBlocksMouseLook = true;
        
    }

    public void EnablePlayer()
    {

        vp_FPInput.MouseCursorForced = false;
        vp_FPInput.MouseCursorBlocksMouseLook = true;

    }

}
