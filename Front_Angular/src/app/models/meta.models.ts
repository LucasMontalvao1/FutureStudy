export interface MetaDia {
    id: number;
    materiaId: number;
    materiaNome: string;
    topicoId?: number;
    topicoNome?: string;
    titulo: string;
    descricao?: string;
    quantidade: number;
    unidade: string;
    progresso: number;
    dataInicio: Date;
    dataFim: Date;
    concluida: boolean;
    percentualConcluido: number;
  }