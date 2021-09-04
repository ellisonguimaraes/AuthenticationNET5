# Autenticação e Autorização com .NET 5

Para o código utilizamos as dependências a seguir:

```C#
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="5.0.9" />
<PackageReference Include="Pomelo.EntityFrameworkCore.MySql" Version="5.0.1" />
<PackageReference Include="Swashbuckle.AspNetCore" Version="5.6.3" />
```

Foi necessário também a criação da tabela no banco de dados:

```C#
CREATE TABLE `users` (
	`id` INT(11) NOT NULL AUTO_INCREMENT,
	`email` VARCHAR(100) NOT NULL DEFAULT '0',
	`password` VARCHAR(130) NOT NULL DEFAULT '0',
	`refresh_token` VARCHAR(500) NULL DEFAULT '0',
	`refresh_token_expiry_time` DATETIME NULL DEFAULT NULL,
	PRIMARY KEY (`id`),
	UNIQUE `user_name` (`email`)
)
ENGINE=InnoDB DEFAULT CHARSET=LATIN1;

INSERT INTO `users` (`email`, `password`, `refresh_token`, `refresh_token_expiry_time`) VALUES
('ellison.guimaraes@gmail.com', '24-0B-E5-18-FA-BD-27-24-DD-B6-F0-4E-EB-1D-A5-96-74-48-D7-E8-31-C0-8C-8F-A8-22-80-9F-74-C7-20-A9', 'h9lzVOoLlBoTbcQrh/e16/aIj+4p6C67lLdDbBRMsjE=', '2020-09-27 17:30:49');
```

> A senha está criptografada com SHA256 e nesse caso é `admin123`. Segue JSON:
>
> ```json
> {
>     "email": "ellison.guimaraes@gmail.com",
>     "password": "admin123"
> }
> ```

E também a configuração da *ConnectionString* no `appsettings.json` é essencial:

```C#
{ 
	"ConnectionStrings": {
		"MysqlConnectionString": "Server=localhost;DataBase=authdb;Uid=root;Pwd=admin"
	}, 
    ...
```



# Diretórios

Há os seguintes diretórios no projeto:

```
│   
└─── AuthProject
    │   appsettings.Development.json
    │   appsettings.json
    │   AuthProject.csproj
    │   Program.cs
    │   Startup.cs
    │                      
    ├───Business
    │   │   LoginBusiness.cs
    │   │   
    │   └───Interfaces
    │           ILoginBusiness.cs
    │           
    ├───Controllers
    │       AuthController.cs
    │       
    ├───Models
    │   │   User.cs
    │   │   
    │   ├───Configuration
    │   │       TokenConfiguration.cs
    │   │       
    │   ├───Context
    │   │       ApplicationContext.cs
    │   │       
    │   └───DTO
    │           TokenDTO.cs
    │           UserDTO.cs
    │           
    │                   
    ├───Properties
    │       launchSettings.json
    │       
    ├───Repository
    │   │   UserRepository.cs
    │   │   
    │   └───Interfaces
    │           IUserRepository.cs
    │           
    ├───Scripts
    │       script.sql
    │       
    └───Services
        │   TokenService.cs
        │   
        └───Interfaces
                ITokenService.cs
```

Falaremos sobre alguns diretórios:

- O diretório ==Repository== contém a implementação do acesso a DB dos usuários. Nela contém:
    - Diretório de interfaces que contém a interface `IUserRepository`;
    - A implementação de `IUserRepository`, o `UserRepository`. Nele há métodos como:
        - `User ValidateCredentials(User user)`: método utilizado pelo método `ValidateCredentials(User user)` de `LoginBusiness` para validar um usuário `User` através do seu *login* e senha;
        - `User ValidateCredentials(string email)`: método utilizado pelo método `ValidateCredentials(TokenDTO token)` de `LoginBusiness` para validar um usuário `User` através do seu email;
        - `bool RevokeToken(string email)`: método de *logout*; 
        - `User RefreshUserInfo(User user)`: método usado para *update* do usuário, mais especificamente usado para atualizar o **Token** e do **RefreshToken**.
- O diretório ==Script== é o arquivo SQL necessário para executar no banco;
- O diretório ==Services== contém os serviços relacionados a geração de token. Nele contém:
    - Diretório de interfaces que contém a interface `ITokenService`;
    - A implementação de `ITokenService`, o `TokenService`. Nele a métodos como:
        - `string GenerateAccessToken(IEnumerable<Claim> claims)` que gera um novo **Token de Acesso** (`AccessToken`). Ele recebe uma lista de `Claim`;
        - `string GenerateRefreshToken()` que gera um novo **RefreshToken**;
        - `ClaimsPrincipal GetPrincipalFromExpiredToken(string token)` retorna as `ClaimsPrincipal` com base no token. Ele é usado no `LoginBusiness` pelo método `TokenDTO ValidateCredentials(TokenDTO token)` que recebe um **token**, ou seja, ao realizar um **RefreshToken**.
- O diretório ==Models== que contém os modelos usados no programa. Dentro dele temos:
    - O modelo `User`;
    - Mais três diretórios de modelo, são eles:
        - **DTO**: as classes de DTO `TokenDTO` e `UserDTO`;
        - **Context**: a classe de contexto do *EntityFramework* `ApplicationContext`;
        - **Configuration**: que contém a classe de configuração do Token `TokenConfiguration`, classe essa que é instanciada no `Startup.cs`, recebe os parâmetros de configuração do **token** do `appsettings.json` e é injetada via DI;
- O diretório ==Controller== que contém a classe `AuthController` referente as operações de autenticação do sistema. Nela contém os métodos:
    - `Signin` usado para entrar com login e senha;
    - `Refresh` que a partir do **RefreshToken** é gerado uma nova token de acesso **AccessToken**;
    - `Revoke` que realiza o logout, ou seja, exclui o **RefreshToken** do usuário no banco.
- O diretório ==Business== que trata da camada de negócio do sistema, e nela contém:
    - Um diretório de interfaces contendo a interface `ILoginBusiness`;
    - A implementação de `ILoginBusiness`, o `LoginBusiness`. Essa implementação contém os métodos:
        - `ValidateCredentials(UserDTO userDTO)` utilizado pelo `Signin` no *controller* para autenticar um `User` com base em seu *login* e senha;
        - `TokenDTO ValidateCredentials(TokenDTO token)` utilizado pelo `Refresh` no *controller* para gerar um novo **token** de acesso com base no **RefreshToken**;
        - `bool RevokeToken(string email)` utilizado pelo `Revoke` no *controller* para logout.



# Rotas

Existem 3 rotas para esta API:

## Signin ou Login

É usado a rota ==localhost:5000/api/auth/signin/== com usuário e senha enviado via BODY Params, sendo um JSON no formato:

```json
{
    "email": "ellison.guimaraes@gmail.com",
    "password": "admin123"
}
```

O retorno é uma `TokenDTO`:

```json
{
    "authenticated": true,
    "createdDate": "2021-09-03 23:12:10",
    "expirationDate": "2021-09-04 00:12:10",
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJqdGkiOiIyMWVhYjRkOWNjOGM0ODM4YTM3MjQ4YWQzZTEyODVmNCIsInVuaXF1ZV9uYW1lIjoiZWxsaXNvbi5ndWltYXJhZXNAZ21haWwuY29tIiwiZXhwIjoxNjMwNzI1MTI5LCJpc3MiOiJJc3N1ZXJBdXRoIiwiYXVkIjoiQXVkaWVuY2VBdXRoIn0.pI9dkQ9amIgi5lwfaOZmt6DsizIvbVutNlwNtBYq1mo",
    "refreshToken": "WYPLrcPQaeNKCxsEKALvWJLab1aX9NLUDXsjnXpxIoo="
}
```

## Refresh

É usado a rota ==localhost:5000/api/auth/refresh/== com um JSON no formato `TokenDTO` via BODY Params:

```json
{
     "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJqdGkiOiIyMWVhYjRkOWNjOGM0ODM4YTM3MjQ4YWQzZTEyODVmNCIsInVuaXF1ZV9uYW1lIjoiZWxsaXNvbi5ndWltYXJhZXNAZ21haWwuY29tIiwiZXhwIjoxNjMwNzI1MTI5LCJpc3MiOiJJc3N1ZXJBdXRoIiwiYXVkIjoiQXVkaWVuY2VBdXRoIn0.pI9dkQ9amIgi5lwfaOZmt6DsizIvbVutNlwNtBYq1mo",
    "refreshToken": "WYPLrcPQaeNKCxsEKALvWJLab1aX9NLUDXsjnXpxIoo="
}
```

O retorno é um novo `TokenDTO`:

```json
{
    "authenticated": true,
    "createdDate": "2021-09-03 23:14:22",
    "expirationDate": "2021-09-04 00:14:22",
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJqdGkiOiIyMWVhYjRkOWNjOGM0ODM4YTM3MjQ4YWQzZTEyODVmNCIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWUiOiJlbGxpc29uLmd1aW1hcmFlc0BnbWFpbC5jb20iLCJleHAiOjE2MzA3MjUyNjIsImlzcyI6Iklzc3VlckF1dGgiLCJhdWQiOlsiQXVkaWVuY2VBdXRoIiwiQXVkaWVuY2VBdXRoIl19.82veWwA8WJA1wKG0at1ybmXUshqK0w3pnHiFLUvnKTo",
    "refreshToken": "z85N5qUvr446X3sHyEVP0TgX6G/3qiZmaAt+zuK3J0I="
}
```

## Revoke

É usado a rota ==localhost:5000/api/auth/revoke/==, e para ele, é necessário estar logado. Para isso precisamos informar via **HEADER Params** a Key~Value:

```
Key: 	Authorization
Value: 	Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJqdGkiOiIyMWVhYjRkOWNjOGM0ODM4YTM3MjQ4YWQzZTEyODVmNCIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWUiOiJlbGxpc29uLmd1aW1hcmFlc0BnbWFpbC5jb20iLCJleHAiOjE2MzA3MjUyNjIsImlzcyI6Iklzc3VlckF1dGgiLCJhdWQiOlsiQXVkaWVuY2VBdXRoIiwiQXVkaWVuY2VBdXRoIl19.82veWwA8WJA1wKG0at1ybmXUshqK0w3pnHiFLUvnKTo
```

Onde o Value é a palavra `"Bearer {{AccessToken}}"`.

Se sucesso é retornado um 204(**No Content**).
