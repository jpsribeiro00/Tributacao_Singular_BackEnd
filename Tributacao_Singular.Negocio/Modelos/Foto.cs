﻿using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tributacao_Singular.Negocio.Modelos
{
    public class Foto: Entity
    {
        public byte[] Src { get; set; }
        public Guid idUsuario { get; set; }

        public Foto() { }
    
        public Foto(byte[] _src, Guid _idUsuario)
        {
            Src = _src;
            idUsuario = _idUsuario;
        }

        public bool EhValido()
        {
            var resultadoValidacao = new FotoValidation().Validate(this);
            return resultadoValidacao.IsValid;
        }
    }

    public class FotoValidation : AbstractValidator<Foto>
    {
        public FotoValidation()
        {
            RuleFor(p => p.Src)
                .NotEmpty().WithMessage("O campo {PropertyName} precisa ser fornecido");

            RuleFor(p => p.idUsuario)
                .NotEmpty().WithMessage("O campo {PropertyName} precisa ser fornecido");
        }
    }
}
