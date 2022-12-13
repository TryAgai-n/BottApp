using System.Threading.Tasks;

namespace BottApp.Database.Document.Statistic;

public interface IDocumentStatisticRepository
{
    Task<DocumentStatisticModel> CreateModel(int viewCount, int likeCount);

    Task IncrementViewById(int documentId, int viewCountIncrement = 1);


}