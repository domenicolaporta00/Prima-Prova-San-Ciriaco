using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;

namespace WB3DAssets.FenceModularSystem
{
public partial class FenceModularWindow : EditorWindow
{
    enum State { Idle, FreeMove, DirectionSelect, RailPreview, CornerSelect }

enum ContinueVariant
{
    V1,
    V2,
    V3,
    V4,
    V5
}

bool variantV1Available;

bool variantV2Available;

Texture2D variantV1Preview;

Texture2D variantV2Preview;

Texture2D variantV3Preview;

Texture2D variantV4Preview;

Texture2D variantV5Preview;

string VariantTag => selectedVariant switch
{
    ContinueVariant.V2 => "V2",
    ContinueVariant.V3 => "V3",
    ContinueVariant.V4 => "V4",
    ContinueVariant.V5 => "V5",
    _ => "V1"
};

//Cache element positions per fence root + variant tag for instant restore on switch-back
Dictionary<(GameObject root, string tag), List<(int sibling, Vector3 pos, Quaternion rot)>> variantPosCache
    = new Dictionary<(GameObject, string), List<(int, Vector3, Quaternion)>>();

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

    const string Root = "Assets/Fence Modular System";

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

    const string GhostMatName     = "GhostPreview_MAT";
    static readonly bool hasCurvedRails = false; // Set to true for fence types with curved sections

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
static readonly int GateSingleGizmoIdHint = "FMS_GateSingle".GetHashCode();
static readonly int GateDoubleGizmoIdHint = "FMS_GateDouble".GetHashCode();

const float Dot22_5 = 0.9238795f; // cos(22.5°)

const float Dot67_5 = 0.3826834f; // cos(67.5°)

    static readonly Color BaseCol     = new(0.45f, 0.75f, 1f, 0.55f);
    static readonly Color GateTextCol = new(1f, 1f, 1f, 1f);

    static readonly Color ActiveCol = new(0.65f, 1f, 0.30f, 0.95f);

    State state;

    bool buildMode;

bool canContinueBuild;
GameObject selectedContinuePillar;

const float FenceListHeight = 100f;

readonly List<GameObject> finalizedFences = new();

readonly Dictionary<GameObject, GameObject> fenceStartPillars = new();

readonly Dictionary<GameObject, ChainIndexCache> chainIndexByRoot = new();

readonly Dictionary<GameObject, List<int>> deletedRailIdsByRoot = new();

// Ghost snaps: when a rail is deleted, store its SnapPoint positions so the path can bridge gaps
readonly Dictionary<GameObject, List<(Vector3 snap1, Vector3 snap2)>> deletedRailSnapsByRoot = new();
// Temporary: snap positions of currently selected rails (captured before potential delete)
readonly Dictionary<int, (Vector3 snap1, Vector3 snap2)> pendingRailSnaps = new();

const string StartMarkerName = "__BMS_START__";

const string VariantLockMarkerName = "__BMS_VARIANT_LOCKED__";

int selectedFenceIndex = -1;

Vector2 fenceScroll;

System.Action onHierarchyChanged;

bool suppressDeleteUndo;

readonly HashSet<int> protectedPillarIds = new();

bool lastSelectionWasRail;

bool lastSelectionWasPillar;

GameObject lastRailDeleteFenceRoot;
readonly Dictionary<int, GameObject> railIdToFenceRoot = new(); // instanceId → fenceRoot, survives selection clear

readonly HashSet<int> railCoSelectedPillarIds = new(); // Pillars visually co-selected with rail
bool suppressSelectionChanged; // Prevent recursion when setting Selection.objects

    GameObject ghost;

    Material ghostMat;

static MaterialPropertyBlock ghostPropBlock;

static readonly Color GhostInvalidCol = new(1f, 0.45f, 0.4f, 0.65f);  // bright reddish warning

static readonly int PropBaseColor  = Shader.PropertyToID("_BaseColor");   // HDRP Lit + URP

static readonly int PropUnlitColor = Shader.PropertyToID("_UnlitColor");  // HDRP Unlit

static readonly int PropColor      = Shader.PropertyToID("_Color");       // Built-in fallback

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
bool continueFlipElements; // Flip when continuing from start pillar

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

GameObject continueTargetFence;
Vector3 continueScale = Vector3.one; // Scale inherited from target fence

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
// Paid Asset Store listing the upsell button opens. TODO: set the real URL.
const string FullVersionUrl = "https://assetstore.unity.com/packages/3d/environments/fence-modular-system-370338";

bool _freeComputed;
readonly HashSet<ContinueVariant> _availTypes = new HashSet<ContinueVariant>();
readonly Dictionary<string, int> _newFinish = new Dictionary<string, int>();
readonly Dictionary<string, int> _wornFinish = new Dictionary<string, int>();

void EnsureFreeAvailability()
{
    if (_freeComputed && _availTypes.Count > 0) return; // self-heal: recompute while still empty
    _availTypes.Clear();
    _newFinish.Clear();
    _wornFinish.Clear();
    foreach (ContinueVariant v in System.Enum.GetValues(typeof(ContinueVariant)))
    {
        string tag = VariantTagOf(v);
        if (FindAsset<GameObject>("post_" + tag + "M_PREFAB") != null) _availTypes.Add(v);
        int nc = 0, wc = 0;
        for (int i = 1; i <= 4; i++)
        {
            if (FindAsset<Material>("fence_" + tag + "_new"  + i + "_MAT") != null) nc++;
            if (FindAsset<Material>("fence_" + tag + "_worn" + i + "_MAT") != null) wc++;
        }
        _newFinish[tag] = nc;
        _wornFinish[tag] = wc;
    }
    // Only cache once assets are actually present. If this runs during a domain
    // reload (before the AssetDatabase is ready) nothing is found - leave it
    // uncached so the next OnGUI frame retries instead of caching an empty result.
    if (_availTypes.Count > 0) _freeComputed = true;
}

int FinishCount(string tag, bool worn)
{
    EnsureFreeAvailability();
    var d = worn ? _wornFinish : _newFinish;
    return d.TryGetValue(tag, out var c) ? c : 0;
}

readonly Dictionary<string, int> _firstNewIdx = new Dictionary<string, int>();
readonly Dictionary<string, int> _firstWornIdx = new Dictionary<string, int>();

// First available finish index (0-3) for a type, e.g. V1->0 (new1), V3->2 (new3).
// Caches only once found, so an early call (before assets load) safely retries.
int FirstFinishIndex(string tag, bool worn)
{
    var cache = worn ? _firstWornIdx : _firstNewIdx;
    if (cache.TryGetValue(tag, out var idx)) return idx;
    for (int i = 1; i <= 4; i++)
        if (FindAsset<Material>("fence_" + tag + "_" + (worn ? "worn" : "new") + i + "_MAT") != null)
        {
            cache[tag] = i - 1;
            return i - 1;
        }
    return 0;
}

ContinueVariant NextAvailableType(ContinueVariant cur, int dir)
{
    EnsureFreeAvailability();
    int n = System.Enum.GetValues(typeof(ContinueVariant)).Length;
    int idx = (int)cur;
    for (int step = 0; step < n; step++)
    {
        idx = ((idx + dir) % n + n) % n;
        var cand = (ContinueVariant)idx;
        if (_availTypes.Contains(cand)) return cand;
    }
    return cur;
}

    [MenuItem("Tools/Fence Modular System")]
    static void Open()
    {
        var w = GetWindow<FenceModularWindow>("Fence Modular System");
        w.minSize = w.maxSize = new Vector2(300, 560);
    }

void OnEnable()
{
    Selection.selectionChanged += OnSelectionChanged;
SceneView.duringSceneGui += OnSceneGUI_Overlay;
onHierarchyChanged = () =>
{
    CleanupFinalizedFences();
    RebuildProtectedPillarIdCache();
    Repaint();
};
EditorApplication.hierarchyChanged += onHierarchyChanged;
ObjectChangeEvents.changesPublished += OnObjectChanges_BlockPillarDelete;
Undo.undoRedoPerformed += OnUndoRedoPerformed;

// Scan immediately (works when scene is already loaded)
ScanSceneForExistingFences();
foreach (var root in finalizedFences)
    RebuildChainIndexForRoot(root);
RebuildProtectedPillarIdCache();

// Re-scan delayed (catches scene not yet ready on fresh import)
EditorApplication.delayCall += () =>
{
    EnsurePipelineMaterials();
    ScanSceneForExistingFences();
    foreach (var root in finalizedFences)
        RebuildChainIndexForRoot(root);
    RebuildProtectedPillarIdCache();
    LoadPreviewTextures();
    OnSelectionChanged();
    Repaint();
};

if (topPreviews == null) topPreviews = new Texture2D[4];
topPreviewIndex = 4; // start with "No Tops"
textureVariantIsWorn = false; // default = NEW
selectedVariant = ContinueVariant.V1; // hard default on window open
}

void LoadPreviewTextures()
{
    variantV1Preview = FindAsset<Texture2D>("fence_V1_PREVIEW");
    variantV2Preview = FindAsset<Texture2D>("fence_V2_PREVIEW");
    variantV3Preview = FindAsset<Texture2D>("fence_V3_PREVIEW");
    variantV4Preview = FindAsset<Texture2D>("fence_V4_PREVIEW");
    variantV5Preview = FindAsset<Texture2D>("fence_V5_PREVIEW");

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

// Cache last rail/gate selection so we can react after user deletes it
bool selIsRailOrGate = sel && (IsRailInstance(sel) || GetGateType(sel) != 0);
lastSelectionWasRail   = selIsRailOrGate;
lastSelectionWasPillar = sel && IsPillarInstance(sel);
lastRailDeleteFenceRoot = selIsRailOrGate
    ? FindOwningFenceRoot(sel.transform)?.gameObject
    : null;

// Build instanceId → fenceRoot map for ALL selected rails/gates
foreach (var o in Selection.objects)
{
    var go = o as GameObject;
    if (!go || (!IsRailInstance(go) && GetGateType(go) == 0)) continue;
    var root = FindOwningFenceRoot(go.transform)?.gameObject;
    if (root) railIdToFenceRoot[go.GetInstanceID()] = root;

    // Capture snap positions before potential delete
    var s1 = FindSnap(go.transform, RailStartSnap);
    var s2 = FindSnap(go.transform, RailEndSnap);
    if (s1 && s2) pendingRailSnaps[go.GetInstanceID()] = (s1.position, s2.position);
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
    SetFenceGizmosVisible(true);
}
else if (finalizedFences.Contains(sel))
{
    Tools.hidden = false;          // Transform gizmos ON
    SetFenceGizmosVisible(false); // MeshCollider + LOD gizmos OFF
    SyncUiFromFenceRoot(sel);
    EnsureFencePivotCentered(sel); // full-version parity: center pivot on root selection
}
else
{
    // Child selected → hide transform + collider/LOD gizmos
    var root = FindOwningFenceRoot(sel.transform);
    bool isFenceChild = root != null;
    bool isRoot = isFenceChild && root.gameObject == sel;

    Tools.hidden = isFenceChild && !isRoot;
    SetFenceGizmosVisible(!isFenceChild);

    if (isRoot) SyncUiFromFenceRoot(sel);
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

SetFenceGizmosVisible(true);
Tools.hidden = false;
}

void OnGUI()
{
// Cache UI target once per frame (IMGUI stability!)
GameObject uiRoot = FindFenceRootFromSelection(Selection.activeGameObject);
bool hasUiRoot = uiRoot != null;

// ALWAYS scan full root for UI display (root OR child selected)
if (uiRoot)
{
    ApplyUiStateFromFenceRoot(uiRoot);
}
else
{
    // UI defaults only when NOT building (Continue Build clears selection on purpose)
    if (!buildMode && !continueAnchorActive)
    {
        topPreviewIndex = topPreviews.Length;
        textureVariantIsWorn = false;
        newTextureIndex = FirstFinishIndex(VariantTag, false);  // FREE: first available finish for this type
        wornTextureIndex = FirstFinishIndex(VariantTag, true);
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
        ApplyFullDetailToAllFences(fullDetailMode);
    GUILayout.EndVertical();
    EditorGUILayout.EndHorizontal();

    GUILayout.Space(10);

    // =========================================================
    // FENCE TYPE (full width, 300x300) — always enabled
    // =========================================================

    GUILayout.Label("Choose Fence Type", EditorStyles.boldLabel);
    GUILayout.Space(4);

    float previewSize = 300f;
    Rect rVar = GUILayoutUtility.GetRect(previewSize, previewSize);
    DrawPreviewBorder(rVar);

    Texture2D varTex = selectedVariant switch
    {
        ContinueVariant.V2 => variantV2Preview,
        ContinueVariant.V3 => variantV3Preview,
        ContinueVariant.V4 => variantV4Preview,
        ContinueVariant.V5 => variantV5Preview,
        _ => variantV1Preview
    };

if (varTex)
{
    GUI.BeginGroup(rVar);
    var draw = new Rect(0f, PreviewImageYOffset, rVar.width, rVar.height);
    GUI.DrawTexture(draw, varTex, ScaleMode.ScaleToFit, true);
    GUI.EndGroup();

GUI.Label(
    new Rect(
        rVar.x,
        rVar.y + rVar.height - 30f,
        rVar.width,
        18f
    ),
    selectedVariant switch { ContinueVariant.V1 => "Picket Fence", ContinueVariant.V2 => "Privacy Fence", ContinueVariant.V3 => "Wrought Iron Fence", ContinueVariant.V4 => "Split Rail Fence", _ => "Concrete Fence" },
    noHoverFrameLabel
);
}

float arrowY = rVar.y + rVar.height - UIArrowSize - UIArrowInset;
Rect vLeft  = new Rect(rVar.x + UIArrowInset, arrowY, UIArrowSize, UIArrowSize);
Rect vRight = new Rect(rVar.x + rVar.width - UIArrowSize - UIArrowInset, arrowY, UIArrowSize, UIArrowSize);

if (DrawUIArrow(vLeft, "<"))
{
    var prev = selectedVariant;
    selectedVariant = NextAvailableType(prev, -1); // FREE: skip locked types
    if (selectedVariant != prev) SwitchFenceType(prev, selectedVariant);
    Repaint();
}

if (DrawUIArrow(vRight, ">"))
{
    var prev = selectedVariant;
    selectedVariant = NextAvailableType(prev, +1); // FREE: skip locked types
    if (selectedVariant != prev) SwitchFenceType(prev, selectedVariant);
    Repaint();
}

// ---- FREE VERSION upsell ----
GUILayout.Space(6);
EditorGUILayout.LabelField("LITE version - 2 of 5 fence types, 1 finish each.", EditorStyles.miniLabel);
{
    var _bg = GUI.backgroundColor;
    GUI.backgroundColor = new Color(0.45f, 0.85f, 0.45f);
    if (GUILayout.Button("Get the Full Version  (all 5 types & finishes)", GUILayout.Height(26)))
        Application.OpenURL(FullVersionUrl);
    GUI.backgroundColor = _bg;
}

GUILayout.Space(10);
EditorGUI.BeginDisabledGroup(!hasUiRoot);
EditorGUILayout.LabelField("Choose Texture Variant", EditorStyles.boldLabel);
GUILayout.Space(5);

    float halfW = (position.width - 5f) * 0.5f;
    GUILayout.BeginHorizontal();

// =========================================================
// NEW
// =========================================================
GUILayout.BeginVertical(GUILayout.Width(halfW));

bool newSelected = GUILayout.Toggle(!textureVariantIsWorn, "New", EditorStyles.radioButton);
if (newSelected && textureVariantIsWorn)
{
    ApplyTextureVariantToSelectedFence("worn" + (wornTextureIndex + 1), "new" + (newTextureIndex + 1));
    textureVariantIsWorn = false;
}

if (Event.current.type == EventType.MouseUp &&
    GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
{
    ApplyTextureVariantToSelectedFence("worn" + (wornTextureIndex + 1), "new" + (newTextureIndex + 1));
    Repaint();
}

GUILayout.Space(4);

EditorGUI.BeginDisabledGroup(textureVariantIsWorn);

Rect rNew = GUILayoutUtility.GetRect(halfW, 210);
DrawPreviewBorder(rNew);

var newTex = FindAsset<Texture2D>($"fence_{VariantTag}_new" + (newTextureIndex + 1) + "_PREVIEW");
if (newTex)
{
    GUI.BeginGroup(new Rect(rNew.x, rNew.y, rNew.width, 180));
    var draw = new Rect(0f, PreviewImageYOffset, rNew.width, 180);
    GUI.DrawTexture(draw, newTex, ScaleMode.ScaleToFit, true);
    GUI.EndGroup();
}

GUI.Label(
    new Rect(rNew.x, rNew.y + 180f, rNew.width, 18f),
    $"New #{newTextureIndex + 1}",
    noHoverFrameLabel
);

float newArrowY = rNew.y + 178f;
Rect nLeft  = new Rect(rNew.x + UIArrowInset, newArrowY, UIArrowSize, UIArrowSize);
Rect nRight = new Rect(rNew.x + rNew.width - UIArrowSize - UIArrowInset, newArrowY, UIArrowSize, UIArrowSize);

if (FinishCount(VariantTag, false) > 1) // FREE: hide cycle arrows when only one finish ships
{
if (DrawUIArrow(nLeft, "<"))
{
    string oldToken = "new" + (newTextureIndex + 1);
    newTextureIndex = (newTextureIndex + 3) % 4;
    string newToken = "new" + (newTextureIndex + 1);
    ApplyTextureVariantToSelectedFence(oldToken, newToken);
    Repaint();
}

if (DrawUIArrow(nRight, ">"))
{
    string oldToken = "new" + (newTextureIndex + 1);
    newTextureIndex = (newTextureIndex + 1) % 4;
    string newToken = "new" + (newTextureIndex + 1);
    ApplyTextureVariantToSelectedFence(oldToken, newToken);
    Repaint();
}
}

EditorGUI.EndDisabledGroup();

// Click on New image → select New
if (textureVariantIsWorn && Event.current.type == EventType.MouseDown && rNew.Contains(Event.current.mousePosition))
{
    ApplyTextureVariantToSelectedFence("worn" + (wornTextureIndex + 1), "new" + (newTextureIndex + 1));
    textureVariantIsWorn = false;
    Event.current.Use();
    Repaint();
}

GUILayout.EndVertical();

GUILayout.Space(5); //space between New and Worn Image

// =========================================================
// WORN (disabled when New is selected)
// =========================================================
GUILayout.BeginVertical(GUILayout.Width(halfW));

bool wornSelected = GUILayout.Toggle(textureVariantIsWorn, "Worn", EditorStyles.radioButton);
if (wornSelected && !textureVariantIsWorn)
{
    ApplyTextureVariantToSelectedFence("new" + (newTextureIndex + 1), "worn" + (wornTextureIndex + 1));
    textureVariantIsWorn = true;
}

if (Event.current.type == EventType.MouseUp &&
    GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
{
    ApplyTextureVariantToSelectedFence("new" + (newTextureIndex + 1), "worn" + (wornTextureIndex + 1));
    Repaint();
}

GUILayout.Space(4);

EditorGUI.BeginDisabledGroup(!textureVariantIsWorn);

Rect rWorn = GUILayoutUtility.GetRect(halfW, 210);
DrawPreviewBorder(rWorn);

var wornTex = FindAsset<Texture2D>($"fence_{VariantTag}_worn" + (wornTextureIndex + 1) + "_PREVIEW");
if (wornTex)
{
    GUI.BeginGroup(new Rect(rWorn.x, rWorn.y, rWorn.width, 180));
    var draw = new Rect(0f, PreviewImageYOffset, rWorn.width, 180);
    GUI.DrawTexture(draw, wornTex, ScaleMode.ScaleToFit, true);
    GUI.EndGroup();
}

GUI.Label(
    new Rect(rWorn.x, rWorn.y + 180f, rWorn.width, 18f),
    $"Worn #{wornTextureIndex + 1}",
    noHoverFrameLabel
);

float wornArrowY = rWorn.y + 178f;
Rect wLeft  = new Rect(rWorn.x + UIArrowInset, wornArrowY, UIArrowSize, UIArrowSize);
Rect wRight = new Rect(rWorn.x + rWorn.width - UIArrowSize - UIArrowInset, wornArrowY, UIArrowSize, UIArrowSize);

if (FinishCount(VariantTag, true) > 1) // FREE: hide cycle arrows when only one finish ships
{
if (DrawUIArrow(wLeft, "<"))
{
    string oldToken = "worn" + (wornTextureIndex + 1);
    wornTextureIndex = (wornTextureIndex + 3) % 4;
    string newToken = "worn" + (wornTextureIndex + 1);
    ApplyTextureVariantToSelectedFence(oldToken, newToken);
    Repaint();
}

if (DrawUIArrow(wRight, ">"))
{
    string oldToken = "worn" + (wornTextureIndex + 1);
    wornTextureIndex = (wornTextureIndex + 1) % 4;
    string newToken = "worn" + (wornTextureIndex + 1);
    ApplyTextureVariantToSelectedFence(oldToken, newToken);
    Repaint();
}
}

EditorGUI.EndDisabledGroup();

// Click on Worn image → select Worn
if (!textureVariantIsWorn && Event.current.type == EventType.MouseDown && rWorn.Contains(Event.current.mousePosition))
{
    ApplyTextureVariantToSelectedFence("new" + (newTextureIndex + 1), "worn" + (wornTextureIndex + 1));
    textureVariantIsWorn = true;
    Event.current.Use();
    Repaint();
}

GUILayout.EndVertical();

GUILayout.EndHorizontal();

EditorGUI.EndDisabledGroup();

GUILayout.Space(10);
EditorGUILayout.LabelField("Options", EditorStyles.boldLabel);
GUILayout.Space(5);

EditorGUI.BeginDisabledGroup(!hasUiRoot);
if (GUILayout.Button("Flip Selected", GUILayout.Height(26)))
{
    FlipSelectedFenceElements();
}
EditorGUI.EndDisabledGroup();

GUILayout.Space(3);

// Flip Single-Gate: enabled only if a single_gate_ prefab is selected inside a fence
bool isSingleGateSelected = IsSelectedSingleGate();
EditorGUI.BeginDisabledGroup(!isSingleGateSelected);
if (GUILayout.Button("Mirror Single-Gate", GUILayout.Height(26)))
{
    FlipSingleGateAroundSectionCenter();
}
EditorGUI.EndDisabledGroup();

if (Event.current.type == EventType.Layout)
{
    CleanupFinalizedFences();
    UpdateContinueBuildState();

    if (GetUiTargetFenceRoot() != null)
    {
        UpdateVariantAvailabilityFromHierarchy();
        UpdateTextureVariantFromHierarchy();
    }
}
}

static string VariantTagOf(ContinueVariant v) => v switch
{
    ContinueVariant.V2 => "V2",
    ContinueVariant.V3 => "V3",
    ContinueVariant.V4 => "V4",
    ContinueVariant.V5 => "V5",
    _ => "V1"
};

void SwitchFenceType(ContinueVariant from, ContinueVariant to)
{
    if (from == to) return;
    string fromTag = VariantTagOf(from);
    string toTag   = VariantTagOf(to);

    if (GetUiTargetFenceRoot() != null)
    {
        ReplaceFenceVariant(fromTag, toTag);
        RemoveAllTopsFromSelectedFence();
    }
}

void FlipSelectedFenceElements()
{
    var toFlip = new List<Transform>();

    foreach (var obj in Selection.objects)
    {
        var go = obj as GameObject;
        if (!go) continue;

        if (finalizedFences.Contains(go))
        {
            foreach (Transform child in go.transform)
                if (IsFlippableElement(child.gameObject)) toFlip.Add(child);
        }
        else if (FindFenceRootFromSelection(go) != null && IsFlippableElement(go))
        {
            toFlip.Add(go.transform);
        }
    }

    if (toFlip.Count == 0) return;

    Undo.IncrementCurrentGroup();
    int undoGroup = Undo.GetCurrentGroup();

    foreach (var t in toFlip)
        FlipVisuals180(t.gameObject);

    Undo.CollapseUndoOperations(undoGroup);
}

// Only rails and gates are flippable
static bool IsFlippableElement(GameObject go)
{
    if (!go) return false;
    var src = PrefabUtility.GetCorrespondingObjectFromSource(go);
    if (!src) return false;
    string n = src.name;
    return (n.StartsWith("sectionB_") || n.StartsWith("sectionBCrvd_") ||
            n.StartsWith("single_gate_") || n.StartsWith("double_gate_")) &&
           n.EndsWith("_PREFAB");
}

// Mirror visuals around SnapPoint1 on local Z axis, leaving SnapPoints untouched
// Check if currently selected object is a single_gate_ prefab inside a fence
bool IsSelectedSingleGate()
{
    var sel = Selection.activeGameObject;
    if (!sel) return false;
    var src = PrefabUtility.GetCorrespondingObjectFromSource(sel);
    if (!src || !src.name.StartsWith("single_gate_")) return false;
    return FindFenceRootFromSelection(sel) != null;
}

// Flip a single-gate by mirroring its transform on local X around the midpoint
// of the two pillars. Swaps SnapPoint names afterwards so SnapPoint1 still
// references the left pillar (required for variant swap to work correctly).
void FlipSingleGateAroundSectionCenter()
{
    var gate = Selection.activeGameObject;
    if (!gate) return;

    var sp1 = FindSnap(gate.transform, "SnapPoint1");
    var sp2 = FindSnap(gate.transform, "SnapPoint2");
    if (!sp1 || !sp2) return;

    Vector3 pillarMid = (sp1.position + sp2.position) * 0.5f;

    Undo.RecordObject(gate.transform, "Flip Single-Gate");

    var s = gate.transform.localScale;
    s.x *= -1f;
    gate.transform.localScale = s;

    Vector3 midAfter = (sp1.position + sp2.position) * 0.5f;
    gate.transform.position += pillarMid - midAfter;

    // Swap snap names: after mirror, SnapPoint1 sits on the right pillar.
    // Rename so SnapPoint1 still references the left pillar.
    Undo.RecordObject(sp1, "Flip Single-Gate");
    Undo.RecordObject(sp2, "Flip Single-Gate");
    sp1.name = "__tmp_swap__";
    sp2.name = "SnapPoint1";
    sp1.name = "SnapPoint2";
}

static void FlipVisuals180(GameObject go)
{
    if (!go) return;
    var snap = FindSnap(go.transform, "SnapPoint1");
    float pivotZ = snap ? go.transform.InverseTransformPoint(snap.position).z : 0f;

    // Flip all direct children except SnapPoints
    foreach (Transform child in go.transform)
    {
        if (!child) continue;
        if (child.name.StartsWith("SnapPoint")) continue;

        if (!Undo.isProcessing)
            Undo.RecordObject(child, "Flip Visual");

        // Mirror position around pivot in root-local Z
        Vector3 localPos = child.localPosition;
        localPos.z = 2f * pivotZ - localPos.z;
        child.localPosition = localPos;

        // Flip on Z
        var s = child.localScale;
        s.z *= -1f;
        child.localScale = s;
    }
}

}
} // namespace WB3DAssets.FenceModularSystem
