#define ICALL_TABLE_corlib 1

static int corlib_icall_indexes [] = {
216,
227,
228,
229,
230,
231,
232,
233,
234,
235,
238,
239,
357,
358,
359,
387,
388,
389,
416,
417,
418,
510,
511,
512,
515,
554,
555,
557,
559,
561,
563,
568,
576,
577,
578,
579,
580,
581,
582,
583,
584,
673,
682,
683,
752,
759,
762,
764,
769,
770,
772,
773,
777,
778,
780,
781,
784,
785,
786,
789,
791,
794,
796,
798,
807,
873,
875,
877,
887,
888,
889,
891,
897,
898,
899,
900,
901,
909,
910,
911,
915,
916,
918,
920,
1135,
1313,
1314,
7535,
7536,
7538,
7539,
7540,
7541,
7542,
7544,
7545,
7546,
7547,
7564,
7566,
7571,
7573,
7575,
7577,
7628,
7629,
7631,
7632,
7633,
7634,
7635,
7637,
7639,
8686,
8690,
8692,
8693,
8694,
8695,
9113,
9114,
9115,
9116,
9134,
9135,
9136,
9181,
9262,
9265,
9273,
9274,
9275,
9276,
9277,
9622,
9626,
9627,
9656,
9691,
9698,
9705,
9716,
9719,
9742,
9820,
9822,
9832,
9834,
9835,
9842,
9858,
9878,
9879,
9887,
9889,
9896,
9897,
9900,
9902,
9907,
9913,
9914,
9921,
9923,
9935,
9938,
9939,
9940,
9951,
9961,
9967,
9968,
9969,
9971,
9972,
9989,
9991,
10006,
10024,
10051,
10081,
10082,
10571,
10657,
10658,
10870,
10871,
10878,
10879,
10880,
10885,
10940,
11433,
11434,
11819,
11821,
11822,
11828,
11838,
12812,
12833,
12835,
12837,
};
void ves_icall_System_Array_InternalCreate (int,int,int,int,int);
int ves_icall_System_Array_GetCorElementTypeOfElementTypeInternal (int);
int ves_icall_System_Array_IsValueOfElementTypeInternal (int,int);
int ves_icall_System_Array_CanChangePrimitive (int,int,int);
int ves_icall_System_Array_FastCopy (int,int,int,int,int);
int ves_icall_System_Array_GetLengthInternal_raw (int,int,int);
int ves_icall_System_Array_GetLowerBoundInternal_raw (int,int,int);
void ves_icall_System_Array_GetGenericValue_icall (int,int,int);
void ves_icall_System_Array_GetValueImpl_raw (int,int,int,int);
void ves_icall_System_Array_SetGenericValue_icall (int,int,int);
void ves_icall_System_Array_SetValueImpl_raw (int,int,int,int);
void ves_icall_System_Array_SetValueRelaxedImpl_raw (int,int,int,int);
void ves_icall_System_Runtime_RuntimeImports_ZeroMemory (int,int);
void ves_icall_System_Runtime_RuntimeImports_Memmove (int,int,int);
void ves_icall_System_Buffer_BulkMoveWithWriteBarrier (int,int,int,int);
int ves_icall_System_Delegate_AllocDelegateLike_internal_raw (int,int);
int ves_icall_System_Delegate_CreateDelegate_internal_raw (int,int,int,int,int);
int ves_icall_System_Delegate_GetVirtualMethod_internal_raw (int,int);
void ves_icall_System_Enum_GetEnumValuesAndNames_raw (int,int,int,int);
int ves_icall_System_Enum_InternalGetCorElementType (int);
void ves_icall_System_Enum_InternalGetUnderlyingType_raw (int,int,int);
int ves_icall_System_Environment_get_ProcessorCount ();
int ves_icall_System_Environment_get_TickCount ();
int64_t ves_icall_System_Environment_get_TickCount64 ();
void ves_icall_System_Environment_FailFast_raw (int,int,int,int);
void ves_icall_System_GC_register_ephemeron_array_raw (int,int);
int ves_icall_System_GC_get_ephemeron_tombstone_raw (int);
void ves_icall_System_GC_SuppressFinalize_raw (int,int);
void ves_icall_System_GC_ReRegisterForFinalize_raw (int,int);
void ves_icall_System_GC_GetGCMemoryInfo (int,int,int,int,int,int);
int ves_icall_System_GC_AllocPinnedArray_raw (int,int,int);
int ves_icall_System_Object_MemberwiseClone_raw (int,int);
double ves_icall_System_Math_Ceiling (double);
double ves_icall_System_Math_Cos (double);
double ves_icall_System_Math_Floor (double);
double ves_icall_System_Math_Log (double);
double ves_icall_System_Math_Pow (double,double);
double ves_icall_System_Math_Sin (double);
double ves_icall_System_Math_Sqrt (double);
double ves_icall_System_Math_Tan (double);
double ves_icall_System_Math_ModF (double,int);
int ves_icall_RuntimeMethodHandle_GetFunctionPointer_raw (int,int);
void ves_icall_RuntimeMethodHandle_ReboxFromNullable_raw (int,int,int);
void ves_icall_RuntimeMethodHandle_ReboxToNullable_raw (int,int,int,int);
int ves_icall_RuntimeType_GetCorrespondingInflatedMethod_raw (int,int,int);
void ves_icall_RuntimeType_make_array_type_raw (int,int,int,int);
void ves_icall_RuntimeType_make_byref_type_raw (int,int,int);
void ves_icall_RuntimeType_make_pointer_type_raw (int,int,int);
void ves_icall_RuntimeType_MakeGenericType_raw (int,int,int,int);
int ves_icall_RuntimeType_GetMethodsByName_native_raw (int,int,int,int,int);
int ves_icall_RuntimeType_GetPropertiesByName_native_raw (int,int,int,int,int);
int ves_icall_RuntimeType_GetConstructors_native_raw (int,int,int);
int ves_icall_System_RuntimeType_CreateInstanceInternal_raw (int,int);
void ves_icall_RuntimeType_GetDeclaringMethod_raw (int,int,int);
void ves_icall_System_RuntimeType_getFullName_raw (int,int,int,int,int);
void ves_icall_RuntimeType_GetGenericArgumentsInternal_raw (int,int,int,int);
int ves_icall_RuntimeType_GetGenericParameterPosition (int);
int ves_icall_RuntimeType_GetEvents_native_raw (int,int,int,int);
int ves_icall_RuntimeType_GetFields_native_raw (int,int,int,int,int);
void ves_icall_RuntimeType_GetInterfaces_raw (int,int,int);
int ves_icall_RuntimeType_GetNestedTypes_native_raw (int,int,int,int,int);
void ves_icall_RuntimeType_GetDeclaringType_raw (int,int,int);
void ves_icall_RuntimeType_GetName_raw (int,int,int);
void ves_icall_RuntimeType_GetNamespace_raw (int,int,int);
int ves_icall_RuntimeType_FunctionPointerReturnAndParameterTypes_raw (int,int);
int ves_icall_RuntimeTypeHandle_GetAttributes (int);
int ves_icall_RuntimeTypeHandle_GetMetadataToken_raw (int,int);
void ves_icall_RuntimeTypeHandle_GetGenericTypeDefinition_impl_raw (int,int,int);
int ves_icall_RuntimeTypeHandle_GetCorElementType (int);
int ves_icall_RuntimeTypeHandle_HasInstantiation (int);
int ves_icall_RuntimeTypeHandle_IsInstanceOfType_raw (int,int,int);
int ves_icall_RuntimeTypeHandle_HasReferences_raw (int,int);
int ves_icall_RuntimeTypeHandle_GetArrayRank_raw (int,int);
void ves_icall_RuntimeTypeHandle_GetAssembly_raw (int,int,int);
void ves_icall_RuntimeTypeHandle_GetElementType_raw (int,int,int);
void ves_icall_RuntimeTypeHandle_GetModule_raw (int,int,int);
void ves_icall_RuntimeTypeHandle_GetBaseType_raw (int,int,int);
int ves_icall_RuntimeTypeHandle_type_is_assignable_from_raw (int,int,int);
int ves_icall_RuntimeTypeHandle_IsGenericTypeDefinition (int);
int ves_icall_RuntimeTypeHandle_GetGenericParameterInfo_raw (int,int);
int ves_icall_RuntimeTypeHandle_is_subclass_of_raw (int,int,int);
int ves_icall_RuntimeTypeHandle_IsByRefLike_raw (int,int);
void ves_icall_System_RuntimeTypeHandle_internal_from_name_raw (int,int,int,int,int,int);
int ves_icall_System_String_FastAllocateString_raw (int,int);
int ves_icall_System_Type_internal_from_handle_raw (int,int);
int ves_icall_System_ValueType_InternalGetHashCode_raw (int,int,int);
int ves_icall_System_ValueType_Equals_raw (int,int,int,int);
int ves_icall_System_Threading_Interlocked_CompareExchange_Int (int,int,int);
void ves_icall_System_Threading_Interlocked_CompareExchange_Object (int,int,int,int);
int ves_icall_System_Threading_Interlocked_Decrement_Int (int);
int ves_icall_System_Threading_Interlocked_Increment_Int (int);
int64_t ves_icall_System_Threading_Interlocked_Increment_Long (int);
int ves_icall_System_Threading_Interlocked_Exchange_Int (int,int);
void ves_icall_System_Threading_Interlocked_Exchange_Object (int,int,int);
int64_t ves_icall_System_Threading_Interlocked_CompareExchange_Long (int,int64_t,int64_t);
int64_t ves_icall_System_Threading_Interlocked_Exchange_Long (int,int64_t);
int ves_icall_System_Threading_Interlocked_Add_Int (int,int);
int64_t ves_icall_System_Threading_Interlocked_Add_Long (int,int64_t);
void ves_icall_System_Threading_Monitor_Monitor_Enter_raw (int,int);
void mono_monitor_exit_icall_raw (int,int);
void ves_icall_System_Threading_Monitor_Monitor_pulse_raw (int,int);
void ves_icall_System_Threading_Monitor_Monitor_pulse_all_raw (int,int);
int ves_icall_System_Threading_Monitor_Monitor_wait_raw (int,int,int,int);
void ves_icall_System_Threading_Monitor_Monitor_try_enter_with_atomic_var_raw (int,int,int,int,int);
void ves_icall_System_Threading_Thread_InitInternal_raw (int,int);
int ves_icall_System_Threading_Thread_GetCurrentThread ();
void ves_icall_System_Threading_InternalThread_Thread_free_internal_raw (int,int);
int ves_icall_System_Threading_Thread_GetState_raw (int,int);
void ves_icall_System_Threading_Thread_SetState_raw (int,int,int);
void ves_icall_System_Threading_Thread_ClrState_raw (int,int,int);
void ves_icall_System_Threading_Thread_SetName_icall_raw (int,int,int,int);
int ves_icall_System_Threading_Thread_YieldInternal ();
void ves_icall_System_Threading_Thread_SetPriority_raw (int,int,int);
void ves_icall_System_Runtime_Loader_AssemblyLoadContext_PrepareForAssemblyLoadContextRelease_raw (int,int,int);
int ves_icall_System_Runtime_Loader_AssemblyLoadContext_GetLoadContextForAssembly_raw (int,int);
int ves_icall_System_Runtime_Loader_AssemblyLoadContext_InternalLoadFile_raw (int,int,int,int);
int ves_icall_System_Runtime_Loader_AssemblyLoadContext_InternalInitializeNativeALC_raw (int,int,int,int,int);
int ves_icall_System_Runtime_Loader_AssemblyLoadContext_InternalLoadFromStream_raw (int,int,int,int,int,int);
int ves_icall_System_Runtime_Loader_AssemblyLoadContext_InternalGetLoadedAssemblies_raw (int);
int ves_icall_System_GCHandle_InternalAlloc_raw (int,int,int);
void ves_icall_System_GCHandle_InternalFree_raw (int,int);
int ves_icall_System_GCHandle_InternalGet_raw (int,int);
void ves_icall_System_GCHandle_InternalSet_raw (int,int,int);
int ves_icall_System_Runtime_InteropServices_Marshal_GetLastPInvokeError ();
void ves_icall_System_Runtime_InteropServices_Marshal_SetLastPInvokeError (int);
void ves_icall_System_Runtime_InteropServices_Marshal_StructureToPtr_raw (int,int,int,int);
int ves_icall_System_Runtime_InteropServices_NativeLibrary_LoadByName_raw (int,int,int,int,int,int);
int ves_icall_System_Runtime_CompilerServices_RuntimeHelpers_InternalGetHashCode_raw (int,int);
int ves_icall_System_Runtime_CompilerServices_RuntimeHelpers_GetObjectValue_raw (int,int);
int ves_icall_System_Runtime_CompilerServices_RuntimeHelpers_GetUninitializedObjectInternal_raw (int,int);
void ves_icall_System_Runtime_CompilerServices_RuntimeHelpers_InitializeArray_raw (int,int,int);
int ves_icall_System_Runtime_CompilerServices_RuntimeHelpers_GetSpanDataFrom_raw (int,int,int,int);
int ves_icall_System_Runtime_CompilerServices_RuntimeHelpers_SufficientExecutionStack ();
int ves_icall_System_Runtime_CompilerServices_RuntimeHelpers_InternalBox_raw (int,int,int);
int ves_icall_System_Reflection_Assembly_GetEntryAssembly_raw (int);
int ves_icall_System_Reflection_Assembly_InternalLoad_raw (int,int,int,int);
int ves_icall_System_Reflection_Assembly_InternalGetType_raw (int,int,int,int,int,int);
int ves_icall_System_Reflection_AssemblyName_GetNativeName (int);
int ves_icall_MonoCustomAttrs_GetCustomAttributesInternal_raw (int,int,int,int);
int ves_icall_MonoCustomAttrs_GetCustomAttributesDataInternal_raw (int,int);
int ves_icall_MonoCustomAttrs_IsDefinedInternal_raw (int,int,int);
int ves_icall_System_Reflection_FieldInfo_internal_from_handle_type_raw (int,int,int);
int ves_icall_System_Reflection_FieldInfo_get_marshal_info_raw (int,int);
int ves_icall_System_Reflection_LoaderAllocatorScout_Destroy (int);
void ves_icall_System_Reflection_RuntimeAssembly_GetManifestResourceNames_raw (int,int,int);
void ves_icall_System_Reflection_RuntimeAssembly_GetExportedTypes_raw (int,int,int);
void ves_icall_System_Reflection_RuntimeAssembly_GetInfo_raw (int,int,int,int);
int ves_icall_System_Reflection_RuntimeAssembly_GetManifestResourceInternal_raw (int,int,int,int,int);
void ves_icall_System_Reflection_Assembly_GetManifestModuleInternal_raw (int,int,int);
void ves_icall_System_Reflection_RuntimeCustomAttributeData_ResolveArgumentsInternal_raw (int,int,int,int,int,int,int);
void ves_icall_RuntimeEventInfo_get_event_info_raw (int,int,int);
int ves_icall_reflection_get_token_raw (int,int);
int ves_icall_System_Reflection_EventInfo_internal_from_handle_type_raw (int,int,int);
int ves_icall_RuntimeFieldInfo_ResolveType_raw (int,int);
int ves_icall_RuntimeFieldInfo_GetParentType_raw (int,int,int);
int ves_icall_RuntimeFieldInfo_GetFieldOffset_raw (int,int);
int ves_icall_RuntimeFieldInfo_GetValueInternal_raw (int,int,int);
void ves_icall_RuntimeFieldInfo_SetValueInternal_raw (int,int,int,int);
int ves_icall_RuntimeFieldInfo_GetRawConstantValue_raw (int,int);
int ves_icall_reflection_get_token_raw (int,int);
void ves_icall_get_method_info_raw (int,int,int);
int ves_icall_get_method_attributes (int);
int ves_icall_System_Reflection_MonoMethodInfo_get_parameter_info_raw (int,int,int);
int ves_icall_System_MonoMethodInfo_get_retval_marshal_raw (int,int);
int ves_icall_System_Reflection_RuntimeMethodInfo_GetMethodFromHandleInternalType_native_raw (int,int,int,int);
int ves_icall_RuntimeMethodInfo_get_name_raw (int,int);
int ves_icall_RuntimeMethodInfo_get_base_method_raw (int,int,int);
int ves_icall_reflection_get_token_raw (int,int);
int ves_icall_InternalInvoke_raw (int,int,int,int,int);
void ves_icall_RuntimeMethodInfo_GetPInvoke_raw (int,int,int,int,int);
int ves_icall_RuntimeMethodInfo_MakeGenericMethod_impl_raw (int,int,int);
int ves_icall_RuntimeMethodInfo_GetGenericArguments_raw (int,int);
int ves_icall_RuntimeMethodInfo_GetGenericMethodDefinition_raw (int,int);
int ves_icall_RuntimeMethodInfo_get_IsGenericMethodDefinition_raw (int,int);
int ves_icall_RuntimeMethodInfo_get_IsGenericMethod_raw (int,int);
void ves_icall_InvokeClassConstructor_raw (int,int);
int ves_icall_InternalInvoke_raw (int,int,int,int,int);
int ves_icall_reflection_get_token_raw (int,int);
int ves_icall_System_Reflection_RuntimeModule_ResolveMethodToken_raw (int,int,int,int,int,int);
void ves_icall_RuntimePropertyInfo_get_property_info_raw (int,int,int,int);
int ves_icall_reflection_get_token_raw (int,int);
int ves_icall_System_Reflection_RuntimePropertyInfo_internal_from_handle_type_raw (int,int,int);
void ves_icall_DynamicMethod_create_dynamic_method_raw (int,int,int,int,int);
void ves_icall_AssemblyBuilder_basic_init_raw (int,int);
void ves_icall_AssemblyBuilder_UpdateNativeCustomAttributes_raw (int,int);
void ves_icall_ModuleBuilder_basic_init_raw (int,int);
void ves_icall_ModuleBuilder_set_wrappers_type_raw (int,int,int);
int ves_icall_ModuleBuilder_getUSIndex_raw (int,int,int);
int ves_icall_ModuleBuilder_getToken_raw (int,int,int,int);
int ves_icall_ModuleBuilder_getMethodToken_raw (int,int,int,int);
void ves_icall_ModuleBuilder_RegisterToken_raw (int,int,int,int);
int ves_icall_TypeBuilder_create_runtime_class_raw (int,int);
int ves_icall_System_IO_Stream_HasOverriddenBeginEndRead_raw (int,int);
int ves_icall_System_IO_Stream_HasOverriddenBeginEndWrite_raw (int,int);
int ves_icall_System_Diagnostics_Debugger_IsAttached_internal ();
int ves_icall_System_Diagnostics_Debugger_IsLogging ();
void ves_icall_System_Diagnostics_Debugger_Log (int,int,int);
int ves_icall_System_Diagnostics_StackFrame_GetFrameInfo (int,int,int,int,int,int,int,int);
void ves_icall_System_Diagnostics_StackTrace_GetTrace (int,int,int,int);
int ves_icall_Mono_RuntimeClassHandle_GetTypeFromClass (int);
void ves_icall_Mono_RuntimeGPtrArrayHandle_GPtrArrayFree (int);
int ves_icall_Mono_SafeStringMarshal_StringToUtf8 (int);
void ves_icall_Mono_SafeStringMarshal_GFree (int);
static void *corlib_icall_funcs [] = {
// token 216,
ves_icall_System_Array_InternalCreate,
// token 227,
ves_icall_System_Array_GetCorElementTypeOfElementTypeInternal,
// token 228,
ves_icall_System_Array_IsValueOfElementTypeInternal,
// token 229,
ves_icall_System_Array_CanChangePrimitive,
// token 230,
ves_icall_System_Array_FastCopy,
// token 231,
ves_icall_System_Array_GetLengthInternal_raw,
// token 232,
ves_icall_System_Array_GetLowerBoundInternal_raw,
// token 233,
ves_icall_System_Array_GetGenericValue_icall,
// token 234,
ves_icall_System_Array_GetValueImpl_raw,
// token 235,
ves_icall_System_Array_SetGenericValue_icall,
// token 238,
ves_icall_System_Array_SetValueImpl_raw,
// token 239,
ves_icall_System_Array_SetValueRelaxedImpl_raw,
// token 357,
ves_icall_System_Runtime_RuntimeImports_ZeroMemory,
// token 358,
ves_icall_System_Runtime_RuntimeImports_Memmove,
// token 359,
ves_icall_System_Buffer_BulkMoveWithWriteBarrier,
// token 387,
ves_icall_System_Delegate_AllocDelegateLike_internal_raw,
// token 388,
ves_icall_System_Delegate_CreateDelegate_internal_raw,
// token 389,
ves_icall_System_Delegate_GetVirtualMethod_internal_raw,
// token 416,
ves_icall_System_Enum_GetEnumValuesAndNames_raw,
// token 417,
ves_icall_System_Enum_InternalGetCorElementType,
// token 418,
ves_icall_System_Enum_InternalGetUnderlyingType_raw,
// token 510,
ves_icall_System_Environment_get_ProcessorCount,
// token 511,
ves_icall_System_Environment_get_TickCount,
// token 512,
ves_icall_System_Environment_get_TickCount64,
// token 515,
ves_icall_System_Environment_FailFast_raw,
// token 554,
ves_icall_System_GC_register_ephemeron_array_raw,
// token 555,
ves_icall_System_GC_get_ephemeron_tombstone_raw,
// token 557,
ves_icall_System_GC_SuppressFinalize_raw,
// token 559,
ves_icall_System_GC_ReRegisterForFinalize_raw,
// token 561,
ves_icall_System_GC_GetGCMemoryInfo,
// token 563,
ves_icall_System_GC_AllocPinnedArray_raw,
// token 568,
ves_icall_System_Object_MemberwiseClone_raw,
// token 576,
ves_icall_System_Math_Ceiling,
// token 577,
ves_icall_System_Math_Cos,
// token 578,
ves_icall_System_Math_Floor,
// token 579,
ves_icall_System_Math_Log,
// token 580,
ves_icall_System_Math_Pow,
// token 581,
ves_icall_System_Math_Sin,
// token 582,
ves_icall_System_Math_Sqrt,
// token 583,
ves_icall_System_Math_Tan,
// token 584,
ves_icall_System_Math_ModF,
// token 673,
ves_icall_RuntimeMethodHandle_GetFunctionPointer_raw,
// token 682,
ves_icall_RuntimeMethodHandle_ReboxFromNullable_raw,
// token 683,
ves_icall_RuntimeMethodHandle_ReboxToNullable_raw,
// token 752,
ves_icall_RuntimeType_GetCorrespondingInflatedMethod_raw,
// token 759,
ves_icall_RuntimeType_make_array_type_raw,
// token 762,
ves_icall_RuntimeType_make_byref_type_raw,
// token 764,
ves_icall_RuntimeType_make_pointer_type_raw,
// token 769,
ves_icall_RuntimeType_MakeGenericType_raw,
// token 770,
ves_icall_RuntimeType_GetMethodsByName_native_raw,
// token 772,
ves_icall_RuntimeType_GetPropertiesByName_native_raw,
// token 773,
ves_icall_RuntimeType_GetConstructors_native_raw,
// token 777,
ves_icall_System_RuntimeType_CreateInstanceInternal_raw,
// token 778,
ves_icall_RuntimeType_GetDeclaringMethod_raw,
// token 780,
ves_icall_System_RuntimeType_getFullName_raw,
// token 781,
ves_icall_RuntimeType_GetGenericArgumentsInternal_raw,
// token 784,
ves_icall_RuntimeType_GetGenericParameterPosition,
// token 785,
ves_icall_RuntimeType_GetEvents_native_raw,
// token 786,
ves_icall_RuntimeType_GetFields_native_raw,
// token 789,
ves_icall_RuntimeType_GetInterfaces_raw,
// token 791,
ves_icall_RuntimeType_GetNestedTypes_native_raw,
// token 794,
ves_icall_RuntimeType_GetDeclaringType_raw,
// token 796,
ves_icall_RuntimeType_GetName_raw,
// token 798,
ves_icall_RuntimeType_GetNamespace_raw,
// token 807,
ves_icall_RuntimeType_FunctionPointerReturnAndParameterTypes_raw,
// token 873,
ves_icall_RuntimeTypeHandle_GetAttributes,
// token 875,
ves_icall_RuntimeTypeHandle_GetMetadataToken_raw,
// token 877,
ves_icall_RuntimeTypeHandle_GetGenericTypeDefinition_impl_raw,
// token 887,
ves_icall_RuntimeTypeHandle_GetCorElementType,
// token 888,
ves_icall_RuntimeTypeHandle_HasInstantiation,
// token 889,
ves_icall_RuntimeTypeHandle_IsInstanceOfType_raw,
// token 891,
ves_icall_RuntimeTypeHandle_HasReferences_raw,
// token 897,
ves_icall_RuntimeTypeHandle_GetArrayRank_raw,
// token 898,
ves_icall_RuntimeTypeHandle_GetAssembly_raw,
// token 899,
ves_icall_RuntimeTypeHandle_GetElementType_raw,
// token 900,
ves_icall_RuntimeTypeHandle_GetModule_raw,
// token 901,
ves_icall_RuntimeTypeHandle_GetBaseType_raw,
// token 909,
ves_icall_RuntimeTypeHandle_type_is_assignable_from_raw,
// token 910,
ves_icall_RuntimeTypeHandle_IsGenericTypeDefinition,
// token 911,
ves_icall_RuntimeTypeHandle_GetGenericParameterInfo_raw,
// token 915,
ves_icall_RuntimeTypeHandle_is_subclass_of_raw,
// token 916,
ves_icall_RuntimeTypeHandle_IsByRefLike_raw,
// token 918,
ves_icall_System_RuntimeTypeHandle_internal_from_name_raw,
// token 920,
ves_icall_System_String_FastAllocateString_raw,
// token 1135,
ves_icall_System_Type_internal_from_handle_raw,
// token 1313,
ves_icall_System_ValueType_InternalGetHashCode_raw,
// token 1314,
ves_icall_System_ValueType_Equals_raw,
// token 7535,
ves_icall_System_Threading_Interlocked_CompareExchange_Int,
// token 7536,
ves_icall_System_Threading_Interlocked_CompareExchange_Object,
// token 7538,
ves_icall_System_Threading_Interlocked_Decrement_Int,
// token 7539,
ves_icall_System_Threading_Interlocked_Increment_Int,
// token 7540,
ves_icall_System_Threading_Interlocked_Increment_Long,
// token 7541,
ves_icall_System_Threading_Interlocked_Exchange_Int,
// token 7542,
ves_icall_System_Threading_Interlocked_Exchange_Object,
// token 7544,
ves_icall_System_Threading_Interlocked_CompareExchange_Long,
// token 7545,
ves_icall_System_Threading_Interlocked_Exchange_Long,
// token 7546,
ves_icall_System_Threading_Interlocked_Add_Int,
// token 7547,
ves_icall_System_Threading_Interlocked_Add_Long,
// token 7564,
ves_icall_System_Threading_Monitor_Monitor_Enter_raw,
// token 7566,
mono_monitor_exit_icall_raw,
// token 7571,
ves_icall_System_Threading_Monitor_Monitor_pulse_raw,
// token 7573,
ves_icall_System_Threading_Monitor_Monitor_pulse_all_raw,
// token 7575,
ves_icall_System_Threading_Monitor_Monitor_wait_raw,
// token 7577,
ves_icall_System_Threading_Monitor_Monitor_try_enter_with_atomic_var_raw,
// token 7628,
ves_icall_System_Threading_Thread_InitInternal_raw,
// token 7629,
ves_icall_System_Threading_Thread_GetCurrentThread,
// token 7631,
ves_icall_System_Threading_InternalThread_Thread_free_internal_raw,
// token 7632,
ves_icall_System_Threading_Thread_GetState_raw,
// token 7633,
ves_icall_System_Threading_Thread_SetState_raw,
// token 7634,
ves_icall_System_Threading_Thread_ClrState_raw,
// token 7635,
ves_icall_System_Threading_Thread_SetName_icall_raw,
// token 7637,
ves_icall_System_Threading_Thread_YieldInternal,
// token 7639,
ves_icall_System_Threading_Thread_SetPriority_raw,
// token 8686,
ves_icall_System_Runtime_Loader_AssemblyLoadContext_PrepareForAssemblyLoadContextRelease_raw,
// token 8690,
ves_icall_System_Runtime_Loader_AssemblyLoadContext_GetLoadContextForAssembly_raw,
// token 8692,
ves_icall_System_Runtime_Loader_AssemblyLoadContext_InternalLoadFile_raw,
// token 8693,
ves_icall_System_Runtime_Loader_AssemblyLoadContext_InternalInitializeNativeALC_raw,
// token 8694,
ves_icall_System_Runtime_Loader_AssemblyLoadContext_InternalLoadFromStream_raw,
// token 8695,
ves_icall_System_Runtime_Loader_AssemblyLoadContext_InternalGetLoadedAssemblies_raw,
// token 9113,
ves_icall_System_GCHandle_InternalAlloc_raw,
// token 9114,
ves_icall_System_GCHandle_InternalFree_raw,
// token 9115,
ves_icall_System_GCHandle_InternalGet_raw,
// token 9116,
ves_icall_System_GCHandle_InternalSet_raw,
// token 9134,
ves_icall_System_Runtime_InteropServices_Marshal_GetLastPInvokeError,
// token 9135,
ves_icall_System_Runtime_InteropServices_Marshal_SetLastPInvokeError,
// token 9136,
ves_icall_System_Runtime_InteropServices_Marshal_StructureToPtr_raw,
// token 9181,
ves_icall_System_Runtime_InteropServices_NativeLibrary_LoadByName_raw,
// token 9262,
ves_icall_System_Runtime_CompilerServices_RuntimeHelpers_InternalGetHashCode_raw,
// token 9265,
ves_icall_System_Runtime_CompilerServices_RuntimeHelpers_GetObjectValue_raw,
// token 9273,
ves_icall_System_Runtime_CompilerServices_RuntimeHelpers_GetUninitializedObjectInternal_raw,
// token 9274,
ves_icall_System_Runtime_CompilerServices_RuntimeHelpers_InitializeArray_raw,
// token 9275,
ves_icall_System_Runtime_CompilerServices_RuntimeHelpers_GetSpanDataFrom_raw,
// token 9276,
ves_icall_System_Runtime_CompilerServices_RuntimeHelpers_SufficientExecutionStack,
// token 9277,
ves_icall_System_Runtime_CompilerServices_RuntimeHelpers_InternalBox_raw,
// token 9622,
ves_icall_System_Reflection_Assembly_GetEntryAssembly_raw,
// token 9626,
ves_icall_System_Reflection_Assembly_InternalLoad_raw,
// token 9627,
ves_icall_System_Reflection_Assembly_InternalGetType_raw,
// token 9656,
ves_icall_System_Reflection_AssemblyName_GetNativeName,
// token 9691,
ves_icall_MonoCustomAttrs_GetCustomAttributesInternal_raw,
// token 9698,
ves_icall_MonoCustomAttrs_GetCustomAttributesDataInternal_raw,
// token 9705,
ves_icall_MonoCustomAttrs_IsDefinedInternal_raw,
// token 9716,
ves_icall_System_Reflection_FieldInfo_internal_from_handle_type_raw,
// token 9719,
ves_icall_System_Reflection_FieldInfo_get_marshal_info_raw,
// token 9742,
ves_icall_System_Reflection_LoaderAllocatorScout_Destroy,
// token 9820,
ves_icall_System_Reflection_RuntimeAssembly_GetManifestResourceNames_raw,
// token 9822,
ves_icall_System_Reflection_RuntimeAssembly_GetExportedTypes_raw,
// token 9832,
ves_icall_System_Reflection_RuntimeAssembly_GetInfo_raw,
// token 9834,
ves_icall_System_Reflection_RuntimeAssembly_GetManifestResourceInternal_raw,
// token 9835,
ves_icall_System_Reflection_Assembly_GetManifestModuleInternal_raw,
// token 9842,
ves_icall_System_Reflection_RuntimeCustomAttributeData_ResolveArgumentsInternal_raw,
// token 9858,
ves_icall_RuntimeEventInfo_get_event_info_raw,
// token 9878,
ves_icall_reflection_get_token_raw,
// token 9879,
ves_icall_System_Reflection_EventInfo_internal_from_handle_type_raw,
// token 9887,
ves_icall_RuntimeFieldInfo_ResolveType_raw,
// token 9889,
ves_icall_RuntimeFieldInfo_GetParentType_raw,
// token 9896,
ves_icall_RuntimeFieldInfo_GetFieldOffset_raw,
// token 9897,
ves_icall_RuntimeFieldInfo_GetValueInternal_raw,
// token 9900,
ves_icall_RuntimeFieldInfo_SetValueInternal_raw,
// token 9902,
ves_icall_RuntimeFieldInfo_GetRawConstantValue_raw,
// token 9907,
ves_icall_reflection_get_token_raw,
// token 9913,
ves_icall_get_method_info_raw,
// token 9914,
ves_icall_get_method_attributes,
// token 9921,
ves_icall_System_Reflection_MonoMethodInfo_get_parameter_info_raw,
// token 9923,
ves_icall_System_MonoMethodInfo_get_retval_marshal_raw,
// token 9935,
ves_icall_System_Reflection_RuntimeMethodInfo_GetMethodFromHandleInternalType_native_raw,
// token 9938,
ves_icall_RuntimeMethodInfo_get_name_raw,
// token 9939,
ves_icall_RuntimeMethodInfo_get_base_method_raw,
// token 9940,
ves_icall_reflection_get_token_raw,
// token 9951,
ves_icall_InternalInvoke_raw,
// token 9961,
ves_icall_RuntimeMethodInfo_GetPInvoke_raw,
// token 9967,
ves_icall_RuntimeMethodInfo_MakeGenericMethod_impl_raw,
// token 9968,
ves_icall_RuntimeMethodInfo_GetGenericArguments_raw,
// token 9969,
ves_icall_RuntimeMethodInfo_GetGenericMethodDefinition_raw,
// token 9971,
ves_icall_RuntimeMethodInfo_get_IsGenericMethodDefinition_raw,
// token 9972,
ves_icall_RuntimeMethodInfo_get_IsGenericMethod_raw,
// token 9989,
ves_icall_InvokeClassConstructor_raw,
// token 9991,
ves_icall_InternalInvoke_raw,
// token 10006,
ves_icall_reflection_get_token_raw,
// token 10024,
ves_icall_System_Reflection_RuntimeModule_ResolveMethodToken_raw,
// token 10051,
ves_icall_RuntimePropertyInfo_get_property_info_raw,
// token 10081,
ves_icall_reflection_get_token_raw,
// token 10082,
ves_icall_System_Reflection_RuntimePropertyInfo_internal_from_handle_type_raw,
// token 10571,
ves_icall_DynamicMethod_create_dynamic_method_raw,
// token 10657,
ves_icall_AssemblyBuilder_basic_init_raw,
// token 10658,
ves_icall_AssemblyBuilder_UpdateNativeCustomAttributes_raw,
// token 10870,
ves_icall_ModuleBuilder_basic_init_raw,
// token 10871,
ves_icall_ModuleBuilder_set_wrappers_type_raw,
// token 10878,
ves_icall_ModuleBuilder_getUSIndex_raw,
// token 10879,
ves_icall_ModuleBuilder_getToken_raw,
// token 10880,
ves_icall_ModuleBuilder_getMethodToken_raw,
// token 10885,
ves_icall_ModuleBuilder_RegisterToken_raw,
// token 10940,
ves_icall_TypeBuilder_create_runtime_class_raw,
// token 11433,
ves_icall_System_IO_Stream_HasOverriddenBeginEndRead_raw,
// token 11434,
ves_icall_System_IO_Stream_HasOverriddenBeginEndWrite_raw,
// token 11819,
ves_icall_System_Diagnostics_Debugger_IsAttached_internal,
// token 11821,
ves_icall_System_Diagnostics_Debugger_IsLogging,
// token 11822,
ves_icall_System_Diagnostics_Debugger_Log,
// token 11828,
ves_icall_System_Diagnostics_StackFrame_GetFrameInfo,
// token 11838,
ves_icall_System_Diagnostics_StackTrace_GetTrace,
// token 12812,
ves_icall_Mono_RuntimeClassHandle_GetTypeFromClass,
// token 12833,
ves_icall_Mono_RuntimeGPtrArrayHandle_GPtrArrayFree,
// token 12835,
ves_icall_Mono_SafeStringMarshal_StringToUtf8,
// token 12837,
ves_icall_Mono_SafeStringMarshal_GFree,
};
static uint8_t corlib_icall_flags [] = {
0,
0,
0,
0,
0,
4,
4,
0,
4,
0,
4,
4,
0,
0,
0,
4,
4,
4,
4,
0,
4,
0,
0,
0,
4,
4,
4,
4,
4,
0,
4,
4,
0,
0,
0,
0,
0,
0,
0,
0,
0,
4,
4,
4,
4,
4,
4,
4,
4,
4,
4,
4,
4,
4,
4,
4,
0,
4,
4,
4,
4,
4,
4,
4,
4,
0,
4,
4,
0,
0,
4,
4,
4,
4,
4,
4,
4,
4,
0,
4,
4,
4,
4,
4,
4,
4,
4,
0,
0,
0,
0,
0,
0,
0,
0,
0,
0,
0,
4,
4,
4,
4,
4,
4,
4,
0,
4,
4,
4,
4,
4,
0,
4,
4,
4,
4,
4,
4,
4,
4,
4,
4,
4,
0,
0,
4,
4,
4,
4,
4,
4,
4,
0,
4,
4,
4,
4,
0,
4,
4,
4,
4,
4,
0,
4,
4,
4,
4,
4,
4,
4,
4,
4,
4,
4,
4,
4,
4,
4,
4,
4,
0,
4,
4,
4,
4,
4,
4,
4,
4,
4,
4,
4,
4,
4,
4,
4,
4,
4,
4,
4,
4,
4,
4,
4,
4,
4,
4,
4,
4,
4,
4,
4,
4,
0,
0,
0,
0,
0,
0,
0,
0,
0,
};
