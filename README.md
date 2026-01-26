# Swarm

Swarm não é o jogo em si — é o **núcleo** de um jogo.  
O objetivo é manter a lógica e o estado do domínio independentes, expondo um **GameSnapshot** que qualquer Presentation pode renderizar com estilos diferentes.

O núcleo (Domain/Application/Infra) é reaproveitável e a Presentation pode mudar para criar outras versões visuais da mesma ideia.

Usamos arquivos de configuração (GameSessionConfig) e um manifest para organizar “tutorials/levels” e demonstrar funcionalidades.

## Estrutura
- **Domain**: regras do jogo
- **Application**: serviços e mapeadores
- **Infrastructure**: loaders/configs
- **Presentation**: MonoGame (render e input)

## Rodar
```
dotnet run --project src/Swarm.Presentation
```

## Resetar o manifest
O manifest usado em runtime fica em `src/Swarm.Presentation/bin/<Debug|Release>/net8.0/Content/GameSessionConfigManifest.json`.

Para resetar:
- apague o arquivo no `bin/`, ou
- edite e marque `Completed: false` (ou `ActiveIndex: 0`).
