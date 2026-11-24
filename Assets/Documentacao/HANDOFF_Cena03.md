# HANDOFF - Cena03 / Cena02 (Old-Kiri)

## O QUE JA FOI FEITO (por Claude, via Unity MCP porta 6400)
1. CAMERA cena03: o problema era o objeto TmpCam (camera extra, depth 50) por cima da Main Camera. Removido. Agora Main Camera (CinemachineBrain) + Virtual Camera (Follow=Xomi, Confiner=LimiteTela) igual a cena teste.
2. MARTA (Assets/Codigos/MartaCorrida.cs, reescrito): entra correndo pelo topo, para acima do Xomi, diz 2 falas via DialogoTulipa (sem escolhas, sem item), sai correndo pra baixo, some e chama SenhorioAbordagem.Armar(). Colisor dela fica desligado (so efeito). Falas configuradas no componente DialogoTulipa da Marta (falasIniciais; opcoes vazio):
   - Licenca, moco! Cortaram a agua do bairro de novo.
   - Vou ali embaixo avisar a dona Cida que o caminhao-pipa ta chegando!
3. SENHORIO (objeto Senhorio na cena03, script Assets/Codigos/SenhorioAbordagem.cs): armado pela Marta; quando o Xomi passa de Y=-27 ele aparece acima da tela, anda ate o Xomi (Xomi trava), pergunta quem e e oferece emprego, as 3 opcoes aparecem uma a uma e na 3a ele corta no meio (digita so 14 letras), obriga o Xomi a trabalhar (cobrar aluguel de quem nao paga), as falas tem desprezo disfarçado. Depois leva o Xomi pela mao ate o topo (Y=34) e faz fade preto + LoadScene(cena02). Todas as falas estao editaveis no Inspector (falasChegada, textosOpcoes, falasInterrupcao).
4. CENA02: objeto IntroCena02 (script IntroCena02.cs): fade-in, trava o Xomi, mostra SO a caixa de dialogo do senhorio (rostos do sprite senhorio Falando, reaproveitando a Caixa dialogo do Taison) mandando cobrar o Taison; depois destrava o Xomi. O senhorio NAO aparece fisicamente na cena02.
5. Utilitarios: Assets/Codigos/CutsceneUtil.cs (CutsceneUtil.TravarXomi/XomiOlhar + classe TelaFade, fade preto criado por codigo).
6. Sprites: 'senhorio Falando.png' estava sem fatiar - fatiei em 5 rostos (grade 2x3, 64x64 por celula). Spritesheet do senhorio tem 24 frames = 4 direcoes x 6 frames: 0-5, 6-11, 12-17, 18-23. PALPITE: 0-5 = frente (andandoBaixo), 6-11 = costas (andandoCima). Se estiver invertido, troque no Inspector do Senhorio (andandoBaixo / andandoCima).
7. Build Settings: adicionadas cena03 e cena02 (antes so MenuPrincipal e cena teste).

## PENDENTE / PARA CONFERIR
- Testar o fluxo completo no Play (Marta -> senhorio -> leva ate o topo -> cena02) e ajustar visualmente: escala do senhorio (10.5), yGatilho (-27), yTopo (34), posicaoXomiAoLado, velocidades.
- Conferir se o caminho reto ate o topo (x constante) atravessa casas/obstaculos; se sim, trocar por waypoints.
- Conferir se a animacao de corrida da Marta precisa de flipX ao sair pro lado.
- Se quiser o 'senhorio so aparece na cena02 depois da cena03' (nao ao abrir cena02 direto), adicionar flag estatica.
- execute_code do MCP compila em C# 6 (sem funcoes locais, usar UnityEngine.Object.X).

## PROMPT PARA OUTRA IA CONTINUAR
Voce esta ajudando num jogo Unity 2D (projeto Old-Kiri, instancia Unity MCP porta 6400, cena ativa cena03). Leia Assets/Documentacao/HANDOFF_Cena03.md. O fluxo desejado e: (1) camera da cena03 segue o Xomi como na cena teste (ja corrigido: removido TmpCam); (2) a Marta aparece correndo, diz apenas 'Licenca moco, cortaram a agua do bairro de novo' e 'Vou ali embaixo avisar a dona Cida que o caminhao pipa ta chegando', sem interacao do jogador; (3) depois o senhorio chega ao Xomi um pouco mais a frente no cenario, pergunta quem ele e e ja oferece um emprego; o Xomi parece ter escolhas, mas enquanto as opcoes vao aparecendo o senhorio interrompe e o obriga a trabalhar cobrando aluguel de quem nao paga; as falas do senhorio mostram desprezo pelo Xomi so nas entrelinhas; (4) o senhorio leva o Xomi pela mao ate o topo do mapa e carrega a cena02; (5) na cena02 o senhorio nao aparece, so a caixa de dialogo (sprite 'senhorio Falando') dizendo que o Xomi deve cobrar o Taison. Tudo isso ja esta implementado nos scripts MartaCorrida, SenhorioAbordagem, IntroCena02, CutsceneUtil. Sua tarefa: rodar no Play, verificar o console, corrigir bugs, ajustar timing/posicoes/sprites (conferir direcao dos frames do senhorio) e polir. Nao quebre DialogoTulipa/DialogoTaison existentes.
