using System;
using System.Collections.Generic;
using System.Linq;

public static class Exercise04_SuccessfulPaymentsByCustomer
{
    public static void Run()
    {
        var invoices = new List<PaymentInvoice>
        {
            new PaymentInvoice
            {
                InvoiceId = "INV-1",
                CustomerId = "C1",
                Amount = 100
            },
            new PaymentInvoice
            {
                InvoiceId = "INV-2",
                CustomerId = "C1",
                Amount = 200
            },
            new PaymentInvoice
            {
                InvoiceId = "INV-3",
                CustomerId = "C2",
                Amount = 300
            },
            new PaymentInvoice
            {
                InvoiceId = "INV-4",
                CustomerId = "C3",
                Amount = 400
            }
        };

        var payments = new List<Payment>
        {
            new Payment
            {
                PaymentId = "P1",
                InvoiceId = "INV-1",
                Amount = 100,
                Status = PaymentStatus.Success
            },
            new Payment
            {
                PaymentId = "P2",
                InvoiceId = "INV-2",
                Amount = 50,
                Status = PaymentStatus.Success
            },
            new Payment
            {
                PaymentId = "P3",
                InvoiceId = "INV-2",
                Amount = 25,
                Status = PaymentStatus.Failed
            },
            new Payment
            {
                PaymentId = "P4",
                InvoiceId = "INV-3",
                Amount = 150,
                Status = PaymentStatus.Success
            },
            new Payment
            {
                PaymentId = "P5",
                InvoiceId = "INV-4",
                Amount = 100,
                Status = PaymentStatus.Reversed
            }
        };

        var service = new PaymentReportService();

        var result = service.GetSuccessfulPaymentsByCustomer(
            invoices,
            payments);

        foreach (var item in result)
        {
            Console.WriteLine(
                $"CustomerId: {item.CustomerId}, " +
                $"TotalPaid: {item.TotalPaid}");
        }
    }
}

public enum PaymentStatus
{
    Success,
    Failed,
    Reversed
}

public class PaymentInvoice
{
    public required string InvoiceId { get; set; }
    public required string CustomerId { get; set; }
    public decimal Amount { get; set; }
}

public class Payment
{
    public required string PaymentId { get; set; }
    public required string InvoiceId { get; set; }
    public decimal Amount { get; set; }
    public PaymentStatus Status { get; set; }
}

public class CustomerPaymentSummary
{
    public required string CustomerId { get; set; }
    public decimal TotalPaid { get; set; }
}

public class PaymentReportService
{
    public List<CustomerPaymentSummary> GetSuccessfulPaymentsByCustomer(
        List<PaymentInvoice> invoices,
        List<Payment> payments)
    {
        // Your code here
        var customerIdToTotalAmount = new Dictionary<string, decimal>();

        var successfulPayments = payments
        .Where(p => p.Status == PaymentStatus.Success);

        var getCustomerIdByInvoiceId = invoices
        .ToDictionary(x => x.InvoiceId);

        foreach (var payment in successfulPayments)
        {
            var foundInvoice = getCustomerIdByInvoiceId.TryGetValue(payment.InvoiceId, out var paymentInvoice);

            if (foundInvoice && paymentInvoice != null)
            {
                var key = paymentInvoice.CustomerId;
                if (!customerIdToTotalAmount.ContainsKey(key))
                {
                    customerIdToTotalAmount[key] = payment.Amount;
                }
                else
                {
                    customerIdToTotalAmount[key] += payment.Amount;
                }
            }

        }

        return customerIdToTotalAmount
        .Select(x => new CustomerPaymentSummary
        {
            CustomerId = x.Key,
            TotalPaid = x.Value
        })
        .ToList();

        throw new NotImplementedException();
    }
}