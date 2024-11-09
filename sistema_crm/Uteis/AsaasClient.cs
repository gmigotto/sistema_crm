using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using static System.Net.WebRequestMethods;

public class AsaasClient {
        private readonly string _token;
        private readonly string _url;
        private readonly string _env;

        public AsaasClient()
        {
            _token = "$aact_YTU5YTE0M2M2N2I4MTliNzk0YTI5N2U5MzdjNWZmNDQ6OjAwMDAwMDAwMDAwMDAwNzY3NTE6OiRhYWNoXzQyMGE0NWVhLTU3MmYtNGI4OC1hZDFkLWZlM2YxODIyNDFjNQ==";
            _url = "https://sandbox.asaas.com";
            _env = "LOCAL";
        }

        private Dictionary<string, object> BuildPayCardPayload(Dictionary<string, object> payload)
    {
        var cardInfo = payload["card_info"] as Dictionary<string, object>;
        var cardHolderInfo = payload["card_holder_info"] as Dictionary<string, object>;

        if (cardInfo == null || cardHolderInfo == null)
        {
            throw new ArgumentException("Invalid payload format. 'card_info' or 'card_holder_info' is not a dictionary.");
        }

        return new Dictionary<string, object>
        {
        { "billingType", "CREDIT_CARD" },
        { "creditCard", new Dictionary<string, object>
            {
                { "holderName", cardInfo["name"].ToString() },
                { "number", cardInfo["number"].ToString().Replace(" ", "") },
                { "expiryMonth", cardInfo["expiry_month"] },
                { "expiryYear", $"20{cardInfo["expiry_year"]}" },
                { "ccv", cardInfo["cvv"] }
            }
        },
        { "creditCardHolderInfo", new Dictionary<string, object>
            {
                { "name", cardHolderInfo["name"].ToString() },
                { "cpfCnpj", cardHolderInfo["cpf"].ToString() },
                { "email", cardHolderInfo["email"].ToString() },
                { "postalCode", cardHolderInfo["cep"].ToString() },
                { "addressNumber", cardHolderInfo["number"].ToString() },
                { "phone", cardHolderInfo["phone"].ToString() }
            }
        },
        { "customer", payload["customer"] },
        { "value", payload["total_value"] },
        { "dueDate", DateTime.Now.AddDays(1).ToString("yyyy-MM-dd") }
    };
    }

        private Dictionary<string, object> BuildPayloadCustomer(Dictionary<string, object> customerInfo)
        {
            return new Dictionary<string, object>
        {
            { "name", customerInfo["name"] },
            { "cpfCnpj", customerInfo["cpf"] },
            { "mobilePhone", customerInfo["phone"] }
        };
        }

        public async Task<Dictionary<string, object>> CreateCustomerAsync(Dictionary<string, object> customerData)
        {
            var customerInfo = BuildPayloadCustomer(customerData);
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("access_token", _token);
                client.DefaultRequestHeaders.Add("accept", "application/json");

            var content = new StringContent(JsonSerializer.Serialize(customerInfo), System.Text.Encoding.UTF8, "application/json");
            Console.WriteLine("Conteúdo do JSON enviado:");
            Console.WriteLine(await content.ReadAsStringAsync()); // Exibe o JSON que está sendo enviado para a API

            var response = await client.PostAsync($"{_url}/api/v3/customers", content);
            Console.WriteLine("Status da resposta:");
            Console.WriteLine(response.StatusCode); // Exibe o status da resposta HTTP
            Console.WriteLine("Cabeçalhos da resposta:");
            Console.WriteLine(response.Headers); // Exibe os cabeçalhos da resposta

            var jsonString = await response.Content.ReadAsStringAsync();
            Console.WriteLine("Conteúdo da resposta JSON:");
            Console.WriteLine(jsonString); //

            return JsonSerializer.Deserialize<Dictionary<string, object>>(jsonString);
            }
        }

        private async Task<Dictionary<string, object>> CreateOrderPayCardAsync(Dictionary<string, object> orderData)
        {
            var orderInfo = BuildPayCardPayload(orderData);
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("access_token", _token);
                client.DefaultRequestHeaders.Add("accept", "application/json");

                var content = new StringContent(JsonSerializer.Serialize(orderInfo), System.Text.Encoding.UTF8, "application/json");
                var response = await client.PostAsync($"{_url}/api/v3/payments", content);
                var jsonString = await response.Content.ReadAsStringAsync();

                return JsonSerializer.Deserialize<Dictionary<string, object>>(jsonString);
            }
        }

        private Func<Dictionary<string, object>, Task<Dictionary<string, object>>> GetPaymentType(string paymentType)
        {
            return paymentType switch
            {
                "card" => CreateOrderPayCardAsync,
                _ => throw new ArgumentException("Invalid payment type"),
            };
        }

        public async Task<Dictionary<string, object>> CreateOrderAsync(Dictionary<string, object> orderData)
        {
            if (_env == "LOCAL" && (decimal)orderData["total_value"] > 5000)
            {
                orderData["total_value"] = 5000;
            }

            var paymentMethod = "card";
            var createOrderFunc = GetPaymentType(paymentMethod);

            return await createOrderFunc(orderData);
        }
    
}

