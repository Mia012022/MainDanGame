using System;
using System.Collections.Generic;

namespace DanGame.Models;

public partial class CreditCardInfo
{
    public int UserId { get; set; }

    public string PaymentMethod { get; set; } = null!;

    public string CardNumber { get; set; } = null!;

    public byte ExpiryMonth { get; set; }

    public short ExpiryYear { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string City { get; set; } = null!;

    public string BillingAddress { get; set; } = null!;

    public string PostalCode { get; set; } = null!;

    public string? BillingAddress2 { get; set; }

    public string PhoneNumber { get; set; } = null!;

    public virtual User ? User { get; set; } = null!;
}
