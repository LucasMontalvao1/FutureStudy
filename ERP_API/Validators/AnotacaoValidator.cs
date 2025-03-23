using ERP_API.Models.DTOs;
using ERP_API.Models.Entities;
using FluentValidation;

namespace ERP_API.Validators
{
    public class AnotacaoRequestDtoValidator : AbstractValidator<AnotacaoRequestDto>
    {
        public AnotacaoRequestDtoValidator()
        {
            RuleFor(x => x.SessaoId)
                .NotEmpty().WithMessage("O ID da sessão é obrigatório")
                .GreaterThan(0).WithMessage("ID da sessão inválido");

            RuleFor(x => x.Titulo)
                .NotEmpty().WithMessage("O título é obrigatório")
                .MaximumLength(100).WithMessage("O título não pode ter mais de 100 caracteres");

            RuleFor(x => x.Conteudo)
                .NotEmpty().WithMessage("O conteúdo é obrigatório");
        }
    }

    public class AnotacaoUpdateDtoValidator : AbstractValidator<AnotacaoUpdateDto>
    {
        public AnotacaoUpdateDtoValidator()
        {
            RuleFor(x => x.Titulo)
                .MaximumLength(100).WithMessage("O título não pode ter mais de 100 caracteres");
        }
    }

    public class AnotacaoValidator : AbstractValidator<Anotacao>
    {
        public AnotacaoValidator()
        {
            RuleFor(x => x.UsuarioId)
                .NotEmpty().WithMessage("O usuário é obrigatório")
                .GreaterThan(0).WithMessage("ID do usuário inválido");

            RuleFor(x => x.SessaoId)
                .NotEmpty().WithMessage("A sessão é obrigatória")
                .GreaterThan(0).WithMessage("ID da sessão inválido");

            RuleFor(x => x.Titulo)
                .NotEmpty().WithMessage("O título é obrigatório")
                .MaximumLength(100).WithMessage("O título não pode ter mais de 100 caracteres");

            RuleFor(x => x.Conteudo)
                .NotEmpty().WithMessage("O conteúdo é obrigatório");
        }
    }
}