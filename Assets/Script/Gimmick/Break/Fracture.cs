// Fracture.cs：メッシュを破片化する本体
//
// 今回の追加点：
// ・破片側のRigidbody設定を調整して、プレイヤーに押されやすくする
// ・破片のDrag/Angular Drag/Collision Detection/Interpolationを設定
//
// 注意：
// CharacterControllerのプレイヤーはRigidbodyを自動で強く押さないことがある。
// その場合は同梱の CharacterPushRigidbodies.cs をPlayerに付ける。

using System.Collections.Generic;
using System.Linq;
using Project.Scripts.Utils;
using UnityEngine;

namespace Project.Scripts.Fractures
{
    public static class Fracture
    {
        public static ChunkGraphManager FractureGameObject(
            GameObject gameObject,
            Anchor anchor,
            int seed,
            int totalChunks,
            Material insideMaterial,
            Material outsideMaterial,
            float jointBreakForce,
            float density)
        {
            Mesh mesh = GetWorldMesh(gameObject);

            NvBlastExtUnity.setSeed(seed);

            var nvMesh = new NvMesh(
                mesh.vertices,
                mesh.normals,
                mesh.uv,
                mesh.vertexCount,
                mesh.GetIndices(0),
                (int)mesh.GetIndexCount(0)
            );

            List<Mesh> meshes = FractureMeshesInNvblast(totalChunks, nvMesh);
            float chunkMass = mesh.Volume() * density / totalChunks;

            List<GameObject> chunks =
                BuildChunks(insideMaterial, outsideMaterial, meshes, chunkMass);

            foreach (GameObject chunk in chunks)
            {
                ConnectTouchingChunks(chunk, jointBreakForce);
            }

            AnchorChunks(gameObject, anchor);

            var fractureGameObject = new GameObject("Fracture");

            foreach (GameObject chunk in chunks)
            {
                // worldPositionStays = true
                // 破片のワールド位置を維持したまま親に入れる。
                chunk.transform.SetParent(fractureGameObject.transform, true);
            }

            ChunkGraphManager graphManager =
                fractureGameObject.AddComponent<ChunkGraphManager>();

            graphManager.Setup(fractureGameObject.GetComponentsInChildren<Rigidbody>());

            return graphManager;
        }

        private static void AnchorChunks(GameObject gameObject, Anchor anchor)
        {
            Transform transform = gameObject.transform;
            Bounds bounds = gameObject.GetCompositeMeshBounds();

            IEnumerable<Collider> anchoredColliders =
                GetAnchoredColliders(anchor, transform, bounds);

            foreach (Collider collider in anchoredColliders)
            {
                if (collider == null)
                    continue;

                Rigidbody chunkRb = collider.GetComponent<Rigidbody>();

                if (chunkRb != null)
                    chunkRb.isKinematic = true;
            }
        }

        private static List<GameObject> BuildChunks(
            Material insideMaterial,
            Material outsideMaterial,
            List<Mesh> meshes,
            float chunkMass)
        {
            return meshes.Select((chunkMesh, i) =>
            {
                GameObject chunk =
                    BuildChunk(insideMaterial, outsideMaterial, chunkMesh, chunkMass);

                chunk.name += " [" + i + "]";
                return chunk;
            }).ToList();
        }

        private static List<Mesh> FractureMeshesInNvblast(int totalChunks, NvMesh nvMesh)
        {
            var fractureTool = new NvFractureTool();

            fractureTool.setRemoveIslands(false);
            fractureTool.setSourceMesh(nvMesh);

            var sites = new NvVoronoiSitesGenerator(nvMesh);
            sites.uniformlyGenerateSitesInMesh(totalChunks);

            fractureTool.voronoiFracturing(0, sites);
            fractureTool.finalizeFracturing();

            int meshCount = fractureTool.getChunkCount();

            var meshes = new List<Mesh>(fractureTool.getChunkCount());

            for (int i = 1; i < meshCount; i++)
            {
                meshes.Add(ExtractChunkMesh(fractureTool, i));
            }

            return meshes;
        }

        private static IEnumerable<Collider> GetAnchoredColliders(
            Anchor anchor,
            Transform meshTransform,
            Bounds bounds)
        {
            var anchoredChunks = new HashSet<Collider>();
            float frameWidth = .01f;

            Vector3 meshWorldCenter = meshTransform.TransformPoint(bounds.center);
            Vector3 meshWorldExtents = bounds.extents.Multiply(meshTransform.lossyScale);

            if (anchor.HasFlag(Anchor.Left))
            {
                Vector3 center =
                    meshWorldCenter - meshTransform.right * meshWorldExtents.x;

                Vector3 halfExtents =
                    meshWorldExtents.Abs().SetX(frameWidth);

                anchoredChunks.UnionWith(
                    Physics.OverlapBox(center, halfExtents, meshTransform.rotation)
                );
            }

            if (anchor.HasFlag(Anchor.Right))
            {
                Vector3 center =
                    meshWorldCenter + meshTransform.right * meshWorldExtents.x;

                Vector3 halfExtents =
                    meshWorldExtents.Abs().SetX(frameWidth);

                anchoredChunks.UnionWith(
                    Physics.OverlapBox(center, halfExtents, meshTransform.rotation)
                );
            }

            if (anchor.HasFlag(Anchor.Bottom))
            {
                Vector3 center =
                    meshWorldCenter - meshTransform.up * meshWorldExtents.y;

                Vector3 halfExtents =
                    meshWorldExtents.Abs().SetY(frameWidth);

                anchoredChunks.UnionWith(
                    Physics.OverlapBox(center, halfExtents, meshTransform.rotation)
                );
            }

            if (anchor.HasFlag(Anchor.Top))
            {
                Vector3 center =
                    meshWorldCenter + meshTransform.up * meshWorldExtents.y;

                Vector3 halfExtents =
                    meshWorldExtents.Abs().SetY(frameWidth);

                anchoredChunks.UnionWith(
                    Physics.OverlapBox(center, halfExtents, meshTransform.rotation)
                );
            }

            if (anchor.HasFlag(Anchor.Front))
            {
                Vector3 center =
                    meshWorldCenter - meshTransform.forward * meshWorldExtents.z;

                Vector3 halfExtents =
                    meshWorldExtents.Abs().SetZ(frameWidth);

                anchoredChunks.UnionWith(
                    Physics.OverlapBox(center, halfExtents, meshTransform.rotation)
                );
            }

            if (anchor.HasFlag(Anchor.Back))
            {
                Vector3 center =
                    meshWorldCenter + meshTransform.forward * meshWorldExtents.z;

                Vector3 halfExtents =
                    meshWorldExtents.Abs().SetZ(frameWidth);

                anchoredChunks.UnionWith(
                    Physics.OverlapBox(center, halfExtents, meshTransform.rotation)
                );
            }

            return anchoredChunks;
        }

        private static Mesh ExtractChunkMesh(NvFractureTool fractureTool, int index)
        {
            var outside = fractureTool.getChunkMesh(index, false);
            var inside = fractureTool.getChunkMesh(index, true);

            Mesh chunkMesh = outside.toUnityMesh();

            chunkMesh.subMeshCount = 2;
            chunkMesh.SetIndices(inside.getIndexes(), MeshTopology.Triangles, 1);

            chunkMesh.RecalculateBounds();

            return chunkMesh;
        }

        private static Mesh GetWorldMesh(GameObject gameObject)
        {
            CombineInstance[] combineInstances = gameObject
                .GetComponentsInChildren<MeshFilter>()
                .Where(mf => mf != null && ValidateMesh(mf.mesh))
                .Select(mf => new CombineInstance()
                {
                    mesh = mf.mesh,
                    transform = mf.transform.localToWorldMatrix
                })
                .ToArray();

            var totalMesh = new Mesh();

            totalMesh.CombineMeshes(combineInstances, true);
            totalMesh.RecalculateBounds();

            return totalMesh;
        }

        private static bool ValidateMesh(Mesh mesh)
        {
            if (mesh == null)
            {
                Debug.LogError("Mesh is null.");
                return false;
            }

            if (mesh.isReadable == false)
            {
                Debug.LogError("Mesh [" + mesh + "] has to be readable.");
                return false;
            }

            if (mesh.vertices == null || mesh.vertices.Length == 0)
            {
                Debug.LogError("Mesh [" + mesh + "] does not have any vertices.");
                return false;
            }

            if (mesh.uv == null || mesh.uv.Length == 0)
            {
                Debug.LogError("Mesh [" + mesh + "] does not have any uvs.");
                return false;
            }

            return true;
        }

        private static GameObject BuildChunk(
            Material insideMaterial,
            Material outsideMaterial,
            Mesh mesh,
            float mass)
        {
            var chunk = new GameObject("Chunk");

            // 各破片が自身の中心で縮小されるよう、Meshの中心をChunkのTransform位置にする。
            Vector3 chunkWorldCenter = RecenterMeshPivot(mesh);
            chunk.transform.position = chunkWorldCenter;
            chunk.transform.rotation = Quaternion.identity;
            chunk.transform.localScale = Vector3.one;

            MeshRenderer renderer = chunk.AddComponent<MeshRenderer>();

            renderer.sharedMaterials = new[]
            {
                outsideMaterial,
                insideMaterial
            };

            MeshFilter meshFilter = chunk.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = mesh;

            Rigidbody rigidbody = chunk.AddComponent<Rigidbody>();

            // 破片が重すぎるとプレイヤーが押せないので、最低値と最大値で軽く制限。
            rigidbody.mass = Mathf.Clamp(mass, 0.05f, 8f);

            // 破片が永遠に滑り続けないように少しだけ抵抗を入れる。
            rigidbody.linearDamping = 0.2f;
            rigidbody.angularDamping = 0.05f;

            // 破片がすり抜けにくくなる。
            rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

            // 見た目のカクつきを少し減らす。
            rigidbody.interpolation = RigidbodyInterpolation.Interpolate;

            MeshCollider meshCollider = chunk.AddComponent<MeshCollider>();
            meshCollider.sharedMesh = mesh;
            meshCollider.inflateMesh = true;
            meshCollider.convex = true;

            // プレイヤーと衝突するLayerにする。
            // DefaultとPlayerの衝突がPhysics設定でONなら押せる。
            chunk.layer = LayerMask.NameToLayer("Default");

            return chunk;
        }

        private static Vector3 RecenterMeshPivot(Mesh mesh)
        {
            mesh.RecalculateBounds();

            Vector3 center = mesh.bounds.center;
            Vector3[] vertices = mesh.vertices;

            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] -= center;
            }

            mesh.vertices = vertices;
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();

            return center;
        }

        private static void ConnectTouchingChunks(
            GameObject chunk,
            float jointBreakForce,
            float touchRadius = .01f)
        {
            Rigidbody rb = chunk.GetComponent<Rigidbody>();
            MeshFilter meshFilter = chunk.GetComponent<MeshFilter>();

            if (rb == null || meshFilter == null || meshFilter.mesh == null)
                return;

            Mesh mesh = meshFilter.mesh;

            var overlaps = new HashSet<Rigidbody>();
            Vector3[] vertices = mesh.vertices;

            for (int i = 0; i < vertices.Length; i++)
            {
                Vector3 worldPosition =
                    chunk.transform.TransformPoint(vertices[i]);

                Collider[] hits =
                    Physics.OverlapSphere(worldPosition, touchRadius);

                for (int j = 0; j < hits.Length; j++)
                {
                    Collider hit = hits[j];

                    if (hit == null)
                        continue;

                    Rigidbody hitRb = hit.GetComponent<Rigidbody>();

                    if (hitRb == null)
                        continue;

                    overlaps.Add(hitRb);
                }
            }

            foreach (Rigidbody overlap in overlaps)
            {
                if (overlap == null)
                    continue;

                if (overlap.gameObject == chunk.gameObject)
                    continue;

                FixedJoint joint =
                    overlap.gameObject.AddComponent<FixedJoint>();

                joint.connectedBody = rb;
                joint.breakForce = jointBreakForce;
            }
        }
    }
}
