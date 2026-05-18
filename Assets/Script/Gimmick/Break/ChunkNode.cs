// ChunkNode：破片同士のつながりを管理するノード
// 修正点：Joint / Rigidbody / connectedBody がnullでも落ちにくいように安全化。

using System.Collections.Generic;
using System.Linq;
using Project.Scripts.Utils;
using UnityEngine;

namespace Project.Scripts.Fractures
{
    public class ChunkNode : MonoBehaviour
    {
        public HashSet<ChunkNode> Neighbours = new HashSet<ChunkNode>();
        public ChunkNode[] NeighboursArray = new ChunkNode[0];

        private Dictionary<Joint, ChunkNode> JointToChunk = new Dictionary<Joint, ChunkNode>();
        private Dictionary<ChunkNode, Joint> ChunkToJoint = new Dictionary<ChunkNode, Joint>();

        private Rigidbody rb;
        private Vector3 frozenPos;
        private Quaternion frozenRot;
        private bool frozen;

        public bool IsStatic => rb != null && rb.isKinematic;
        public Color Color { get; set; } = Color.black;
        public bool HasBrokenLinks { get; private set; }

        private bool Contains(ChunkNode chunkNode)
        {
            return Neighbours.Contains(chunkNode);
        }

        private void FixedUpdate()
        {
            if (frozen)
            {
                transform.position = frozenPos;
                transform.rotation = frozenRot;
            }
        }

        public void Setup()
        {
            rb = GetComponent<Rigidbody>();

            if (rb == null)
            {
                Debug.LogWarning(name + " に Rigidbody が無いため ChunkNode.Setup を中断");
                return;
            }

            Freeze();

            JointToChunk.Clear();
            ChunkToJoint.Clear();

            foreach (Joint joint in GetComponents<Joint>())
            {
                if (joint == null)
                    continue;

                if (joint.connectedBody == null)
                    continue;

                ChunkNode chunk = joint.connectedBody.GetOrAddComponent<ChunkNode>();

                JointToChunk[joint] = chunk;
                ChunkToJoint[chunk] = joint;
            }

            foreach (ChunkNode chunkNode in ChunkToJoint.Keys)
            {
                if (chunkNode == null)
                    continue;

                Neighbours.Add(chunkNode);

                if (chunkNode.Contains(this) == false)
                    chunkNode.Neighbours.Add(this);
            }

            NeighboursArray = Neighbours.ToArray();
        }

        private void OnJointBreak(float breakForce)
        {
            HasBrokenLinks = true;
        }

        public void CleanBrokenLinks()
        {
            List<Joint> brokenLinks = JointToChunk.Keys.Where(j => j == null).ToList();

            foreach (Joint link in brokenLinks)
            {
                ChunkNode body = JointToChunk[link];

                JointToChunk.Remove(link);

                if (body != null)
                {
                    ChunkToJoint.Remove(body);
                    body.Remove(this);
                    Neighbours.Remove(body);
                }
            }

            NeighboursArray = Neighbours.ToArray();
            HasBrokenLinks = false;
        }

        private void Remove(ChunkNode chunkNode)
        {
            if (chunkNode == null)
                return;

            ChunkToJoint.Remove(chunkNode);
            Neighbours.Remove(chunkNode);
            NeighboursArray = Neighbours.ToArray();
        }

        public void Unfreeze()
        {
            frozen = false;

            if (rb == null)
                return;

            rb.constraints = RigidbodyConstraints.None;
            rb.useGravity = true;
            rb.gameObject.layer = LayerMask.NameToLayer("Default");
        }

        private void Freeze()
        {
            if (rb == null)
                return;

            frozen = true;
            rb.constraints = RigidbodyConstraints.FreezeAll;
            rb.useGravity = false;
            rb.gameObject.layer = LayerMask.NameToLayer("FrozenChunks");
            frozenPos = rb.transform.position;
            frozenRot = rb.transform.rotation;
        }

        private void OnDrawGizmos()
        {
            Rigidbody body = GetComponent<Rigidbody>();

            if (body == null)
                return;

            Vector3 worldCenterOfMass = transform.TransformPoint(body.centerOfMass);

            if (IsStatic)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(worldCenterOfMass, 0.05f);
            }
            else
            {
                Gizmos.color = Color.SetAlpha(0.5f);
                Gizmos.DrawSphere(worldCenterOfMass, 0.1f);
            }

            foreach (Joint joint in JointToChunk.Keys)
            {
                if (joint && joint.connectedBody != null && rb != null)
                {
                    Vector3 from = transform.TransformPoint(rb.centerOfMass);
                    Vector3 to = joint.connectedBody.transform.TransformPoint(joint.connectedBody.centerOfMass);
                    Gizmos.color = Color;
                    Gizmos.DrawLine(from, to);
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            foreach (ChunkNode node in Neighbours)
            {
                if (node == null)
                    continue;

                MeshFilter meshFilter = node.GetComponent<MeshFilter>();

                if (meshFilter == null || meshFilter.mesh == null)
                    continue;

                Gizmos.color = Color.yellow.SetAlpha(.2f);
                Gizmos.DrawMesh(meshFilter.mesh, node.transform.position, node.transform.rotation);
            }
        }
    }
}
