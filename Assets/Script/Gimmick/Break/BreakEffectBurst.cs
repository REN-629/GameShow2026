// îjâÛÉGÉtÉFÉNÉgÅAîjï–ÇÉâÉìÉ_ÉÄÇ…îÚÇŒÇµç≈å„ÇÕèkè¨ÇµÇƒè¡Ç∑
using System.Collections;
using UnityEngine;
public class BreakEffectBurst : MonoBehaviour
{
    [Header("îÚÇ—éUÇË")]
    public float minForce = 2f;
    public float maxForce = 5f;
    public float upwardForce = 1.5f;
    public float randomDirectionAmount = 1.0f;
    [Header("âÒì]")]
    public float minTorque = 3f;
    public float maxTorque = 8f;
    [Header("è¡Ç¶ÇÈÇ‹Ç≈")]
    public float lifeTime = 3f;
    public float shrinkTime = 0.6f;
    void Start()
    {
        Rigidbody[] bodies = GetComponentsInChildren<Rigidbody>();
        foreach (Rigidbody rb in bodies)
        {
            Vector3 randomDir = Random.insideUnitSphere * randomDirectionAmount;
            Vector3 outwardDir = (rb.transform.position - transform.position).normalized;
            Vector3 finalDir = outwardDir + randomDir + Vector3.up * upwardForce;
            finalDir.Normalize();
            float force = Random.Range(minForce, maxForce);
            float torque = Random.Range(minTorque, maxTorque);
            rb.AddForce(finalDir * force, ForceMode.Impulse);
            rb.AddTorque(Random.insideUnitSphere * torque, ForceMode.Impulse);
        }
        StartCoroutine(ShrinkAndDestroy());
    }
    IEnumerator ShrinkAndDestroy()
    {
        yield return new WaitForSeconds(lifeTime);
        Transform[] pieces = GetComponentsInChildren<Transform>();
        Vector3[] originalScales = new Vector3[pieces.Length];
        for (int i = 0; i < pieces.Length; i++)
        {
            originalScales[i] = pieces[i].localScale;
        }
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(0.01f, shrinkTime);
            float scaleRate = 1f - t;
            for (int i = 0; i < pieces.Length; i++)
            {
                if (pieces[i] == transform)
                    continue;
                pieces[i].localScale = originalScales[i] * scaleRate;
            }
            yield return null;
        }
        Destroy(gameObject);
    }
}