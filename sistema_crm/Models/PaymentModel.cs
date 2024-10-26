namespace sistema_crm.Models
{
    public class PaymentModel
    {
        public int Id { get; set; }
        public string? NumeroCartaoDeCredito { get; set; }
        public string? MesExpiracao { get; set; }
        public string? AnoExpiracao { get; set; }
        public string? cvv { get; set; }

        public string? NomeTitular { get; set; }
        public string? Cpf { get; set; }

        public string? Email { get; set; }
        public string? Cep { get; set; }

        public string? NumeroCasa { get; set; }

        public string? Telefone { get; set; }

        public string? valor { get; set; }


    }
}