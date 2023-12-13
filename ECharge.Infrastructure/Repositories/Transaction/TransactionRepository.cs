using System.Net;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using ClosedXML.Excel;
using ECharge.Domain.CibPay.Interface;
using ECharge.Domain.CibPay.Model;
using ECharge.Domain.Entities;
using ECharge.Domain.EVtrip.Interfaces;
using ECharge.Domain.Repositories.Transaction.DTO;
using ECharge.Domain.Repositories.Transaction.Interface;
using ECharge.Domain.Repositories.Transaction.Query;
using ECharge.Infrastructure.Services.DatabaseContext;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace ECharge.Infrastructure.Repositories.Transaction
{
    public class TransactionRepository : ITransactionRepository
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        private readonly ICibPayService _cibPayService;
        private readonly IWebHostEnvironment _environment;
        private readonly IChargePointApiClient _chargePointApiClient;
        private readonly DateTime _dateForFilter;

        public TransactionRepository(DataContext context, IMapper mapper, ICibPayService cibPayService,
            IWebHostEnvironment environment, IChargePointApiClient chargePointApiClient, IConfiguration configuration)
        {
            _context = context;
            _mapper = mapper;
            _cibPayService = cibPayService;
            _environment = environment;
            _chargePointApiClient = chargePointApiClient;
            _dateForFilter = Convert.ToDateTime(configuration["CibPay:DateForFilter"]);
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
                transactions = transactions.Where(x =>
                    x.UpdatedTime.HasValue && x.UpdatedTime.Value.Date == query.UpdatedTime.Value.Date &&
                    x.UpdatedTime.Value.ToShortTimeString() == query.UpdatedTime.Value.ToShortTimeString());
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
                transactions = transactions.Where(x =>
                    !string.IsNullOrEmpty(x.ChargePointName) && x.ChargePointName.Contains(query.ChargePointName));
            }

            if (query.MaxVoltage.HasValue)
            {
                transactions = transactions.Where(x =>
                    x.MaxVoltage.HasValue && x.MaxVoltage.Value == query.MaxVoltage.Value);
            }

            if (query.MaxAmperage.HasValue)
            {
                transactions = transactions.Where(x =>
                    x.MaxAmperage.HasValue && x.MaxAmperage.Value == query.MaxAmperage.Value);
            }

            if (query.OrderId.HasValue)
            {
                transactions = transactions.Where(x => x.OrderId == query.OrderId.Value);
            }

            if (!string.IsNullOrEmpty(query.ProviderOrderId))
            {
                transactions = transactions.Where(x =>
                    !string.IsNullOrEmpty(x.Order.OrderId) && x.Order.OrderId.Contains(query.ProviderOrderId));
            }

            if (query.Created.HasValue)
            {
                transactions = transactions.Where(x =>
                    x.Order.Created.Date == query.Created.Value.Date && x.Order.Created.ToShortTimeString() ==
                    query.Created.Value.ToShortTimeString());
            }

            if (query.Updated.HasValue)
            {
                transactions = transactions.Where(x =>
                    x.Order.Updated.HasValue && x.Order.Updated.Value.Date == query.Updated.Value.Date &&
                    x.Order.Updated.Value.ToShortTimeString() == query.Updated.Value.ToShortTimeString());
            }

            if (!string.IsNullOrEmpty(query.Currency))
            {
                transactions = transactions.Where(x =>
                    !string.IsNullOrEmpty(x.Order.Currency) && x.Order.Currency.ToLowerInvariant()
                        .Contains(query.Currency.ToLowerInvariant()));
            }

            if (query.AmountCharged.HasValue)
            {
                transactions = transactions.Where(x =>
                    decimal.Compare(x.Order.AmountCharged, query.AmountCharged.Value) == 0);
            }

            if (query.OrderStatus.HasValue)
            {
                transactions = transactions.Where(x => x.Order.Status == query.OrderStatus.Value);
            }

            if (!string.IsNullOrEmpty(query.MerchantOrderId))
            {
                transactions = transactions.Where(x =>
                    !string.IsNullOrEmpty(x.Order.MerchantOrderId) &&
                    x.Order.MerchantOrderId.Contains(query.MerchantOrderId));
            }

            if (query.AmountRefunded.HasValue)
            {
                transactions = transactions.Where(x =>
                    decimal.Compare(x.Order.AmountRefunded, query.AmountRefunded.Value) == 0);
            }

            if (!string.IsNullOrEmpty(query.Description))
            {
                transactions = transactions.Where(x =>
                    !string.IsNullOrEmpty(x.Order.Description) && x.Order.Description.Contains(query.Description));
            }

            if (query.Amount.HasValue)
            {
                transactions = transactions.Where(x => decimal.Compare(x.Order.Amount, query.Amount.Value) == 0);
            }

            if (!string.IsNullOrEmpty(query.Pan))
            {
                transactions = transactions.Where(x =>
                    !string.IsNullOrEmpty(x.Order.Pan) && x.Order.Pan.Contains(query.Pan));
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
                await response.SetDataAsync(adminTransactionDtos.OrderByDescending(x => x.Created), query.PageIndex,
                    query.PageSize);
            }
            else
            {
                response.StatusCode = System.Net.HttpStatusCode.NotFound;
                response.Message = "No results were found matching the entered information";
            }

            return response;
        }

        public async Task<BaseResponseWithPagination<NotificationDTO>> AllNotifications(NotificationQuery query)
        {
            var inbox = _context.Notifications.AsQueryable();

            if (query.HasSeen.HasValue)
            {
                inbox = inbox.Where(x => x.HasSeen == query.HasSeen.Value);
            }

            if (query.IsCableStatus.HasValue)
            {
                inbox = inbox.Where(x => x.IsCableStatus == query.IsCableStatus.Value);
            }

            if (!string.IsNullOrEmpty(query.SessionId))
            {
                inbox = inbox.Where(x => string.Equals(x.SessionId, query.SessionId));
            }

            if (!string.IsNullOrEmpty(query.UserId))
            {
                inbox = inbox.Where(x => x.UserId != null && string.Equals(x.UserId, query.UserId));
            }

            if (query.CreatedDate.HasValue)
            {
                inbox = inbox.Where(x => DateTime.Compare(x.CreatedDate.Date, query.CreatedDate.Value.Date) == 0);
            }

            var inboxDTO = inbox.ProjectTo<NotificationDTO>(_mapper.ConfigurationProvider);

            var response = new BaseResponseWithPagination<NotificationDTO>();

            if (inboxDTO.Any())
            {
                response.StatusCode = System.Net.HttpStatusCode.OK;
                await response.SetDataAsync(inboxDTO.OrderByDescending(x => x.CreatedDate), query.PageIndex, query.PageSize);
            }
            else
            {
                response.StatusCode = System.Net.HttpStatusCode.NotFound;
                response.Message = "No results were found matching the entered information";
            }

            return response;
        }

        public async Task<BaseResponse<NotificationDTO>> SingleNotification(int id, string userId)
        {
            var response = new BaseResponse<NotificationDTO>();

            if (await _context.Notifications.AnyAsync(x => x.Id == id && x.UserId == userId))
            {
                var notification =
                    await _context.Notifications.FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);

                var notificationDTO = _mapper.Map<NotificationDTO>(notification);
                response.StatusCode = System.Net.HttpStatusCode.OK;
                response.Data = notificationDTO;
                notification.HasSeen = true;
                await _context.SaveChangesAsync();
            }
            else
            {
                response.StatusCode = System.Net.HttpStatusCode.NotFound;
                response.Message = "No results were found matching the entered information";
            }

            return response;
        }

        public async Task<BaseResponseWithPagination<CableStateDTO>> GetCableStates(CableStateQuery query)
        {
            var cableStates = _context.CableStateHooks.AsQueryable();

            if (!string.IsNullOrEmpty(query.ChargePointId))
            {
                cableStates = cableStates.Where(x => x.ChargePointId == query.ChargePointId);
            }

            if (!string.IsNullOrEmpty(query.SessionId))
            {
                cableStates = cableStates.Where(x => x.SessionId == query.SessionId);
            }

            if (!string.IsNullOrEmpty(query.CableState))
            {
                cableStates = cableStates.Where(x => x.CableState == query.CableState.ToUpper());
            }
            
            if (query.CreatedDateFrom.HasValue)
            {
                cableStates = cableStates.Where(x => x.CreatedDate >= query.CreatedDateFrom.Value);
            }

            if (query.CreatedDateTo.HasValue)
            {
                cableStates = cableStates.Where(x => x.CreatedDate <= query.CreatedDateTo.Value);
            }

            var cableStatesDto = cableStates.ProjectTo<CableStateDTO>(_mapper.ConfigurationProvider);

            var response = new BaseResponseWithPagination<CableStateDTO>();

            if (cableStatesDto.Any())
            {
                response.StatusCode = System.Net.HttpStatusCode.OK;
                await response.SetDataAsync(cableStatesDto.OrderByDescending(x => x.CreatedDate), query.PageIndex, query.PageSize);
            }
            else
            {
                response.StatusCode = System.Net.HttpStatusCode.NotFound;
                response.Message = "No results were found matching the entered information";
            }

            return response;
        }

        public async Task<BaseResponseWithPagination<OrderStatusChangedDTO>> GetOrderStatusChangedHooks(
            OrderStatusChangedQuery query)
        {
            var orderStatusChangedHooks = _context.OrderStatusChangedHooks.AsQueryable();

            if (!string.IsNullOrEmpty(query.ChargePointId))
            {
                orderStatusChangedHooks = orderStatusChangedHooks.Where(x => x.ChargerId == query.ChargePointId);
            }

            if (!string.IsNullOrEmpty(query.SessionId))
            {
                orderStatusChangedHooks = orderStatusChangedHooks.Where(x => x.SessionId == query.SessionId);
            }

            if (!string.IsNullOrEmpty(query.OrderUuid))
            {
                orderStatusChangedHooks = orderStatusChangedHooks.Where(x => x.OrderUuid == query.OrderUuid);
            }

            if (query.CreatedDateFrom.HasValue)
            {
                orderStatusChangedHooks = orderStatusChangedHooks.Where(x => x.CreatedDate >= query.CreatedDateFrom.Value);
            }

            if (query.CreatedDateTo.HasValue)
            {
                orderStatusChangedHooks = orderStatusChangedHooks.Where(x => x.CreatedDate <= query.CreatedDateTo.Value);
            }

            var orderStatusChangedHooksDto =
                orderStatusChangedHooks.ProjectTo<OrderStatusChangedDTO>(_mapper.ConfigurationProvider);

            var response = new BaseResponseWithPagination<OrderStatusChangedDTO>();

            if (orderStatusChangedHooksDto.Any())
            {
                response.StatusCode = System.Net.HttpStatusCode.OK;
                await response.SetDataAsync(orderStatusChangedHooksDto.OrderByDescending(x => x.CreatedDate),
                    query.PageIndex, query.PageSize);
            }
            else
            {
                response.StatusCode = System.Net.HttpStatusCode.NotFound;
                response.Message = "No results were found matching the entered information";
            }

            return response;

        }

        public async Task<BaseResponse<TransactionReportDTO>> GetTransactionReport(GetOrdersQuery query)
        {
            query.PageIndex = null;
            query.PageSize = null;

            var response = new BaseResponse<TransactionReportDTO>();

            try
            {
                var result = await _cibPayService.GetOrdersList(query);

                if (result is { Data: { Orders: not null, Orders.Count: not 0 } })
                {
                    var innerResponse = new TransactionReportDTO();
                    var transactions = result.Data.Orders;
                    innerResponse.AllTransactionCount = transactions.Count;
                    innerResponse.FromDate = transactions.Min(x => x.Created);
                    innerResponse.ToDate = transactions.Max(x => x.Created);
                    foreach (var tr in transactions)
                    {
                        innerResponse.TotalTransactionAmount += tr.Amount;
                        innerResponse.TotalChargedTransactionAmount += tr.AmountCharged;
                        innerResponse.TotalProfit += (tr.AmountCharged - tr.AmountRefunded);
                        innerResponse.TotalRefundedTransactionAmount += tr.AmountRefunded;

                        if (tr.Status == "new")
                        {
                            innerResponse.UnpaidTransactionCount++;
                            innerResponse.TotalUnpaidTransactionAmount += tr.Amount;
                        }
                        else if (tr.Status == "charged")
                        {
                            innerResponse.ChargedTransactionCount++;
                        }
                        else if (tr.Status == "refunded")
                        {
                            innerResponse.RefundedTransactionCount++;
                        }
                        else if (tr.Status == "rejected")
                        {
                            innerResponse.RejectedTransactionCount++;
                            innerResponse.TotalRejectedTransactionAmount += tr.Amount;
                        }
                        else if (tr.Status == "declined")
                        {
                            innerResponse.DeclinedTransactionCount++;
                            innerResponse.TotalDeclinedTransactionAmount += tr.Amount;
                        }

                    }

                    response.Data = innerResponse;
                    response.StatusCode = System.Net.HttpStatusCode.OK;
                }
                else
                {
                    response.StatusCode = System.Net.HttpStatusCode.NotFound;
                    response.Message = "No results were found matching the entered information";
                }

            }
            catch (Exception ex)
            {
                response.Error = ex.Message;
            }

            return response;
        }

        public async Task<BaseResponseWithPagination<SingleOrderResponse>> GetAllCibTransactions(GetOrdersQuery query)
        {
            var response = new BaseResponseWithPagination<SingleOrderResponse>();

            var pageSize = query.PageSize;
            var pageIndex = query.PageIndex;

            query.PageSize = null;
            query.PageIndex = null;

            if (query.NeedArchive)
            {
                query.CreatedTo = _dateForFilter;
            }

            try
            {
                var result = await _cibPayService.GetOrdersList(query);

                if (result is { Data: { Orders: not null, Orders.Count: not 0 } })
                {
                    var totalCount = result.Data.Orders.Count;

                    query.PageSize = pageSize;
                    query.PageIndex = pageIndex;

                    var result1 = await _cibPayService.GetOrdersList(query);

                    if (result1 is { Data: { Orders: not null, Orders.Count: not 0 } result2 })
                    {
                        response.TotalCount = totalCount;
                        response.PageIndex = pageIndex.Value;
                        response.PageSize = pageSize.Value;
                        response.TotalPage = (int)Math.Ceiling(totalCount / (double)pageSize);
                        response.Data = result2.Orders.OrderByDescending(x => x.Created).AsQueryable();
                    }
                    else
                        response.Message = "No results were found matching the entered information";
                }
                else
                    response.Message = "No results were found matching the entered information";
            }
            catch (Exception ex)
            {
                response.Error = ex.Message;
            }

            return response;
        }

        public async Task<ExcelFileResponse> GetTransactionExcel(GetOrdersQuery query)
        {
            var pageSize = query.PageSize;
            var pageIndex = query.PageIndex;

            query.PageSize = null;
            query.PageIndex = null;

            try
            {
                var result = await _cibPayService.GetOrdersList(query);

                using var wb = new XLWorkbook();
                var ws = wb.Worksheets.Add("Bank Əməliyyatları");

                if (result is { Data: { Orders: not null, Orders.Count: not 0 } orders })
                {
                    ws.Row(1).Height = 15;
                    ws.Row(2).Height = 15;
                    ws.Row(3).Height = 15;

                    ws.Range("A1:K2").Merge();
                    ws.Range("A1:K2").Value = "Bank Əməliyyatları cədvəli";
                    ws.Range("A1:K2").Style.Font.FontSize = 14;
                    ws.Range("A1:K2").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    ws.Range("A1:K2").Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    ws.Range("A1:K2").Style.Font.Bold = true;

                    ws.Cell("A3").Value = "#";
                    ws.Cell("A3").Style.Fill.BackgroundColor = XLColor.FromArgb(0, 112, 192);
                    ws.Cell("A3").Style.Font.FontColor = XLColor.White;
                    ws.Cell("A3").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    ws.Cell("A3").Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    ws.Cell("A3").Style.Font.Bold = true;
                    ws.Cell("A3").Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    ws.Cell("A3").Style.Border.TopBorder = XLBorderStyleValues.Thin;
                    ws.Cell("A3").Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                    ws.Cell("A3").Style.Border.RightBorder = XLBorderStyleValues.Thin;

                    ws.Cell("B3").Value = "Tranzaksiya nömrəsi";
                    ws.Cell("B3").Style.Fill.BackgroundColor = XLColor.FromArgb(0, 112, 192);
                    ws.Cell("B3").Style.Font.FontColor = XLColor.White;
                    ws.Cell("B3").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    ws.Cell("B3").Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    ws.Cell("B3").Style.Font.Bold = true;
                    ws.Cell("B3").Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    ws.Cell("B3").Style.Border.TopBorder = XLBorderStyleValues.Thin;
                    ws.Cell("B3").Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                    ws.Cell("B3").Style.Border.RightBorder = XLBorderStyleValues.Thin;

                    ws.Cell("C3").Value = "Təsdiq kodu";
                    ws.Cell("C3").Style.Fill.BackgroundColor = XLColor.FromArgb(0, 112, 192);
                    ws.Cell("C3").Style.Font.FontColor = XLColor.White;
                    ws.Cell("C3").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    ws.Cell("C3").Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    ws.Cell("C3").Style.Font.Bold = true;
                    ws.Cell("C3").Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    ws.Cell("C3").Style.Border.TopBorder = XLBorderStyleValues.Thin;
                    ws.Cell("C3").Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                    ws.Cell("C3").Style.Border.RightBorder = XLBorderStyleValues.Thin;

                    ws.Cell("D3").Value = "Sifariş nömrəsi";
                    ws.Cell("D3").Style.Fill.BackgroundColor = XLColor.FromArgb(0, 112, 192);
                    ws.Cell("D3").Style.Font.FontColor = XLColor.White;
                    ws.Cell("D3").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    ws.Cell("D3").Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    ws.Cell("D3").Style.Font.Bold = true;
                    ws.Cell("D3").Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    ws.Cell("D3").Style.Border.TopBorder = XLBorderStyleValues.Thin;
                    ws.Cell("D3").Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                    ws.Cell("D3").Style.Border.RightBorder = XLBorderStyleValues.Thin;

                    ws.Cell("E3").Value = "Kart nömrəsi";
                    ws.Cell("E3").Style.Fill.BackgroundColor = XLColor.FromArgb(0, 112, 192);
                    ws.Cell("E3").Style.Font.FontColor = XLColor.White;
                    ws.Cell("E3").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    ws.Cell("E3").Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    ws.Cell("E3").Style.Font.Bold = true;
                    ws.Cell("E3").Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    ws.Cell("E3").Style.Border.TopBorder = XLBorderStyleValues.Thin;
                    ws.Cell("E3").Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                    ws.Cell("E3").Style.Border.RightBorder = XLBorderStyleValues.Thin;

                    ws.Cell("F3").Value = "Status";
                    ws.Cell("F3").Style.Fill.BackgroundColor = XLColor.FromArgb(0, 112, 192);
                    ws.Cell("F3").Style.Font.FontColor = XLColor.White;
                    ws.Cell("F3").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    ws.Cell("F3").Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    ws.Cell("F3").Style.Font.Bold = true;
                    ws.Cell("F3").Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    ws.Cell("F3").Style.Border.TopBorder = XLBorderStyleValues.Thin;
                    ws.Cell("F3").Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                    ws.Cell("F3").Style.Border.RightBorder = XLBorderStyleValues.Thin;

                    ws.Cell("G3").Value = "Məbləğ";
                    ws.Cell("G3").Style.Fill.BackgroundColor = XLColor.FromArgb(0, 112, 192);
                    ws.Cell("G3").Style.Font.FontColor = XLColor.White;
                    ws.Cell("G3").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    ws.Cell("G3").Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    ws.Cell("G3").Style.Font.Bold = true;
                    ws.Cell("G3").Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    ws.Cell("G3").Style.Border.TopBorder = XLBorderStyleValues.Thin;
                    ws.Cell("G3").Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                    ws.Cell("G3").Style.Border.RightBorder = XLBorderStyleValues.Thin;

                    ws.Cell("H3").Value = "Ödənilən məbləğ";
                    ws.Cell("H3").Style.Fill.BackgroundColor = XLColor.FromArgb(0, 112, 192);
                    ws.Cell("H3").Style.Font.FontColor = XLColor.White;
                    ws.Cell("H3").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    ws.Cell("H3").Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    ws.Cell("H3").Style.Font.Bold = true;
                    ws.Cell("H3").Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    ws.Cell("H3").Style.Border.TopBorder = XLBorderStyleValues.Thin;
                    ws.Cell("H3").Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                    ws.Cell("H3").Style.Border.RightBorder = XLBorderStyleValues.Thin;

                    ws.Cell("I3").Value = "Geri qaytarılan məbləğ";
                    ws.Cell("I3").Style.Fill.BackgroundColor = XLColor.FromArgb(0, 112, 192);
                    ws.Cell("I3").Style.Font.FontColor = XLColor.White;
                    ws.Cell("I3").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    ws.Cell("I3").Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    ws.Cell("I3").Style.Font.Bold = true;
                    ws.Cell("I3").Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    ws.Cell("I3").Style.Border.TopBorder = XLBorderStyleValues.Thin;
                    ws.Cell("I3").Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                    ws.Cell("I3").Style.Border.RightBorder = XLBorderStyleValues.Thin;

                    ws.Cell("J3").Value = "Əməliyyat tarixi";
                    ws.Cell("J3").Style.Fill.BackgroundColor = XLColor.FromArgb(0, 112, 192);
                    ws.Cell("J3").Style.Font.FontColor = XLColor.White;
                    ws.Cell("J3").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    ws.Cell("J3").Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    ws.Cell("J3").Style.Font.Bold = true;
                    ws.Cell("J3").Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    ws.Cell("J3").Style.Border.TopBorder = XLBorderStyleValues.Thin;
                    ws.Cell("J3").Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                    ws.Cell("J3").Style.Border.RightBorder = XLBorderStyleValues.Thin;

                    ws.Cell("K3").Value = "Yenilənmə tarixi";
                    ws.Cell("K3").Style.Fill.BackgroundColor = XLColor.FromArgb(0, 112, 192);
                    ws.Cell("K3").Style.Font.FontColor = XLColor.White;
                    ws.Cell("K3").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    ws.Cell("K3").Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    ws.Cell("K3").Style.Font.Bold = true;
                    ws.Cell("K3").Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    ws.Cell("K3").Style.Border.TopBorder = XLBorderStyleValues.Thin;
                    ws.Cell("K3").Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                    ws.Cell("K3").Style.Border.RightBorder = XLBorderStyleValues.Thin;

                    int rowIndex = 4;
                    foreach (var data in orders.Orders.OrderByDescending(x=>x.Created))
                    {
                        ws.Cell("A" + rowIndex).Value = rowIndex - 3;
                        ws.Cell("A" + rowIndex).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        ws.Cell("A" + rowIndex).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        ws.Cell("A" + rowIndex).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                        ws.Cell("A" + rowIndex).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                        ws.Cell("A" + rowIndex).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                        ws.Cell("A" + rowIndex).Style.Border.RightBorder = XLBorderStyleValues.Thin;

                        ws.Cell("B" + rowIndex).Value = !string.IsNullOrEmpty(data.Id) ? data.Id : "-";
                        ws.Cell("B" + rowIndex).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        ws.Cell("B" + rowIndex).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        ws.Cell("B" + rowIndex).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                        ws.Cell("B" + rowIndex).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                        ws.Cell("B" + rowIndex).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                        ws.Cell("B" + rowIndex).Style.Border.RightBorder = XLBorderStyleValues.Thin;

                        ws.Cell("C" + rowIndex).Value = !string.IsNullOrEmpty(data.AuthCode) ? data.AuthCode : "-";
                        ws.Cell("C" + rowIndex).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        ws.Cell("C" + rowIndex).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        ws.Cell("C" + rowIndex).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                        ws.Cell("C" + rowIndex).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                        ws.Cell("C" + rowIndex).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                        ws.Cell("C" + rowIndex).Style.Border.RightBorder = XLBorderStyleValues.Thin;

                        ws.Cell("D" + rowIndex).Value = data.MerchantOrderId;
                        ws.Cell("D" + rowIndex).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        ws.Cell("D" + rowIndex).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        ws.Cell("D" + rowIndex).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                        ws.Cell("D" + rowIndex).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                        ws.Cell("D" + rowIndex).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                        ws.Cell("D" + rowIndex).Style.Border.RightBorder = XLBorderStyleValues.Thin;

                        ws.Cell("E" + rowIndex).Value = !string.IsNullOrEmpty(data.Pan) ? data.Pan : "-";
                        ws.Cell("E" + rowIndex).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        ws.Cell("E" + rowIndex).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        ws.Cell("E" + rowIndex).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                        ws.Cell("E" + rowIndex).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                        ws.Cell("E" + rowIndex).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                        ws.Cell("E" + rowIndex).Style.Border.RightBorder = XLBorderStyleValues.Thin;

                        string statusInAz = "-";

                        if (data.Status == "new")
                        {
                            statusInAz = "Ödənilməyib";
                        }
                        else if (data.Status == "charged")
                        {
                            statusInAz = "Ödənilib";
                        }
                        else if (data.Status == "refunded")
                        {
                            statusInAz = "Geri qaytarılıb";
                        }
                        else if (data.Status == "declined")
                        {
                            statusInAz = "İmtina edilib";
                        }
                        else if (data.Status == "rejected")
                        {
                            statusInAz = "Rədd edilib";
                        }

                        ws.Cell("F" + rowIndex).Value = statusInAz;
                        ws.Cell("F" + rowIndex).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        ws.Cell("F" + rowIndex).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        ws.Cell("F" + rowIndex).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                        ws.Cell("F" + rowIndex).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                        ws.Cell("F" + rowIndex).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                        ws.Cell("F" + rowIndex).Style.Border.RightBorder = XLBorderStyleValues.Thin;

                        ws.Cell("G" + rowIndex).Value = data.Amount;
                        ws.Cell("G" + rowIndex).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        ws.Cell("G" + rowIndex).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        ws.Cell("G" + rowIndex).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                        ws.Cell("G" + rowIndex).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                        ws.Cell("G" + rowIndex).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                        ws.Cell("G" + rowIndex).Style.Border.RightBorder = XLBorderStyleValues.Thin;

                        ws.Cell("H" + rowIndex).Value = data.AmountCharged;
                        ws.Cell("H" + rowIndex).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        ws.Cell("H" + rowIndex).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        ws.Cell("H" + rowIndex).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                        ws.Cell("H" + rowIndex).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                        ws.Cell("H" + rowIndex).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                        ws.Cell("H" + rowIndex).Style.Border.RightBorder = XLBorderStyleValues.Thin;

                        ws.Cell("I" + rowIndex).Value = data.AmountRefunded;
                        ws.Cell("I" + rowIndex).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        ws.Cell("I" + rowIndex).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        ws.Cell("I" + rowIndex).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                        ws.Cell("I" + rowIndex).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                        ws.Cell("I" + rowIndex).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                        ws.Cell("I" + rowIndex).Style.Border.RightBorder = XLBorderStyleValues.Thin;

                        ws.Cell("J" + rowIndex).Value = data.Created.ToString("dd-MM-yyyy");
                        ws.Cell("J" + rowIndex).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        ws.Cell("J" + rowIndex).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        ws.Cell("J" + rowIndex).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                        ws.Cell("J" + rowIndex).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                        ws.Cell("J" + rowIndex).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                        ws.Cell("J" + rowIndex).Style.Border.RightBorder = XLBorderStyleValues.Thin;

                        ws.Cell("K" + rowIndex).Value =
                            data.Updated.HasValue ? data.Updated.Value.ToString("dd-MM-yyyy") : "-";
                        ws.Cell("K" + rowIndex).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        ws.Cell("K" + rowIndex).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        ws.Cell("K" + rowIndex).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                        ws.Cell("K" + rowIndex).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                        ws.Cell("K" + rowIndex).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                        ws.Cell("K" + rowIndex).Style.Border.RightBorder = XLBorderStyleValues.Thin;

                        rowIndex++;
                    }

                    ws.Column("A").AdjustToContents();
                    ws.Column("B").AdjustToContents();
                    ws.Column("C").AdjustToContents();
                    ws.Column("D").AdjustToContents();
                    ws.Column("E").AdjustToContents();
                    ws.Column("F").AdjustToContents();
                    ws.Column("G").AdjustToContents();
                    ws.Column("H").AdjustToContents();
                    ws.Column("I").AdjustToContents();
                    ws.Column("J").AdjustToContents();
                    ws.Column("K").AdjustToContents();
                    ws.Column("L").AdjustToContents();
                    ws.Column("M").AdjustToContents();
                    ws.Column("N").AdjustToContents();
                    ws.Column("O").AdjustToContents();
                    ws.Column("P").AdjustToContents();
                    ws.Column("Q").AdjustToContents();
                    ws.Column("R").AdjustToContents();
                    ws.Column("S").AdjustToContents();
                    ws.Column("T").AdjustToContents();
                    ws.Column("U").AdjustToContents();
                    ws.Column("V").AdjustToContents();
                    ws.Column("W").AdjustToContents();


                    using var stream = new MemoryStream();
                    wb.SaveAs(stream);
                    var content = stream.ToArray();

                    // Save to a temporary file
                    var tempFilePath = Path.Combine(_environment.ContentRootPath, "TempFiles");

                    using (var fileStream = new FileStream(tempFilePath, FileMode.Create))
                    {
                        stream.Position = 0;
                        stream.CopyTo(fileStream);
                    }

                    // Return the temporary file as a download
                    var fileContent = File.ReadAllBytes(tempFilePath);
                    var fileType =
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"; // Set the appropriate content type based on your file
                    var fileName = "ApplicantDatas.xlsx"; // Set the desired file name

                    File.Delete(tempFilePath);

                    return new ExcelFileResponse
                    {
                        ContentType = fileType,
                        FileBytes = fileContent,
                        FileName = fileName
                    };
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return null;
        }

        public async Task<BaseResponse<List<StatisticReport>>> GetStatisticsReport()
        {
            var response = new BaseResponse<List<StatisticReport>>();
            try
            {
                var result = await _cibPayService.GetOrdersList(new GetOrdersQuery
                {
                    Expand = "custom_fields"
                });

                if (result?.Data?.Orders?.Any() != true)
                {
                    response.StatusCode = HttpStatusCode.NotFound;
                    response.Message = "No results were found matching the entered information";
                    return response;
                }

                var orderResponses = result.Data.Orders.ToList();

                var chargePointIds = orderResponses
                    .Where(order => order.CustomFields != null && order.CustomFields.ContainsKey("charge_point_id"))
                    .Select(order => order.CustomFields["charge_point_id"].ToString())
                    .Distinct()
                    .ToList();

                if (chargePointIds.Count == 0)
                {
                    response.StatusCode = HttpStatusCode.NotFound;
                    response.Message = "No results were found matching the entered information";
                    return response;
                }

                var chargePointResults = await Task.WhenAll(chargePointIds
                    .Select(id => _chargePointApiClient.GetSingleChargerAsync(id)));

                var statisticReports = new List<StatisticReport>();

                foreach (var groupOrder in orderResponses
                             .Where(order =>
                                 order.CustomFields != null && order.CustomFields.ContainsKey("charge_point_id"))
                             .GroupBy(order => order.CustomFields["charge_point_id"].ToString()))
                {
                    var chargePointResult = chargePointResults
                        .FirstOrDefault(result => result?.Result?.Id == groupOrder.Key);

                    if (chargePointResult?.Result == null)
                        continue;

                    var singleChargePointSessions =
                        await _chargePointApiClient.GetSingleChargePointSessions(groupOrder.Key);
                    var totalDurationInMinutes = singleChargePointSessions.Sum(x => x.DurationInMinutes);

                    var totalProfit = groupOrder.Sum(order => order.AmountCharged - order.AmountRefunded);

                    statisticReports.Add(new StatisticReport
                    {
                        ChargePointId = groupOrder.Key,
                        ChargePointName = chargePointResult.Result.Name,
                        TotalTransactions = groupOrder.Count(),
                        TotalProfit = totalProfit,
                        TotalDurationInMinutes = totalDurationInMinutes ?? 0
                    });
                }

                if (statisticReports.Count == 0)
                {
                    response.StatusCode = HttpStatusCode.NotFound;
                    response.Message = "No results were found matching the entered information";
                    return response;
                }

                response.Data = statisticReports;
            }
            catch (Exception e)
            {
                response.Error = e.Message;
            }

            return response;
        }
    }
}