public class PhysarumEnvironment
{
    public int[] agents; // each agent species has a index. 0 Means no agent on that spot
    public float[] chemicals;
    public int[] spec_mask;
    public int width;
    public int height;

    public PhysarumEnvironment(int w, int h)
    {
        agents = new int[w * h];
        chemicals = new float[w * h];
        spec_mask = new int[w * h];
        width = w;
        height = h;
    }

}
