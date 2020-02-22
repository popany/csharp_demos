using NUnit.Framework;
using unit_test.Utils;
using System.Linq;

namespace unit_test.TestFixtures
{
    public class ExampleBase { }

    public interface IExampleA { }

    public interface IExampleB { }

    public class ExampleDerived : ExampleBase, IExampleA, IExampleB { }

    public class Example { }

    class Assembly
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void PrintAssemblies()
        {
            System.AppDomain currentDomain = System.AppDomain.CurrentDomain;
            System.Reflection.Assembly[] assemblies = currentDomain.GetAssemblies();

            System.Text.StringBuilder stringbuilder = new System.Text.StringBuilder();
            stringbuilder.Append("\n");
            foreach (System.Reflection.Assembly assembly in assemblies)
            {
                stringbuilder.AppendFormat("name: {0}", assembly.GetName());
                stringbuilder.Append("\n");
            }
            Log.Info(stringbuilder.ToString());
        }
        
        System.Reflection.Assembly GetAssemblyByName(string name)
        {
            return System.AppDomain.CurrentDomain.GetAssemblies().SingleOrDefault(assembly => assembly.GetName().Name == name);
        }

        private static void DisplayGenericParameters(System.Type t)
        {
            if (!t.IsGenericType)
            {
                System.Console.WriteLine("Type '{0}' is not generic.");
                return;
            }
            if (!t.IsGenericTypeDefinition)
            {
                t = t.GetGenericTypeDefinition();
            }

            System.Type[] typeParameters = t.GetGenericArguments();
            System.Console.WriteLine("\nListing {0} type parameters for type '{1}'.",
                typeParameters.Length, t);

            foreach (System.Type tParam in typeParameters)
            {
                System.Console.WriteLine("\r\nType parameter {0}:", tParam.ToString());

                foreach (System.Type c in tParam.GetGenericParameterConstraints())
                {
                    if (c.IsInterface)
                    {
                        System.Console.WriteLine("    Interface constraint: {0}", c);
                    }
                    else
                    {
                        System.Console.WriteLine("    Base type constraint: {0}", c);
                    }
                }

                ListConstraintAttributes(tParam);
            }
        }

        private static void ListConstraintAttributes(System.Type t)
        {
            System.Reflection.GenericParameterAttributes constraints =
                t.GenericParameterAttributes & System.Reflection.GenericParameterAttributes.SpecialConstraintMask;

            if ((constraints & System.Reflection.GenericParameterAttributes.ReferenceTypeConstraint)
                != System.Reflection.GenericParameterAttributes.None)
            {
                System.Console.WriteLine("    ReferenceTypeConstraint");
            }

            if ((constraints & System.Reflection.GenericParameterAttributes.NotNullableValueTypeConstraint)
                != System.Reflection.GenericParameterAttributes.None)
            {
                System.Console.WriteLine("    NotNullableValueTypeConstraint");
            }

            if ((constraints & System.Reflection.GenericParameterAttributes.DefaultConstructorConstraint)
                != System.Reflection.GenericParameterAttributes.None)
            {
                System.Console.WriteLine("    DefaultConstructorConstraint");
            }
        }

        [Test]
        public void Define_a_Generic_Type_with_Reflection_Emit() // https://docs.microsoft.com/en-us/dotnet/framework/reflection-and-codedom/how-to-define-a-generic-type-with-reflection-emit
        {
            System.AppDomain currentDomain = System.AppDomain.CurrentDomain;
            System.Reflection.AssemblyName assemblyName = new System.Reflection.AssemblyName("GenericEmitExample1");
            System.Reflection.Emit.AssemblyBuilder assemblyBuilder = System.Reflection.Emit.AssemblyBuilder.DefineDynamicAssembly(
                assemblyName,
                System.Reflection.Emit.AssemblyBuilderAccess.Run);

            System.Reflection.Emit.ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule(
                assemblyName.Name + ".dll");

            System.Reflection.Emit.TypeBuilder typeBuilder = moduleBuilder.DefineType(
                "Sample",
                System.Reflection.TypeAttributes.Public);

            System.Console.WriteLine("Type 'Sample' is generic: {0}", typeBuilder.IsGenericType);

            string[] typeParamNames = { "TFirst", "TSecond" };
            System.Reflection.Emit.GenericTypeParameterBuilder[] typeParams =
                typeBuilder.DefineGenericParameters(typeParamNames);

            System.Reflection.Emit.GenericTypeParameterBuilder TFirst = typeParams[0];
            System.Reflection.Emit.GenericTypeParameterBuilder TSecond = typeParams[1];

            System.Console.WriteLine("Type 'Sample' is generic: {0}", typeBuilder.IsGenericType);

            TFirst.SetGenericParameterAttributes(
                System.Reflection.GenericParameterAttributes.DefaultConstructorConstraint |
                System.Reflection.GenericParameterAttributes.ReferenceTypeConstraint);

            System.Type baseType = typeof(ExampleBase);
            System.Type interfaceA = typeof(IExampleA);
            System.Type interfaceB = typeof(IExampleB);

            TSecond.SetBaseTypeConstraint(baseType);
            System.Type[] interfaceTypes = { interfaceA, interfaceB };
            TSecond.SetInterfaceConstraints(interfaceTypes);

            System.Reflection.Emit.FieldBuilder fieldBuilder = typeBuilder.DefineField(
                "ExampleField",
                TFirst,
                System.Reflection.FieldAttributes.Private);

            System.Type listOf = typeof(System.Collections.Generic.List<>);
            System.Type listOfTFirst = listOf.MakeGenericType(TFirst);
            System.Type[] paramTypes = { TFirst.MakeArrayType() };

            System.Reflection.Emit.MethodBuilder methodBuilder = typeBuilder.DefineMethod(
                    "ExampleMethod",
                    System.Reflection.MethodAttributes.Public | System.Reflection.MethodAttributes.Static,
                    listOfTFirst,
                    paramTypes);

            System.Reflection.Emit.ILGenerator ilgen = methodBuilder.GetILGenerator();

            System.Type ienumOf = typeof(System.Collections.Generic.IEnumerable<>);
            System.Type TfromListOf = listOf.GetGenericArguments()[0];
            System.Type ienumOfT = ienumOf.MakeGenericType(TfromListOf);
            System.Type[] ctorArgs = { ienumOfT };

            System.Reflection.ConstructorInfo ctorPrep = listOf.GetConstructor(ctorArgs);
            System.Reflection.ConstructorInfo ctor = System.Reflection.Emit.TypeBuilder.GetConstructor(listOfTFirst, ctorPrep);

            ilgen.Emit(System.Reflection.Emit.OpCodes.Ldarg_0);
            ilgen.Emit(System.Reflection.Emit.OpCodes.Newobj, ctor);
            ilgen.Emit(System.Reflection.Emit.OpCodes.Ret);

            System.Type finished = typeBuilder.CreateType();

            System.Type[] typeArgs = { typeof(Example), typeof(ExampleDerived) };
            System.Type constructed = finished.MakeGenericType(typeArgs);
            System.Reflection.MethodInfo methodInfo = constructed.GetMethod("ExampleMethod");

            Example[] input = { new Example(), new Example() };
            object[] arguments = { input };

            System.Collections.Generic.List<Example> listX = (System.Collections.Generic.List<Example>)methodInfo.Invoke(null, arguments);

            System.Console.WriteLine(
                "\nThere are {0} elements in the List<Example>.",
                listX.Count);

            DisplayGenericParameters(finished);

            System.Reflection.Assembly assembly = GetAssemblyByName("GenericEmitExample1");

            System.Type t = assembly.GetType("Sample");
            System.Reflection.Assembly a = System.Reflection.Assembly.GetAssembly(t);
        }
    }
}
