using AutoMapper;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Product
        CreateMap<Product, ProductDto>().ReverseMap();

        // Formula
        CreateMap<Formula, FormulaDto>().ReverseMap();

        // Material
        CreateMap<Material, MaterialDto>().ReverseMap();

        // Employee
        CreateMap<Employee, EmployeeDto>().ReverseMap();

        // Type
        CreateMap<Type, TypeDto>().ReverseMap();

        // Unit
        CreateMap<Unit, UnitDto>().ReverseMap();

        // Property
        CreateMap<Property, PropertyDto>().ReverseMap();

        // FormulaMaterial
        CreateMap<FormulaMaterial, FormulaMaterialDto>().ReverseMap();

        // FormulaProperty
        CreateMap<FormulaProperty, FormulaPropertyDto>().ReverseMap();
        CreateMap<Role, RoleDto>().ReverseMap();
    }
}
