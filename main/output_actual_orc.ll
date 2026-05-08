; ModuleID = 'repl_module'
source_filename = "repl_module"

%dataframe = type { ptr, ptr, ptr, i64 }

@str = private unnamed_addr constant [6 x i8] c"Harry\00", align 1
@str.1 = private unnamed_addr constant [4 x i8] c"Bob\00", align 1
@str.2 = private unnamed_addr constant [6 x i8] c"Harry\00", align 1
@str.3 = private unnamed_addr constant [5 x i8] c"name\00", align 1
@str.4 = private unnamed_addr constant [4 x i8] c"age\00", align 1
@str.5 = private unnamed_addr constant [7 x i8] c"isCool\00", align 1
@str.6 = private unnamed_addr constant [1 x i8] zeroinitializer, align 1
@df = global ptr null

define ptr @main_0() {
entry:
  %arr_header = call ptr @malloc(i64 24)
  %arr_data = call ptr @malloc(i64 24)
  %0 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header, i32 0, i32 0
  %1 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header, i32 0, i32 1
  %2 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header, i32 0, i32 2
  store i64 3, ptr %0, align 4
  store i64 3, ptr %1, align 4
  store ptr %arr_data, ptr %2, align 8
  %elem_ptr = getelementptr ptr, ptr %arr_data, i64 0
  store ptr @str, ptr %elem_ptr, align 8
  %elem_ptr1 = getelementptr ptr, ptr %arr_data, i64 1
  store ptr @str.1, ptr %elem_ptr1, align 8
  %elem_ptr2 = getelementptr ptr, ptr %arr_data, i64 2
  store ptr @str.2, ptr %elem_ptr2, align 8
  %arr_header3 = call ptr @malloc(i64 24)
  %arr_data4 = call ptr @malloc(i64 24)
  %3 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header3, i32 0, i32 0
  %4 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header3, i32 0, i32 1
  %5 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header3, i32 0, i32 2
  store i64 3, ptr %3, align 4
  store i64 3, ptr %4, align 4
  store ptr %arr_data4, ptr %5, align 8
  %elem_ptr5 = getelementptr i64, ptr %arr_data4, i64 0
  store i64 12, ptr %elem_ptr5, align 4
  %elem_ptr6 = getelementptr i64, ptr %arr_data4, i64 1
  store i64 23, ptr %elem_ptr6, align 4
  %elem_ptr7 = getelementptr i64, ptr %arr_data4, i64 2
  store i64 34, ptr %elem_ptr7, align 4
  %arr_header8 = call ptr @malloc(i64 24)
  %arr_data9 = call ptr @malloc(i64 3)
  %6 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header8, i32 0, i32 0
  %7 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header8, i32 0, i32 1
  %8 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header8, i32 0, i32 2
  store i64 3, ptr %6, align 4
  store i64 3, ptr %7, align 4
  store ptr %arr_data9, ptr %8, align 8
  %elem_ptr10 = getelementptr i1, ptr %arr_data9, i64 0
  store i1 true, ptr %elem_ptr10, align 1
  %elem_ptr11 = getelementptr i1, ptr %arr_data9, i64 1
  store i1 false, ptr %elem_ptr11, align 1
  %elem_ptr12 = getelementptr i1, ptr %arr_data9, i64 2
  store i1 false, ptr %elem_ptr12, align 1
  %data_header = call ptr @malloc(i64 24)
  %data_buffer = call ptr @malloc(i64 24)
  %9 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header, i32 0, i32 0
  store i64 3, ptr %9, align 4
  %10 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header, i32 0, i32 1
  store i64 3, ptr %10, align 4
  %11 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header, i32 0, i32 2
  store ptr %data_buffer, ptr %11, align 8
  %data_gep = getelementptr ptr, ptr %data_buffer, i64 0
  store ptr %arr_header, ptr %data_gep, align 8
  %data_gep13 = getelementptr ptr, ptr %data_buffer, i64 1
  store ptr %arr_header3, ptr %data_gep13, align 8
  %data_gep14 = getelementptr ptr, ptr %data_buffer, i64 2
  store ptr %arr_header8, ptr %data_gep14, align 8
  %arr_header15 = call ptr @malloc(i64 24)
  %arr_data16 = call ptr @malloc(i64 24)
  %12 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header15, i32 0, i32 0
  %13 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header15, i32 0, i32 1
  %14 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header15, i32 0, i32 2
  store i64 3, ptr %12, align 4
  store i64 3, ptr %13, align 4
  store ptr %arr_data16, ptr %14, align 8
  %elem_ptr17 = getelementptr ptr, ptr %arr_data16, i64 0
  store ptr @str.3, ptr %elem_ptr17, align 8
  %elem_ptr18 = getelementptr ptr, ptr %arr_data16, i64 1
  store ptr @str.4, ptr %elem_ptr18, align 8
  %elem_ptr19 = getelementptr ptr, ptr %arr_data16, i64 2
  store ptr @str.5, ptr %elem_ptr19, align 8
  %arr_header20 = call ptr @malloc(i64 24)
  %arr_data21 = call ptr @malloc(i64 24)
  %15 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header20, i32 0, i32 0
  %16 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header20, i32 0, i32 1
  %17 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header20, i32 0, i32 2
  store i64 3, ptr %15, align 4
  store i64 3, ptr %16, align 4
  store ptr %arr_data21, ptr %17, align 8
  %elem_ptr22 = getelementptr ptr, ptr %arr_data21, i64 0
  store ptr @str.6, ptr %elem_ptr22, align 8
  %elem_ptr23 = getelementptr ptr, ptr %arr_data21, i64 1
  store ptr null, ptr %elem_ptr23, align 8
  %elem_ptr24 = getelementptr ptr, ptr %arr_data21, i64 2
  store ptr null, ptr %elem_ptr24, align 8
  %df_instance = tail call ptr @malloc(i32 ptrtoint (ptr getelementptr (%dataframe, ptr null, i32 1) to i32))
  %18 = getelementptr inbounds nuw %dataframe, ptr %df_instance, i32 0, i32 0
  store ptr %arr_header15, ptr %18, align 8
  %19 = getelementptr inbounds nuw %dataframe, ptr %df_instance, i32 0, i32 1
  store ptr %data_header, ptr %19, align 8
  %20 = getelementptr inbounds nuw %dataframe, ptr %df_instance, i32 0, i32 2
  store ptr %arr_header20, ptr %20, align 8
  %21 = getelementptr inbounds nuw %dataframe, ptr %df_instance, i32 0, i32 3
  store i64 3, ptr %21, align 4
  store ptr %df_instance, ptr @df, align 8
  %runtime_obj = call ptr @malloc(i64 16)
  %runtime_cast = bitcast ptr %runtime_obj to ptr
  %tag_ptr = getelementptr inbounds nuw { i64, ptr }, ptr %runtime_cast, i32 0, i32 0
  store i64 0, ptr %tag_ptr, align 8
  %data_ptr = getelementptr inbounds nuw { i64, ptr }, ptr %runtime_cast, i32 0, i32 1
  store ptr null, ptr %data_ptr, align 8
  ret ptr %runtime_obj
}

declare i32 @printf(ptr, ...)

declare noalias ptr @malloc(i64)
