using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace BottApp.Database.Document.Like;


public class LikedDocumentRepository : AbstractRepository<LikedDocumentModel>, ILikedDocumentRepository
{
    public LikedDocumentRepository(PostgreSqlContext context, ILoggerFactory loggerFactory) : base(context, loggerFactory)
    {
    }


    public async Task<LikedDocumentModel> CreateModel(int userId, int documentId, bool isLiked)
    {
        var model = LikedDocumentModel.CreateModel(userId, documentId, isLiked);

        var result = await CreateModelAsync(model);
        if (result == null)
        {
            throw new Exception($"Document is not liked. User id {userId} tried to like document id {documentId}");
        }

        return result;
    }
}