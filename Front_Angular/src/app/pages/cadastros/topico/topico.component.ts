import { Component, OnInit, OnDestroy, ViewEncapsulation, Inject, PLATFORM_ID } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Subscription } from 'rxjs';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatButtonModule } from '@angular/material/button';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatSelectModule } from '@angular/material/select';
import { MateriasService } from '../../../services/materias.service';
import { TopicosService } from '../../../services/topicos.service';
import { Materia } from '../../../models/materia.model';
import { Topico } from '../../../models/topico.model';

// Enum para diferentes estados do componente
enum FormModeEnum {
  HIDDEN,
  CREATE,
  EDIT
}

@Component({
  selector: 'app-topico',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatButtonModule,
    MatSnackBarModule,
    MatTooltipModule,
    MatSelectModule
  ],
  templateUrl: './topico.component.html',
  styleUrls: ['./topico.component.scss'],
  encapsulation: ViewEncapsulation.None
})
export class TopicoComponent implements OnInit, OnDestroy {
  // Expondo o enum para o template
  FormMode = FormModeEnum;
  
  // Formulário - inicializado no construtor
  topicoForm!: FormGroup;
  formMode: FormModeEnum = FormModeEnum.HIDDEN;
  submitted = false;
  
  // Dados
  topicos: Topico[] = [];
  materias: Materia[] = [];
  
  // Estado da UI
  carregando = false;
  carregandoMaterias = false;
  materiaSelecionada: number | null = null;
  topicoId: number | null = null;
  isBrowser: boolean;
  
  // Subscriptions
  private subscriptions = new Subscription();

  constructor(
    private fb: FormBuilder,
    private topicosService: TopicosService,
    private materiasService: MateriasService,
    private snackBar: MatSnackBar,
    @Inject(PLATFORM_ID) private platformId: Object
  ) {
    this.isBrowser = isPlatformBrowser(this.platformId);
  }

  ngOnInit(): void {
    // Só executa no navegador, não no servidor
    if (this.isBrowser) {
      this.topicoForm = this.initializeForm();
      this.setupSubscriptions();
      this.carregarMaterias();
    }
  }

  ngOnDestroy(): void {
    // Desinscrever de todas as subscrições
    this.subscriptions.unsubscribe();
  }

  // Inicialização do formulário com validações
  private initializeForm(): FormGroup {
    return this.fb.group({
      nome: ['', [
        Validators.required, 
        Validators.maxLength(100),
        Validators.pattern(/^[a-zA-Z0-9\s\-_àáâãéêíóôõúçÀÁÂÃÉÊÍÓÔÕÚÇ.,():;]+$/) // Permite caracteres comuns em nomes de tópicos
      ]],
      materia_id: [null, Validators.required]
    });
  }

  // Configuração de todas as subscrições
  private setupSubscriptions(): void {
    // Subscrição para materias
    this.subscriptions.add(
      this.materiasService.materias$.subscribe(
        (materias) => {
          console.log('Materias carregadas:', materias);
          this.materias = materias;
          this.carregandoMaterias = false;
        }
      )
    );
  
    // Subscriptions para topicos
    this.subscriptions.add(
      this.topicosService.topicosFiltrados$.subscribe(
        (topicos) => {
          console.log('Tópicos filtrados atualizados:', topicos);
          this.topicos = topicos;
        }
      )
    );
  
    // Adicionar subscrição para todos os tópicos também
    this.subscriptions.add(
      this.topicosService.topicos$.subscribe(
        (topicos) => {
          console.log('Todos os tópicos carregados:', topicos);
        }
      )
    );
  }

  get f() {
    return this.topicoForm.controls;
  }

  carregarMaterias(): void {
    this.materiasService.carregarMaterias();
  }

  onMateriaSelecionada(materiaId: number | null): void {
    console.log(`Matéria selecionada: ${materiaId}`);
    this.materiaSelecionada = materiaId;
    
    if (materiaId === null) {
      console.log('Limpando filtros - mostrando todos os tópicos');
      this.topicosService.limparFiltros();
    } else {
      console.log(`Filtrando tópicos para matéria ID: ${materiaId}`);
      this.topicosService.filtrarTopicosPorMateria(materiaId);
    }
    
    // Verificar o resultado da filtragem
    setTimeout(() => {
      console.log('Tópicos filtrados após seleção:', this.topicos);
    }, 500);
    
    // Resetar o formulário quando mudar de matéria
    this.limparFormulario();
  }

  novoTopico(): void {
    if (this.materiaSelecionada === null) {
      this.mostrarSnackbar('Selecione uma matéria antes de criar um tópico', 'error');
      return;
    }
    
    this.formMode = FormModeEnum.CREATE;
    this.topicoId = null;
    this.topicoForm.reset({ 
      nome: '', 
      materia_id: this.materiaSelecionada
    });
    
    this.scrollToForm();
  }

  private scrollToForm(): void {
    setTimeout(() => {
      const element = document.querySelector('.form-card');
      element?.scrollIntoView({ behavior: 'smooth', block: 'start' });
    }, 100);
  }

  salvarTopico(): void {
    this.submitted = true;

    if (this.topicoForm.invalid) {
      this.topicoForm.markAllAsTouched(); 
      this.mostrarSnackbar('Por favor, corrija os erros no formulário', 'error');
      return;
    }

    const dados = { ...this.topicoForm.value };

    if (this.formMode === FormModeEnum.EDIT && this.topicoId !== null) {
      this.atualizarTopico(dados);
    } else {
      this.criarTopico(dados);
    }
  }

  private criarTopico(dados: any): void {
    console.log('Dados enviados para API ao criar tópico:', dados);
    
    this.subscriptions.add(
      this.topicosService.criarTopico(dados).subscribe({
        next: (resposta) => {
          console.log('Resposta da API ao criar tópico:', resposta);
          this.limparFormulario();
          this.mostrarSnackbar(`Tópico "${dados.nome}" criado com sucesso!`, 'success');
        },
        error: (e) => {
          console.error('Erro detalhado ao criar tópico:', e);
          if (e.error) {
            console.error('Detalhes do erro:', e.error);
          }
          this.mostrarSnackbar('Erro ao criar tópico', 'error');
        },
      })
    );
  }

  private atualizarTopico(dados: any): void {
    if (!this.topicoId) return;
    
    this.subscriptions.add(
      this.topicosService.atualizarTopico(this.topicoId, dados).subscribe({
        next: () => {
          this.limparFormulario();
          this.mostrarSnackbar(`Tópico "${dados.nome}" atualizado com sucesso!`, 'success');
        },
        error: (e) => {
          console.error('Erro ao atualizar', e);
          this.mostrarSnackbar('Erro ao atualizar tópico', 'error');
        },
      })
    );
  }

  editarTopico(topico: Topico): void {
    this.formMode = FormModeEnum.EDIT;
    this.topicoId = topico.id;

    this.topicoForm.patchValue({
      nome: topico.nome,
      materia_id: this.getMateriaid(topico)
    });

    this.scrollToForm();
  }

  limparFormulario(): void {
    this.submitted = false;
    this.formMode = FormModeEnum.HIDDEN;
    this.topicoId = null;
    this.topicoForm.reset({ 
      nome: '', 
      materia_id: this.materiaSelecionada
    });
  }

  confirmarExclusao(topico: Topico): void {
    if (confirm(`Deseja realmente excluir o tópico "${topico.nome}"?`)) {
      this.excluirTopico(topico);
    }
  }
  
  private excluirTopico(topico: Topico): void {
    this.subscriptions.add(
      this.topicosService.excluirTopico(topico.id).subscribe({
        next: () => {
          if (this.formMode === FormModeEnum.EDIT && this.topicoId === topico.id) {
            this.limparFormulario();
          }
          this.mostrarSnackbar(`Tópico "${topico.nome}" excluído com sucesso!`, 'info');
        },
        error: (e) => {
          console.error('Erro ao excluir', e);
          this.mostrarSnackbar('Erro ao excluir tópico', 'error');
        },
      })
    );
  }
  
  getMateriaid(topico: Topico): number | null {
    return topico.materia_id ?? null;
  }
  
  obterNomeMateria(materiaId: number | null): string {
    if (materiaId === null) return 'Sem matéria';
    const materia = this.materias.find(m => m.id === materiaId);
    return materia?.nome ?? 'Matéria não encontrada';
  }
  
  obterCorMateria(materiaId: number | null): string {
    if (materiaId === null) return '#CCCCCC';
    const materia = this.materias.find(m => m.id === materiaId);
    return materia?.cor ?? '#CCCCCC';
  }

  mostrarSnackbar(mensagem: string, tipo: 'success' | 'error' | 'info' = 'info'): void {
    this.snackBar.open(mensagem, 'Fechar', {
      duration: 5000,
      horizontalPosition: 'end',
      verticalPosition: 'top',
      panelClass: `${tipo}-snackbar`
    });
  }
  
  // Função auxiliar para determinar se uma cor é clara ou escura
  // para ajustar o texto da badge de matéria
  isLightColor(hexColor: string): boolean {
    // Verifica se é uma cor válida em formato hex
    if (!hexColor || !/^#[0-9A-Fa-f]{6}$/.test(hexColor)) {
      return false;
    }

    // Extrai os componentes RGB
    const r = parseInt(hexColor.substring(1, 3), 16);
    const g = parseInt(hexColor.substring(3, 5), 16);
    const b = parseInt(hexColor.substring(5, 7), 16);

    // Calcula a luminosidade (peso percebido de claridade)
    // Fórmula: 0.299R + 0.587G + 0.114B
    const luminance = (0.299 * r + 0.587 * g + 0.114 * b) / 255;

    // Retorna true se a cor for clara (luminância > 0.6)
    return luminance > 0.6;
  }
}