using System.Collections.Generic;
using UnityEngine;

// Press R during Play mode to put every Rigidbody back where it started,
// so you can replay the ball-drop demo without restarting.
public class BallResetter : MonoBehaviour
{
    struct Saved { public Rigidbody rb; public Vector3 pos; public Quaternion rot; }
    readonly List<Saved> saved = new List<Saved>();

    void Start()
    {
        foreach (var rb in GetComponentsInChildren<Rigidbody>())
            saved.Add(new Saved { rb = rb, pos = rb.transform.position, rot = rb.transform.rotation });
    }

    void Update()
    {
        if (!ResetPressed()) return;
        foreach (var s in saved)
        {
#if UNITY_6000_0_OR_NEWER
            s.rb.linearVelocity = Vector3.zero;
#else
            s.rb.velocity = Vector3.zero;
#endif
            s.rb.angularVelocity = Vector3.zero;
            s.rb.position = s.pos;
            s.rb.rotation = s.rot;
            s.rb.transform.SetPositionAndRotation(s.pos, s.rot);
            s.rb.WakeUp();
        }
    }

    static bool ResetPressed()
    {
#if ENABLE_INPUT_SYSTEM
        return UnityEngine.InputSystem.Keyboard.current != null &&
               UnityEngine.InputSystem.Keyboard.current.rKey.wasPressedThisFrame;
#elif ENABLE_LEGACY_INPUT_MANAGER
        return Input.GetKeyDown(KeyCode.R);
#else
        return false;
#endif
    }
}
