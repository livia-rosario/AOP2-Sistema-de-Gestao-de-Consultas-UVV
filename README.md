# 🩺 Sistema de Gestão de Consultas UVV

Aplicação web em **C# / ASP.NET Core MVC** para cadastro de usuários e gerenciamento de consultas médicas ou profissionais, com autenticação por cookies, persistência via **Entity Framework Core** e interface em **Bootstrap 5**.

---

## 👩‍💻 Autoria

**LÍVIA ROSÁRIO BARBOSA**

Matrícula: **202639318**

---

## 📋 Sumário

- [Requisitos](#-requisitos)
- [Como executar](#-como-executar)
- [Estrutura do projeto](#-estrutura-do-projeto)
- [Banco de dados](#-banco-de-dados)
- [Configuração e injeção de dependência](#-configuração-e-injeção-de-dependência)
- [Cadastro e autenticação](#-cadastro-e-autenticação)
- [CRUD de consultas](#-crud-de-consultas)
- [Validação e segurança](#-validação-e-segurança)
- [Interface (Bootstrap)](#-interface-bootstrap)
- [Rotas e respostas HTTP](#-rotas-e-respostas-http)
- [Configuração local e hospedagem](#-configuração-local-e-hospedagem)
- [Escopo do sistema](#-escopo-do-sistema)
- [Vídeo demonstrativo](#-vídeo-demonstrativo)

---

## ✅ Requisitos

| Item | Versão / Detalhe |
|---|---|
| Sistema operacional | Windows |
| SDK | .NET 10 |
| Banco de dados | SQL Server Express LocalDB |
| Pacotes EF Core | Fixados na versão `10.0.11` |
| Editor recomendado | VS Code |

---

## 🚀 Como executar

Dentro da pasta que contém `GestaoConsultasUVV.csproj`, rode em sequência:

```powershell
dotnet restore
dotnet tool restore
dotnet ef database update
dotnet run
```

Depois, acesse:

```
http://localhost:5195
```

> ⚠️ A aplicação precisa **permanecer em execução** no terminal enquanto você navega no site. Para encerrar, use `Ctrl+C`.

### Porta de execução

O projeto usa apenas **http://localhost:5195**, configurado em `Properties/launchSettings.json`.

Se aparecer `address already in use`, outro processo está usando a porta. Se o site já estiver rodando, use essa execução. Para reiniciá-lo, encerre a execução anterior com `Ctrl+C` e rode `dotnet run` novamente.

---

## 📁 Estrutura do projeto

| Pasta / Arquivo | Responsabilidade |
|---|---|
| `Models/Usuario.cs` | Entidade de usuário — nome, e-mail, hash da senha, data de cadastro e coleção de consultas |
| `Models/Consulta.cs` | Entidade de consulta — especialidade, data/horário, descrição e vínculo com o usuário |
| `ViewModels/` | Campos e validações dos formulários de cadastro, login e consulta |
| `Controllers/ContaController.cs` | Cadastro, autenticação por cookies e logout |
| `Controllers/ConsultasController.cs` | Listagem, criação, edição e exclusão das consultas da conta autenticada |
| `Controllers/HomeController.cs` | Página inicial e tratamento de erro |
| `Data/AppDbContext.cs` | Acesso às entidades via EF Core; configuração de relacionamentos e índices |
| `Migrations/` | Código que cria e atualiza a estrutura do banco |
| `Views/` | Páginas Razor e formulários HTML |
| `wwwroot/` | CSS, Bootstrap, jQuery e arquivos estáticos |
| `Program.cs` | Registro de serviços, autenticação e pipeline HTTP |
| `appsettings.json` | Connection string e configuração de logs |

**Fluxo MVC:** a requisição HTTP chega → a rota seleciona uma *action* do controller → a *action* consulta/modifica dados via `AppDbContext` → retorna uma *View* ou um redirecionamento → a *View* apresenta os dados e formulários.

---

## 🗄️ Banco de dados

### Connection String

Definida em `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=GestaoConsultasUVV;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
  }
}
```

A conexão usa a **conta do Windows** (Trusted Connection). O banco se chama `GestaoConsultasUVV` e contém as tabelas:

- `dbo.Usuarios`
- `dbo.Consultas`
- `dbo.__EFMigrationsHistory`

### Modelagem

- **Relacionamento:** um usuário pode ter várias consultas. `Consulta.UsuarioId` é a chave estrangeira para `Usuario.Id`, configurada no `AppDbContext` via `HasOne`, `WithMany` e `HasForeignKey`.
- **Índice único** em `Usuario.Email` — impede duplicidade de cadastro.
- **Índice composto** em `Consulta.UsuarioId` + `Consulta.DataHora` — otimiza a consulta dos registros de uma conta por data.
- **Exclusão em cascata:** se um usuário for removido do banco, suas consultas são removidas junto. *(Não existe tela para excluir usuários — essa remoção só ocorreria diretamente no banco.)*

### Abordagem Code First

Os modelos definem as entidades, e a migration `CriacaoInicial` cria o esquema do banco. `SaveChangesAsync()` grava inclusões, alterações e exclusões de registros; *migrations* alteram a estrutura das tabelas.

**Aplicar migrations existentes:**

```powershell
dotnet ef database update
```

> No Console do Gerenciador de Pacotes do Visual Studio, o comando equivalente é `Update-Database`. Se necessário, instale as ferramentas com `Install-Package Microsoft.EntityFrameworkCore.Tools -Version 10.0.11`.

**Criar uma nova migration** (para mudanças futuras nos modelos):

```powershell
dotnet ef migrations add NomeDaAlteracao
dotnet ef database update
```

> ℹ️ A migration inicial já está no repositório — **não precisa ser gerada novamente** na instalação.

### Visualizando as tabelas no VS Code

1. Instale a extensão **SQL Server (mssql)**.
2. Conecte ao servidor `(localdb)\MSSQLLocalDB` com Windows Authentication, banco `GestaoConsultasUVV`.
   - O perfil **"Consultas UVV - LocalDB"** já está salvo em `.vscode/settings.json`.
3. As tabelas aparecem na **árvore da extensão SQL Server** (não na árvore de arquivos do projeto) — expanda o banco → pasta `Tables`.
4. Use o arquivo `sql/VisualizarBanco.sql`, que já contém consultas prontas para listar tabelas, usuários, consultas e migrations. Abra o arquivo, conecte ao perfil salvo, selecione o `SELECT` desejado e rode com **Execute Query**.

---

## ⚙️ Configuração e injeção de dependência

Em `Program.cs`:

- `AddControllersWithViews()` registra os componentes MVC e o filtro de validação antiforgery.
- `AddDbContext<AppDbContext>()` configura o provedor SQL Server a partir da connection string.

Os controllers recebem `AppDbContext` **pelo construtor**, em vez de instanciar a conexão manualmente. O `ContaController` também recebe `IPasswordHasher<Usuario>`, registrado via `AddScoped`.

**Pipeline HTTP (ordem importa):**

```
UseRouting()        → identifica a rota
UseAuthentication() → reconhece o usuário pelo cookie
UseAuthorization()  → verifica o acesso
MapControllerRoute() → padrão {controller=Home}/{action=Index}/{id?}
```

---

## 🔐 Cadastro e autenticação

### Cadastro

1. O controller verifica `ModelState.IsValid`.
2. O e-mail é normalizado e checado quanto à duplicidade.
3. É criado um `Usuario`, com `SenhaHash` calculado por `PasswordHasher<Usuario>`.
4. O registro é salvo no banco.

### Login

1. O usuário é localizado pelo e-mail.
2. A senha é verificada com `VerifyHashedPassword`.
3. Se válida, é criada uma identidade com duas *claims*: `NameIdentifier` (ID) e `Name` (nome).
4. `SignInAsync` emite o cookie de autenticação; `SignOutAsync` o remove no logout.

### Detalhes do cookie e da senha

| Aspecto | Comportamento |
|---|---|
| Tipo de cookie | `HttpOnly` |
| Validade | 30 minutos, com renovação deslizante |
| Persistência | Não é um cookie persistente |
| Redirecionamento pós-login | Aceita apenas URLs locais |
| Senha em texto puro | Recebida no formulário e processada no servidor — **não é gravada no banco** |
| `SenhaHash` | Armazena o resultado do hash |
| `DataCadastro` | Preenchida no servidor, em UTC |

---

## 📅 CRUD de consultas

O `ConsultasController` possui `[Authorize]` — todas as ações exigem login. O ID da conta autenticada vem da *claim* `NameIdentifier`.

| Ação | Comportamento |
|---|---|
| **Listar** | Filtra consultas pelo usuário logado e ordena por `DataHora`. Usa `AsNoTracking()` para evitar rastreamento desnecessário na leitura. |
| **Criar** | Recebe `ConsultaViewModel`, valida os campos, monta a entidade e atribui o ID da conta autenticada. |
| **Editar** | Localiza o registro pelo ID da consulta **e** pelo ID do usuário; altera apenas especialidade, data/horário e descrição. |
| **Excluir** | GET mostra a tela de confirmação; POST localiza o registro da conta, remove e salva. |

O método `MinhaConsulta` centraliza o filtro de propriedade usado na edição e exclusão — um ID inexistente ou pertencente a outra conta retorna **404**.

Após uma gravação bem-sucedida, a ação redireciona para a listagem. `TempData` transporta a mensagem de sucesso para a próxima página.

---

## ✔️ Validação e segurança

- **Data Annotations** (`Required`, `StringLength`, `EmailAddress`, `Compare`) nas entidades e ViewModels.
- `ModelState.IsValid` valida os dados no servidor antes de qualquer gravação.
- **ViewModels limitam os campos recebidos** — `UsuarioId`, `SenhaHash` e `DataCadastro` nunca são editáveis via formulário.
- No formulário de consulta, `DataHora` é *nullable* (para detectar campo vazio), mas obrigatório na entidade. O horário é tratado como horário local, sem conversão de fuso. O sistema aceita datas passadas.
- **Tag Helpers** (`asp-for`, `asp-validation-for`, `asp-validation-summary`) vinculam campos e mensagens aos ViewModels.
- Validação client-side via jQuery Validation + Unobtrusive Validation; a validação server-side continua funcionando **mesmo sem JavaScript**.
- `AutoValidateAntiforgeryToken` verifica os tokens dos formulários POST.
- A renderização Razor codifica os valores em HTML e o EF Core parametriza as consultas — essas medidas ajudam a prevenir XSS e SQL Injection, respectivamente.

---

## 🎨 Interface (Bootstrap)

As páginas usam **Bootstrap 5**, carregado localmente em `wwwroot/lib/bootstrap/dist/css/bootstrap.min.css` — **sem dependência de CDN**.

A paleta é definida em variáveis CSS no `site.css`: petróleo profundo (`#0D5C56`) na navegação e nos cabeçalhos, menta (`#14B8A6`) nos botões principais e ícones, cinza suave (`#F4F7F6`) no fundo e grafite (`#192A27`) nos textos. Os botões menta usam texto grafite para manter a legibilidade. Mensagens de erro e ações de exclusão mantêm o vermelho.

O layout compartilhado `Views/Shared/_Layout.cshtml` define cabeçalho, navegação, área de mensagens e rodapé; `RenderBody()` insere o conteúdo de cada tela.

O logo é uma cruz em SVG, em `wwwroot/images/logo.svg`, reutilizada na navegação, na página inicial e no ícone da aba. A página inicial usa um cartão centralizado e responsivo com os links de acesso.

| Classe Bootstrap | Uso |
|---|---|
| `container` | Largura e alinhamento do conteúdo |
| `card`, `p-4` | Área e espaçamento dos formulários |
| `form-label`, `form-control` | Rótulos e campos de texto |
| `btn btn-success` | Botões de cadastro e salvamento |
| `btn-outline-danger` | Ação de excluir |
| `table`, `table-striped`, `table-responsive` | Listagem de consultas com rolagem em telas estreitas |
| `alert alert-success` | Mensagens após cadastro, edição e exclusão |
| `d-flex`, `flex-wrap`, `gap-2` | Organização de botões e navegação |

`wwwroot/css/site.css` contém ajustes de fonte (Segoe UI, com fallback do sistema), largura dos formulários, quebra de texto, foco e bordas de validação. A tela de login usa um cartão compacto com destaque verde, ícone de calendário, campos maiores e botão de largura inteira; esses estilos ficam restritos à classe `login-card`. O JavaScript do Bootstrap **não é carregado**, pois nenhuma tela usa componentes que dependem dele.

A View parcial `Views/Consultas/_Formulario.cshtml` é compartilhada entre as telas de criação e edição, mantendo os mesmos campos e regras visuais.

---

## 🌐 Rotas e respostas HTTP

Este é um projeto **MVC**: as respostas são HTML ou redirecionamentos — não há endpoints de API JSON. Formulários enviam `application/x-www-form-urlencoded`.

| Método | Rota | Operação |
|---|---|---|
| GET / POST | `/Conta/Cadastro` | Abrir formulário / cadastrar usuário |
| GET / POST | `/Conta/Login` | Abrir formulário / autenticar |
| POST | `/Conta/Sair` | Encerrar autenticação |
| GET | `/Consultas` | Listar consultas da conta |
| GET / POST | `/Consultas/Criar` | Abrir formulário / salvar consulta |
| GET / POST | `/Consultas/Editar/{id}` | Abrir formulário / atualizar consulta |
| GET / POST | `/Consultas/Excluir/{id}` | Abrir confirmação / excluir consulta |

### Códigos de resposta comuns

| Código | Situação |
|---|---|
| `200` | Páginas e formulários carregados normalmente |
| `302` | Redirecionamento após ação bem-sucedida |
| `400` | Token antiforgery ausente ou inválido |
| `404` | Consulta não encontrada ou não pertence à conta |

> Uma entrada inválida em formulário normalmente retorna **200**, com o próprio formulário reexibido junto das mensagens de validação.

### Depurando requisições

- No navegador: `F12` → aba **Network/Rede** → ative **Preserve log** → filtre por `All` ou `Doc`.
  - Ao salvar uma consulta, o esperado é: `POST /Consultas/Criar → 302` seguido de `GET /Consultas → 200`.
- No terminal (ambiente de **Development**): o nível `Information` de `Microsoft.AspNetCore.Hosting.Diagnostics` mostra método, URL, status e duração de cada requisição. Configurado em `appsettings.Development.json` — **os corpos dos formulários não são registrados**.

---

## 🖥️ Configuração local e hospedagem

O único perfil de execução é **HTTP**, em `http://localhost:5195`, para desenvolvimento local. Uma eventual hospedagem deve configurar HTTPS no servidor.

### Em produção

- O pipeline ativa **HSTS** e redirecionamento HTTPS.
- O cookie de autenticação passa a exigir transporte seguro.
- `TrustServerCertificate=True` foi definido apenas para o banco **local** — configure certificados adequados no ambiente de hospedagem.
- Credenciais de outro servidor podem ser fornecidas via variável de ambiente `ConnectionStrings__DefaultConnection`, **sem gravar senhas no repositório**.

---

## 🎯 Escopo do sistema

O sistema mantém **registros pessoais de consultas**. Ele **não**:

- integra agendas de clínicas;
- reserva disponibilidade de profissionais;
- realiza cancelamento externo de atendimentos.

---

## 🎥 Vídeo demonstrativo

📎 Link do vídeo: *a adicionar*
