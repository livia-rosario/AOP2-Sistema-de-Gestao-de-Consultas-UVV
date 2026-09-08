-- Conexão: (localdb)\MSSQLLocalDB
-- Autenticação do Windows. Este arquivo apenas consulta dados.
USE [GestaoConsultasUVV];
GO

-- 1. Tabelas existentes
SELECT TABLE_SCHEMA AS Esquema, TABLE_NAME AS Tabela
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_TYPE = 'BASE TABLE'
ORDER BY TABLE_NAME;
GO

-- 2. Usuários (não exibe o hash da senha)
SELECT Id, Nome, Email, DataCadastro
FROM dbo.Usuarios
ORDER BY Id DESC;
GO

-- 3. Consultas e o usuário ao qual pertencem
SELECT c.Id, c.Especialidade, c.DataHora, c.Descricao,
       c.UsuarioId, u.Nome AS NomeUsuario
FROM dbo.Consultas AS c
INNER JOIN dbo.Usuarios AS u ON u.Id = c.UsuarioId
ORDER BY c.DataHora, c.Id;
GO

-- 4. Estrutura das colunas, incluindo o campo SenhaHash
SELECT TABLE_NAME AS Tabela, COLUMN_NAME AS Coluna,
       DATA_TYPE AS Tipo, CHARACTER_MAXIMUM_LENGTH AS Limite,
       IS_NULLABLE AS AceitaNulo
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME IN ('Usuarios', 'Consultas')
ORDER BY TABLE_NAME, ORDINAL_POSITION;
GO

-- 5. Migration aplicada pelo Entity Framework Core
SELECT MigrationId, ProductVersion
FROM dbo.__EFMigrationsHistory;
GO
