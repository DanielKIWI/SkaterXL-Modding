// Changed Functions:

public void Awake() {
    //...
    //Added Lines:
    GameObject gameObject = new GameObject();
    gameObject.AddComponent<SkateTrainer>();
    UnityEngine.Object.DontDestroyOnLoad(gameObject);
    //...
}