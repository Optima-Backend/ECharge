using AutoMapper;
using AutoMapper.QueryableExtensions;
using ECharge.Domain.Entities;
using ECharge.Domain.Repositories.Transaction.DTO;
using ECharge.Domain.Repositories.Transaction.Interface;
using ECharge.Domain.Repositories.Transaction.Query;
using ECharge.Infrastructure.Services.DatabaseContext;
using Microsoft.EntityFrameworkCore;

namespace ECharge.Infrastructure.Repositories.Transaction
{
    public class TransactionRepository : ITransactionRepository
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public TransactionRepository(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<BaseResponseWithPagination<AdminTransactionDTO>> GetAdminTransactions(TransactionQuery query)
        {
            IQueryable<Session> transactions = _context.Sessions.Include(x => x.Order);

            if (!string.IsNullOrEmpty(query.Id))
            {
                transactions = transactions.Where(x => x.Id == query.Id);
            }

            if (!string.IsNullOrEmpty(query.ChargerPointId))
            {
                transactions = transactions.Where(x => x.ChargerPointId == query.ChargerPointId);
            }

            if (query.StartDate.HasValue)
            {
                var nowUtc = DateTime.UtcNow;

                if (query.EndDate.HasValue)
                {
                    transactions = transactions.Where(x =>
                        x.StartDate.HasValue && x.StartDate >= query.StartDate.Value &&
                        x.EndDate.HasValue && x.EndDate <= query.EndDate.Value);
                }
                else
                {
                    transactions = transactions.Where(x =>
                        x.StartDate.HasValue && x.StartDate >= query.StartDate.Value);
                }
            }
            else if (query.EndDate.HasValue)
            {
                transactions = transactions.Where(x =>
                    x.EndDate.HasValue && x.EndDate <= query.EndDate.Value);
            }



            if (query.DurationInMinutes.HasValue)
            {
                transactions = transactions.Where(x => x.DurationInMinutes == query.DurationInMinutes);
            }

            if (query.PricePerHour.HasValue)
            {
                transactions = transactions.Where(x => x.PricePerHour == query.PricePerHour.Value);
            }

            if (query.SessionStatus.HasValue)
            {
                transactions = transactions.Where(x => x.Status == query.SessionStatus.Value);
            }

            if (query.UpdatedTime.HasValue)
            {
                transactions = transactions.Where(x => x.UpdatedTime.HasValue && x.UpdatedTime.Value.Date == query.UpdatedTime.Value.Date && x.UpdatedTime.Value.ToShortTimeString() == query.UpdatedTime.Value.ToShortTimeString());
            }

            if (!string.IsNullOrEmpty(query.UserId))
            {
                transactions = transactions.Where(x => !string.IsNullOrEmpty(x.UserId) && x.UserId == query.UserId);
            }

            if (!string.IsNullOrEmpty(query.Name))
            {
                transactions = transactions.Where(x => !string.IsNullOrEmpty(x.Name) && x.Name.Contains(query.Name));
            }

            if (!string.IsNullOrEmpty(query.Email))
            {
                transactions = transactions.Where(x => !string.IsNullOrEmpty(x.Email) && x.Email.Contains(query.Email));
            }

            if (!string.IsNullOrEmpty(query.ChargePointName))
            {
                transactions = transactions.Where(x => !string.IsNullOrEmpty(x.ChargePointName) && x.ChargePointName.Contains(query.ChargePointName));
            }

            if (query.MaxVoltage.HasValue)
            {
                transactions = transactions.Where(x => x.MaxVoltage.HasValue && x.MaxVoltage.Value == query.MaxVoltage.Value);
            }

            if (query.MaxAmperage.HasValue)
            {
                transactions = transactions.Where(x => x.MaxAmperage.HasValue && x.MaxAmperage.Value == query.MaxAmperage.Value);
            }

            if (query.OrderId.HasValue)
            {
                transactions = transactions.Where(x => x.OrderId == query.OrderId.Value);
            }

            if (!string.IsNullOrEmpty(query.ProviderOrderId))
            {
                transactions = transactions.Where(x => !string.IsNullOrEmpty(x.Order.OrderId) && x.Order.OrderId.Contains(query.ProviderOrderId));
            }

            if (query.Created.HasValue)
            {
                transactions = transactions.Where(x => x.Order.Created.Date == query.Created.Value.Date && x.Order.Created.ToShortTimeString() == query.Created.Value.ToShortTimeString());
            }

            if (query.Updated.HasValue)
            {
                transactions = transactions.Where(x => x.Order.Updated.HasValue && x.Order.Updated.Value.Date == query.Updated.Value.Date && x.Order.Updated.Value.ToShortTimeString() == query.Updated.Value.ToShortTimeString());
            }

            if (!string.IsNullOrEmpty(query.Currency))
            {
                transactions = transactions.Where(x => !string.IsNullOrEmpty(x.Order.Currency) && x.Order.Currency.ToLowerInvariant().Contains(query.Currency.ToLowerInvariant()));
            }

            if (query.AmountCharged.HasValue)
            {
                transactions = transactions.Where(x => decimal.Compare(x.Order.AmountCharged, query.AmountCharged.Value) == 0);
            }

            if (query.OrderStatus.HasValue)
            {
                transactions = transactions.Where(x => x.Order.Status == query.OrderStatus.Value);
            }

            if (!string.IsNullOrEmpty(query.MerchantOrderId))
            {
                transactions = transactions.Where(x => !string.IsNullOrEmpty(x.Order.MerchantOrderId) && x.Order.MerchantOrderId.Contains(query.MerchantOrderId));
            }

            if (query.AmountRefunded.HasValue)
            {
                transactions = transactions.Where(x => decimal.Compare(x.Order.AmountRefunded, query.AmountRefunded.Value) == 0);
            }

            if (!string.IsNullOrEmpty(query.Description))
            {
                transactions = transactions.Where(x => !string.IsNullOrEmpty(x.Order.Description) && x.Order.Description.Contains(query.Description));
            }

            if (query.Amount.HasValue)
            {
                transactions = transactions.Where(x => decimal.Compare(x.Order.Amount, query.Amount.Value) == 0);
            }

            if (!string.IsNullOrEmpty(query.Pan))
            {
                transactions = transactions.Where(x => !string.IsNullOrEmpty(x.Order.Pan) && x.Order.Pan.Contains(query.Pan));
            }

            if (query.OrderCreatedAt.HasValue)
            {
                transactions = transactions.Where(x => x.Order.OrderCreatedAt.Date == query.OrderCreatedAt.Value.Date);
            }

            var adminTransactionDtos = transactions.ProjectTo<AdminTransactionDTO>(_mapper.ConfigurationProvider);

            var response = new BaseResponseWithPagination<AdminTransactionDTO>();

            if (adminTransactionDtos.Any())
            {
                response.StatusCode = System.Net.HttpStatusCode.OK;
                await response.SetDataAsync(adminTransactionDtos, query.PageIndex, query.PageSize);
            }
            else
            {
                response.StatusCode = System.Net.HttpStatusCode.NotFound;
                response.Message = "No results were found matching the entered information";
            }

            return response;
        }
    }
}

