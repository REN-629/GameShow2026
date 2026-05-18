// FractureThis：耐久0になった時にオブジェクトを破片化するスクリプト
// 全修正版：Startで破砕しない、元モデルを消す、破片を縮小して消す。

using UnityEngine;
using Random = System.Random;

namespace Project.Scripts.Fractures
{
    public class FractureThis : MonoBehaviour
    {
        [Header("破砕設定")]
        [SerializeField] private Anchor anchor = Anchor.Bottom;

        [Tooltip("破片数。多いほど細かいが重い。30〜80推奨")]
        [SerializeField] private int chunks = 60;

        [Tooltip("破片の密度・重さ")]
        [SerializeField] private float density = 50f;

        [Tooltip("破片同士のつながり強度")]
        [SerializeField] private float internalStrength = 100f;

        [Header("破砕マテリアル")]
        [SerializeField] private Material insideMaterial;
        [SerializeField] private Material outsideMaterial;

        [Header("元の見た目")]
        public GameObject originalVisualRoot;
        public bool autoFindModelChild = true;

        [Header("破砕後に元オブジェクトを消す設定")]
        public bool disableOriginalRenderers = true;
        public bool disableOriginalColliders = true;
        public bool disableOriginalVisualRootObject = true;

        [Tooltip("Root全体を非表示にする。破片まで消える場合があるので通常OFF推奨")]
        public bool hideRootObjectAfterFracture = false;

        public bool fractureOnlyOnce = true;

        [Header("破片の自動消滅")]
        public bool enableChunkLifetime = true;
        public float chunkLifeTime = 5f;
        public float chunkShrinkTime = 1f;
        public bool freezeChunksWhenShrink = true;
        public bool destroyChunksAfterShrink = true;

        [Header("デバッグ")]
        public bool debugLog = true;

        private Random rng = new Random();
        private bool hasFractured = false;

        private void Start()
        {
            TryAutoFindOriginalVisualRoot();
        }

        public ChunkGraphManager BreakNow()
        {
            if (fractureOnlyOnce && hasFractured)
                return null;

            hasFractured = true;
            TryAutoFindOriginalVisualRoot();

            Renderer[] originalRenderers = GetOriginalRenderers();
            Collider[] originalColliders = GetOriginalColliders();

            if (debugLog)
            {
                Debug.Log(name + " をFractureで破砕します / chunks=" + chunks);
            }

            ChunkGraphManager manager = FractureGameobject();

            SetupChunkLifetime(manager);

            if (disableOriginalColliders)
                DisableColliders(originalColliders);

            if (disableOriginalRenderers)
                DisableRenderers(originalRenderers);

            if (disableOriginalVisualRootObject && originalVisualRoot != null)
                originalVisualRoot.SetActive(false);

            if (hideRootObjectAfterFracture)
                gameObject.SetActive(false);

            return manager;
        }

        public ChunkGraphManager FractureGameobject()
        {
            int seed = rng.Next();

            return Fracture.FractureGameObject(
                gameObject,
                anchor,
                seed,
                chunks,
                insideMaterial,
                outsideMaterial,
                internalStrength,
                density
            );
        }

        void SetupChunkLifetime(ChunkGraphManager manager)
        {
            if (!enableChunkLifetime)
                return;

            if (manager == null)
            {
                if (debugLog)
                    Debug.LogWarning(name + " の破砕結果 manager がnull。破片寿命処理を追加できません。");

                return;
            }

            FractureChunkLifetime lifetime =
                manager.gameObject.GetComponent<FractureChunkLifetime>();

            if (lifetime == null)
                lifetime = manager.gameObject.AddComponent<FractureChunkLifetime>();

            lifetime.lifeTime = chunkLifeTime;
            lifetime.shrinkTime = chunkShrinkTime;
            lifetime.freezeRigidbodyWhenShrink = freezeChunksWhenShrink;
            lifetime.destroyRootAfterShrink = destroyChunksAfterShrink;
            lifetime.debugLog = debugLog;
        }

        void TryAutoFindOriginalVisualRoot()
        {
            if (originalVisualRoot != null)
                return;

            if (!autoFindModelChild)
                return;

            Transform model = transform.Find("model");

            if (model != null)
            {
                originalVisualRoot = model.gameObject;

                if (debugLog)
                    Debug.Log(name + " の originalVisualRoot を自動設定: " + originalVisualRoot.name);
            }
        }

        Renderer[] GetOriginalRenderers()
        {
            if (originalVisualRoot != null)
                return originalVisualRoot.GetComponentsInChildren<Renderer>(true);

            return GetComponentsInChildren<Renderer>(true);
        }

        Collider[] GetOriginalColliders()
        {
            if (originalVisualRoot != null)
                return originalVisualRoot.GetComponentsInChildren<Collider>(true);

            return GetComponentsInChildren<Collider>(true);
        }

        void DisableRenderers(Renderer[] renderers)
        {
            foreach (Renderer r in renderers)
            {
                if (r != null)
                    r.enabled = false;
            }
        }

        void DisableColliders(Collider[] colliders)
        {
            foreach (Collider c in colliders)
            {
                if (c != null)
                    c.enabled = false;
            }
        }
    }
}
