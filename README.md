# Sistema de Gestão de Consultas UVV

Trabalho AOP2 de Desenvolvimento Web Back-end. Aplicação C# com ASP.NET Core MVC, EF Core e SQL Server.

## Funcionalidades

- Cadastro de usuário com nome, e-mail, senha e data de cadastro.
- Login e logout por cookies.
- Cadastro, listagem, edição e exclusão das consultas do usuário autenticado.
- Especialidade, data/horário e descrição obrigatórios.
- Interface simples em português, usando Bootstrap local e botões verdes.

## Requisitos para executar

- Windows com SQL Server Express LocalDB instalado.
- SDK do .NET 10 (o projeto foi desenvolvido com o SDK 10.0.400).
- VS Code ou outro editor. Visual Studio Community não é obrigatório.
- Acesso à internet apenas para restaurar os pacotes na primeira execução.

## Banco de dados e connection string

A configuração está em `appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=GestaoConsultasUVV;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
}
```

No JSON, a barra invertida aparece duplicada. A conexão usa a conta do Windows; não contém usuário nem senha de SQL Server.

`TrustServerCertificate=True` é uma configuração para o banco local de desenvolvimento. Em hospedagem, configure o servidor e o certificado apropriados e mantenha credenciais fora do repositório, por exemplo na variável `ConnectionStrings__DefaultConnection`.

Se usar uma instância SQL Server Express completa, substitua o servidor por `.\\SQLEXPRESS`, conforme o nome da instância instalada.

## Executar pelo VS Code

Abra um terminal na pasta que contém `GestaoConsultasUVV.csproj`:

```powershell
dotnet restore
dotnet tool restore
dotnet ef database update
dotnet run --launch-profile http
```

Abra **http://localhost:5195**. Cadastre sua conta, entre e clique em **Cadastrar consulta**.

A migration inicial já está na pasta `Migrations`. O comando `dotnet ef database update` cria o banco, as tabelas `Usuarios` e `Consultas` e o histórico de migrations. Não é necessário gerar novamente a migration inicial.

Para uma futura alteração nos modelos:

```powershell
dotnet ef migrations add NomeDaAlteracao
dotnet ef database update
```

### Update-Database no Visual Studio

O equivalente ensinado nos materiais é `Update-Database`. No Console do Gerenciador de Pacotes do Visual Studio, instale as ferramentas caso ainda não estejam disponíveis:

```powershell
Install-Package Microsoft.EntityFrameworkCore.Tools -Version 10.0.11
Update-Database
```

Para gerar uma nova migration nesse console, use `Add-Migration NomeDaAlteracao`. No VS Code, use os comandos `dotnet ef` acima.

### HTTPS local (opcional)

```powershell
dotnet dev-certs https --trust
dotnet run --launch-profile https
```

Nesse perfil, abra **https://localhost:7190**. O perfil HTTP é usado apenas para testes locais; em produção a aplicação exige cookies seguros e redireciona para HTTPS.

## Testar com Postman

1. Inicie a aplicação.
2. No Postman Desktop, clique em **Import** e selecione [a coleção](postman/ConsultasUVV.postman_collection.json).
3. Nas variáveis da coleção, ajuste `baseUrl` para a URL exibida pelo terminal. O padrão é `http://localhost:5195`.
4. Mantenha o cookie jar ativado. A coleção desativa o acompanhamento automático de redirecionamentos; se sua versão não importar essa opção, desative **Automatically follow redirects** nas configurações das requisições.
5. Execute as requisições na ordem numérica ou use **Run collection**, com uma iteração.

A coleção contém 19 requisições e verificações: cadastro, senha incorreta, login, validação, criação, listagem, edição, exclusão, logout e acesso sem autenticação. Ela captura automaticamente os tokens antiforgery dos formulários e o ID da consulta. O Postman mantém os cookies de sessão.

O primeiro passo gera um e-mail fictício novo. A consulta de teste é excluída ao final; a conta fictícia permanece no banco. Se interromper o fluxo autenticado, apague os cookies de `localhost` antes de recomeçar pelo passo 01.

**Este é um projeto MVC, não uma API JSON.** Envie os dados em **Body > x-www-form-urlencoded**. As respostas normais são HTML (200), redirecionamento após gravação/login (302), solicitação sem token (400) e consulta inexistente ou de outra conta (404).

GET abre telas e consulta dados. POST realiza cadastro, login, criação, edição, exclusão e logout, como nos formulários MVC do material. GET de exclusão apenas mostra a confirmação.

Referências do Postman: [cookies](https://learning.postman.com/latest-v-12/docs/use/send-requests/response-data/cookies) e [redirecionamentos](https://learning.postman.com/help/resolve-issues/sending-requests/my-request-is-redirected-to-a-get-request).

## Organização e critérios da atividade

| Critério | Implementação |
| --- | --- |
| Arquitetura e SoC (0,4) | `Models`, `Views`, `Controllers`, `Data` e `ViewModels`; dependências recebidas pelo construtor. |
| Persistência (0,4) | `Data/AppDbContext.cs`, EF Core com SQL Server, relacionamento e migration Code First em `Migrations`. |
| Funcionalidades (0,6) | `ContaController` com cadastro/login/logout e `ConsultasController` com CRUD. |
| Segurança e validação (0,3) | Data Annotations, `ModelState.IsValid`, cookies, `[Authorize]`, hash de senha e tokens antiforgery. |
| Entrega e documentação (0,3) | README, repositório GitHub e coleção Postman. Vídeo demonstrativo pendente; PDF será preparado posteriormente. |

`ViewModels` contém apenas os campos recebidos pelos formulários. Assim, `UsuarioId`, `SenhaHash` e `DataCadastro` não são aceitos como campos editáveis enviados pelo navegador.

A senha é recebida no formulário e transformada pelo `PasswordHasher<Usuario>` do ASP.NET Core. O banco armazena `SenhaHash`, nunca a senha original. O e-mail é normalizado e possui índice único.

O contexto é registrado com `AddDbContext` no `Program.cs`. No pipeline, `UseAuthentication()` vem antes de `UseAuthorization()`. Além do atributo `[Authorize]`, todas as consultas são filtradas pelo ID do usuário autenticado, inclusive ao editar e excluir. O filtro `AutoValidateAntiforgeryToken` valida os tokens dos formulários POST.

A data de cadastro é gravada em UTC. O horário das consultas é informado e exibido como horário local, sem conversão de fuso. Datas passadas são aceitas, pois o enunciado não exige bloqueio e permite manter registros históricos. O sistema registra consultas; não faz integração com clínicas nem reserva horários de profissionais.

## Relação com os materiais da disciplina

A implementação segue os PDFs fornecidos na pasta `DevWeb`:

- **Conhecendo Detalhes do ASP.NET Core MVC**: MVC e SoC (p. 3–4), configuração e injeção de dependência (p. 12–13).
- **Construção de uma Aplicação com ASP.NET Core MVC**: DbContext, connection string e DI (p. 9–12), migrations e Update-Database (p. 12–15), formulários e Data Annotations (p. 16–24).
- **Segurança, autenticação e autorização com ASP.NET MVC**: saída Razor codificada (p. 6–7), tokens antiforgery (p. 27–30), autenticação por cookies e ordem do middleware (p. 38–39), Authorize/AllowAnonymous (p. 44–47) e hash de senhas (p. 48–50).

A adaptação principal é executar os comandos pelo terminal do VS Code e usar .NET 10, disponível no ambiente. Mantém-se uma única aplicação MVC com acesso ao EF Core pelos controladores.

## Verificação manual

Em 08/09/2026, a aplicação compilou sem avisos ou erros e o banco foi criado pela migration inicial. Foram aprovadas 29 verificações HTTP com SQL Server real (incluindo CRUD, validações, CSRF, XSS e isolamento entre usuários). Os scripts e as 19 requisições da coleção também foram exercitados por um executor local, com 34 asserções aprovadas; a importação na interface do Postman fica disponível para teste pela autora.

As telas inicial e de consulta, o login e as mensagens de validação também foram conferidos no navegador. Os testes utilizaram contas fictícias terminadas em `@example.invalid` e removeram suas consultas ao final.

- Cadastre uma conta, teste senha incorreta e depois faça login.
- Crie uma consulta, atualize os campos e confirme a alteração na lista.
- Abra a confirmação de exclusão, cancele e confirme que o registro continua existindo.
- Exclua pelo botão de confirmação e confira a listagem.
- Entre com outra conta: ela não deve ver, editar nem excluir consultas da primeira.
- Tente formulários vazios e e-mail inválido.
- Saia da conta e acesse `/Consultas`: deve redirecionar para login.

## Problemas comuns

- **LocalDB não encontrado:** conclua a instalação de `SqlLocalDB.msi` e abra novamente o terminal.
- **Banco/tabela não encontrado:** execute `dotnet ef database update` com o mesmo usuário Windows que executará o site.
- **Arquivo em uso ao compilar:** pare a aplicação com Ctrl+C antes de compilar novamente.
- **Porta ocupada:** pare a instância anterior ou use `dotnet run --launch-profile http --urls http://localhost:5196` e ajuste `baseUrl` no Postman.
- **400 no Postman:** execute novamente o GET que captura o token, mantendo os cookies. Depois do login, obtenha um novo token.
- **302 no Postman:** é esperado após POST bem-sucedido; consulte o cabeçalho `Location`.
- **Erro na restauração:** confira a conexão à internet e execute `dotnet restore` e `dotnet tool restore`.

## Entrega

Repositório: https://github.com/livia-rosario/AOP2-Sistema-de-Gestao-de-Consultas-UVV

**Vídeo demonstrativo: PENDENTE.** Antes da entrega, substitua esta indicação pelo link acessível do vídeo (YouTube, Loom ou similar). Mostre cadastro, login, criação, listagem, edição e exclusão de consulta.

O PDF para o portal será preparado posteriormente, com o nome da participante e o link do repositório. Os commits e o envio ao GitHub serão feitos pela autora.
