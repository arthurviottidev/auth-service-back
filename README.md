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