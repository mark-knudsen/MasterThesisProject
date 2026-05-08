; ModuleID = 'repl_module'
source_filename = "repl_module"

%dataframe = type { ptr, ptr, ptr }
%RuntimeValue = type { i64, ptr }

@str = private unnamed_addr constant [4 x i8] c"bob\00", align 1
@str.1 = private unnamed_addr constant [5 x i8] c"karl\00", align 1
@str.2 = private unnamed_addr constant [5 x i8] c"Mary\00", align 1
@str.3 = private unnamed_addr constant [5 x i8] c"name\00", align 1
@str.4 = private unnamed_addr constant [4 x i8] c"age\00", align 1
@str.5 = private unnamed_addr constant [7 x i8] c"string\00", align 1
@str.6 = private unnamed_addr constant [4 x i8] c"int\00", align 1
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
  %data_buffer = call ptr @malloc(i64 16)
  %gep = getelementptr ptr, ptr %data_buffer, i64 0
  store ptr %arr_header, ptr %gep, align 8
  %gep12 = getelementptr ptr, ptr %data_buffer, i64 1
  store ptr %arr_header8, ptr %gep12, align 8
  %df = tail call ptr @malloc(i32 ptrtoint (ptr getelementptr (%dataframe, ptr null, i32 1) to i32))
  %raw_ptr13 = call ptr @malloc(i64 48)
  %ptr_int14 = ptrtoint ptr %raw_ptr13 to i64
  %ptr_offset15 = add i64 %ptr_int14, 31
  %ptr_aligned_int16 = and i64 %ptr_offset15, -32
  %arr_data_aligned17 = inttoptr i64 %ptr_aligned_int16 to ptr
  %arr_header18 = call ptr @malloc(i64 24)
  %6 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header18, i32 0, i32 0
  store i64 2, ptr %6, align 4
  %7 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header18, i32 0, i32 1
  store i64 2, ptr %7, align 4
  %8 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header18, i32 0, i32 2
  store ptr %arr_data_aligned17, ptr %8, align 8
  %elem_ptr19 = getelementptr ptr, ptr %arr_data_aligned17, i64 0
  store ptr @str.3, ptr %elem_ptr19, align 32
  %elem_ptr20 = getelementptr ptr, ptr %arr_data_aligned17, i64 1
  store ptr @str.4, ptr %elem_ptr20, align 32
  %9 = getelementptr inbounds nuw %dataframe, ptr %df, i32 0, i32 0
  store ptr %arr_header18, ptr %9, align 8
  %10 = getelementptr inbounds nuw %dataframe, ptr %df, i32 0, i32 1
  store ptr %data_buffer, ptr %10, align 8
  %raw_ptr21 = call ptr @malloc(i64 48)
  %ptr_int22 = ptrtoint ptr %raw_ptr21 to i64
  %ptr_offset23 = add i64 %ptr_int22, 31
  %ptr_aligned_int24 = and i64 %ptr_offset23, -32
  %arr_data_aligned25 = inttoptr i64 %ptr_aligned_int24 to ptr
  %arr_header26 = call ptr @malloc(i64 24)
  %11 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header26, i32 0, i32 0
  store i64 2, ptr %11, align 4
  %12 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header26, i32 0, i32 1
  store i64 2, ptr %12, align 4
  %13 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header26, i32 0, i32 2
  store ptr %arr_data_aligned25, ptr %13, align 8
  %elem_ptr27 = getelementptr ptr, ptr %arr_data_aligned25, i64 0
  store ptr @str.5, ptr %elem_ptr27, align 32
  %elem_ptr28 = getelementptr ptr, ptr %arr_data_aligned25, i64 1
  store ptr @str.6, ptr %elem_ptr28, align 32
  %14 = getelementptr inbounds nuw %dataframe, ptr %df, i32 0, i32 2
  store ptr %arr_header26, ptr %14, align 8
  store ptr %df, ptr @df, align 8
  %runtime_obj = call ptr @malloc(i64 16)
  %runtime_cast = bitcast ptr %runtime_obj to ptr
  %tag_ptr = getelementptr inbounds nuw %RuntimeValue, ptr %runtime_cast, i32 0, i32 0
  store i64 7, ptr %tag_ptr, align 4
  %data_ptr = getelementptr inbounds nuw %RuntimeValue, ptr %runtime_cast, i32 0, i32 1
  store ptr %df, ptr %data_ptr, align 8
  ret ptr %runtime_obj
}

declare i32 @printf(ptr, ...)

declare noalias ptr @malloc(i64)
