using MelonLoader;
using System;
using System.Collections.Generic;
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDKBase;
using VRCSDK2;

[assembly: MelonInfo(typeof(Mod), "Faggot Gravity Gun Thing", "1", "Not Beretta", "https://github.com/cutielovesyou/VRC-Object-Sync-Gravity-Power-Thing")]
[assembly: MelonGame("VRChat", "VRChat")]

public class Mod : MelonMod
{
    private bool Toggle = false;
    private bool LaunchToggle = true;
    private bool SmoothToggle = true;
    private bool RotateWithHead = true;
    private List<GameObject> GrabbedObjects = new List<GameObject>();
    private Dictionary<GameObject, GameObject> RaycastPointObjects = new Dictionary<GameObject, GameObject>();
    private GameObject SelectSphere;
    private float SelectSphereMultiplier = 5;

    public override void OnApplicationStart()
    {
        MelonLogger.Msg("\n\n" +
            "===== CONTROLS =====\n\n" +
            "[Return (Enter) + 0] To activate mod, left click for selecting and hold to do the cool stuff.\n" +
            "[Return (Enter) + 1] To toggle launch option (move direction after letting go of it)\n" +
            "[Return (Enter) + 2] To toggle smooth movement.\n" +
            "[Return (Enter) + 3] Emergency clear grabbed objects.\n" +
            "[Return (Enter) + 4] Reset ball size.\n" +
            "[Return (Enter) + 5] To toggle rotation with head.\n" +
            "[Return (Enter) + 6] Max ball size.\n" +
            "[- or =] to change orb size.\n" +
            "Scroll wheel to drag in and out.\n\n");
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
            if (Input.GetKey(KeyCode.Return))
            {
                bool Print = false;
                var MSG = "";
                if (Input.GetKeyDown(KeyCode.Alpha0))
                {
                    Toggle = !Toggle;
                    switch (Toggle) { case true: MSG = $"Mod Enabled."; break; case false: MSG = $"Mod Disabled."; break; }
                    Print = true;
                }
                if (Input.GetKeyDown(KeyCode.Alpha1))
                {
                    LaunchToggle = !LaunchToggle;
                    switch (LaunchToggle) { case true: MSG = $"Launch Toggle Enabled."; break; case false: MSG = $"Launch Toggle Disabled."; break; }
                    Print = true;
                }
                if (Input.GetKeyDown(KeyCode.Alpha2))
                {
                    SmoothToggle = !SmoothToggle;
                    switch (SmoothToggle) { case true: MSG = $"Smooth Toggle Enabled."; break; case false: MSG = $"Smooth Toggle Disabled."; break; }
                    Print = true;
                }
                if (Input.GetKeyDown(KeyCode.Alpha3))
                {
                    GrabbedObjects.Clear();
                    RaycastPointObjects.Clear();
                }
                if (Input.GetKeyDown(KeyCode.Alpha4))
                {
                    SelectSphereMultiplier = 5;
                }
                if (Input.GetKeyDown(KeyCode.Alpha5))
                {
                    RotateWithHead = !RotateWithHead;
                }
                if (Input.GetKeyDown(KeyCode.Alpha6))
                {
                    SelectSphereMultiplier = ushort.MaxValue;
                }
                if (Print)
                {
                    MelonLogger.Msg(MSG);
                }
            }
            if (Toggle)
            {
                if (Input.GetKeyDown(KeyCode.Minus))
                {
                    SelectSphereMultiplier *= .5f;
                }
                if (Input.GetKeyDown(KeyCode.Equals))
                {
                    SelectSphereMultiplier *= 2f;
                }
                if (Input.GetAxis("Mouse ScrollWheel") > 0f)
                {
                    foreach (var RaycastPointObject in RaycastPointObjects)
                    {
                        RaycastPointObject.Value.transform.position += Camera.current.transform.forward * (Vector3.Distance(Camera.current.transform.position, RaycastPointObject.Value.transform.position) / 4);
                    }
                }
                if (Input.GetAxis("Mouse ScrollWheel") < 0f)
                {
                    foreach (var RaycastPointObject in RaycastPointObjects)
                    {
                        RaycastPointObject.Value.transform.position = Vector3.Lerp(Camera.current.transform.position, RaycastPointObject.Value.transform.position, .9f);
                    }
                }
                if (Input.GetMouseButton(0))
                {
                    if (GrabbedObjects.Count == 0)
                    {
                        if (Physics.Raycast(Camera.current.transform.position, Camera.current.transform.forward, out var Hit, short.MaxValue))
                        {
                            SelectSphere.SetActive(true);
                            SelectSphere.transform.localScale = new Vector3(1 * SelectSphereMultiplier, 1 * SelectSphereMultiplier, 1 * SelectSphereMultiplier);
                            SelectSphere.transform.position = Hit.point;
                            foreach(var SyncObj in UnityEngine.Object.FindObjectsOfType<VRCObjectSync>())
                            {
                                if (SelectSphere.GetComponent<Collider>().bounds.Contains(SyncObj.transform.position))
                                {
                                    GrabbedObjects.Add(SyncObj.gameObject);
                                    var RaycastPointObject = new GameObject();
                                    RaycastPointObject.transform.position = SyncObj.transform.position;
                                    RaycastPointObject.transform.rotation = SyncObj.transform.rotation;
                                    RaycastPointObject.transform.parent = Camera.current.transform;
                                    RaycastPointObjects.Add(SyncObj.gameObject, RaycastPointObject);
                                }
                            }
                        }
                        else
                        {
                            SelectSphere.SetActive(false);
                        }
                    }
                    else
                    {
                        SelectSphere.SetActive(false);
                        foreach(var GrabbedObject in RaycastPointObjects)
                        {
                            Networking.SetOwner(Networking.LocalPlayer, GrabbedObject.Key);
                            if (GrabbedObject.Key.GetComponent<Rigidbody>() != null)
                            {
                                GrabbedObject.Key.GetComponent<Rigidbody>().velocity = Vector3.zero;
                                GrabbedObject.Key.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
                            }
                            if (SmoothToggle)
                            {
                                GrabbedObject.Key.transform.position = Vector3.Lerp(GrabbedObject.Key.transform.position, GrabbedObject.Value.transform.position, .1f);
                                if (RotateWithHead)
                                {
                                    GrabbedObject.Key.transform.rotation = Quaternion.Lerp(GrabbedObject.Key.transform.rotation, GrabbedObject.Value.transform.rotation, .1f);
                                }
                            }
                            else
                            {
                                GrabbedObject.Key.transform.position = GrabbedObject.Value.transform.position;
                                if (RotateWithHead)
                                {
                                    GrabbedObject.Key.transform.rotation = GrabbedObject.Value.transform.rotation;
                                }
                            }
                        }
                    }
                }
                else
                {
                    SelectSphere.SetActive(false);
                    foreach (var GrabbedObject in RaycastPointObjects)
                    {
                        if (GrabbedObject.Key.GetComponent<Rigidbody>() != null)
                        {
                            GrabbedObject.Key.GetComponent<Rigidbody>().AddForce((GrabbedObject.Value.transform.position - GrabbedObject.Key.transform.position) * 15, ForceMode.VelocityChange);
                        }
                    }
                    GrabbedObjects.Clear();
                    RaycastPointObjects.Clear();
                }
            }
        }
        catch(Exception ex)
        {
            MelonLogger.Error($"IGNORE THIS ERROR!!! ONLY FOR DEBUGGING THIS MOD. {ex}");
        }
    }
}