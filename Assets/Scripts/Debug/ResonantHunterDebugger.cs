using UnityEngine;

/// <summary>
/// Debug helper to test ResonantHunter behavior - attach to any GameObject in scene
/// Shows current player color and forces specific colors for testing
/// </summary>
public class ResonantHunterDebugger : MonoBehaviour
{
    [Header("Debug Controls")]
    public bool showDebugUI = true;
    public KeyCode forceRedKey = KeyCode.F1;
    public KeyCode forceGreenKey = KeyCode.F2;
    public KeyCode forceBlueKey = KeyCode.F3;
    public KeyCode forceNoneKey = KeyCode.F4;
    
    private ResonanceCore playerResonanceCore;
    private ResonantHunter[] allHunters;
    
    void Start()
    {
        // Find player resonance core
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerResonanceCore = player.GetComponent<ResonanceCore>();
        }
        
        // Find all resonant hunters
        RefreshHunterList();
    }
    
    void Update()
    {
        // Refresh hunter list periodically
        if (Time.frameCount % 60 == 0) // Every 60 frames
        {
            RefreshHunterList();
        }
        
        // Handle debug key inputs
        if (playerResonanceCore != null)
        {
            if (Input.GetKeyDown(forceRedKey))
            {
                playerResonanceCore.SetColor(ResonanceColor.Red);
                Debug.Log("Debug: Forced player color to RED");
            }
            else if (Input.GetKeyDown(forceGreenKey))
            {
                playerResonanceCore.SetColor(ResonanceColor.Green);
                Debug.Log("Debug: Forced player color to GREEN");
            }
            else if (Input.GetKeyDown(forceBlueKey))
            {
                playerResonanceCore.SetColor(ResonanceColor.Blue);
                Debug.Log("Debug: Forced player color to BLUE");
            }
            else if (Input.GetKeyDown(forceNoneKey))
            {
                playerResonanceCore.SetColor(ResonanceColor.None);
                Debug.Log("Debug: Forced player color to NONE");
            }
        }
    }
    
    void RefreshHunterList()
    {
        allHunters = FindObjectsByType<ResonantHunter>(FindObjectsSortMode.None);
    }
    
    void OnGUI()
    {
        if (!showDebugUI) return;
        
        GUILayout.BeginArea(new Rect(10, 10, 300, 400));
        GUILayout.Label("=== RESONANT HUNTER DEBUGGER ===");
        
        // Player info
        if (playerResonanceCore != null)
        {
            GUILayout.Label($"Player Color: {playerResonanceCore.GetCurrentColor()}");
        }
        else
        {
            GUILayout.Label("Player Color: NOT FOUND!");
        }
        
        GUILayout.Space(10);
        GUILayout.Label("=== HUNTERS ===");
        
        if (allHunters != null && allHunters.Length > 0)
        {
            for (int i = 0; i < allHunters.Length; i++)
            {
                ResonantHunter hunter = allHunters[i];
                if (hunter != null)
                {
                    float distance = playerResonanceCore != null ? 
                        Vector3.Distance(hunter.transform.position, playerResonanceCore.transform.position) : 0f;
                    
                    GUILayout.Label($"Hunter {i + 1}:");
                    GUILayout.Label($"  Color: {hunter.GetColor()}");
                    GUILayout.Label($"  Distance: {distance:F1}");
                    
                    // Access private fields via reflection for debugging
                    var hunterType = typeof(ResonantHunter);
                    var isAvoidingField = hunterType.GetField("isAvoiding", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    var lastPlayerColorField = hunterType.GetField("lastPlayerColor", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    
                    if (isAvoidingField != null && lastPlayerColorField != null)
                    {
                        bool isAvoiding = (bool)isAvoidingField.GetValue(hunter);
                        ResonanceColor lastPlayerColor = (ResonanceColor)lastPlayerColorField.GetValue(hunter);
                        
                        GUILayout.Label($"  Last Player Color: {lastPlayerColor}");
                        GUILayout.Label($"  Is Avoiding: {(isAvoiding ? "YES" : "NO")}");
                    }
                    
                    GUILayout.Space(5);
                }
            }
        }
        else
        {
            GUILayout.Label("No Resonant Hunters found!");
        }
        
        GUILayout.Space(10);
        GUILayout.Label("=== CONTROLS ===");
        GUILayout.Label("F1: Force Red");
        GUILayout.Label("F2: Force Green");
        GUILayout.Label("F3: Force Blue");
        GUILayout.Label("F4: Force None");
        
        GUILayout.EndArea();
    }
}