using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class UISnapToPlayer : MonoBehaviour
{
    [HideInInspector] public GameObject playerHeadCamera;

    [SerializeField] private bool useMainCamera;

    public bool UseMainCamera => useMainCamera;

    private bool _offsetStatus;

    // TODO: for late we can adjust the offset based on if the second panel is open or not!
    public bool OffsetStatus
    {
        get => _offsetStatus;
        set => _offsetStatus = value;
    }

    public float distance = 1.3f;
    public float yOffset = 0f;
    public float yOffsetToHeadTreshold = 0.3f;
    public float angleOffsetClamp = 45f;
    public Quaternion headForwardRotationOffset = default;

    public event Action Enabled;
    public event Action Disabled;

    private Coroutine yRepositionCoroutine = null;
    private Coroutine hRepositionCoroutine = null;
    private Vector3 hCenter = default;

    private void OnEnable()
    {
        if (useMainCamera)
        {
            if (Camera.main != null)
            {
                playerHeadCamera = Camera.main.gameObject;
            }
            else
            {
                Debug.LogError("No main camera found");
            }
        }

        SnapToPosition();
        Enabled?.Invoke();
    }

    private void OnDisable()
    {
        yRepositionCoroutine = null;
        hRepositionCoroutine = null;
        Disabled?.Invoke();
    }

    private void SnapToPosition()
    {
        if (playerHeadCamera == null) return;

        var headF = Vector3.ProjectOnPlane(headForwardRotationOffset * playerHeadCamera.transform.forward, Vector3.up).normalized;
        var headR = Quaternion.LookRotation(headF, Vector3.up);
        hCenter = playerHeadCamera.transform.position;
        hCenter.y = 0;
        var pos = hCenter + headR * (Vector3.forward * distance);
        pos.y = playerHeadCamera.transform.position.y;
        transform.position = pos;
        transform.rotation = headR;
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(UISnapToPlayer))]
public class UISnapToPlayerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        UISnapToPlayer script = (UISnapToPlayer)target;

        EditorGUI.BeginChangeCheck(); // Start checking for changes

        DrawDefaultInspector();

        if (!script.UseMainCamera)
        {
            script.playerHeadCamera = (GameObject)EditorGUILayout.ObjectField("Player Head Camera",
                script.playerHeadCamera, typeof(GameObject), true);
        }

        if (EditorGUI.EndChangeCheck()) // Check if any changes occurred
        {
            EditorUtility.SetDirty(script); // Mark the object as dirty if changes occurred
        }
    }
}
#endif
