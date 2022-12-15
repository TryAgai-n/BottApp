using System.Collections.Generic;
using System.Threading.Tasks;

namespace BottApp.Database.Document.Statistic;

public interface IDocumentStatisticRepository
{
    Task<List<DocumentStatisticModel>> ListTopDocuments(int skip, int take = 10);
    Task<DocumentStatisticModel> CreateModel(int documentId, int viewCount, int likeCount);
}