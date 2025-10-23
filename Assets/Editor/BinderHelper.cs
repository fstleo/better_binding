#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using BetterBinding.Runtime;
using UnityEngine;

namespace BetterBinding.Editor
{
    public static class BinderHelper
    {
        private static readonly Type BaseType = typeof(IViewModel);
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
                .Where(property => property.Name.Contains("Property") 
                                   || BaseType.IsAssignableFrom(property.PropertyType) 
                                      && property.PropertyType != BaseType)
                .Select(property => (property.Name, property.PropertyType));
        }
        
        public static IEnumerable<Type> GetBindableTypesFor(Type propertyType)
        {
            var baseType = BaseType.IsAssignableFrom(propertyType) 
                ? typeof(IBindable<IViewModel>) 
                : typeof(IBindable<>).MakeGenericType(propertyType);
            
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => baseType.IsAssignableFrom(p) 
                            && !MonoBehaviourType.IsAssignableFrom(p)
                            && p != baseType 
                            && !p.IsAbstract 
                            && !p.IsInterface); 
            return types;
        }
    }
}
