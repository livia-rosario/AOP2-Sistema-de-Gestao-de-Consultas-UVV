# Sistema de Gestão de Consultas UVV

Aplicação web em C# para cadastrar usuários e gerenciar suas consultas. O projeto utiliza ASP.NET Core MVC, Entity Framework Core, SQL Server LocalDB e Bootstrap.

## Executar

Requisitos: Windows, SDK do .NET 10 e SQL Server Express LocalDB. Os pacotes do EF Core estão fixados na versão 10.0.11. A edição pode ser feita no VS Code.

No terminal, dentro da pasta que contém `GestaoConsultasUVV.csproj`:

```powershell
dotnet restore
dotnet tool restore
dotnet ef database update
dotnet run --launch-profile http
```

Abra **http://localhost:5195**. A aplicação precisa permanecer em execução durante o uso pelo navegador. Ctrl+C encerra o servidor.

Se preferir a porta 5196:

```powershell
dotnet run --launch-profile http --urls http://localhost:5196
```

Use a URL exibida no terminal. A mensagem `address already in use` significa que outro processo já ocupa a porta; não é necessário iniciar duas cópias do site.

## Estrutura do código

| Pasta ou arquivo | Responsabilidade |
| --- | --- |
| `Models/Usuario.cs` | Entidade de usuário, com nome, e-mail, hash da senha, data de cadastro e coleção de consultas. |
| `Models/Consulta.cs` | Entidade de consulta, com especialidade, data/horário, descrição e vínculo com o usuário. |
| `ViewModels` | Campos e validações dos formulários de cadastro, login e consulta. |
| `Controllers/ContaController.cs` | Cadastro, autenticação por cookies e saída da conta. |
| `Controllers/ConsultasController.cs` | Listagem, criação, edição e exclusão das consultas da conta autenticada. |
| `Controllers/HomeController.cs` | Página inicial e tratamento de erro. |
| `Data/AppDbContext.cs` | Acesso às entidades pelo EF Core e configuração do relacionamento e dos índices. |
| `Migrations` | Código que cria e atualiza a estrutura do banco. |
| `Views` | Páginas Razor e formulários HTML. |
| `wwwroot` | CSS, Bootstrap, jQuery e arquivos estáticos. |
| `Program.cs` | Registro de serviços, autenticação e pipeline HTTP. |
| `appsettings.json` | Connection string e configuração de logs. |

O fluxo MVC parte da requisição HTTP: a rota seleciona uma ação do controlador; a ação consulta ou modifica os dados pelo contexto e retorna uma View ou um redirecionamento. As Views apresentam os dados e os formulários.

## Banco de dados

A connection string fica em `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=GestaoConsultasUVV;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
  }
}
```

A conexão utiliza a conta do Windows. O banco se chama `GestaoConsultasUVV`; as tabelas são `dbo.Usuarios`, `dbo.Consultas` e `dbo.__EFMigrationsHistory`.

Um usuário pode ter várias consultas. `Consulta.UsuarioId` é a chave estrangeira que aponta para `Usuario.Id`. O contexto configura essa relação com `HasOne`, `WithMany` e `HasForeignKey`.

O índice único em `Usuario.Email` impede duplicidade no banco. O índice composto em `Consulta.UsuarioId` e `Consulta.DataHora` atende à consulta dos registros de uma conta por data. A relação configura exclusão em cascata caso um usuário seja removido do banco; não existe tela de exclusão de usuários.

A abordagem é Code First: os modelos definem as entidades e a migration `CriacaoInicial` cria o esquema. `SaveChangesAsync()` salva inclusões, alterações e exclusões de registros; migrations alteram a estrutura das tabelas.

Para aplicar as migrations:

```powershell
dotnet ef database update
```

No Console do Gerenciador de Pacotes do Visual Studio, o equivalente é **Update-Database**. Se necessário, instale as ferramentas do console com `Install-Package Microsoft.EntityFrameworkCore.Tools -Version 10.0.11`.

Para uma futura mudança nos modelos:

```powershell
dotnet ef migrations add NomeDaAlteracao
dotnet ef database update
```

A migration inicial já está no repositório; não deve ser gerada novamente na instalação.

### Visualizar as tabelas no VS Code

A extensão **SQL Server (mssql)** pode conectar ao servidor `(localdb)\MSSQLLocalDB` com **Windows Authentication** e banco `GestaoConsultasUVV`. O perfil **Consultas UVV - LocalDB** está em `.vscode/settings.json`.

As tabelas aparecem na árvore da extensão SQL Server, não na árvore de arquivos do projeto. O arquivo [sql/VisualizarBanco.sql](sql/VisualizarBanco.sql) contém consultas para listar tabelas, usuários, consultas e migrations.

Abra esse arquivo, conecte ao perfil **Consultas UVV - LocalDB**, selecione o SELECT desejado e use **Execute Query**. Para navegar pelas tabelas, expanda o banco e a pasta **Tables** na extensão SQL Server.

## Configuração e injeção de dependência

Em `Program.cs`, `AddControllersWithViews()` registra os componentes MVC e o filtro de validação antiforgery. `AddDbContext<AppDbContext>()` configura o provedor SQL Server a partir da connection string.

Os controladores recebem `AppDbContext` por seus construtores, em vez de criar a conexão manualmente. `ContaController` também recebe `IPasswordHasher<Usuario>`, registrado por `AddScoped`.

No pipeline, `UseRouting()` identifica a rota, `UseAuthentication()` reconhece o usuário pelo cookie e `UseAuthorization()` verifica o acesso. `MapControllerRoute()` usa o padrão `{controller=Home}/{action=Index}/{id?}`.

## Cadastro e autenticação

No cadastro, o controlador verifica `ModelState.IsValid`, normaliza o e-mail e verifica se ele já existe. Depois cria `Usuario`, calcula `SenhaHash` com `PasswordHasher<Usuario>` e salva o registro.

No login, o usuário é localizado pelo e-mail e a senha é verificada por `VerifyHashedPassword`. Uma autenticação válida cria uma identidade com duas claims: `NameIdentifier` (ID) e `Name` (nome). `SignInAsync` emite o cookie e `SignOutAsync` o remove no logout.

O cookie é HttpOnly, tem validade configurada de 30 minutos e renovação deslizante. Não é criado como cookie persistente. O redirecionamento após login aceita apenas URLs locais.

A senha original existe somente na entrada do formulário; não é gravada no banco. `SenhaHash` guarda o resultado do hash. `DataCadastro` é preenchida no servidor em UTC.

## CRUD de consultas

O controlador de consultas possui `[Authorize]`. O ID da conta vem da claim `NameIdentifier`.

- **Listar:** filtra `Consultas` pelo usuário e ordena por `DataHora`. `AsNoTracking()` evita rastreamento desnecessário na leitura.
- **Criar:** recebe `ConsultaViewModel`, valida os campos, monta a entidade e atribui o ID da conta autenticada.
- **Editar:** localiza o registro pelo ID da consulta e pelo ID do usuário; altera somente especialidade, data/horário e descrição.
- **Excluir:** o GET mostra a confirmação; o POST localiza o registro da conta, remove e salva.

O método `MinhaConsulta` centraliza o filtro de propriedade usado na edição e exclusão. Um ID inexistente ou pertencente a outra conta retorna 404.

Depois de uma gravação bem-sucedida, a ação retorna um redirecionamento para a lista. `TempData` transporta a mensagem de sucesso para a próxima página.

## Validação

As entidades e os ViewModels usam Data Annotations como `Required`, `StringLength`, `EmailAddress` e `Compare`. `ModelState.IsValid` valida os dados no servidor antes da gravação.

Os ViewModels limitam os campos recebidos. `UsuarioId`, `SenhaHash` e `DataCadastro` não podem ser enviados como campos editáveis do formulário.

No formulário de consulta, `DataHora` é nullable para detectar o campo não preenchido. Na entidade, a data é obrigatória. O horário é informado e exibido como horário local, sem conversão de fuso. O sistema aceita registros com datas passadas.

Os Tag Helpers `asp-for`, `asp-validation-for` e `asp-validation-summary` vinculam os campos e mensagens aos ViewModels. A validação no navegador usa jQuery Validation e Unobtrusive Validation; a validação do servidor continua funcionando sem JavaScript.

O filtro `AutoValidateAntiforgeryToken` verifica os tokens dos formulários POST. A renderização Razor codifica os valores apresentados em HTML, e o EF Core parametriza as consultas de dados.

## Interface e Bootstrap

As páginas usam **Bootstrap 5**, carregado do próprio projeto em `wwwroot/lib/bootstrap/dist/css/bootstrap.min.css`. Não há dependência de CDN durante a navegação.

O layout compartilhado `Views/Shared/_Layout.cshtml` define o cabeçalho, navegação, área de mensagens e rodapé. `RenderBody()` insere o conteúdo de cada tela nesse layout.

| Classe Bootstrap | Uso nas páginas |
| --- | --- |
| `container` | Largura e alinhamento do conteúdo. |
| `card` e `p-4` | Área e espaçamento dos formulários. |
| `form-label` e `form-control` | Rótulos, campos e caixas de texto. |
| `btn btn-success` | Botões verdes de cadastro e salvamento. |
| `btn-outline-danger` | Ação de excluir. |
| `table`, `table-striped`, `table-responsive` | Listagem das consultas e rolagem em telas estreitas. |
| `alert alert-success` | Mensagens após cadastro, edição e exclusão. |
| `d-flex`, `flex-wrap` e `gap-2` | Organização dos botões e da navegação. |

`wwwroot/css/site.css` contém apenas ajustes de fonte, largura dos formulários, quebra de texto, foco e bordas de validação. A fonte usa a família Segoe UI, com fallback para a fonte do sistema. O JavaScript do Bootstrap não é carregado, pois as telas não usam componentes que dependem dele.

`Views/Consultas/_Formulario.cshtml` é uma View parcial compartilhada pelas telas de criação e edição, para manter os mesmos campos e regras visuais.

## Rotas e respostas HTTP

Esta aplicação é MVC: as respostas são HTML ou redirecionamentos, não JSON. Os formulários enviam `application/x-www-form-urlencoded`.

| Método | Rota | Operação |
| --- | --- | --- |
| GET / POST | `/Conta/Cadastro` | Abrir formulário / cadastrar usuário. |
| GET / POST | `/Conta/Login` | Abrir formulário / autenticar. |
| POST | `/Conta/Sair` | Encerrar autenticação. |
| GET | `/Consultas` | Listar consultas da conta. |
| GET / POST | `/Consultas/Criar` | Abrir formulário / salvar consulta. |
| GET / POST | `/Consultas/Editar/{id}` | Abrir formulário / atualizar consulta. |
| GET / POST | `/Consultas/Excluir/{id}` | Abrir confirmação / excluir consulta. |

Respostas comuns: **200** para páginas e formulários, **302** para redirecionamentos, **400** para token antiforgery ausente/inválido e **404** para consultas não acessíveis. Uma entrada inválida normalmente retorna o formulário com status 200 e mensagens de validação.

Para visualizar as requisições, abra **F12 → Network / Rede**, ative **Preserve log** e selecione **All** ou **Doc**. Ao salvar uma consulta, observe o **POST /Consultas/Criar → 302** seguido de **GET /Consultas → 200**.

Em Development, o nível `Information` de `Microsoft.AspNetCore.Hosting.Diagnostics` também mostra método, URL, status e duração das requisições no terminal. A configuração fica em `appsettings.Development.json` e não registra os corpos dos formulários.

## Configuração local e hospedagem

O perfil HTTP serve para desenvolvimento local. O perfil HTTPS usa `https://localhost:7190`:

```powershell
dotnet dev-certs https --trust
dotnet run --launch-profile https
```

Em produção, o pipeline ativa HSTS e redirecionamento HTTPS, e o cookie exige transporte seguro. A configuração `TrustServerCertificate=True` foi definida para o banco local; configure certificados adequados na hospedagem. Credenciais de outro servidor podem ser fornecidas por `ConnectionStrings__DefaultConnection`, sem gravar senhas no repositório.

O sistema mantém registros pessoais de consultas. Não integra agendas de clínicas, não reserva disponibilidade de profissionais e não realiza o cancelamento externo de atendimentos.

## Vídeo demonstrativo

Link do vídeo: a adicionar.
