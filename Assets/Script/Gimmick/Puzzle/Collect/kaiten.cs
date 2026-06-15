using UnityEngine;

public class kaiten : MonoBehaviour
{
    [Header("‰ñ“]İ’è")]
    public Vector3 rotationSpeed = new Vector3(0, 0, 0); //1•bŠÔ‚Ì‰ñ“]Šp“x

    void Update()
    {
        //–ˆƒtƒŒ[ƒ€­‚µ‚¸‚Â‰ñ“]
        transform.Rotate(rotationSpeed * Time.deltaTime);
    }
}