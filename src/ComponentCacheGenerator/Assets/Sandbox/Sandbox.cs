using UnityEngine;
using ComponentCacheGenerator;

[GenerateComponentCache(typeof(Rigidbody), PropertyName = "rb")]
[GenerateComponentCache(typeof(SampleComponent), PropertyName = "sample", SearchScope = ComponentSearchScope.Children)]
public partial class Sandbox : MonoBehaviour
{
    void Start()
    {
        CacheComponents();
    }

    void FixedUpdate()
    {
        rb.velocity = Vector2.one;
        sample.SayHello();
    }
}