# Auth Service — Backend

API REST desenvolvida em ASP.NET Core (.NET 10) com o papel de Identity Provider central de um ecossistema de microsserviços. 
Todos os outros serviços do portfólio vão validar o JWT emitido por essa API.

## Stack

- ASP.NET Core 10
- Dapper com SQL puro, sem ORM
- PostgreSQL
- JWT + Refresh Token
- BCrypt para hash de senha
- FluentValidation
- Swagger

## Arquitetura

O projeto segue uma separação em quatro camadas:

- `AuthService.Domain` — entidades e interfaces
- `AuthService.Application` — serviços, DTOs e validators
- `AuthService.Infrastructure` — repositórios e acesso ao banco
- `AuthService.API` — controllers, middleware e configuração

## Funcionalidades

- Registro de usuário
- Login com retorno de JWT e refresh token
- Refresh de token
- Recuperação de senha por e-mail
- Controle de roles (admin/user)
- Endpoint `GET /api/auth/me` para validação de sessão
- Middleware JWT reutilizável por outros serviços
- Rotas administrativas para gestão de usuários

## Como rodar localmente

Pré-requisitos: .NET 10 e PostgreSQL.

1. Clone o repositório
2. Crie o banco de dados e execute o script em `Database/V1__initial_schema.sql`
3. Configure os secrets da aplicação dentro de `AuthService.API` usando `dotnet user-secrets set` para as chaves `ConnectionStrings:DefaultConnection`, `Jwt:Secret`, `Email:SenderEmail` e `Email:SenderPassword`
4. Rode a aplicação com `dotnet run --project AuthService.API`
5. Acesse a documentação em `http://localhost:5186/swagger`

## Endpoints principais

| Método | Rota | Descrição |
|--------|------|-----------|
| POST | /api/auth/register | Registro de usuário |
| POST | /api/auth/login | Login |
| POST | /api/auth/refresh | Refresh de token |
| POST | /api/auth/forgot-password | Solicitação de recuperação de senha |
| POST | /api/auth/reset-password | Redefinição de senha |
| GET | /api/auth/me | Dados do usuário autenticado |
| GET | /api/user | Listagem de usuários (admin) |
| PATCH | /api/user/{id}/activate | Ativar usuário (admin) |
| PATCH | /api/user/{id}/deactivate | Desativar usuário (admin) |
| PATCH | /api/user/{id}/role | Atualizar role (admin) |


# Database Migrations

Migrations escritas em SQL.

## Convenção de nomenclatura
`V{versão}__{descrição}.sql`  
Exemplo: `V1__initial_schema.sql`, `V2__add_user_avatar.sql`

## Como aplicar
Abra o pgAdmin, selecione o banco `auth_service` e execute o arquivo no Query Tool.

## Migrations
| Versão | Arquivo | Descrição |
|--------|---------|-----------|
| V1 | V1__initial_schema.sql | Criação das tabelas iniciais |