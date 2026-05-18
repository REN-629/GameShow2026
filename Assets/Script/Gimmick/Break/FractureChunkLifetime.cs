// FractureChunkLifetime：Fractureで生成された破片を指定秒後に縮小して消す
//
// 各ChunkのPivotが正しく中心にある前提で、localScaleを使って縮小します。
// Fracture.cs側の RecenterMeshPivot() により、各破片は自身の中心で縮みます。

using System.Collections;
using UnityEngine;

namespace Project.Scripts.Fractures
{
    public class FractureChunkLifetime : MonoBehaviour
    {
        [Header("寿命")]
        public float lifeTime = 5f;
        public float shrinkTime = 1f;

        [Header("縮小開始時")]
        public bool freezeRigidbodyWhenShrink = true;

        [Header("最後に消す")]
        public bool destroyRootAfterShrink = true;

        [Header("デバッグ")]
        public bool debugLog = false;

        private Transform[] chunkTransforms;
        private Vector3[] originalScales;

        void Start()
        {
            CacheChunks();
            StartCoroutine(LifeRoutine());
        }

        void CacheChunks()
        {
            chunkTransforms = GetComponentsInChildren<Transform>(true);
            originalScales = new Vector3[chunkTransforms.Length];

            for (int i = 0; i < chunkTransforms.Length; i++)
            {
                originalScales[i] = chunkTransforms[i].localScale;
            }

            if (debugLog)
            {
                Debug.Log(name + " 破片寿命開始 / pieces=" + chunkTransforms.Length);
            }
        }

        IEnumerator LifeRoutine()
        {
            yield return new WaitForSeconds(lifeTime);

            if (freezeRigidbodyWhenShrink)
                FreezeRigidbodies();

            float t = 0f;

            while (t < 1f)
            {
                t += Time.deltaTime / Mathf.Max(0.01f, shrinkTime);

                float scaleRate = 1f - t;

                for (int i = 0; i < chunkTransforms.Length; i++)
                {
                    Transform tr = chunkTransforms[i];

                    if (tr == null)
                        continue;

                    if (tr == transform)
                        continue;

                    tr.localScale = originalScales[i] * scaleRate;
                }

                yield return null;
            }

            if (destroyRootAfterShrink)
            {
                Destroy(gameObject);
            }
            else
            {
                DisableRenderersAndColliders();
            }
        }

        void FreezeRigidbodies()
        {
            Rigidbody[] bodies = GetComponentsInChildren<Rigidbody>(true);

            foreach (Rigidbody rb in bodies)
            {
                if (rb == null)
                    continue;

                if (!rb.isKinematic)
                {
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }

                rb.isKinematic = true;
                rb.useGravity = false;
            }
        }

        void DisableRenderersAndColliders()
        {
            Renderer[] renderers = GetComponentsInChildren<Renderer>(true);

            foreach (Renderer r in renderers)
            {
                if (r != null)
                    r.enabled = false;
            }

            Collider[] colliders = GetComponentsInChildren<Collider>(true);

            foreach (Collider c in colliders)
            {
                if (c != null)
                    c.enabled = false;
            }
        }
    }
}
