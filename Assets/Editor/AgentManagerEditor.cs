using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AgentManager))]
public class AgentManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        
        AgentManager agentManager = (AgentManager)target;

        if (GUILayout.Button("Kill Wave"))
        {
            agentManager.KillWave();
        }
    }
}
