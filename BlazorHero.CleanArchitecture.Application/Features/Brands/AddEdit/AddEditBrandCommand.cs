﻿using AutoMapper;
using BlazorHero.CleanArchitecture.Application.Interfaces.Repositories;
using BlazorHero.CleanArchitecture.Domain.Entities.Catalog;
using BlazorHero.CleanArchitecture.Shared.Wrapper;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using LazyCache;
using BlazorHero.CleanArchitecture.Shared.Constants.Application;

namespace BlazorHero.CleanArchitecture.Application.Features.Brands.AddEdit
{
    public partial class AddEditBrandCommand : IRequest<Result<int>>
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Tax { get; set; }
    }

    public class AddEditBrandCommandHandler : IRequestHandler<AddEditBrandCommand, Result<int>>
    {
        private readonly IMapper _mapper;
        private readonly IStringLocalizer<AddEditBrandCommandHandler> _localizer;
        private readonly IUnitOfWork _unitOfWork;

        public AddEditBrandCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, IStringLocalizer<AddEditBrandCommandHandler> localizer)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _localizer = localizer;
        }

        public async Task<Result<int>> Handle(AddEditBrandCommand command, CancellationToken cancellationToken)
        {
            if (command.Id == 0)
            {
                var brand = _mapper.Map<Brand>(command);
                await _unitOfWork.Repository<Brand>().AddAsync(brand);
                await _unitOfWork.ComitAndRemoveCache(cancellationToken, ApplicationConstants.Cache.GetAllBrandsCacheKey);
                return Result<int>.Success(brand.Id, _localizer["Brand Saved"]);
            }
            else
            {
                var brand = await _unitOfWork.Repository<Brand>().GetByIdAsync(command.Id);
                if (brand != null)
                {
                    brand.Name = command.Name ?? brand.Name;
                    brand.Tax = (command.Tax == 0) ? brand.Tax : command.Tax;
                    brand.Description = command.Description ?? brand.Description;
                    await _unitOfWork.Repository<Brand>().UpdateAsync(brand);
                    await _unitOfWork.ComitAndRemoveCache(cancellationToken, ApplicationConstants.Cache.GetAllBrandsCacheKey);
                    return Result<int>.Success(brand.Id, _localizer["Brand Updated"]);
                }
                else
                {
                    return Result<int>.Fail(_localizer["Brand Not Found!"]);
                }
            }
        }
    }
}