using ERP_API.Models.DTOs;
using ERP_API.Models.Enums;
using FluentValidation;

namespace ERP_API.Validators
{
    public class MetaRequestValidator : AbstractValidator<MetaRequestDto>
    {
        public MetaRequestValidator()
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
            When(x => x.Tipo == TipoMeta.Tempo, () =>
            {
                RuleFor(x => x.Unidade)
                    .Must(u => u == UnidadeMeta.Minutos || u == UnidadeMeta.Horas)
                    .WithMessage("Para metas de tempo, a unidade deve ser 'minutos' ou 'horas'");
            });

            When(x => x.Tipo == TipoMeta.QtdSessoes, () =>
            {
                RuleFor(x => x.Unidade)
                    .Equal(UnidadeMeta.Sessoes)
                    .WithMessage("Para metas de quantidade de sessões, a unidade deve ser 'sessoes'");
            });

            When(x => x.Tipo == TipoMeta.Topicos, () =>
            {
                RuleFor(x => x.Unidade)
                    .Equal(UnidadeMeta.Topicos)
                    .WithMessage("Para metas de tópicos, a unidade deve ser 'topicos'");

                RuleFor(x => x.MateriaId)
                    .NotNull()
                    .WithMessage("Para metas de tópicos, a matéria é obrigatória");
            });

            // Regras para frequência
            When(x => x.Frequencia == FrequenciaMeta.Semanal, () =>
            {
                RuleFor(x => x.DiasSemana)
                    .NotEmpty()
                    .WithMessage("Para metas com frequência semanal, os dias da semana são obrigatórios");
            });
        }
    }

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
            When(x => x.Tipo == TipoMeta.Tempo, () =>
            {
                RuleFor(x => x.Unidade)
                    .Must(u => u == UnidadeMeta.Minutos || u == UnidadeMeta.Horas)
                    .WithMessage("Para metas de tempo, a unidade deve ser 'minutos' ou 'horas'");
            });

            When(x => x.Tipo == TipoMeta.QtdSessoes, () =>
            {
                RuleFor(x => x.Unidade)
                    .Equal(UnidadeMeta.Sessoes)
                    .WithMessage("Para metas de quantidade de sessões, a unidade deve ser 'sessoes'");
            });

            When(x => x.Tipo == TipoMeta.Topicos, () =>
            {
                RuleFor(x => x.Unidade)
                    .Equal(UnidadeMeta.Topicos)
                    .WithMessage("Para metas de tópicos, a unidade deve ser 'topicos'");

                RuleFor(x => x.MateriaId)
                    .NotNull()
                    .WithMessage("Para metas de tópicos, a matéria é obrigatória");
            });

            // Regras para frequência
            When(x => x.Frequencia == FrequenciaMeta.Semanal, () =>
            {
                RuleFor(x => x.DiasSemana)
                    .NotEmpty()
                    .WithMessage("Para metas com frequência semanal, os dias da semana são obrigatórios");
            });
        }
    }
}