# DataFlow — Documentação de Funcionamento

Esta documentação descreve o funcionamento do DataFlow

---

# FrontEnd

## 1. Funcionamento

O FrontEnd é responsável pela interação com o usuário e pela apresentação do processo de importação.

O fluxo começa quando o usuário seleciona um arquivo `.xlsx`.

A partir desse momento, o FrontEnd:

1. recebe o arquivo;
2. verifica se a extensão é `.xlsx`;
3. calcula um SHA-256 para uso exclusivo do cache local;
4. consulta o IndexedDB para verificar se o mesmo conteúdo já foi validado;
5. caso não exista cache, envia o arquivo para o BackEnd;
6. apresenta o resultado das validações;
7. monta a grid com os registros retornados;
8. mostra os registros válidos e os registros com erro;
9. permite consultar os erros de cada registro;
10. habilita a confirmação apenas quando não existem erros;
11. solicita ao BackEnd a geração do JSON;
12. exibe o JSON visualmente ou realiza o download do arquivo retornado.

O FrontEnd não lê nem valida o conteúdo do Excel para aplicar regras de negócio.

A origem das informações apresentadas na grid é sempre:

```text
BackEnd
ou
Cache local de uma resposta anteriormente retornada pelo BackEnd
```

## 2. Regra

As regras do FrontEnd são regras de interface e de fluxo.

### Seleção do arquivo

O arquivo deve possuir extensão:

```text
.xlsx
```

Essa verificação serve apenas como proteção inicial da interface. A validação oficial continua sendo realizada no BackEnd.

### Exibição das etapas

O fluxo é cumulativo:

```text
Upload
↓
Validação
↓
Pré-visualização
```

As etapas anteriores continuam visíveis à medida que o usuário avança.

### Cache local

O FrontEnd calcula um SHA-256 do conteúdo do arquivo exclusivamente para identificar arquivos já processados no navegador.

O hash do FrontEnd serve somente para o IndexedDB. Ele não é utilizado como hash oficial do JSON final.

### Confirmação

Se existir pelo menos um registro com:

```text
status = erro
```

o botão **Confirmar** permanece desabilitado.

A interface não revalida os campos. Ela utiliza o status recebido do BackEnd.

### Geração do JSON

O FrontEnd não monta o JSON final.

Quando o usuário solicita **Gerar JSON Visual** ou **Gerar Arquivo JSON**, o arquivo XLSX original é enviado novamente ao BackEnd.

O FrontEnd apenas recebe o JSON pronto.

## 3. Validação

A validação realizada diretamente pelo FrontEnd é mínima.

### Validação local

O FrontEnd verifica apenas a extensão `.xlsx`.

Essa validação evita que o usuário envie por engano outro tipo de arquivo.

### Validação de negócio

As regras de negócio não são executadas no FrontEnd.

O FrontEnd apenas apresenta os resultados retornados pelo BackEnd.

A Tela 2 apresenta:

```text
Estrutura da planilha
Número mínimo de registros
Colunas obrigatórias
```

A Tela 3 apresenta os registros com:

```text
Válido
ou
Erro
```

Quando um registro possui erro, o FrontEnd recebe também a lista de problemas e a apresenta ao usuário.

## 4. Comportamento

### Arquivo ainda não validado

```text
Usuário seleciona arquivo
↓
FrontEnd calcula hash local
↓
Cache não encontrado
↓
Usuário inicia validação
↓
Arquivo é enviado ao BackEnd
↓
Resultado é recebido
↓
Grid é montada
```

### Arquivo já validado

```text
Usuário seleciona arquivo
↓
FrontEnd calcula hash local
↓
Cache encontrado
↓
Resposta anterior é recuperada
↓
Tela de validação é apresentada como concluída
↓
Grid é carregada
```

Nesse caso não é necessário executar novamente o endpoint de validação.

### Registro com erro

O registro permanece visível na grid.

Seu status é apresentado como **Erro**.

Ao clicar no status, o usuário visualiza todos os problemas retornados pelo BackEnd.

### Cancelamento

Ao cancelar uma importação:

- o arquivo atual é removido da operação;
- a validação desaparece;
- a grid desaparece;
- a tela de upload volta ao estado inicial.

O cache IndexedDB não é apagado.

### Geração do JSON

Quando todos os registros estão válidos:

```text
Usuário confirma
↓
Escolhe JSON Visual ou Arquivo JSON
↓
FrontEnd envia novamente o XLSX ao BackEnd
↓
BackEnd gera o JSON
↓
FrontEnd recebe a resposta
```

No JSON Visual, o FrontEnd apenas formata a apresentação.

No download, o FrontEnd cria o arquivo a partir da resposta recebida, sem alterar os dados.

---

# BackEnd

## 1. Funcionamento

O BackEnd é responsável por toda a lógica de validação e pela geração oficial do JSON.

Existem dois fluxos principais:

```text
POST /api/importacoes/validar
POST /api/importacoes/gerar-json
```

### Endpoint de validação

O endpoint `POST /api/importacoes/validar` recebe o XLSX e:

1. verifica o request;
2. abre a planilha;
3. utiliza a primeira worksheet;
4. valida a estrutura do arquivo;
5. valida a quantidade mínima de registros;
6. valida as colunas obrigatórias;
7. se o arquivo estiver correto, percorre os registros;
8. valida cada registro;
9. retorna as validações, os registros, os status e os erros encontrados.

### Endpoint de geração

O endpoint `POST /api/importacoes/gerar-json` recebe novamente o XLSX original.

O BackEnd:

1. lê novamente o arquivo;
2. reutiliza o mesmo método de validação;
3. impede a geração se a Tela 2 falhar;
4. impede a geração se qualquer registro possuir erro;
5. calcula o SHA-256 oficial do arquivo;
6. normaliza os dados;
7. monta o JSON final;
8. retorna o JSON ao FrontEnd.

O JSON nunca é criado a partir dos dados da grid.

O arquivo XLSX é sempre reprocessado pelo BackEnd antes da geração.

## 2. Regra

### Modelo da planilha

A planilha deve possuir exatamente as colunas:

```text
Matrícula*
Nome*
Curso*
Data de Nascimento*
Email
Mensalidade
```

A ordem deve ser mantida.

Não deve existir conteúdo em colunas adicionais além do modelo.

### Quantidade mínima

A planilha deve possuir no mínimo 10 registros.

Linhas completamente vazias são ignoradas.

### Campos obrigatórios

São obrigatórios:

```text
Matrícula
Nome
Curso
Data de Nascimento
```

Email e Mensalidade são opcionais.

### Matrícula

A matrícula:

- deve estar preenchida;
- deve possuir somente números;
- deve ter no mínimo 8 dígitos;
- permanece como texto para preservar zeros à esquerda.

### Nome

O nome deve estar preenchido.

### Curso

O curso deve estar preenchido.

### Data de nascimento

A data deve:

- estar preenchida;
- ser válida;
- representar idade mínima de 18 anos completos.

### Email

O email é opcional.

Nesta versão não existe validação de formato.

### Mensalidade

A mensalidade é opcional.

Quando preenchida:

- deve ser numérica;
- deve ser maior que zero.

### Regra de status

Depois que todas as regras de um registro são executadas:

```text
nenhum erro
→ status = valido
```

```text
um ou mais erros
→ status = erro
```

O BackEnd não interrompe a validação no primeiro erro.

Todos os problemas encontrados no registro são retornados.

### Regra para geração do JSON

O JSON somente pode ser criado quando:

```text
Tela 2 aprovada
E
todos os registros válidos
```

Se qualquer registro possuir erro, o JSON não é gerado.

## 3. Validação

A validação é dividida em dois níveis.

### Validação do arquivo

Primeiro são executadas:

#### Estrutura da planilha

Verifica:

- cabeçalhos;
- ordem;
- quantidade de colunas;
- inexistência de conteúdo extra.

#### Número mínimo de registros

Verifica se existem pelo menos 10 linhas com conteúdo.

#### Colunas obrigatórias

Verifica a presença dos cabeçalhos obrigatórios.

Se qualquer uma dessas validações falhar:

```text
Sucesso = false
Registros = []
```

### Validação dos registros

Quando o arquivo é aprovado, cada linha é processada individualmente.

Cada registro recebe:

```text
dados
status
erros
```

Exemplo:

```json
{
  "matricula": "20300009",
  "nome": "Isabela Cristina Nunes",
  "curso": "Contabilidade",
  "dataNascimento": "25/03/2004",
  "email": "isabela@exemplo.com",
  "mensalidade": "ABC",
  "status": "erro",
  "erros": [
    {
      "campo": "Mensalidade",
      "mensagem": "A mensalidade deve ser um valor numérico."
    }
  ]
}
```

A resposta permite que o FrontEnd monte a grid sem repetir nenhuma regra de validação.

## 4. Comportamento

### Arquivo inválido no request

Quando:

- não existe arquivo;
- o arquivo está vazio;
- a extensão não é `.xlsx`;

o BackEnd rejeita a requisição.

### Arquivo com estrutura inválida

```text
o arquivo é recebido
↓
a validação é executada
↓
Sucesso = false
↓
nenhum registro é enviado para a grid
```

### Arquivo válido com registros inválidos

É possível existir:

```text
Sucesso = true
```

e, ao mesmo tempo:

```text
registro.status = erro
```

Isso significa que o arquivo pode ser processado e exibido, mas alguns registros precisam ser corrigidos.

Nesse cenário:

- a grid é exibida;
- os registros problemáticos permanecem visíveis;
- a confirmação fica bloqueada no FrontEnd;
- o endpoint de geração do JSON também bloqueia a criação do arquivo.

### Geração do JSON

Ao receber uma solicitação de geração:

```text
XLSX
↓
revalidação completa
↓
verificação de registros
↓
cálculo do SHA-256
↓
normalização
↓
JSON final
```

O JSON contém:

```text
versao
tipoOperacao
geradoEm
arquivoOrigem
quantidadeRegistros
registros
```

O campo `arquivoOrigem.sha256` é calculado pelo BackEnd diretamente a partir dos bytes do XLSX.

O hash calculado no navegador não é utilizado para compor o JSON.

### Normalização do JSON

Antes da resposta final:

```text
Matrícula
→ permanece string

Data de nascimento
→ yyyy-MM-dd

Email vazio
→ null

Mensalidade válida
→ número

Mensalidade vazia
→ null
```

Os campos `status` e `erros` não fazem parte do JSON final.

---

# Resumo

## FrontEnd

| Área          | Responsabilidade                                         |
| ------------- | -------------------------------------------------------- |
| Funcionamento | Controlar a interação e apresentar o fluxo               |
| Regra         | Controlar interface, cache, confirmação e apresentação   |
| Validação     | Somente extensão local; regras de negócio vêm do BackEnd |
| Comportamento | Cache hit/miss, grid, erros, cancelamento e geração      |

## BackEnd

| Área          | Responsabilidade                                           |
| ------------- | ---------------------------------------------------------- |
| Funcionamento | Processar XLSX, validar e gerar JSON                       |
| Regra         | Definir todas as regras de negócio                         |
| Validação     | Validar arquivo e registros                                |
| Comportamento | Retornar resultados, bloquear erros e gerar o JSON oficial |
