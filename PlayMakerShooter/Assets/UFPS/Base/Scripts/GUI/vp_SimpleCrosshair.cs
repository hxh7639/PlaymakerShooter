/////////////////////////////////////////////////////////////////////////////////
//
//	vp_SimpleCrosshair.cs
//	© Opsive. All Rights Reserved.
//	https://twitter.com/Opsive
//	http://www.opsive.com
//
//	description:	this script is just a stub for your own a way cooler crosshair
//					system. it simply draws a classic FPS crosshair center screen.
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using UnityEngine.UI;

public class vp_SimpleCrosshair : MonoBehaviour
{
    // andy-
    bool isGreen = false;
    bool isRed = false;
    bool isNormal = false;
    private GameObject raycastedObj;


    [Header("Raycast Setting")]
    [SerializeField]
    private float rayLength = 10;
    [SerializeField] private LayerMask layerToCheck;


    [Header("References")]
    [SerializeField] private PlayerVitals playerVitals;
    [SerializeField] private Text itemNameText;
    [SerializeField] private Camera fpsCamera;
    //-andy

    // crosshair texture
    public Texture m_ImageCrosshair = null;

	public bool Hide = false;					// use this if you want to hide the crosshair without disabling it (crosshair needs to be enabled for interaction to work)
	public bool HideOnFirstPersonZoom = true;
	public bool HideOnDeath = true;
	
	protected vp_FPPlayerEventHandler m_Player = null;

    // andy-
    void Update()
    {
        RaycastHit hit;
        Vector3 fwd = fpsCamera.transform.TransformDirection(Vector3.forward);

        Debug.DrawRay(fpsCamera.transform.position, fwd * 50, Color.red);


        if (Physics.Raycast(fpsCamera.transform.position, fwd, out hit, rayLength, layerToCheck.value))
        {
            if (hit.collider.CompareTag("Consumable"))
            {
                CrosshairGreen();
                raycastedObj = hit.collider.gameObject;
                print(raycastedObj);
                //update UI name

                if (Input.GetMouseButton(0))
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
        //-andy
    }

    protected virtual void Awake()
	{
		
		m_Player = GameObject.FindObjectOfType(typeof(vp_FPPlayerEventHandler)) as vp_FPPlayerEventHandler; // cache the player event handler
		
	}
	
	
	/// <summary>
	/// registers this component with the event handler (if any)
	/// </summary>
	protected virtual void OnEnable()
	{

		// allow this monobehaviour to talk to the player event handler
		if (m_Player != null)
			m_Player.Register(this);

	}


	/// <summary>
	/// unregisters this component from the event handler (if any)
	/// </summary>
	protected virtual void OnDisable()
	{

		// unregister this monobehaviour from the player event handler
		if (m_Player != null)
			m_Player.Unregister(this);

	}


	/// <summary>
	/// draws the crosshair texture smack in the middle of the screen
	/// </summary>
	void OnGUI()
	{

		if (m_ImageCrosshair == null)
			return;

		if (Hide)
			return;

		if(HideOnFirstPersonZoom && m_Player.Zoom.Active && m_Player.IsFirstPerson.Get())
			return;

		if(HideOnDeath && m_Player.Dead.Active)
			return;

		GUI.color = new Color(1, 1, 1, 0.8f);
		GUI.DrawTexture(new Rect((Screen.width * 0.5f) - (m_ImageCrosshair.width * 0.5f),
			(Screen.height * 0.5f) - (m_ImageCrosshair.height * 0.5f), m_ImageCrosshair.width,
			m_ImageCrosshair.height), m_ImageCrosshair);
		GUI.color = Color.white;        
	
        CrosshairGreen();

    }

    void CrosshairGreen()
    {
        isGreen = true;
        isRed = false;
        isNormal = false;

    }

    void CrosshairRed()
    {
        isGreen = false;
        isRed = true;
        isNormal = false;
    }

    void CrosshairNormal()
    {
        isGreen = false;
        isRed = false;
        isNormal = true;
    }


    protected virtual Texture OnValue_Crosshair
	{
		get { return m_ImageCrosshair; }
		set { m_ImageCrosshair = value; }
	}
	

}

