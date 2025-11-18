using System.Threading.Tasks;

namespace AdventureGame.Services
{
    public interface IEmbeddingModel
    {
        Task<float[]> GetEmbeddingAsync(long[] ids, long[] mask, long[] tokenTypes);
    }
}
