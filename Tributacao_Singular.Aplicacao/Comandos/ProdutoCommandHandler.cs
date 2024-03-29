﻿using AutoMapper;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tributacao_Singular.Aplicacao.Comandos.ProdutoComandos;
using Tributacao_Singular.Aplicacao.Excecoes;
using Tributacao_Singular.Aplicacao.ViewModels;
using Tributacao_Singular.Negocio.Interfaces;
using Tributacao_Singular.Negocio.Modelos;
using Vir_Fundos_Infraestrutura.Comunicacao.Mediador;
using Vir_Fundos_Infraestrutura.Mensagens;
using Vir_Fundos_Infraestrutura.Mensagens.Notificacao;

namespace Tributacao_Singular.Aplicacao.Comandos
{
    public class ProdutoCommandHandler :
        IRequestHandler<AtualizarProdutoComando, bool>,
        IRequestHandler<RemoverProdutoComando, bool>,
        IRequestHandler<AdicionarProdutoComando, bool>
    {
        private readonly IMediatorHandler mediadorHandler;
        private readonly IProdutoRepositorio respositorioProduto;
        private readonly IClienteRepositorio respositorioCliente;
        private readonly ICategoriaRepositorio respositorioCategoria;
        private readonly IMapper mapper;

        public ProdutoCommandHandler(IMediatorHandler mediadorHandler, IProdutoRepositorio respositorioProduto, IClienteRepositorio respositorioCliente, IMapper mapper, ICategoriaRepositorio respositorioCategoria)
        {
            this.mediadorHandler = mediadorHandler;
            this.respositorioProduto = respositorioProduto;
            this.respositorioCliente = respositorioCliente;
            this.respositorioCategoria = respositorioCategoria;
            this.mapper = mapper;
        }

        public async Task<bool> Handle(AdicionarProdutoComando request, CancellationToken cancellationToken)
        {
            try
            {
                if (!ValidarComando(request)) return false;

                var ClienteExiste = await respositorioCliente.ObterClienteProdutosPorId(request.ClienteId);

                var ProcuraCategoriaBase = await respositorioCategoria.Buscar(x => x.descricao.Equals("CategoriaBase"));

                if (ClienteExiste == null)
                {
                    await mediadorHandler.PublicarNotificacao(new NotificacaoDominio(request.Tipo, "Não existe um Cliente informado."));
                    return false;
                }
                else if (ProcuraCategoriaBase == null)
                {
                    await mediadorHandler.PublicarNotificacao(new NotificacaoDominio(request.Tipo, "Não existe uma categoria base informada, favor contactar serviço tecnico."));
                    return false;
                }
                else 
                {
                    var categoriaBase = ProcuraCategoriaBase.First();

                    var produto = new Produto();
                    produto.Id = request.Id;
                    produto.descricao = request.descricao;
                    produto.EAN = request.EAN;
                    produto.NCM = request.NCM;
                    produto.Status = 0;
                    produto.ClienteId = ClienteExiste.Id;
                    produto.CategoriaId = categoriaBase.Id;
                    produto.Cliente = null;
                    produto.Categoria = null;

                    await respositorioProduto.Adicionar(produto);

                    return true;
                }
            }
            catch (DominioException ex)
            {
                await mediadorHandler.PublicarNotificacao(new NotificacaoDominio(request.Tipo, ex.Message));
                return false;
            }
            catch (Exception ex)
            {
                await mediadorHandler.PublicarNotificacao(new NotificacaoDominio(request.Tipo, ex.Message));
                return false;
            }
        }

        public async Task<bool> Handle(AtualizarProdutoComando request, CancellationToken cancellationToken)
        {
            try 
            {
                if (!ValidarComando(request)) return false;

                var ProdutoExiste = await respositorioProduto.ObterPorId(request.Id);

                if (ProdutoExiste == null) 
                {
                    await mediadorHandler.PublicarNotificacao(new NotificacaoDominio(request.Tipo, "Não Existe um Produto Informado."));
                    return false;
                }
                else 
                {
                    ProdutoExiste.descricao = request.descricao;
                    ProdutoExiste.EAN = request.EAN;
                    ProdutoExiste.NCM = request.NCM;
                    ProdutoExiste.Status = request.Status;
                    ProdutoExiste.CategoriaId = request.CategoriaId;

                    await respositorioProduto.Atualizar(ProdutoExiste);

                    return true;
                }
            }
            catch (DominioException ex)
            {
                await mediadorHandler.PublicarNotificacao(new NotificacaoDominio(request.Tipo, ex.Message));
                return false;
            }
            catch (Exception ex)
            {
                await mediadorHandler.PublicarNotificacao(new NotificacaoDominio(request.Tipo, ex.Message));
                return false;
            }
        }

        public async Task<bool> Handle(RemoverProdutoComando request, CancellationToken cancellationToken)
        {
            try
            {
                if (!ValidarComando(request)) return false;

                await respositorioProduto.Remover(request.Id);

                return true;
            }
            catch (DominioException ex)
            {
                await mediadorHandler.PublicarNotificacao(new NotificacaoDominio(request.Tipo, ex.Message));
                return false;
            }
            catch (Exception ex)
            {
                await mediadorHandler.PublicarNotificacao(new NotificacaoDominio(request.Tipo, ex.Message));
                return false;
            }
        }

        private bool ValidarComando(Comando mensagem)
        {
            if (mensagem.EhValido()) return true;

            foreach (var error in mensagem.ResultadoValidacao.Errors)
            {
                mediadorHandler.PublicarNotificacao(new NotificacaoDominio(mensagem.Tipo, error.ErrorMessage));
            }

            return false;
        }
    }
}
