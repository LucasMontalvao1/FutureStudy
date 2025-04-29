import { Component, OnInit, OnDestroy, ViewEncapsulation } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Subscription } from 'rxjs';
import { Materia } from '../../../models/materia.model';
import { Categoria } from '../../../models/categoria.model';
import { MateriasService } from '../../../services/materias.service';
import { CategoriasService } from '../../../services/categorias.service';
import { ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatButtonModule } from '@angular/material/button';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatSelectModule } from '@angular/material/select';

// Enum para diferentes estados do componente
enum FormModeEnum {
  HIDDEN,
  CREATE,
  EDIT
}

@Component({
  selector: 'app-materia',
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
  templateUrl: './materia.component.html',
  styleUrls: ['./materia.component.scss'],
  encapsulation: ViewEncapsulation.None
})
export class MateriaComponent implements OnInit, OnDestroy {
  // Expondo o enum para o template
  FormMode = FormModeEnum;
  
  // Formulário - inicializado no construtor
  materiaForm!: FormGroup;
  formMode: FormModeEnum = FormModeEnum.HIDDEN;
  submitted = false;
  
  // Dados
  materias: Materia[] = [];
  categorias: Categoria[] = [];
  
  // Estado da UI
  carregando = false;
  carregandoCategorias = false;
  categoriaSelecionada: number | null = null;
  materiaId: number | null = null;
  
  // Subscriptions
  private subscriptions = new Subscription();

  constructor(
    private fb: FormBuilder,
    private materiasService: MateriasService,
    private categoriasService: CategoriasService,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit(): void {
    this.materiaForm = this.initializeForm();
    this.setupSubscriptions();
    this.carregarCategorias();
  }

  ngOnDestroy(): void {
    // Desinscrever de todas as subscrições
    this.subscriptions.unsubscribe();
  }

  // Inicialização do formulário com validações aprimoradas
  private initializeForm(): FormGroup {
    const hexColorPattern = /^#[0-9A-Fa-f]{6}$/;
    
    return this.fb.group({
      nome: ['', [
        Validators.required, 
        Validators.maxLength(100),
        Validators.pattern(/^[a-zA-Z0-9\s\-_àáâãéêíóôõúçÀÁÂÃÉÊÍÓÔÕÚÇ]+$/) // Apenas caracteres válidos
      ]],
      cor: ['#CCCCCC', [
        Validators.required, 
        Validators.pattern(hexColorPattern)
      ]],
      categoriaid: [null, Validators.required]
    });
  }

  // Configuração de todas as subscrições
  private setupSubscriptions(): void {
    // Subscrição para categorias
    this.subscriptions.add(
      this.categoriasService.categorias$.subscribe(
        (categorias) => {
          this.categorias = categorias;
          this.carregandoCategorias = false;
        }
      )
    );

    this.subscriptions.add(
      this.categoriasService.carregando$.subscribe(
        (estado) => (this.carregandoCategorias = estado)
      )
    );
    
    // Subscriptions para materias
    this.subscriptions.add(
      this.materiasService.materiasFiltrados$.subscribe(
        (materias) => (this.materias = materias)
      )
    );

    this.subscriptions.add(
      this.materiasService.carregando$.subscribe(
        (estado) => (this.carregando = estado)
      )
    );
  }

  get f() {
    return this.materiaForm.controls;
  }

  carregarCategorias(): void {
    this.categoriasService.carregarCategorias();
  }

  onCategoriaSelecionada(categoriaId: number | null): void {
    this.categoriaSelecionada = categoriaId;
    
    if (categoriaId === null) {
      this.materiasService.limparFiltros();
    } else {
      this.materiasService.filtrarMateriasPorCategoria(categoriaId);
    }
    
    // Resetar o formulário quando mudar de categoria
    this.limparFormulario();
  }

  novaMateria(): void {
    if (this.categoriaSelecionada === null) {
      this.mostrarSnackbar('Selecione uma categoria antes de criar uma matéria', 'error');
      return;
    }
    
    this.formMode = FormModeEnum.CREATE;
    this.materiaId = null;
    this.materiaForm.reset({ 
      nome: '', 
      cor: '#CCCCCC',
      categoriaid: this.categoriaSelecionada
    });
    
    this.scrollToForm();
  }

  private scrollToForm(): void {
    setTimeout(() => {
      const element = document.querySelector('.form-card');
      element?.scrollIntoView({ behavior: 'smooth', block: 'start' });
    }, 100);
  }

  salvarMateria(): void {
    this.submitted = true;

    if (this.materiaForm.invalid) {
      this.materiaForm.markAllAsTouched(); 
      this.mostrarSnackbar('Por favor, corrija os erros no formulário', 'error');
      return;
    }

    const dados = { ...this.materiaForm.value };

    if (this.formMode === FormModeEnum.EDIT && this.materiaId !== null) {
      this.atualizarMateria(dados);
    } else {
      this.criarMateria(dados);
    }
  }

  private criarMateria(dados: any): void {
    this.subscriptions.add(
      this.materiasService.criarMateria(dados).subscribe({
        next: () => {
          this.limparFormulario();
          this.mostrarSnackbar(`Matéria "${dados.nome}" criada com sucesso!`, 'success');
        },
        error: (e) => {
          console.error('Erro ao criar', e);
          this.mostrarSnackbar('Erro ao criar matéria', 'error');
        },
      })
    );
  }

  private atualizarMateria(dados: any): void {
    if (!this.materiaId) return;
    
    this.subscriptions.add(
      this.materiasService.atualizarMateria(this.materiaId, dados).subscribe({
        next: () => {
          this.limparFormulario();
          this.mostrarSnackbar(`Matéria "${dados.nome}" atualizada com sucesso!`, 'success');
        },
        error: (e) => {
          console.error('Erro ao atualizar', e);
          this.mostrarSnackbar('Erro ao atualizar matéria', 'error');
        },
      })
    );
  }

  editarMateria(materia: Materia): void {
    this.formMode = FormModeEnum.EDIT;
    this.materiaId = materia.id;

    this.materiaForm.patchValue({
      nome: materia.nome,
      cor: materia.cor || '#CCCCCC',
      categoriaid: this.getCategoriaId(materia)
    });

    this.scrollToForm();
  }

  limparFormulario(): void {
    this.submitted = false;
    this.formMode = FormModeEnum.HIDDEN;
    this.materiaId = null;
    this.materiaForm.reset({ 
      nome: '', 
      cor: '#CCCCCC',
      categoriaid: this.categoriaSelecionada
    });
  }

  confirmarExclusao(materia: Materia): void {
    if (confirm(`Deseja realmente excluir a matéria "${materia.nome}"?`)) {
      this.excluirMateria(materia);
    }
  }
  
  private excluirMateria(materia: Materia): void {
    this.subscriptions.add(
      this.materiasService.excluirMateria(materia.id).subscribe({
        next: () => {
          if (this.formMode === FormModeEnum.EDIT && this.materiaId === materia.id) {
            this.limparFormulario();
          }
          this.mostrarSnackbar(`Matéria "${materia.nome}" excluída com sucesso!`, 'info');
        },
        error: (e) => {
          console.error('Erro ao excluir', e);
          this.mostrarSnackbar('Erro ao excluir matéria', 'error');
        },
      })
    );
  }
  
  getCategoriaId(materia: Materia): number | null {
    return materia.categoria_id ?? materia.categoriaId ?? null;
  }
  
  obterNomeCategoria(categoriaId: number | null): string {
    if (categoriaId === null) return 'Sem categoria';
    const categoria = this.categorias.find(c => c.id === categoriaId);
    return categoria?.nome ?? 'Categoria não encontrada';
  }
  
  obterCorCategoria(categoriaId: number | null): string {
    if (categoriaId === null) return '#CCCCCC';
    const categoria = this.categorias.find(c => c.id === categoriaId);
    return categoria?.cor ?? '#CCCCCC';
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
  // para ajustar o texto da badge de categoria
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