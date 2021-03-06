using System;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using app.Components.Summary;
using app.Entities;
using app.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;

namespace app.Components.Reports
{
    public class CompanyReports
    {
        private readonly HttpClient _client;
        private readonly ITelegramBotClient _botClient;
        private readonly IEpumpDataRepository _epumpDataRepository;
        private Stream _pdfReport;
        private const string DateTimeFormat = "MMMM dd, yyyy";
        private readonly string _endDate = DateTime.Today.ToString("MMMM dd, yyyy");
        private EpumpData _userData;
        public CompanyReports(ITelegramBotClient botClient, IEpumpDataRepository epumpDataRepository, IHttpClientFactory httpClientFactory)
        {
            _epumpDataRepository = epumpDataRepository;
            _botClient = botClient;
            _client = httpClientFactory.CreateClient("EpumpReportApi");
        }

        private static string ConvertTextToDateTime(string dateTimeText)
        {
            // TODO adjust the time settings
            // -7 might be more than a week
            // Epump Date Format is "Jan 1st, 2021"
            return dateTimeText switch
            {
                "Today" => DateTime.Today.ToString(DateTimeFormat),
                "Yesterday" => DateTime.Today.AddDays(-1).ToString(DateTimeFormat),
                "Week" => DateTime.Today.AddDays(-7).ToString(DateTimeFormat),
                "Month" => DateTime.Today.AddMonths(-1).ToString(DateTimeFormat),
                _ => DateTime.Today.AddDays(-3).ToString(DateTimeFormat),
            };
        }

        public async Task<Message> SendCompanyBranchSalesReportAsync(CallbackQuery query, Message message)
        {
            _userData = await _epumpDataRepository.GetUserDetailsAsync(message.Chat.Id);
            var uri = $"EpumpReport/Company/CompanyBranchSalesReport?companyId={_userData.CompanyId}&startDate={ConvertTextToDateTime(query.Data)}&endDate={_endDate}";

            var result = await GetReportWithSummaryDataAsync(message, uri, _userData);

            await _botClient.SendTextMessageAsync(message.Chat.Id, $"Summary\nYou sold:\nPMS:{result.pmsAmount}\nAGO: {result.agoAmount}\nDPK: {result.dpkAmount}\nTotal: {result.totalAmount}");
            return await _botClient.SendDocumentAsync(query.Message.Chat.Id, new InputOnlineFile(_pdfReport, "CompanyBranchSalesReport.pdf"));
        }

        public async Task<Message> SendCompanyCashflowReportAsync(CallbackQuery query, Message message)
        {
            // TODO this report hasn't been modified in ReportAPI
            _userData = await _epumpDataRepository.GetUserDetailsAsync(message.Chat.Id);
            var uri = $"EpumpReport/Company/CompanyCashflowReport?companyId={_userData.CompanyId}&startDate={ConvertTextToDateTime(query.Data)}&endDate={_endDate}";
            var content = await GetReportWithoutSummaryDataAsync(message, uri, _userData);

            return await _botClient.SendDocumentAsync(query.Message.Chat.Id, new InputOnlineFile(content, "CompanyCashFlowReport.pdf"));
        }

        public async Task<Message> SendCompanyExpenseCategoriesReportAsync(CallbackQuery query, Message message)
        {
            _userData = await _epumpDataRepository.GetUserDetailsAsync(message.Chat.Id);

            var uri = $"EpumpReport/Company/Management/ExpenseCategoriesReport?companyId={_userData.CompanyId}";
            var content = await GetReportWithoutSummaryDataAsync(message, uri, _userData);
            return await _botClient.SendDocumentAsync(query.Message.Chat.Id, new InputOnlineFile(content, "CompanyExpenseCategoriesReport.pdf"));
        }

        public async Task<Message> SendCompanyOutstandingPaymentsReportAsync(CallbackQuery query, Message message)
        {
            _userData = await _epumpDataRepository.GetUserDetailsAsync(message.Chat.Id);
            var uri = $"EpumpReport/Company/Management/OutstandingPayments?companyId={_userData.CompanyId}";

            var result = await GetReportWithSummaryDataAsync(message, uri, _userData);

            await _botClient.SendTextMessageAsync(message.Chat.Id, $@"Summary
Outstanding Amount: {result.outstandingAmount}");

            return await _botClient.SendDocumentAsync(query.Message.Chat.Id, new InputOnlineFile(_pdfReport, "CompanyOutstandingPaymentsReport.pdf"));
        }

        public async Task<Message> SendCompanyRetainershipReportAsync(CallbackQuery query, Message message)
        {
            _userData = await _epumpDataRepository.GetUserDetailsAsync(message.Chat.Id);
            var uri = $"EpumpReport/Company/Retainerships/RetainershipReport?companyId={_userData.CompanyId}";

            var content = await GetReportWithoutSummaryDataAsync(message, uri, _userData);
            return await _botClient.SendDocumentAsync(query.Message.Chat.Id, new InputOnlineFile(content, "CompanyRetainershipReport.pdf"));
        }

        public async Task<Message> SendCompanySalesSummaryReportAsync(CallbackQuery query, Message message)
        {
            _userData = await _epumpDataRepository.GetUserDetailsAsync(message.Chat.Id);

            var uri = $"EpumpReport/Company/CompanySalesSummaryReport?companyId={_userData.CompanyId}&startDate={ConvertTextToDateTime(query.Data)}&endDate={_endDate}";
            var result = await GetReportWithSummaryDataAsync(message, uri, _userData);

            await _botClient.SendTextMessageAsync(message.Chat.Id, $@"Summary

PMS Tank Sales: {result.pmsTankSale}
AGO Tank Sales: {result.agoTankSale}
DPK Tank Sales: {result.dpkTankSale}
PMS Pump Sales: {result.pmsPumpSale}
AGO Pump Sales: {result.agoPumpSale}
DPK Pump Sales: {result.dpkPumpSale}
Total Sales: {result.totalAmount}
");
            return await _botClient.SendDocumentAsync(query.Message.Chat.Id, new InputOnlineFile(_pdfReport, "CompanySalesSummaryReport.pdf"));
        }

        public async Task<Message> SendCompanyTankStockReportAsync(CallbackQuery query, Message message)
        {
            _userData = await _epumpDataRepository.GetUserDetailsAsync(message.Chat.Id);
            var uri = $"???EpumpReport???/Company???/CompanyTankStockReport?companyId={_userData.CompanyId}";

            var result = await GetReportWithSummaryDataAsync(message, uri, _userData);
            await _botClient.SendTextMessageAsync(message.Chat.Id, $@"Summary

PMS Volume: {result.pmsVolume}
AGO Volume: {result.agoVolume}
LPG Volume: {result.dpkVolume}
DPK Volume: {result.lpgVolume}
");
            return await _botClient.SendDocumentAsync(query.Message.Chat.Id, new InputOnlineFile(_pdfReport, "CompanyTankStockReport.pdf"));
        }

        public async Task<Message> SendCompanyTanksFilledReportAsync(Message message)
        {
            var date = ValidateDateInput(message);
            if (date == "Invalid Date")
            {
                return await _botClient.SendTextMessageAsync(message.Chat.Id, "Please input time in this format: 'January 01, 2000'", replyMarkup: new ForceReplyMarkup());
            }

            _userData = await _epumpDataRepository.GetUserDetailsAsync(message.Chat.Id);

            var uri = $"EpumpReport/Company/CompanyTanksFilledReport?companyId={_userData.CompanyId}&date={date}";
            var result = await GetReportWithSummaryDataAsync(message, uri, _userData);
            await _botClient.SendTextMessageAsync(message.Chat.Id, $@"Summary

Volume Discharged(Epump): {result.epumpDischarge}
Volume Discharged(Manual): {result.manualDischarge}
");

            return await _botClient.SendDocumentAsync(message.Chat.Id, new InputOnlineFile(_pdfReport, "CompanyTanksFilledReport.pdf"));
        }

        private static string ValidateDateInput(Message message)
        {
            return DateTime.TryParseExact(message.Text, DateTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date) 
                ? date.ToString(DateTimeFormat) 
                : "Invalid Date";
        }

        public async Task<Message> SendCompanyVarianceReportAsync(CallbackQuery query, Message message)
        {
            _userData = await _epumpDataRepository.GetUserDetailsAsync(message.Chat.Id);
            string uri = $"EpumpReport/Company/CompanyVarianceReport?companyId={_userData.CompanyId}&startDate={ConvertTextToDateTime(query.Data)}&endDate={_endDate}";
            var result = await GetReportWithSummaryDataAsync(message, uri, _userData);

            await _botClient.SendTextMessageAsync(message.Chat.Id, $@"Summary

Total Volume Sold(Epump): {result.totalEpumpVolumeSold}
Total Volume Sold(Manual): {result.totalManualVolumeSold}
Variance: {result.totalVariance}");
            return await _botClient.SendDocumentAsync(query.Message.Chat.Id, new InputOnlineFile(_pdfReport, "CompanyVarianceReport.pdf"));
        }

        public async Task<Message> SendCompanyWalletFundRequestReportAsync(CallbackQuery query, Message message)
        {
            _userData = await _epumpDataRepository.GetUserDetailsAsync(message.Chat.Id);

            var uri = $"???EpumpReport/Company/Retainerships/WalletFundRequestReport?companyId={_userData.CompanyId}&status={query.Data}";
            try
            {
                var result = await GetReportWithSummaryDataAsync(message, uri, _userData);
                await _botClient.SendTextMessageAsync(query.Message.Chat.Id, $"Summary\nAmount Paid: {result.amountPaid}");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return await _botClient.SendDocumentAsync(query.Message.Chat.Id, new InputOnlineFile(_pdfReport, "WalletFundRequest.pdf"));
        }

        public async Task<Message> SendCompanyPosTransactionsReportAsync(CallbackQuery query, Message message)
        {
            _userData = await _epumpDataRepository.GetUserDetailsAsync(message.Chat.Id);
            string uri = $"EpumpReport/PosTransactionReport?companyId={_userData.CompanyId}&startDate={ConvertTextToDateTime(query.Data)}&endDate={_endDate}";

            var result = await GetReportWithSummaryDataAsync(message, uri, _userData);
            await _botClient.SendTextMessageAsync(message.Chat.Id, $@"Summary
Amount of cashbacks : {result.cashbackAmount}
Failed Transactions: {result.failedAmount}
Succesful Transactions: {result.succesfulAmount}
");
            return await _botClient.SendDocumentAsync(query.Message.Chat.Id, new InputOnlineFile(_pdfReport, "Pos Transactions Report.pdf"));
        }

        public async Task<Message> SendCompanyWalletReportAsync(CallbackQuery query, Message message)
        {
            _userData = await _epumpDataRepository.GetUserDetailsAsync(message.Chat.Id);
            var uri = $"EpumpReport/Company/Management/CompanyWalletReport?companyId={_userData.CompanyId}";

            var result = await GetReportWithSummaryDataAsync(message, uri, _userData);
            await _botClient.SendTextMessageAsync(message.Chat.Id, $@"Summary

Wallet Balance: {ParseAsCurrency(result.walletBalance)}
Wallet BookBalance: {ParseAsCurrency(result.walletBookBalance)}
");
            return await _botClient.SendDocumentAsync(query.Message.Chat.Id, new InputOnlineFile(_pdfReport, "CompanyWalletReport.pdf"));
        }

        public async Task<Message> SendCompanyZonesReportAsync(CallbackQuery query, Message message)
        {
            // TODO Custom Exception Middleware
            _userData = await _epumpDataRepository.GetUserDetailsAsync(message.Chat.Id);

            var uri = $"EpumpReport/Company/Management/ZonesReport?companyId={_userData.CompanyId}";
            var content = await GetReportWithoutSummaryDataAsync(message, uri, _userData);
            return await _botClient.SendDocumentAsync(query.Message.Chat.Id, new InputOnlineFile(content, "CompanyZonesReport.pdf"));
        }

        private async Task<SummaryData> GetReportWithSummaryDataAsync(Message message, string uri, EpumpData userData)
        {
            // this makes it a little less repetitive
            await _botClient.DeleteMessageAsync(message.Chat.Id, message.MessageId);
            _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {userData.AuthKey}");

            await _botClient.SendTextMessageAsync(message.Chat.Id, "Generating PDF...");

            var requestMessage = new HttpRequestMessage(HttpMethod.Get, uri);
            var response = await _client.SendAsync(requestMessage);

            var content = await response.Content.ReadAsStreamAsync();
            var result = await JsonSerializer.DeserializeAsync<SummaryData>(content);

            // Todo add check for NullReferenceException
            _pdfReport = new MemoryStream(result.pdfReport);
            return result;
        }

        private async Task<Stream> GetReportWithoutSummaryDataAsync(Message message, string uri, EpumpData userData)
        {
            // delete the message
            await _botClient.DeleteMessageAsync(message.Chat.Id, message.MessageId);
            _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {userData.AuthKey}");

            await _botClient.SendTextMessageAsync(message.Chat.Id, "Generating PDF...");

            var requestMessage = new HttpRequestMessage(HttpMethod.Get, uri);
            var response = await _client.SendAsync(requestMessage);
            
            var content = await response.Content.ReadAsStreamAsync();
            return content;
        }

        private string ParseAsCurrency(string value)
        {
            var x = Convert.ToDouble(value);
            return x.ToString("C2", CultureInfo.CreateSpecificCulture(("HA-LATN-NG")));
        }
    }
}