using System.Threading.Tasks;

namespace BottApp.Database.Document.Like;

public interface ILikedDocumentRepository
{
    Task<LikedDocumentModel> CreateModel(int userId, int documentId, bool isLiked);
}