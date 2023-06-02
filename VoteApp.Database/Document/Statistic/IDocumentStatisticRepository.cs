using System.Threading.Tasks;

namespace VoteApp.Database.Document.Statistic;

public interface IDocumentStatisticRepository
{
    Task<DocumentStatisticModel> CreateModel(int viewCount, int likeCount);
    

}