using UnityEngine;
using ComponentCacheGenerator;

[GenerateComponentCache(typeof(Rigidbody), "rb")]
[GenerateComponentCache(typeof(SampleComponent), "sample", SearchScope = ComponentSearchScope.Children)]
public partial class Sandbox : MonoBehaviour
{
    void FixedUpdate()
    {
        rb.velocity = Vector2.one;
        sample.SayHello();
    }
}