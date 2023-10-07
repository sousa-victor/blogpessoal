using blogpessoal.Model;
using blogpessoalTeste.Factory;
using FluentAssertions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Xunit.Extensions.Ordering;

namespace blogpessoalTeste.Controller
{
    public class UserControllerTest : IClassFixture<WebAppFactory>
    {

        protected readonly WebAppFactory _factory;
        protected HttpClient _client;

        private readonly dynamic token;
        private string Id { get; set; } = string.Empty;

        public UserControllerTest(WebAppFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();

            token = GetToken();
        }

        private static dynamic GetToken()
        {
            dynamic data = new ExpandoObject();
            data.sub = "root@root.com";
            return data;
        }

        [Fact, Order(1)]
        public async Task DeveCriarUmUsuario()
        {
            var novoUsuario = new Dictionary<string, string>//()
            {
                { "nome", "Ingrid" },
                { "usuario", "ingrid@email.com" },
                { "senha", "12345678" },
                { "foto", "-" }
            };

            var usuarioJson = JsonConvert.SerializeObject(novoUsuario);
            var corpoRequisicao = new StringContent(usuarioJson, Encoding.UTF8, "application/json");

            var resposta = await _client.PostAsync("/usuarios/cadastrar", corpoRequisicao);

            resposta.EnsureSuccessStatusCode();

            resposta.StatusCode.Should().Be(HttpStatusCode.Created);
        }
        [Fact, Order(2)]
        public async Task NaoDeveCriarUsuarioDuplicado()
        {
            var novoUsuario = new Dictionary<string, string>
            {
                { "nome", "Juliana Andrews" },
                { "usuario", "juliana@email.com.br" },
                { "senha", "12345678" },
                { "foto", "-" }
            };

            var usuarioJson = JsonConvert.SerializeObject(novoUsuario);

            var corpoRequisicao = new StringContent(usuarioJson, Encoding.UTF8, "application/json");

            await _client.PostAsync("/usuarios/cadastrar", corpoRequisicao);

            //Enviar a segunda vez

            var resposta = await _client.PostAsync("/usuarios/cadastrar", corpoRequisicao);

            resposta.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact, Order(3)]
        public async Task DeveAtualizarUsuario()
        {
            // Criar Usuário
            var novoUsuario = new Dictionary<string, string>
            {
                { "nome", "Paulo Antunes" },
                { "usuario", "paulo@email.com.br" },
                { "senha", "12345678" },
                { "foto", "-" }
            };

            var postJson = JsonConvert.SerializeObject(novoUsuario);

            var corpoRequisicaoPost = new StringContent(postJson, Encoding.UTF8, "application/json");

            var respostaPost = await _client.PostAsync("/usuarios/cadastrar", corpoRequisicaoPost);

            var corpoRespostaPost = await respostaPost.Content.ReadFromJsonAsync<User>();

            if (corpoRespostaPost is not null)
            {
                Id = corpoRespostaPost.Id.ToString();
            }

            //Atualizar Usuário

            var atualizarUsuario = new Dictionary<string, string>
            {
                { "id", Id },
                { "nome", "Paulo Cesar Antunes" },
                { "usuario", "paulo_cesar@email.com.br" },
                { "senha", "12345678" },
                { "foto", "-" }
            };

            var updateJson = JsonConvert.SerializeObject(atualizarUsuario);

            var corpoRequisicaoUpdate = new StringContent(updateJson, Encoding.UTF8, "application/json");

            _client.SetFakeBearerToken((object)token);

            var respostaPut = await _client.PutAsync("/usuarios/atualizar", corpoRequisicaoUpdate);

            respostaPut.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact, Order(4)]
        public async Task DeveListarTodosOsUsuarios()
        {
            _client.SetFakeBearerToken((object)token);

            var resposta = await _client.GetAsync("/usuarios/all");

            resposta.StatusCode.Should().Be(HttpStatusCode.OK);

        }
    }
}
