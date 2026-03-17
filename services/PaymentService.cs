using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
namespace EthioHomes.services
{
    public class PaymentService

    {

       


        private readonly IConfiguration _config;

        public PaymentService(IConfiguration config)
        {
            _config = config;
        }

        public async Task<string> CreateChapaPayment(decimal amount, string currency, string email, string firstName, string lastName, string txRef, string returnUrl, string phoneNumber)
        {
            var secretKey = _config["Chapa:SecretKey"];
            if (string.IsNullOrEmpty(secretKey))
            {
                throw new Exception("Chapa secret key is missing.");
            }

            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", secretKey);

            var body = new
            {
                amount = amount.ToString("F2"),
                currency = currency,
                email = email,
                first_name = firstName,
                last_name = lastName,
                phone_number = phoneNumber,
                tx_ref = txRef,
                return_url = returnUrl,
                customization = new
                {
                    title = "EthioHomes",
                    description = "Telebirr Payment"
                }

            };

            var json = JsonConvert.SerializeObject(body);

            var response = await client.PostAsync("https://api.chapa.co/v1/transaction/initialize", // `/sandbox` for  using real key
                new StringContent(json, Encoding.UTF8, "application/json"));

            var result = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                //  Log response for debugging
                throw new Exception("Chapa error: " + result);
            }

            dynamic obj = JsonConvert.DeserializeObject(result);
            return obj.data.checkout_url;
        }

    }
}