using UnityEngine;

public class PlayerSpawnManager : MonoBehaviour
{
    void Start()
    {
        Vector3? savedPosition = SceneManager.Instance.GetAndClearSavedPosition();

        if (savedPosition.HasValue)
        {
            Vector3 originalPosition = savedPosition.Value;
            Vector3 newPosition = new Vector3(originalPosition.x,
                                            originalPosition.y,
                                            originalPosition.z - 1f);

            transform.position = newPosition;
        }
    }
}