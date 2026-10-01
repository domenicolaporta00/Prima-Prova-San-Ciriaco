using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;

namespace WB3DAssets.BalustradeModularSystem
{
public partial class BalustradeModularWindow : EditorWindow
{
    enum State { Idle, FreeMove, DirectionSelect, RailPreview, CornerSelect }

enum ContinueVariant
{
    V1,
    V2
}

bool variantV1Available;

bool variantV2Available;

Texture2D variantV1Preview;

Texture2D variantV2Preview;

Texture2D balusterStylePreview;

Texture2D[] balusterStylePreviews;

int balusterStyleIndex = 0;

Texture2D[] topPreviews;

int topPreviewIndex = 0;

static GUIStyle noHoverFrameLabel;

static readonly Color ArrowBgCol =
    new Color(0x38 / 255f, 0x38 / 255f, 0x38 / 255f, 1f);

const float UIArrowSize = 22f;      // background rectangle size

const float UIArrowInset = 10f;     // left/right padding inside preview

const float PreviewImageYOffset = -15f; // negative = up

ContinueVariant selectedVariant = ContinueVariant.V1;

bool textureVariantIsWorn = false;

int newTextureIndex = 0; // 0 = new1, 1 = new2, 2 = new3, 3 = new4
int wornTextureIndex = 0; // 0 = worn1, 1 = worn2, 2 = worn3, 3 = worn4

    const string Root = "Assets/Balustrade Modular System";

    const string RootHDRP = Root + "/HDRP";

    const string RootURPBuiltIn = Root + "/URP & Built-In";
    const string RootURP  = RootURPBuiltIn + "/URP";
    const string RootBuiltIn = RootURPBuiltIn + "/Built-In";

    static string PipelineRoot
    {
        get
        {
            var rp = UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline;
            if (rp == null) return RootBuiltIn;
            string typeName = rp.GetType().Name;
            if (typeName.Contains("HDRenderPipeline")) return RootHDRP;
            return RootURP;
        }
    }

    const string PillarPrefabName = "pillar_V1E_PREFAB";

    const string RailPrefabName   = "blstrs_1V1_PREFAB";

    const string CurvedRailPrefabName = "blstrsCrvd_1V1_PREFAB";

const string PillarMPrefabName = "pillar_V1M_PREFAB";

const string PillarTPrefabName = "pillar_V1T_PREFAB";

const string PillarCPrefabName = "pillar_V1C_PREFAB";

const string PillarC45PrefabName = "pillar_V1C45_PREFAB";

    const string GhostMatName     = "GhostPreview_MAT";

    const string PillarSnapName   = "SnapPoint1"; // X+ = build dir

    const string RailStartSnap    = "SnapPoint1"; // Rail SnapPoint1 local X+ points to start pillar

const string RailEndSnap      = "SnapPoint2";

const string TopSnapName      = "SnapPointTop";

static readonly string[] TopPrefabNames =
{
    "top1_PREFAB",
    "top2_PREFAB",
    "top3_PREFAB",
    "top4_PREFAB",
};

const float ArrowLength = 0.4f;

const float ArrowHeadSize = 0.12f;

static readonly int ContinueGizmoIdHint = "BMS_ContinueDoubleArrow".GetHashCode();

const float Dot22_5 = 0.9238795f; // cos(22.5°)

const float Dot67_5 = 0.3826834f; // cos(67.5°)

    static readonly Color BaseCol   = new(0.45f, 0.75f, 1f, 0.55f);

    static readonly Color ActiveCol = new(0.65f, 1f, 0.30f, 0.95f);

    State state;

    bool buildMode;

bool canContinueBuild;

GameObject selectedContinuePillar;

const float BalustradeListHeight = 100f;

readonly List<GameObject> finalizedBalustrades = new();

readonly Dictionary<GameObject, GameObject> balustradeStartPillars = new();

readonly Dictionary<GameObject, ChainIndexCache> chainIndexByRoot = new();

readonly Dictionary<GameObject, List<int>> deletedRailIdsByRoot = new();

const string StartMarkerName = "__BMS_START__";

const string VariantLockMarkerName = "__BMS_VARIANT_LOCKED__";

int selectedBalustradeIndex = -1;

Vector2 balustradeScroll;

System.Action onHierarchyChanged;

bool suppressDeleteUndo;

readonly HashSet<int> protectedPillarIds = new();

bool lastSelectionWasRail;

bool lastSelectionWasPillar;

GameObject lastRailDeleteBalustradeRoot;

// Cache: selected rail ID → captured snap world poses + isCurved flag.
// Used to materialize a ghost-rail under the balustrade root when the rail is deleted,
// so variant switches can rebuild correctly across the gap.
struct PendingRailSnap
{
    public Vector3 snap1Pos, snap2Pos;
    public Quaternion snap1Rot, snap2Rot;
    public bool isCurved;
    public GameObject root;
}
readonly Dictionary<int, PendingRailSnap> pendingRailSnaps = new();

readonly HashSet<int> railCoSelectedPillarIds = new(); // Pillars visually co-selected with rail
bool suppressSelectionChanged; // Prevent recursion when setting Selection.objects

    GameObject ghost;

    Material ghostMat;

static MaterialPropertyBlock ghostPropBlock;

static readonly Color GhostInvalidCol = new(1f, 0.45f, 0.4f, 0.65f);  // bright reddish warning

static readonly int PropBaseColor  = Shader.PropertyToID("_BaseColor");   // HDRP Lit + URP

static readonly int PropUnlitColor = Shader.PropertyToID("_UnlitColor");  // HDRP Unlit

static readonly int PropColor      = Shader.PropertyToID("_Color");       // Built-in fallback

const float FlatNormalThreshold = 0.9998f; // dot(normal, up) must exceed this

bool fullDetailMode = true;

    GameObject lastPlacedPillarM;

GameObject lastPlacedPillarE;

readonly List<GameObject> currentBuildObjects = new();

    Vector3 frozenPos;

    Vector3 activeDir = Vector3.right;

    Transform lastPillarSnap;

bool continueDirOverrideActive;

Vector3 continueDirOverride;

string continueTSnapOverride; // "SnapPointT1" or "SnapPointT2"

bool isV1MContinueMode; // Flag for reduced CornerSelect from V1M

readonly List<GameObject> ghostPillarsM = new();

    GameObject railPreviewRoot;

    readonly List<GameObject> railSegs = new();

    Vector3 railAnchorPos;

    float segLen;

GameObject curvedGhostRoot;

GameObject ghostCurvedRail;

GameObject ghostCurvedPillar;

bool curvedGhostActive;

int consecutiveUnmirroredCurvedRails = 0;

int consecutiveUnmirroredPillarM = 0;

Transform curvedGhostEndSnap;

string curvedOutSnapName;

GameObject hover90Root;

GameObject hover90EndPillarE;

GameObject hover90Rail;

GameObject hover90PillarM;

readonly List<GameObject> hoverChainRails = new();

readonly List<GameObject> hoverChainPillarsM = new();

Vector3 hoverChainAnchorPos;

Vector3 hoverChainDir;

float hoverChainSegLen;

Transform hoverChainStartSnap; // Cached start snap for corner pillars

GameObject dirSelectHoverRoot;

readonly List<GameObject> dirSelectHoverRails = new();

readonly List<GameObject> dirSelectHoverPillars = new();

float dirSelectSegLen;

GameObject continueAnchorPillar;   // hidden V1E

bool continueAnchorActive;


Transform continueSnapProxy;

GameObject continueTargetBalustrade;
Vector3 continueScale = Vector3.one; // Scale inherited from target balustrade

int continueUndoGroup = -1; // Undo group captured at Continue Build start, collapsed on finalize

int continueTopIndex = -1; // -1 = no tops

GameObject continueGhostPillarM;

// Close loop detection
GameObject closeLoopTargetPillar;
bool closeLoopDetected;
char closeLoopReplacementType; // 'E','M','C','4'(C45),'T' or '\0'
Vector3 closeLoopApproachDir;
Quaternion closeLoopFitRotation;
Vector3 closeLoopFitPosition;
GameObject closeLoopReplacementGhost; // visual swap ghost
GameObject closeLoopOriginalGhost;    // hidden original last ghost
static readonly Color GhostCloseLoopCol = new(0.2f, 1f, 0.35f, 0.9f);

bool ENABLE_CONTINUE_BUILD = true;
bool ENABLE_V1M_VARIANT_LOCK = false; // Set true to re-enable V1M variant lock dialog

// ===================== FREE VERSION (Asset Store sampler) =====================
// Paid Asset Store listing the upsell button opens.
const string FullVersionUrl = "https://assetstore.unity.com/packages/3d/props/exterior/balustrade-modular-system-232938";

bool _freeComputed;
readonly HashSet<int> _availBaluster = new HashSet<int>(); // 1-based baluster style numbers present
readonly HashSet<int> _availTop = new HashSet<int>();       // 1-based top numbers present
int _newTexCount, _wornTexCount;

void EnsureFreeAvailability()
{
    if (_freeComputed) return;
    _availBaluster.Clear(); _availTop.Clear(); _newTexCount = 0; _wornTexCount = 0;
    for (int i = 1; i <= 12; i++)
        if (FindAsset<GameObject>("blstrs_" + i + "V1_PREFAB") != null) _availBaluster.Add(i);
    for (int i = 1; i <= 4; i++)
        if (FindAsset<GameObject>("top" + i + "_PREFAB") != null) _availTop.Add(i);
    for (int i = 1; i <= 4; i++)
    {
        if (FindAsset<Material>("blstrs_new"  + i + "_MAT") != null) _newTexCount++;
        if (FindAsset<Material>("blstrs_worn" + i + "_MAT") != null) _wornTexCount++;
    }
    // Self-healing: only cache once assets are present (avoids caching an empty
    // result when this runs during a domain reload before AssetDatabase is ready).
    if (_availBaluster.Count > 0) _freeComputed = true;
}

int NewTexCount()  { EnsureFreeAvailability(); return _newTexCount; }
int WornTexCount() { EnsureFreeAvailability(); return _wornTexCount; }

// Next available baluster style index (0-based) in cycle direction, skipping locked styles.
int NextAvailableBalusterIndex(int curIndex, int dir)
{
    EnsureFreeAvailability();
    if (_availBaluster.Count == 0) return curIndex;
    int n = balusterStylePreviews.Length;
    int idx = curIndex;
    for (int step = 0; step < n; step++)
    {
        idx = ((idx + dir) % n + n) % n;
        if (_availBaluster.Contains(idx + 1)) return idx;
    }
    return curIndex;
}

// Next available top carousel index. Slot count is topPreviews.Length + 1, where
// the last slot (== topPreviews.Length) is "No Tops". Skips locked tops.
int NextAvailableTopIndex(int curIndex, int dir)
{
    EnsureFreeAvailability();
    int noTops = topPreviews.Length;
    int n = topPreviews.Length + 1;
    int idx = curIndex;
    for (int step = 0; step < n; step++)
    {
        idx = ((idx + dir) % n + n) % n;
        if (idx == noTops) return idx;            // "No Tops" always allowed
        if (_availTop.Contains(idx + 1)) return idx;
    }
    return curIndex;
}

void OpenFullVersion() { Application.OpenURL(FullVersionUrl); }

    [MenuItem("Tools/Balustrade Modular System")]
    static void Open()
    {
        var w = GetWindow<BalustradeModularWindow>("Balustrade Modular System");
        w.minSize = w.maxSize = new Vector2(300, 560);
    }

void OnEnable()
{
    Selection.selectionChanged += OnSelectionChanged;
SceneView.duringSceneGui += OnSceneGUI_Overlay;
onHierarchyChanged = () =>
{
    CleanupFinalizedBalustrades();
    RebuildProtectedPillarIdCache();
    Repaint();
};
EditorApplication.hierarchyChanged += onHierarchyChanged;
ObjectChangeEvents.changesPublished += OnObjectChanges_BlockPillarDelete;
Undo.undoRedoPerformed += OnUndoRedoPerformed;

// Scan scene for existing balustrades (survives Editor restart)
ScanSceneForExistingBalustrades();
RebuildProtectedPillarIdCache();

// Delay asset loading until AssetDatabase is fully ready
EditorApplication.delayCall += () =>
{
    EnsurePipelineMaterials();
    LoadPreviewTextures();
    Repaint();
};

balusterStyleIndex = 0;
if (balusterStylePreviews == null) balusterStylePreviews = new Texture2D[12];
if (topPreviews == null) topPreviews = new Texture2D[4];
topPreviewIndex = 4; // start with "No Tops"
textureVariantIsWorn = false; // default = NEW
selectedVariant = ContinueVariant.V1; // hard default on window open
}

void LoadPreviewTextures()
{
    variantV1Preview = FindAsset<Texture2D>("pillar_V1_PREVIEW");
    variantV2Preview = FindAsset<Texture2D>("pillar_V2_PREVIEW");
    balusterStylePreviews = new Texture2D[]
    {
        FindAsset<Texture2D>("blstrs_1_PREVIEW"),
        FindAsset<Texture2D>("blstrs_2_PREVIEW"),
        FindAsset<Texture2D>("blstrs_3_PREVIEW"),
        FindAsset<Texture2D>("blstrs_4_PREVIEW"),
        FindAsset<Texture2D>("blstrs_5_PREVIEW"),
        FindAsset<Texture2D>("blstrs_6_PREVIEW"),
        FindAsset<Texture2D>("blstrs_7_PREVIEW"),
        FindAsset<Texture2D>("blstrs_8_PREVIEW"),
        FindAsset<Texture2D>("blstrs_9_PREVIEW"),
        FindAsset<Texture2D>("blstrs_10_PREVIEW"),
        FindAsset<Texture2D>("blstrs_11_PREVIEW"),
        FindAsset<Texture2D>("blstrs_12_PREVIEW"),
    };

    topPreviews = new Texture2D[]
    {
        FindAsset<Texture2D>("top1_PREVIEW"),
        FindAsset<Texture2D>("top2_PREVIEW"),
        FindAsset<Texture2D>("top3_PREVIEW"),
        FindAsset<Texture2D>("top4_PREVIEW"),
    };
    topPreviewIndex = topPreviews.Length;
}

void OnSelectionChanged()
{
    if (suppressSelectionChanged) return;

    var sel = Selection.activeGameObject;

// Cache last rail selection so we can react after user deletes it (Destroy event has no object reference)
lastSelectionWasRail = sel && IsRailInstance(sel);
lastSelectionWasPillar = sel && IsPillarInstance(sel);
lastRailDeleteBalustradeRoot = lastSelectionWasRail
    ? FindOwningBalustradeRoot(sel.transform)?.gameObject
    : null;

// Cache snap world poses of ALL currently selected rails so OnObjectChanges
// can later materialize ghost-rail markers under the root for variant reposition.
// Drop stale entries first: a rail that's no longer selected must not stay in
// pendingRailSnaps, otherwise a later delete on a DIFFERENT rail would also
// hide the previously-cached one.
{
    var currentSelIds = new HashSet<int>();
    foreach (var o in Selection.objects)
        if (o is GameObject g) currentSelIds.Add(g.GetInstanceID());
    var stale = new List<int>();
    foreach (var k in pendingRailSnaps.Keys)
        if (!currentSelIds.Contains(k)) stale.Add(k);
    foreach (var k in stale) pendingRailSnaps.Remove(k);
}
foreach (var obj in Selection.objects)
{
    var go = obj as GameObject;
    if (!go || !IsRailInstance(go)) continue;
    var s1 = FindSnap(go.transform, RailStartSnap);
    var s2 = FindSnap(go.transform, RailEndSnap);
    if (!s1 || !s2) continue;
    var rootGo = FindOwningBalustradeRoot(go.transform)?.gameObject;
    if (!rootGo) continue;
    var srcAsset = PrefabUtility.GetCorrespondingObjectFromSource(go);
    bool isCurved = srcAsset && srcAsset.name.StartsWith("blstrsCrvd_");
    pendingRailSnaps[go.GetInstanceID()] = new PendingRailSnap
    {
        snap1Pos = s1.position, snap1Rot = s1.rotation,
        snap2Pos = s2.position, snap2Rot = s2.rotation,
        isCurved = isCurved,
        root = rootGo
    };
}

// Rail selected → co-select connected pillars
// Single: orphan pillars only. Multi: pillars between selected rails.
bool isSingleSelect = Selection.objects.Length <= 1;
if (isSingleSelect) railCoSelectedPillarIds.Clear();
if (lastSelectionWasRail && sel)
{
    if (isSingleSelect)
    {
        var pillars = FindCoSelectPillarsForRail(sel, onlyOrphans: true);
        if (pillars.Count > 0)
        {
            foreach (var p in pillars)
                railCoSelectedPillarIds.Add(p.GetInstanceID());

            var capturedPillars = pillars.ToArray();
            var capturedSelection = Selection.objects;
            suppressSelectionChanged = true;
            EditorApplication.delayCall += () =>
            {
                var objs = new List<Object>(capturedSelection);
                foreach (var p in capturedPillars)
                    if (p && !objs.Contains(p)) objs.Add(p);
                if (objs.Count > capturedSelection.Length)
                    Selection.objects = objs.ToArray();
                EditorApplication.delayCall += () => suppressSelectionChanged = false;
            };
        }
    }
    else
    {
        // Multi-select: strip old co-selected pillars, find between-pillars
        var selectedRails = new List<GameObject>();
        var cleanSelection = new List<Object>();
        foreach (var o in Selection.objects)
        {
            var go = o as GameObject;
            if (go && railCoSelectedPillarIds.Contains(go.GetInstanceID())) continue;
            cleanSelection.Add(o);
            if (go && IsRailInstance(go)) selectedRails.Add(go);
        }
        railCoSelectedPillarIds.Clear();

        var pillars = FindPillarsBetweenRails(selectedRails.ToArray());
        foreach (var p in pillars)
            railCoSelectedPillarIds.Add(p.GetInstanceID());

        bool needsUpdate = pillars.Count > 0 || cleanSelection.Count < Selection.objects.Length;
        if (needsUpdate)
        {
            var capturedPillars = pillars.ToArray();
            var capturedClean = cleanSelection.ToArray();
            suppressSelectionChanged = true;
            EditorApplication.delayCall += () =>
            {
                var objs = new List<Object>(capturedClean);
                foreach (var p in capturedPillars)
                    if (p && !objs.Contains(p)) objs.Add(p);
                Selection.objects = objs.ToArray();
                EditorApplication.delayCall += () => suppressSelectionChanged = false;
            };
        }
    }
}

    // --- TRANSFORM GIZMO CONTROL ---
if (!sel)
{
    Tools.hidden = false;
    SetBalustradeGizmosVisible(true);
}
else if (finalizedBalustrades.Contains(sel))
{
    Tools.hidden = false;          // Transform gizmos ON
    SetBalustradeGizmosVisible(false); // MeshCollider + LOD gizmos OFF
    SyncUiFromBalustradeRoot(sel);
    EnsureBalustradePivotCentered(sel); // full-version parity: center pivot on root selection
}
else
{
    // Lazy discovery: find owning root (may add untracked Balustrade_ root)
    var root = FindBalustradeRootFromSelection(sel);
    bool isBalustradeChild = root != null;
    bool isRoot = isBalustradeChild && root == sel;

    Tools.hidden = isBalustradeChild && !isRoot;
    SetBalustradeGizmosVisible(!isBalustradeChild);

    if (isRoot) SyncUiFromBalustradeRoot(sel);
}

    UpdateContinueBuildState();
    SceneView.RepaintAll();
    Repaint();
}

void OnDisable()
{
    Selection.selectionChanged -= OnSelectionChanged;
SceneView.duringSceneGui -= OnSceneGUI_Overlay;
if (onHierarchyChanged != null)
    EditorApplication.hierarchyChanged -= onHierarchyChanged;
ObjectChangeEvents.changesPublished -= OnObjectChanges_BlockPillarDelete;
Undo.undoRedoPerformed -= OnUndoRedoPerformed;
onHierarchyChanged = null;
    StopBuildMode();

SetBalustradeGizmosVisible(true);
Tools.hidden = false;
}

void OnGUI()
{
// Cache UI target once per frame (IMGUI stability!)
GameObject uiRoot = FindBalustradeRootFromSelection(Selection.activeGameObject);
bool hasUiRoot = IsExplicitBalustradeRootSelected();

// ALWAYS scan full root for UI display (root OR child selected)
if (uiRoot)
{
    ApplyUiStateFromBalustradeRoot(uiRoot);
}
else
{
    // UI defaults only when NOT building (Continue Build clears selection on purpose)
    if (!buildMode && !continueAnchorActive)
    {
        selectedVariant = ContinueVariant.V1;
        topPreviewIndex = topPreviews.Length;
        balusterStyleIndex = 0;
        textureVariantIsWorn = false;
        newTextureIndex = 0;
        wornTextureIndex = 0;
    }
}

    if (noHoverFrameLabel == null)
    {
        noHoverFrameLabel = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold,
            fontSize = 13,
            normal  = { textColor = Color.white },
            hover   = { textColor = Color.white },
            active  = { textColor = Color.white },
            focused = { textColor = Color.white }
        };
    }

    EditorGUILayout.BeginHorizontal();
    if (GUILayout.Button(buildMode ? "Stop Build Mode" : "Start Build Mode", GUILayout.Height(32), GUILayout.ExpandWidth(true)))
    {
        if (buildMode) StopBuildMode();
        else StartBuildMode();
    }

    // Full Detail Mode toggle (beside button)
GUILayout.Space(4); // ← X-Offset (horizontal)
    GUILayout.BeginVertical(GUILayout.Width(80));
    GUILayout.Space(9); // ← Y-Offset (vertikal)
    EditorGUI.BeginChangeCheck();
    fullDetailMode = EditorGUILayout.ToggleLeft(
        new GUIContent("Full Detail",
            "When enabled, all elements display at highest detail (LOD0) regardless of camera distance. "
            + "Disable for runtime LOD optimization."),
        fullDetailMode,
        GUILayout.Width(80)
    );
    if (EditorGUI.EndChangeCheck())
        ApplyFullDetailToAllBalustrades(fullDetailMode);
    GUILayout.EndVertical();
    EditorGUILayout.EndHorizontal();

    GUILayout.Space(5);

    // Half width for all side-by-side preview columns
    float halfW = (position.width - 5f) * 0.5f;

    // =========================================================
    // VARIANT + TOP (SIDE BY SIDE)
    // =========================================================
GUILayout.BeginHorizontal();

    // ================= LEFT: VARIANT =================
EditorGUI.BeginDisabledGroup(
    !hasUiRoot
);

    GUILayout.BeginVertical(GUILayout.Width(halfW));

    GUILayout.Label("Choose Pillar Variant", EditorStyles.boldLabel);
    GUILayout.Space(4);

    Rect rVar = GUILayoutUtility.GetRect(halfW, 180);
    DrawPreviewBorder(rVar);

    Texture2D varTex =
        selectedVariant == ContinueVariant.V1
            ? variantV1Preview
            : variantV2Preview;

if (varTex)
{
    GUI.BeginGroup(rVar);
    var draw = new Rect(0f, PreviewImageYOffset, rVar.width, rVar.height);
    GUI.DrawTexture(draw, varTex, ScaleMode.ScaleToFit, true);
    GUI.EndGroup();

GUI.Label(
    new Rect(
        rVar.x,
        rVar.y + rVar.height - 30f, // ← height
        rVar.width,
        18f
    ),
    selectedVariant == ContinueVariant.V1 ? "Variant V1" : "Variant V2",
    noHoverFrameLabel
);
}

float arrowY = rVar.y + rVar.height - UIArrowSize - UIArrowInset;
Rect vLeft = new Rect(rVar.x + UIArrowInset, arrowY, UIArrowSize, UIArrowSize);
Rect vRight = new Rect(rVar.x + rVar.width - UIArrowSize - UIArrowInset, arrowY, UIArrowSize, UIArrowSize);

if (DrawUIArrow(vLeft, "<"))
{
    var prev = selectedVariant;

    // loop ALWAYS
    selectedVariant =
        selectedVariant == ContinueVariant.V1
            ? ContinueVariant.V2
            : ContinueVariant.V1;

    // V1 -> V2
    if (prev == ContinueVariant.V1 && selectedVariant == ContinueVariant.V2 && variantV2Available)
    {
        ReplaceBalustradeVariant("V1", "V2");
        RemoveAllTopsFromSelectedBalustrade(); // V2 must not use tops
    }

// V2 -> V1
if (prev == ContinueVariant.V2 && selectedVariant == ContinueVariant.V1 && variantV1Available)
{
    ReplaceBalustradeVariant("V2", "V1");

    if (topPreviewIndex < TopPrefabNames.Length)
        ApplyTopToSelectedBalustrade(topPreviewIndex);
    else
        RemoveAllTopsFromSelectedBalustrade();
}

    Repaint();
}

if (DrawUIArrow(vRight, ">"))
{
    var prev = selectedVariant;

    // loop ALWAYS
    selectedVariant =
        selectedVariant == ContinueVariant.V1
            ? ContinueVariant.V2
            : ContinueVariant.V1;

    // V1 -> V2
    if (prev == ContinueVariant.V1 && selectedVariant == ContinueVariant.V2 && variantV2Available)
    {
        ReplaceBalustradeVariant("V1", "V2");
        RemoveAllTopsFromSelectedBalustrade(); // V2 must not use tops
    }

// V2 -> V1
if (prev == ContinueVariant.V2 && selectedVariant == ContinueVariant.V1 && variantV1Available)
{
    ReplaceBalustradeVariant("V2", "V1");

    if (topPreviewIndex < TopPrefabNames.Length)
        ApplyTopToSelectedBalustrade(topPreviewIndex);
    else
        RemoveAllTopsFromSelectedBalustrade();
}

    Repaint();
}

    GUILayout.EndVertical();
EditorGUI.EndDisabledGroup();

    GUILayout.Space(5); //space between Pillar and Top Image

    // ================= RIGHT: TOP =================
EditorGUI.BeginDisabledGroup(
    selectedVariant != ContinueVariant.V1 ||
    !IsExplicitBalustradeRootSelected()
);
    GUILayout.BeginVertical(GUILayout.Width(halfW));

    GUILayout.Label("Choose Top (V1 only)", EditorStyles.boldLabel);
    GUILayout.Space(4);

    Rect rTop = GUILayoutUtility.GetRect(halfW, 180);
    DrawPreviewBorder(rTop);

if (topPreviewIndex < topPreviews.Length)
{
    var topTex = topPreviews[topPreviewIndex];
    if (topTex)
    {
        GUI.BeginGroup(rTop);
        var draw = new Rect(0f, PreviewImageYOffset, rTop.width, rTop.height);
        GUI.DrawTexture(draw, topTex, ScaleMode.ScaleToFit, true);
        GUI.EndGroup();
    }
}
else
{
var noHoverLabel = new GUIStyle(GUI.skin.label)
{
    alignment = TextAnchor.MiddleCenter,
    fontSize = 24,
    fontStyle = FontStyle.Bold,
    normal = { textColor = Color.white },
    hover  = { textColor = Color.white },
    active = { textColor = Color.white },
    focused = { textColor = Color.white }
};

GUI.Label(rTop, "No Tops", noHoverLabel);
}

string topLabel =
    topPreviewIndex < topPreviews.Length
        ? $"Top #{topPreviewIndex + 1}"
        : string.Empty;

GUI.Label(
    new Rect(
        rTop.x,
        rTop.y + rTop.height - 30f, // ← height
        rTop.width,
        18f
    ),
    topLabel,
    noHoverFrameLabel
);

    // Overlay arrows (top)
float topArrowY = rTop.y + rTop.height - UIArrowSize - UIArrowInset;

Rect tLeft = new Rect(rTop.x + UIArrowInset, topArrowY, UIArrowSize, UIArrowSize);
Rect tRight = new Rect(rTop.x + rTop.width - UIArrowSize - UIArrowInset, topArrowY, UIArrowSize, UIArrowSize);

EditorGUI.DrawRect(tLeft, ArrowBgCol);
if (GUI.Button(tLeft, "<"))
{
topPreviewIndex = NextAvailableTopIndex(topPreviewIndex, -1); // FREE: skip locked tops

if (selectedVariant == ContinueVariant.V1)
{
    if (topPreviewIndex < TopPrefabNames.Length)
        ApplyTopToSelectedBalustrade(topPreviewIndex);
    else
        RemoveAllTopsFromSelectedBalustrade();
}

    Repaint();
}

EditorGUI.DrawRect(tRight, ArrowBgCol);
if (GUI.Button(tRight, ">"))
{
topPreviewIndex = NextAvailableTopIndex(topPreviewIndex, +1); // FREE: skip locked tops

if (selectedVariant == ContinueVariant.V1)
{
    if (topPreviewIndex < TopPrefabNames.Length)
        ApplyTopToSelectedBalustrade(topPreviewIndex);
    else
        RemoveAllTopsFromSelectedBalustrade();
}

    Repaint();
}

    GUILayout.EndVertical();
EditorGUI.EndDisabledGroup();
    GUILayout.EndHorizontal();
EditorGUI.EndDisabledGroup();

    // =========================================================
    // BALUSTER STYLE (UNCHANGED, BELOW)
    // =========================================================
EditorGUI.BeginDisabledGroup(!hasUiRoot);
    GUILayout.Space(12);
    GUILayout.Label("Choose Baluster Style", EditorStyles.boldLabel);
    GUILayout.Space(4);

    if (balusterStylePreviews != null && balusterStylePreviews.Length > 0)
    {
        Rect r = GUILayoutUtility.GetRect(230, 230);
        DrawPreviewBorder(r);

        var tex = balusterStylePreviews[balusterStyleIndex];
        if (tex)
            GUI.DrawTexture(r, tex, ScaleMode.ScaleToFit);

float bArrowY = r.y + r.height - UIArrowSize - UIArrowInset;

Rect bLeft = new Rect(r.x + UIArrowInset, bArrowY, UIArrowSize, UIArrowSize);
Rect bRight = new Rect(r.x + r.width - UIArrowSize - UIArrowInset, bArrowY, UIArrowSize, UIArrowSize);

EditorGUI.DrawRect(bLeft, ArrowBgCol);
if (GUI.Button(bLeft, "<"))
{
    int prev = balusterStyleIndex;
    balusterStyleIndex = NextAvailableBalusterIndex(prev, -1); // FREE: skip locked styles
    if (balusterStyleIndex != prev)
        ReplaceBalusterStyle(prev + 1, balusterStyleIndex + 1, selectedVariant);
    Repaint();
}

EditorGUI.DrawRect(bRight, ArrowBgCol);
if (GUI.Button(bRight, ">"))
{
    int prev = balusterStyleIndex;
    balusterStyleIndex = NextAvailableBalusterIndex(prev, +1); // FREE: skip locked styles
    if (balusterStyleIndex != prev)
        ReplaceBalusterStyle(prev + 1, balusterStyleIndex + 1, selectedVariant);
    Repaint();
}

        // Label
var noHoverTopLabel = new GUIStyle(GUI.skin.label)
{
    alignment = TextAnchor.UpperCenter,
    fontStyle = FontStyle.Bold,
    normal = { textColor = Color.white },
    hover  = { textColor = Color.white },
    active = { textColor = Color.white },
    focused = { textColor = Color.white }
};

GUI.Label(
    new Rect(
        r.x,
        r.y + r.height - 30f,   // ← BOTTOM
        r.width,
        20f
    ),
    $"Baluster Style #{balusterStyleIndex + 1}",
    noHoverTopLabel
);

GUILayout.Space(10);
EditorGUILayout.LabelField("Choose Texture Variant", EditorStyles.boldLabel);
GUILayout.Space(5);

GUILayout.BeginHorizontal();

// =========================================================
// NEW
// =========================================================
GUILayout.BeginVertical(GUILayout.Width(halfW));

bool newSelected = GUILayout.Toggle(!textureVariantIsWorn, "New", EditorStyles.radioButton);
if (newSelected && textureVariantIsWorn)
{
    ApplyTextureVariantToSelectedBalustrade("worn" + (wornTextureIndex + 1), "new" + (newTextureIndex + 1));
    textureVariantIsWorn = false;
}

if (Event.current.type == EventType.MouseUp &&
    GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
{
    ApplyTextureVariantToSelectedBalustrade("worn" + (wornTextureIndex + 1), "new" + (newTextureIndex + 1));
    Repaint();
}

GUILayout.Space(4);

Rect rNew = GUILayoutUtility.GetRect(halfW, 180);
DrawPreviewBorder(rNew);

// Click on image activates "new" variant (must run BEFORE BeginDisabledGroup,
// otherwise the disabled group swallows the click on the currently-inactive image)
if (textureVariantIsWorn &&
    Event.current.type == EventType.MouseDown &&
    Event.current.button == 0 &&
    rNew.Contains(Event.current.mousePosition))
{
    ApplyTextureVariantToSelectedBalustrade("worn" + (wornTextureIndex + 1), "new" + (newTextureIndex + 1));
    textureVariantIsWorn = false;
    Event.current.Use();
    Repaint();
}

EditorGUI.BeginDisabledGroup(textureVariantIsWorn);

var newTex = FindAsset<Texture2D>("new" + (newTextureIndex + 1) + "_PREVIEW");
if (newTex)
{
    GUI.BeginGroup(rNew);
    var draw = new Rect(0f, PreviewImageYOffset, rNew.width, rNew.height);
    GUI.DrawTexture(draw, newTex, ScaleMode.ScaleToFit, true);
    GUI.EndGroup();
}

// Label between arrows
GUI.Label(
    new Rect(
        rNew.x,
        rNew.y + rNew.height - 30f,
        rNew.width,
        18f
    ),
    $"New #{newTextureIndex + 1}",
    noHoverFrameLabel
);

// Arrows — cycle through new1/new2/new3/new4
float newArrowY = rNew.y + rNew.height - UIArrowSize - UIArrowInset;
Rect nLeft  = new Rect(rNew.x + UIArrowInset, newArrowY, UIArrowSize, UIArrowSize);
Rect nRight = new Rect(rNew.x + rNew.width - UIArrowSize - UIArrowInset, newArrowY, UIArrowSize, UIArrowSize);

if (NewTexCount() > 1) // FREE: hide cycle arrows when only one finish ships
{
if (DrawUIArrow(nLeft, "<"))
{
    string oldToken = "new" + (newTextureIndex + 1);
    newTextureIndex = (newTextureIndex + 3) % 4;
    string newToken = "new" + (newTextureIndex + 1);
    ApplyTextureVariantToSelectedBalustrade(oldToken, newToken);
    Repaint();
}

if (DrawUIArrow(nRight, ">"))
{
    string oldToken = "new" + (newTextureIndex + 1);
    newTextureIndex = (newTextureIndex + 1) % 4;
    string newToken = "new" + (newTextureIndex + 1);
    ApplyTextureVariantToSelectedBalustrade(oldToken, newToken);
    Repaint();
}
}

EditorGUI.EndDisabledGroup();

GUILayout.EndVertical();

GUILayout.Space(5); //space between New and Worn Image

// =========================================================
// WORN (disabled when New is selected)
// =========================================================
GUILayout.BeginVertical(GUILayout.Width(halfW));

bool wornSelected = GUILayout.Toggle(textureVariantIsWorn, "Worn", EditorStyles.radioButton);
if (wornSelected && !textureVariantIsWorn)
{
    ApplyTextureVariantToSelectedBalustrade("new" + (newTextureIndex + 1), "worn" + (wornTextureIndex + 1));
    textureVariantIsWorn = true;
}

if (Event.current.type == EventType.MouseUp &&
    GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
{
    ApplyTextureVariantToSelectedBalustrade("new" + (newTextureIndex + 1), "worn" + (wornTextureIndex + 1));
    Repaint();
}

GUILayout.Space(4);

Rect rWorn = GUILayoutUtility.GetRect(halfW, 180);
DrawPreviewBorder(rWorn);

// Click on image activates "worn" variant
if (!textureVariantIsWorn &&
    Event.current.type == EventType.MouseDown &&
    Event.current.button == 0 &&
    rWorn.Contains(Event.current.mousePosition))
{
    ApplyTextureVariantToSelectedBalustrade("new" + (newTextureIndex + 1), "worn" + (wornTextureIndex + 1));
    textureVariantIsWorn = true;
    Event.current.Use();
    Repaint();
}

EditorGUI.BeginDisabledGroup(!textureVariantIsWorn);

var wornTex = FindAsset<Texture2D>("worn" + (wornTextureIndex + 1) + "_PREVIEW");
if (wornTex)
{
    GUI.BeginGroup(rWorn);
    var draw = new Rect(0f, PreviewImageYOffset, rWorn.width, rWorn.height);
    GUI.DrawTexture(draw, wornTex, ScaleMode.ScaleToFit, true);
    GUI.EndGroup();
}

// Label between arrows
GUI.Label(
    new Rect(
        rWorn.x,
        rWorn.y + rWorn.height - 30f,
        rWorn.width,
        18f
    ),
    $"Worn #{wornTextureIndex + 1}",
    noHoverFrameLabel
);

// Arrows — cycle through worn1/worn2/worn3/worn4
float wornArrowY = rWorn.y + rWorn.height - UIArrowSize - UIArrowInset;
Rect wLeft  = new Rect(rWorn.x + UIArrowInset, wornArrowY, UIArrowSize, UIArrowSize);
Rect wRight = new Rect(rWorn.x + rWorn.width - UIArrowSize - UIArrowInset, wornArrowY, UIArrowSize, UIArrowSize);

if (WornTexCount() > 1) // FREE: hide cycle arrows when only one finish ships
{
if (DrawUIArrow(wLeft, "<"))
{
    string oldToken = "worn" + (wornTextureIndex + 1);
    wornTextureIndex = (wornTextureIndex + 3) % 4;
    string newToken = "worn" + (wornTextureIndex + 1);
    ApplyTextureVariantToSelectedBalustrade(oldToken, newToken);
    Repaint();
}

if (DrawUIArrow(wRight, ">"))
{
    string oldToken = "worn" + (wornTextureIndex + 1);
    wornTextureIndex = (wornTextureIndex + 1) % 4;
    string newToken = "worn" + (wornTextureIndex + 1);
    ApplyTextureVariantToSelectedBalustrade(oldToken, newToken);
    Repaint();
}
}

EditorGUI.EndDisabledGroup();

GUILayout.EndVertical();

GUILayout.EndHorizontal();
    }

EditorGUI.EndDisabledGroup();

GUILayout.FlexibleSpace();

// Subtle hint that Unity's standard Undo works on every tool action.
var undoTipStyle = new GUIStyle(EditorStyles.miniLabel)
{
    fontStyle = FontStyle.Italic,
    alignment = TextAnchor.MiddleCenter,
    wordWrap = true
};
undoTipStyle.normal.textColor = new Color(0.55f, 0.55f, 0.55f);
GUILayout.Label("Tip: Use Ctrl+Z to undo any action.", undoTipStyle);
GUILayout.Space(4);

// ---- FREE VERSION upsell (bottom - away from Start Build Mode) ----
GUILayout.Label("LITE version: 3 of 12 balusters, 1 finish.", EditorStyles.miniLabel);
{
    var _bg = GUI.backgroundColor;
    GUI.backgroundColor = new Color(0.45f, 0.85f, 0.45f);
    if (GUILayout.Button("Get the Full Version", GUILayout.Height(24)))
        OpenFullVersion();
    GUI.backgroundColor = _bg;
}
GUILayout.Space(4);

if (Event.current.type == EventType.Layout)
{
    CleanupFinalizedBalustrades();
    UpdateContinueBuildState();

    if (GetUiTargetBalustradeRoot() != null)
    {
        UpdateVariantAvailabilityFromHierarchy();
        UpdateBalusterStyleFromHierarchy();
        UpdateTextureVariantFromHierarchy();
    }
}
}

}
} // namespace WB3DAssets.BalustradeModularSystem
