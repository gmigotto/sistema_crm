using System.Collections.Generic;

public class PaymentModel
{
    public string Description { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMethodId { get; set; } // ex: "visa", "master"
    public string Email { get; set; }

    public string Token { get; set; }

}
