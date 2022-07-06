using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using EventBus.Api;

namespace EventBus{
    internal static class ASMEventListenerFactory{
        /*
        * Fields
        */
        private static AssemblyBuilder dynamicAssembly;
        private static ModuleBuilder dynamicModule;
        private static readonly Dictionary<MethodInfo, Type> cache = new Dictionary<MethodInfo, Type>();


        /*
        * Static methods
        */
        private static AssemblyBuilder GetDynamicAssembly(){
            if(dynamicAssembly == null){
                dynamicAssembly = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("ClassLoaderFactoryAsm"), AssemblyBuilderAccess.Run);
            }
            return dynamicAssembly;
        }
         
        private static ModuleBuilder GetDynamicModule(){
            if(dynamicModule == null){
                dynamicModule = GetDynamicAssembly().DefineDynamicModule("DynamicModule");
            }
            return dynamicModule;
        }
        
        private static string GetUniqueName(MethodInfo callback){
            string eventTypeName = callback.GetParameters()[0].ParameterType.Name;
            return $"{callback.DeclaringType.FullName}_{callback.Name}_{eventTypeName}";
        }

        private static Type CreateWrapper(MethodInfo callback){
            if(cache.TryGetValue(callback, out Type cachedType)){
                return cachedType;
            }
            
            bool isStatic = callback.IsStatic;
            string name = GetUniqueName(callback);
            string desc = name.Replace('.',  '/');
            string instType = callback.DeclaringType.FullName.Replace('.',  '/');
            string eventType = callback.GetParameters()[0].ParameterType.FullName.Replace('.',  '/');
            
            //Define a public type as a direct subtype of Object, implementing the IEventListener interface
            TypeBuilder tb = GetDynamicModule().DefineType(name, TypeAttributes.Public, typeof(object), new Type[]{typeof(IEventListener)});
            
            //If the callback is an instance method, we add a field to the wrapper type to hold the instance this method is for (passed in the constructor)
            FieldBuilder instanceField = null;
            if(!callback.IsStatic){
                //Add a new public field to the type, named instance, of type Object
                instanceField = tb.DefineField("instance", typeof(object), FieldAttributes.Public);
            }

            //Add a public constructor to the type. If the function is not static, the constructor takes an object as an argument
            Type[] constructorParams = callback.IsStatic ? Type.EmptyTypes : new Type[]{typeof(object)};
            ConstructorBuilder cb = tb.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, constructorParams);
            ILGenerator ctorIL = cb.GetILGenerator();
            //Push arg 0 (the new instance) onto the call stack, then call the constructor for Object
            ctorIL.Emit(OpCodes.Ldarg_0);
            ctorIL.Emit(OpCodes.Call, typeof(object).GetConstructor(Type.EmptyTypes));
            
            //If this is an instance method, we need to store the object argument in this type's instance field
            if(!callback.IsStatic){
                //Push this wrapper instance onto the call stack
                ctorIL.Emit(OpCodes.Ldarg_0);
                //Push the object which was passed as an argument onto the call stack
                ctorIL.Emit(OpCodes.Ldarg_1);
                //Store the object argument  in the instance field
                ctorIL.Emit(OpCodes.Stfld, instanceField);
            }

            //Return from this type's constructor
            ctorIL.Emit(OpCodes.Ret);


            //Add a public function named invoke
            MethodBuilder mbInvoke = tb.DefineMethod(
                nameof(IEventListener.Invoke), //Name
                MethodAttributes.Public | MethodAttributes.Virtual, //Attributes
                typeof(void), //Return type
                new Type[]{typeof(Event)}); //Parameter types
            ILGenerator invokeIL = mbInvoke.GetILGenerator();
            if(callback.IsStatic){
                //Push the event which was passed into this method onto the call stack
                invokeIL.Emit(OpCodes.Ldarg_1);
                //Call the static callback function, passing in the event
                invokeIL.Emit(OpCodes.Call, callback);
                //Return from the Invoke method
                invokeIL.Emit(OpCodes.Ret);
            }else{ //If the callback is non-static
                //Push the 'this' instance of the wrapper onto the call stack
                invokeIL.Emit(OpCodes.Ldarg_0);
                //Push the instance field from the wrapper instance onto the call stack, replacing the value stored by Ldarg_0
                invokeIL.Emit(OpCodes.Ldfld, instanceField);
                //Cast the value from the instance field to the type that the callback method was declared on. This leaves the cast instance on the call stack
                invokeIL.Emit(OpCodes.Castclass, callback.DeclaringType);
                //Push the event which was passed into this method onto the call stack
                invokeIL.Emit(OpCodes.Ldarg_1);
                //Call the callback function on the instance object, passing in the event
                invokeIL.Emit(OpCodes.Call, callback);
                //Return from the Invoke method
                invokeIL.Emit(OpCodes.Ret);
            }
            //Explicitly mark the constructed invoke method as being an interface implementation
            tb.DefineMethodOverride(mbInvoke, typeof(IEventListener).GetMethod(nameof(IEventListener.Invoke)));
            
            //Build, cache, and return the new type definition
            Type newType = tb.CreateType();
            cache.Add(callback, newType);
            return newType;
        }

        public static IEventListener Create(MethodInfo callback, object target){
            Type type = CreateWrapper(callback);
            if(callback.IsStatic){
                return (IEventListener)Activator.CreateInstance(type);
            }else{
                return (IEventListener)Activator.CreateInstance(type, target);
            }
        }
    }
}
