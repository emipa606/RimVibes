using UnityEngine;

namespace RimVibes.Components;

internal class AdditionalHook : MonoBehaviour
{
    private void OnGUI()
    {
        GUI.matrix = HookComponent.Matrix;
        GUI.color = new Color(0f, 0f, 0f, 0f);
        GUI.Button(HookComponent.HUDBounds, "");
        GUI.color = Color.white;
    }
}