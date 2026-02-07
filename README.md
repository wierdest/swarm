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

## Publicar (Windows .exe / Linux)
Windows (.exe, roda no Windows ou via Wine):
```
dotnet publish src/Swarm.Presentation/Swarm.Presentation.csproj \
  -c Release -r win-x64 --self-contained true \
  -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true \
  -o dist/win-x64
```

Linux (binário nativo):
```
dotnet publish src/Swarm.Presentation/Swarm.Presentation.csproj \
  -c Release -r linux-x64 --self-contained true \
  -o dist/linux-x64
```

Saída:
- `dist/win-x64/Swarm.Presentation.exe`
- `dist/linux-x64/Swarm.Presentation`

## Resetar o manifest
O manifest usado em runtime fica em `src/Swarm.Presentation/bin/<Debug|Release>/net8.0/Content/GameSessionConfigManifest.json`.

Para resetar:
- apague o arquivo no `bin/`, ou
- edite e marque `Completed: false` (ou `ActiveIndex: 0`).
