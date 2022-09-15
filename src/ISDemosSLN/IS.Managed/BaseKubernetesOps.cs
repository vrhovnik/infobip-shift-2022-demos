using k8s;

namespace IS.Managed;

public abstract class BaseKubernetesOps
{
    internal readonly IKubernetes client;

    protected BaseKubernetesOps(IKubernetes client)
    {
        this.client = client;
    }
}