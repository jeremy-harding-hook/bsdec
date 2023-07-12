using BsdecCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Bsdec.Serialisers
{
    internal class BinarySerialiser : ISerialiser
    {
        public static object Deserialise(Stream inputStream, Type type)
        {
            MethodInfo readerMethod = GetReaderMethod(type);
            ParameterInfo[] parameters = GetValidatedReaderParameters(readerMethod);
            object instance = CreateInstance(type);
            CallReaderMethod(instance, readerMethod, inputStream, parameters.Select(x => x.DefaultValue).ToArray());
            return instance;
        }

        public static void Serialise(object? serialisee, Stream outputStream)
        {
            if (serialisee == null)
            {
                Console.Error.WriteLine("Cannot serialise null object to binary. Aborting.");
                throw new GenericErrorException();
            }
            MethodInfo writerMethod = GetWriterMethod(serialisee.GetType());
            ParameterInfo[] parameters = GetValidatedWriterParameters(writerMethod);
            CallWriterMethod(serialisee, writerMethod, outputStream, parameters.Select(x => x.DefaultValue).ToArray());
        }

        private static MethodInfo GetReaderMethod(Type type)
        {
            IEnumerable<MethodInfo> readerMethodCandidates = type.GetMethods().Where(x => x.CustomAttributes.Any(y => y.AttributeType == typeof(BsdecReadMethodAttribute)));
            if (!readerMethodCandidates.Any())
            {
                Console.Error.WriteLine($"The schema given does not support reading binary files.");
                throw new GenericErrorException();
            }
            if (readerMethodCandidates.Count() > 1)
            {
                Console.Error.WriteLine($"The schema given is not in a supported format; it presents {readerMethodCandidates.Count()} different reader methods.");
                throw new GenericErrorException();
            }
            return readerMethodCandidates.First();
        }

        private static ParameterInfo[] GetValidatedReaderParameters(MethodInfo method)
        {
            ParameterInfo[] parameters = method.GetParameters();

            if (parameters.First().ParameterType != typeof(BinaryReader))
            {
                Console.Error.WriteLine($"The schema given is not in a supported format; the reader method's first parameter is required to be of type {nameof(BinaryReader)}.");
                throw new GenericErrorException();
            }

            for (int i = 1; i < parameters.Length; i++)
            {
                if (!parameters[i].HasDefaultValue)
                {
                    Console.Error.WriteLine($"The schema given is not in a supported format; all parameters of the reader method other than the first are required to have default values.");
                    Console.Error.WriteLine($"In particular, no default value is present for parameter {parameters[i].Name}");
                    throw new GenericErrorException();
                }
            }

            return parameters;
        }

        private static object CreateInstance(Type type)
        {
            object? result;
            try
            {
                result = Activator.CreateInstance(type);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Could not create instance of {type.FullName}, possibly because the parameterless constructor is not public. Exception: {ex}");
                throw new GenericErrorException();
            }
            if (result == null)
            {
                Console.Error.WriteLine($"Error creating instance of {type.FullName}: for some reason it is a nullable value type, which are not supported.");
                throw new GenericErrorException();
            }
            return result;
        }

        /// <summary>
        /// Invokes the reader method.
        /// </summary>
        /// <param name="instance">The instance upon which to invoke the method.</param>
        /// <param name="readerMethod">The method to invoke.</param>
        /// <param name="inputStream">The input stream to be read.</param>
        /// <param name="parameters">The array of parameters to be passed. Note that the first parameter in this array will be replaced by the binary reader.</param>
        /// <exception cref="GenericErrorException">If an exception is raised during invokation.</exception>
        public static void CallReaderMethod(object instance, MethodInfo readerMethod, Stream inputStream, object?[] parameters)
        {
            try
            {
                BinaryReader reader = new(inputStream);
                parameters[0] = reader;
                readerMethod.Invoke(instance, parameters);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"There was an unexpected issue when invoking the reader method contained in the schema. Exception: {ex.InnerException}");
                throw new GenericErrorException();
            }
        }

        private static MethodInfo GetWriterMethod(Type type)
        {
            IEnumerable<MethodInfo> writerMethodCandidates = type.GetMethods().Where(x => x.CustomAttributes.Any(y => y.AttributeType == typeof(BsdecWriteMethodAttribute)));
            if (!writerMethodCandidates.Any())
            {
                Console.Error.WriteLine($"The schema given does not support writing binary files.");
                throw new GenericErrorException();
            }
            if (writerMethodCandidates.Count() > 1)
            {
                Console.Error.WriteLine($"The schema given is not in a supported format; it presents {writerMethodCandidates.Count()} different writer methods.");
                throw new GenericErrorException();
            }
            return writerMethodCandidates.First();
        }

        private static ParameterInfo[] GetValidatedWriterParameters(MethodInfo method)
        {
            ParameterInfo[] parameters = method.GetParameters();

            if (parameters.First().ParameterType != typeof(BinaryWriter))
            {
                Console.Error.WriteLine($"The schema given is not in a supported format; the writer method's first parameter is required to be of type {nameof(BinaryWriter)}.");
                throw new GenericErrorException();
            }

            for (int i = 1; i < parameters.Length; i++)
            {
                if (!parameters[i].HasDefaultValue)
                {
                    Console.Error.WriteLine($"The schema given is not in a supported format; all parameters of the writer method other than the first are required to have default values.");
                    Console.Error.WriteLine($"In particular, no default value is present for parameter {parameters[i].Name}");
                    throw new GenericErrorException();
                }
            }

            return parameters;
        }

        /// <summary>
        /// Invokes the writer method.
        /// </summary>
        /// <param name="instance">The instance upon which to invoke the method.</param>
        /// <param name="writerMethod">The method to invoke.</param>
        /// <param name="outputStream">The input stream to be read.</param>
        /// <param name="parameters">The array of parameters to be passed. Note that the first parameter in this array will be replaced by the binary writer.</param>
        /// <exception cref="GenericErrorException">If an exception is raised during invocation.</exception>
        public static void CallWriterMethod(object instance, MethodInfo writerMethod, Stream outputStream, object?[] parameters)
        {
            try
            {
                BinaryWriter writer = new(outputStream);
                parameters[0] = writer;
                writerMethod.Invoke(instance, parameters);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"There was an unexpected issue when invoking the writer method contained in the schema. Exception: {ex}");
                throw new GenericErrorException();
            }
        }
    }
}
