using ERP_API.Models.DTOs;
using ERP_API.Models.Entities;
using FluentValidation;

namespace ERP_API.Validators
{
    public class SessaoEstudoRequestDtoValidator : AbstractValidator<SessaoEstudoRequestDto>
    {
        public SessaoEstudoRequestDtoValidator()
        {
            RuleFor(x => x.MateriaId)
                .NotEmpty().WithMessage("A matéria é obrigatória")
                .GreaterThan(0).WithMessage("ID da matéria inválido");

            RuleFor(x => x.TopicoId)
                .GreaterThan(0).WithMessage("ID do tópico inválido")
                .When(x => x.TopicoId.HasValue);
        }
    }

    public class SessaoEstudoValidator : AbstractValidator<SessaoEstudo>
    {
        public SessaoEstudoValidator()
        {
            RuleFor(x => x.UsuarioId)
                .NotEmpty().WithMessage("O usuário é obrigatório")
                .GreaterThan(0).WithMessage("ID do usuário inválido");

            RuleFor(x => x.MateriaId)
                .NotEmpty().WithMessage("A matéria é obrigatória")
                .GreaterThan(0).WithMessage("ID da matéria inválido");

            RuleFor(x => x.TopicoId)
                .GreaterThan(0).WithMessage("ID do tópico inválido")
                .When(x => x.TopicoId.HasValue);

            RuleFor(x => x.DataInicio)
            .NotEmpty().WithMessage("A data de início é obrigatória")
            .When(x => x.Id != 0);

            RuleFor(x => x.DataFim)
                .GreaterThan(x => x.DataInicio)
                .When(x => x.DataFim.HasValue)
                .WithMessage("A data de fim deve ser posterior à data de início");
        }
    }

    public class PausaRequestDtoValidator : AbstractValidator<PausaRequestDto>
    {
        public PausaRequestDtoValidator()
        {
            RuleFor(x => x.SessaoId)
                .NotEmpty().WithMessage("O ID da sessão é obrigatório")
                .GreaterThan(0).WithMessage("ID da sessão inválido");
        }
    }

    public class PausaValidator : AbstractValidator<PausaSessao>
    {
        public PausaValidator()
        {
            RuleFor(x => x.UsuarioId)
                .NotEmpty().WithMessage("O usuário é obrigatório")
                .GreaterThan(0).WithMessage("ID do usuário inválido");

            RuleFor(x => x.SessaoId)
                .NotEmpty().WithMessage("A sessão é obrigatória")
                .GreaterThan(0).WithMessage("ID da sessão inválido");

            RuleFor(x => x.Inicio)
                .NotEmpty().WithMessage("A data de início é obrigatória");

            RuleFor(x => x.Fim)
                .GreaterThan(x => x.Inicio)
                .When(x => x.Fim.HasValue)
                .WithMessage("A data de fim deve ser posterior à data de início");
        }
    }
}