// ============================================================
//  MazeGenerator.cs  —  Editor Script
//  Place in:  Assets/Scripts/Editor/MazeGenerator.cs
// ============================================================
#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Generates a 3D maze using a Depth-First Search (recursive backtracker).
/// Open via:  Tools ▶ Generate Maze
///
/// Grid layout:
///   • Each cell is CELL_SIZE × CELL_SIZE world units
///   • Walls are 1-unit thick cubes scaled to fill gaps
///   • The maze always has a guaranteed path from Start → Key → Door
///
/// After generation:
///   1. Place your Starter Assets player at the START marker
///   2. Place the Key cube at the KEY marker
///   3. Place the Door at the DOOR marker
///   4. Place traps and guard waypoints inside the maze corridors
/// </summary>
public class MazeGenerator : EditorWindow
{
    // ── Tuneable constants ────────────────────────────────────────────────────
    private const int   COLS      = 12;       // maze columns
    private const int   ROWS      = 12;       // maze rows
    private const float CELL_SIZE = 4f;       // world units per cell
    private const float WALL_H    = 3f;       // wall height
    private const float WALL_T    = 0.5f;     // wall thickness

    // ── Editor fields ─────────────────────────────────────────────────────────
    private Material _floorMat;
    private Material _wallMat;

    [MenuItem("Tools/Generate Maze")]
    public static void ShowWindow()
    {
        GetWindow<MazeGenerator>("Maze Generator");
    }

    void OnGUI()
    {
        GUILayout.Label("Maze Generator", EditorStyles.boldLabel);
        GUILayout.Space(6);

        _floorMat = (Material)EditorGUILayout.ObjectField("Floor Material", _floorMat, typeof(Material), false);
        _wallMat  = (Material)EditorGUILayout.ObjectField("Wall Material",  _wallMat,  typeof(Material), false);

        GUILayout.Space(10);

        if (GUILayout.Button("Generate Maze", GUILayout.Height(40)))
        {
            GenerateMaze();
        }

        GUILayout.Space(6);
        if (GUILayout.Button("Clear Maze"))
        {
            ClearMaze();
        }
    }

    // ── Main generation entry point ───────────────────────────────────────────
    private void GenerateMaze()
    {
        ClearMaze();

        // Parent container
        GameObject root = new GameObject("Maze");
        Undo.RegisterCreatedObjectUndo(root, "Generate Maze");

        // Run DFS to carve passages
        bool[,] visited = new bool[COLS, ROWS];
        // wallH[c,r] = true  → horizontal wall between (c,r) and (c,r+1) exists
        // wallV[c,r] = true  → vertical   wall between (c,r) and (c+1,r) exists
        bool[,] wallH = new bool[COLS,     ROWS + 1];
        bool[,] wallV = new bool[COLS + 1, ROWS    ];

        // Start: all walls present
        for (int c = 0; c <= COLS; c++)
            for (int r = 0; r <  ROWS; r++)
                wallV[c, r] = true;
        for (int c = 0; c <  COLS; c++)
            for (int r = 0; r <= ROWS; r++)
                wallH[c, r] = true;

        // Carve with DFS from (0,0)
        DFS(0, 0, visited, wallH, wallV);

        // Remove border walls for start & end openings
        wallV[0,    0]        = false;   // entrance on left side
        wallV[COLS, ROWS - 1] = false;   // exit on right side

        // ── Build floor ───────────────────────────────────────────────────────
        GameObject floor = CreateBox("Floor", root.transform,
            new Vector3(COLS * CELL_SIZE * 0.5f, -0.25f, ROWS * CELL_SIZE * 0.5f),
            new Vector3(COLS * CELL_SIZE, 0.5f, ROWS * CELL_SIZE),
            _floorMat);

        // ── Build outer boundary walls ────────────────────────────────────────
        // We leave the entrance/exit gaps open (handled above by removing walls)
        // The DFS already placed inner walls; outer border is added here.

        // ── Place walls ───────────────────────────────────────────────────────
        // Horizontal walls (run along X axis, separate rows)
        for (int c = 0; c < COLS; c++)
        {
            for (int r = 0; r <= ROWS; r++)
            {
                if (!wallH[c, r]) continue;
                float x = c * CELL_SIZE + CELL_SIZE * 0.5f;
                float z = r * CELL_SIZE;
                CreateBox($"WH_{c}_{r}", root.transform,
                    new Vector3(x, WALL_H * 0.5f, z),
                    new Vector3(CELL_SIZE, WALL_H, WALL_T),
                    _wallMat);
            }
        }

        // Vertical walls (run along Z axis, separate columns)
        for (int c = 0; c <= COLS; c++)
        {
            for (int r = 0; r < ROWS; r++)
            {
                if (!wallV[c, r]) continue;
                float x = c * CELL_SIZE;
                float z = r * CELL_SIZE + CELL_SIZE * 0.5f;
                CreateBox($"WV_{c}_{r}", root.transform,
                    new Vector3(x, WALL_H * 0.5f, z),
                    new Vector3(WALL_T, WALL_H, CELL_SIZE),
                    _wallMat);
            }
        }

        // ── Place markers ─────────────────────────────────────────────────────
        PlaceMarker("START",  root.transform, CellCenter(0, 0),            Color.green);
        PlaceMarker("KEY",    root.transform, CellCenter(COLS/2, ROWS/2),  Color.yellow);
        PlaceMarker("DOOR",   root.transform, CellCenter(COLS-1, ROWS-1),  Color.blue);

        Debug.Log("Maze generated! Move START / KEY / DOOR markers as needed.");
        Selection.activeGameObject = root;
    }

    // ── DFS Recursive Backtracker ─────────────────────────────────────────────
    private static readonly (int dc, int dr)[] Dirs =
    {
        ( 0,  1),   // North
        ( 0, -1),   // South
        ( 1,  0),   // East
        (-1,  0),   // West
    };

    private void DFS(int c, int r, bool[,] visited,
                     bool[,] wallH, bool[,] wallV)
    {
        visited[c, r] = true;

        // Shuffle directions for randomness
        var dirs = new List<(int, int)>(Dirs);
        for (int i = dirs.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (dirs[i], dirs[j]) = (dirs[j], dirs[i]);
        }

        foreach (var (dc, dr) in dirs)
        {
            int nc = c + dc;
            int nr = r + dr;

            if (nc < 0 || nc >= COLS || nr < 0 || nr >= ROWS) continue;
            if (visited[nc, nr]) continue;

            // Remove the wall between (c,r) and (nc,nr)
            if (dc == 1)  wallV[c + 1, r] = false;   // East
            if (dc == -1) wallV[c,     r] = false;    // West
            if (dr == 1)  wallH[c, r + 1] = false;   // North
            if (dr == -1) wallH[c, r    ] = false;    // South

            DFS(nc, nr, visited, wallH, wallV);
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────────
    private Vector3 CellCenter(int c, int r)
        => new Vector3(c * CELL_SIZE + CELL_SIZE * 0.5f, 0.5f,
                       r * CELL_SIZE + CELL_SIZE * 0.5f);

    private GameObject CreateBox(string name, Transform parent,
                                 Vector3 pos, Vector3 scale, Material mat)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name             = name;
        go.transform.parent = parent;
        go.transform.position    = pos;
        go.transform.localScale  = scale;
        if (mat != null)
            go.GetComponent<Renderer>().sharedMaterial = mat;
        return go;
    }

    private void PlaceMarker(string label, Transform parent, Vector3 pos, Color c)
    {
        var go           = new GameObject($"[{label}]");
        go.transform.parent   = parent;
        go.transform.position = pos;

        // Visible sphere in Scene view
        #if UNITY_EDITOR
        // Add a small sphere just for visual reference
        var sphere           = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.name          = label + "_Visual";
        sphere.transform.parent        = go.transform;
        sphere.transform.localPosition = Vector3.zero;
        sphere.transform.localScale    = Vector3.one * 0.5f;
        DestroyImmediate(sphere.GetComponent<Collider>());
        sphere.GetComponent<Renderer>().sharedMaterial = CreateColorMat(c);
        #endif
    }

    private Material CreateColorMat(Color c)
    {
        var mat   = new Material(Shader.Find("Standard"));
        mat.color = c;
        return mat;
    }

    private void ClearMaze()
    {
        var existing = GameObject.Find("Maze");
        if (existing != null)
        {
            Undo.DestroyObjectImmediate(existing);
        }
    }
}
#endif
