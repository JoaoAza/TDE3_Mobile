# SQLApp — TDE de Banco de Dados Relacional com .NET MAUI

Adaptação do SQLApp fornecido em aula para o modelo Cliente, Produto, Compra e ItemCompra do TDE.

## Abrir e executar

1. Extraia este ZIP em uma pasta.
2. Abra `SQLApp.slnx` no Visual Studio com suporte a .NET 10 e desenvolvimento .NET MAUI, como no projeto original. Também é possível abrir `SQLApp/SQLApp.csproj`.
3. Aguarde a restauração dos pacotes NuGet. Foram mantidas as versões da base: `sqlite-net-pcl` 1.11.285, `SQLiteNetExtensions.Async` 2.1.0 e `Microsoft.Extensions.Logging.Debug` 10.0.0.
4. Escolha Windows Machine ou um dispositivo/emulador Android configurado e execute.
5. Não é necessário instalar SQL Server nem criar o banco manualmente. O arquivo `compras.db3` é criado no diretório de dados do aplicativo no primeiro acesso.

O arquivo `pessoas.db3` da aplicação anterior não é alterado. A primeira execução desta versão começa sem registros. Este pacote contém o código-fonte, não um APK ou executável compilado.

## Como utilizar

1. No menu principal, abra **Cadastro de Clientes** e inclua um cliente.
2. Abra **Cadastro de Produtos** e inclua um produto com preço e estoque.
3. Abra **Compra**, selecione o cliente, informe a data e clique em **Incluir**.
4. A compra recém-criada fica selecionada. Clique em **Itens da compra selecionada**.
5. Selecione um produto, informe a quantidade e clique em **Incluir**. O valor parcial é calculado automaticamente.
6. Volte para Compra para conferir o total atualizado.

Em cada cadastro, toque em um registro da lista para preencher o formulário, ou digite o código e clique em **Consultar**. Use **Alterar** ou **Excluir** para o registro indicado. Para outro cadastro, use **Limpar Form** e depois **Incluir**. Os códigos são gerados automaticamente.

Em Clientes e Produtos, digite o nome na busca e pressione Enter ou a lupa do teclado. O seletor A–Z / Z–A ordena os resultados. **Atualizar lista** mantém o filtro; apague a busca para voltar a exibir todos os registros.

## Correspondência com os requisitos

| Requisito | Implementação |
| --- | --- |
| Menu com Clientes, Produtos e Compra | `View/MenuPage.xaml` e code-behind |
| Validações no back-end antes do banco | `LerFormulario()` e eventos nas quatro páginas; funções em `View/CValidacao.cs` |
| CRUD de Cliente | `ClientePage`, `CCliente` e `CClienteService` |
| CRUD de Produto | `ProdutoPage`, `CProduto` e `CProdutoService` |
| CRUD de Compra | `CompraPage`, `CCompra` e `CCompraService` |
| CRUD de ItemCompra | `ItemCompraPage`, `CItemCompra` e `CItemCompraService` |
| Relacionamentos | `Compra.CodigoCliente`, `ItemCompra.CodigoProduto` e `ItemCompra.CodigoCompra`, com chaves estrangeiras no SQLite |

## Padrão da base preservado

- C# e XAML, .NET MAUI 10, SQLite local e os pacotes NuGet do projeto recebido.
- Pastas `Model`, `ModelView` e `View`, classes com prefixo C e serviços com métodos Insert, Update, Delete e consultas.
- Eventos `Clicked` no code-behind, campos `Entry`, seletores `Picker`, `CollectionView` e `ObservableCollection`.
- Estilos, fontes, cores, ícone e arquivos de plataforma do projeto original.
- Fluxo simples de formulários com Incluir, Alterar, Excluir, Consultar e Limpar Form.

A conexão foi centralizada em `CBancoService`, e todas as operações aguardam sua inicialização. As consultas usam parâmetros. O tratamento assíncrono usa `await`, mantendo as atualizações da interface no contexto da página.

As tabelas são criadas com SQL explícito para que o SQLite imponha efetivamente as chaves estrangeiras e as restrições. As operações CRUD continuam usando `SQLiteAsyncConnection`, como na base. O pacote SQLiteNetExtensions.Async foi mantido, mas não é necessário para este controle manual dos relacionamentos.

## Campos e regras

- Cliente: código, nome (até 50 caracteres), RG numérico (até 10 dígitos) e telefone (até 14 caracteres, contendo de 8 a 11 dígitos).
- Produto: código, nome (até 50), tipo (até 50), preço positivo com até duas casas decimais e quantidade em estoque inteira não negativa.
- Compra: código, data, total calculado e cliente existente.
- ItemCompra: código, produto existente, compra existente, quantidade inteira positiva e valor parcial calculado.
- Preço e total são limitados nesta aplicação a R$ 999.999.999,99 para validação e controle de valores.
- O preço aceita `12,50` ou `12.50`, sem separador de milhar.
- O RG usa `long` no C# porque até 10 dígitos podem ultrapassar o limite de `int`. Por ser numérico conforme o modelo do PDF, não conserva zeros à esquerda nem aceita letras.
- SQLite não tem tipos nativos `NVARCHAR`, `MONEY` ou `DATE` iguais aos do modelo lógico. Foram usados TEXT, NUMERIC e a representação de DateTime em ticks do sqlite-net. O código C# utiliza `decimal` para os valores monetários e `DateTime` para a data, gravada sem horário.
- A compra é criada com total zero; os itens são cadastrados na sequência. Enquanto não houver itens, ela funciona como cabeçalho em preenchimento. Não existe etapa adicional de finalização no enunciado.
- Inserir um item reduz o estoque; alterar recalcula a diferença; excluir devolve a quantidade. Estoque, item e total são alterados na mesma transação. Qualquer falha desfaz a operação inteira.
- Ao alterar apenas a quantidade de um item, é preservado o preço que foi praticado. Ao trocar o produto, é utilizado o preço atual do novo produto. Alterar o preço do cadastro não muda compras anteriores.
- O estoque no cadastro representa a quantidade disponível atual e pode ser ajustado manualmente.
- Cliente com compras e produto com itens vinculados não podem ser excluídos. Excluir uma compra pede confirmação, exclui seus itens e devolve o estoque.

## Roteiro de teste no Visual Studio

1. Cadastre Ana, RG `1234567890`, telefone `11999998888`.
2. Cadastre Caderno, tipo Papelaria, preço `12,50`, estoque `10`.
3. Crie uma compra para Ana e acrescente 2 Cadernos: parcial e total esperados `25,00`; estoque esperado `8`.
4. Altere a quantidade do item para 3: parcial e total `37,50`; estoque `7`.
5. Tente alterar a quantidade para 20: deve aparecer erro e os valores anteriores devem permanecer.
6. Altere o preço do produto para `15,00` e depois a quantidade do item para 4: o item anterior mantém preço unitário `12,50`, total `50,00`, estoque `6`.
7. Cadastre outro produto com estoque e adicione um segundo item à mesma compra. Confira a soma dos parciais.
8. Consulte cada tabela pelo código, altere nome/telefone/tipo/data/cliente e confira a persistência após fechar e abrir o app.
9. Tente excluir o cliente e um produto vinculados: devem ser bloqueados.
10. Exclua um item: seu estoque deve ser devolvido e o total reduzido.
11. Exclua a compra: os itens restantes devem ser removidos e suas quantidades devolvidas. Depois, os cadastros sem vínculos podem ser excluídos.
12. Teste código inválido, campos vazios, texto com mais de 50 caracteres, RG inválido, preço zero/negativo, estoque negativo e quantidade zero.

## Verificação desta entrega

Foram verificados a sintaxe C#, a estrutura XML/XAML, os vínculos entre controles e eventos e o esquema SQL real em SQLite, incluindo bloqueio de referências inexistentes, exclusões com vínculos e dados inválidos.

Os modelos, serviços e validadores C# foram compilados com .NET 10 e testados com o pacote SQLite real do projeto. Passaram 32 verificações de CRUD, busca parametrizada, totalização, estoque, preço histórico, relacionamentos, rollback e entradas inválidas.

Para repetir os testes de lógica, execute na pasta principal:

```sh
dotnet run --project Testes/Checks.csproj
```

O projeto de testes usa os mesmos arquivos C# da aplicação e substitui somente o caminho de armazenamento do MAUI por uma pasta temporária isolada. Ele é independente da solução principal e não adiciona dependências ao aplicativo.

A compilação e a execução da interface MAUI em Windows/Android não foram realizadas neste ambiente. Execute o roteiro acima no Visual Studio antes da entrega.

## Envio pelo GitHub

O PDF solicita entrega via GitHub. Envie o conteúdo desta pasta para o seu repositório, incluindo a solução, o projeto, os fontes, os recursos e este README. Os diretórios `.vs`, `bin` e `obj`, os arquivos de configuração pessoal e os bancos locais estão no `.gitignore` e não devem ser incluídos. Este pacote não foi publicado em uma conta GitHub.
