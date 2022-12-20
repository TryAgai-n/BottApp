using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BottApp.Database.User;

namespace BottApp.Database.Document;

public interface IDocumentRepository
{
    Task<DocumentModel> CreateModel(
        int userId,
        string? documentType,
        string? documentExtension,
        DateTime createdAt,
        string? path,
        string? caption,
        DocumentInPath documentInPath,
        InNomination? documentNomination
    );


    Task<DocumentModel> GetOneByDocumentId(int documentId);
    
    Task<DocumentModel> GetFirstDocumentByPath(DocumentInPath documentInPath);
    Task<DocumentModel> GetFirstDocumentByNomination(InNomination? documentNomination);
    Task<List<DocumentModel>> ListDocumentsByPath(DocumentInPath documentInPath);
    Task<List<DocumentModel>> ListDocumentsByNomination(InNomination? documentNomination, int skip, int take = 1, bool withOrderByView = false);
    Task<List<DocumentModel?>> GetListByNomination(DocumentModel documentModel);
    Task<bool> CheckSingleDocumentInNominationByUser(UserModel user, InNomination? documentNomination);
    Task<int> GetCountByNomination(InNomination? documentNomination);
    Task<List<DocumentModel>> ListMostViewedDocuments(int skip = 0, int take = 10);

    Task IncrementViewByDocument(DocumentModel model);
    Task IncrementLikeByDocument(DocumentModel model);
}