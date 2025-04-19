import { Component, OnInit, OnDestroy, PLATFORM_ID, Inject } from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CronometroService, SessaoEstudoRequestDto } from '../../../services/cronometro.service';
import { TopicosService } from '../../../services/topicos.service';
import { AnotacoesService } from '../../../services/anotacoes.service';
import { Subject, takeUntil, finalize, debounceTime } from 'rxjs';
import { Topico } from '../../../models/topico.model';
import { Categoria } from '../../../models/categoria.model';
import { Materia } from '../../../models/materia.model';
import { SessaoEstudo } from '../../../models/sessao-estudo.models';
import { CalendarioComponent } from '../calendario/calendario.component';
import { ViewChild } from '@angular/core';


@Component({
  selector: 'app-cronometro',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './cronometro.component.html',
  styleUrls: ['./cronometro.component.scss']
})
export class CronometroComponent implements OnInit, OnDestroy {
  tempo: string = '00:00:00';
  categoriaId: number | null = null;
  materiaId: number | null = null;
  topicoId: number | null = null;
  notas: string = '';
  ultimasNotas: string = ''; 
  
  categorias: Categoria[] = [];
  materias: Materia[] = [];
  materiasFiltradas: Materia[] = [];
  topicos: Topico[] = [];
  topicosFiltrados: Topico[] = [];
  
  carregando = false;
  sessaoAtiva = false;
  sessaoPausada = false;
  erro: string | null = null;

    // Modal de retorno
    mostrarModalRetorno = false;
    pausaAutomatica = false;
  
  private destruir$ = new Subject<void>();
  private notasChanged$ = new Subject<string>();
  private sessaoAtual: SessaoEstudo | null = null;
  private pausaId: number | null = null;
  private isBrowser: boolean;
  
  @ViewChild('calendarioComponent') calendarioComponent!: CalendarioComponent;

  constructor(
    private cronometroService: CronometroService,
    private topicosService: TopicosService,
    private anotacoesService: AnotacoesService, 
    @Inject(PLATFORM_ID) private platformId: Object
  ) {
    this.isBrowser = isPlatformBrowser(this.platformId);
  }

  ngOnInit() {
    if (!this.isBrowser) return;
    
    this.registrarObservadores();
    this.carregarDadosIniciais();
    this.configurarEventosPaginaVisibilidade();
    
    this.notasChanged$
      .pipe(
        takeUntil(this.destruir$),
        debounceTime(5000) 
      )
      .subscribe(notas => {
        if (notas !== this.ultimasNotas) {
          this.salvarNotasNaAPI();
          this.ultimasNotas = notas;
        }
      });
  }
  
  ngOnDestroy() {
    this.destruir$.next();
    this.destruir$.complete();

    // Remover event listeners
    if (this.isBrowser) {
      window.removeEventListener('beforeunload', this.handleBeforeUnload);
      window.removeEventListener('offline', this.handleOffline);
      window.removeEventListener('online', this.handleOnline);
    }
  }

   // Configurar os eventos para detectar quando o usuário fecha a página ou perde a conexão
   private configurarEventosPaginaVisibilidade(): void {
    if (!this.isBrowser) return;
    
    // Evento antes de fechar a página
    window.addEventListener('beforeunload', this.handleBeforeUnload.bind(this));
    
    // Evento para detectar quando o usuário perde a conexão
    window.addEventListener('offline', this.handleOffline.bind(this));
    
    // Evento para detectar quando o usuário volta online (para mostrar o modal)
    window.addEventListener('online', this.handleOnline.bind(this));
  }
  
  // Manipulador para quando o usuário perde a conexão
  private handleOffline(): void {
    if (!this.sessaoAtual || this.sessaoPausada) return;
    
    this.pausarAutomaticamente();
  }
  
  // Manipulador para quando o usuário volta online
  private handleOnline(): void {
    if (this.pausaAutomatica) {
      this.mostrarModalRetorno = true;
    }
  }
  
  // Manipulador para evento de fechar a página
  private handleBeforeUnload(event: BeforeUnloadEvent): void {
    if (this.sessaoAtual && !this.sessaoPausada) {
      this.pausarAutomaticamente();
      
      // Mensagem para o navegador (pode não aparecer em todos os navegadores)
      const mensagem = 'Você tem uma sessão de estudos ativa. Tem certeza que deseja sair?';
      event.returnValue = mensagem; // Isso é o correto para BeforeUnloadEvent
      // Remover o return, pois a função deve retornar void
    }
  }
  
  // Pausar automaticamente quando o usuário sai da página
  private pausarAutomaticamente(): void {
    if (!this.sessaoAtual?.id || this.sessaoPausada) return;
    
    this.pausaAutomatica = true;
    
    // Salvar as notas antes de pausar
    if (this.notas !== this.ultimasNotas) {
      this.salvarNotasNaAPI();
    }
    
    this.cronometroService.pausarSessao(this.sessaoAtual.id)
      .subscribe({
        next: (pausaResponse) => {
          console.log('Sessão pausada automaticamente');
          this.sessaoPausada = true;
          this.pausaId = pausaResponse.id;
          
          // Armazenar o estado da sessão no localStorage
          if (this.isBrowser) {
            localStorage.setItem('pausaAutomatica', 'true');
            localStorage.setItem('pausaId', String(this.pausaId));
            localStorage.setItem('sessaoId', String(this.sessaoAtual?.id));
          }
        },
        error: (erro) => console.error('Erro ao pausar automaticamente:', erro)
      });
  }
  
  // Verificar ao iniciar se há sessão pausada automaticamente
  private verificarRetornoSessao(): void {
    if (!this.isBrowser) return;
    
    const pausaAutomatica = localStorage.getItem('pausaAutomatica');
    const pausaId = localStorage.getItem('pausaId');
    const sessaoId = localStorage.getItem('sessaoId');
    
    if (pausaAutomatica === 'true' && pausaId && sessaoId) {
      this.pausaAutomatica = true;
      this.pausaId = Number(pausaId);
      
      // Carregar estado da sessão se necessário
      this.carregarEstadoSessao(Number(sessaoId));
      
      // Mostrar modal de retorno
      this.mostrarModalRetorno = true;
    }
  }
  
  // Carregar estado da sessão pausada
  private carregarEstadoSessao(sessaoId: number): void {
    // Implementar lógica para buscar o estado da sessão no backend
    this.cronometroService.obterSessao(sessaoId)
      .subscribe({
        next: (sessao) => {
          this.sessaoAtual = sessao;
          this.sessaoAtiva = true;
          this.sessaoPausada = true;
          this.categoriaId = sessao.categoriaId ?? null;
          this.materiaId = sessao.materiaId;
          // Converter undefined para null se necessário
          this.topicoId = sessao.topicoId !== undefined ? sessao.topicoId : null;
          
          // Carregar notas da sessão
          this.carregarNotasDaSessao(sessaoId);
        },
        error: (erro) => console.error('Erro ao carregar estado da sessão:', erro)
      });
  }
  
  // Ações do modal de retorno
  continuarSessao(): void {
    if (!this.pausaId) return;
    
    this.cronometroService.retomarSessao(this.pausaId)
      .subscribe({
        next: () => {
          this.pausaAutomatica = false;
          this.sessaoPausada = false;
          this.pausaId = null;
          this.mostrarModalRetorno = false;
          
          // Limpar dados do localStorage
          if (this.isBrowser) {
            localStorage.removeItem('pausaAutomatica');
            localStorage.removeItem('pausaId');
            localStorage.removeItem('sessaoId');
          }
        },
        error: (erro) => this.handleError('retomar a sessão', erro)
      });
  }
  
  finalizarSessaoRetorno(): void {
    if (!this.sessaoAtual?.id) return;
    
    this.cronometroService.finalizarSessao(this.sessaoAtual.id)
      .subscribe({
        next: () => {
          this.sessaoAtual = null;
          this.sessaoAtiva = false;
          this.sessaoPausada = false;
          this.pausaId = null;
          this.pausaAutomatica = false;
          this.mostrarModalRetorno = false;
          this.notas = '';
          this.ultimasNotas = '';
          
          // Limpar dados do localStorage
          if (this.isBrowser) {
            localStorage.removeItem('pausaAutomatica');
            localStorage.removeItem('pausaId');
            localStorage.removeItem('sessaoId');

            this.calendarioComponent.reconstruirDadosAgrupados();
          }
        },
        error: (erro) => this.handleError('finalizar a sessão', erro)
      });
  }
  
  private registrarObservadores(): void {
    this.cronometroService.tempo$
      .pipe(takeUntil(this.destruir$))
      .subscribe(tempo => this.tempo = tempo);
    
    this.cronometroService.sessaoAtual$
      .pipe(takeUntil(this.destruir$))
      .subscribe(sessao => {
        this.sessaoAtual = sessao;
        this.sessaoAtiva = !!sessao;
        this.sessaoPausada = sessao?.status === 'pausada';
        
        if (sessao && sessao.id) {
          this.carregarNotasDaSessao(sessao.id);
        }
      });
    
    this.cronometroService.categorias$
      .pipe(takeUntil(this.destruir$))
      .subscribe({
        next: (categorias) => {
          this.categorias = categorias || [];
          if (this.categorias.length > 0 && this.categoriaId === null) {
            this.selecionarCategoria(this.categorias[0].id);
          }
        },
        error: (erro) => this.handleError('carregar categorias', erro)
      });
    
    this.cronometroService.materias$
      .pipe(takeUntil(this.destruir$))
      .subscribe({
        next: (materias) => this.materias = materias || [],
        error: (erro) => this.handleError('carregar matérias', erro)
      });
    
    this.cronometroService.materiasFiltradas$
      .pipe(takeUntil(this.destruir$))
      .subscribe({
        next: (materias) => {
          this.materiasFiltradas = materias || [];
          if (this.materiasFiltradas.length > 0 && this.materiaId === null) {
            this.materiaId = this.materiasFiltradas[0].id;
            this.carregarTopicos(this.materiaId);
          } else if (this.materiasFiltradas.length === 0) {
            this.materiaId = null;
            this.topicoId = null;
          }
        },
        error: (erro) => this.handleError('carregar matérias filtradas', erro)
      });
    
    this.topicosService.topicos$
      .pipe(takeUntil(this.destruir$))
      .subscribe({
        next: (topicos) => this.topicos = topicos || [],
        error: (erro) => this.handleError('carregar tópicos', erro)
      });
    
    this.topicosService.topicosFiltrados$
      .pipe(takeUntil(this.destruir$))
      .subscribe({
        next: (topicos) => {
          this.topicosFiltrados = topicos || [];
          if (this.topicosFiltrados.length > 0) {
            this.topicoId = this.topicosFiltrados[0].id;
          } else {
            this.topicoId = null;
          }
        },
        error: (erro) => this.handleError('carregar tópicos filtrados', erro)
      });
    
    this.cronometroService.carregando$
      .pipe(takeUntil(this.destruir$))
      .subscribe(carregando => this.carregando = carregando);
  }
  
  private carregarDadosIniciais(): void {
    try {
      this.cronometroService.carregarCategorias();
      this.cronometroService.carregarMaterias();
      this.topicosService.carregarTopicos();
      this.verificarRetornoSessao(); // Verificar se existe sessão pausada automaticamente
    } catch (error) {
      this.handleError('carregar dados iniciais', error);
    }
  }

  
  private carregarNotasDaSessao(sessaoId: number): void {
    if (!this.isBrowser) return;
    
    this.anotacoesService.getAnotacoesBySessao(sessaoId)
      .pipe(takeUntil(this.destruir$))
      .subscribe({
        next: (anotacoes) => {
          if (anotacoes && anotacoes.length > 0) {
            this.notas = anotacoes[0].conteudo || '';
            this.ultimasNotas = this.notas; 
          }
        },
        error: (erro) => console.error('Erro ao carregar anotações da sessão:', erro)
      });
  }
  
  private handleError(operacao: string, erro: any): void {
    console.error(`Erro ao ${operacao}:`, erro);
    this.erro = `Não foi possível ${operacao}.`;
    this.carregando = false;
  }
  

  selecionarCategoria(categoriaId: number): void {
    if (!this.isBrowser || !categoriaId) return;
    
    this.categoriaId = categoriaId;
    this.materiaId = null;
    this.topicoId = null;
    
    this.cronometroService.carregarMateriasPorCategoria(categoriaId);
  }
  

  selecionarMateria(materiaId: number): void {
    if (!this.isBrowser || !materiaId) return;
    
    this.materiaId = materiaId;
    this.topicoId = null;
    this.carregarTopicos(materiaId);
  }
  

  carregarTopicos(materiaId: number): void {
    if (!this.isBrowser) return;
    
    this.topicosService.carregarTopicosPorMateria(materiaId);
  }
  
  obterNomeCategoria = (id: number | null): string =>
    this.categorias.find(cat => cat.id === id)?.nome ?? '';
  
  obterNomeMateria = (id: number | null): string =>
    this.materias.find(mat => mat.id === id)?.nome ?? '';
  
  obterNomeTopico = (id: number | null): string =>
    this.topicos.find(t => t.id === id)?.nome ?? '';
  
  validarSelecoes(): boolean {
    if (!this.categoriaId) {
      this.erro = 'Selecione uma categoria antes de iniciar';
      return false;
    }
    
    if (!this.materiaId) {
      this.erro = 'Selecione uma matéria antes de iniciar';
      return false;
    }
    
    if (!this.topicoId && this.topicosFiltrados.length > 0) {
      this.erro = 'Selecione um tópico antes de iniciar';
      return false;
    }
    
    this.erro = null;
    return true;
  }
  
  iniciar(): void {
    if (!this.isBrowser || !this.validarSelecoes()) {
      return;
    }
    
    this.erro = null;
    this.carregando = true;
    
    const request: SessaoEstudoRequestDto = {
      categoriaId: this.categoriaId!,
      materiaId: this.materiaId!, 
      topicoId: this.topicoId!
    };
    
    this.cronometroService.iniciarSessao(request)
      .pipe(finalize(() => this.carregando = false))
      .subscribe({
        next: (sessaoCriada) => {
          this.sessaoAtual = sessaoCriada;
          this.sessaoAtiva = true;
          this.sessaoPausada = false;
          this.notas = '';
          this.ultimasNotas = '';
        },
        error: (erro) => this.handleError('iniciar a sessão', erro)
      });
  }
  
  pausar(): void {
    if (!this.isBrowser || !this.sessaoAtual?.id) {
      if (!this.sessaoAtual?.id) this.erro = 'Não há sessão ativa para pausar/retomar.';
      return;
    }
    
    this.carregando = true;
    this.erro = null;
    
    if (this.sessaoPausada) {
      if (!this.pausaId) {
        this.handleError('retomar a sessão', 'ID de pausa não encontrado');
        return;
      }
      
      this.cronometroService.retomarSessao(this.pausaId)
        .pipe(finalize(() => this.carregando = false))
        .subscribe({
          next: () => {
            this.sessaoPausada = false;
            this.pausaId = null;
          },
          error: (erro) => this.handleError('retomar a sessão', erro)
        });
    } else {
      this.cronometroService.pausarSessao(this.sessaoAtual.id)
        .pipe(finalize(() => this.carregando = false))
        .subscribe({
          next: (pausaResponse) => {
            this.sessaoPausada = true;
            this.pausaId = pausaResponse.id;
          },
          error: (erro) => this.handleError('pausar a sessão', erro)
        });
    }
  }
  
  parar(): void {
    if (!this.isBrowser || !this.sessaoAtual?.id) {
      if (!this.sessaoAtual?.id) this.erro = 'Não há sessão ativa para finalizar.';
      return;
    }
    
    if (this.notas !== this.ultimasNotas) {
      this.salvarNotasNaAPI();
    }
    
    this.carregando = true;
    this.erro = null;
    
    this.cronometroService.finalizarSessao(this.sessaoAtual.id)
      .pipe(finalize(() => this.carregando = false))
      .subscribe({
        next: () => {
          this.sessaoAtual = null;
          this.sessaoAtiva = false;
          this.sessaoPausada = false;
          this.pausaId = null;
          this.notas = '';
          this.ultimasNotas = '';
        },
        error: (erro) => this.handleError('finalizar a sessão', erro)
      });
  }
  
  onNotasChange(): void {
    this.notasChanged$.next(this.notas);
  }
  
  salvarNotas(): void {
    if (this.notas !== this.ultimasNotas) {
      this.salvarNotasNaAPI();
    }
  }
  
  private salvarNotasNaAPI(): void {
    if (!this.isBrowser || !this.sessaoAtual?.id || !this.notas.trim()) {
      return;
    }
    
    const hoje = new Date();
    const dataFormatada = hoje.toLocaleDateString('pt-BR'); // Ex: "13/04/2025"
    const titulo = `Anotações de estudo - ${dataFormatada}`;

    this.carregando = true;
    this.erro = null;
    
    this.anotacoesService.getAnotacoesBySessao(this.sessaoAtual.id)
      .pipe(finalize(() => this.carregando = false))
      .subscribe({
        next: (anotacoes) => {
          if (anotacoes && anotacoes.length > 0) {
            const anotacaoExistente = anotacoes[0];
            
            this.anotacoesService.updateAnotacao(anotacaoExistente.id, {
              sessaoId: this.sessaoAtual!.id,
              titulo,
              conteudo: this.notas.trim()
            }).subscribe({
              next: () => {
                console.log('Anotação atualizada com sucesso');
                this.ultimasNotas = this.notas;
              },
              error: (erro) => this.handleError('atualizar anotação', erro)
            });
          } else {
            const novaAnotacao = {
              sessaoId: this.sessaoAtual!.id,
              titulo,
              conteudo: this.notas.trim()
            };
            
            this.anotacoesService.createAnotacao(novaAnotacao).subscribe({
              next: () => {
                console.log('Nova anotação criada com sucesso');
                this.ultimasNotas = this.notas;
              },
              error: (erro) => this.handleError('criar anotação', erro)
            });
          }
        },
        error: (erro) => this.handleError('verificar anotações existentes', erro)
      });
  }
}