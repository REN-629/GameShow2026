//耐久0になったオブジェクトを破片化する

using UnityEngine;
using Random = System.Random;

namespace Project.Scripts.Fractures
{
    public class FractureThis : MonoBehaviour
    {
        [Header("破砕設定")]

        [Tooltip("どこを固定するか。壁ならBottomが扱いやすい")]
        [SerializeField] private Anchor anchor = Anchor.Bottom;

        [Tooltip("破片の数。多いほど細かいが重い。最初は30〜80推奨")]
        [SerializeField] private int chunks = 60;

        [Tooltip("破片の密度、重さ")]
        [SerializeField] private float density = 50f;

        [Tooltip("破片同士のつながり強度")]
        [SerializeField] private float internalStrength = 100f;

        [Header("破砕マテリアル")]

        [Tooltip("割れた内側に使うマテリアル")]
        [SerializeField] private Material insideMaterial;

        [Tooltip("外側に使うマテリアル")]
        [SerializeField] private Material outsideMaterial;

        [Header("元の見た目")]

        [Tooltip("破砕後に非表示にする元モデル。Stone/model みたいな子を入れる")]
        public GameObject originalVisualRoot;

        [Tooltip("Original Visual Rootが未設定なら、子の名前 model を自動で探す")]
        public bool autoFindModelChild = true;

        [Header("破砕後")]

        [Tooltip("破砕後に元のRendererをOFFにする")]
        public bool disableOriginalRenderers = true;

        [Tooltip("破砕後に元のColliderをOFFにする")]
        public bool disableOriginalColliders = true;

        [Tooltip("破砕後にoriginalVisualRootをSetActive(false)")]
        public bool disableOriginalVisualRootObject = true;

        [Tooltip("OFF推奨")]
        public bool hideRootObjectAfterFracture = false;

        [Tooltip("同じオブジェクトを一度だけ破砕する")]
        public bool fractureOnlyOnce = true;


        [Header("デバッグ")]
        public bool debugLog = true;

        private Random rng = new Random();
        private bool hasFractured = false;

        private void Start()
        {
            FractureGameobject();
            TryAutoFindOriginalVisualRoot();
        }

        // AttributeDurabilityから呼ばれる入口。
        public ChunkGraphManager BreakNow()
        {
            if (fractureOnlyOnce && hasFractured)
                return null;

            hasFractured = true;

            TryAutoFindOriginalVisualRoot();

            //重要
            //破砕する前に元から存在していたRenderer/Colliderだけを覚える
            //破砕後に探すと新しく生成された破片まで混ざる可能性がある
            Renderer[] originalRenderers = GetOriginalRenderers();
            Collider[] originalColliders = GetOriginalColliders();

            if (debugLog)
            {
                Debug.Log(
                    name
                    + " をFractureで破砕します / chunks="
                    + chunks
                    + " / originalVisualRoot="
                    + (originalVisualRoot != null ? originalVisualRoot.name : "null")
                    + " / originalRenderers="
                    + originalRenderers.Length
                );
            }

            ChunkGraphManager manager = FractureGameobject();

            //元ColliderをOFF
            if (disableOriginalColliders)
            {
                DisableColliders(originalColliders);
            }

            //元RendererをOFF
            if (disableOriginalRenderers)
            {
                DisableRenderers(originalRenderers);
            }

            //modelオブジェクト自体も非表示
            if (disableOriginalVisualRootObject && originalVisualRoot != null)
            {
                originalVisualRoot.SetActive(false);
            }

            //Root全体を消すのは通常OFF推奨。
            //Fracture側が破片をこのRoot配下に作る場合、Rootごと消すと破片まで消えるため。
            if (hideRootObjectAfterFracture)
            {
                gameObject.SetActive(false);
            }

            return manager;
        }

        //実際の破砕処理。
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
                {
                    Debug.Log(name + " の originalVisualRoot を自動設定: " + originalVisualRoot.name);
                }
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
