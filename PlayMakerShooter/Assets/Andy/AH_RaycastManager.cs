using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AH_RaycastManager : MonoBehaviour {

    private GameObject raycastedObj;
    private vp_SimpleCrosshair vp_SimpleCrosshair;

    [Header("Raycast Setting")]
    [SerializeField] private float rayLength = 10;
    [SerializeField] private LayerMask layerToCheck;

    [Header("References")]
    [SerializeField] private PlayerVitals playerVitals;
    [SerializeField] private Text itemNameText;

    void Update () {
        RaycastHit hit;
        Vector3 fwd = transform.TransformDirection(Vector3.forward);

        if(Physics.Raycast(transform.position,fwd,out hit, rayLength,layerToCheck.value))
        {
            if (hit.collider.CompareTag("Consumable"))
            {
                CrosshairGreen();
                raycastedObj = hit.collider.gameObject;
                //update UI name

                if(Input.GetMouseButton(0))
                {
                    //Object properties
                }
            }
        }
        else
        {
            CrosshairNormal();
            //item name reset
        }
		
	}

    void CrosshairGreen()
    {

    }

    void CrosshairRed()
    {

    }

    void CrosshairNormal()
    {

    }

}
