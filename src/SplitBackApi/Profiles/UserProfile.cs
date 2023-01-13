﻿using AutoMapper;
using SplitBackApi.Domain;
using SplitBackApi.Endpoints.Requests;
using MongoDB.Bson;

namespace SplitBackApi.Configuration;

public class StringToObjectIdConverter : ITypeConverter<string, ObjectId>
{
  public ObjectId Convert(string source, ObjectId destination, ResolutionContext context)
  {
    return ObjectId.Parse(source);
  }
}
public class UserProfile : Profile
{
  public UserProfile()
  {
    //source ->target
    CreateMap<UserCreateDto, User>();
    CreateMap<LabelDto, Label>();

    CreateMap<ExpenseParticipantDto, Participant>()
    .ForMember(dest => dest.Id, opt => opt
    .MapFrom(src => src.ParticipantId));

    CreateMap<ExpenseSpenderDto, Spender>()
    .ForMember(dest => dest.Id, opt => opt
    .MapFrom(src => src.SpenderId));

    CreateMap<NewExpenseDto, Expense>()
    .ForMember(dest => dest.Id, opt => opt
    .MapFrom(src => ObjectId.GenerateNewId()));

    CreateMap<NewTransferDto, Transfer>()
    .ForMember(dest => dest.Id, opt => opt
    .MapFrom(src => ObjectId.GenerateNewId()));

    CreateMap<EditExpenseDto, Expense>();
    CreateMap<EditTransferDto, Transfer>();
    CreateMap<CreateGroupDto, Group>();

    CreateMap<Expense, ExpenseSnapshot>()
    .ForMember(dest => dest.Id, opt => opt
    .MapFrom(src => ObjectId.GenerateNewId()));

    CreateMap<Transfer, TransferSnapshot>()
    .ForMember(dest => dest.Id, opt => opt
    .MapFrom(src => ObjectId.GenerateNewId()));

    CreateMap<NewCommentDto, Comment>()
    .ForMember(dest => dest.Id, opt => opt
    .MapFrom(src => ObjectId.GenerateNewId()));
  }
}