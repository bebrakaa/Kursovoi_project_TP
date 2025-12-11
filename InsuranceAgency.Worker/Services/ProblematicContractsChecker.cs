using InsuranceAgency.Application.Interfaces.Repositories;
using InsuranceAgency.Application.Interfaces.Services;
using InsuranceAgency.Application.Common.Validation;
using InsuranceAgency.Domain.Entities;
using InsuranceAgency.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace InsuranceAgency.Worker.Services;

public class ProblematicContractsChecker
{
    private readonly IContractRepository _contractRepository;
    private readonly IDocumentVerificationRepository _verificationRepository;
    private readonly INotificationService _notificationService;
    private readonly ILogger<ProblematicContractsChecker> _logger;

    public ProblematicContractsChecker(
        IContractRepository contractRepository,
        IDocumentVerificationRepository verificationRepository,
        INotificationService notificationService,
        ILogger<ProblematicContractsChecker> logger)
    {
        _contractRepository = contractRepository;
        _verificationRepository = verificationRepository;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task CheckAndProcessProblematicContractsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting problematic contracts check at {Time}", DateTime.UtcNow);

        try
        {
            var problemsFound = 0;

            // 1. Проверка просроченных договоров
            problemsFound += await ProcessOverdueContractsAsync(cancellationToken);

            // 2. Проверка неоплаченных договоров (старше 7 дней)
            problemsFound += await ProcessUnpaidContractsAsync(TimeSpan.FromDays(7), cancellationToken);

            // 3. Проверка договоров, требующих продления (заканчиваются через 30 дней)
            problemsFound += await ProcessContractsRequiringRenewalAsync(30, cancellationToken);

            // 4. Помечаем истекшие договоры
            problemsFound += await ProcessExpiredContractsAsync(cancellationToken);

            // 5. Проверка целостности данных договора и верификации клиента
            problemsFound += await ProcessDataIntegrityAndVerificationAsync(cancellationToken);

            _logger.LogInformation(
                "Problematic contracts check completed. Found {Count} problems at {Time}",
                problemsFound,
                DateTime.UtcNow);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during problematic contracts check");
            throw;
        }
    }

    private async Task<int> ProcessOverdueContractsAsync(CancellationToken cancellationToken)
    {
        var overdueContracts = await _contractRepository.GetOverdueContractsAsync();
        var count = 0;

        foreach (var contract in overdueContracts)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            try
            {
                // Помечаем договор как просроченный, если еще не помечен
                if (contract.Status != ContractStatus.Overdue)
                {
                    contract.MarkOverdue();
                    await _contractRepository.UpdateAsync(contract);
                    await _contractRepository.SaveChangesAsync();
                }

                // Помечаем как проблемный
                if (!contract.IsFlaggedProblem)
                {
                    contract.MarkProblematic("Contract is overdue");
                    await _contractRepository.UpdateAsync(contract);
                    await _contractRepository.SaveChangesAsync();
                }

                // Отправляем уведомление клиенту
                if (contract.Client != null && !string.IsNullOrEmpty(contract.Client.Email))
                {
                    await SendOverdueNotificationAsync(contract);
                }

                count++;
                _logger.LogWarning(
                    "Found overdue contract: {ContractId} ({ContractNumber}) for client {ClientEmail}",
                    contract.Id,
                    contract.Number ?? "N/A",
                    contract.Client?.Email ?? "N/A");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing overdue contract {ContractId}", contract.Id);
            }
        }

        return count;
    }

    private async Task<int> ProcessUnpaidContractsAsync(TimeSpan overdueThreshold, CancellationToken cancellationToken)
    {
        var unpaidContracts = await _contractRepository.GetUnpaidContractsAsync(overdueThreshold);
        var count = 0;

        foreach (var contract in unpaidContracts)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            try
            {
                // Помечаем как проблемный, если еще не помечен
                if (!contract.IsFlaggedProblem)
                {
                    contract.MarkProblematic($"Contract unpaid for more than {overdueThreshold.Days} days");
                    await _contractRepository.UpdateAsync(contract);
                    await _contractRepository.SaveChangesAsync();
                }

                // Отправляем уведомление клиенту
                if (contract.Client != null && !string.IsNullOrEmpty(contract.Client.Email))
                {
                    await SendUnpaidNotificationAsync(contract, overdueThreshold);
                }

                count++;
                _logger.LogWarning(
                    "Found unpaid contract: {ContractId} ({ContractNumber}) for client {ClientEmail}",
                    contract.Id,
                    contract.Number ?? "N/A",
                    contract.Client?.Email ?? "N/A");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing unpaid contract {ContractId}", contract.Id);
            }
        }

        return count;
    }

    private async Task<int> ProcessContractsRequiringRenewalAsync(int daysBeforeExpiration, CancellationToken cancellationToken)
    {
        var contractsRequiringRenewal = await _contractRepository.GetContractsRequiringRenewalAsync(daysBeforeExpiration);
        var count = 0;

        foreach (var contract in contractsRequiringRenewal)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            try
            {
                // Только напоминание: не считаем проблемным
                // Отправляем уведомление о необходимости продления
                if (contract.Client != null && !string.IsNullOrEmpty(contract.Client.Email))
                {
                    await SendRenewalReminderAsync(contract, daysBeforeExpiration);
                }

                _logger.LogInformation(
                    "Contract requires renewal: {ContractId} ({ContractNumber}) expires on {EndDate}",
                    contract.Id,
                    contract.Number ?? "N/A",
                    contract.EndDate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing contract requiring renewal {ContractId}", contract.Id);
            }
        }

        return count;
    }

    private async Task<int> ProcessExpiredContractsAsync(CancellationToken cancellationToken)
    {
        var expiredContracts = await _contractRepository.GetExpiredContractsAsync();
        var count = 0;

        foreach (var contract in expiredContracts)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            try
            {
                // Помечаем договор как истекший
                if (contract.Status != ContractStatus.Expired)
                {
                    contract.Expire();
                    await _contractRepository.UpdateAsync(contract);
                    await _contractRepository.SaveChangesAsync();
                }

                // Отправляем уведомление клиенту
                if (contract.Client != null && !string.IsNullOrEmpty(contract.Client.Email))
                {
                    await SendExpiredNotificationAsync(contract);
                }

                count++;
                _logger.LogInformation(
                    "Marked contract as expired: {ContractId} ({ContractNumber})",
                    contract.Id,
                    contract.Number ?? "N/A");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing expired contract {ContractId}", contract.Id);
            }
        }

        return count;
    }

    private async Task<int> ProcessDataIntegrityAndVerificationAsync(CancellationToken cancellationToken)
    {
        var allContracts = await _contractRepository.GetAllAsync();
        var count = 0;

        foreach (var contract in allContracts)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            var problems = new List<string>();

            // Некорректные данные договора
            if (string.IsNullOrWhiteSpace(contract.Number))
                problems.Add("Missing contract number");
            if (contract.ClientId == Guid.Empty)
                problems.Add("Missing client");
            if (contract.ServiceId == Guid.Empty)
                problems.Add("Missing insurance service");
            if (contract.Premium.Amount <= 0)
                problems.Add("Premium amount must be positive");
            if (contract.EndDate < contract.StartDate)
                problems.Add("EndDate earlier than StartDate");

            // Неверифицированные обязательные данные клиента
            var verifications = await _verificationRepository.GetByClientIdAsync(contract.ClientId);
            var pendingRequired = verifications
                .Where(v => VerificationRules.IsRequiredType(v.DocumentType) &&
                            v.Status == VerificationStatus.Pending)
                .Select(v => v.DocumentType ?? "unknown")
                .ToList();
            var missingRequired = VerificationRules.RequiredPersonalDataTypes
                .Where(required => !verifications.Any(v =>
                    VerificationRules.IsSameType(v.DocumentType, required) &&
                    v.Status == VerificationStatus.Approved))
                .ToList();

            if (pendingRequired.Any())
                problems.Add($"Pending verification for: {string.Join(", ", pendingRequired)}");
            if (missingRequired.Any())
                problems.Add($"No approved verification for: {string.Join(", ", missingRequired)}");

            if (problems.Any())
            {
                try
                {
                    var reason = string.Join("; ", problems);
                    if (!contract.IsFlaggedProblem)
                    {
                        contract.MarkProblematic(reason);
                        await _contractRepository.UpdateAsync(contract);
                        await _contractRepository.SaveChangesAsync();
                    }
                    count++;
                    _logger.LogWarning("Data/verification issue in contract {ContractId} ({Number}): {Reason}",
                        contract.Id, contract.Number ?? "N/A", reason);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error marking contract {ContractId} as problematic due to data/verification issues", contract.Id);
                }
            }
        }

        return count;
    }

    private async Task SendOverdueNotificationAsync(Contract contract)
    {
        var subject = $"Договор {contract.Number ?? contract.Id.ToString()} просрочен";
        var body = $@"
Уважаемый(ая) {contract.Client?.FullName ?? "Клиент"}!

Ваш договор страхования {contract.Number ?? contract.Id.ToString()} просрочен.
Дата окончания: {contract.EndDate:dd.MM.yyyy}

Пожалуйста, свяжитесь с нами для решения вопроса о продлении договора.

С уважением,
Страховое агентство
";

        await _notificationService.SendAsync(contract.Client!.Email, subject, body);
    }

    private async Task SendUnpaidNotificationAsync(Contract contract, TimeSpan overdueThreshold)
    {
        var subject = $"Требуется оплата договора {contract.Number ?? contract.Id.ToString()}";
        var body = $@"
Уважаемый(ая) {contract.Client?.FullName ?? "Клиент"}!

Ваш договор страхования {contract.Number ?? contract.Id.ToString()} не оплачен более {overdueThreshold.Days} дней.
Сумма к оплате: {contract.Premium.Amount} {contract.Premium.Currency}

Пожалуйста, произведите оплату в ближайшее время.

С уважением,
Страховое агентство
";

        await _notificationService.SendAsync(contract.Client!.Email, subject, body);
    }

    private async Task SendRenewalReminderAsync(Contract contract, int daysBeforeExpiration)
    {
        var subject = $"Напоминание: договор {contract.Number ?? contract.Id.ToString()} заканчивается через {daysBeforeExpiration} дней";
        var body = $@"
Уважаемый(ая) {contract.Client?.FullName ?? "Клиент"}!

Напоминаем, что ваш договор страхования {contract.Number ?? contract.Id.ToString()} заканчивается {contract.EndDate:dd.MM.yyyy}.

Для продления договора, пожалуйста, свяжитесь с нашим агентством.

С уважением,
Страховое агентство
";

        await _notificationService.SendAsync(contract.Client!.Email, subject, body);
    }

    private async Task SendExpiredNotificationAsync(Contract contract)
    {
        var subject = $"Договор {contract.Number ?? contract.Id.ToString()} истек";
        var body = $@"
Уважаемый(ая) {contract.Client?.FullName ?? "Клиент"}!

Ваш договор страхования {contract.Number ?? contract.Id.ToString()} истек {contract.EndDate:dd.MM.yyyy}.

Если вы хотите продлить договор, пожалуйста, свяжитесь с нашим агентством.

С уважением,
Страховое агентство
";

        await _notificationService.SendAsync(contract.Client!.Email, subject, body);
    }
}

