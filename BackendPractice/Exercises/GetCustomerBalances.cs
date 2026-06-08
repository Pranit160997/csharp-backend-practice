using System;
using System.Collections.Generic;
using System.Linq;

public static class Exercise05_CustomerOutstandingBalanceReport
{
    public static void Run()
    {
        var invoices = new List<BalanceInvoice>
        {
            new BalanceInvoice { InvoiceId = "INV-1", CustomerId = "C1", Amount = 100 },
            new BalanceInvoice { InvoiceId = "INV-2", CustomerId = "C1", Amount = 200 },
            new BalanceInvoice { InvoiceId = "INV-3", CustomerId = "C2", Amount = 300 },
            new BalanceInvoice { InvoiceId = "INV-4", CustomerId = "C3", Amount = 400 }
        };

        var payments = new List<BalancePayment>
        {
            new BalancePayment { PaymentId = "P1", InvoiceId = "INV-1", Amount = 100, Status = BalancePaymentStatus.Success },
            new BalancePayment { PaymentId = "P2", InvoiceId = "INV-2", Amount = 50, Status = BalancePaymentStatus.Success },
            new BalancePayment { PaymentId = "P3", InvoiceId = "INV-3", Amount = 100, Status = BalancePaymentStatus.Success },
            new BalancePayment { PaymentId = "P4", InvoiceId = "INV-4", Amount = 200, Status = BalancePaymentStatus.Failed }
        };

        var service = new BalanceReportService();

        var result = service.GetCustomerBalances(invoices, payments);

        foreach (var item in result)
        {
            Console.WriteLine(
                $"CustomerId: {item.CustomerId}, " +
                $"TotalInvoiced: {item.TotalInvoiced}, " +
                $"TotalPaid: {item.TotalPaid}, " +
                $"OutstandingBalance: {item.OutstandingBalance}");
        }
    }
}

public enum BalancePaymentStatus
{
    Success,
    Failed,
    Reversed
}

public class BalanceInvoice
{
    public required string InvoiceId { get; set; }
    public required string CustomerId { get; set; }
    public decimal Amount { get; set; }
}

public class BalancePayment
{
    public required string PaymentId { get; set; }
    public required string InvoiceId { get; set; }
    public decimal Amount { get; set; }
    public BalancePaymentStatus Status { get; set; }
}

public class CustomerBalanceSummary
{
    public required string CustomerId { get; set; }
    public decimal TotalInvoiced { get; set; }
    public decimal TotalPaid { get; set; }
    public decimal OutstandingBalance { get; set; }
}

public class BalanceReportService
{
    public List<CustomerBalanceSummary> GetCustomerBalances(
        List<BalanceInvoice> invoices,
        List<BalancePayment> payments)
    {
        // Your code here
        var resultList = new List<CustomerBalanceSummary>();

        var successfulPayments = payments
        .Where(p => p.Status == BalancePaymentStatus.Success);

        var totalInvoiced = new Dictionary<string, decimal>();
        var totalPaid = new Dictionary<string, decimal>();

        var getByInvoiceId = invoices.ToDictionary(x => x.InvoiceId);

        foreach (var invoice in invoices)
        {
            var key = invoice.CustomerId;

            if (!totalInvoiced.ContainsKey(key))
            {
                totalInvoiced[key] = invoice.Amount;
            }
            else
            {
                totalInvoiced[key] += invoice.Amount;
            }
        }

        foreach (var payment in successfulPayments)
        {
            var foundInvoice = getByInvoiceId.TryGetValue(payment.InvoiceId, out var balanceInvoice);
            if (foundInvoice && balanceInvoice != null)
            {
                if (!totalPaid.ContainsKey(balanceInvoice.CustomerId))
                {
                    totalPaid[balanceInvoice.CustomerId] = payment.Amount;
                }
                else
                {
                    totalPaid[balanceInvoice.CustomerId] += payment.Amount;
                }
            }
        }

        foreach (var item in totalInvoiced)
        {
            var customerId = item.Key;
            var invoicedAmount = item.Value;

            totalPaid.TryGetValue(customerId, out var paidAmount);

            var result = new CustomerBalanceSummary
            {
                CustomerId = customerId,
                TotalInvoiced = invoicedAmount,
                TotalPaid = paidAmount,
                OutstandingBalance = invoicedAmount - paidAmount
            };

            resultList.Add(result);
        }

        return resultList;


        throw new NotImplementedException();
    }
}