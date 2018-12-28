﻿namespace TestRunner
{
    public static class TypeDiscoverer
    {
        private static global::System.Type[] _types;

        public static global::System.Collections.Generic.IEnumerable<global::System.Type> GetAllTypes()
        {
            GetAllTypesPrivate(ref _types);
            return _types;
        }

        private static void GetAllTypesPrivate(ref global::System.Type[] types)
        {
            if (_types != null)
            {
                return;
            }
#if NET20 || NET30 || NET40 || NET45 || NET46 || NET47 || NETCOREAPP1_0 || NETCOREAPP1_1 || NETCOREAPP2_1 || NETCOREAPP2_2 || NETSTANDARD1_5 || NETSTANDARD1_6
            types = global::System.Reflection.IntrospectionExtensions.GetTypeInfo(typeof(TypeDiscoverer)).Assembly.GetTypes();
#else
            types = new []
			{
				typeof(System.Threading.ThreadTest),
			};
#endif
		}
    }
}
