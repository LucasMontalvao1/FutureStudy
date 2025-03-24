import { MetaDia } from "./meta.models";
import { SessaoEstudo } from "./sessao-estudo.models";

export interface DiaCalendario {
    dia: number;
    minutosEstudados: number;
  }
  
  export interface DetalheDia {
    data: Date;
    sessoes: SessaoEstudo[];
    totalMinutos: number;
    metas: MetaDia[];
  }