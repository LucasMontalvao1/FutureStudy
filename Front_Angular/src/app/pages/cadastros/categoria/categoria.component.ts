import { Component, OnInit, OnDestroy, ViewEncapsulation } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Subscription } from 'rxjs';
import { Categoria } from '../../../models/categoria.model';
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

@Component({
  selector: 'app-categoria',
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
    MatTooltipModule
  ],
  templateUrl: './categoria.component.html',
  styleUrls: ['./categoria.component.scss'],
  encapsulation: ViewEncapsulation.None // Permite que os estilos sejam aplicados globalmente
})
export class CategoriaComponent implements OnInit, OnDestroy {
  categoriaForm: FormGroup;
  categorias: Categoria[] = [];
  carregando = false;
  modoEdicao = false;
  mostrandoFormulario = false;
  categoriaId: number | null = null;
  submitted = false;

  private subscriptions = new Subscription();

  constructor(
    private fb: FormBuilder,
    private categoriasService: CategoriasService,
    private snackBar: MatSnackBar
  ) {
    this.categoriaForm = this.fb.group({
      nome: ['', [Validators.required, Validators.maxLength(100)]],
      cor: ['#CCCCCC', [Validators.required, Validators.pattern(/^#[0-9A-Fa-f]{6}$/)]],
    });
  }

  ngOnInit(): void {
    this.carregarCategorias();

    this.subscriptions.add(
      this.categoriasService.categorias$.subscribe(
        (cats) => (this.categorias = cats)
      )
    );

    this.subscriptions.add(
      this.categoriasService.carregando$.subscribe(
        (estado) => (this.carregando = estado)
      )
    );
  }

  ngOnDestroy(): void {
    this.subscriptions.unsubscribe();
  }

  get f() {
    return this.categoriaForm.controls;
  }

  novaCategoria(): void {
    this.mostrandoFormulario = true;
    this.modoEdicao = false;
    this.categoriaId = null;
    this.categoriaForm.reset({ nome: '', cor: '#CCCCCC' });
    setTimeout(() => {
      const element = document.querySelector('.form-card');
      element?.scrollIntoView({ behavior: 'smooth', block: 'start' });
    }, 100);
  }

  carregarCategorias(): void {
    this.categoriasService.carregarCategorias();
  }

  salvarCategoria(): void {
    this.submitted = true;

    if (this.categoriaForm.invalid) {
      this.mostrarSnackbar('Por favor, corrija os erros no formulário', 'error');
      return;
    }

    const dados = { ...this.categoriaForm.value };

    if (this.modoEdicao && this.categoriaId !== null) {
      this.categoriasService.atualizarCategoria(this.categoriaId, dados).subscribe({
        next: () => {
          this.limparFormulario();
          this.mostrarSnackbar(`Categoria "${dados.nome}" atualizada com sucesso!`, 'success');
        },
        error: (e) => {
          console.error('Erro ao atualizar', e);
          this.mostrarSnackbar('Erro ao atualizar categoria', 'error');
        },
      });
    } else {
      this.categoriasService.criarCategoria(dados).subscribe({
        next: () => {
          this.limparFormulario();
          this.mostrarSnackbar(`Categoria "${dados.nome}" criada com sucesso!`, 'success');
        },
        error: (e) => {
          console.error('Erro ao criar', e);
          this.mostrarSnackbar('Erro ao criar categoria', 'error');
        },
      });
    }
  }

  editarCategoria(categoria: Categoria): void {
    this.mostrandoFormulario = true;
    this.modoEdicao = true;
    this.categoriaId = categoria.id;

    this.categoriaForm.patchValue({
      nome: categoria.nome,
      cor: categoria.cor,
    });

    setTimeout(() => {
      const element = document.querySelector('.form-card');
      element?.scrollIntoView({ behavior: 'smooth', block: 'start' });
    }, 100);
  }

  limparFormulario(): void {
    this.submitted = false;
    this.modoEdicao = false;
    this.categoriaId = null;
    this.mostrandoFormulario = false;
    this.categoriaForm.reset({ nome: '', cor: '#CCCCCC' });
  }

  confirmarExclusao(categoria: Categoria): void {
    if (confirm(`Deseja realmente excluir a categoria "${categoria.nome}"?`)) {
      this.categoriasService.excluirCategoria(categoria.id!).subscribe({
        next: () => {
          if (this.modoEdicao && this.categoriaId === categoria.id) {
            this.limparFormulario();
          }
          this.mostrarSnackbar(`Categoria "${categoria.nome}" excluída com sucesso!`, 'info');
        },
        error: (e) => {
          console.error('Erro ao excluir', e);
          this.mostrarSnackbar('Erro ao excluir categoria', 'error');
        },
      });
    }
  }

  mostrarSnackbar(mensagem: string, tipo: 'success' | 'error' | 'info' = 'info'): void {
    this.snackBar.open(mensagem, 'Fechar', {
      duration: 5000,
      horizontalPosition: 'end',
      verticalPosition: 'top',
      panelClass: `${tipo}-snackbar`
    });
  }
}