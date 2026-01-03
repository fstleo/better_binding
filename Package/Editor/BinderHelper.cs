#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BetterBinding.Editor.Utils;
using BetterBinding.Runtime;
using UnityEngine;

namespace BetterBinding.Editor
{
    public static class BinderHelper
    {
        private const string PropertyName = "Property";
        private static readonly Type BaseType = typeof(IViewModel);
        private static readonly Type BaseCollectionType = typeof(ICollectionViewModel<>);
        private static readonly Type BindableInterface = typeof(IBindable<>);
        private static readonly Type MonoBehaviourType = typeof(MonoBehaviour);
        public static readonly Dictionary<string, ulong> ContractsByName = new();
        public static readonly Dictionary<ulong, string> ContractsById = new();
        
        public static readonly Dictionary<ulong, Dictionary<ulong, (string Name, Type Type)>> PropertiesByContracts = new();

        static BinderHelper()
        {
            foreach (var contract in GetContractTypes())
            {
                var hash = CalculateHash(contract.Name);
                ContractsByName.Add(contract.Name, hash);
                ContractsById.Add(hash, contract.Name);
                PropertiesByContracts.Add(hash,
                    GetBindableProperties(contract)
                        .ToDictionary(property => CalculateHash(property.Name), 
                            property => property));
            }      
        }
        
        //TODO: move to a separate private static ulong CalculateHash(string read)
        private static ulong CalculateHash(string read)
        {
            var hashedValue = 3074457345618258791ul;
            foreach (var t in read)
            {
                hashedValue += t;
                hashedValue *= 3074457345618258799ul;
            }
            return hashedValue;
        }

        private static IEnumerable<Type> GetContractTypes()
        {
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => BaseType.IsAssignableFrom(p) && p != BaseType); 
            return types;
        }

        private static IEnumerable<(string Name, Type Type)> GetBindableProperties(Type type)
        {
            return type.GetProperties()
                .Where(property => property.PropertyType.Name.Contains(PropertyName) 
                                   || BaseType.IsAssignableFrom(property.PropertyType) 
                                      && property.PropertyType != BaseType)
                .Select(property => (property.Name, property.PropertyType));
        }
        
        public static IEnumerable<Type> GetBindableTypesFor(Type propertyType)
        {
            Type? baseType = null;
            if (BaseType.IsAssignableFrom(propertyType))
            {
                baseType = typeof(IBindable<IViewModel>);
            }

            var baseCollectionType = BaseCollectionType.MakeGenericType(propertyType.GenericTypeArguments);
            if (baseCollectionType.IsAssignableFrom(propertyType))
            {
                baseType = BindableInterface.MakeGenericType(baseCollectionType);
            }

            baseType ??= BindableInterface.MakeGenericType(propertyType);

            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p =>
                {
                    if (!p.IsAbstract && !p.IsInterface && p != baseType
                        && p.GetCustomAttribute<HideInBinderAttribute>() == null
                        && !MonoBehaviourType.IsAssignableFrom(p)
                        && ContainsBindingInterface(p))
                    {
                        return !p.IsGenericType && baseType.IsAssignableFrom(p);
                    }

                    return false;
                });
            return types;
        }

        private static bool ContainsBindingInterface(Type t)
        {
            return t.GetInterface(BindableInterface.Name) != null;
        }
    }
}
