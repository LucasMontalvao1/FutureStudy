using ERP_API.Models;
using ERP_API.Models.Enums;
using FluentValidation;

namespace ERP_API.Validators
{
    public class MetaValidator : AbstractValidator<Meta>
    {
        public MetaValidator()
        {
            RuleFor(x => x.Titulo)
                .NotEmpty().WithMessage("O título da meta é obrigatório")
                .MaximumLength(100).WithMessage("O título deve ter no máximo 100 caracteres");

            RuleFor(x => x.QuantidadeTotal)
                .GreaterThan(0).WithMessage("A quantidade total deve ser maior que zero");

            RuleFor(x => x.DataInicio)
                .NotEmpty().WithMessage("A data de início é obrigatória");

            RuleFor(x => x.DataFim)
                .GreaterThan(x => x.DataInicio)
                .When(x => x.DataFim.HasValue)
                .WithMessage("A data de fim deve ser posterior à data de início");

            // Regras específicas por tipo de meta
            When(x => x.TipoMeta == TipoMeta.TempoTotal, () =>
            {
                RuleFor(x => x.Unidade)
                    .Must(u => u == UnidadeMeta.Minutos || u == UnidadeMeta.Horas)
                    .WithMessage("Para metas de tempo total, a unidade deve ser 'minutos' ou 'horas'");
            });

            When(x => x.TipoMeta == TipoMeta.SessoesConcluidas, () =>
            {
                RuleFor(x => x.Unidade)
                    .Equal(UnidadeMeta.Sessoes)
                    .WithMessage("Para metas de sessões concluídas, a unidade deve ser 'sessoes'");
            });

            When(x => x.TipoMeta == TipoMeta.TopicosEstudados, () =>
            {
                RuleFor(x => x.Unidade)
                    .Equal(UnidadeMeta.Topicos)
                    .WithMessage("Para metas de tópicos estudados, a unidade deve ser 'topicos'");

                RuleFor(x => x.MateriaId)
                    .NotNull()
                    .WithMessage("Para metas de tópicos, a matéria é obrigatória");
            });

            When(x => x.TipoMeta == TipoMeta.CategoriasCompletas, () =>
            {
                RuleFor(x => x.Unidade)
                    .Equal(UnidadeMeta.Categorias)
                    .WithMessage("Para metas de categorias completas, a unidade deve ser 'categorias'");
            });

            // Regras para frequência
            When(x => x.Frequencia == FrequenciaMeta.Semanal, () =>
            {
                RuleFor(x => x.DiasSemana)
                    .NotEmpty()
                    .WithMessage("Para metas com frequência semanal, os dias da semana são obrigatórios")
                    .Must(ValidateDiasSemana)
                    .WithMessage("Os dias da semana devem ser válidos (dom,seg,ter,qua,qui,sex,sab)");
            });

            RuleFor(x => x.NotificarPorcentagem)
                .InclusiveBetween(1, 100)
                .When(x => x.NotificarQuandoConcluir)
                .WithMessage("A porcentagem de notificação deve estar entre 1 e 100");
        }

        private bool ValidateDiasSemana(string? diasSemana)
        {
            if (string.IsNullOrEmpty(diasSemana)) return false;

            var diasValidos = new[] { "dom", "seg", "ter", "qua", "qui", "sex", "sab" };
            var dias = diasSemana.Split(',');

            return dias.All(dia => diasValidos.Contains(dia.Trim().ToLower()));
        }
    }
}