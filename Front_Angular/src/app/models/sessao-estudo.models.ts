import { AnotacaoSessao } from "./anotacao.models";

export interface SessaoEstudo {
    id: number;
    materiaId: number;
    materiaNome: string;
    categoriaId?: number;
    topicoId?: number;
    topicoNome?: string;
    dataInicio: Date;
    dataFim?: Date;
    duracao: number; // em minutos
    status: 'emAndamento' | 'pausada' | 'finalizada';
    pausas?: PausaSessao[];
    anotacoes?: AnotacaoSessao[];
  }
  
  export interface PausaSessao {
    id: number;
    dataInicio: Date;
    dataFim?: Date;
    duracao?: number; // em minutos
  }

  export interface SessaoDashboardStatsDto {
    tempoTotalEstudado: string; 
    diasEstudados: number;
    totalDias: number;
    percentualDiasEstudados: number;
    materiaMaisEstudada: string;
    horasMateriaMaisEstudada: number;
  }