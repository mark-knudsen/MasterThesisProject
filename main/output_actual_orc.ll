; ModuleID = 'repl_module'
source_filename = "repl_module"

%dataframe = type { ptr, ptr, ptr }
%RuntimeValue = type { i64, ptr }

@str = private unnamed_addr constant [4 x i8] c"bob\00", align 1
@str.1 = private unnamed_addr constant [5 x i8] c"karl\00", align 1
@str.2 = private unnamed_addr constant [5 x i8] c"Mary\00", align 1
@str.3 = private unnamed_addr constant [5 x i8] c"name\00", align 1
@str.4 = private unnamed_addr constant [4 x i8] c"age\00", align 1
@str.5 = private unnamed_addr constant [7 x i8] c"isCool\00", align 1
@str.6 = private unnamed_addr constant [8 x i8] c"savings\00", align 1
@str.7 = private unnamed_addr constant [1 x i8] zeroinitializer, align 1
@df = global ptr null

define ptr @main_0() {
entry:
  %raw_ptr = call ptr @malloc(i64 56)
  %ptr_int = ptrtoint ptr %raw_ptr to i64
  %ptr_offset = add i64 %ptr_int, 31
  %ptr_aligned_int = and i64 %ptr_offset, -32
  %arr_data_aligned = inttoptr i64 %ptr_aligned_int to ptr
  %arr_header = call ptr @malloc(i64 24)
  %0 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header, i32 0, i32 0
  store i64 3, ptr %0, align 4
  %1 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header, i32 0, i32 1
  store i64 3, ptr %1, align 4
  %2 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header, i32 0, i32 2
  store ptr %arr_data_aligned, ptr %2, align 8
  %elem_ptr = getelementptr ptr, ptr %arr_data_aligned, i64 0
  store ptr @str, ptr %elem_ptr, align 32
  %elem_ptr1 = getelementptr ptr, ptr %arr_data_aligned, i64 1
  store ptr @str.1, ptr %elem_ptr1, align 32
  %elem_ptr2 = getelementptr ptr, ptr %arr_data_aligned, i64 2
  store ptr @str.2, ptr %elem_ptr2, align 32
  %raw_ptr3 = call ptr @malloc(i64 56)
  %ptr_int4 = ptrtoint ptr %raw_ptr3 to i64
  %ptr_offset5 = add i64 %ptr_int4, 31
  %ptr_aligned_int6 = and i64 %ptr_offset5, -32
  %arr_data_aligned7 = inttoptr i64 %ptr_aligned_int6 to ptr
  %arr_header8 = call ptr @malloc(i64 24)
  %3 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header8, i32 0, i32 0
  store i64 3, ptr %3, align 4
  %4 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header8, i32 0, i32 1
  store i64 3, ptr %4, align 4
  %5 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header8, i32 0, i32 2
  store ptr %arr_data_aligned7, ptr %5, align 8
  %elem_ptr9 = getelementptr i64, ptr %arr_data_aligned7, i64 0
  store i64 12, ptr %elem_ptr9, align 32
  %elem_ptr10 = getelementptr i64, ptr %arr_data_aligned7, i64 1
  store i64 23, ptr %elem_ptr10, align 32
  %elem_ptr11 = getelementptr i64, ptr %arr_data_aligned7, i64 2
  store i64 44, ptr %elem_ptr11, align 32
  %raw_ptr12 = call ptr @malloc(i64 35)
  %ptr_int13 = ptrtoint ptr %raw_ptr12 to i64
  %ptr_offset14 = add i64 %ptr_int13, 31
  %ptr_aligned_int15 = and i64 %ptr_offset14, -32
  %arr_data_aligned16 = inttoptr i64 %ptr_aligned_int15 to ptr
  %arr_header17 = call ptr @malloc(i64 24)
  %6 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header17, i32 0, i32 0
  store i64 3, ptr %6, align 4
  %7 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header17, i32 0, i32 1
  store i64 3, ptr %7, align 4
  %8 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header17, i32 0, i32 2
  store ptr %arr_data_aligned16, ptr %8, align 8
  %elem_ptr18 = getelementptr i1, ptr %arr_data_aligned16, i64 0
  store i1 true, ptr %elem_ptr18, align 32
  %elem_ptr19 = getelementptr i1, ptr %arr_data_aligned16, i64 1
  store i1 true, ptr %elem_ptr19, align 32
  %elem_ptr20 = getelementptr i1, ptr %arr_data_aligned16, i64 2
  store i1 false, ptr %elem_ptr20, align 32
  %raw_ptr21 = call ptr @malloc(i64 56)
  %ptr_int22 = ptrtoint ptr %raw_ptr21 to i64
  %ptr_offset23 = add i64 %ptr_int22, 31
  %ptr_aligned_int24 = and i64 %ptr_offset23, -32
  %arr_data_aligned25 = inttoptr i64 %ptr_aligned_int24 to ptr
  %arr_header26 = call ptr @malloc(i64 24)
  %9 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header26, i32 0, i32 0
  store i64 3, ptr %9, align 4
  %10 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header26, i32 0, i32 1
  store i64 3, ptr %10, align 4
  %11 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header26, i32 0, i32 2
  store ptr %arr_data_aligned25, ptr %11, align 8
  %elem_ptr27 = getelementptr double, ptr %arr_data_aligned25, i64 0
  store double 1.000000e+00, ptr %elem_ptr27, align 32
  %elem_ptr28 = getelementptr double, ptr %arr_data_aligned25, i64 1
  store double 1.000000e+03, ptr %elem_ptr28, align 32
  %elem_ptr29 = getelementptr double, ptr %arr_data_aligned25, i64 2
  store double 1.000000e+05, ptr %elem_ptr29, align 32
  %data_ptrs_header = call ptr @malloc(i64 24)
  %data_ptrs_buffer = call ptr @malloc(i64 32)
  %12 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_ptrs_header, i32 0, i32 0
  store i64 4, ptr %12, align 4
  %13 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_ptrs_header, i32 0, i32 1
  store i64 4, ptr %13, align 4
  %14 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_ptrs_header, i32 0, i32 2
  store ptr %data_ptrs_buffer, ptr %14, align 8
  %data_gep = getelementptr ptr, ptr %data_ptrs_buffer, i64 0
  store ptr %arr_header, ptr %data_gep, align 8
  %data_gep30 = getelementptr ptr, ptr %data_ptrs_buffer, i64 1
  store ptr %arr_header8, ptr %data_gep30, align 8
  %data_gep31 = getelementptr ptr, ptr %data_ptrs_buffer, i64 2
  store ptr %arr_header17, ptr %data_gep31, align 8
  %data_gep32 = getelementptr ptr, ptr %data_ptrs_buffer, i64 3
  store ptr %arr_header26, ptr %data_gep32, align 8
  %raw_ptr33 = call ptr @malloc(i64 64)
  %ptr_int34 = ptrtoint ptr %raw_ptr33 to i64
  %ptr_offset35 = add i64 %ptr_int34, 31
  %ptr_aligned_int36 = and i64 %ptr_offset35, -32
  %arr_data_aligned37 = inttoptr i64 %ptr_aligned_int36 to ptr
  %arr_header38 = call ptr @malloc(i64 24)
  %15 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header38, i32 0, i32 0
  store i64 4, ptr %15, align 4
  %16 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header38, i32 0, i32 1
  store i64 4, ptr %16, align 4
  %17 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header38, i32 0, i32 2
  store ptr %arr_data_aligned37, ptr %17, align 8
  %elem_ptr39 = getelementptr ptr, ptr %arr_data_aligned37, i64 0
  store ptr @str.3, ptr %elem_ptr39, align 32
  %elem_ptr40 = getelementptr ptr, ptr %arr_data_aligned37, i64 1
  store ptr @str.4, ptr %elem_ptr40, align 32
  %elem_ptr41 = getelementptr ptr, ptr %arr_data_aligned37, i64 2
  store ptr @str.5, ptr %elem_ptr41, align 32
  %elem_ptr42 = getelementptr ptr, ptr %arr_data_aligned37, i64 3
  store ptr @str.6, ptr %elem_ptr42, align 32
  %raw_ptr43 = call ptr @malloc(i64 64)
  %ptr_int44 = ptrtoint ptr %raw_ptr43 to i64
  %ptr_offset45 = add i64 %ptr_int44, 31
  %ptr_aligned_int46 = and i64 %ptr_offset45, -32
  %arr_data_aligned47 = inttoptr i64 %ptr_aligned_int46 to ptr
  %arr_header48 = call ptr @malloc(i64 24)
  %18 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header48, i32 0, i32 0
  store i64 4, ptr %18, align 4
  %19 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header48, i32 0, i32 1
  store i64 4, ptr %19, align 4
  %20 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header48, i32 0, i32 2
  store ptr %arr_data_aligned47, ptr %20, align 8
  %elem_ptr49 = getelementptr ptr, ptr %arr_data_aligned47, i64 0
  store ptr @str.7, ptr %elem_ptr49, align 32
  %elem_ptr50 = getelementptr ptr, ptr %arr_data_aligned47, i64 1
  store i64 0, ptr %elem_ptr50, align 32
  %elem_ptr51 = getelementptr ptr, ptr %arr_data_aligned47, i64 2
  store i1 false, ptr %elem_ptr51, align 32
  %elem_ptr52 = getelementptr ptr, ptr %arr_data_aligned47, i64 3
  store double 0.000000e+00, ptr %elem_ptr52, align 32
  %df_instance = tail call ptr @malloc(i32 ptrtoint (ptr getelementptr (%dataframe, ptr null, i32 1) to i32))
  %21 = getelementptr inbounds nuw %dataframe, ptr %df_instance, i32 0, i32 0
  store ptr %arr_header38, ptr %21, align 8
  %22 = getelementptr inbounds nuw %dataframe, ptr %df_instance, i32 0, i32 1
  store ptr %data_ptrs_header, ptr %22, align 8
  %23 = getelementptr inbounds nuw %dataframe, ptr %df_instance, i32 0, i32 2
  store ptr %arr_header48, ptr %23, align 8
  store ptr %df_instance, ptr @df, align 8
  %runtime_obj = call ptr @malloc(i64 16)
  %runtime_cast = bitcast ptr %runtime_obj to ptr
  %tag_ptr = getelementptr inbounds nuw %RuntimeValue, ptr %runtime_cast, i32 0, i32 0
  store i64 7, ptr %tag_ptr, align 4
  %data_ptr = getelementptr inbounds nuw %RuntimeValue, ptr %runtime_cast, i32 0, i32 1
  store ptr %df_instance, ptr %data_ptr, align 8
  ret ptr %runtime_obj
}

declare i32 @printf(ptr, ...)

declare noalias ptr @malloc(i64)
