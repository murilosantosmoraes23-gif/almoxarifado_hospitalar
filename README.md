# Almoxarifado Hospitalar

## Integrantes

- Murilo dos Santos de Moraes
- Leonardo Teles Pereira

Curso: Ciência da Computação
Disciplina: Desenvolvimento de Software Visual

## Resumo

O Almoxarifado Hospitalar é uma API para gerenciar materiais de consumo setor
e requisições de setores de um hospital. Permite cadastrar materiais,
registrar colaboradores vinculados a setores e criar pedidos com vários
itens de consumo setor. O atendimento pode ser parcial, registrando as quantidades
efetivamente entregues e atualizando o estoque. O projeto foi desenvolvido
para fins acadêmicos, inspirado no funcionamento de um almoxarifado hospitalar.

## Tecnologias

- C# e .NET 8
- ASP.NET Core Minimal API
- Entity Framework Core 8
- SQLite
- Git e GitHub
- REST Client do VS Code para testes HTTP

## Entidades e relacionamentos

O banco contém cinco entidades:

- Setor: identifica o destino dos materiais.
- Usuario: representa um colaborador e seu setor.
- Material: armazena nome, unidade de medida, estoque e situação.
- Requisicao: registra solicitante, setor, data e situação do pedido.
- ItemRequisicao: vincula uma requisição a um material e registra
  as quantidades solicitada e atendida.

Um usuário pode estar associado a um setor. Cada requisição possui
um solicitante e um setor. Cada item pertence a uma requisição e referencia
um material previamente cadastrado.

## Funcionalidades

- Cadastro, listagem, edição e exclusão de materiais.
- Desativação de materiais pela edição.
- Cadastro inicial de oito setores.
- Consulta dos setores ativos.
- Cadastro de colaboradores vinculados a setores existentes.
- Armazenamento de senhas como hash.
- Criação de requisições com vários materiais.
- Listagem e consulta detalhada das requisições.
- Atendimento total, parcial ou sem entrega por falta de estoque.
- Baixa de estoque conforme as quantidades entregues.
- Proteção contra atendimento repetido e exclusão de materiais utilizados.

## Descrição das funcionalidades

### Materiais

O cadastro exige nome preenchido e estoque não negativo. Cada material
possui uma unidade de medida, como unidade, pacote ou resma. As quantidades
são contabilizadas nessa unidade, sem conversão automática.

A edição permite alterar os dados, o saldo e a situação do material.
A exclusão física é permitida somente quando o material não possui
vínculo com itens de requisição. Materiais com histórico podem ser
desativados pela edição.

### Setores e colaboradores

Os setores são cadastrados na inicialização, caso ainda não existam:
UTI Azul, UTI Verde, UTI Laranja, UTI Lilás, Ambulatório, Farmácia Central,
PS - Pronto Socorro e CC - Centro Cirúrgico.

O cadastro de colaborador exige um setor ativo existente e rejeita
e-mail já cadastrado. O perfil é definido pela API como Colaborador.
A senha é armazenada como hash e não é devolvida na resposta.

### Requisições

Uma requisição deve conter pelo menos um material ativo, quantidades
positivas e nenhum material repetido. O solicitante deve existir e
possuir um setor ativo.

O setor do colaborador é copiado para o pedido. A criação não reserva
nem desconta estoque. É permitido solicitar materiais sem estoque,
pois o atendimento pode ser parcial.

### Atendimento e estoque

O atendimento exige informar todos os materiais do pedido. A quantidade
entregue não pode ser negativa, superar a solicitada ou superar o estoque.

Quantidade zero representa falta de estoque e somente é aceita quando
o saldo do material é zero. Materiais inativos não podem ser entregues.

A operação registra as quantidades atendidas, desconta o estoque e
encerra o pedido como Atendida, AtendidaParcialmente ou NaoAtendida.
O saldo não entregue não permanece pendente: exige uma nova requisição.

A gravação ocorre em transação. Requisições encerradas não podem
ser atendidas novamente.

## Rotas

| Método | Caminho | Finalidade |
| --- | --- | --- |
| GET | /setores | Listar setores ativos |
| POST | /usuarios | Cadastrar colaborador |
| GET | /materiais | Listar materiais |
| POST | /materiais | Cadastrar material |
| PUT | /materiais/{id} | Editar ou desativar material |
| DELETE | /materiais/{id} | Excluir material sem histórico |
| POST | /requisicoes | Criar requisição |
| GET | /requisicoes | Listar requisições |
| GET | /requisicoes/{id} | Consultar pedido e itens |
| POST | /requisicoes/{id}/atender | Atender e encerrar pedido |

## Como executar

Pré-requisitos: SDK .NET 8, Git e, para utilizar Send Request,
VS Code com a extensão REST Client.

Execute os comandos na ordem:

    git clone https://github.com/murilosantosmoraes23-gif/almoxarifado_hospitalar.git
    cd almoxarifado_hospitalar
    dotnet tool restore
    dotnet restore
    dotnet tool run dotnet-ef database update
    dotnet run

A criação do banco deve ocorrer antes da primeira execução.
Os setores são cadastrados automaticamente ao iniciar a aplicação.

Confira o endereço exibido em "Now listening on" e ajuste a variável
@baseUrl do arquivo testes.http, se necessário.

Mantenha a API rodando em um terminal e utilize Send Request no
arquivo testes.http. Não é necessário um segundo terminal.

O banco SQLite é local e não é distribuído pelo Git. Em um banco novo,
cadastre o colaborador e os materiais antes de criar uma requisição.
Utilize os IDs retornados nas respostas: os IDs dos testes de exemplo
não são garantidos em todos os bancos.

Repetir um POST de cadastro pode criar outro registro. Consultas devem
ser feitas utilizando GET.

## Testes manuais realizados

- Cadastro, consulta e edição de material.
- Rejeição de nome vazio e estoque negativo.
- Retorno de erro ao editar material inexistente.
- Cadastro de colaborador com setor existente.
- Rejeição de cadastro com setor inexistente.
- Criação, listagem e consulta de requisição com dois materiais.
- Atendimento parcial: entrega de papel e copos sem estoque.
- Conferência da redução do papel de 10 para 7 resmas.
- Rejeição de um segundo atendimento da mesma requisição.
- Exclusão de material sem histórico.
- Bloqueio de exclusão de material vinculado a requisição.

Os testes foram realizados manualmente por HTTP.

## Escopo e limitações desta versão

Esta entrega é uma API acadêmica, sem interface gráfica própria.
Não possui login, autenticação ou autorização por perfil.
O identificador do solicitante é informado na requisição e não
comprova a identidade de quem envia o pedido.

O cadastro armazena hash de senha, mas isso não representa um sistema
de login implementado. As rotas não estão restritas a gestores.

Não foram implementados cancelamento, recusa com justificativa,
relatório de consumo ou identificação do gestor que realizou o atendimento.
O estoque ainda pode ser alterado diretamente pela edição de materiais.

## Colaboração

Murilo dos Santos de Moraes participou da estrutura da API, modelagem,
configuração do banco, migrations, setores, colaboradores, requisições,
atendimento, ajustes e testes.

Leonardo Teles Pereira implementou o CRUD inicial de materiais.

O desenvolvimento utilizou branches, commits e pull requests.
O histórico do repositório apresenta as contribuições e integrações.

## Uso de IA

Ferramenta utilizada: ChatGPT.

A ferramenta auxiliou no planejamento, nas explicações de C#,
na elaboração e revisão de trechos de código, na interpretação de erros,
na organização dos testes e na redação desta documentação.
