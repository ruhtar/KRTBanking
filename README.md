# KRT Bank

KRT Bank é uma **API de gerenciamento de contas**, desenvolvida em **.NET**, seguindo os princípios de **Domain-Driven Design (DDD)**, com **domínios ricos, objetos de valor**, e implementações de **testes unitários**.

O sistema é projetado para operações **CRUD** (GET, POST, PUT, DELETE) de contas, com **cache distribuído**, **mensageria assíncrona** e integração com serviços da AWS.

O projeto também possui uma Lambda chamada **AccountEventPublisher**, responsável por publicar eventos de exclusão, atualização e criação de `Accounts` em um tópico SNS da AWS. 

---

## Estrutura do projeto da API

O projeto está organizado em **5 projetos principais**, seguindo uma arquitetura em camadas:

* **KRTBank.API**: Camada de apresentação, responsável por expor os endpoints REST da API.
* **KRTBank.Application**: Contém serviços e DTOs da aplicação.
* **KRTBank.Domain**: Camada de domínio, com a entidade `Account`, objetos de valor, como `CPF` e `HolderName` e algumas regras de negócio.
* **KRTBank.Infrastructure**: Implementações de repositórios, integração com DynamoDB e Redis.
* **KRTBank.Tests**: Testes unitários garantindo mais confiabilidade à aplicação.

---

## Funcionalidades

* **CRUD de Accounts**

  * **GET**: Busca contas por Id e utiliza cache distribuído (Redis).
  * **POST**: Criação de novas contas.
  * **PUT**: Atualização de contas existentes.
  * **DELETE**: Remoção de contas.

* **Cache Distribuído**

  * Implementado com **Redis** via Docker.
  * Permite consultas mais rápidas e evita sobrecarga no banco de dados, reduzindo os custos de leitura.

* **Banco de Dados**

  * Utiliza **Amazon DynamoDB**.
  * Streams do DynamoDB capturam alterações nas contas.

* **Event-driven**

  * As alterações em contas disparam **streams** que são processadas por uma **AWS Lambda** (`AccountEventPublisher`).
  * Lambda trata o payload e publica eventos em um **tópico SNS**, disponibilizando-os para outros sistemas.

---

## Padrões e Boas Práticas

* **Domain-Driven Design (DDD)**

  * Domínio rico em `Account`, garantindo consistência de seu estado interno através dos construtores.
  * Objetos de valor (`Value Objects`) para manter consistência e validação de dados.

* **Repository Pattern**

  * Separação clara entre a lógica de negócio/aplicação e persistência.

* **Options Pattern**

  * Configurações do Redis injetadas via `IOptions`, garantindo tipagem forte para as configurações, bem como validação em tempo de compilação.

* **Result Pattern**

  * Operações retornam objetos de resultado (`Result`) com status e mensagens.

* **Testes**

  * Cobertura com **XUnit** e **Moq**, garantindo confiabilidade das regras de negócio e de aplicação.

---

## Arquitetura

Fluxo resumido:

1. API recebe requisições REST (`GET`, `POST`, `PUT`, `DELETE`).
2. Consulta ao **cache Redis** para `GET`.
3. Persistência e atualização no **DynamoDB**.
4. Streams do DynamoDB acionam **Lambda** (`AccountEventPublisher`).
5. Lambda processa payload e publica eventos no **SNS**.

---

## Tecnologias Utilizadas

* **C# e .NET 8**
* **AWS DynamoDB**
* **AWS Lambda**
* **AWS SNS**
* **Redis (via Docker)**
* **Domain-Driven Design (DDD)**
* **XUnit + Moq**
* **Patterns:** Repository, Options, Result