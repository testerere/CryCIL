﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using CryEngine.Extensions;

namespace CryEngine.Serialization
{
    public class CrySerializer : IFormatter
    {
        StreamWriter Writer { get; set; }
        StreamReader Reader { get; set; }
        FormatterConverter Converter { get; set; }

        /// <summary>
        /// We store a dictionary of all serialized objects in order to not create new instances of types with identical hash codes. (same objects)
        /// </summary>
        Dictionary<int, ObjectReference> ObjectReferences { get; set; }

        public CrySerializer()
        {
            Converter = new FormatterConverter();
            ObjectReferences = new Dictionary<int, ObjectReference>();
        }

        public void Serialize(Stream stream, object graph)
        {
#if !(RELEASE && RELEASE_DISABLE_CHECKS)
            if (stream == null)
                throw new ArgumentNullException("stream");
            if (graph == null)
                throw new ArgumentNullException("graph");
#endif

            Writer = new StreamWriter(stream) { AutoFlush = true };

            ObjectReferences.Clear();
            CurrentLine = 0;

            StartWrite(new ObjectReference("root", graph));
            stream.Seek(0, SeekOrigin.Begin);
        }

        void WriteLine(object value)
        {
            CurrentLine++;
            Writer.WriteLine(value);
        }

        /// <summary>
        /// Checks if this object has already been serialized.
        /// </summary>
        /// <param name="objectReference"></param>
        /// <returns>true if object had already been serialized.</returns>
        bool TryWriteReference(ObjectReference objectReference)
        {
            var type = objectReference.SerializationType;
            if (type == SerializationType.Enumerable ||
                type == SerializationType.GenericEnumerable ||
                type == SerializationType.Object ||
                type == SerializationType.MemberInfo ||
                type == SerializationType.MemberInfo ||
                type == SerializationType.Delegate ||
                type == SerializationType.Type)
            {
                foreach (var pair in ObjectReferences)
                {
                    if (pair.Value.Value.Equals(objectReference.Value))
                    {
                        WriteReference(objectReference, pair.Key);
                        return true;
                    }
                }

                ObjectReferences.Add(CurrentLine, objectReference);
            }

            return false;
        }

        void StartWrite(ObjectReference objectReference)
        {
            Type valueType = objectReference.Value != null ? objectReference.Value.GetType() : null;

            WriteLine(objectReference.Name);

            if (TryWriteReference(objectReference))
                return;

            WriteLine(objectReference.SerializationType);

            switch(objectReference.SerializationType)
            {
                case SerializationType.Null:
                    break;
                case SerializationType.IntPtr:
                    WriteIntPtr(objectReference);
                    break;
                case SerializationType.Any:
                    WriteAny(objectReference);
                    break;
                case SerializationType.String:
                    WriteString(objectReference);
                    break;
                case SerializationType.Enumerable:
                    WriteEnumerable(objectReference);
                    break;
                case SerializationType.GenericEnumerable:
                    WriteGenericEnumerable(objectReference);
                    break;
                case SerializationType.Enum:
                    WriteEnum(objectReference);
                    break;
                case SerializationType.Type:
                    WriteType(objectReference);
                    break;
                case SerializationType.Delegate:
                    WriteDelegate(objectReference);
                    break;
                case SerializationType.MemberInfo:
                    WriteMemberInfo(objectReference);
                    break;
                case SerializationType.Object:
                    WriteObject(objectReference);
                    break;
            }
        }

        void WriteIntPtr(ObjectReference objectReference)
        {
            WriteLine(((IntPtr)objectReference.Value).ToInt64());
        }

        void WriteReference(ObjectReference objReference, int line)
        {
            WriteLine(SerializationType.Reference);
            WriteLine(line);
        }

        void WriteAny(ObjectReference objectReference)
        {
            WriteType(objectReference.Value.GetType());
            WriteLine(Converter.ToString(objectReference.Value));
        }

        void WriteString(ObjectReference objectReference)
        {
            WriteLine(objectReference.Value);
        }

        void WriteEnum(ObjectReference objectReference)
        {
            WriteType(objectReference.Value.GetType());
            WriteLine(objectReference.Value);
        }

        void WriteEnumerable(ObjectReference objectReference)
        {
            var array = (objectReference.Value as IEnumerable).Cast<object>();
            var numElements = array.Count();
            WriteLine(numElements);

            WriteType(GetIEnumerableElementType(array.GetType()));

            for (int i = 0; i < numElements; i++)
                StartWrite(new ObjectReference(i.ToString(), array.ElementAt(i)));
        }

        void WriteGenericEnumerable(ObjectReference objectReference)
        {
            var enumerable = (objectReference.Value as IEnumerable).Cast<object>();

            WriteLine(enumerable.Count());

            var type = objectReference.Value.GetType();
            WriteType(type);

            if (type.Implements<IDictionary>())
            {
                int i = 0;
                foreach (var element in enumerable)
                {
                    StartWrite(new ObjectReference("key_" + i.ToString(), element.GetType().GetProperty("Key").GetValue(element, null)));
                    StartWrite(new ObjectReference("value_" + i.ToString(), element.GetType().GetProperty("Value").GetValue(element, null)));
                    i++;
                }
            }
            else
            {
                for (int i = 0; i < enumerable.Count(); i++)
                    StartWrite(new ObjectReference(i.ToString(), enumerable.ElementAt(i)));
            }
        }

        void WriteObject(ObjectReference objectReference)
        {
            var type = objectReference.Value.GetType();
            WriteType(type);

            while (type != null)
            {
                var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                WriteLine(fields.Length);
                foreach (var field in fields)
                    StartWrite(new ObjectReference(field.Name, field.GetValue(objectReference.Value)));

                type = type.BaseType;
            }
        }

        void WriteMemberInfo(ObjectReference objectReference)
        {
            var memberInfo = objectReference.Value as MemberInfo;
            WriteMemberInfo(memberInfo);
        }

        void WriteMemberInfo(MemberInfo memberInfo)
        {
            WriteLine(memberInfo.Name);
            WriteType(memberInfo.ReflectedType);
            WriteLine(memberInfo.MemberType);
        }

        void WriteDelegate(ObjectReference objectReference)
        {
            var _delegate = objectReference.Value as Delegate;
            WriteType(_delegate.GetType());
            WriteMemberInfo(_delegate.Method);
            if (_delegate.Target != null)
            {
                WriteLine("target");
                StartWrite(new ObjectReference("delegateTarget", _delegate.Target));
            }
            else
                WriteLine("null_target");
        }

        void WriteType(ObjectReference objectReference)
        {
            WriteType(objectReference.Value as Type);
        }

        void WriteType(Type type)
        {
            WriteLine(type.IsGenericType);

            if (type.IsGenericType)
            {
                WriteLine(type.GetGenericTypeDefinition().FullName);

                var genericArgs = type.GetGenericArguments();
                WriteLine(genericArgs.Length);
                foreach (var genericArg in genericArgs)
                    WriteType(genericArg);
            }
            else
                WriteLine(type.FullName);
        }

        public object Deserialize(Stream stream)
        {
#if !(RELEASE && RELEASE_DISABLE_CHECKS)
            if (stream == null || stream.Length == 0)
                throw new ArgumentNullException("stream");
#endif

            Reader = new StreamReader(stream);

            ObjectReferences.Clear();
            CurrentLine = 0;

            return StartRead().Value;
        }

        string ReadLine()
        {
            CurrentLine++;

            return Reader.ReadLine();
        }

        ObjectReference StartRead()
        {
            var objReference = new ObjectReference();

            objReference.Name = ReadLine();

            objReference.SerializationType = (SerializationType)Enum.Parse(typeof(SerializationType), ReadLine());
            int line = CurrentLine;

            switch (objReference.SerializationType)
            {
                case SerializationType.Null: break;
                case SerializationType.Reference: ReadReference(objReference); break;
                case SerializationType.Object: ReadObject(objReference); break;
                case SerializationType.GenericEnumerable: ReadGenericEnumerable(objReference); break;
                case SerializationType.Enumerable: ReadEnumerable(objReference); break;
                case SerializationType.Enum: ReadEnum(objReference); break;
                case SerializationType.Any: ReadAny(objReference); break;
                case SerializationType.String: ReadString(objReference); break;
                case SerializationType.MemberInfo: ReadMemberInfo(objReference); break;
                case SerializationType.Type: ReadType(objReference); break;
                case SerializationType.Delegate: ReadDelegate(objReference); break;
                case SerializationType.IntPtr: ReadIntPtr(objReference); break;
            }

#if !(RELEASE && RELEASE_DISABLE_CHECKS)
            if (!objReference.AllowNull && objReference.Value == null && objReference.SerializationType != SerializationType.Null)
                throw new SerializationException(string.Format("Failed to deserialize object of type {0} {1} at line {2}!", objReference.SerializationType, objReference.Name, line));
#endif

            return objReference;
        }

        void ReadIntPtr(ObjectReference objReference)
        {
            objReference.Value = new IntPtr(Int64.Parse(ReadLine()));
        }

        void ReadReference(ObjectReference objReference)
        {
            int referenceLine = int.Parse(ReadLine());
            objReference.Value = ObjectReferences[referenceLine].Value;

#if !(RELEASE && RELEASE_DISABLE_CHECKS)
            if (objReference.Value == null)
                throw new SerializationException(string.Format("Failed to obtain reference {0} at line {1}! Last line was {2})", objReference.Name, referenceLine, CurrentLine));
#endif
        }

        void ReadObject(ObjectReference objReference)
        {
            AddReferenceToObject(objReference);

            var type = ReadType();

            try
            {
                objReference.Value = Activator.CreateInstance(type);
            }
            catch (MissingMethodException) { objReference.AllowNull = true; } // types lacking default constructors can't be serialized.

            while (type != null)
            {
                var numFields = int.Parse(ReadLine());

                for (int i = 0; i < numFields; i++)
                {
                    var fieldReference = StartRead();

                    var fieldInfo = type.GetField(fieldReference.Name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                    if (fieldInfo != null)
                    {
                        if (objReference.Value != null)
                            fieldInfo.SetValue(objReference.Value, fieldReference.Value);
                    }
                    else
                        throw new MissingFieldException(string.Format("Failed to find field {0} in type {1}", fieldReference.Name, type.Name));
                }

                type = type.BaseType;
            }
        }

        void ReadEnumerable(ObjectReference objReference)
        {
            AddReferenceToObject(objReference);

            var numElements = int.Parse(ReadLine());
            var type = ReadType();

            objReference.Value = Array.CreateInstance(type, numElements);
            var array = objReference.Value as Array;

            for (int i = 0; i != numElements; ++i)
                array.SetValue(StartRead().Value, i);
        }

        void ReadGenericEnumerable(ObjectReference objReference)
        {
            AddReferenceToObject(objReference);

            int elements = int.Parse(ReadLine());

            var type = ReadType();

            objReference.Value = Activator.CreateInstance(type);

            if (type.Implements<IDictionary>())
            {
                var dict = objReference.Value as IDictionary;

                for (int i = 0; i < elements; i++)
                {
                    var key = StartRead().Value;
                    var value = StartRead().Value;

                    dict.Add(key, value);
                }
            }
            else
            {
                var list = objReference.Value as IList;

                for (int i = 0; i < elements; i++)
                    list.Add(StartRead().Value);
            }
        }

        void ReadAny(ObjectReference objReference)
        {
            var type = ReadType();
            string valueString = ReadLine();

            objReference.Value = Converter.Convert(valueString, type);
        }

        void ReadString(ObjectReference objReference)
        {
            objReference.Value = ReadLine();
        }

        void ReadEnum(ObjectReference objReference)
        {
            var type = ReadType();
            string valueString = ReadLine();

            objReference.Value = Enum.Parse(type, valueString);
        }

        void ReadMemberInfo(ObjectReference objReference)
        {
            AddReferenceToObject(objReference);

            objReference.Value = ReadMemberInfo();
        }

        MemberInfo ReadMemberInfo()
        {
            var memberName = ReadLine();

            var reflectedType = ReadType();
            var memberType = (MemberTypes)Enum.Parse(typeof(MemberTypes), ReadLine());

            var bindingFlags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance;
            switch (memberType)
            {
                case MemberTypes.Method:
                    return reflectedType.GetMethod(memberName, bindingFlags);
                case MemberTypes.Field:
                    return reflectedType.GetField(memberName, bindingFlags);
                case MemberTypes.Property:
                    return reflectedType.GetProperty(memberName, bindingFlags);
            }

            return null;
        }

        void ReadDelegate(ObjectReference objReference)
        {
            AddReferenceToObject(objReference);

            var delegateType = ReadType();
            var methodInfo = ReadMemberInfo() as MethodInfo;

            if (ReadLine() == "target")
                objReference.Value = Delegate.CreateDelegate(delegateType, StartRead().Value, methodInfo);
            else
                objReference.Value = Delegate.CreateDelegate(delegateType, methodInfo);
        }

        void ReadType(ObjectReference objReference)
        {
            AddReferenceToObject(objReference);

            objReference.Value = ReadType();
        }

        Type ReadType()
        {
            bool isGeneric = bool.Parse(ReadLine());

            var type = GetType(ReadLine());

            if (isGeneric)
            {
                var numGenericArgs = int.Parse(ReadLine());
                var genericArgs = new Type[numGenericArgs];
                for (int i = 0; i < numGenericArgs; i++)
                    genericArgs[i] = ReadType();

                return type.MakeGenericType(genericArgs);
            }

            return type;
        }

        Type GetType(string typeName)
        {
#if !(RELEASE && RELEASE_DISABLE_CHECKS)
            if (typeName == null)
                throw new ArgumentNullException("typeName");
            if (typeName.Length == 0)
                throw new ArgumentException("typeName cannot have zero length");
#endif

            if (typeName.Contains('+'))
            {
                var splitString = typeName.Split('+');
                var ownerType = GetType(splitString.First());

                return ownerType.Assembly.GetType(typeName);
            }

            Type type = Type.GetType(typeName);
            if (type != null)
                return type;

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = assembly.GetType(typeName);
                if (type != null)
                    return type;
            }

            throw new TypeLoadException(string.Format("Could not localize type with name {0}", typeName));
        }

        static Type GetIEnumerableElementType(Type enumerableType)
        {
            Type type = enumerableType.GetElementType();
            if (type != null)
                return type;

            // Not an array type, we've got to use an alternate method to get the type of elements contained within.
            if (enumerableType.IsGenericType && enumerableType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                type = enumerableType.GetGenericArguments()[0];
            else
            {
                Type[] interfaces = enumerableType.GetInterfaces();
                foreach (Type i in interfaces)
                    if (i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                        type = i.GetGenericArguments()[0];
            }

            return type;
        }

        void AddReferenceToObject(ObjectReference objReference)
        {
            ObjectReferences.Add(CurrentLine - 1, objReference);
        }

        int CurrentLine { get; set; }

        public SerializationBinder Binder { get { return null; } set { } }
        public ISurrogateSelector SurrogateSelector { get { return null; } set { } }
        public StreamingContext Context { get { return new StreamingContext(StreamingContextStates.Persistence); } set { } }
    }
}