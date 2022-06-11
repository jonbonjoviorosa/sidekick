using System;
using Sidekick.Model;

namespace Sidekick.Api.DataAccessLayer.Interfaces
{
    public interface IFAQsRepository
    {
        APIResponse Add(string _auth, FAQsDto _fAQ);
        APIResponse Edit(string _auth, FAQsDto _fAQ);
        APIResponse List(Guid? _fAQsId = null);

        APIResponse Status(string _auth, FAQStatus _fAQ);

        APIResponse ViewLegalDoc(ELegalDocType _type);

        APIResponse AddEditLegalDoc(string _auth, LegalDocumentDto _legal);
    }
}
