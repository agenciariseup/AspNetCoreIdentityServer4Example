using IdentityModel.Client;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace AspNetCoreIdenityServer4Example.Client
{
    public class Program
    {
        public static void Main(string[] args) => MainAsync().GetAwaiter().GetResult();

        private static async Task MainAsync()
        {
            // aqui nós pegamos as configurações do nosso servidor através da URL do mesmo.
            // precisamos pegar o EndPoint do token para podermos fazer o login.
            var disco = await DiscoveryClient.GetAsync("http://localhost:5000");
            
            // nesta parte, temos um exemplo de requisição com o tipo "password" 
            // esta é a forma mais comum
            var tokenClient = new TokenClient(disco.TokenEndpoint, "password.client", "secret");
            var tokenResponse = await tokenClient.RequestResourceOwnerPasswordAsync("manaces", "123456", "api1");

            // Se tiver um erro, nós escrevemos no console
            if (tokenResponse.IsError)
            {
                Console.WriteLine(tokenResponse.Error);
                Console.ReadKey();
                return;
            }

            // caso não tenha erro, escrevemos o token de acesso bem como as configurações e
            // permissões que o usuário logado tem
            Console.WriteLine(tokenResponse.Json);
            Console.WriteLine("\n\n");

            // agora chamamos a api segura passando um token de acesso fornecido pelo servidor quando logamos
            // dessa forma, precisamos passa-lo no header da requisição
            // que é exatamente o que fazemos nessa parte
            var client = new HttpClient();
            client.SetBearerToken(tokenResponse.AccessToken);

            var response = await client.GetAsync("http://localhost:5000/identity");
            if (!response.IsSuccessStatusCode)
            {
                // caso tenha dado erro, nós mostramos qual o código do erro
                // se foi por causa de autorização ou algo do tipo
                Console.WriteLine(response.StatusCode);
                Console.ReadKey();
            }
            else
            {
                // se deu tudo certo, exibimos aquele retorno das claims que falei
                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine(JArray.Parse(content));
                Console.ReadKey();
            }
        }
    }
}
