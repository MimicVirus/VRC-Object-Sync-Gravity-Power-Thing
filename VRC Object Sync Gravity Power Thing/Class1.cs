using MelonLoader;
using UnityEngine;
using VRC.SDKBase;
using VRCSDK2;

[assembly: MelonInfo(typeof(Mod), "Faggot Gravity Gun Thing", "1", "Not Beretta", "https://github.com/cutielovesyou/VRC-Object-Sync-Gravity-Power-Thing")]
[assembly: MelonGame("VRChat", "VRChat")]

public class Mod : MelonMod
{
    private bool Toggle = false;
    private bool LaunchToggle = true;
    private GameObject GrabbedObject;
    private GameObject RaycastPointObject;
    private GameObject SelectSphere;
    private float SelectSphereMultiplier = 5;

    public override void OnApplicationStart()
    {
        MelonLogger.Msg("\n\n===== CONTROLS =====\n\n[Left CTRL + Left SHIFT + `] To activate mod, left click for selecting and hold to do the cool shit.\n[Left CTRL + Left SHIFT + 1] To toggle launch option (move direction after letting go of it)\n- and = to change orb size.\nScroll wheel to drag in and out.\n\n");
    }

    public override void OnUpdate()
    {
        try
        {
            if (SelectSphere == null)
            {
                SelectSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                SelectSphere.layer = 2;
                var SelectSphereCollider = SelectSphere.GetComponent<Collider>();
                SelectSphereCollider.isTrigger = true;

                Material material = new Material(Shader.Find("Standard"));
                material.SetFloat("_Mode", 2f);
                material.SetInt("_SrcBlend", 5);
                material.SetInt("_DstBlend", 10);
                material.SetInt("_ZWrite", 0);
                material.DisableKeyword("_ALPHATEST_ON");
                material.EnableKeyword("_ALPHABLEND_ON");
                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = 3000;
                Color color = new Color(0, 1, 1, .3f);
                material.color = color;
                SelectSphere.GetComponent<Renderer>().material = material;
            }
            if (RaycastPointObject == null)
            {
                RaycastPointObject = new GameObject();
                RaycastPointObject.transform.parent = Camera.current.transform;
            }
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.BackQuote))
            {
                Toggle = !Toggle;
                var MSG = "";
                switch (Toggle) { case true: MSG = $"Mod Enabled."; break; case false: MSG = $"Mod Disabled."; break; }
                MelonLogger.Msg(MSG);
            }
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.Alpha1))
            {
                LaunchToggle = !LaunchToggle;
                var MSG = "";
                switch (LaunchToggle) { case true: MSG = $"Launch Toggle Enabled."; break; case false: MSG = $"Launch Toggle Disabled."; break; }
                MelonLogger.Msg(MSG);
            }
            if (Input.GetKeyDown(KeyCode.Minus))
            {
                if (Toggle)
                {
                    SelectSphereMultiplier *= .5f;
                }
            }
            if (Input.GetKeyDown(KeyCode.Equals))
            {
                if (Toggle)
                {
                    SelectSphereMultiplier *= 1.5f;
                }
            }
            if (Input.GetAxis("Mouse ScrollWheel") > 0f)
            {
                if (Toggle)
                {
                    RaycastPointObject.transform.position += Camera.current.transform.forward * (Vector3.Distance(Camera.current.transform.position, RaycastPointObject.transform.position) / 4);
                }
            }
            if (Input.GetAxis("Mouse ScrollWheel") < 0f)
            {
                if (Toggle)
                {
                    RaycastPointObject.transform.position = Vector3.Lerp(Camera.current.transform.position, RaycastPointObject.transform.position, .9f);
                }
            }
            if (Input.GetMouseButton(0))
            {
                if (Toggle)
                {
                    SelectSphere.GetComponent<Renderer>().enabled = true;
                    if (GrabbedObject == null)
                    {
                        if (Physics.Raycast(Camera.current.transform.position, Camera.current.transform.TransformDirection(Vector3.forward), out var hit, 10000))
                        {
                            SelectSphere.transform.localScale = new Vector3(1 * SelectSphereMultiplier, 1 * SelectSphereMultiplier, 1 * SelectSphereMultiplier);
                            SelectSphere.transform.position = hit.point;
                            GameObject ObjSync = null;
                            foreach (var Sync in Resources.FindObjectsOfTypeAll<VRC.SDK3.Components.VRCObjectSync>())
                            {
                                if (SelectSphere.GetComponent<Collider>().bounds.Contains(Sync.gameObject.transform.position))
                                {
                                    ObjSync = Sync.gameObject;
                                    break;
                                }
                            }
                            foreach (var Sync in Resources.FindObjectsOfTypeAll<VRC_ObjectSync>())
                            {
                                if (SelectSphere.GetComponent<Collider>().bounds.Contains(Sync.gameObject.transform.position))
                                {
                                    ObjSync = Sync.gameObject;
                                    break;
                                }
                            }
                            if (ObjSync != null)
                            {
                                GrabbedObject = ObjSync.gameObject;
                                RaycastPointObject.transform.position = GrabbedObject.transform.position;
                            }
                        }
                    }
                    else
                    {
                        SelectSphere.GetComponent<Renderer>().enabled = false;
                        GrabbedObject.transform.position = Vector3.Lerp(GrabbedObject.transform.position, RaycastPointObject.transform.position, .1f);
                        GrabbedObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
                        GrabbedObject.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
                        if (!Networking.IsOwner(Networking.LocalPlayer, GrabbedObject))
                        {
                            Networking.SetOwner(Networking.LocalPlayer, GrabbedObject);
                        }
                    }
                }
            }
            else
            {
                SelectSphere.GetComponent<Renderer>().enabled = false;
                if (GrabbedObject != null)
                {
                    if (LaunchToggle)
                    {
                        GrabbedObject.GetComponent<Rigidbody>().AddForce((RaycastPointObject.transform.position - GrabbedObject.transform.position) * 15, ForceMode.VelocityChange);
                    }
                }
            }
            GrabbedObject = null;
        }
        catch
        {
        }
    }
}