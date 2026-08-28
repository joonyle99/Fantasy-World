using System.IO;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace FantasyWorld.EditorTools
{
    /// <summary>
    /// 선택한 그룹의 자식 메시들을 하나의 콜리전 메시로 합쳐 그룹 루트에 MeshCollider 하나만 남긴다.
    /// 렌더러(MeshFilter/MeshRenderer)는 그대로 두고 자식의 Collider 만 제거한다.
    /// 정적 환경(지형·바위·울타리)의 콜라이더 개수를 줄이는 용도.
    /// </summary>
    public static class CollisionMeshCombiner
    {
        private const string OUTPUT_FOLDER = "Assets/Project Assets/Prefabs/Collision";

        [MenuItem("Tools/콜라이더 개수 세기 (선택한 그룹)")]
        private static void CountSelection()
        {
            var root = Selection.activeGameObject;
            if (root == null)
            {
                Debug.LogWarning("셀 그룹을 하나 선택하세요.");
                return;
            }

            int mesh = 0, box = 0, capsule = 0, sphere = 0, other = 0;
            foreach (var collider in root.GetComponentsInChildren<Collider>(true))
            {
                if (collider is MeshCollider) mesh++;
                else if (collider is BoxCollider) box++;
                else if (collider is CapsuleCollider) capsule++;
                else if (collider is SphereCollider) sphere++;
                else other++;
            }

            var total = mesh + box + capsule + sphere + other;
            Debug.Log($"'{root.name}' 콜라이더 총 {total}개 — Mesh {mesh}, Box {box}, Capsule {capsule}, Sphere {sphere}, 기타 {other}");
        }

        [MenuItem("Tools/콜라이더 합치기 (선택한 그룹)")]
        private static void CombineSelection()
        {
            var root = Selection.activeGameObject;
            if (root == null)
            {
                Debug.LogWarning("합칠 그룹(부모 오브젝트)을 하나 선택하세요.");
                return;
            }

            var filters = root.GetComponentsInChildren<MeshFilter>(true);
            var instances = new List<CombineInstance>();
            var worldToRoot = root.transform.worldToLocalMatrix;

            foreach (var filter in filters)
            {
                var mesh = filter.sharedMesh;
                if (mesh == null || filter.gameObject == root)
                    continue;

                for (var sub = 0; sub < mesh.subMeshCount; sub++)
                {
                    instances.Add(new CombineInstance
                    {
                        mesh = mesh,
                        subMeshIndex = sub,
                        transform = worldToRoot * filter.transform.localToWorldMatrix,
                    });
                }
            }

            if (instances.Count == 0)
            {
                Debug.LogWarning($"'{root.name}' 아래에서 합칠 메시를 찾지 못했습니다.");
                return;
            }

            var combined = new Mesh
            {
                name = $"{root.name}_Collision",
                indexFormat = UnityEngine.Rendering.IndexFormat.UInt32,
            };
            combined.CombineMeshes(instances.ToArray(), true, true);
            combined.RecalculateBounds();

            if (!Directory.Exists(OUTPUT_FOLDER))
                Directory.CreateDirectory(OUTPUT_FOLDER);

            var assetPath = $"{OUTPUT_FOLDER}/{root.name}_Collision.asset";
            AssetDatabase.CreateAsset(combined, AssetDatabase.GenerateUniqueAssetPath(assetPath));

            // 자식 콜라이더 제거
            var removed = 0;
            foreach (var collider in root.GetComponentsInChildren<Collider>(true))
            {
                if (collider.gameObject == root)
                    continue;

                Undo.DestroyObjectImmediate(collider);
                removed++;
            }

            // 루트에 병합 콜라이더 하나
            var rootCollider = root.GetComponent<MeshCollider>();
            if (rootCollider == null)
                rootCollider = Undo.AddComponent<MeshCollider>(root);

            rootCollider.sharedMesh = combined;
            rootCollider.convex = false;

            EditorUtility.SetDirty(root);
            AssetDatabase.SaveAssets();

            Debug.Log($"'{root.name}': 자식 콜라이더 {removed}개 제거 → 병합 MeshCollider 1개 ({combined.triangles.Length / 3} tris). 메시: {assetPath}");
        }
    }
}
