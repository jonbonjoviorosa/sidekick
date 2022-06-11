using Microsoft.EntityFrameworkCore;
using Sidekick.Api.DataAccessLayer.Interfaces;
using Sidekick.Api.Helpers;
using Sidekick.Api.Helpers.IHelpers;
using Sidekick.Model.PaymentMethod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sidekick.Api.DataAccessLayer.Repositories
{
    public class PaymentMethodRepository: IPaymentMethodRepository
    {
        private readonly APIDBContext context;
        private readonly IUserHelper userHelper;

        public PaymentMethodRepository(APIDBContext context,
            IUserHelper userHelper)
        {
            this.context = context;
            this.userHelper = userHelper;
        }

        public async Task<PaymentMethod_Card> GetPaymentMethodCard(Guid paymentMethodCardId)
        {
            var data = await context.PaymentMehod_Cards
                .Where(x => x.PaymentMethod_CardId == paymentMethodCardId)
                .FirstOrDefaultAsync();
            if (data == null)
            {
                return null;
            }
            return data;
        }

        public async Task<PaymentMethod_Card> GetPaymentMethodCardForUser(Guid paymentMethodCardId)
        {
            var data = await context.PaymentMehod_Cards
                .Where(x => x.UserId == paymentMethodCardId)
                .FirstOrDefaultAsync();
            if (data == null)
            {
                return null;
            }
            return data;
        }


        public async Task<IEnumerable<PaymentMethod_Card>> GetPaymentMethodCards(Guid userId)
        {
            var encryptedData = await context.PaymentMehod_Cards
                .Where(x => x.UserId == userId)
                .ToListAsync();
            
            //var list = new List<PaymentMethod_Card>();
            //foreach (var data in encryptedData)
            //{
            //    list.Add(data);
            //}
            return encryptedData;
        }

        public async Task InsertUpdatePaymentMethodCard(PaymentMethod_Card paymentMethod)
        {
            var currentLogin = userHelper.GetCurrentUserGuidLogin();
            var dateNow = Helper.GetDateTime();
            var getPaymentMethod = await GetPaymentMethodCardForUser(currentLogin);
            if (getPaymentMethod != null)
            {
                getPaymentMethod.FullName = paymentMethod.FullName;
                getPaymentMethod.CardNumber = paymentMethod.CardNumber;
                getPaymentMethod.ExpirationDate_Month = paymentMethod.ExpirationDate_Month;
                getPaymentMethod.ExpirationDate_Year = paymentMethod.ExpirationDate_Year;
                getPaymentMethod.CardType = paymentMethod.CardType;
                getPaymentMethod.CVV = string.Empty;
                //getPaymentMethod.PostCode = Helper.Encrypt(paymentMethod.PostCode);
                getPaymentMethod.LastEditedBy = currentLogin;
                getPaymentMethod.LastEditedDate = dateNow;
                context.PaymentMehod_Cards.Update(getPaymentMethod);
            }
            else
            {
                getPaymentMethod = new PaymentMethod_Card();
                getPaymentMethod.UserId = currentLogin;
                getPaymentMethod.PaymentMethod_CardId = Guid.NewGuid();
                getPaymentMethod.FullName = paymentMethod.FullName;
                getPaymentMethod.CardNumber = paymentMethod.CardNumber;
                getPaymentMethod.ExpirationDate_Month = paymentMethod.ExpirationDate_Month;
                getPaymentMethod.ExpirationDate_Year = paymentMethod.ExpirationDate_Year;
                getPaymentMethod.CVV = string.Empty;
                getPaymentMethod.CardType = paymentMethod.CardType;
                //getPaymentMethod.PostCode = Helper.Encrypt(paymentMethod.PostCode);
                getPaymentMethod.CreatedBy = currentLogin;
                getPaymentMethod.CreatedDate = dateNow;
                context.PaymentMehod_Cards.Add(getPaymentMethod);
            }
            await context.SaveChangesAsync();
        }

        public async Task RemovePaymentMethodCard(Guid paymentMethodCardId)
        {
            var getPaymentMethod = await GetPaymentMethodCard(paymentMethodCardId);
            if (getPaymentMethod != null)
            {
                context.PaymentMehod_Cards.Remove(getPaymentMethod);
                await context.SaveChangesAsync();
            }
        }

        private PaymentMethod_Card DecryptCardData(PaymentMethod_Card card)
        {
            card.PaymentMethod_CardId = card.PaymentMethod_CardId;
            card.FullName = Helper.Decrypt(card.FullName);
            card.CardNumber = Helper.Decrypt(card.CardNumber);
            card.ExpirationDate_Month = Helper.Decrypt(card.ExpirationDate_Month);
            card.ExpirationDate_Year = Helper.Decrypt(card.ExpirationDate_Year);
            card.CVV = Helper.Decrypt(card.CVV);
            return card;

        }
    }
}
